# Error Overview Export Process

The Error Overview Export process reads the materialized error tables from the
GeoPackage produced by [Geopackage Generation](vsa-geopackage-generation.md)
and exports them to an Excel workbook (XLSX) with two configurable data sheets
and optional pivot table overview sheets for WK and GEP priorities.

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
| `overviewWkSheet`              | `string?`                  | Sheet name for the WK pivot overview. `null` to skip.            |
| `overviewGepSheet`             | `string?`                  | Sheet name for the GEP pivot overview. `null` to skip.           |
| `overviewRowFields`            | `IList<string>?`           | Attribute keys for pivot row fields (after the priority field).  |
| `overviewFilterFields`         | `IList<string>?`           | Attribute keys for pivot report filter fields.                   |
| `overviewValueField`           | `string?`                  | Attribute key for the pivot count value field.                   |
| `overviewValueName`            | `string?`                  | Display name for the pivot value column.                         |

### Mapping validation

The `attributeMapping` and `columnMapping` for each sheet must define exactly
the same set of keys. A mismatch (key present in one but not the other) causes
an `ArgumentException` at construction time, listing the mismatched keys.

### Pivot overview validation

When either `overviewWkSheet` or `overviewGepSheet` is set, all remaining
`overview*` parameters must be provided. Each attribute key in
`overviewRowFields`, `overviewFilterFields`, and `overviewValueField` must
exist in `errorDataAttributeMapping`; a missing key causes an
`ArgumentException` at construction time. The WK sheet uses the `wk` attribute
as its first row field, the GEP sheet uses `gep`.

## Inputs

| Parameter    | Source                | Type            | Description                                             |
|--------------|-----------------------|-----------------|---------------------------------------------------------|
| `geopackage` | Geopackage Generation | `IPipelineFile` | The populated GeoPackage containing `ca_error_data` and `ca_error_object`. |

## Output

| Key             | Type            | Description                                    |
|-----------------|-----------------|------------------------------------------------|
| `errorOverview` | `IPipelineFile` | The generated Excel workbook (`error-overview.xlsx`). |
| `status_message` | `LocalizedText` | Localized status message reporting the number of exported errors. Surfaced in the UI via the `StatusMessage` output action. |

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
3. If configured, pivot table overview sheets are created. Each sheet contains
   a ClosedXML pivot table that references the error data sheet's used range.
   The pivot groups errors by priority level (WK or GEP), class, and error
   type, with configurable report filters and a count aggregation. Column A
   is set to width 105 and column B to width 13.
4. The workbook is saved via ClosedXML to a pipeline output file.
