# DataGovIL — .NET Core Web API over data.gov.il

Two projects:

- **`src/DataGovIL.Client`** — a generic, reusable C# client for the data.gov.il CKAN Action
  API (the OpenAPI spec you shared). It knows about `package_search`, `package_show`,
  `organization_show/list`, `resource_search/show`, `license_list`, `status_show`,
  `scheming_dataset_schema_show`, and — most importantly for data access —
  **`datastore_search`**, which is how you read the actual rows inside a resource.
  It has no idea what "vehicles" or "manufacturers" are; it just talks CKAN.

- **`src/DataGovIL.Api`** — an ASP.NET Core Web API with two controllers built on top of the
  client, wired to the three resource ids you listed:
  - `142afde2-6228-49f9-8a29-9b6c3a0cbe40` — תוצרים ודגמים של כלי רכב WLTP
  - `053cea08-09bc-40ec-8f7a-156f0677aff3` — מספרי רישוי, פרטי/מסחרי (חלק 1)
  - `0866573c-40cd-4ca8-91d2-9dd2d7a492e5` — מספרי רישוי, פרטי/מסחרי (המשך, חלק 2)

## Running it

```bash
dotnet restore
dotnet run --project src/DataGovIL.Api
```

Then open `/swagger` (the launch profile does this automatically).

## Endpoints

### `GET /api/vehicles/{registrationNumber}`
Looks up one vehicle by exact registration number (מספר רישוי). Since the dataset on the
portal is split into two resources ("...המשך"), the service checks the first resource and
falls back to the continuation resource automatically. Returns `404` if not found in either.

### `GET /api/vehicles?q=...&page=1&pageSize=20`
Free-text, paged search across the primary vehicle resource.

### `GET /api/manufacturers?manufacturer=...&page=1&pageSize=20`
Paged list of manufacturer/model rows from the WLTP dataset. Pass `manufacturer` to filter to
one exact manufacturer name.

## ⚠️ About the field names — please verify before relying on this

`VehicleRecord` and `ManufacturerModelRecord` (in `src/DataGovIL.Api/Models`) map to the
commonly-used Hebrew/transliterated column ids for these two dataset families (e.g.
`mispar_rechev`, `tozeret_nm`, `degem_nm`, `shnat_yitzur`...). I could not call data.gov.il
from this environment to confirm the *exact* live column ids for these specific resource ids
(`robots.txt` blocks automated fetches), so treat those property names as a well-informed
starting point, not a guarantee.

To confirm the real columns, call this once per resource and read the `"fields"` array in the
response:

```
GET https://data.gov.il/api/3/action/datastore_search?resource_id=053cea08-09bc-40ec-8f7a-156f0677aff3&limit=0
```

If any names differ, just update the `[JsonPropertyName("...")]` attributes — nothing else
needs to change, and no data is lost in the meantime either way: every row also carries an
`ExtensionData` dictionary with every raw column, mapped or not.

## Extending the client to other datasets

Because `ICkanApiClient.DatastoreSearchAsync<TRecord>` is generic, adding a new endpoint for
another data.gov.il resource is just: new resource id in config → new `TRecord` DTO → new
service method calling `DatastoreSearchAsync<TRecord>(new DatastoreSearchQuery { ResourceId = ..., Filters = ..., ... })`.
No changes to the client itself are needed.
