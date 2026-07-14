using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics.CodeAnalysis;
using VsaCheckerAnalytics.TestHelpers;

namespace VsaCheckerAnalytics.Processors.GeopackageGeneration;

[TestClass]
public class ErrorDataMaterializerTest
{
    [TestMethod]
    public async Task Materialize_DeduplicatesAcrossProfiles_AndDropsSuppressedReaderRows()
    {
        using var connection = await SetUpAndMaterializeAsync();

        // 9 seeded CSV rows dedupe to 8 groups. OBJ_ID_Abwasserbauwerk is suppressed and the
        // unknown igcheck error 9999 is excluded (it becomes an orphan) => 6 rows remain.
        var count = QueryLong(connection, "SELECT COUNT(*) FROM ca_error_data");
        Assert.AreEqual(6L, count);

        var suppressed = QueryLong(
            connection,
            "SELECT COUNT(*) FROM ca_error_data WHERE detail LIKE '%OBJ_ID_Abwasserbauwerk%'");
        Assert.AreEqual(0L, suppressed);
    }

    [TestMethod]
    public async Task Materialize_AggregatesCheckType_AcrossProfiles()
    {
        using var connection = await SetUpAndMaterializeAsync();

        var checkType = QueryString(
            connection,
            "SELECT check_type FROM ca_error_data WHERE tid = 'LT001' AND errorid = '1001'");

        Assert.AreEqual("vsa-a; vsa-t", checkType);
    }

    [TestMethod]
    public async Task Materialize_MapsModule_FromCsvModule()
    {
        using var connection = await SetUpAndMaterializeAsync();

        Assert.AreEqual("gep_check", QueryString(connection, "SELECT module FROM ca_error_data WHERE errorid = '1001'"));
        Assert.AreEqual("igcheck", QueryString(connection, "SELECT module FROM ca_error_data WHERE errorid = '11' AND detail LIKE 'BetreiberRef%'"));
        Assert.AreEqual("ig", QueryString(connection, "SELECT check_type FROM ca_error_data WHERE errorid = '11' AND detail LIKE 'BetreiberRef%'"));
    }

    [TestMethod]
    public async Task Materialize_EnrichesIgcheck_FromErrorMatrix_WithFeatureAttributes()
    {
        using var connection = await SetUpAndMaterializeAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT error, recommendation, recommendation_detail, wk, gep, funktionhierarchisch, eigentuemer, status FROM ca_error_data WHERE tid = 'LT001' AND errorid = '1001'";
        using var reader = cmd.ExecuteReader();

        Assert.IsTrue(reader.Read());
        Assert.AreEqual("Fehler 1001", reader.GetString(0));
        Assert.AreEqual("Fix 1001", reader.GetString(1));
        Assert.AreEqual("Ctx 1001", reader.GetString(2));
        Assert.AreEqual(3L, reader.GetInt64(3));
        Assert.AreEqual(2L, reader.GetInt64(4));
        Assert.AreEqual("PAA.Transportleitung", reader.GetString(5));
        Assert.AreEqual("Gemeinde Aarau", reader.GetString(6));
        Assert.AreEqual("in_Betrieb", reader.GetString(7));
    }

    [TestMethod]
    public async Task Materialize_ExcludesUnknownIgcheckError_WhenNoMatrixRow()
    {
        using var connection = await SetUpAndMaterializeAsync();

        // 9999 has no error matrix row, so it is unknown and must not reach ca_error_data;
        // it surfaces via the orphan detection instead.
        var count = QueryLong(connection, "SELECT COUNT(*) FROM ca_error_data WHERE tid = 'KN003' AND errorid = '9999'");

        Assert.AreEqual(0L, count);
    }

    [TestMethod]
    public async Task Materialize_RendersReaderBaseTemplate_WithExtractedAttribute()
    {
        using var connection = await SetUpAndMaterializeAsync();

        var error = QueryString(
            connection,
            "SELECT error FROM ca_error_data WHERE errorid = '11' AND detail LIKE 'DatenherrRef%'");

        Assert.AreEqual("Pflichtattribut DatenherrRef fehlt", error);
    }

