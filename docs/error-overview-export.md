# Error Overview Export Prozess

Der Error Overview Export Prozess liest die materialisierten Fehlertabellen aus
dem von der [Geopackage Generation](vsa-geopackage-generation.md) erzeugten
GeoPackage und exportiert sie in eine Excel-Arbeitsmappe (XLSX) mit zwei
konfigurierbaren Datenblättern und optionalen Pivot-Übersichtsblättern für die
WK- und GEP-Prioritäten.

Zusätzlich befüllt der Prozess eine Kopie der kantonalen Fehlermatrix-Vorlage
(`ErrorMatrixKanton.xlsx`) mit der Attribut-Statistik aus `ca_statistics_attribute`
und gibt sie als zweiten Download zurück. Es entstehen also zwei Ausgabedateien:
die Fehlerübersicht und die kantonale Fehlermatrix.

Dieser Prozessor implementiert den Teil "Fehlerübersicht" des
[Excel Mapper](architektur.md#excel-mapper), der in der Architektur beschrieben
ist.

## Konfiguration

Alle Konfigurationsparameter werden aus der Pipeline-YAML aufgelöst
(`default_config` / `process_config_overwrites`). Blattnamen, Spaltenpositionen
und Anzeige-Header sind vollständig konfigurationsgesteuert, sodass Änderungen am
Excel-Layout keine Codeänderungen erfordern.

| Parameter                      | Typ                          | Beschreibung                                                     |
|--------------------------------|------------------------------|-----------------------------------------------------------------|
| `errorDataSheet`               | `string`                     | Excel-Blattname für den `ca_error_data`-Export.                 |
| `errorDataAttributeMapping`    | `IDictionary<string,string>` | Bildet Attribut-Keys (SQLite-Spaltennamen) auf Excel-Header-Anzeigenamen ab. |
| `errorDataColumnMapping`       | `IDictionary<string,string>` | Bildet dieselben Attribut-Keys auf Excel-Spaltenbuchstaben ab (A, B, ...). |
| `errorObjectSheet`             | `string`                     | Excel-Blattname für den `ca_error_object`-Export.               |
| `errorObjectAttributeMapping`  | `IDictionary<string,string>` | Bildet Attribut-Keys auf Anzeigenamen für das Objektblatt ab.   |
| `errorObjectColumnMapping`     | `IDictionary<string,string>` | Bildet dieselben Attribut-Keys auf Spaltenbuchstaben für das Objektblatt ab. |
| `overviewWkSheet`              | `string?`                    | Blattname für die WK-Pivot-Übersicht. `null` zum Überspringen.  |
| `overviewGepSheet`             | `string?`                    | Blattname für die GEP-Pivot-Übersicht. `null` zum Überspringen. |
| `overviewRowFields`            | `IList<string>?`             | Attribut-Keys für die Pivot-Zeilenfelder (nach dem Prioritätsfeld). |
| `overviewFilterFields`         | `IList<string>?`             | Attribut-Keys für die Pivot-Berichtsfilterfelder.               |
| `overviewValueField`           | `string?`                    | Attribut-Key für das Pivot-Zählwertfeld.                        |
| `overviewValueName`            | `string?`                    | Anzeigename für die Pivot-Wertespalte.                          |
| `cantonErrorObjectSheet`       | `string`                     | Blattname des Rohdatenblatts in der kantonalen Fehlermatrix-Vorlage (`raw_data`). |
| `cantonErrorColumnMapping`     | `IDictionary<string,string>` | Bildet Attribut-Keys von `ca_statistics_attribute` auf Excel-Spaltenbuchstaben im Rohdatenblatt ab. |

### Mapping-Validierung

Das `attributeMapping` und das `columnMapping` jedes Blatts müssen exakt
dieselbe Menge an Keys definieren. Eine Abweichung (Key im einen vorhanden, im
anderen nicht) führt zur Konstruktionszeit zu einer `ArgumentException`, die die
abweichenden Keys auflistet.

### Pivot-Übersichts-Validierung

Wenn entweder `overviewWkSheet` oder `overviewGepSheet` gesetzt ist, müssen alle
übrigen `overview*`-Parameter angegeben werden. Jeder Attribut-Key in
`overviewRowFields`, `overviewFilterFields` und `overviewValueField` muss in
`errorDataAttributeMapping` vorhanden sein; ein fehlender Key führt zur
Konstruktionszeit zu einer `ArgumentException`. Das WK-Blatt verwendet das
Attribut `uc` als erstes Zeilenfeld, das GEP-Blatt das Attribut `gsp`.

### Kantonale Fehlermatrix

`cantonErrorColumnMapping` bildet die acht Spalten von `ca_statistics_attribute`
(`tabelle`, `attribut`, `anzahl_total`, `anzahl_paa`, `anzahl_saa`, `anzahl_null`,
`anzahl_null_paa`, `anzahl_null_saa`) auf Spaltenbuchstaben des Rohdatenblatts der
Vorlage ab. Anders als die Fehlerdatenblätter hat das Rohdatenblatt **keine
Kopfzeile**: die Statistikzeilen werden ab Zeile 1 geschrieben. Für dieses Mapping
gibt es keine Startup-Validierung; es adressiert schlicht die Spalten, die die
Vorlage im Rohdatenblatt erwartet. Die View liefert zusätzlich eine
`sortierung`-Spalte, die die Zeilenreihenfolge festlegt und beim Rücklesen für
`ORDER BY sortierung` verwendet wird (nicht ins Excel geschrieben); so entsprechen
die Zeilen den fest verdrahteten Zellbezügen der Validierungsblätter der Vorlage.

## Inputs

| Parameter    | Quelle                | Typ             | Beschreibung                                            |
|--------------|-----------------------|-----------------|--------------------------------------------------------|
| `geopackage` | Geopackage Generation | `IPipelineFile` | Das befüllte GeoPackage mit `ca_error_data`, `ca_error_object` und `ca_statistics_attribute`. |
| `cantonErrorMatrixTemplate` | Ressourcen (`${file()}`) | `IPipelineFile` | Excel-Vorlage der kantonalen Fehlermatrix (`ErrorMatrixKanton.xlsx`), direkt aus dem Ressourcenverzeichnis injiziert. |

## Output

`RunAsync` gibt ein `ErrorOverviewExportResult` mit den folgenden Properties zurück:

| Property             | Typ             | Beschreibung                                   |
|-----------------|-----------------|------------------------------------------------|
| `ErrorOverview` | `IPipelineFile` | Die erzeugte Excel-Arbeitsmappe (`error-overview.xlsx`). Wird über die Output-Action `Download` bereitgestellt. |
| `CantonErrorMatrix` | `IPipelineFile` | Beschreibbare Kopie der Vorlage `ErrorMatrixKanton.xlsx` mit der in das Rohdatenblatt geschriebenen Attribut-Statistik (`kantonale_fehlermatrix.xlsx`). Wird über die Output-Action `Download` bereitgestellt. |
| `StatusMessage` | `LocalizedText` | Lokalisierte Statusmeldung mit der Anzahl exportierter Fehler. Wird über die Output-Action `StatusMessage` in der Oberfläche angezeigt. |

## Verarbeitung

1. Das GeoPackage wird über `Microsoft.Data.Sqlite` schreibgeschützt geöffnet.
2. Für jedes konfigurierte Blatt (`ca_error_data`, `ca_error_object`):
   - Ein Arbeitsblatt wird mit dem konfigurierten Blattnamen erstellt.
   - Zeile 1 wird mit den Header-Anzeigenamen aus dem Attribut-Mapping befüllt,
     platziert an den durch das Spalten-Mapping definierten Spaltenpositionen.
   - Alle Zeilen der Quelltabelle werden abgefragt (nur die gemappten Spalten)
     und ab Zeile 2 geschrieben.
   - Zellwerte behalten ihren SQLite-Typ: Ganzzahlen und Gleitkommazahlen
     bleiben in Excel numerisch, Text bleibt Text, NULL-Zellen bleiben leer.
3. Falls konfiguriert, werden Pivot-Übersichtsblätter erstellt. Jedes Blatt
   enthält eine ClosedXML-Pivot-Tabelle, die auf den verwendeten Bereich des
   Fehlerdatenblatts verweist. Die Pivot-Tabelle gruppiert Fehler nach
   Prioritätsstufe (WK oder GEP), Klasse und Fehlertyp, mit konfigurierbaren
   Berichtsfiltern und einer Zählaggregation. Spalte A wird auf Breite 105 und
   Spalte B auf Breite 13 gesetzt.
4. Die Arbeitsmappe wird über ClosedXML in eine Pipeline-Output-Datei
   geschrieben (`ErrorOverview`).
5. Die kantonale Fehlermatrix wird befüllt (`ExportCantonMatrixAsync`):
   - Über `IPipelineFileManager.CreateWritableCopyAsync` wird eine eigene, beschreibbare
     Kopie der Vorlage `ErrorMatrixKanton.xlsx` angelegt und in place bearbeitet.
   - Ist die Tabelle `ca_statistics_attribute` vorhanden, werden ihre Zeilen
     (`ORDER BY sortierung`) aus derselben schreibgeschützten Verbindung in das
     Rohdatenblatt (`cantonErrorObjectSheet`) geschrieben: jeder Attribut-Key in die
     per `cantonErrorColumnMapping` definierte Spalte, ab Zeile 1 und ohne Kopfzeile.
     Zellwerte behalten ihren SQLite-Typ (Ganzzahl / Gleitkommazahl bleiben
     numerisch); NULL-Werte werden übersprungen, die Zelle bleibt leer.
   - Fehlt `ca_statistics_attribute` (z.B. bei minimalen Testfixtures), bleibt das
     Rohdatenblatt leer und es wird eine Warnung protokolliert.
   - Vor dem Schreiben werden die Inhalte des benutzten Bereichs geleert, aber
     **keine Zeilen gelöscht**: die Validierungsblätter der Vorlage verweisen per
     fester Zelle auf das Rohdatenblatt, ein Zeilenlöschen würde diese Referenzen zu
     `#REF!` machen.
   - Zum Schluss wird das erste Arbeitsblatt wieder aktiviert, damit die Mappe beim
     Öffnen auf dem Validierungsblatt steht und nicht auf dem Rohdatenblatt.
