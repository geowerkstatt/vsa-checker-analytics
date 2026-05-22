using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace VsaCheckerAnalytics.Processors.GeopackageGeneration;

[TestClass]
public class ErrorDataMaterializerTest
{
    [TestMethod]
    public async Task Materialize_PopulatesCaErrorData_WithAllSourceRows()
    {
        using var connection = await SetUpAndMaterializeAsync();

        var count = QueryLong(connection, "SELECT COUNT(*) FROM ca_error_data");

        Assert.AreEqual(6L, count);
    }

    [TestMethod]
    public async Task Materialize_EnrichesLeitung_WithFeatureAttributes()
    {
        using var connection = await SetUpAndMaterializeAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT funktionhierarchisch, eigentuemer, status FROM ca_error_data WHERE tid = 'LT001' AND errorid = 't_001'";
        using var reader = cmd.ExecuteReader();

        Assert.IsTrue(reader.Read());
        Assert.AreEqual("PAA.Transportleitung", reader.GetString(0));
        Assert.AreEqual("Gemeinde Aarau", reader.GetString(1));
        Assert.AreEqual("in_Betrieb", reader.GetString(2));
    }

    [TestMethod]
    public async Task Materialize_EnrichesKnoten_WithFeatureAttributes()
    {
        using var connection = await SetUpAndMaterializeAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT funktionhierarchisch, eigentuemer, status FROM ca_error_data WHERE tid = 'KN001' AND errorid = 'fp_001'";
        using var reader = cmd.ExecuteReader();

        Assert.IsTrue(reader.Read());
        Assert.AreEqual("SAA", reader.GetString(0));
        Assert.AreEqual("Gemeinde Aarau", reader.GetString(1));
        Assert.AreEqual("in_Betrieb", reader.GetString(2));
    }

    [TestMethod]
    public async Task Materialize_SetsNullEnrichment_ForNonEnrichedAndUnknownClasses()
    {
        using var connection = await SetUpAndMaterializeAsync();

        var teileinzugsgebietNulls = QueryLong(
            connection,
            "SELECT COUNT(*) FROM ca_error_data WHERE class = 'Teileinzugsgebiet' AND funktionhierarchisch IS NULL AND eigentuemer IS NULL AND status IS NULL");
        var massnahmeNulls = QueryLong(
            connection,
            "SELECT COUNT(*) FROM ca_error_data WHERE class = 'Massnahme' AND funktionhierarchisch IS NULL AND eigentuemer IS NULL AND status IS NULL");

        Assert.AreEqual(1L, teileinzugsgebietNulls);
        Assert.AreEqual(1L, massnahmeNulls);
    }

    [TestMethod]
    public async Task Materialize_DeterminesCheckTypeAndModule_FromCsvModule()
    {
        using var connection = await SetUpAndMaterializeAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT check_type, module FROM ca_error_data ORDER BY errorid";
        using var reader = cmd.ExecuteReader();

        var rows = new List<(string CheckType, string Module)>();
        while (reader.Read())
        {
            rows.Add((reader.GetString(0), reader.GetString(1)));
        }

        var regularRow = rows.First(r => r.CheckType == "A");
        Assert.AreEqual("gep_check", regularRow.Module);

        var readerRow = rows.First(r => r.CheckType == "ig");
        Assert.AreEqual("igcheck", readerRow.Module);
    }

    [TestMethod]
    public async Task Materialize_FallsBackToDescription_WhenCmsgIsNull()
    {
        using var connection = await SetUpAndMaterializeAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT error FROM ca_error_data WHERE tid = 'LT002' AND errorid = 't_003'";
        var error = cmd.ExecuteScalar();

        Assert.AreEqual("CSV Desc 5", error);
    }

    [TestMethod]
    public async Task Materialize_AggregatesCaErrorObject_WithCountAndMaxPriority()
    {
        using var connection = await SetUpAndMaterializeAsync();

        var objectCount = QueryLong(connection, "SELECT COUNT(*) FROM ca_error_object");
        Assert.AreEqual(5L, objectCount);

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT count_error, wk_max, gep_max FROM ca_error_object WHERE tid = 'LT001' AND class = 'Leitung'";
        using var reader = cmd.ExecuteReader();

        Assert.IsTrue(reader.Read());
        Assert.AreEqual(2L, reader.GetInt64(0));
        Assert.AreEqual(2L, reader.GetInt64(1));
        Assert.AreEqual(2L, reader.GetInt64(2));
    }