    [TestMethod]
    public async Task Materialize_PrefersAttributeOverride_OverBaseRow()
    {
        using var connection = await SetUpAndMaterializeAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT error, wk, gep FROM ca_error_data WHERE errorid = '11' AND detail LIKE 'BetreiberRef%'";
        using var reader = cmd.ExecuteReader();

        Assert.IsTrue(reader.Read());
        Assert.AreEqual("Pflichtattribut BetreiberRef fehlt", reader.GetString(0));
        Assert.AreEqual(2L, reader.GetInt64(1));
        Assert.AreEqual(2L, reader.GetInt64(2));
    }

    [TestMethod]
    public async Task Materialize_AppliesConditionOverride_WhenObjectAttributeMatches()
    {
        using var connection = await SetUpAndMaterializeAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT error, wk FROM ca_error_data WHERE errorid = '11' AND detail LIKE 'FunktionHierarchisch%'";
        using var reader = cmd.ExecuteReader();

        Assert.IsTrue(reader.Read());
        Assert.AreEqual("Pflichtattribut FunktionHierarchisch fehlt (SAA-Pflichtfeld)", reader.GetString(0));
        Assert.AreEqual(1L, reader.GetInt64(1));
    }

    [TestMethod]
    public async Task Materialize_ExtractsLengthAndMax_ForTooLongReaderError()
    {
        using var connection = await SetUpAndMaterializeAsync();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT error, recommendation_detail FROM ca_error_data WHERE errorid = '12'";
        using var reader = cmd.ExecuteReader();

        Assert.IsTrue(reader.Read());
        Assert.AreEqual("OBJ_ID zu lang: 20 Zeichen (Max. 16). Muss <= 16 Zeichen sein.", reader.GetString(0));
        Assert.AreEqual("OBJ_ID auf maximal 16 Zeichen kürzen.", reader.GetString(1));
    }

    [TestMethod]
    public async Task Materialize_SetsNullEnrichment_ForNonEnrichedClass()
    {
        using var connection = await SetUpAndMaterializeAsync();

        var nulls = QueryLong(
            connection,
            "SELECT COUNT(*) FROM ca_error_data WHERE class = 'Teileinzugsgebiet' AND funktionhierarchisch IS NULL AND eigentuemer IS NULL AND status IS NULL");

        Assert.AreEqual(1L, nulls);
    }

    [TestMethod]
    public async Task Materialize_AggregatesCaErrorObject_WithCountAndMaxPriority()
    {
        using var connection = await SetUpAndMaterializeAsync();

        // Objects: (LT001, Leitung), (KN001, Knoten), (TEG001, Teileinzugsgebiet).
        // KN003 had only the unknown error 9999, which is excluded, so it produces no object.
        Assert.AreEqual(3L, QueryLong(connection, "SELECT COUNT(*) FROM ca_error_object"));

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT count_error, wk_max, gep_max FROM ca_error_object WHERE tid = 'LT001' AND class = 'Leitung'";
        using var reader = cmd.ExecuteReader();

        Assert.IsTrue(reader.Read());
        Assert.AreEqual(3L, reader.GetInt64(0));
        Assert.AreEqual(3L, reader.GetInt64(1));
        Assert.AreEqual(2L, reader.GetInt64(2));
    }

    [TestMethod]
    public async Task Materialize_RendersFrench_AndTranslatesRoleName()
    {
        using var connection = await SetUpAndMaterializeAsync("FR");

        var error = QueryString(
            connection,
            "SELECT error FROM ca_error_data WHERE errorid = '11' AND detail LIKE 'DatenherrRef%'");

        Assert.AreEqual("Attribut obligatoire MAITRE_DES_DONNEESRef manquant", error);
    }

