# Error Overview Export Process

The Error Overview Export process reads the materialized error tables from the
GeoPackage produced by [Geopackage Generation](vsa-geopackage-generation.md)
and exports them to an Excel workbook (XLSX) with two configurable sheets.

This processor implements the "Fehlerübersicht" part of the
[Excel Mapper](architektur.md#excel-mapper) described in the architecture.

## Configuration

All configuration parameters are resolved from the pipeline YAML
(`default_config` / `process_config_overwrites`). Sheet names, column
positions, and display headers are fully driven by configuration so that
changes to the Excel layout do not require code changes.

| Parameter                      | Type                       | Description                                                      |
|--------------------------------|----------------------------|------------------------------------------------------------------|
| `errorDataSheet`               | `string`                   | Excel sheet name for the `ca_error_data` export.                 |
| `errorDataAttributeMapping`    | `IDictionary<string,string>` | Maps attribute keys (SQLite column names) to Excel header display names. |
| `errorDataColumnMapping`       | `IDictionary<string,string>` | Maps the same attribute keys to Excel column letters (A, B, ...). |
| `errorObjectSheet`             | `string`                   | Excel sheet name for the `ca_error_object` export.               |
| `errorObjectAttributeMapping`  | `IDictionary<string,string>` | Maps attribute keys to display names for the object sheet.       |
| `errorObjectColumnMapping`     | `IDictionary<string,string>` | Maps the same attribute keys to column letters for the object sheet. |

### Mapping validation

The `attributeMapping` and `columnMapping` for each sheet must define exactly
the same set of keys. A mismatch (key present in one but not the other) causes
an `ArgumentException` at construction time, listing the mismatched keys.

## Inputs

| Parameter    | Source                | Type            | Description                                             |
|--------------|-----------------------|-----------------|---------------------------------------------------------|
| `geopackage` | Geopackage Generation | `IPipelineFile` | The populated GeoPackage containing `ca_error_data` and `ca_error_object`. |

## Output

| Key             | Type            | Description                                    |
|-----------------|-----------------|------------------------------------------------|
| `errorOverview` | `IPipelineFile` | The generated Excel workbook (`error-overview.xlsx`). |

## Processing

1. The GeoPackage is opened read-only via `Microsoft.Data.Sqlite`.
2. For each configured sheet (`ca_error_data`, `ca_error_object`):
   - A worksheet is created with the configured sheet name.
   - Row 1 is populated with header display names from the attribute mapping,
     placed in the column positions defined by the column mapping.
   - All rows from the source table are queried (selecting only the mapped
     columns) and written starting at row 2.
   - Cell values preserve their SQLite type: integers and doubles remain
     numeric in Excel, text remains text, NULL cells are left blank.
3. The workbook is saved via ClosedXML to a pipeline output file.
