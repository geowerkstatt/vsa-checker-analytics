# VSA Matcher Process

The VSA Matcher routes input files by semantic role, extracts metadata from the
GEP transfer file, and enriches the pipeline with application resources and
VSA repository data.

It is the central classification step: everything upstream (unzip) is generic,
everything downstream (aggregation, topology, Excel) depends on the named
outputs this process produces.

## Configuration

Constructor parameters are resolved by `PipelineProcessFactory` from the
pipeline YAML configuration. `ILogger` and `IPipelineFileManager` are
injected automatically by the framework.

| Parameter                  | Type     | Description                                                       |
|----------------------------|----------|-------------------------------------------------------------------|
| `geoPackageTemplatePath2020`  | `string` | Path to the GeoPackage template for model version 2020.           |
| `geoPackageTemplatePath20201` | `string` | Path to the GeoPackage template for model version 2020.1.         |
| `errorMatrixPath`             | `string` | Path to the error matrix XLSX.                                    |
| `vsaOrgTableUrl2020`         | `string` | URL of the standard organisation table (2020) on the VSA repository (https://www.vsa.ch/models/organisation/vsa_organisationen.xtf).  |
| `vsaOrgTableUrl20201`        | `string` | URL of the standard organisation table (2020.1) on the VSA repository (https://www.vsa.ch/models/organisation/vsa_organisationen_2020_1.xtf).|

## Inputs

The `RunAsync` method receives two collections:

| Parameter       | Source             | Description                                                        |
|-----------------|--------------------|--------------------------------------------------------------------|
| `uploadFiles`   | User upload        | The originally uploaded files (GEP transfer file, optional organisation table, ZIP). |
| `unzippedFiles` | ZIP Unpacker       | Files extracted from the GEP checker ZIP by the preceding unzip step. 3*3 files, 9 in total: CSV, XTF, and Log for each of the three VSA check classes (a, FP, T). |

## Outputs

`RunAsync` returns a `Dictionary<string, object?>` with the following keys:

| Key                 | Type              | Description                                                                 |
|---------------------|-------------------|-----------------------------------------------------------------------------|
| `gep`               | `IPipelineFile[]` | All uploaded files identified as GEP transfer files. Step Post condition will ensure exactly one is present. |
| `model_version`     | `string?`         | `"2020"` or `"2020.1"`, derived from the GEP ILI model name. Only set when exactly one GEP is found. Mandatory for further processing, Step Post condition will ensure its presence. |
| `language`          | `string?`         | `"DE"` or `"FR"`, derived from the GEP ILI model name. Only set when exactly one GEP is found. Mandatory for further processing, Step Post condition will ensure its presence. |
| `user_org_table`    | `IPipelineFile[]` | All uploaded files identified as user organisation tables. Step Post condition will ensure zero or one is present. |
| `checker_csv_a`     | `IPipelineFile[]` | Checker CSVs for ARA (a), matched by filename ending in `_a_err`. Step Post condition will ensure exactly one is present. |
| `checker_csv_fp`    | `IPipelineFile[]` | Checker CSVs for Fachpruefungen (FP), matched by filename ending in `_fp_err`. Step Post condition will ensure exactly one is present. |
| `checker_csv_t`     | `IPipelineFile[]` | Checker CSVs for Traegerschaft (T), matched by filename ending in `_t_err`. Step Post condition will ensure exactly one is present. |
| `gpkg_template`     | `IPipelineFile?`  | GeoPackage template copied from app resources, matching the model version. Mandatory for further processing, Step Post condition will ensure its presence. |
| `standard_org_table`| `IPipelineFile?`  | Standard organisation table fetched from the VSA repository, matching the model version. Mandatory for further processing, Step Post condition will ensure its presence. |
| `error_matrix`      | `IPipelineFile?`  | Error matrix XLSX copied from app resources. Mandatory for further processing, Step Post condition will ensure its presence. |

## File identification

### GEP transfer file

Identified by matching the INTERLIS model names declared in the XTF header
against static model sets. Model matching is **case-insensitive**. Both
INTERLIS 2.4 and 2.3 header formats are supported.

Version 2020.1 is checked before 2020 (more specific first).

| Language | Model version | ILI model name            |
|----------|---------------|---------------------------|
| de       | 2020          | `VSADSSMINI_2020_LV95`    |
| de       | 2020.1        | `VSADSSMINI_2020_1_LV95`  |
| fr       | 2020          | `VSASDEEMINI_2020_LV95`   |
| fr       | 2020.1        | `VSASDEEMINI_2020_1_LV95` |

### Organisation table

Also identified by ILI model name matching (case-insensitive).

| Language | ILI model name                  |
|----------|---------------------------------|
| de       | `SIA405_Base_Abwasser_LV95`     |
| de       | `SIA405_Base_Abwasser_1_LV95`   |
| fr       | `SIA405_Base_Eaux_usees_LV95`   |
| fr       | `SIA405_Base_Eaux_usees_1_LV95` |

### Checker CSVs

Only `.csv` files inside the `check` directory (as set by the `UnzipProcess`
via `OriginalRelativePath`) are considered. The filename (without extension) is
then matched against regex patterns.

| Output key       | Pattern     | Matches example            |
|------------------|-------------|----------------------------|
| `checker_csv_a`  | `_a_err$`   | `check/gep_a_err.csv`     |
| `checker_csv_fp` | `_fp_err$`  | `check/gep_fp_err.csv`    |
| `checker_csv_t`  | `_t_err$`   | `check/gep_t_err.csv`     |

