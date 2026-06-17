# VSA Geopackage Generation Prozess

Der VSA Geopackage Generation Prozess importiert die vom
[VSA Matcher](vsa-matcher.md) erzeugten INTERLIS-Transferdateien in das
schema-only GeoPackage-Template, reichert das Ergebnis mit Checker-CSV-Daten und
einer Fehlermatrix an und materialisiert analytische Tabellen. Er gibt ein
einzelnes befülltes GeoPackage für die nachgelagerten Prozessoren aus.

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

## Interlis-Import

Die drei Transferdateien werden nacheinander über
`IIli2GpkgClient.ImportToGeoPackageAsync` in das Template-GeoPackage importiert.
Jeder Import liest den aktuellen GeoPackage-Stream plus eine XTF und schreibt das
Ergebnis in eine neu angelegte Pipeline-Datei, die zum Input des nächsten
Schritts wird.

Import-Reihenfolge:

1. `defaultOrgs`: Standard-Organisationstabelle
2. `userOrgs`: *nur wenn eine Organisationstabelle aus dem Upload vorhanden ist*
3. `dssMini`: GEP-Transferdatei

Jeder Import schreibt in eine temporäre `gpkg-step-{label}.gpkg`-Datei, die zum
Input des nächsten Schritts wird.

### ili2gpkg-Argumente

Die folgenden `ili2gpkg`-Flags werden für jeden Importschritt gesetzt:

| Argument                | Wert | Begründung                                                               |
|-------------------------|------|-------------------------------------------------------------------------|
| `--skipReferenceErrors` | an   | Fortfahren, wenn XTF-Referenzen nicht aufgelöst werden können.           |
| `--skipGeometryErrors`  | an   | Fortfahren, wenn Geometriefehler auftreten.                             |
| `--disableValidation`   | an   | Die INTERLIS-Validierung ist Aufgabe des GEP-Checkers, nicht unsere.     |
| `--importTid`           | an   | Importiert die INTERLIS-TID in die Datenbank (für nachgelagerte Joins erforderlich). |

## Checker-CSV-Import

Nach dem INTERLIS-Import werden die drei Checker-CSV-Dateien als Tabellen
`checker_csv_t`, `checker_csv_a` und `checker_csv_fp` in das GeoPackage
importiert. Die CSV-Dateien verwenden die Windows-1252-Kodierung. Für jede
Tabelle werden Join-Indizes auf (`ErrorId`, `Model`, `Class`) erstellt.

## Fehlermatrix-Import

Die Fehlermatrix-XLSX wird in die Tabelle `error_matrix` importiert, mit einem
Join-Index auf (`cid`, `model`, `class_de`).

## Analytics

Zunächst werden zwei analytische Views erstellt:

1. `v_checker_csv_all` vereinigt die drei Checker-CSV-Tabellen mit einer
   `source`-Spalte (`T`, `A`, `FP`).
2. `v_checker_errors` verknüpft den Vereinigungs-View mit der Fehlermatrix und
   reichert jede CSV-Zeile mit lokalisierten Beschreibungen und Prioritäten an.

Anschliessend materialisiert `ErrorDataMaterializer` zwei Tabellen aus dem
Errors-View:

- **`ca_error_data`** enthält eine Zeile pro Checker-Fehler. Jede Zeile trägt die
  Fehlerbeschreibung, Prioritätsspalten (`wk`, `gep`), die Empfehlung und (für
  Leitung und Knoten) die Anreicherung aus den Feature-Tabellen
  (`funktionhierarchisch`, `eigentuemer`, `status`). Feature-Klassen, deren
  Tabellen nicht im GeoPackage vorhanden sind, werden von einem Catch-all
  behandelt, das die Anreicherungsspalten auf NULL setzt.
- **`ca_error_object`** aggregiert `ca_error_data` nach (`tid`, `class`) mit
  `COUNT(*)`, `MAX(wk)` und `MAX(gep)`.

Die `check_type`-Spalte wird aus dem CSV-Feld `Module` abgeleitet: `reader` wird
zu `ig`, andernfalls ist der Wert die CSV-Quelle (`T`, `A`, `FP`). Die
`module`-Spalte folgt einer ähnlichen Logik: `reader` wird zu `igcheck`, alles
andere zu `gep_check`.

Der finale Output wird nach `generated.gpkg` geschrieben.

## Fehlerverhalten

Wenn ein `ili2gpkg`-Importschritt ein nicht erfolgreiches Ergebnis meldet,
protokolliert der Prozess die Worker-Ausgabe auf Debug-Level und gibt `null` für
`generatedGeopackage` zurück. Die verbleibenden Anreicherungsschritte werden
übersprungen. Nachgelagerte Prozessoren erkennen das fehlende GPKG über ihre
eigene Pre-Condition und brechen die Pipeline ab.
