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
| `geoPackage`      | VSA Matcher  | `IPipelineFile`  | Schema-only GeoPackage-Template (`GpkgTemplate`), passend zur GEP-Modellversion.    |
| `dssMiniXtf`      | VSA Matcher  | `IPipelineFile`  | Die GEP- bzw. DSS-Mini-INTERLIS-Transferdatei (`Gep`).                               |
| `defaultOrgsXtf`  | VSA Matcher  | `IPipelineFile`  | Standard-Organisationstabelle aus dem VSA-Repository (`StandardOrgTable`).          |
| `userOrgsXtf`     | VSA Matcher  | `IPipelineFile?` | Optionale benutzerdefinierte Organisationstabelle aus dem Upload (`UserOrgTable`). Kann leer sein.   |
| `checkerCsvT`     | VSA Matcher  | `IPipelineFile`  | Checker-CSV-Datei für Trägerschaft (T).                                              |
| `checkerCsvA`     | VSA Matcher  | `IPipelineFile`  | Checker-CSV-Datei für ARA (A).                                                       |
| `checkerCsvFp`    | VSA Matcher  | `IPipelineFile`  | Checker-CSV-Datei für Fachprüfungen (FP).                                            |
| `errorMatrix`     | Ressourcen (`${file()}`) | `IPipelineFile`  | Fehlermatrix-XLSX-Datei, die Fehler-IDs auf Beschreibungen und Prioritäten abbildet. Wird direkt aus dem Ressourcenverzeichnis injiziert. |
| `language`        | VSA Matcher  | `string`         | Sprachcode (`DE` oder `FR`) für die lokalisierten Spalten der Fehlermatrix.          |

## Output

`RunAsync` gibt ein `GeopackageGenerationResult` mit den folgenden Properties zurück:

| Property                   | Typ              | Beschreibung                                                                 |
|-----------------------|------------------|-----------------------------------------------------------------------------|
| `GeneratedGeopackage` | `IPipelineFile?` | Das befüllte GeoPackage mit dem Namen `generated.gpkg`. `null`, wenn ein ili2gpkg-Importschritt fehlgeschlagen ist. |
| `StatusMessage`      | `LocalizedText`  | Lokalisierte Statusmeldung: eine Erfolgszusammenfassung oder ein Hinweis auf einen fehlgeschlagenen INTERLIS-Import, wenn `GeneratedGeopackage` `null` ist. Wird über die Output-Action `StatusMessage` in der Oberfläche angezeigt. |

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

Das `error_matrix`-Schema (alle Spalten und der Join-Index auf (`cid`, `model`,
`class_de`)) ist an einer Stelle definiert: im eingebetteten Skript
`ErrorMatrixSchema.sql`, das vor dem Import ausgeführt wird. Danach wird
`error_matrix` aus zwei Quellen über denselben `ErrorMatrixImporter` befüllt:
zuerst die Fehlermatrix-XLSX (die igcheck- bzw. Profil-Matrix), dann die 33
category-level `base`-Zeilen aus der eingebetteten `errorMatrixBaseError.xlsx`
(eine pro Reader-`ErrorId`, mit lokalisierten Meldungen, Empfehlungen und
Prioritäten). `error_matrix` wird als `attributes`-Layer registriert.

Direkt danach legt der `ReaderErrorRulesInitializer` über das eingebettete Skript
`ReaderErrorRules.sql` die Tabelle `reader_error_rules` an und befüllt sie
mit den attribut- bzw. bedingungsspezifischen Overrides und den
Unterdrückungsregeln (`suppress`). Das ist das von geowerkstatt gepflegte
Reader-Fehler-Wissen, das nicht über die igcheck-XLSX transportiert wird.

`reader_error_rules` wird als `attributes`-Layer registriert.

## Analytics

Die Analytics-Schritte laufen auf der Kopie `generated.gpkg`, die als finaler
Output zurückgegeben wird.

### Views

