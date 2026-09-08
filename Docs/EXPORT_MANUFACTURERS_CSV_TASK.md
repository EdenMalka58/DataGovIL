# TASK: Export deduplicated manufacturer/model catalog to `data.csv`

> Read `PROJECT_CONTEXT.md` first for the overall architecture. This task builds on
> `IManufacturerService` / `ManufacturersController` in `DataGovIL.Api`, which already reads the
> WLTP make/model resource (`142afde2-6228-49f9-8a29-9b6c3a0cbe40`) via
> `ICkanApiClient.DatastoreSearchAsync<ManufacturerModelRecord>`.

## Goal

Pull **every** row from the WLTP make/model datastore resource (not just one page), collapse it
down to one row per unique `(tozeret_cd, degem_cd)` pair, and write the result to a CSV file
named `data.csv`.

## Why this needs new code (not just a bigger `pageSize`)

The existing `GET /api/manufacturers` endpoint returns one page at a time, sized for a normal
HTTP response. This task needs **all** rows in the resource, fetched by looping
`datastore_search` with `limit`/`offset` until exhausted — that can be a lot of pages and take
longer than a typical HTTP request should be allowed to run. Do **not** implement this as
synchronous work inside a normal controller action, since a slow/flaky client, a reverse proxy,
or Kestrel's own request timeout can all kill it mid-export.

## Deduplication logic

- **Uniqueness key:** `(tozeret_cd, degem_cd)` — i.e. one output row per distinct manufacturer
  code + model code combination.
- Multiple raw rows can share the same key and differ only in `shnat_yitzur` (model year). All
  such rows must be merged into a single output row.
- For every other field you carry into the output row (`tozeret_nm`, `degem_nm`,
  `kinuy_mishari`, etc.), just take the value from any one of the merged raw rows (they should
  be identical across the group; if you find they aren't in practice, log a warning instead of
  silently picking one — that would indicate the resource has more meaningful variation than
  expected).

## New computed field: `years`

For each unique `(tozeret_cd, degem_cd)` group, collect every `shnat_yitzur` value found across
its raw rows, and compute:

- `fromYear` = the minimum year in the group
- `toYear` = the maximum year in the group

Format the `years` column as:

- `"{fromYear}-{toYear}"` when the group has **more than one distinct year**
  (e.g. `2012-2022`).
- `"{fromYear}-"` (trailing dash, no end year) when the group has **only one distinct year**.
  This is intentional, not a bug: a single observed year is treated as "still in production
  since that year" rather than a closed one-year range like `2008-2008`.

  **Confirm this interpretation makes sense once you see real data** — it's my best reading of
  the original spec ("2008- אם אין המשך אז הכוונה עד היום" — "2008- if there's no continuation,
  it means until today"), but if `shnat_yitzur` turns out to have gaps or a different meaning
  than "model year this trim was sold", revisit the rule.

### Example output rows

| tozeret_cd | tozeret_nm | degem_cd | degem_nm | years |
|---|---|---|---|---|
| 413 | טויוטה יפן | 304 | ZVG12L-KHXNBW | 2012-2022 |
| 413 | טויוטה יפן | 512 | ABC123 | 2008- |

## Fetching all rows (pagination loop)

Reuse `ICkanApiClient.DatastoreSearchAsync<ManufacturerModelRecord>` with a `DatastoreSearchQuery`
against `WltpMakeModelResourceId`. Loop:

1. Start at `offset = 0` with a reasonably large `limit` (e.g. 1000 — datastore_search typically
   allows large limits; confirm the resource doesn't cap it lower and adjust if so).
2. Request with `IncludeTotal = true` **on the first call only** (cheap way to know when to stop
   and to log progress); it's fine to leave it `true` on every call too if performance is
   acceptable, but it does cost more on the server side per the OpenAPI spec's docs on
   `include_total`.
3. Append `result.Records` to an in-memory accumulator (or fold directly into the
   dedup dictionary as you go, keyed by `(tozeret_cd, degem_cd)` — that avoids holding two full
   copies of the data in memory at once).
4. Advance `offset += limit`.
5. Stop when a page returns fewer records than `limit` (or zero), or once `offset >= total` if
   you captured `total`.
6. Add basic resilience: if a page request fails, retry a few times with backoff before giving
   up the whole export (log which offset failed).

## Writing the CSV

- File name: `data.csv`.
- Decide a sensible output location (e.g. a configurable `ExportOptions:OutputDirectory` in
  `appsettings.json`, defaulting to something like `App_Data/exports/` under the content root) —
  don't hardcode an absolute path.
- Use a proper CSV writer rather than hand-rolled string concatenation, so quoting/escaping of
  Hebrew text, commas, and quotes inside fields is handled correctly. `CsvHelper` (NuGet) is a
  reasonable, well-established choice; add it as a package reference if you go that route.
- Write UTF-8 **with BOM** so the file opens correctly with Hebrew text in Excel.
- Columns, in order: `tozeret_cd`, `tozeret_nm`, `degem_cd`, `degem_nm`, `kinuy_mishari`, `years`
  (add/drop columns as needed based on what's actually useful once you see the real data shape —
  this is a starting point, not a rigid contract).

## Where to run this (avoiding request timeouts)

The instruction was explicitly "pick a place in the code that runs the loop without hitting a
timeout — maybe a background service, or wherever you think is best." Recommended approach:

1. Implement the fetch-dedupe-write logic as its own service, e.g.
   `IManufacturerCsvExportService.ExportAsync(CancellationToken)`, independent of any HTTP
   request lifecycle.
2. Trigger it one of these ways (pick whichever fits how this will actually be used):
   - **On-demand via an endpoint that returns immediately:** `POST /api/manufacturers/export`
     starts the export as a fire-and-forget background task (e.g. queued onto an
     `IHostedService`/`BackgroundService` via a `Channel<T>` work queue — this is the standard
     ASP.NET Core "queued background tasks" pattern) and responds `202 Accepted` right away,
     without the client waiting on the HTTP connection for the whole export. Optionally expose a
     `GET /api/manufacturers/export/status` endpoint to poll progress/completion (e.g. "not
     started" / "running, N pages fetched" / "completed at {time}, {count} rows" / "failed:
     {error}"), backed by some simple shared state (a singleton status object is enough for a
     single-instance deployment; don't over-engineer this with a job queue database unless the
     project already needs one).
   - **A one-off console/worker mode:** if this only needs to run occasionally/manually (e.g. by
     an admin), a separate `BackgroundService` that runs the export once on startup when a
     config flag or CLI argument is set could be simpler than adding new HTTP endpoints. Use
     judgment based on how this will actually be invoked in practice.
3. Whichever approach you pick, make sure the export logic itself doesn't run on the ASP.NET
   Core request thread pool in a way that's tied to a specific HTTP request's cancellation token
   — it should keep running even if the triggering HTTP request/connection ends.
4. Log progress periodically (e.g. every N pages) so a long export is observable, not silent.

## Acceptance criteria

- [ ] Running the export produces `data.csv` with one row per unique `(tozeret_cd, degem_cd)`.
- [ ] The `years` column matches the rules above, verified against a few manually-checked
      manufacturer/model groups that are known to have multiple `shnat_yitzur` values.
- [ ] The export completes successfully even though it may take longer than a typical HTTP
      request timeout — triggering it does not require the calling HTTP client to keep a
      connection open for the full duration.
- [ ] The pagination loop correctly terminates (no infinite loop, no off-by-one skipping/
      duplicating the boundary record between pages).
- [ ] Hebrew text round-trips correctly when the CSV is opened in Excel.
