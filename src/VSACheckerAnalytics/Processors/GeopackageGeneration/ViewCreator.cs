using Microsoft.Data.Sqlite;
using System.Diagnostics.CodeAnalysis;
using VsaCheckerAnalytics.Geopackage;

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

        var sql = $"CREATE VIEW \"{viewName}\" AS {string.Join(" UNION ALL ", selects)}";

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();

        GeopackageMetadata.RegisterAttributes(connection, viewName);
    }

    /// <summary>
    /// Creates a view that adds an <c>is_known</c> flag to every checker CSV union row: <c>1</c> when
    /// the error can be described, otherwise <c>0</c>. An <c>igcheck</c> row is known when a matching
    /// non-<c>base</c> error matrix row exists (<c>cid</c>, <c>model</c> NULL-or-equal, class NULL/empty-or-equal);
    /// a <c>reader</c> row is known when a <c>base</c> row exists for its ErrorId. This is the single
    /// classification consumed both by the build view (keeps <c>is_known = 1</c>) and by the orphan
    /// detection (keeps <c>is_known = 0</c>), so the two cannot disagree. The view is internal plumbing
    /// and is intentionally not registered as a GeoPackage layer.
    /// </summary>
    /// <param name="viewName">Name of the view to create.</param>
    /// <param name="unionViewName">Name of the checker CSV union view.</param>
    /// <param name="errorMatrixTableName">Name of the error matrix table (igcheck rows plus <c>base</c> reader rows).</param>
    /// <param name="language">Language code (<c>"DE"</c> or <c>"FR"</c>) selecting the class column.</param>
    [SuppressMessage("Security", "CA2100", Justification = "Table, view, and column names are internal pipeline constants, not user input.")]
    internal void CreateCheckerCsvClassifiedView(string viewName, string unionViewName, string errorMatrixTableName, string language)
    {
        var classColumn = string.Equals(language, "FR", StringComparison.OrdinalIgnoreCase) ? "class_fr" : "class_de";

        var sql = $"""
            CREATE VIEW "{viewName}" AS
            SELECT c.*,
                CASE
                    WHEN c."Module" = 'igcheck' AND EXISTS (
                        SELECT 1 FROM "{errorMatrixTableName}" em
                        WHERE em.checkmodel != 'base'
                          AND em.cid = c."ErrorId"
                          AND (em.model IS NULL OR em.model = c."Model")
                          AND (em."{classColumn}" IS NULL OR em."{classColumn}" = '' OR em."{classColumn}" = c."Class"))
                    THEN 1
                    WHEN c."Module" = 'reader' AND EXISTS (
                        SELECT 1 FROM "{errorMatrixTableName}" b
                        WHERE b.checkmodel = 'base' AND b.cid = c."ErrorId")
                    THEN 1
                    ELSE 0
                END AS is_known
            FROM "{unionViewName}" c
            """;

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Executes the embedded <c>AdditionalViews.sql</c> script, which creates the additional VSA
    /// views and registers each one in the GeoPackage metadata right after its <c>CREATE VIEW</c>:
    /// every view in <c>gpkg_contents</c>, and the spatial views also in <c>gpkg_geometry_columns</c>.
    /// </summary>
    internal void CreateAdditionalViews()
        => EmbeddedSql.Execute(connection, "AdditionalViews.sql");

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
