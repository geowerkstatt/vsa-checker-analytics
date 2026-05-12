# GeoPackage-Tooling im VSA-Plugin

## Ausgangslage

Das VSA-Plugin arbeitet mit GeoPackage-Dateien (GPKG). Ein GeoPackage ist eine SQLite-Datenbank, die Geometrien als BLOBs in einem standardisierten Binärformat speichert. Für Prozessoren wie **Network Topology** und **Geopackage Generation** müssen diese Geometrien gelesen, manipuliert und zurückgeschrieben werden.

## Begriffe

| Begriff | Bedeutung |
|---------|-----------|
| **GPKG** | GeoPackage — eine SQLite-Datenbank mit Geodaten (OGC-Standard) |
| **GPB** | GeoPackage Binary — das Binärformat der Geometrie-BLOBs in GPKG-Tabellen |
| **WKB** | Well-Known Binary — standardisiertes Binärformat für Geometrien, eingebettet in GPB |
| **NTS** | NetTopologySuite — .NET-Bibliothek für Geometrie-Operationen |
| **SRS ID** | Spatial Reference System ID — Kennung des Koordinatensystems (z.B. 2056 = LV95) |

## Tooling-Entscheid

### Empfehlung: NTS + Microsoft.Data.Sqlite

| NuGet-Paket | Zweck |
|-------------|-------|
| `Microsoft.Data.Sqlite` | SQLite-Zugriff auf die GPKG-Datei |
| `NetTopologySuite` | Geometrien lesen, schreiben und manipulieren |

### Verworfene Alternativen

| Alternative | Grund für Ausschluss |
|-------------|---------------------|
| **GDAL (.NET Bindings)** | Native C/C++ Abhängigkeit, komplex mit AssemblyLoadContext-Plugin-Loading, Overkill für Vektor-Geometrie |
| **NetTopologySuite.IO.GeoPackage** | Letztes Update 2019, bekannte Bugs, unmaintained |

## Aufbau eines Geometrie-BLOBs (GPB-Format)

Jede Geometrie-Spalte in einer GPKG-Tabelle enthält einen BLOB im GeoPackage Binary (GPB) Format. Dieses besteht aus einem Header gefolgt von der eigentlichen Geometrie im WKB-Format:

```
GPB = Header + WKB

┌──────────────────────────────────────────┐
│ GPB-Header (8–72 Bytes)                  │
│  ├─ Magic:    2 Bytes  ("GP")            │
│  ├─ Version:  1 Byte                     │
│  ├─ Flags:    1 Byte  (Byte-Order,       │
│  │                      Envelope-Typ)    │
│  ├─ SRS ID:   4 Bytes (Koordinatensystem)│
│  └─ Envelope: 0–64 Bytes (Bounding Box)  │
├──────────────────────────────────────────┤
│ WKB-Geometrie (variable Länge)           │
│  (Standard OGC Well-Known Binary)        │
└──────────────────────────────────────────┘
```

### Envelope-Grössen

Der Envelope-Typ im Flags-Byte bestimmt die Grösse der Bounding Box:

| Code | Typ  | Bytes |
|------|------|-------|
| 0    | Keine | 0    |
| 1    | XY    | 32   |
| 2    | XYZ   | 48   |
| 3    | XYM   | 48   |
| 4    | XYZM  | 64   |

## Verarbeitungs-Pipeline (Lesen → Bearbeiten → Schreiben)

```
Microsoft.Data.Sqlite   →  GPKG öffnen, BLOB aus Tabelle lesen
        │
        ▼
Eigener GPB-Reader      →  Header parsen, SRS ID merken, WKB extrahieren
        │
        ▼
NTS WKBReader           →  WKB → NTS Geometry-Objekt
        │
        ▼
NTS Operationen         →  z.B. Snap, MakeValid, Buffer, Union
        │
        ▼
NTS WKBWriter           →  Geometry → WKB
        │
        ▼
Eigener GPB-Writer      →  Header neu aufbauen + WKB → BLOB
        │
        ▼
Microsoft.Data.Sqlite   →  BLOB zurück in GPKG schreiben
```

### GPB-Reader (Lesen)

1. Magic Bytes `0x47, 0x50` prüfen
2. Flags-Byte lesen → Byte-Order und Envelope-Grösse bestimmen
3. SRS ID lesen (4 Bytes)
4. Envelope überspringen
5. Rest (= WKB) an NTS `WKBReader` übergeben
6. `geometry.SRID` manuell aus dem Header setzen

### GPB-Writer (Schreiben)

1. Header aufbauen: Magic, Version, Flags, SRS ID, Envelope aus Geometry berechnen
2. Geometrie via NTS `WKBWriter` serialisieren
3. Header + WKB zusammenfügen → fertiger BLOB

## Relevante GPKG-Metadaten-Tabellen

Beim Zugriff auf ein GeoPackage sind folgende Systemtabellen relevant:

| Tabelle | Inhalt |
|---------|--------|
| `gpkg_contents` | Verzeichnis aller Feature-Tabellen mit Metadaten |
| `gpkg_geometry_columns` | Welche Spalte einer Tabelle die Geometrie enthält |
| `gpkg_spatial_ref_sys` | Koordinatensystem-Definitionen (SRS ID → EPSG etc.) |

## Hinweise

- **SRS ID**: Der GPB-Header enthält die SRS ID, das WKB selbst nicht. Nach dem Deserialisieren muss `geometry.SRID` manuell gesetzt werden.
- **Byte-Order**: Header und WKB-Body können theoretisch unterschiedliche Byte-Order haben (Spec erlaubt es).
- **Eigener GPB-Reader/Writer**: Ca. 25 Zeilen C# pro Richtung. Der Header ist simpel genug, um auf externe Pakete zu verzichten.

## Bezug zur Pipeline

Der hier beschriebene Mechanismus wird in folgenden Prozessoren benötigt:

- **Geopackage Generation** — Geometrien aus importierten Daten lesen und aggregieren
- **Network Topology** — Netzknoten und Leitungen topologisch verbinden (Snapping, Linien erweitern, neue Kanten erstellen)
