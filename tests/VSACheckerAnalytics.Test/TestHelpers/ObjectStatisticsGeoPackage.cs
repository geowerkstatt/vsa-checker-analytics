using Microsoft.Data.Sqlite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using VsaCheckerAnalytics.Processors.GeopackageGeneration;
using VsaCheckerAnalytics.Processors.NetworkTopologyPatcher;

namespace VsaCheckerAnalytics.TestHelpers;

/// <summary>
/// Builds a GeoPackage from a model template with the real views of <c>AdditionalViews.sql</c> and a few
/// Teileinzugsgebiete, Knoten and Leitungen, the sources of the object statistics in the error overview.
/// </summary>
internal static class ObjectStatisticsGeoPackage
{
    private const int Srid = 2056;

    /// <summary>
    /// Copies <paramref name="templateFileName"/> from the test data into <paramref name="directory"/>, creates the
    /// analytical views and, unless <paramref name="withObjects"/> is false, seeds the standard objects:
    /// <list type="bullet">
    ///   <item>Teileinzugsgebiete <c>TEG1</c> (Mischsystem, with perimeter) and <c>TEG2</c> (Trennsystem, no perimeter).</item>
    ///   <item>Knoten <c>KN1</c> (PAA, with detail geometry) and <c>KN2</c> (SAA, without).</item>
    ///   <item>Leitungen <c>LT1</c> (PAA.Sammelkanal), <c>LT2</c> (SAA.Strassenentwaesserung) and <c>LT3</c> (no values, no geometry), all built in 1989.</item>
    /// </list>
    /// </summary>
    internal static string Create(string directory, string templateFileName = "template_ca_dssmini_2020_1_d.gpkg", bool withObjects = true)
    {
        var path = Path.Combine(directory, $"objects-{Guid.NewGuid():N}.gpkg");
        File.Copy(Path.Combine(AppContext.BaseDirectory, "Testdata", templateFileName), path);

        using var connection = Open(path);
        EmbeddedSql.Execute(connection, "AdditionalViews.sql");

        if (withObjects)
        {
            InsertTeileinzugsgebiet(connection, 1, "TEG1", "Mischsystem", "POLYGON ((2600000 1200000, 2600010 1200000, 2600010 1200010, 2600000 1200000))");
            InsertTeileinzugsgebiet(connection, 2, "TEG2", "Trennsystem", null);

            InsertKnoten(connection, 10, "KN1", "PAA", "Kontroll_Einsteigschacht", "POINT (2600100 1200100)", "POLYGON ((2600099 1200099, 2600101 1200099, 2600101 1200101, 2600099 1200099))");
            InsertKnoten(connection, 11, "KN2", "SAA", "Einlaufschacht", "POINT (2600200 1200200)", null);

            InsertLeitung(connection, 20, "LT1", "PAA.Sammelkanal", "Mischabwasser", new WKTReader().Read("LINESTRING (2600100 1200100, 2600200 1200200)"));
            InsertLeitung(connection, 21, "LT2", "SAA.Strassenentwaesserung", "Niederschlagsabwasser", new WKTReader().Read("LINESTRING (2600200 1200200, 2600300 1200300)"));
            InsertLeitung(connection, 22, "LT3", null, null, null);
        }

        return path;
    }

    /// <summary>
    /// Adds a Leitung to the GeoPackage at <paramref name="path"/>.
    /// </summary>
    internal static void InsertLeitung(string path, long tId, string tid, string? funktionHierarchisch, string? nutzungsartIst, Geometry? verlauf)
    {
        using var connection = Open(path);
        InsertLeitung(connection, tId, tid, funktionHierarchisch, nutzungsartIst, verlauf);
    }

    private static SqliteConnection Open(string path)
    {
        var connection = new SqliteConnection($"Data Source={path};Pooling=false");
        connection.Open();

        // Only the rows the views read are seeded, without baskets or organisations to reference.
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = OFF";
        command.ExecuteNonQuery();
        return connection;
    }

    private static void InsertTeileinzugsgebiet(SqliteConnection connection, long tId, string tid, string entwaesserungssystemIst, string? perimeterWkt)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO teileinzugsgebiet (T_Id, T_basket, T_Ili_Tid, bezeichnung, letzte_aenderung, entwaesserungssystem_ist, perimeter)
            VALUES (@tId, 1, @tid, @tid, '2024-01-01', @entwaesserungssystemIst, @perimeter)
            """;
        command.Parameters.AddWithValue("@tId", tId);
        command.Parameters.AddWithValue("@tid", tid);
        command.Parameters.AddWithValue("@entwaesserungssystemIst", entwaesserungssystemIst);
        command.Parameters.AddWithValue("@perimeter", Blob(perimeterWkt));
        command.ExecuteNonQuery();
    }

    private static void InsertKnoten(SqliteConnection connection, long tId, string tid, string funktionHierarchisch, string funktion, string lageWkt, string? detailgeometrieWkt)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO knoten (T_Id, T_basket, T_Ili_Tid, bezeichnung, eigentuemerref, letzte_aenderung, funktionhierarchisch, funktion, detailgeometrie)
            VALUES (@tId, 1, @tid, @tid, 1, '2024-01-01', @funktionHierarchisch, @funktion, @detailgeometrie);

            INSERT INTO knoten_lage (T_Id, T_basket, lage) VALUES (@tId, 1, @lage);
            """;
        command.Parameters.AddWithValue("@tId", tId);
        command.Parameters.AddWithValue("@tid", tid);
        command.Parameters.AddWithValue("@funktionHierarchisch", funktionHierarchisch);
        command.Parameters.AddWithValue("@funktion", funktion);
        command.Parameters.AddWithValue("@detailgeometrie", Blob(detailgeometrieWkt));
        command.Parameters.AddWithValue("@lage", Blob(lageWkt));
        command.ExecuteNonQuery();
    }

    private static void InsertLeitung(SqliteConnection connection, long tId, string tid, string? funktionHierarchisch, string? nutzungsartIst, Geometry? verlauf)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO leitung (T_Id, T_basket, T_Ili_Tid, bezeichnung, eigentuemerref, letzte_aenderung, baujahr, funktionhierarchisch, nutzungsart_ist, verlauf)
            VALUES (@tId, 1, @tid, @tid, 1, '2024-01-01', 1989, @funktionHierarchisch, @nutzungsartIst, @verlauf)
            """;
        command.Parameters.AddWithValue("@tId", tId);
        command.Parameters.AddWithValue("@tid", tid);
        command.Parameters.AddWithValue("@funktionHierarchisch", (object?)funktionHierarchisch ?? DBNull.Value);
        command.Parameters.AddWithValue("@nutzungsartIst", (object?)nutzungsartIst ?? DBNull.Value);
        command.Parameters.AddWithValue("@verlauf", verlauf is null ? DBNull.Value : GeoPackageGeometryCodec.WriteGeometry(verlauf, Srid));
        command.ExecuteNonQuery();
    }

    private static object Blob(string? wkt)
        => wkt is null ? DBNull.Value : GeoPackageGeometryCodec.WriteGeometry(new WKTReader().Read(wkt), Srid);
}