    [TestMethod]
    public async Task Materialize_PartitionsRows_KnownToData_UnknownToOrphans_SuppressedToNeither()
    {
        using var connection = await SetUpAndMaterializeAsync();

        new OrphanInspector(connection, NullLogger.Instance)
            .MaterializeOrphans("ca_error_orphans", "v_checker_csv_classified");

        // Unknown igcheck error 9999: in orphans, not in ca_error_data.
        Assert.AreEqual(0L, QueryLong(connection, "SELECT COUNT(*) FROM ca_error_data WHERE errorid = '9999'"));
        Assert.AreEqual(1L, QueryLong(connection, "SELECT COUNT(*) FROM ca_error_orphans WHERE \"ErrorId\" = '9999'"));

        // Known error 1001: in ca_error_data, not in orphans.
        Assert.IsGreaterThanOrEqualTo(1L, QueryLong(connection, "SELECT COUNT(*) FROM ca_error_data WHERE errorid = '1001'"));
        Assert.AreEqual(0L, QueryLong(connection, "SELECT COUNT(*) FROM ca_error_orphans WHERE \"ErrorId\" = '1001'"));

        // Suppressed reader error (OBJ_ID_Abwasserbauwerk on KN002): in neither table.
        Assert.AreEqual(0L, QueryLong(connection, "SELECT COUNT(*) FROM ca_error_data WHERE detail LIKE '%OBJ_ID_Abwasserbauwerk%'"));
        Assert.AreEqual(0L, QueryLong(connection, "SELECT COUNT(*) FROM ca_error_orphans WHERE \"Tid\" = 'KN002'"));

        // Orphans are exactly the rows the classified view flags as unknown.
        Assert.AreEqual(
            QueryLong(connection, "SELECT COUNT(*) FROM v_checker_csv_classified WHERE is_known = 0"),
            QueryLong(connection, "SELECT COUNT(*) FROM ca_error_orphans"));
    }

    [TestMethod]
    public async Task Materialize_ExtractsAttributeAndTid_ForBadReferenceReaderError()
    {
        using var connection = await SetUpAndMaterializeExtractionAsync();

        var error = QueryString(connection, "SELECT error FROM ca_error_data WHERE errorid = '15'");

        Assert.AreEqual("Referenz KnotenRef zeigt auf unbekanntes Objekt (TID=xyz-1)", error);
    }

    [TestMethod]
    public async Task Materialize_ExtractsAttribute_ForWrongTargetClassReaderError()
    {
        using var connection = await SetUpAndMaterializeExtractionAsync();

        var error = QueryString(connection, "SELECT error FROM ca_error_data WHERE errorid = '21'");

        Assert.AreEqual("Referenz TypeRef zeigt auf falsche Zielklasse", error);
    }

    [TestMethod]
    public async Task Materialize_ExtractsConstraintName_ForFailedConstraintReaderError()
    {
        using var connection = await SetUpAndMaterializeExtractionAsync();

        var error = QueryString(connection, "SELECT error FROM ca_error_data WHERE errorid = '60'");

        Assert.AreEqual("MANDATORY Constraint MyC verletzt", error);
    }

    [TestMethod]
    public async Task Materialize_ExtractsViolatingValues_ForUniqueConstraintReaderError()
    {
        using var connection = await SetUpAndMaterializeExtractionAsync();

        var error = QueryString(connection, "SELECT error FROM ca_error_data WHERE errorid = '70' AND tid = 'X70'");

        Assert.AreEqual("UNIQUE Constraint verletzt für: a,b", error);
    }

    [TestMethod]
    public async Task Materialize_MissingClosingParen_LeavesValuesEmpty_ForUniqueConstraintReaderError()
    {
        using var connection = await SetUpAndMaterializeExtractionAsync();

        // The "(values=" opener has no closing ")", so the values extract to NULL and {ATTRS} renders empty.
        var error = QueryString(connection, "SELECT error FROM ca_error_data WHERE errorid = '70' AND tid = 'X70B'");

        Assert.AreEqual("UNIQUE Constraint verletzt für: ", error);
    }

