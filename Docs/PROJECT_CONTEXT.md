# PROJECT_CONTEXT.md — DataGovIL

> Read this before touching any code. It exists so an AI assistant (or a human) picking up
> this repo cold has the same context I had when building it. Keep it up to date as the
> project evolves — when you make an architectural decision, add it here.

## Goal

A .NET Core Web API that exposes clean, typed endpoints backed by data.gov.il's public CKAN
datastore (Israeli government open data — vehicle registrations and vehicle model catalogs,
initially). The design deliberately separates two concerns:

1. A **generic CKAN API client** that knows nothing about vehicles — it only knows how to talk
   to CKAN's Action API (packages, resources, organizations, and row-level `datastore_search`).
2. A **thin, purpose-built Web API** on top of it that knows about *this* domain (vehicles,
   manufacturers) and wires specific resource ids to specific typed DTOs.

This split matters: adding support for a new data.gov.il dataset should never require touching
`DataGovIL.Client` — only adding a new DTO + service + controller in `DataGovIL.Api`.

## Solution layout

```
DataGovIL.sln
src/
  DataGovIL.Client/                 <- generic, reusable CKAN client (own NuGet-able project)
    CkanApiClient.cs                <- ICkanApiClient implementation, all HTTP calls live here
    ICkanApiClient.cs                <- interface — read this first to see the client's surface
    CkanClientOptions.cs             <- BaseUrl / ApiKey / Timeout, bound from config
    CkanApiException.cs              <- thrown on CKAN success:false or deserialization failure
    ServiceCollectionExtensions.cs   <- AddDataGovIlClient(...) DI helper
    Json/
      FlexibleStringConverter.cs     <- see "Known gotchas" below — important, don't remove
    Models/
      CkanEnvelope.cs                <- { help, success, result, error } wrapper every CKAN
                                         response comes in
      DatastoreModels.cs             <- DatastoreSearchQuery (request) + DatastoreSearchResult<T>
                                         (response) for datastore_search — this is the endpoint
                                         used to read actual rows
      PackageModels.cs                <- PackageInfo/OrganizationInfo/ResourceInfo/etc. for the
                                         dataset-metadata endpoints (package_search, etc.)

  DataGovIL.Api/                    <- the actual Web API, references DataGovIL.Client
    Program.cs                      <- DI wiring — start here to see how everything connects
    appsettings.json                <- "DataGovIL" section (client config) + "VehicleDataResources"
                                         section (the 3 resource ids below)
    Controllers/
      VehiclesController.cs         <- GET /api/vehicles/{registrationNumber}, GET /api/vehicles
      ManufacturersController.cs    <- GET /api/manufacturers,
                                         POST /api/manufacturers/export (202 + background job),
                                         GET /api/manufacturers/export/status
    Services/
      VehicleService.cs             <- IVehicleService — registration lookup, checks 2 resources
      ManufacturerService.cs        <- IManufacturerService — paged make/model list
      ManufacturerCsvExportService.cs
                                    <- IManufacturerCsvExportService — full-catalog fetch,
                                         (tozeret_cd, degem_cd) dedupe, data.csv write
      ManufacturerExportBackgroundService.cs
                                    <- IHostedService that runs the export on host lifetime
      ManufacturerExportWorkQueue.cs
                                    <- Channel<T> handoff from HTTP -> worker
      ManufacturerExportStatusStore.cs
                                    <- singleton in-process job status
    Models/
      VehicleRecord.cs              <- DTO for one row of the vehicle registration resource
      ManufacturerModelRecord.cs    <- DTO for one row of the WLTP make/model resource
      ManufacturerCsvRow.cs         <- data.csv column shape
      ManufacturerExportStatus.cs   <- export job snapshot
      ExportOptions.cs              <- OutputDirectory / PageSize / retries (appsettings)
      PagedResult.cs                <- generic paging envelope returned by both controllers,
                                         also holds VehicleDataResourceOptions (the 3 ids)
```

