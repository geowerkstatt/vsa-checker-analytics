using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using System.Globalization;
using System.Text;

namespace VsaCheckerAnalytics.Processors.GeopackageGeneration;

[TestClass]
public class GeopackageGenerationIntegrationTest
{
    private static readonly string[] CsvColumns =
        ["Module", "ErrorId", "Category", "Group", "Description", "Model", "Topic", "Bid", "Class", "Tid", "Line", "CharPos", "Geom1", "Geom2", "UserAttributes", "Profiles"];

    private static readonly string[] ErrorMatrixColumns =
        ["cid", "ccat", "cmsg_de", "cmsg_fr", "class_de", "class_fr", "checkmodel", "model", "prio_uc", "prio_gsp", "sub_project_gsp_de", "sub_project_gsp_fr", "required_action_de", "required_action_fr", "action_context_de", "action_context_fr"];

    private static readonly string[] BaseErrorColumns =
        ["cid", "ccat", "cmsg_de", "cmsg_fr", "class_de", "class_fr", "checkmodel", "model", "prio_uc", "prio_gsp", "sub_project_gsp_de", "sub_project_gsp_fr", "required_action_de", "required_action_fr", "action_context_de", "action_context_fr", "cmsg_it", "error_type_de", "error_type_fr", "error_type_it", "required_action_it", "action_context_it"];

    private static readonly string[] CsvJoinIndexColumns = ["ErrorId", "Model", "Class"];

    [TestMethod]
    public async Task ImportCsvsAndErrorMatrixAndCreateViews()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var templatePath = GetTestdataPath("template_ca_dssmini_2020_1_d.gpkg");
        var tempGpkgPath = Path.Combine(Path.GetTempPath(), $"integration-test-{Guid.NewGuid():N}.gpkg");
        File.Copy(templatePath, tempGpkgPath);

