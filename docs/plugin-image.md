# Plugin-Image

Jedes Release baut ein Container-Image mit dem Plugin und seinen Ressourcen. geopilot hängt es schreibgeschützt ein. So kommt das Plugin ohne Kopie von Hand ins Deployment ([geopilot#1029](https://github.com/geowerkstatt/geopilot/issues/1029)).

## Inhalt

Das Image enthält nur Dateien und startet nie ein Programm. Es hat zwei Ordner:

| Ordner       | Inhalt                                                                                                          |
|--------------|-----------------------------------------------------------------------------------------------------------------|
| `plugin/`    | Publish-Output des Plugins, nur für linux-x64.                                                                  |
| `resources/` | Die Dateien für den Betrieb aus `Resources/vsa-checker-analytics/`: drei Fehlermatrizen und zwei GeoPackage-Vorlagen. |

Die Testdaten in `Resources/` kommen nicht ins Image.

## Release

`release.yml` pusht das Image als `ghcr.io/geowerkstatt/vsa-checker-analytics:v<Version>`. Das ZIP am Release gibt es weiterhin.

Ein Deployment sollte das Image per Digest pinnen, zum Beispiel `ghcr.io/geowerkstatt/vsa-checker-analytics:v<Version>@sha256:<Digest>`. Ein Tag allein kann später auf ein anderes Image zeigen.

Zum Image gehört eine Build-Attestation. Dieser Befehl prüft, dass das Image aus dem Release-Workflow von `main` stammt:

```bash
gh attestation verify oci://ghcr.io/geowerkstatt/vsa-checker-analytics@sha256:<Digest> \
  --repo geowerkstatt/vsa-checker-analytics \
  --cert-identity https://github.com/geowerkstatt/vsa-checker-analytics/.github/workflows/release.yml@refs/heads/main
```

## Einhängen

Der Host hängt die beiden Ordner getrennt ein, beide schreibgeschützt:

| Ordner im Image | Pfad im geopilot-Container                                             |
|-----------------|------------------------------------------------------------------------|
| `plugin/`       | `/plugins/vsa-checker-analytics`                                       |
| `resources/`    | `<Storage:ResourcesDirectory>/vsa-checker-analytics`, im Standard `/resources/vsa-checker-analytics` |

geopilot lädt das Plugin über die Einstellung `Pipeline:Plugins`, zum Beispiel als Umgebungsvariable:

```text
Pipeline__Plugins=/plugins/vsa-checker-analytics/VSACheckerAnalytics.dll
```

In Kubernetes ab 1.36 ist das ein Image-Volume, zweimal eingehängt mit `subPath`:

```yaml
containers:
  - name: geopilot
    volumeMounts:
      - name: vsa-checker-analytics
        mountPath: /plugins/vsa-checker-analytics
        subPath: plugin
        readOnly: true
      - name: vsa-checker-analytics
        mountPath: /resources/vsa-checker-analytics
        subPath: resources
        readOnly: true
volumes:
  - name: vsa-checker-analytics
    image:
      reference: ghcr.io/geowerkstatt/vsa-checker-analytics:v<Version>@sha256:<Digest>
```

Docker Compose ab v5.5.0 hängt das Image genauso ein, mit `type: image` und `image.subpath`.

## Ressourcen in der Pipeline

Die Fehlermatrizen liest die Pipeline mit `${file(...)}`. Der Pfad ist relativ zu `Storage:ResourcesDirectory` und beginnt darum mit `vsa-checker-analytics/`. Die [Referenz-Pipeline](../Pipelines/vsaCheckerAnalytics.yaml) macht es so:

```yaml
errorMatrix: "${file(vsa-checker-analytics/errorMatrix.xlsx)}"
cantonErrorMatrixTemplate: "${file(vsa-checker-analytics/errorMatrixKanton_de.xlsx)}"
```

Die GeoPackage-Vorlagen sind dagegen normale Pfade in der Konfiguration des [VSA Matchers](vsa-matcher.md). Sie brauchen den vollen Pfad. Zusammen mit den beiden Organisationstabellen sieht das so aus:

```yaml
processes:
  - id: vsa_matcher
    implementation: VsaCheckerAnalytics.Processors.VsaMatcher.VsaMatcherProcess
    default_config:
      geoPackageTemplatePath2020: /resources/vsa-checker-analytics/template_ca_dssmini_2020_d.gpkg
      geoPackageTemplatePath20201: /resources/vsa-checker-analytics/template_ca_dssmini_2020_1_d.gpkg
      vsaOrgTableUrl2020: https://www.vsa.ch/models/organisation/vsa_organisationen.xtf
      vsaOrgTableUrl20201: https://www.vsa.ch/models/organisation/vsa_organisationen_2020_1.xtf
```

## Lokal bauen

```bash
dotnet publish src/VSACheckerAnalytics/VSACheckerAnalytics.csproj -c Release -r linux-x64 -o publish
docker build -t vsa-checker-analytics:local .
```