## The three data.gov.il resources this project targets

Configured in `src/DataGovIL.Api/appsettings.json` under `VehicleDataResources`:

| Resource id | Dataset (Hebrew) | Used by |
|---|---|---|
| `053cea08-09bc-40ec-8f7a-156f0677aff3` | מספרי רישוי של כלי רכב פרטיים ומסחריים | `VehicleService` (primary) |
| `0866573c-40cd-4ca8-91d2-9dd2d7a492e5` | מספרי רישוי... — המשך (dataset is split in two on the portal) | `VehicleService` (fallback) |
| `142afde2-6228-49f9-8a29-9b6c3a0cbe40` | תוצרים ודגמים של כלי רכב WLTP | `ManufacturerService` |

Base CKAN API: `https://data.gov.il/api/3`. Full OpenAPI spec for the CKAN layer was provided
up front and is what `CkanApiClient` is modeled on (action-style RPC, POST for anything needing
nested JSON like `filters`, envelope response shape).

## Known gotchas — read before "fixing" things that look like bugs

1. **CKAN datastore JSON typing is inconsistent.** The same conceptual column (e.g.
   `mispar_rechev`, `tozeret_cd`, `degem_cd`) can come back as a JSON *number* even though it's
   an identifier, not something to do arithmetic on. `System.Text.Json` throws by default when
   a JSON number lands on a `string` property. **`Json/FlexibleStringConverter.cs`** fixes this
   globally (registered on `CkanApiClient.DefaultJsonOptions`) by accepting number/bool/string
   tokens for any `string?` property and stringifying them. If you add new DTOs with `string?`
   properties, you get this tolerance for free — don't special-case individual fields, and don't
   remove this converter.

