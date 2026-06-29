using Microsoft.Data.Sqlite;
using NetTopologySuite.Geometries;

namespace VsaCheckerAnalytics.Geopackage;

/// <summary>
/// Writes GeoPackage layer metadata so GIS clients (e.g. QGIS) auto-discover tables and views
/// as renderable layers. Non-spatial layers are registered in <c>gpkg_contents</c> as
/// <c>attributes</c>; spatial layers are registered as <c>features</c> and additionally declare
/// their geometry column in <c>gpkg_geometry_columns</c>.
/// </summary>
internal static class GeopackageMetadata
{
    /// <summary>
    /// Registers <paramref name="tableName"/> as a non-spatial <c>attributes</c> layer in <c>gpkg_contents</c>.
    /// </summary>
    /// <param name="connection">An open SQLite connection to the GeoPackage.</param>
    /// <param name="tableName">Name of the table or view to register.</param>
    internal static void RegisterAttributes(SqliteConnection connection, string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO gpkg_contents
                (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id)
            VALUES (@name, 'attributes', @name, NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL)
            """;
        command.Parameters.AddWithValue("@name", tableName);
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Registers <paramref name="tableName"/> as a spatial <c>features</c> layer: an entry in
    /// <c>gpkg_contents</c> plus a geometry-column declaration in <c>gpkg_geometry_columns</c>.
    /// The bounding box is left empty; call <see cref="SetLayerExtent"/> once geometries are written.
    /// </summary>
    /// <param name="connection">An open SQLite connection to the GeoPackage.</param>
    /// <param name="tableName">Name of the table or view to register.</param>
    /// <param name="geometryColumn">Name of the geometry column.</param>
    /// <param name="geometryType">GeoPackage geometry type name (e.g. <c>POINT</c>, <c>LINESTRING</c>).</param>
    /// <param name="srid">Spatial reference system identifier.</param>
    internal static void RegisterFeature(
        SqliteConnection connection,
        string tableName,
        string geometryColumn,
        string geometryType,
        int srid)
    {
        using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, srs_id)
                VALUES (@name, 'features', @name, NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), @srid)
                """;
            command.Parameters.AddWithValue("@name", tableName);
            command.Parameters.AddWithValue("@srid", srid);
            command.ExecuteNonQuery();
        }

        using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m)
                VALUES (@name, @column, @type, @srid, 0, 0)
                """;
            command.Parameters.AddWithValue("@name", tableName);
            command.Parameters.AddWithValue("@column", geometryColumn);
            command.Parameters.AddWithValue("@type", geometryType);
            command.Parameters.AddWithValue("@srid", srid);
            command.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Updates the <c>gpkg_contents</c> bounding box of <paramref name="tableName"/> to the
    /// given <paramref name="extent"/>. No-op when the extent is empty (no features written).
    /// </summary>
    /// <param name="connection">An open SQLite connection to the GeoPackage.</param>
    /// <param name="tableName">Name of the registered feature layer.</param>
    /// <param name="extent">Bounding box of the written geometries.</param>
    internal static void SetLayerExtent(SqliteConnection connection, string tableName, Envelope extent)
    {
        if (extent.IsNull)
        {
            return;
        }

        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE gpkg_contents SET min_x = @min_x, min_y = @min_y, max_x = @max_x, max_y = @max_y
            WHERE table_name = @name
            """;
        command.Parameters.AddWithValue("@name", tableName);
        command.Parameters.AddWithValue("@min_x", extent.MinX);
        command.Parameters.AddWithValue("@min_y", extent.MinY);
        command.Parameters.AddWithValue("@max_x", extent.MaxX);
        command.Parameters.AddWithValue("@max_y", extent.MaxY);
        command.ExecuteNonQuery();
    }
}
