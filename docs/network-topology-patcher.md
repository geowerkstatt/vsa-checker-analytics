# Network Topology Patcher Prozess

Der Network Topology Patcher übernimmt ein vorbereitetes GeoPackage
aus der [Geopackage-Generation](vsa-geopackage-generation.md) und führt eine Netzwerkvervollständigung durch, damit im Anschluss Netzverfolgungstools eingesetzt werden können (z.B. QGIS).
Es werden folgende Vervollständigungen gemacht:      
1. Lageungleichheit von Knoten und Leitungsendpunkten: 
   Kanten (`leitungen`) referenzieren ihren Start- und Endknoten (`knoten`) über Attribute (`knoten_vonref`, `knoten_nachref`), 
   aber die tatsächlichen Endpunkt-Stützpunkte der Liniengeometrie (`verlauf`) stimmen nicht in allen Fällen mit den referenzierten Knotenkoordinaten überein. 
   Es müssen kurze Verbindungsliniensegmente erstellt werden, um diese Lücken zu schliessen, sodass jede Kante exakt von `Knoten(knoten_vonref)` bis `Knoten(knoten_nachref)` verläuft. 
2. Verbindungslinien für Pumpen und Wehre:
   Pumpen und Wehre (`ueberlauf_foerderaggregat`) sind nur attributiv als Verknüpfung zweier Knoten beschrieben. Zusätzliche Liniensegmente sollen zwischen diesen Knotenpaaren konstruiert werden, damit die Zuordnung graphisch (auf der GIS-Canvas) kontrolliert werden kann.

Die Verbindungslinien werden in folgende Layers eingetragen:
- **`ca_topo_network_edges`**:
  Enthält für jede ursprüngliche `leitung` ein Feature, wobei die Liniengeometrie (`verlauf`)
  allenfalls mit einer Verbindungslinie am Anfang und/oder am Ende erweitert wurde. Eine
  Verbindungslinie wird immer dann eingefügt, wenn der referenzierte Knoten auflösbar ist und
  der Abstand zwischen Knotenkoordinate und `verlauf`-Endpunkt mindestens `MinConnectorLength`
  beträgt — unabhängig von der `funktion` des Knotens. So entsteht ein topologisch
  geschlossenes Netzwerk für nachgelagerte Netzverfolgungstools.
  Zudem gibt es für jedes `ueberlauf_foerderaggregat` ein Feature mit der Linie zwischen den beiden Knoten.
- **`ca_topo_extra_edges`**:
  Enthält nur die zusätzlich erzeugten Verbindungslinien der `leitungen` und `ueberlauf_foerderaggregat` ohne die ursprünglichen `leitungen`.
  Die Verbindungslinien für `leitungen` werden hier zusätzlich nach `funktion` des Knotens gefiltert, zu welchem die Verbindungslinie führt.
  Nur Verbindungslinien zu diesen Knoten-Funktionen werden auf diesem Layer eingetragen:
    - `abflussloseGrube`
    - `Absturzbauwerk`
    - `Abwasserfaulraum`
    - `Duekerkammer`
    - `Duekeroberhaupt`
    - `Faulgrube`
    - `Gelaendemulde`
    - `Geschiebefang`
    - `Guellegrube`
    - `Klaergrube`
    - `Regenbecken_Durchlaufbecken`
    - `Regenbecken_Fangbecken`
    - `Regenbecken_Fangkanal`
    - `Regenbecken_Regenklaerbecken`
    - `Regenbecken_Regenrueckhaltebecken`
    - `Regenbecken_Regenrueckhaltekanal`
    - `Regenbecken_Stauraumkanal`
    - `Regenbecken_Verbundbecken`
    - `Wirbelfallschacht`
    - `Pumpwerk`
    - `Trennbauwerk`
    - `Regenueberlauf`

## Konfiguration

Der Prozess nimmt keine Pipeline-Konfiguration entgegen. Folgende Konstanten
sind in `NetworkTopologyPatcher` fest hinterlegt:

| Konstante                    | Wert          | Beschreibung                                                                                       |
|------------------------------|---------------|----------------------------------------------------------------------------------------------------|
| `MinConnectorLength`         | `0.10` m      | Verbindungssegmente kürzer als dieser Wert werden verworfen. |
| `Srid`                       | `2056`        | SRID aller verarbeiteten VSA-Daten und der ausgegebenen Geometriespalten.        |
| `ValidConnectorFunktionen`   | Allow-Liste   | Knoten-`funktion`-Werte (Schächte, Becken, Pumpwerke etc.), für die ein Verbindungssegment zusätzlich als eigenes Feature in `ca_topo_extra_edges` abgelegt wird. Die Allow-Liste beeinflusst NICHT die Erzeugung der Verbindungssegmente in `ca_topo_network_edges` — dort werden die Lücken zu allen auflösbaren Knoten geschlossen. |

## Inputs

| Parameter     | Quelle                    | Typ             | Beschreibung                                                                                       |
|---------------|---------------------------|-----------------|----------------------------------------------------------------------------------------------------|
| `geoPackage`  | Geopackage-Generierung    | `IPipelineFile` | Vorbereitetes GeoPackage mit den Tabellen `leitung`, `knoten_lage`, `knoten` und `ueberlauf_foerderaggregat`. |