    [TestMethod]
    public async Task Materialize_RendersBaseTooLongTemplate_WithAllExtractedParameters()
    {
        using var connection = await SetUpAndMaterializeExtractionAsync();

        var error = QueryString(
            connection,
            "SELECT error FROM ca_error_data WHERE errorid = '12' AND detail LIKE 'the value of MyAttr%'");

        Assert.AreEqual("Attribut MyAttr zu lang (20 Zeichen, max. 16)", error);
    }

    [TestMethod]
    public async Task Materialize_MissingMaxAnchor_LeavesPlaceholderEmpty_WithoutGarbage()
    {
        using var connection = await SetUpAndMaterializeExtractionAsync();

        // The message lacks the "> " anchor, so length and max extract to NULL. The row must still
        // materialize and render with empty {N}/{MAX} placeholders rather than a garbage substring.
        var error = QueryString(
            connection,
            "SELECT error FROM ca_error_data WHERE errorid = '12' AND detail LIKE '%BadAttr%'");

        Assert.AreEqual("Attribut BadAttr zu lang ( Zeichen, max. )", error);
    }

    private static async Task<SqliteConnection> SetUpAndMaterializeAsync(string language = "DE")
    {
        var connection = CreateOpenConnection();
        CreateTestSchemas(connection);
        SeedTestData(connection);

        new ViewCreator(connection).CreateCheckerCsvClassifiedView(
            "v_checker_csv_classified", "v_checker_csv_all", "error_matrix", language);

        var materializer = new ErrorDataMaterializer(connection, NullLogger.Instance);
        materializer.CreateBuildView("v_ca_error_data_build", "v_checker_csv_classified", "error_matrix", "reader_error_rules", language);
        await materializer.MaterializeAsync("v_ca_error_data_build", CancellationToken.None);

        return connection;
    }

