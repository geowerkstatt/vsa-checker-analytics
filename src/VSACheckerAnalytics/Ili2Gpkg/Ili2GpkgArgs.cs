namespace VsaCheckerAnalytics.Ili2Gpkg;

/// <summary>
/// Arguments forwarded to the ili2gpkg worker.
/// Properties map to the corresponding ili2gpkg command line options.
/// Methods on <see cref="IIli2GpkgClient"/> ignore properties that do not apply to the operation.
/// </summary>
public sealed class Ili2GpkgArgs
{
    /// <summary>
    /// When set, the import operation also creates the database schema before importing data.
    /// Maps to the ili2gpkg option <c>--doSchemaImport</c>.
    /// Only relevant for import operations.
    /// </summary>
    public bool DoSchemaImport { get; set; }

    /// <summary>
    /// Enables NULL constraints in the generated SQL schema.
    /// Maps to the ili2gpkg option <c>--sqlEnableNull</c>.
    /// </summary>
    public bool SqlEnableNull { get; set; }

    /// <summary>
    /// Continues the import when reference errors are encountered.
    /// Maps to the ili2gpkg option <c>--skipReferenceErrors</c>.
    /// </summary>
    public bool SkipReferenceErrors { get; set; }

    /// <summary>
    /// Continues the import when geometry errors are encountered.
    /// Maps to the ili2gpkg option <c>--skipGeometryErrors</c>.
    /// </summary>
    public bool SkipGeometryErrors { get; set; }

    /// <summary>
    /// Disables INTERLIS validation during the import.
    /// Maps to the ili2gpkg option <c>--disableValidation</c>.
    /// </summary>
    public bool DisableValidation { get; set; }

    /// <summary>
    /// Imports the INTERLIS TID into the database.
    /// Maps to the ili2gpkg option <c>--importTid</c>.
    /// </summary>
    public bool ImportTid { get; set; }

    /// <summary>
    /// Strokes arcs on data import.
    /// Maps to the ili2gpkg option <c>--strokeArcs</c>.
    /// </summary>
    public bool StrokeArcs { get; set; }
}
