# VSA Geopackage Generation Process

The VSA Geopackage Generation process imports the INTERLIS transfer files
produced by the [VSA Matcher](vsa-matcher.md) into the schema-only
GeoPackage template and emits a single populated GeoPackage for the
downstream processors.

In its current scope the process covers the **Interlis-Import** step
described in [architektur.md](architektur.md#geopackage-generation-aggregation);
error preparation and statistics will be added as additional steps in
the same process.

## Configuration

| Parameter             | Type                   | Description                                                                                  |
|-----------------------|------------------------|----------------------------------------------------------------------------------------------|
| `jobsDirectory`       | `string`               | Local path the `ili2gpkg` worker has mounted as `ILI2GPKG_JOBS_DIR`. Used to exchange files between plugin and worker. |

## Inputs

The `RunAsync` method receives four collections:

| Parameter         | Source       | Type             | Description                                                                          |
|-------------------|--------------|------------------|--------------------------------------------------------------------------------------|
| `geoPackage`      | VSA Matcher  | `IPipelineFile`  | Schema-only GeoPackage template (`gpkg_template`), matching the GEP model version.   |
| `dssMiniXtf`      | VSA Matcher  | `IPipelineFile`  | The GEP / DSS Mini INTERLIS transfer file (`gep`).                                   |
| `defaultOrgsXtf`  | VSA Matcher  | `IPipelineFile`  | Standard organisation table from the VSA repository (`standard_org_table`).          |
| `userOrgsXtf`     | VSA Matcher  | `IPipelineFile?` | Optional user organisation table from the upload (`user_org_table`). May be empty.   |

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

The final import writes to the canonical output file `generated.gpkg`;
intermediate steps use temporary `gpkg-step-{label}.gpkg` files.

### ili2gpkg arguments

The following `ili2gpkg` flags are set for every import step:

| Argument                | Value | Rationale                                                                |
|-------------------------|-------|--------------------------------------------------------------------------|
| `--skipReferenceErrors` | on    | Continue when XTF references cannot be resolved.                         |
| `--skipGeometryErrors`  | on    | Continue when geometry errors are encountered.                           |
| `--disableValidation`   | on    | INTERLIS validation is the GEP-Checker's responsibility, not ours.       |
| `--importTid`           | on    | Import the INTERLIS TID into the database (needed for downstream joins). |

## Failure behaviour

If any `ili2gpkg` import step reports a non-success result, the process
logs the worker output at debug level and returns `null` for
`generatedGeopackage`. Downstream processors detect the missing GPKG via
their own pre-condition and abort the pipeline.
