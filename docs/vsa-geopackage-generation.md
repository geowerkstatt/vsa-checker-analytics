# VSA Geopackage Generation Prozess

Der VSA Geopackage Generation Prozess importiert die vom
[VSA Matcher](vsa-matcher.md) erzeugten INTERLIS-Transferdateien in das
schema-only GeoPackage-Template, reichert das Ergebnis mit Checker-CSV-Daten und
einer Fehlermatrix an, erstellt analytische Views und materialisiert
Auswertungstabellen. Alle erzeugten Tabellen und Views werden in den
GeoPackage-Metadaten registriert, damit GIS-Clients wie QGIS sie als Layer
finden. Er gibt ein einzelnes befülltes GeoPackage für die nachgelagerten
Prozessoren aus.

## Konfiguration

| Parameter             | Typ                    | Beschreibung                                                                                  |
|-----------------------|------------------------|----------------------------------------------------------------------------------------------|
| `jobsDirectory`       | `string`               | Lokaler Pfad, den der `ili2gpkg`-Worker als `ILI2GPKG_JOBS_DIR` eingehängt hat. Dient dem Dateiaustausch zwischen Plugin und Worker. |

## Inputs

| Parameter         | Quelle       | Typ              | Beschreibung                                                                          |
|-------------------|--------------|------------------|--------------------------------------------------------------------------------------|
| `geoPackage`      | VSA Matcher  | `IPipelineFile`  | Schema-only GeoPackage-Template (`gpkg_template`), passend zur GEP-Modellversion.    |
| `dssMiniXtf`      | VSA Matcher  | `IPipelineFile`  | Die GEP- bzw. DSS-Mini-INTERLIS-Transferdatei (`gep`).                               |
| `defaultOrgsXtf`  | VSA Matcher  | `IPipelineFile`  | Standard-Organisationstabelle aus dem VSA-Repository (`standard_org_table`).          |
| `userOrgsXtf`     | VSA Matcher  | `IPipelineFile?` | Optionale benutzerdefinierte Organisationstabelle aus dem Upload (`user_org_table`). Kann leer sein.   |
| `checkerCsvT`     | VSA Matcher  | `IPipelineFile`  | Checker-CSV-Datei für Trägerschaft (T).                                              |
| `checkerCsvA`     | VSA Matcher  | `IPipelineFile`  | Checker-CSV-Datei für ARA (A).                                                       |
| `checkerCsvFp`    | VSA Matcher  | `IPipelineFile`  | Checker-CSV-Datei für Fachprüfungen (FP).                                            |
| `errorMatrix`     | VSA Matcher  | `IPipelineFile`  | Fehlermatrix-XLSX-Datei, die Fehler-IDs auf Beschreibungen und Prioritäten abbildet.  |
| `language`        | VSA Matcher  | `string`         | Sprachcode (`DE` oder `FR`) für die lokalisierten Spalten der Fehlermatrix.          |

## Output

| Key                   | Typ              | Beschreibung                                                                 |
|-----------------------|------------------|-----------------------------------------------------------------------------|
| `generatedGeopackage` | `IPipelineFile?` | Das befüllte GeoPackage mit dem Namen `generated.gpkg`. `null`, wenn ein ili2gpkg-Importschritt fehlgeschlagen ist. |
| `status_message`      | `LocalizedText`  | Lokalisierte Statusmeldung: eine Erfolgszusammenfassung oder ein Hinweis auf einen fehlgeschlagenen INTERLIS-Import, wenn `generatedGeopackage` `null` ist. Wird über die Output-Action `StatusMessage` in der Oberfläche angezeigt. |

## Anreicherungsschritte und Zwischenstände

Der Prozess läuft in vier Phasen: INTERLIS-Import, Checker-CSV-Import,
Fehlermatrix-Import und Analytics. Jede Phase schreibt in eine neu angelegte
Pipeline-Datei, die zum Input der nächsten Phase wird, sodass die
Zwischenstände inspizierbar bleiben:

