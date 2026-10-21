using System.Text.Json.Serialization;

namespace VsaCheckerAnalytics.Processors.ErrorOverviewExport;

/// <summary>
/// The configuration of one data sheet of the error overview, written in the pipeline YAML as
/// <c>{ sheet: Name, columns: { ... } }</c>.
/// </summary>
/// <param name="Sheet">The name of the data sheet.</param>
/// <param name="Columns">Maps the attribute keys of the sheet's source to their Excel column and header.</param>
public sealed record DataSheetConfig(
    [property: JsonPropertyName("sheet")] string Sheet,
    [property: JsonPropertyName("columns")] IReadOnlyDictionary<string, ColumnConfig> Columns);

/// <summary>
/// The column of one attribute on a data sheet, written as <c>attribute: { column: A, header: Text }</c>.
/// </summary>
/// <param name="Column">The Excel column letter the attribute fills, e.g. <c>A</c>.</param>
/// <param name="Header">The header text in row 1 of that column.</param>
public sealed record ColumnConfig(
    [property: JsonPropertyName("column")] string Column,
    [property: JsonPropertyName("header")] string Header);

/// <summary>
/// A pivot sheet over a data sheet, fixed in code. Its fields are attribute keys of the data sheet's configured columns.
/// </summary>
/// <param name="Sheet">The name of the pivot sheet.</param>
/// <param name="Location">The top left cell of the pivot table, report filters included.</param>
/// <param name="Rows">The row fields, outermost first.</param>
/// <param name="Filters">The report filter fields.</param>
/// <param name="Values">The value columns.</param>
/// <param name="Subtotals">Whether each group of an outer row field shows Excel's automatic subtotals.</param>
/// <param name="ColumnWidths">The widths of the pivot sheet's first columns.</param>
internal sealed record PivotConfig(
    string Sheet,
    string Location,
    IReadOnlyList<string> Rows,
    IReadOnlyList<string> Filters,
    IReadOnlyList<PivotValueConfig> Values,
    bool Subtotals,
    IReadOnlyList<double> ColumnWidths);

/// <summary>
/// A value column of a pivot sheet: the count of <see cref="Field"/>, or with <see cref="Share"/> its share of the total.
/// </summary>
/// <param name="Field">The attribute key whose rows are counted.</param>
/// <param name="Name">The header of the value column.</param>
/// <param name="Share">Whether the count is shown as a share of the total.</param>
internal sealed record PivotValueConfig(string Field, string Name, bool Share);
