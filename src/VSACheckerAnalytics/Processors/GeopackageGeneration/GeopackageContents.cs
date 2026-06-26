using Microsoft.Data.Sqlite;

namespace VsaCheckerAnalytics.Processors.GeopackageGeneration;

/// <summary>
/// Registers non-spatial tables and views as <c>attributes</c> layers in the GeoPackage
/// <c>gpkg_contents</c> metadata table, so GIS clients (e.g. QGIS) list them. Spatial feature
/// views register themselves next to their <c>CREATE VIEW</c> in <c>AdditionalViews.sql</c>,
/// which also declares their geometry column in <c>gpkg_geometry_columns</c>.
/// </summary>
internal static class GeopackageContents
{
    /// <summary>
    /// Registers <paramref name="name"/> as an <c>attributes</c> layer in <c>gpkg_contents</c>.
    /// </summary>
    /// <param name="connection">An open SQLite connection to the GeoPackage.</param>
    /// <param name="name">Name of the table or view to register.</param>
    internal static void RegisterAttributes(SqliteConnection connection, string name)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO gpkg_contents
                (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id)
            VALUES (@name, 'attributes', @name, NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL)
            """;
        command.Parameters.AddWithValue("@name", name);
        command.ExecuteNonQuery();
    }
}