1. INTERLIS-Import: `gpkg-step-{label}.gpkg` (eine Datei pro Importschritt)
2. Checker-CSV-Import: `gpkg-with-csvs.gpkg`
3. Fehlermatrix-Import: `gpkg-with-error-matrix.gpkg`
4. Analytics: `generated.gpkg` (finaler Output)

## Interlis-Import

Die Organisations- und GEP-Transferdateien werden nacheinander über
`IIli2GpkgClient.ImportToGeoPackageAsync` in das Template-GeoPackage importiert.
Jeder Import liest den aktuellen GeoPackage-Stream plus eine XTF und schreibt das
Ergebnis in eine neu angelegte `gpkg-step-{label}.gpkg`-Datei, die zum Input des
nächsten Schritts wird.

Import-Reihenfolge:

1. `defaultOrgs`: Standard-Organisationstabelle
2. `userOrgs`: *nur wenn eine Organisationstabelle aus dem Upload vorhanden ist*
3. `dssMini`: GEP-Transferdatei

### ili2gpkg-Argumente

Die folgenden `ili2gpkg`-Flags werden für jeden Importschritt gesetzt:

| Argument                | Wert | Begründung                                                               |
|-------------------------|------|-------------------------------------------------------------------------|
| `--skipReferenceErrors` | an   | Fortfahren, wenn XTF-Referenzen nicht aufgelöst werden können.           |
| `--skipGeometryErrors`  | an   | Fortfahren, wenn Geometriefehler auftreten.                             |
| `--disableValidation`   | an   | Die INTERLIS-Validierung ist Aufgabe des GEP-Checkers, nicht unsere.     |
| `--importTid`           | an   | Importiert die INTERLIS-TID in die Datenbank (für nachgelagerte Joins erforderlich). |
| `--strokeArcs`          | an   | Wandelt Bögen beim Import in Liniensegmente um (Stroking). Die nachgelagerte Geometrieverarbeitung (NetTopologySuite) arbeitet mit segmentierten Geometrien. |

## Checker-CSV-Import