    [TestMethod]
    public async Task Materialize_IsIdempotent_OnReRun()
    {
        using var connection = CreateOpenConnection();
        CreateTestSchemas(connection);
        SeedTestData(connection);

        var materializer = new ErrorDataMaterializer(connection, NullLogger.Instance);
        materializer.CreateBuildView("v_ca_error_data_build", "v_checker_errors", "DE");

        await materializer.MaterializeAsync("v_ca_error_data_build", CancellationToken.None);
        await materializer.MaterializeAsync("v_ca_error_data_build", CancellationToken.None);

        var errorDataCount = QueryLong(connection, "SELECT COUNT(*) FROM ca_error_data");
        var errorObjectCount = QueryLong(connection, "SELECT COUNT(*) FROM ca_error_object");

        Assert.AreEqual(6L, errorDataCount);
        Assert.AreEqual(5L, errorObjectCount);
    }

    [TestMethod]
    public async Task Materialize_UsesFrenchColumns_WhenLanguageIsFr()
    {
        using var connection = await SetUpAndMaterializeAsync("FR");

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT error, recommendation, recommendation_detail FROM ca_error_data WHERE tid = 'LT001' AND errorid = 't_001'";
        using var reader = cmd.ExecuteReader();

        Assert.IsTrue(reader.Read());
        Assert.AreEqual("Erreur FR 1", reader.GetString(0));
        Assert.AreEqual("Fix FR 1", reader.GetString(1));
        Assert.AreEqual("Ctx FR 1", reader.GetString(2));
    }

    private static async Task<SqliteConnection> SetUpAndMaterializeAsync(string language = "DE")
    {
        var connection = CreateOpenConnection();
        CreateTestSchemas(connection);
        SeedTestData(connection);

        var materializer = new ErrorDataMaterializer(connection, NullLogger.Instance);
        materializer.CreateBuildView("v_ca_error_data_build", "v_checker_errors", language);
        await materializer.MaterializeAsync("v_ca_error_data_build", CancellationToken.None);

        return connection;
    }

