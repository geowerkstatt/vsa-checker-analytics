namespace VsaCheckerAnalytics.Processors.ErrorOverviewExport;

/// <summary>
/// A data sheet of the error overview: a header row over the <see cref="Rows"/>, whose values are numbers, text or
/// <see langword="null"/>, and the pivot sheets that summarize it.
/// </summary>
internal sealed record DataSheet(string Name, IReadOnlyList<SheetColumn> Columns, IReadOnlyList<IReadOnlyList<object?>> Rows, IReadOnlyList<PivotSheet> Pivots);

/// <summary>
/// A column of a <see cref="DataSheet"/>: its number (1 for column A) and its header.
/// </summary>
internal sealed record SheetColumn(int Number, string Header);

/// <summary>
/// A pivot sheet over a <see cref="DataSheet"/>. Its pivot table starts at <see cref="Location"/>, report filters
/// included, groups the rows by the <see cref="RowFields"/> (outermost first, with or without Excel's automatic
/// <see cref="Subtotals"/> per group) and shows the <see cref="Values"/>. All fields are headers of the data sheet.
/// <see cref="ColumnWidths"/> sets the widths of the sheet's first columns.
/// </summary>
internal sealed record PivotSheet(
    string SheetName,
    string Location,
    IReadOnlyList<string> RowFields,
    bool Subtotals,
    IReadOnlyList<string> Filters,
    IReadOnlyList<PivotValue> Values,
    IReadOnlyList<double> ColumnWidths);

/// <summary>
/// A value column of a <see cref="PivotSheet"/>: the count of <see cref="Field"/>, as a number or as a share of the total.
/// </summary>
internal sealed record PivotValue(string Field, string Name, bool ShareOfTotal);