Nach dem INTERLIS-Import werden die drei Checker-CSV-Dateien als Tabellen
`checker_csv_t`, `checker_csv_a` und `checker_csv_fp` in das GeoPackage
importiert. Für jede Tabelle werden Join-Indizes auf (`ErrorId`, `Model`, `Class`) erstellt und jede
Tabelle wird als `attributes`-Layer in den GeoPackage-Metadaten registriert
(siehe [GeoPackage-Metadaten-Registrierung](#geopackage-metadaten-registrierung)).

## Fehlermatrix-Import

Die Fehlermatrix-XLSX wird in die Tabelle `error_matrix` importiert, mit einem
Join-Index auf (`cid`, `model`, `class_de`). Auch `error_matrix` wird als
`attributes`-Layer registriert.

## Analytics

Die Analytics-Schritte laufen auf der Kopie `generated.gpkg`, die als finaler
Output zurückgegeben wird.

### Views

1. `v_checker_csv_all` vereinigt die drei Checker-CSV-Tabellen via `UNION ALL`
   und ergänzt eine `source`-Spalte (`T`, `A`, `FP`). Jede Zeile erhält
   zusätzlich eine über alle drei Tabellen eindeutige `t_id`, indem die Quelle
   als Präfix vorangestellt wird (z.B. `t_…`, `a_…`, `fp_…`).
2. `v_checker_errors` verknüpft den Vereinigungs-View per `INNER JOIN` mit der
   Fehlermatrix (`ErrorId = cid`, `Model = model`, `Class = class_de` bzw.
   `class_fr` je nach Sprache) und reichert jede CSV-Zeile mit lokalisierten
   Beschreibungen und Prioritäten an.
3. `CreateAdditionalViews()` führt das eingebettete Skript `AdditionalViews.sql`
   aus. Es erstellt die VSA-Feature-Views (`v_vsa_*` für Knoten-, Leitungs- und
   Ueberlauf_Foerderaggregat-Varianten) sowie die Fehler-Views für die
   GIS-Darstellung (`v_error_*`, `v_errorlist_*`, `v_error_recommendation_*`,
   `v_error_category_*`). Jede dieser Views registriert sich direkt nach ihrem
   `CREATE VIEW` selbst in den GeoPackage-Metadaten.

`v_checker_csv_all` und `v_checker_errors` werden ebenfalls als
`attributes`-Layer registriert.

### Materialisierung (`ErrorDataMaterializer`)

Der `ErrorDataMaterializer` erzeugt zwei Tabellen aus dem Errors-View:

- Zunächst baut `CreateBuildView` den Build-View `v_ca_error_data_build`, der
  jede vorhandene VSA-Feature-Klasse auf das gemeinsame `ca_error_data`-Schema
  abbildet. Welche Feature-Klassen vorhanden sind, wird zur Laufzeit per
  `TableExists` bestimmt. Für `Leitung` und `Knoten` werden die
  Anreicherungsspalten `funktionhierarchisch`, `eigentuemer` (aus
  `organisation`) und `status` aus den Feature-Tabellen gejoint; alle übrigen
  Klassen sowie ein Catch-all für nicht vorhandene Tabellen setzen diese Spalten
  auf `NULL`.
- `MaterializeAsync` legt die Zieltabellen an und befüllt sie in einer einzigen
  Transaktion:
  - **`ca_error_data`** enthält eine Zeile pro Checker-Fehler, befüllt aus dem
    Build-View.
  - **`ca_error_object`** aggregiert `ca_error_data` nach (`tid`, `class`) mit
    `COUNT(*)`, `MAX(wk)` und `MAX(gep)`.

Die `check_type`-Spalte wird aus dem CSV-Feld `Module` abgeleitet: `reader` wird
zu `ig`, andernfalls ist der Wert die CSV-Quelle (`T`, `A`, `FP`). Die
`module`-Spalte folgt einer ähnlichen Logik: `reader` wird zu `igcheck`, alles
andere zu `gep_check`.

`v_ca_error_data_build`, `ca_error_data` und `ca_error_object` werden als
`attributes`-Layer registriert.

### Orphan-Erkennung (`OrphanInspector`)

Zum Schluss prüft der `OrphanInspector`, ob Checker-CSV-Zeilen ohne passenden
Fehlermatrix-Eintrag existieren (Zeilen, die in `v_checker_csv_all`, aber nicht
in `v_checker_errors` vorkommen). Nur wenn solche Orphans existieren, werden sie
in die Tabelle `ca_error_orphans` materialisiert und eine Warnung mit den
betroffenen (`ErrorId`, `Model`, `Class`)-Schlüsseln protokolliert. Die blosse
Existenz der Tabelle ist damit das Signal, dass das Domain-Team die Fehlermatrix
prüfen sollte.

## GeoPackage-Metadaten-Registrierung

Damit GIS-Clients wie QGIS die analytischen Layer automatisch finden, werden alle
erzeugten Tabellen und Views in den GeoPackage-Metadatentabellen `gpkg_contents`
und `gpkg_geometry_columns` eingetragen (siehe
[GeoPackage-Tooling](geopackage-tooling.md)). Es gibt zwei Mechanismen:

- **Nicht-räumliche Tabellen und Views** (Checker-CSVs, `error_matrix`,
  `v_checker_csv_all`, `v_checker_errors`, `v_ca_error_data_build`,
  `ca_error_data`, `ca_error_object`) werden über den C#-Helfer
  `GeopackageMetadata.RegisterAttributes` als `attributes`-Layer registriert.
- **Räumliche Feature-Views** in `AdditionalViews.sql` registrieren sich selbst
  direkt nach ihrem `CREATE VIEW`: als `features` in `gpkg_contents` und
  zusätzlich in `gpkg_geometry_columns` mit Geometriespalte, Geometrietyp und
  SRS 2056.

## Fehlerverhalten

Wenn ein `ili2gpkg`-Importschritt ein nicht erfolgreiches Ergebnis meldet,
protokolliert der Prozess die Worker-Ausgabe auf Debug-Level und gibt `null` für
`generatedGeopackage` zurück. Die verbleibenden Anreicherungsschritte werden
übersprungen. Nachgelagerte Prozessoren erkennen das fehlende GPKG über ihre
eigene Pre-Condition und brechen die Pipeline ab.
