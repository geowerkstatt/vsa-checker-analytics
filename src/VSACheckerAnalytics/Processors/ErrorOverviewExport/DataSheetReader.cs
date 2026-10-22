using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using VsaCheckerAnalytics.Processors.NetworkTopologyPatcher;

namespace VsaCheckerAnalytics.Processors.ErrorOverviewExport;

/// <summary>
/// Reads a <see cref="SheetDefinition"/> from the generated GeoPackage into an immutable <see cref="DataSheet"/>, with
/// GeoPackage geometries as WKT.
/// </summary>
internal static class DataSheetReader
{
    // Excel stores at most this many characters in a cell.
    private const int MaxCellLength = 32767;

    /// <summary>
    /// Reads the configured attributes of every row of the sheet's source; a missing source or attribute throws.
    /// Geometries that cannot be read, or whose WKT exceeds what an Excel cell holds, come back as <see langword="null"/>.
    /// </summary>
    /// <param name="connection">An open connection to the generated GeoPackage.</param>
    /// <param name="sheet">The sheet to read.</param>
    /// <param name="logger">Logger for skipped geometries.</param>
    /// <returns>The sheet's rows, in the order of its columns.</returns>
    [SuppressMessage("Security", "CA2100", Justification = "The source is an internal constant, the attributes are validated plain column names.")]
    internal static DataSheet Read(SqliteConnection connection, SheetDefinition sheet, ILogger logger)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT {string.Join(", ", sheet.Columns.Select(column => $"\"{column.Attribute}\""))} FROM {sheet.Source}";
        using var reader = command.ExecuteReader();

        var rows = new List<IReadOnlyList<object?>>();
        var skippedGeometries = 0;
        while (reader.Read())
        {
            var values = new object?[reader.FieldCount];
            for (var column = 0; column < values.Length; column++)
            {
                var value = reader.IsDBNull(column) ? null : reader.GetValue(column);

                // GeoPackage geometries are the only blobs in the sources.
                if (value is byte[] geometry)
                {
                    value = ToWkt(geometry);
                    if (value is null)
                        skippedGeometries++;
                }

                values[column] = value;
            }

            rows.Add(values);
        }

        if (skippedGeometries > 0)
        {
            logger.LogWarning(
                "{Count} geometries on sheet '{Sheet}' left empty: unreadable, or longer than the {MaxCellLength} characters an Excel cell holds.",
                skippedGeometries,
                sheet.Name,
                MaxCellLength);
        }

        logger.LogDebug("Read {RowCount} rows for sheet '{Sheet}'.", rows.Count, sheet.Name);
        return new DataSheet(sheet.Name, sheet.Columns.Select(column => column.Column).ToList(), rows, sheet.Pivots);
    }

    private static string? ToWkt(byte[] geometry)
    {
        try
        {
            var wkt = GeoPackageGeometryCodec.ReadGeometry(geometry, out _).AsText();
            return wkt.Length <= MaxCellLength ? wkt : null;
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }
}
