# Error Overview Export Prozess

Der Error Overview Export Prozess liest die materialisierten Fehlertabellen aus
dem von der [Geopackage Generation](vsa-geopackage-generation.md) erzeugten
GeoPackage und exportiert sie zusammen mit [Objektstatistiken](#objektstatistiken)
für Teileinzugsgebiete, Knoten und Leitungen in eine Excel-Arbeitsmappe (XLSX).
Jedes Datenblatt und die Pivot-Blätter darüber sind auf dieselbe Weise konfiguriert.

Zusätzlich befüllt der Prozess eine Kopie der kantonalen Fehlermatrix-Vorlage
(`ErrorMatrixKanton.xlsx`) mit der Attribut-Statistik aus `ca_statistics_attribute`
und gibt sie als zweiten Download zurück. Es entstehen also zwei Ausgabedateien:
die Fehlerübersicht und die kantonale Fehlermatrix.

Dieser Prozessor implementiert den Teil "Fehlerübersicht" des
[Excel Mapper](architektur.md#excel-mapper), der in der Architektur beschrieben
ist.

## Konfiguration

Alle Konfigurationsparameter werden aus der Pipeline-YAML aufgelöst
(`default_config` / `process_config_overwrites`). Jedes Datenblatt wird auf dieselbe
Weise beschrieben, Fehlerblätter wie Statistikblätter: Blattname und Spalten. Das
Rohdatenblatt der kantonalen Fehlermatrix ebenso, nur ohne Header. Die Pivot-Blätter
über den Datenblättern sind fest im Code definiert (siehe [Pivot-Blätter](#pivot-blätter)).

| Parameter       | Typ                  | Beschreibung |
|-----------------|----------------------|--------------|
| `errorData`     | `DataSheetConfig`    | Blatt der Fehler (`ca_error_data`); die Statusmeldung zählt seine Zeilen. |
| `errorObject`   | `DataSheetConfig`    | Blatt der Fehlerobjekte (`ca_error_object`). |
| `catchmentArea` | `DataSheetConfig`    | Statistikblatt der Teileinzugsgebiete (`v_vsa_teileinzugsgebiet`). |
| `node`          | `DataSheetConfig`    | Statistikblatt der Knoten (`v_vsa_knoten` plus `geom_detail`, die Detailgeometrie). |
| `reach`         | `DataSheetConfig`    | Statistikblatt der Leitungen (`v_vsa_leitung` plus `function_hierarchic_group`, PAA oder SAA). |
| `cantonMatrix`  | `CantonMatrixConfig` | Rohdatenblatt der kantonalen Fehlermatrix-Vorlage (`ca_statistics_attribute`), siehe [Kantonale Fehlermatrix](#kantonale-fehlermatrix). |

Die Quelle jedes Blatts (Tabelle, View oder Unterabfrage) ist im Code festgelegt; die
Konfiguration wählt daraus die Attribute. Ein Datenblatt sieht so aus:

```yaml
reach:
  sheet: reach_data
  columns:
    tid: { column: A, header: tid }
    geom: { column: AW, header: geometry (WKT) }
```

`columns` bildet jeden Attribut-Key der Quelle auf Spaltenbuchstabe und Header ab.

### Pivot-Blätter

Die Pivot-Blätter sind fest im Code definiert. Ihre Felder sind Attribut-Keys, die
Spalten ihres Datenblatts müssen sie deshalb enthalten.

| Datenblatt | Pivot-Blatt | Zeilenfelder | Filter | Werte |
|------------|-------------|--------------|--------|-------|
| `errorData` | `Uebersicht WK` | `uc`, `class`, `error` | `check_type`, `function_hierarchic`, `owner`, `status` | Anzahl `fid` («Anzahl Fehler») |
| `errorData` | `Uebersicht GEP` | `gsp`, `class`, `error` | wie WK | wie WK |
| `catchmentArea` | `drainage_system_current` | `drainage_system_current` | | Anzahl und Anteil `tid` |
| `node` | `function` | `function_hierarchic`, `function` | | Anzahl und Anteil `tid` |
| `reach` | `function_hierarchic` | `function_hierarchic` | | Anzahl und Anteil `tid` |
| `reach` | `usage_current` | `function_hierarchic_group`, `usage_current` | | Anzahl und Anteil `tid` |

Die Übersichten beginnen in A1 (die Filter oben), ohne Zwischensummen, Spalte A mit
Breite 105 und Spalte B mit Breite 13. Die Statistik-Pivots beginnen wie in den
Vorlagen in A3, mit Zwischensummen je Gruppe.

### Validierung

Beim Start lehnt der Prozess mit einer `ArgumentException` ab, bei den Datenblättern wie
beim Rohdatenblatt der kantonalen Fehlermatrix: ein Blatt ohne Namen oder Spalten, einen
Attribut-Key, der kein einfacher Spaltenname ist, eine Spalte mit ungültigem Buchstaben
und zwei Attribute in derselben Spalte. Bei den Datenblättern zusätzlich: eine Spalte
ohne Header, eine fehlende Spalte, die ein Pivot braucht, und doppelte Blattnamen in der
Arbeitsmappe (ohne Rücksicht auf Gross- und Kleinschreibung). Fehlt die Quelle eines
Datenblatts im GeoPackage, scheitert der Lauf beim Lesen.

### Kantonale Fehlermatrix

`cantonMatrix` beschreibt das Rohdatenblatt der Vorlage wie ein Datenblatt, aber ohne
Header:

```yaml
cantonMatrix:
  sheet: raw_data
  columns: { tabelle: A, attribut: B, anzahl_total: C, anzahl_paa: D, anzahl_saa: E, anzahl_null: F, anzahl_null_paa: G, anzahl_null_saa: H }
```

`columns` bildet die acht Spalten von `ca_statistics_attribute` auf die
Spaltenbuchstaben ab, die die Vorlage im Rohdatenblatt erwartet. Anders als die
Datenblätter hat das Rohdatenblatt **keine Kopfzeile**: die Statistikzeilen werden ab
Zeile 1 geschrieben. Die View liefert zusätzlich eine
`sortierung`-Spalte, die die Zeilenreihenfolge festlegt und beim Rücklesen für
`ORDER BY sortierung` verwendet wird (nicht ins Excel geschrieben); so entsprechen
die Zeilen den fest verdrahteten Zellbezügen der Validierungsblätter der Vorlage.

## Inputs

| Parameter    | Quelle                | Typ             | Beschreibung                                            |
|--------------|-----------------------|-----------------|--------------------------------------------------------|
| `geopackage` | Geopackage Generation | `IPipelineFile` | Das befüllte GeoPackage mit `ca_error_data`, `ca_error_object`, `ca_statistics_attribute` und den Views `v_vsa_teileinzugsgebiet`, `v_vsa_knoten` und `v_vsa_leitung`. |
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
2. Zuerst werden alle fünf Datenblätter gelesen (`DataSheetReader`): von jeder
   Quelle die konfigurierten Attribute, als unveränderliche Datensätze (`DataSheet`
   mit den `PivotSheet`s darüber). Geometrien werden dabei als WKT gelesen.
3. Danach schreibt der Prozess die Arbeitsmappe, für alle Blätter auf dieselbe
   Weise:
   - Ein Datenblatt erhält in Zeile 1 die Header und ab Zeile 2 die Zeilen, mit
     Autofilter und fixierter Kopfzeile. Zellwerte behalten ihren SQLite-Typ:
     Ganzzahlen und Gleitkommazahlen bleiben in Excel numerisch, Text bleibt
     Text, NULL-Zellen bleiben leer.
   - Jedes Pivot-Blatt enthält eine ClosedXML-Pivot-Tabelle über den geschriebenen
     Bereich seines Datenblatts und steht vor diesem Datenblatt.
   - Reihenfolge: Übersicht WK, Übersicht GEP, Fehlerdaten, Fehlerobjekte, dann je
     Objektklasse die Pivot-Blätter und ihr Rohdatenblatt. Die Mappe öffnet auf dem
     ersten Blatt.
4. Die Arbeitsmappe wird über ClosedXML in eine Pipeline-Output-Datei
   geschrieben (`ErrorOverview`).
5. Die kantonale Fehlermatrix wird befüllt (`ExportCantonMatrixAsync`):
   - Über `IPipelineFileManager.CreateWritableCopyAsync` wird eine eigene, beschreibbare
     Kopie der Vorlage `ErrorMatrixKanton.xlsx` angelegt und in place bearbeitet.
   - Ist die Tabelle `ca_statistics_attribute` vorhanden, werden ihre Zeilen
     (`ORDER BY sortierung`) aus derselben schreibgeschützten Verbindung in das
     Rohdatenblatt (`cantonMatrix.sheet`) geschrieben: jeder Attribut-Key in seine
     Spalte aus `cantonMatrix.columns`, ab Zeile 1 und ohne Kopfzeile.
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

## Objektstatistiken

Die Statistikblätter (`catchmentArea`, `node`, `reach`) bilden mit ihren Pivot-Blättern
die Vorlagen `statistics_catchment_area.xlsx`, `statistics_node.xlsx` und
`statistics_reach.xlsx` aus [Issue #71](https://github.com/geowerkstatt/vsa-checker-analytics/issues/71)
ab. Die Spalten stehen in der Pipeline-YAML, die Pivots fest im Code.

- Die Quelle der Knoten ergänzt `v_vsa_knoten` um `geom_detail` (`knoten.detailgeometrie`),
  die der Leitungen `v_vsa_leitung` um `function_hierarchic_group`: die Obergruppe von
  `function_hierarchic`, der Teil vor dem ersten Punkt (`PAA.Sammelkanal` → `PAA`), wie mit
  dem PO abgesprochen.
- Die Pivots zählen `tid` («Anzahl von tid») und zeigen den Anteil am Total («Anteil»,
  Format 0.00 %), mit Zwischensummen je Gruppe. Die Vorlage der Teileinzugsgebiete
  gruppierte versehentlich nach `direct_discharge_current`; gemeint ist laut PO
  `drainage_system_current`.
- Geometrien werden als WKT geschrieben. Excel fasst höchstens 32'767 Zeichen pro Zelle;
  längere oder nicht lesbare Geometrien bleiben leer, und eine Warnung nennt ihre Anzahl.