        try
        {
            using var connection = new SqliteConnection($"Data Source={tempGpkgPath};Pooling=false");
            connection.Open();

            var csvImporter = new CsvImporter(connection, Encoding.GetEncoding(1252), NullLogger.Instance);

            using (var stream = File.OpenRead(GetTestdataPath("transferdatensatz_2020_1_d_LV95_T-20231205_mini_t_err.csv")))
            {
                await csvImporter.ImportAsync(stream, "checker_csv_t", CsvColumns, CancellationToken.None, CsvJoinIndexColumns);
            }

            using (var stream = File.OpenRead(GetTestdataPath("transferdatensatz_2020_1_d_LV95_T-20231205_mini_a_err.csv")))
            {
                await csvImporter.ImportAsync(stream, "checker_csv_a", CsvColumns, CancellationToken.None, CsvJoinIndexColumns);
            }

            using (var stream = File.OpenRead(GetTestdataPath("transferdatensatz_2020_1_d_LV95_T-20231205_mini_fp_err.csv")))
            {
                await csvImporter.ImportAsync(stream, "checker_csv_fp", CsvColumns, CancellationToken.None, CsvJoinIndexColumns);
            }

            EmbeddedSql.Execute(connection, "ErrorMatrixSchema.sql");

            var errorMatrixImporter = new ErrorMatrixImporter(connection, NullLogger.Instance);
            using (var stream = File.OpenRead(GetTestdataPath("errorMatrix.xlsx")))
            {
                await errorMatrixImporter.ImportAsync(stream, "error_matrix", ErrorMatrixColumns, CancellationToken.None);
            }

            using (var baseStream = EmbeddedResource.OpenRead("errorMatrixBaseError.xlsx"))
            {
                await errorMatrixImporter.ImportAsync(baseStream, "error_matrix", BaseErrorColumns, CancellationToken.None);
            }

            new ReaderErrorRulesInitializer(connection, NullLogger.Instance).Initialize();

            var viewCreator = new ViewCreator(connection);
            viewCreator.CreateCheckerCsvUnionView(
                "v_checker_csv_all",
                ["checker_csv_t", "checker_csv_a", "checker_csv_fp"],
                ["T", "A", "FP"]);

            viewCreator.CreateCheckerCsvClassifiedView(
                "v_checker_csv_classified", "v_checker_csv_all", "error_matrix", "DE");

            var materializer = new ErrorDataMaterializer(connection, NullLogger.Instance);
            materializer.CreateBuildView("v_ca_error_data_build", "v_checker_csv_classified", "error_matrix", "reader_error_rules", "DE");
            await materializer.MaterializeAsync("v_ca_error_data_build", CancellationToken.None);

            Assert.AreEqual(686, GetRowCount(connection, "checker_csv_t"));
            Assert.AreEqual(552, GetRowCount(connection, "checker_csv_a"));
            Assert.AreEqual(473, GetRowCount(connection, "checker_csv_fp"));
            Assert.AreEqual(1711, GetRowCount(connection, "v_checker_csv_all"));

            Assert.AreEqual(686, GetCountWhere(connection, "v_checker_csv_all", "source = 'T'"));
            Assert.AreEqual(552, GetCountWhere(connection, "v_checker_csv_all", "source = 'A'"));
            Assert.AreEqual(473, GetCountWhere(connection, "v_checker_csv_all", "source = 'FP'"));

            var errorDataCount = GetRowCount(connection, "ca_error_data");
            var errorObjectCount = GetRowCount(connection, "ca_error_object");
            var unionCount = GetRowCount(connection, "v_checker_csv_all");
            Assert.IsGreaterThan(0, errorDataCount, "ca_error_data should contain deduplicated checker errors.");
            Assert.IsLessThan(unionCount, errorDataCount, "Deduplication and suppression should reduce rows below the raw CSV union count.");
            Assert.IsGreaterThan(0, errorObjectCount, "ca_error_object should contain aggregated rows.");
            Assert.IsLessThanOrEqualTo(errorDataCount, errorObjectCount, "ca_error_object groups errors, so it must have fewer or equal rows.");

            // This fixture contains only igcheck-module errors, which map to module 'gep_check'.
            // (Reader-error enrichment is covered by ErrorDataMaterializerTest.)
            Assert.IsGreaterThan(0, GetCountWhere(connection, "ca_error_data", "module = 'gep_check'"), "igcheck errors should be present.");
            Assert.AreEqual(0, GetCountWhere(connection, "ca_error_data", "check_type = 'ig'"), "This fixture has no reader errors.");
            Assert.AreEqual(0, GetCountWhere(connection, "ca_error_data", "error IS NULL"), "Every materialized error should have a rendered message.");

            var errorDataColumns = GetColumnNames(connection, "ca_error_data");
            Assert.Contains("tid", errorDataColumns);
            Assert.Contains("check_type", errorDataColumns);
            Assert.Contains("errorid", errorDataColumns);
            Assert.Contains("error", errorDataColumns);
            Assert.Contains("funktionhierarchisch", errorDataColumns);
            Assert.Contains("eigentuemer", errorDataColumns);
            Assert.Contains("wk", errorDataColumns);
            Assert.Contains("gep", errorDataColumns);
            Assert.Contains("recommendation", errorDataColumns);

            var errorObjectColumns = GetColumnNames(connection, "ca_error_object");
            Assert.Contains("tid", errorObjectColumns);
            Assert.Contains("class", errorObjectColumns);
            Assert.Contains("count_error", errorObjectColumns);
            Assert.Contains("wk_max", errorObjectColumns);
            Assert.Contains("gep_max", errorObjectColumns);

            var indexes = GetIndexNames(connection);
            Assert.Contains("ix_checker_csv_t_errorid_model_class", indexes);
            Assert.Contains("ix_checker_csv_a_errorid_model_class", indexes);
            Assert.Contains("ix_checker_csv_fp_errorid_model_class", indexes);
            Assert.Contains("ix_error_matrix_cid_model_class_de", indexes);
            Assert.Contains("ix_ca_error_data_tid_class", indexes);

            var outputDir = Environment.GetEnvironmentVariable("GPKG_OUTPUT_DIR");
            if (!string.IsNullOrEmpty(outputDir))
            {
                if (!Path.IsPathRooted(outputDir))
                {
                    outputDir = Path.GetFullPath(outputDir, AppContext.BaseDirectory);
                }

                Directory.CreateDirectory(outputDir);
                connection.Close();
                var outputPath = Path.Combine(outputDir, "integration_result.gpkg");
                File.Copy(tempGpkgPath, outputPath, overwrite: true);
            }
        }
        finally
        {
            File.Delete(tempGpkgPath);
        }
    }

    private static string GetTestdataPath(string fileName)
        => Path.Combine(AppContext.BaseDirectory, "Testdata", fileName);

#pragma warning disable CA2100
    private static int GetRowCount(SqliteConnection connection, string tableOrView)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM \"{tableOrView}\"";
        return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
    }

    private static int GetCountWhere(SqliteConnection connection, string tableOrView, string whereClause)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM \"{tableOrView}\" WHERE {whereClause}";
        return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
    }

    private static List<string> GetColumnNames(SqliteConnection connection, string tableOrView)
    {
        var columns = new List<string>();
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info(\"{tableOrView}\")";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            columns.Add(reader.GetString(1));
        }

        return columns;
    }

    private static List<string> GetIndexNames(SqliteConnection connection)
    {
        var indexes = new List<string>();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'index' AND name NOT LIKE 'sqlite_%'";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            indexes.Add(reader.GetString(0));
        }

        return indexes;
    }
#pragma warning restore CA2100
}