1. `v_checker_csv_all` vereinigt die drei Checker-CSV-Tabellen via `UNION ALL`
   und ergänzt eine `source`-Spalte (`T`, `A`, `FP`). Jede Zeile erhält
   zusätzlich eine über alle drei Tabellen eindeutige `t_id`, indem die Quelle
   als Präfix vorangestellt wird (z.B. `t_…`, `a_…`, `fp_…`).
2. `v_checker_csv_classified` ergänzt jede Zeile des Vereinigungs-Views um ein
   Flag `is_known`: `1`, wenn der Fehler beschrieben werden kann (igcheck-Zeile
   mit passendem Nicht-`base`-Eintrag in `error_matrix`, oder Reader-Zeile mit
   `base`-Zeile zu ihrer `ErrorId`), sonst `0`. Dieses Flag ist die einzige
   Klassifikation: der Build-View nimmt `is_known = 1`, die Orphan-Erkennung
   `is_known = 0`, damit können die beiden nicht auseinanderlaufen. Der View ist
   interne Plumbing und wird nicht als GeoPackage-Layer registriert.
3. `CreateAdditionalViews()` führt das eingebettete Skript `AdditionalViews.sql`
   aus. Es erstellt die VSA-Feature-Views (`v_vsa_*` für Knoten-, Leitungs- und
   Ueberlauf_Foerderaggregat-Varianten) sowie die Fehler-Views für die
   GIS-Darstellung (`v_error_*`, `v_errorlist_*`, `v_error_recommendation_*`,
   `v_error_category_*`). Jede dieser Views registriert sich direkt nach ihrem
   `CREATE VIEW` selbst in den GeoPackage-Metadaten.

`v_checker_csv_all` wird ebenfalls als `attributes`-Layer registriert;
`v_checker_csv_classified` bleibt bewusst unregistriert.

### Materialisierung (`ErrorDataMaterializer`)

Der `ErrorDataMaterializer` erzeugt zwei Tabellen aus dem klassifizierten View
`v_checker_csv_classified` und nimmt dabei nur bekannte Fehler auf:

- `CreateBuildView` baut den Build-View `v_ca_error_data_build`. Er
  dedupliziert zunächst auf (`Tid`, `Module`, `Description`), sodass derselbe
  logische Fehler, den mehrere Profile melden, zu einer Zeile zusammenfällt. Die
  `check_type`-Spalte aggregiert dabei die beteiligten Profile (z.B.
  `vsa-a; vsa-fp; vsa-t`); Reader-Fehler sind profilunabhängig und werden zu
  `ig`. Die repräsentative Zeile (kleinste `t_id`) liefert `ErrorId`, `Class`,
  `Model` und `Topic`.
- Die Anreicherung läuft in zwei Zweigen:
  - **igcheck**: `LEFT JOIN` auf `error_matrix` (klassen- bzw.
    modellspezifische Zeile gewinnt). Die rohe Validator-Beschreibung dient nur
    noch als Meldungs-Fallback für eine bekannte Zeile mit leerer Vorlage, nicht
    mehr dazu, unbekannte Fehler zu behalten.
  - **reader**: Aus der Validator-Meldung werden Parameter extrahiert
    (`{ATTR}`, `{N}`, `{MAX}`, `{TID}`, `{CONSTRAINT}`, `{ATTRS}`) und über eine
    dreistufige Auflösung (Bedingungs-Override, Attribut-Override,
    `base`-Zeile) zu Meldung, Priorität und Empfehlung verbunden. Die
    Platzhalter werden in die gewählte Sprache gerendert; für FR/IT werden die
    SIA405-Rollennamen übersetzt. Als `suppress` markierte Reader-Fehler
    (bereits durch einen igcheck-Check abgedeckt) fallen weg. Zeilen mit
    `is_known = 0` (unbekannte Fehler) werden hier nicht aufgenommen; sie
    erscheinen ausschliesslich in `ca_error_orphans`.