2. **Column names in `VehicleRecord` / `ManufacturerModelRecord` are best-effort, not verified.**
   I could not fetch data.gov.il directly to confirm field names (`robots.txt` blocks automated
   tools). They're the commonly-used names for these dataset families (`mispar_rechev`,
   `tozeret_nm`, `degem_nm`, `shnat_yitzur`, ...), and real production traffic (see the debugging
   session that led to gotcha #1) has confirmed most of them are correct — but if you hit
   unexpected nulls, run:
   ```
   GET https://data.gov.il/api/3/action/datastore_search?resource_id=<id>&limit=0
   ```
   and check the `"fields"` array in the response for the authoritative column list. Update the
   `[JsonPropertyName("...")]` attribute, nothing else needs to change. Every record also carries
   `[JsonExtensionData] ExtensionData` so unmapped columns are never silently lost — check there
   first if a value seems missing from the typed properties.

3. **`DatastoreSearchQuery` always POSTs, never GETs**, even though the OpenAPI spec shows a GET
   variant of `datastore_search`. This is intentional: `filters` needs to be a real JSON object,
   which the CKAN GET variant doesn't support cleanly (query-string only). Don't "simplify" this
   to a GET call.

4. **`CkanApiClient.UnwrapAsync<TResult>`'s catch block** used to say "was not valid JSON" for
   *any* `JsonException`, which was actively misleading in the case that caused gotcha #1 (the
   JSON was perfectly valid; deserialization into the target C# type failed). The message now
   says "could not be deserialized into '<TypeName>'" and includes `ex.Message`, which for
   `System.Text.Json` names the offending JSON path. Keep this message format if you touch it —
   it's the main debugging aid when a new dataset's shape doesn't match your DTO.

5. **Nullable generic gymnastics in `CkanApiClient.cs`**: several methods use `TResult?` on an
   unconstrained generic and null-forgiving (`!`) on the returned `Task<...>`. This is
   intentional and correct (C# 9+ allows `T?` annotations on unconstrained type parameters; for
   value types it's a no-op, for reference types it's just a nullability annotation, not a
   different runtime type) — don't "fix" the `!` operators without understanding why they're
   there, they're suppressing legitimate nullable-analysis noise, not hiding real bugs.

6. **WLTP `datastore_search` `total` is an estimate.** A `limit=5` probe of
   `142afde2-6228-49f9-8a29-9b6c3a0cbe40` returned `"total": 101342` with
   `"total_was_estimated": true`. `ManufacturerCsvExportService` therefore stops only when a
   page comes back with fewer rows than `limit`, not when `offset >= total`. Using the
   estimated total as a hard stop can drop the tail of the catalog. Rows are also extremely
   wide (~90 columns); the export asks only for the six columns it writes so each page of
   1000 stays small. The full dump is ~100 pages and must not run on an HTTP request thread
   tied to the caller's cancellation token — `POST /api/manufacturers/export` queues work onto
   `ManufacturerExportBackgroundService` and returns `202 Accepted`.

7. **`data.csv` `years` column.** Deduped by `(tozeret_cd, degem_cd)`. Multiple `shnat_yitzur`
   values become `"{min}-{max}"` (e.g. `2021-2024`); a single distinct year becomes
   `"{year}-"` (open-ended, "still in production since that year"). Live rows for
   `(tozeret_cd=5, degem_cd=179)` are 2021/2022/2023/2024 consecutive model years, which
   matches treating `shnat_yitzur` as model year. File is UTF-8 with BOM under
   `ExportOptions:OutputDirectory` (default `App_Data/exports/`).

## Build status

**Not yet verified with a real `dotnet build`** — it was written and hand-reviewed in an
environment with no .NET SDK and no NuGet access. Do a build immediately after opening this in
Cursor and fix anything a real compiler catches (there shouldn't be much, but don't assume it's
guaranteed to compile clean).

```bash
dotnet restore
dotnet build
dotnet run --project src/DataGovIL.Api   # opens /swagger via the launch profile
```

## Suggested next steps

Rough priority order — pick based on what you actually need:

- [ ] Run a real build, fix any compiler errors/warnings.
- [ ] Verify `VehicleRecord`/`ManufacturerModelRecord` field names against live
      `datastore_search` responses (see gotcha #2); add any missing commonly-needed columns.
- [ ] Add automated tests for `CkanApiClient` (mock `HttpClient` via
      `HttpMessageHandler`) and for `VehicleService`'s two-resource fallback logic.
- [ ] Add response caching (e.g. `IMemoryCache` or `HybridCache`) in front of
      `ManufacturerService` — the WLTP make/model list changes rarely and is a good caching
      candidate; vehicle registration lookups should probably stay uncached (data can change,
      and it's PII-adjacent).
- [ ] Decide on a strategy for rate limiting / retry against data.gov.il (it's a public
      government API with no documented SLA) — consider `Microsoft.Extensions.Http.Resilience`.
- [ ] Add integration tests that hit the real data.gov.il API (behind a `[Trait("Category",
      "Integration")]` or similar, so they can be excluded from CI if the portal is flaky).
- [ ] Consider whether `VehicleRecord`'s exposure of full vehicle data (chassis number, owner
      type) needs any access control / PII handling before this becomes a public-facing API.
- [ ] If more data.gov.il datasets get added, keep following the existing pattern: new resource
      id in `appsettings.json` → new DTO in `Api/Models` → new service in `Api/Services` → new
      controller. `DataGovIL.Client` should not need changes.

## Conventions used so far

- Nullable reference types enabled everywhere (`<Nullable>enable</Nullable>`).
- DTOs that map external JSON always carry `[JsonExtensionData] Dictionary<string, object>?
  ExtensionData` so schema drift never silently loses data.
- Controllers catch `CkanApiException` and translate to `502 Bad Gateway` via `Problem(...)` —
  keep this pattern for new controllers rather than letting it bubble to a generic 500.
- Pagination is 1-based (`page=1` is the first page) and expressed via `PagedResult<T>`.
