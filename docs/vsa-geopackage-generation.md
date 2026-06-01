# VSA Geopackage Generation Process

The VSA Geopackage Generation process imports the INTERLIS transfer files
produced by the [VSA Matcher](vsa-matcher.md) into the schema-only
GeoPackage template, enriches the result with checker CSV data and an
error matrix, and materializes analytical tables. It emits a single
populated GeoPackage for the downstream processors.

## Configuration

| Parameter             | Type                   | Description                                                                                  |
|-----------------------|------------------------|----------------------------------------------------------------------------------------------|
| `jobsDirectory`       | `string`               | Local path the `ili2gpkg` worker has mounted as `ILI2GPKG_JOBS_DIR`. Used to exchange files between plugin and worker. |

## Inputs

| Parameter         | Source       | Type             | Description                                                                          |
|-------------------|--------------|------------------|--------------------------------------------------------------------------------------|
| `geoPackage`      | VSA Matcher  | `IPipelineFile`  | Schema-only GeoPackage template (`gpkg_template`), matching the GEP model version.   |
| `dssMiniXtf`      | VSA Matcher  | `IPipelineFile`  | The GEP / DSS Mini INTERLIS transfer file (`gep`).                                   |
| `defaultOrgsXtf`  | VSA Matcher  | `IPipelineFile`  | Standard organisation table from the VSA repository (`standard_org_table`).          |
| `userOrgsXtf`     | VSA Matcher  | `IPipelineFile?` | Optional user organisation table from the upload (`user_org_table`). May be empty.   |
| `checkerCsvT`     | VSA Matcher  | `IPipelineFile`  | Checker CSV file for Traegerschaft (T).                                              |
| `checkerCsvA`     | VSA Matcher  | `IPipelineFile`  | Checker CSV file for ARA (A).                                                        |
| `checkerCsvFp`    | VSA Matcher  | `IPipelineFile`  | Checker CSV file for Fachpruefungen (FP).                                            |
| `errorMatrix`     | VSA Matcher  | `IPipelineFile`  | Error matrix XLSX file mapping error IDs to descriptions and priorities.              |
| `language`        | VSA Matcher  | `string`         | Language code (`DE` or `FR`) for localized error matrix columns.                     |

## Output

| Key                   | Type             | Description                                                                 |
|-----------------------|------------------|-----------------------------------------------------------------------------|
| `generatedGeopackage` | `IPipelineFile?` | The populated GeoPackage, named `generated.gpkg`. `null` if any ili2gpkg import step failed. |

## Interlis-Import

The three transfer files are imported sequentially into the template
GeoPackage via `IIli2GpkgClient.ImportToGeoPackageAsync`. Each import
reads the current GeoPackage stream plus one XTF, and writes the result
to a freshly allocated pipeline file that becomes the input of the next
step.

Import order:

1. `defaultOrgs` — standard organisation table
2. `userOrgs` — *only if an upload organisation table is present*
3. `dssMini` — GEP transfer file

Each import writes to a temporary `gpkg-step-{label}.gpkg` file that
becomes the input of the next step.

### ili2gpkg arguments

The following `ili2gpkg` flags are set for every import step:

| Argument                | Value | Rationale                                                                |
|-------------------------|-------|--------------------------------------------------------------------------|
| `--skipReferenceErrors` | on    | Continue when XTF references cannot be resolved.                         |
| `--skipGeometryErrors`  | on    | Continue when geometry errors are encountered.                           |
| `--disableValidation`   | on    | INTERLIS validation is the GEP-Checker's responsibility, not ours.       |
| `--importTid`           | on    | Import the INTERLIS TID into the database (needed for downstream joins). |

## Checker CSV Import

After the INTERLIS import, the three checker CSV files are imported into
the GeoPackage as tables `checker_csv_t`, `checker_csv_a`, and
`checker_csv_fp`. The CSV files use Windows-1252 encoding. Join indexes
are created on (`ErrorId`, `Model`, `Class`) for each table.

## Error Matrix Import

The error matrix XLSX is imported into the `error_matrix` table with a
join index on (`cid`, `model`, `class_de`).

## Analytics

Two analytical views are created first:

1. `v_checker_csv_all` unions the three checker CSV tables with a
   `source` column (`T`, `A`, `FP`).
2. `v_checker_errors` joins the union view with the error matrix,
   enriching each CSV row with localized descriptions and priorities.

Then `ErrorDataMaterializer` materializes two tables from the errors
view:

- **`ca_error_data`** contains one row per checker error. Each row
  carries the error description, priority columns (`wk`, `gep`),
  recommendation, and (for Leitung and Knoten) enrichment from the
  feature tables (`funktionhierarchisch`, `eigentuemer`, `status`).
  Feature classes whose tables are not present in the GeoPackage are
  handled by a catch-all that sets enrichment columns to NULL.
- **`ca_error_object`** aggregates `ca_error_data` by (`tid`, `class`)
  with `COUNT(*)`, `MAX(wk)`, and `MAX(gep)`.

The `check_type` column is derived from the CSV `Module` field: `reader`
becomes `ig`, otherwise the value is the CSV source (`T`, `A`, `FP`).
The `module` column follows similar logic: `reader` becomes `igcheck`,
everything else becomes `gep_check`.

The final output is written to `generated.gpkg`.

## Failure behaviour

If any `ili2gpkg` import step reports a non-success result, the process
logs the worker output at debug level and returns `null` for
`generatedGeopackage`. The remaining enrichment steps are skipped.
Downstream processors detect the missing GPKG via their own
pre-condition and abort the pipeline.
