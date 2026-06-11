using Microsoft.Data.Sqlite;
using System.Diagnostics.CodeAnalysis;

namespace VsaCheckerAnalytics.Processors.GeopackageGeneration;

/// <summary>
/// Creates SQL views for checker CSV union and error matrix join operations.
/// </summary>
internal sealed class ViewCreator
{
    private readonly SqliteConnection connection;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewCreator"/> class.
    /// </summary>
    /// <param name="connection">An open SQLite connection.</param>
    internal ViewCreator(SqliteConnection connection)
    {
        this.connection = connection;
    }

    /// <summary>
    /// Creates a view that combines all checker CSV tables via UNION ALL, adding a
    /// <c>source</c> column to distinguish the origin of each row. All tables must have
    /// identical column names in identical order; otherwise an
    /// <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    /// <param name="viewName">Name of the view to create.</param>
    /// <param name="tableNames">Names of the tables to union.</param>
    /// <param name="sourceLabels">Labels for the <c>source</c> column, one per table.</param>
    [SuppressMessage("Security", "CA2100", Justification = "Table and view names are internal pipeline constants, not user input.")]
    internal void CreateCheckerCsvUnionView(string viewName, string[] tableNames, string[] sourceLabels)
    {
        if (tableNames.Length != sourceLabels.Length)
        {
            throw new ArgumentException(
                $"tableNames ({tableNames.Length}) and sourceLabels ({sourceLabels.Length}) must have the same length.");
        }

        var referenceColumns = GetColumnNames(tableNames[0]);
        foreach (var tableName in tableNames.Skip(1))
        {
            var columns = GetColumnNames(tableName);
            if (!columns.SequenceEqual(referenceColumns))
            {
                throw new InvalidOperationException(
                    $"Schema mismatch in UNION ALL view '{viewName}': table '{tableName}' has columns " +
                    $"[{string.Join(", ", columns)}] but '{tableNames[0]}' has columns " +
                    $"[{string.Join(", ", referenceColumns)}]. All tables must have identical schemas.");
            }
        }

        var dataColumns = referenceColumns
            .Where(c => !string.Equals(c, "t_id", StringComparison.OrdinalIgnoreCase))
            .Select(c => $"\"{c}\"");
        var columnList = string.Join(", ", dataColumns);

        var selects = tableNames
            .Zip(sourceLabels, (table, label) => $"SELECT LOWER('{label}') || '_' || \"t_id\" AS t_id, {columnList}, '{label}' AS source FROM \"{table}\"");

        var sql = $"CREATE VIEW IF NOT EXISTS \"{viewName}\" AS {string.Join(" UNION ALL ", selects)}";

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Creates a view that INNER JOINs the checker CSV union view with the error matrix.
    /// The join matches on <c>ErrorId = cid</c>, <c>Model = model</c>, and
    /// <c>Class = class_de/class_fr</c> (language-dependent).
    /// </summary>
    /// <param name="viewName">Name of the view to create.</param>
    /// <param name="checkErrorViewName">Name of the checker CSV union view.</param>
    /// <param name="errorMatrixTableName">Name of the error matrix table.</param>
    /// <param name="language">Language code: <c>"DE"</c> or <c>"FR"</c>.</param>
    [SuppressMessage("Security", "CA2100", Justification = "Table, view, and column names are internal pipeline constants, not user input.")]
    internal void CreateCheckerErrorsView(string viewName, string checkErrorViewName, string errorMatrixTableName, string language)
    {
        var classColumn = string.Equals(language, "FR", StringComparison.OrdinalIgnoreCase) ? "class_fr" : "class_de";

        var errorMatrixColumns = GetColumnNames(errorMatrixTableName);

        var errorMatrixSelectColumns = errorMatrixColumns
            .Where(c => !string.Equals(c, "t_id", StringComparison.OrdinalIgnoreCase)
                     && !string.Equals(c, "cid", StringComparison.OrdinalIgnoreCase)
                     && !string.Equals(c, "model", StringComparison.OrdinalIgnoreCase)
                     && !string.Equals(c, "class_de", StringComparison.OrdinalIgnoreCase)
                     && !string.Equals(c, "class_fr", StringComparison.OrdinalIgnoreCase))
            .Select(c => $"e.\"{c}\"");

        var selectList = $"c.*, {string.Join(", ", errorMatrixSelectColumns)}";

        var sql = $"""
            CREATE VIEW IF NOT EXISTS "{viewName}" AS
            SELECT {selectList}
            FROM "{checkErrorViewName}" c
            INNER JOIN "{errorMatrixTableName}" e
                ON c."ErrorId" = e."cid"
                AND c."Model" = e."model"
                AND c."Class" = e."{classColumn}"
            """;

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Creates a view that returns all rows from the checker CSV union view that have
    /// no matching entry in the errors view.
    /// </summary>
    /// <param name="viewName">Name of the view to create.</param>
    /// <param name="unionViewName">Name of the checker CSV union view.</param>
    /// <param name="errorsViewName">Name of the checker errors view.</param>
    [SuppressMessage("Security", "CA2100", Justification = "View names are internal pipeline constants, not user input.")]
    internal void CreateCheckerOrphansView(string viewName, string unionViewName, string errorsViewName)
    {
        var sql = $"""
            CREATE VIEW IF NOT EXISTS "{viewName}" AS
            SELECT c.*
            FROM "{unionViewName}" c
            WHERE NOT EXISTS (
                SELECT 1 FROM "{errorsViewName}" e WHERE e."t_id" = c."t_id"
            )
            """;

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Executes the embedded <c>AdditionalViews.sql</c> script to create the additional VSA views.
    /// </summary>
    [SuppressMessage("Security", "CA2100", Justification = "SQL is loaded from an embedded resource compiled into the assembly, not user input.")]
    internal void CreateAdditionalViews()
    {
        const string resourceName = "VsaCheckerAnalytics.EmbeddedResources.AdditionalViews.sql";

        var assembly = typeof(ViewCreator).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Embedded SQL resource '{resourceName}' not found. Available resources: " +
                $"[{string.Join(", ", assembly.GetManifestResourceNames())}].");

        using var reader = new StreamReader(stream);
        var sql = reader.ReadToEnd();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    [SuppressMessage("Security", "CA2100", Justification = "Table name is an internal pipeline constant, not user input.")]
    private List<string> GetColumnNames(string tableName)
    {
        var columns = new List<string>();
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info(\"{tableName}\")";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            columns.Add(reader.GetString(1));
        }

        return columns;
    }
}