    private static SqliteConnection CreateOpenConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        return connection;
    }

    private static void CreateTestSchemas(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE v_checker_errors (
                t_id TEXT, "Tid" TEXT, source TEXT, "Topic" TEXT, "Class" TEXT,
                "ErrorId" TEXT, "Description" TEXT, "UserAttributes" TEXT,
                "Category" TEXT, "Model" TEXT, "Module" TEXT,
                cmsg_de TEXT, cmsg_fr TEXT, prio_uc TEXT, prio_gsp TEXT,
                required_action_de TEXT, required_action_fr TEXT,
                action_context_de TEXT, action_context_fr TEXT);

            CREATE TABLE leitung (
                T_Id INTEGER PRIMARY KEY, T_Ili_Tid TEXT,
                funktionhierarchisch TEXT, astatus TEXT, eigentuemerref INTEGER);
            CREATE TABLE knoten (
                T_Id INTEGER PRIMARY KEY, T_Ili_Tid TEXT,
                funktionhierarchisch TEXT, astatus TEXT, eigentuemerref INTEGER);
            CREATE TABLE teileinzugsgebiet (T_Id INTEGER PRIMARY KEY, T_Ili_Tid TEXT);
            CREATE TABLE sk_regenueberlauf (T_Id INTEGER PRIMARY KEY, T_Ili_Tid TEXT);
            CREATE TABLE sk_regenueberlaufbecken (T_Id INTEGER PRIMARY KEY, T_Ili_Tid TEXT);
            CREATE TABLE sk_einleitstelle (T_Id INTEGER PRIMARY KEY, T_Ili_Tid TEXT);
            CREATE TABLE ueberlauf_foerderaggregat (T_Id INTEGER PRIMARY KEY, T_Ili_Tid TEXT);
            CREATE TABLE organisation (T_Id INTEGER PRIMARY KEY, bezeichnung TEXT);
            """;
        cmd.ExecuteNonQuery();
    }

    private static void SeedTestData(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO v_checker_errors (
                t_id, "Tid", source, "Topic", "Class", "ErrorId", "Description",
                "UserAttributes", "Category", "Model", "Module",
                cmsg_de, cmsg_fr, prio_uc, prio_gsp,
                required_action_de, required_action_fr,
                action_context_de, action_context_fr)
            VALUES
                ('T_1', 'LT001', 'T', 'Topic1', 'Leitung', 't_001', 'CSV Desc 1', 'attr1', 'Warning', '2020', 'igcheck', 'Fehler DE 1', 'Erreur FR 1', '1', '2', 'Fix DE 1', 'Fix FR 1', 'Ctx DE 1', 'Ctx FR 1'),
                ('A_1', 'LT001', 'A', 'Topic1', 'Leitung', 'a_001', 'CSV Desc 2', 'attr2', 'Error',   '2020', 'igcheck', 'Fehler DE 2', 'Erreur FR 2', '2', '1', 'Fix DE 2', 'Fix FR 2', 'Ctx DE 2', 'Ctx FR 2'),
                ('FP_1','KN001', 'FP','Topic2', 'Knoten',  'fp_001','CSV Desc 3', 'attr3', 'Warning', '2020', 'igcheck', 'Fehler DE 3', 'Erreur FR 3', '1', '1', 'Fix DE 3', 'Fix FR 3', 'Ctx DE 3', 'Ctx FR 3'),
                ('T_2', 'TEG001','T', 'Topic3', 'Teileinzugsgebiet','t_002','CSV Desc 4','attr4','Warning','2020','igcheck','Fehler DE 4','Erreur FR 4','1','2','Fix DE 4','Fix FR 4','Ctx DE 4','Ctx FR 4'),
                ('T_3', 'LT002', 'T', 'Topic1', 'Leitung', 't_003', 'CSV Desc 5', 'attr5', 'Error',   '2020', 'reader',  NULL,          NULL,          NULL, NULL, NULL,       NULL,       NULL,       NULL),
                ('T_4', 'UNK001','T', 'Topic4', 'Massnahme','t_004','CSV Desc 6', 'attr6', 'Warning', '2020', 'igcheck', 'Fehler DE 6', 'Erreur FR 6', '1', '1', 'Fix DE 6', 'Fix FR 6', 'Ctx DE 6', 'Ctx FR 6');

            INSERT INTO leitung (T_Id, T_Ili_Tid, funktionhierarchisch, astatus, eigentuemerref)
            VALUES (1, 'LT001', 'PAA.Transportleitung', 'in_Betrieb', 100),
                   (2, 'LT002', 'PAA.Andere', 'ausser_Betrieb', 101);

            INSERT INTO knoten (T_Id, T_Ili_Tid, funktionhierarchisch, astatus, eigentuemerref)
            VALUES (1, 'KN001', 'SAA', 'in_Betrieb', 100);

            INSERT INTO organisation (T_Id, bezeichnung)
            VALUES (100, 'Gemeinde Aarau'),
                   (101, 'Kanton Aargau');
            """;
        cmd.ExecuteNonQuery();
    }

    [TestMethod]
    public async Task MaterializeLeitung_CreatesBuildView_InGeoPackage()
    {
        using var connection = await SetUpAndMaterializeLeitungAsync();

        var viewSql = QueryString(
            connection,
            "SELECT sql FROM sqlite_master WHERE type = 'view' AND name = 'v_ca_leitung_build'");

        Assert.IsNotNull(viewSql);
        StringAssert.Contains(viewSql, "funktionhierarchisch_klasse");
        StringAssert.Contains(viewSql, "baujahr_klasse");
    }

    [TestMethod]
    public async Task MaterializeLeitung_PopulatesCaLeitung_WithCorrectRowCount()
    {
        using var connection = await SetUpAndMaterializeLeitungAsync();

        var count = QueryLong(connection, "SELECT COUNT(*) FROM ca_leitung");

        Assert.AreEqual(3L, count);
    }

    [TestMethod]
    public async Task MaterializeLeitung_ResolvesOrganisationRefs_ToNames()
    {
        using var connection = await SetUpAndMaterializeLeitungAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT betreiber, eigentuemer, datenherr, datenlieferant FROM ca_leitung WHERE tid = 'LT-001'";
        using var reader = cmd.ExecuteReader();

        Assert.IsTrue(reader.Read());
        Assert.AreEqual("Gemeinde Aarau", reader.GetString(0));
        Assert.AreEqual("Kanton Aargau", reader.GetString(1));
        Assert.AreEqual("Gemeinde Aarau", reader.GetString(2));
        Assert.AreEqual("Kanton Aargau", reader.GetString(3));
    }

    [TestMethod]
    public async Task MaterializeLeitung_ResolvesKnotenRefs_ToTids()
    {
        using var connection = await SetUpAndMaterializeLeitungAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT knoten_nachref, knoten_vonref FROM ca_leitung WHERE tid = 'LT-001'";
        using var reader = cmd.ExecuteReader();

        Assert.IsTrue(reader.Read());
        Assert.AreEqual("KN-002", reader.GetString(0));
        Assert.AreEqual("KN-001", reader.GetString(1));
    }

    [TestMethod]
    public async Task MaterializeLeitung_ResolvesSelfRef_ToTid()
    {
        using var connection = await SetUpAndMaterializeLeitungAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT leitung_nachref FROM ca_leitung WHERE tid = 'LT-001'";
        var value = cmd.ExecuteScalar();

        Assert.AreEqual("LT-002", value);
    }

    [TestMethod]
    public async Task MaterializeLeitung_ComputesFunktionhierarchischKlasse_FromPrefix()
    {
        using var connection = await SetUpAndMaterializeLeitungAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT funktionhierarchisch_klasse FROM ca_leitung ORDER BY tid";
        using var reader = cmd.ExecuteReader();

        Assert.IsTrue(reader.Read());
        Assert.AreEqual("PAA", reader.GetString(0));

        Assert.IsTrue(reader.Read());
        Assert.AreEqual("SAA", reader.GetString(0));

        Assert.IsTrue(reader.Read());
        Assert.IsTrue(reader.IsDBNull(0));
    }

    [TestMethod]
    public async Task MaterializeLeitung_ComputesBaujahrKlasse_AsDecadeRange()
    {
        using var connection = await SetUpAndMaterializeLeitungAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT baujahr_klasse FROM ca_leitung ORDER BY tid";
        using var reader = cmd.ExecuteReader();

        Assert.IsTrue(reader.Read());
        Assert.AreEqual("1990-1999", reader.GetString(0));

        Assert.IsTrue(reader.Read());
        Assert.AreEqual("2010-2019", reader.GetString(0));

        Assert.IsTrue(reader.Read());
        Assert.IsTrue(reader.IsDBNull(0));
    }

    [TestMethod]
    public async Task MaterializeLeitung_PreservesScalarAttributes()
    {
        using var connection = await SetUpAndMaterializeLeitungAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT bezeichnung, status, material, lichte_hoehe, kote_von FROM ca_leitung WHERE tid = 'LT-001'";
        using var reader = cmd.ExecuteReader();

        Assert.IsTrue(reader.Read());
        Assert.AreEqual("W7-W6", reader.GetString(0));
        Assert.AreEqual("in_Betrieb", reader.GetString(1));
        Assert.AreEqual("Beton_Spezialbeton", reader.GetString(2));
        Assert.AreEqual(400L, reader.GetInt64(3));
        Assert.AreEqual(472.37, reader.GetDouble(4));
    }

    [TestMethod]
    public async Task MaterializeLeitung_SetsNullRefs_WhenNoMatch()
    {
        using var connection = await SetUpAndMaterializeLeitungAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT betreiber, eigentuemer, knoten_nachref, knoten_vonref, leitung_nachref, rohrprofilref FROM ca_leitung WHERE tid = 'LT-003'";
        using var reader = cmd.ExecuteReader();

        Assert.IsTrue(reader.Read());
        for (var i = 0; i < reader.FieldCount; i++)
        {
            Assert.IsTrue(reader.IsDBNull(i), $"Column {reader.GetName(i)} should be NULL.");
        }
    }

    [TestMethod]
    public async Task MaterializeLeitung_IsIdempotent_OnReRun()
    {
        using var connection = CreateOpenConnection();
        CreateLeitungTestSchema(connection);
        SeedLeitungTestData(connection);

        var materializer = new ErrorDataMaterializer(connection, NullLogger.Instance);
        materializer.CreateLeitungBuildView("v_ca_leitung_build");

        await materializer.MaterializeLeitungAsync("v_ca_leitung_build", CancellationToken.None);
        await materializer.MaterializeLeitungAsync("v_ca_leitung_build", CancellationToken.None);

        var count = QueryLong(connection, "SELECT COUNT(*) FROM ca_leitung");

        Assert.AreEqual(3L, count);
    }

    [TestMethod]
    public async Task MaterializeKnoten_CreatesBuildView_InGeoPackage()
    {
        using var connection = await SetUpAndMaterializeKnotenAsync();

        var viewSql = QueryString(
            connection,
            "SELECT sql FROM sqlite_master WHERE type = 'view' AND name = 'v_ca_knoten_build'");

        Assert.IsNotNull(viewSql);
        StringAssert.Contains(viewSql, "baujahr_klasse");
    }

    [TestMethod]
    public async Task MaterializeKnoten_PopulatesCaKnoten_WithCorrectRowCount()
    {
        using var connection = await SetUpAndMaterializeKnotenAsync();

        var count = QueryLong(connection, "SELECT COUNT(*) FROM ca_knoten");

        Assert.AreEqual(3L, count);
    }

    [TestMethod]
    public async Task MaterializeKnoten_ResolvesOrganisationRefs_ToNames()
    {
        using var connection = await SetUpAndMaterializeKnotenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT betreiber, eigentuemer, datenherr, datenlieferant FROM ca_knoten WHERE tid = 'KN-001'";
        using var reader = cmd.ExecuteReader();

        Assert.IsTrue(reader.Read());
        Assert.AreEqual("Gemeinde Aarau", reader.GetString(0));
        Assert.AreEqual("Kanton Aargau", reader.GetString(1));
        Assert.AreEqual("Gemeinde Aarau", reader.GetString(2));
        Assert.AreEqual("Kanton Aargau", reader.GetString(3));
    }

    [TestMethod]
    public async Task MaterializeKnoten_ComputesBaujahrKlasse_AsDecadeRange()
    {
        using var connection = await SetUpAndMaterializeKnotenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT baujahr_klasse FROM ca_knoten ORDER BY tid";
        using var reader = cmd.ExecuteReader();

        Assert.IsTrue(reader.Read());
        Assert.AreEqual("1990-1999", reader.GetString(0));

        Assert.IsTrue(reader.Read());
        Assert.AreEqual("2010-2019", reader.GetString(0));

        Assert.IsTrue(reader.Read());
        Assert.IsTrue(reader.IsDBNull(0));
    }

    [TestMethod]
    public async Task MaterializeKnoten_PreservesScalarAttributes()
    {
        using var connection = await SetUpAndMaterializeKnotenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT bezeichnung, status, funktion, deckelkote, sohlenkote, dimension1 FROM ca_knoten WHERE tid = 'KN-001'";
        using var reader = cmd.ExecuteReader();

        Assert.IsTrue(reader.Read());
        Assert.AreEqual("S1", reader.GetString(0));
        Assert.AreEqual("in_Betrieb", reader.GetString(1));
        Assert.AreEqual("Pumpwerk", reader.GetString(2));
        Assert.AreEqual(501.02, reader.GetDouble(3));
        Assert.AreEqual(498.50, reader.GetDouble(4));
        Assert.AreEqual(600L, reader.GetInt64(5));
    }

    [TestMethod]
    public async Task MaterializeKnoten_SetsNullRefs_WhenNoMatch()
    {
        using var connection = await SetUpAndMaterializeKnotenAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT betreiber, eigentuemer, datenherr, datenlieferant FROM ca_knoten WHERE tid = 'KN-003'";
        using var reader = cmd.ExecuteReader();

        Assert.IsTrue(reader.Read());
        for (var i = 0; i < reader.FieldCount; i++)
        {
            Assert.IsTrue(reader.IsDBNull(i), $"Column {reader.GetName(i)} should be NULL.");
        }
    }

    [TestMethod]
    public async Task MaterializeKnoten_IsIdempotent_OnReRun()
    {
        using var connection = CreateOpenConnection();
        CreateKnotenTestSchema(connection);
        SeedKnotenTestData(connection);

        var materializer = new ErrorDataMaterializer(connection, NullLogger.Instance);
        materializer.CreateKnotenBuildView("v_ca_knoten_build");

        await materializer.MaterializeKnotenAsync("v_ca_knoten_build", CancellationToken.None);
        await materializer.MaterializeKnotenAsync("v_ca_knoten_build", CancellationToken.None);

        var count = QueryLong(connection, "SELECT COUNT(*) FROM ca_knoten");

        Assert.AreEqual(3L, count);
    }

    private static async Task<SqliteConnection> SetUpAndMaterializeKnotenAsync()
    {
        var connection = CreateOpenConnection();
        CreateKnotenTestSchema(connection);
        SeedKnotenTestData(connection);

        var materializer = new ErrorDataMaterializer(connection, NullLogger.Instance);
        materializer.CreateKnotenBuildView("v_ca_knoten_build");
        await materializer.MaterializeKnotenAsync("v_ca_knoten_build", CancellationToken.None);

        return connection;
    }

    private static void CreateKnotenTestSchema(SqliteConnection connection)
    {
        var sql = """
            CREATE TABLE knoten (
                T_Id INTEGER PRIMARY KEY, T_Ili_Tid TEXT,
                ara_nr INTEGER, baujahr INTEGER, baulicherzustand TEXT,
                bemerkung TEXT, bezeichnung TEXT,
                deckelkote REAL, dimension1 INTEGER, dimension2 INTEGER,
                finanzierung TEXT, funktion TEXT, funktionhierarchisch TEXT,
                lagegenauigkeit TEXT,
                nutzungsart_geplant TEXT, nutzungsart_ist TEXT,
                obj_id_abwasserbauwerk TEXT, obj_id_deckel TEXT,
                rueckstaukote_ist REAL, sanierungsbedarf TEXT, sohlenkote REAL,
                astatus TEXT, symbolori REAL, zugaenglichkeit TEXT,
                zustandserhebung_jahr INTEGER,
                betreiberref INTEGER, eigentuemerref INTEGER,
                datenherrref INTEGER, datenlieferantref INTEGER,
                letzte_aenderung TEXT);

            CREATE TABLE organisation (
                T_Id INTEGER PRIMARY KEY, bezeichnung TEXT);
            """;
        ExecuteNonQuery(connection, sql);
    }

    private static void SeedKnotenTestData(SqliteConnection connection)
    {
        var sql = """
            INSERT INTO organisation (T_Id, bezeichnung)
            VALUES (100, 'Gemeinde Aarau'), (101, 'Kanton Aargau');

            INSERT INTO knoten (
                T_Id, T_Ili_Tid, baujahr, bezeichnung, funktion, funktionhierarchisch,
                astatus, deckelkote, sohlenkote, dimension1,
                betreiberref, eigentuemerref, datenherrref, datenlieferantref)
            VALUES
                (1, 'KN-001', 1995, 'S1', 'Pumpwerk', 'SAA',
                 'in_Betrieb', 501.02, 498.50, 600,
                 100, 101, 100, 101),
                (2, 'KN-002', 2015, 'S2', 'Einlaufschacht', 'PAA',
                 'in_Betrieb', 502.09, NULL, NULL,
                 100, 101, 100, 101),
                (3, 'KN-003', NULL, 'unbekannt', NULL, NULL,
                 NULL, NULL, NULL, NULL,
                 NULL, NULL, NULL, NULL);
            """;
        ExecuteNonQuery(connection, sql);
    }

    private static async Task<SqliteConnection> SetUpAndMaterializeLeitungAsync()
    {
        var connection = CreateOpenConnection();
        CreateLeitungTestSchema(connection);
        SeedLeitungTestData(connection);

        var materializer = new ErrorDataMaterializer(connection, NullLogger.Instance);
        materializer.CreateLeitungBuildView("v_ca_leitung_build");
        await materializer.MaterializeLeitungAsync("v_ca_leitung_build", CancellationToken.None);

        return connection;
    }

    private static void CreateLeitungTestSchema(SqliteConnection connection)
    {
        var sql = """
            CREATE TABLE leitung (
                T_Id INTEGER PRIMARY KEY, T_Ili_Tid TEXT,
                baujahr INTEGER, baulicherzustand TEXT, bemerkung TEXT, bezeichnung TEXT,
                finanzierung TEXT, funktionhierarchisch TEXT, funktionhydraulisch TEXT,
                hoehengenauigkeit_nach TEXT, hoehengenauigkeit_von TEXT,
                hydr_belastung_ist INTEGER, kote_nach REAL, kote_von REAL,
                laengeeffektiv REAL, lagebestimmung TEXT, leckschutz TEXT,
                lichte_breite INTEGER, lichte_hoehe INTEGER, material TEXT,
                nutzungsart_geplant TEXT, nutzungsart_ist TEXT,
                obj_id_abwasserbauwerk TEXT, obj_id_nachhaltungspunkt TEXT, obj_id_vonhaltungspunkt TEXT,
                profiltyp TEXT, reliner_art TEXT, reliner_nennweite INTEGER,
                sanierungsbedarf TEXT, astatus TEXT, wandrauhigkeit REAL,
                wbw_basisjahr INTEGER, wbw_bauart TEXT, wiederbeschaffungswert REAL,
                zustandserhebung_jahr INTEGER,
                betreiberref INTEGER, eigentuemerref INTEGER,
                knoten_nachref INTEGER, knoten_vonref INTEGER,
                leitung_nachref INTEGER, rohrprofilref INTEGER,
                datenherrref INTEGER, datenlieferantref INTEGER,
                letzte_aenderung TEXT);

            CREATE TABLE knoten (
                T_Id INTEGER PRIMARY KEY, T_Ili_Tid TEXT);

            CREATE TABLE organisation (
                T_Id INTEGER PRIMARY KEY, bezeichnung TEXT);

            CREATE TABLE rohrprofil (
                T_Id INTEGER PRIMARY KEY, bezeichnung TEXT);
            """;
        ExecuteNonQuery(connection, sql);
    }

    private static void SeedLeitungTestData(SqliteConnection connection)
    {
        var sql = """
            INSERT INTO organisation (T_Id, bezeichnung)
            VALUES (100, 'Gemeinde Aarau'), (101, 'Kanton Aargau');

            INSERT INTO knoten (T_Id, T_Ili_Tid)
            VALUES (10, 'KN-001'), (11, 'KN-002');

            INSERT INTO rohrprofil (T_Id, bezeichnung)
            VALUES (50, 'DN400');

            INSERT INTO leitung (
                T_Id, T_Ili_Tid, baujahr, bezeichnung, funktionhierarchisch,
                astatus, material, lichte_hoehe, kote_von,
                betreiberref, eigentuemerref, knoten_nachref, knoten_vonref,
                leitung_nachref, rohrprofilref, datenherrref, datenlieferantref)
            VALUES
                (1, 'LT-001', 1995, 'W7-W6', 'PAA.Sammelkanal',
                 'in_Betrieb', 'Beton_Spezialbeton', 400, 472.37,
                 100, 101, 11, 10, 2, 50, 100, 101),
                (2, 'LT-002', 2015, '50.14-50.13', 'SAA.Strassenentwaesserung',
                 'in_Betrieb', 'Kunststoff', 250, NULL,
                 100, 101, 10, 11, NULL, NULL, 100, 101),
                (3, 'LT-003', NULL, 'unbekannt', NULL,
                 NULL, NULL, NULL, NULL,
                 NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
            """;
        ExecuteNonQuery(connection, sql);
    }

    [SuppressMessage("Security", "CA2100", Justification = "Test queries use hardcoded SQL, not user input.")]
    private static long QueryLong(SqliteConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        return (long)(cmd.ExecuteScalar() ?? 0L);
    }

    [SuppressMessage("Security", "CA2100", Justification = "Test queries use hardcoded SQL, not user input.")]
    private static string? QueryString(SqliteConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        return cmd.ExecuteScalar() as string;
    }

    [SuppressMessage("Security", "CA2100", Justification = "Test queries use hardcoded SQL, not user input.")]
    private static void ExecuteNonQuery(SqliteConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}
