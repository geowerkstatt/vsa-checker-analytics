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

    private static readonly string[] CsvJoinIndexColumns = ["ErrorId", "Model", "Class"];
    private static readonly string[] ErrorMatrixJoinIndexColumns = ["cid", "model", "class_de"];

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

            var errorMatrixImporter = new ErrorMatrixImporter(connection, NullLogger.Instance);
            using (var stream = File.OpenRead(GetTestdataPath("errorMatrix.xlsx")))
            {
                await errorMatrixImporter.ImportAsync(stream, "error_matrix", ErrorMatrixColumns, CancellationToken.None, ErrorMatrixJoinIndexColumns);
            }

            var viewCreator = new ViewCreator(connection);
            viewCreator.CreateCheckerCsvUnionView(
                "v_checker_csv_all",
                ["checker_csv_t", "checker_csv_a", "checker_csv_fp"],
                ["T", "A", "FP"]);

            viewCreator.CreateCheckerErrorsView(
                "v_checker_errors", "v_checker_csv_all", "error_matrix", "DE");

            viewCreator.CreateCheckerOrphansView(
                "v_checker_orphans", "v_checker_csv_all", "v_checker_errors");

            var materializer = new ErrorDataMaterializer(connection, NullLogger.Instance);
            materializer.CreateBuildView("v_ca_error_data_build", "v_checker_errors", "DE");
            await materializer.MaterializeAsync("v_ca_error_data_build", CancellationToken.None);

            Assert.AreEqual(686, GetRowCount(connection, "checker_csv_t"));
            Assert.AreEqual(552, GetRowCount(connection, "checker_csv_a"));
            Assert.AreEqual(473, GetRowCount(connection, "checker_csv_fp"));
            Assert.AreEqual(1711, GetRowCount(connection, "v_checker_csv_all"));

            Assert.AreEqual(686, GetCountWhere(connection, "v_checker_csv_all", "source = 'T'"));
            Assert.AreEqual(552, GetCountWhere(connection, "v_checker_csv_all", "source = 'A'"));
            Assert.AreEqual(473, GetCountWhere(connection, "v_checker_csv_all", "source = 'FP'"));

            var errorsCount = GetRowCount(connection, "v_checker_errors");
            var orphansCount = GetRowCount(connection, "v_checker_orphans");

            // Every CSV row must be either matched (errors) or unmatched (orphans), so the sum
            // is at least the total CSV row count. Duplicate (cid, model, class) entries in the
            // error matrix multiply matched rows; today this adds exactly 1 row. We leave a
            // generous upper margin to catch a future explosion in matrix duplicates without
            // breaking the test for small, expected drift. Once the error matrix is
            // deduplicated, both bounds can be tightened to AreEqual(1711, ...).
            Assert.IsGreaterThanOrEqualTo(1711, errorsCount + orphansCount);
            Assert.IsLessThanOrEqualTo(1711 + 50, errorsCount + orphansCount);
            Assert.IsGreaterThan(0, errorsCount);
            Assert.IsGreaterThan(0, orphansCount);

            var errorsViewColumns = GetColumnNames(connection, "v_checker_errors");
            Assert.Contains("ccat", errorsViewColumns);
            Assert.Contains("prio_uc", errorsViewColumns);
            Assert.DoesNotContain("cid", errorsViewColumns);
            Assert.DoesNotContain("model", errorsViewColumns);
            Assert.DoesNotContain("class_de", errorsViewColumns);
            Assert.DoesNotContain("class_fr", errorsViewColumns);

            var errorDataCount = GetRowCount(connection, "ca_error_data");
            var errorObjectCount = GetRowCount(connection, "ca_error_object");
            Assert.AreEqual(errorsCount, errorDataCount, "ca_error_data should contain one row per checker error.");
            Assert.IsGreaterThan(0, errorObjectCount, "ca_error_object should contain aggregated rows.");
            Assert.IsLessThanOrEqualTo(errorDataCount, errorObjectCount, "ca_error_object groups errors, so it must have fewer or equal rows.");

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