- Die Anreicherungsspalten `funktionhierarchisch`, `eigentuemer` (aus
  `organisation`) und `status` werden weiterhin aus den Feature-Tabellen
  `leitung` und `knoten` gejoint; welche vorhanden sind, bestimmt `TableExists`.
- `MaterializeAsync` legt die Zieltabellen an und befüllt sie in einer einzigen
  Transaktion:
  - **`ca_error_data`** enthält eine Zeile pro dedupliziertem, bekanntem Fehler.
  - **`ca_error_object`** aggregiert `ca_error_data` nach (`tid`, `class`) mit
    `COUNT(*)`, `MAX(wk)` und `MAX(gep)`.

Die Meldungs- und Empfehlungsspalten (`error`, `recommendation`,
`recommendation_detail`) werden einsprachig in der Sprache der hochgeladenen
Daten (`DE` oder `FR`) befüllt; die `module`-Spalte bildet `reader` auf
`igcheck` und alles andere auf `gep_check` ab. `detail` enthält bei
Reader-Zeilen die rohe Validator-Beschreibung, bei igcheck-Zeilen einen leeren
String.

`v_ca_error_data_build`, `ca_error_data` und `ca_error_object` werden als
`attributes`-Layer registriert.

### Orphan-Erkennung (`OrphanInspector`)

Zum Schluss materialisiert der `OrphanInspector` das exakte Komplement der
angereicherten Fehler: alle Zeilen, die der klassifizierte View mit
`is_known = 0` markiert (igcheck-Zeilen ohne passenden Nicht-`base`-Eintrag in
`error_matrix`, sowie Reader-Zeilen ohne `base`-Zeile zu ihrer `ErrorId`). Da
dasselbe `is_known`-Flag auch `ca_error_data` steuert, sind die beiden Mengen
disjunkt und laufen nicht auseinander. Nur wenn solche Orphans existieren, werden
sie in die Tabelle `ca_error_orphans` materialisiert und eine Warnung mit den
betroffenen (`ErrorId`, `Model`, `Class`)-Schlüsseln protokolliert. Die blosse
Existenz der Tabelle ist damit das Signal, dass etwas nicht stimmt (ein
Code-Fehler oder vom Lieferanten erfundene Fehler); im Normalfall bleibt sie
leer.

## GeoPackage-Metadaten-Registrierung

Damit GIS-Clients wie QGIS die analytischen Layer automatisch finden, werden alle
erzeugten Tabellen und Views in den GeoPackage-Metadatentabellen `gpkg_contents`
und `gpkg_geometry_columns` eingetragen (siehe
[GeoPackage-Tooling](geopackage-tooling.md)). Es gibt zwei Mechanismen:

- **Nicht-räumliche Tabellen und Views** (Checker-CSVs, `error_matrix`,
  `reader_error_rules`, `v_checker_csv_all`, `v_ca_error_data_build`,
  `ca_error_data`, `ca_error_object`) werden über den C#-Helfer
  `GeopackageMetadata.RegisterAttributes` als `attributes`-Layer registriert.
  `v_checker_csv_classified` ist interne Plumbing und wird nicht registriert.
- **Räumliche Feature-Views** in `AdditionalViews.sql` registrieren sich selbst
  direkt nach ihrem `CREATE VIEW`: als `features` in `gpkg_contents` und
  zusätzlich in `gpkg_geometry_columns` mit Geometriespalte, Geometrietyp und
  SRS 2056.

## Fehlerverhalten

Wenn ein `ili2gpkg`-Importschritt ein nicht erfolgreiches Ergebnis meldet,
protokolliert der Prozess die Worker-Ausgabe auf Debug-Level und gibt `null` für
`GeneratedGeopackage` zurück. Die verbleibenden Anreicherungsschritte werden
übersprungen. Nachgelagerte Prozessoren erkennen das fehlende GPKG über ihre
eigene Pre-Condition und brechen die Pipeline ab.
