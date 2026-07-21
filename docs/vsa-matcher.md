# VSA Matcher Prozess

Der VSA Matcher ordnet die Eingabedateien nach ihrer semantischen Rolle zu,
extrahiert Metadaten aus der GEP-Transferdatei und reichert die Pipeline mit
Application Resources und Daten aus dem VSA-Repository an.

Er ist der zentrale Klassifizierungsschritt: Alles davor (Entpacken) ist
generisch, alles danach (Aggregation, Topologie, Excel) hängt von den benannten
Outputs ab, die dieser Prozess erzeugt.

## Konfiguration

Die Konstruktorparameter werden vom `PipelineProcessFactory` aus der
Pipeline-YAML-Konfiguration aufgelöst. `ILogger` und `IPipelineFileManager`
werden vom Framework automatisch injiziert.

| Parameter                  | Typ      | Beschreibung                                                       |
|----------------------------|----------|-------------------------------------------------------------------|
| `geoPackageTemplatePath2020`  | `string` | Pfad zum GeoPackage-Template für Modellversion 2020.           |
| `geoPackageTemplatePath20201` | `string` | Pfad zum GeoPackage-Template für Modellversion 2020.1.         |
| `vsaOrgTableUrl2020`         | `string` | URL der Standard-Organisationstabelle (2020) im VSA-Repository (https://www.vsa.ch/models/organisation/vsa_organisationen.xtf).  |
| `vsaOrgTableUrl20201`        | `string` | URL der Standard-Organisationstabelle (2020.1) im VSA-Repository (https://www.vsa.ch/models/organisation/vsa_organisationen_2020_1.xtf).|

## Inputs

Die `RunAsync`-Methode erhält zwei Sammlungen:

| Parameter       | Quelle             | Typ                  | Beschreibung                                                        |
|-----------------|--------------------|----------------------|--------------------------------------------------------------------|
| `uploadFiles`   | Benutzer-Upload    | `IPipelineFile[]`  | Die ursprünglich hochgeladenen Dateien (GEP-Transferdatei, optionale Organisationstabelle, ZIP). |
| `unzippedFiles` | ZIP Unpacker       | `IPipelineFile[]`    | Vom vorangehenden Entpackschritt aus dem GEP-Checker-ZIP extrahierte Dateien. 3*3 Dateien, 9 insgesamt: CSV, XTF und Log für jede der drei VSA-Prüfklassen (a, FP, T). |

## Outputs

`RunAsync` gibt ein `Dictionary<string, object?>` mit den folgenden Keys zurück:

| Key                 | Typ               | Beschreibung                                                                 |
|---------------------|-------------------|-----------------------------------------------------------------------------|
| `gep`               | `IPipelineFile[]` | Alle hochgeladenen Dateien, die als GEP-Transferdateien identifiziert wurden. Die Post-Condition des Schritts stellt sicher, dass genau eine vorhanden ist. |
| `model_version`     | `string?`         | `"2020"` oder `"2020.1"`, abgeleitet aus dem GEP-ILI-Modellnamen. Nur gesetzt, wenn genau eine GEP-Datei gefunden wird. Für die weitere Verarbeitung zwingend; die Post-Condition des Schritts stellt ihre Präsenz sicher. |
| `language`          | `string?`         | `"DE"` oder `"FR"`, abgeleitet aus dem GEP-ILI-Modellnamen. Nur gesetzt, wenn genau eine GEP-Datei gefunden wird. Für die weitere Verarbeitung zwingend; die Post-Condition des Schritts stellt ihre Präsenz sicher. |
| `user_org_table`    | `IPipelineFile[]` | Alle hochgeladenen Dateien, die als benutzerdefinierte Organisationstabellen identifiziert wurden. Die Post-Condition des Schritts stellt sicher, dass null oder eine vorhanden ist. |
| `checker_csv_a`     | `IPipelineFile[]` | Checker-CSVs für ARA (a), erkannt am Dateinamen mit der Endung `_a_err`. Die Post-Condition des Schritts stellt sicher, dass genau eine vorhanden ist. |
| `checker_csv_fp`    | `IPipelineFile[]` | Checker-CSVs für Fachprüfungen (FP), erkannt am Dateinamen mit der Endung `_fp_err`. Die Post-Condition des Schritts stellt sicher, dass genau eine vorhanden ist. |
| `checker_csv_t`     | `IPipelineFile[]` | Checker-CSVs für Trägerschaft (T), erkannt am Dateinamen mit der Endung `_t_err`. Die Post-Condition des Schritts stellt sicher, dass genau eine vorhanden ist. |
| `gpkg_template`     | `IPipelineFile?`  | Aus den Application Resources kopiertes GeoPackage-Template, passend zur Modellversion. Für die weitere Verarbeitung zwingend; die Post-Condition des Schritts stellt seine Präsenz sicher. |
| `standard_org_table`| `IPipelineFile?`  | Aus dem VSA-Repository bezogene Standard-Organisationstabelle, passend zur Modellversion. Für die weitere Verarbeitung zwingend; die Post-Condition des Schritts stellt ihre Präsenz sicher. |
| `status_message`    | `LocalizedText`   | Lokalisierte Statusmeldung, die das Identifizierungsergebnis zusammenfasst (erkannte Modellversion und Sprache sowie die Anzahl gefundener Checker-CSVs, oder ein Hinweis auf eine fehlende bzw. mehrere GEP-Dateien). Wird über die Output-Action `StatusMessage` in der Oberfläche angezeigt. |

## Dateiidentifikation

### GEP-Transferdatei

Identifiziert durch Abgleich der im XTF-Header deklarierten INTERLIS-Modellnamen
gegen statische Modell-Sets. Der Modellabgleich erfolgt ohne Beachtung der
Gross-/Kleinschreibung. Sowohl das INTERLIS-2.4- als auch das 2.3-Header-Format
werden unterstützt.

Version 2020.1 wird vor 2020 geprüft (spezifischere zuerst).

| Sprache  | Modellversion | ILI-Modellname            |
|----------|---------------|---------------------------|
| de       | 2020          | `VSADSSMINI_2020_LV95`    |
| de       | 2020.1        | `VSADSSMINI_2020_1_LV95`  |
| fr       | 2020          | `VSASDEEMINI_2020_LV95`   |
| fr       | 2020.1        | `VSASDEEMINI_2020_1_LV95` |

### Organisationstabelle

Ebenfalls über ILI-Modellnamen-Abgleich identifiziert (ohne Beachtung der
Gross-/Kleinschreibung).

| Sprache  | ILI-Modellname                  |
|----------|---------------------------------|
| de       | `SIA405_Base_Abwasser_LV95`     |
| de       | `SIA405_Base_Abwasser_1_LV95`   |
| fr       | `SIA405_Base_Eaux_usees_LV95`   |
| fr       | `SIA405_Base_Eaux_usees_1_LV95` |

### Checker-CSVs

Nur `.csv`-Dateien innerhalb des `check`-Verzeichnisses (gesetzt vom
`UnzipProcess` über `OriginalRelativePath`) werden berücksichtigt. Der Dateiname
(ohne Endung) wird anschliessend gegen Regex-Muster abgeglichen.

| Output-Key       | Muster      | Beispiel-Treffer           |
|------------------|-------------|----------------------------|
| `checker_csv_a`  | `_a_err$`   | `check/gep_a_err.csv`     |
| `checker_csv_fp` | `_fp_err$`  | `check/gep_fp_err.csv`    |
| `checker_csv_t`  | `_t_err$`   | `check/gep_t_err.csv`     |