## Output

| Key                  | Typ             | Beschreibung                                                                                       |
|----------------------|-----------------|----------------------------------------------------------------------------------------------------|
| `patchedGeopackage`  | `IPipelineFile` | Eine Kopie des Eingabe-GeoPackages mit den beiden Topologie-Layern.    |

Das Output-GeoPackage enthält zwei neue Feature-Tabellen, die in
`gpkg_contents` und `gpkg_geometry_columns` registriert werden, damit QGIS sie
automatisch als darstellbare LineString-Layer erkennt (SRID 2056). Die
Layer-Ausdehnung (Bounding-Box in `gpkg_contents`) wird aus den geschriebenen
Topologie-Geometrien berechnet, damit QGIS auf das tatsächliche Datengebiet
zoomen kann:

### `ca_topo_network_edges`

| Spalte           | Beschreibung                                                                                                                              |
|------------------|-------------------------------------------------------------------------------------------------------------------------------------------|
| `fid`            | Auto-Increment Primärschlüssel des Features.                                                                                              |
| `src_tid`        | `tid` der Ursprungszeile (`leitung` bzw. `ueberlauf_foerderaggregat`), aus der die Kante erzeugt wurde.                                   |
| `knoten_vonref`  | Referenz auf den Startknoten (`leitung.knoten_vonref` bzw. `ueberlauf_foerderaggregat.knotenref`).                                        |
| `knoten_nachref` | Referenz auf den Endknoten (`leitung.knoten_nachref` bzw. `ueberlauf_foerderaggregat.knoten_nachref`).                                    |
| `diff_start`     | Länge des am Anfang angefügten Verbindungssegments in Metern (`0`, wenn kein Segment erzeugt wurde; `NULL` für `ueberlauf_foerderaggregat`). |
| `diff_end`       | Länge des am Ende angefügten Verbindungssegments in Metern (`0`, wenn kein Segment erzeugt wurde; `NULL` für `ueberlauf_foerderaggregat`). |
| `linetype`       | Herkunft der Kante: `topologielinie` für aus `leitung` zusammengeführte Linien, `ueberlauf_foerderaggregat` für synthetische Pumpen-/Wehr-Linien. |
| `geom`           | LineString-Geometrie (GeoPackage Binary, SRID 2056): zusammengeführter Verlauf inkl. allfälliger Verbindungssegmente.                     |

### `ca_topo_extra_edges`

| Spalte      | Beschreibung                                                                                                                                       |
|-------------|----------------------------------------------------------------------------------------------------------------------------------------------------|
| `fid`       | Auto-Increment Primärschlüssel des Features.                                                                                                       |
| `src_tid`   | `tid` der Ursprungszeile (`leitung` bzw. `ueberlauf_foerderaggregat`), aus der das Verbindungssegment erzeugt wurde.                               |
| `tid_pipe`  | `tid` der zugehörigen `leitung` für Leitungs-Verbindungssegmente (entspricht `src_tid`); `NULL` für Linien aus `ueberlauf_foerderaggregat`.        |
| `diff`      | Länge des Verbindungssegments in Metern.                                                                                                           |
| `linetype`  | Herkunft des Segments: `topologielinie` für Leitungs-Connector, `ueberlauf_foerderaggregat` für Pumpen-/Wehr-Linien.                               |
| `geom`      | LineString-Geometrie (als GeoPackage Binary, SRID 2056) des Verbindungssegments.                                                                       |

## Berechnung der Verbindungslinien für `leitungen`

Für jede `leitung` werden die beiden Enden ihres `verlauf` unabhängig
voneinander betrachtet:

1. Auf der Startseite wird der über `knoten_vonref` referenzierte Knoten
   aufgelöst, auf der Endseite der über `knoten_nachref` referenzierte Knoten.
   Lässt sich der Knoten nicht auflösen (NULL-Referenz oder unbekannter
   Knoten), bleibt der `verlauf` auf dieser Seite unverändert.
2. Andernfalls wird ein Verbindungssegment zwischen der Knotenkoordinate
   (aus `knoten_lage`) und dem entsprechenden Endpunkt des `verlauf`
   konstruiert. Ist dieses Segment kürzer als `MinConnectorLength`, wird es
   verworfen — die Lage gilt dann als hinreichend deckungsgleich.
3. Die so ermittelten Verbindungssegmente werden mit dem ursprünglichen
   `verlauf` zu einer einzigen, durchgehenden Linie zusammengeführt und als
   Feature in `ca_topo_network_edges` eingetragen — unabhängig davon, welche
   `funktion` der referenzierte Knoten besitzt. Damit ist das Netzwerk
   topologisch geschlossen. Die Längen der beiden allfälligen
   Verbindungssegmente werden in `diff_start` bzw. `diff_end` festgehalten.
4. Pro Seite wird zusätzlich geprüft, ob die `funktion` des referenzierten
   Knotens in der Allow-Liste `ValidConnectorFunktionen` enthalten ist. Nur
   in diesem Fall wird das Verbindungssegment auch als eigenständiges
   Feature in `ca_topo_extra_edges` abgelegt, sodass die gepatchten Stellen
   isoliert dargestellt und kontrolliert werden können.