    private static SqliteConnection CreateOpenConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        GeopackageMetadataSchema.Create(connection);
        return connection;
    }

    private static void CreateTestSchemas(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE v_checker_csv_all (
                t_id TEXT, "Tid" TEXT, source TEXT, "Topic" TEXT, "Class" TEXT,
                "ErrorId" TEXT, "Description" TEXT, "Model" TEXT, "Module" TEXT);

            CREATE TABLE error_matrix (
                t_id INTEGER PRIMARY KEY AUTOINCREMENT,
                cid TEXT, checkmodel TEXT, ccat TEXT, model TEXT,
                class_de TEXT, class_fr TEXT,
                cmsg_de TEXT, cmsg_fr TEXT, cmsg_it TEXT,
                prio_uc TEXT, prio_gsp TEXT,
                required_action_de TEXT, required_action_fr TEXT, required_action_it TEXT,
                action_context_de TEXT, action_context_fr TEXT, action_context_it TEXT);

            CREATE TABLE reader_error_rules (
                rule_id INTEGER PRIMARY KEY AUTOINCREMENT,
                error_id INTEGER, attr_name TEXT, condition_col TEXT, condition_val TEXT,
                msg_template_de TEXT, msg_template_fr TEXT, msg_template_it TEXT,
                recommendation_de TEXT, recommendation_fr TEXT, recommendation_it TEXT,
                recommendation_detail_de TEXT, recommendation_detail_fr TEXT, recommendation_detail_it TEXT,
                wk INTEGER, gep INTEGER, suppress INTEGER NOT NULL DEFAULT 0);

            CREATE TABLE leitung (
                T_Id INTEGER PRIMARY KEY, T_Ili_Tid TEXT,
                funktionhierarchisch TEXT, astatus TEXT, eigentuemerref INTEGER);
            CREATE TABLE knoten (
                T_Id INTEGER PRIMARY KEY, T_Ili_Tid TEXT,
                funktionhierarchisch TEXT, astatus TEXT, eigentuemerref INTEGER);
            CREATE TABLE organisation (T_Id INTEGER PRIMARY KEY, bezeichnung TEXT);
            """;
        cmd.ExecuteNonQuery();
    }

    private static void SeedTestData(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO v_checker_csv_all (t_id, "Tid", source, "Topic", "Class", "ErrorId", "Description", "Model", "Module")
            VALUES
                ('a_01', 'LT001', 'A',  'T1', 'Leitung',           '1001', 'MANDATORY Abflussbegrenzung', '2020', 'igcheck'),
                ('t_01', 'LT001', 'T',  'T1', 'Leitung',           '1001', 'MANDATORY Abflussbegrenzung', '2020', 'igcheck'),
                ('a_02', 'LT001', 'A',  'T1', 'Leitung',           '11',   'BetreiberRef has to be defined', '2020', 'reader'),
                ('a_03', 'LT001', 'A',  'T1', 'Leitung',           '11',   'DatenherrRef has to be defined', '2020', 'reader'),
                ('a_04', 'KN001', 'A',  'T2', 'Knoten',            '12',   'the value of OBJ_ID is out of range, text is too long 20 > 16', '2020', 'reader'),
                ('a_05', 'KN002', 'A',  'T2', 'Knoten',            '12',   'the value of OBJ_ID_Abwasserbauwerk is out of range, text is too long 40 > 16', '2020', 'reader'),
                ('a_06', 'TEG001','A',  'T3', 'Teileinzugsgebiet', '1002', 'MANDATORY perimeter', '2020', 'igcheck'),
                ('a_07', 'KN003', 'A',  'T2', 'Knoten',            '9999', 'Some unknown igcheck error', '2020', 'igcheck'),
                ('a_08', 'KN001', 'A',  'T2', 'Knoten',            '11',   'FunktionHierarchisch has to be defined', '2020', 'reader');

            INSERT INTO error_matrix (cid, checkmodel, ccat, model, class_de, class_fr, cmsg_de, cmsg_fr, prio_uc, prio_gsp, required_action_de, required_action_fr, action_context_de, action_context_fr)
            VALUES
                ('1001', 'vsa',  'error', '2020', 'Leitung', 'Conduite', 'Fehler 1001', 'Erreur 1001', '3', '2', 'Fix 1001', 'Fix FR 1001', 'Ctx 1001', 'Ctx FR 1001'),
                ('1002', 'vsa',  'error', '2020', NULL,      NULL,       'Fehler 1002', 'Erreur 1002', '1', '1', 'Fix 1002', 'Fix FR 1002', 'Ctx 1002', 'Ctx FR 1002'),
                ('11',   'base', 'error', NULL,   NULL,      NULL,       'Pflichtattribut {ATTR} fehlt', 'Attribut obligatoire {ATTR} manquant', NULL, NULL, 'Daten erheben', 'Saisir', 'Wert erfassen', 'Saisir la valeur'),
                ('12',   'base', 'error', NULL,   NULL,      NULL,       'Attribut {ATTR} zu lang', 'Attribut {ATTR} trop long', NULL, NULL, 'Daten prüfen', 'Vérifier', 'Kürzen', 'Réduire');

            INSERT INTO reader_error_rules (error_id, attr_name, condition_col, condition_val, msg_template_de, msg_template_fr, recommendation_de, recommendation_detail_de, wk, gep, suppress)
            VALUES
                (11, 'BetreiberRef', NULL, NULL, 'Pflichtattribut BetreiberRef fehlt', 'Attribut obligatoire BetreiberRef manquant', 'Daten erheben', 'Betreiber erfassen', 2, 2, 0),
                (11, 'FunktionHierarchisch', 'funktionhierarchisch', 'SAA', 'Pflichtattribut FunktionHierarchisch fehlt (SAA-Pflichtfeld)', 'FunktionHierarchisch SAA', 'Daten erheben', 'SAA setzen', 1, 1, 0),
                (12, 'OBJ_ID', NULL, NULL, 'OBJ_ID zu lang: {N} Zeichen (Max. {MAX}). Muss <= {MAX} Zeichen sein.', 'OBJ_ID trop long', 'Daten prüfen', 'OBJ_ID auf maximal {MAX} Zeichen kürzen.', 2, 2, 0),
                (12, 'OBJ_ID_Abwasserbauwerk', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1);

            INSERT INTO leitung (T_Id, T_Ili_Tid, funktionhierarchisch, astatus, eigentuemerref)
            VALUES (1, 'LT001', 'PAA.Transportleitung', 'in_Betrieb', 100);

            INSERT INTO knoten (T_Id, T_Ili_Tid, funktionhierarchisch, astatus, eigentuemerref)
            VALUES (1, 'KN001', 'SAA', 'in_Betrieb', 100);

            INSERT INTO organisation (T_Id, bezeichnung)
            VALUES (100, 'Gemeinde Aarau');
            """;
        cmd.ExecuteNonQuery();
    }

    private static async Task<SqliteConnection> SetUpAndMaterializeExtractionAsync(string language = "DE")
    {
        var connection = CreateOpenConnection();
        CreateTestSchemas(connection);
        SeedExtractionData(connection);

        new ViewCreator(connection).CreateCheckerCsvClassifiedView(
            "v_checker_csv_classified", "v_checker_csv_all", "error_matrix", language);

        var materializer = new ErrorDataMaterializer(connection, NullLogger.Instance);
        materializer.CreateBuildView("v_ca_error_data_build", "v_checker_csv_classified", "error_matrix", "reader_error_rules", language);
        await materializer.MaterializeAsync("v_ca_error_data_build", CancellationToken.None);

        return connection;
    }

    // Seeds one reader row per extracting ErrorId plus a matching base row, with synthetic messages
    // that mirror the assumed igcheck wording. No real reader-error samples exist in the fixtures, so
    // these pin the assumed format and guard against regressions; they do not prove it matches igcheck.
    private static void SeedExtractionData(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO v_checker_csv_all (t_id, "Tid", source, "Topic", "Class", "ErrorId", "Description", "Model", "Module")
            VALUES
                ('e11',  'X11',  'A', 'T', 'Knoten', '11', 'SomeRef has to be defined', '2020', 'reader'),
                ('e12',  'X12',  'A', 'T', 'Knoten', '12', 'the value of MyAttr is out of range, text is too long 20 > 16', '2020', 'reader'),
                ('e12b', 'X12B', 'A', 'T', 'Knoten', '12', 'the value of BadAttr is out of range, text is too long 20', '2020', 'reader'),
                ('e15',  'X15',  'A', 'T', 'Knoten', '15', 'the value of KnotenRef is out of range, reference points to unknown object tid=xyz-1', '2020', 'reader'),
                ('e21',  'X21',  'A', 'T', 'Knoten', '21', 'the value of TypeRef is out of range', '2020', 'reader'),
                ('e60',  'X60',  'A', 'T', 'Knoten', '60', 'the set constraint MyC failed on object X60', '2020', 'reader'),
                ('e70',  'X70',  'A', 'T', 'Knoten', '70', 'the unique constraint UniqRule (values=a,b) is violated', '2020', 'reader'),
                ('e70b', 'X70B', 'A', 'T', 'Knoten', '70', 'the unique constraint Other (values=a,b is violated', '2020', 'reader');

            INSERT INTO error_matrix (cid, checkmodel, ccat, cmsg_de)
            VALUES
                ('11', 'base', 'error', 'Pflichtattribut {ATTR} fehlt'),
                ('12', 'base', 'error', 'Attribut {ATTR} zu lang ({N} Zeichen, max. {MAX})'),
                ('15', 'base', 'error', 'Referenz {ATTR} zeigt auf unbekanntes Objekt (TID={TID})'),
                ('21', 'base', 'error', 'Referenz {ATTR} zeigt auf falsche Zielklasse'),
                ('60', 'base', 'error', 'MANDATORY Constraint {CONSTRAINT} verletzt'),
                ('70', 'base', 'error', 'UNIQUE Constraint verletzt für: {ATTRS}');
            """;
        cmd.ExecuteNonQuery();
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
}
