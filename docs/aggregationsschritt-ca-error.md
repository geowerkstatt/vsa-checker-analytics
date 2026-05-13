# Aggregationsschritt: von `v_checker_errors` zu `ca_error_data` / `ca_error_object`

Skizze für den noch zu entwickelnden ETL-Schritt, der aus den drei CSV-Quellen
(`checker_csv_t`, `checker_csv_a`, `checker_csv_fp`) und der `error_matrix`
über `v_checker_errors` schliesslich `ca_error_data` (Detail) und
`ca_error_object` (verdichtet pro Feature) erzeugt.

## Stand der Daten

In den Testdaten kommen 7 Klassen vor:

| Klasse                       | Befunde |
|------------------------------|--------:|
| Leitung                      |     886 |
| Knoten                       |     505 |
| SK_Regenueberlauf            |     126 |
| Teileinzugsgebiet            |      80 |
| SK_Regenueberlaufbecken      |      54 |
| SK_Einleitstelle             |      26 |
| Ueberlauf_Foerderaggregat    |       1 |

Jede dieser Klassen ist eine eigene VSA-Tabelle mit unterschiedlichen Spalten
für die Anreicherung. Ein generischer Join wird also "polymorph" — pro Klasse
ein typisierter Pfad.

Die Werte für `wk` und `gep` kommen aus den Matrix-Spalten `prio_uc` und
`prio_gsp` (Werte `"1"` oder `"2"`, als TEXT gespeichert — brauchen einen
`CAST(... AS INTEGER)`).

## Pattern: View als Definition, Tabelle als Persistenz

Empfehlung: die ganze Aufbaulogik in einer **Build-View** kapseln und die
finale Tabelle mit einem trivialen `INSERT … SELECT * FROM <view>` füllen.
So bleibt die ETL-Logik versionierbar im Schema-Migrationsskript der
C#-Anwendung, und der eigentliche Code ist drei Zeilen.

```text
v_ca_error_data_build  (View)  ── INSERT ──►  ca_error_data  (Tabelle)
                                                    │
                                                    └── GROUP BY ──► ca_error_object (Tabelle)
```

## Stufe 1 — `v_ca_error_data_build` als UNION über die Klassen

Pro Klasse ein eigener `SELECT`-Block, alle per `UNION ALL` zusammen.

```sql
CREATE VIEW v_ca_error_data_build AS

-- ── Leitung ──────────────────────────────────────────────────────────────
SELECT
    e."Tid"                                   AS tid,
    e.source                                  AS check_type,        -- 'T'|'A'|'FP'
    e."Topic"                                 AS topic,
    e."Class"                                 AS class,
    e."ErrorId"                               AS errorid,
    COALESCE(e.cmsg_de, e."Description")      AS error,
    e."UserAttributes"                        AS detail,
    l.funktionhierarchisch                    AS funktionhierarchisch,
    org.bezeichnung                           AS eigentuemer,
    l.astatus                                 AS status,
    e."Category"                              AS category,
    e."Model"                                 AS model,
    e."Module"                                AS module,
    CAST(e.prio_uc  AS INTEGER)               AS wk,
    CAST(e.prio_gsp AS INTEGER)               AS gep,
    e.required_action_de                      AS recommendation,
    e.action_context_de                       AS recommendation_detail
FROM   v_checker_errors  e
LEFT JOIN leitung       l   ON l.T_Ili_Tid = e."Tid"
LEFT JOIN organisation  org ON org.T_Id    = l.eigentuemerref
WHERE  e."Class" = 'Leitung'

UNION ALL

-- ── Knoten ────────────────────────────────────────────────────────────────
SELECT
    e."Tid", e.source, e."Topic", e."Class", e."ErrorId",
    COALESCE(e.cmsg_de, e."Description"),
    e."UserAttributes",
    k.funktionhierarchisch,
    org.bezeichnung,
    k.astatus,
    e."Category", e."Model", e."Module",
    CAST(e.prio_uc AS INTEGER), CAST(e.prio_gsp AS INTEGER),
    e.required_action_de, e.action_context_de
FROM   v_checker_errors  e
LEFT JOIN knoten        k   ON k.T_Ili_Tid = e."Tid"
LEFT JOIN organisation  org ON org.T_Id    = k.eigentuemerref
WHERE  e."Class" = 'Knoten'

UNION ALL

-- ── Teileinzugsgebiet (keine eigene funktion/eigentuemer/status) ─────────
SELECT
    e."Tid", e.source, e."Topic", e."Class", e."ErrorId",
    COALESCE(e.cmsg_de, e."Description"),
    e."UserAttributes",
    NULL  AS funktionhierarchisch,
    NULL  AS eigentuemer,
    NULL  AS status,
    e."Category", e."Model", e."Module",
    CAST(e.prio_uc AS INTEGER), CAST(e.prio_gsp AS INTEGER),
    e.required_action_de, e.action_context_de
FROM   v_checker_errors  e
LEFT JOIN teileinzugsgebiet t ON t.T_Ili_Tid = e."Tid"
WHERE  e."Class" = 'Teileinzugsgebiet'

UNION ALL
-- … analog für SK_Regenueberlauf, SK_Regenueberlaufbecken, SK_Einleitstelle,
--   Ueberlauf_Foerderaggregat (jede mit ihrem eigenen LEFT JOIN auf
--   sk_regenueberlauf / sk_regenueberlaufbecken / sk_einleitstelle /
--   ueberlauf_foerderaggregat und ggf. organisation)
;
```

### Begründung

- **Ein SELECT pro Klasse** macht die Joins typisiert und debugbar. Du siehst
  sofort, dass für `Teileinzugsgebiet` die Felder `funktionhierarchisch` /
  `eigentuemer` / `status` einfach NULL sind, weil's sie fachlich gar nicht
  gibt. Ein riesiges `LEFT JOIN ALLES` mit `CASE WHEN class = …` ginge auch,
  wird aber schnell unleserlich; SQLite hat keinen Optimizer, der das so gut
  faltet wie PostgreSQL.
- **`LEFT JOIN`** (nicht INNER) ist wichtig: wenn der CSV-Checker eine TID
  meldet, die in der VSA-Tabelle nicht (mehr) existiert, willst du den Befund
  trotzdem behalten — die Anreicherungs-Felder werden dann eben NULL.
- **`COALESCE(cmsg_de, Description)`** schützt gegen leere Matrix-Einträge.
  Leere Felder werden bereits beim Import durch `CsvImporter` und
  `ErrorMatrixImporter` auf `NULL` gemappt, deshalb funktioniert `COALESCE`
  und `IS NULL` im Build-View direkt — kein zusätzliches `NULLIF` nötig.

### Behandlung der Matrix-Duplikate

Wegen der 6 doppelten `(cid, model, class_de)` in `error_matrix` wird ein
Befund (im aktuellen Testdatensatz `fp_110`) zweimal materialisiert. Drei
pragmatische Optionen, in der Reihenfolge der Empfehlung:

1. **In der Quelle fixen** — die Fachstelle bereinigt die Excel-Datei. Saubere,
   dauerhafte Lösung.
2. **Beim Materialisieren entschärfen** mit einem `DISTINCT` auf der
   Build-View, oder sauberer in C# mit einer
   `INSERT … ON CONFLICT(tid, errorid, check_type) DO NOTHING`-Strategie.
   Voraussetzung: ein UNIQUE-Index auf `(tid, errorid, check_type)`. Das
   dokumentiert die fachliche Regel "ein Befund pro Tid+Regel+Source" gleich
   mit.
3. **In der View** mit einem `GROUP BY` über `error_matrix`. Hässlich, weil's
   das Symptom kuriert, nicht die Ursache.

## Stufe 2 — `ca_error_object` als reine Aggregation

Sobald `ca_error_data` befüllt ist, ist das trivial:

```sql
INSERT INTO ca_error_object (tid, class, count_error, wk_max, gep_max)
SELECT
    tid,
    class,
    COUNT(*)            AS count_error,
    MAX(wk)             AS wk_max,
    MAX(gep)            AS gep_max
FROM   ca_error_data
GROUP BY tid, class;
```

Das ist die Verdichtung — eine Zeile pro betroffenem Objekt, mit der Anzahl
Befunde und der jeweils höchsten Relevanz pro Use-Case.

## Stufe 3 — Klassen-Struktur in der C#-Applikation

Die bestehende Pipeline hat drei Klassen mit klar abgegrenzten
Verantwortlichkeiten:

| Klasse                 | Layer            | Output                                                            |
|------------------------|------------------|-------------------------------------------------------------------|
| `CsvImporter`          | Quelldaten       | `checker_csv_t` / `_a` / `_fp` (Tabellen)                         |
| `ErrorMatrixImporter`  | Quelldaten       | `error_matrix` (Tabelle)                                          |
| `ViewCreator`          | Transform        | `v_checker_csv_all`, `v_checker_errors`, `v_checker_orphans`      |

Der noch fehlende Materialisierungs-Schritt fügt sich als **vierte Klasse**
analog in dieses Muster ein:

| Klasse                 | Layer            | Output                                                            |
|------------------------|------------------|-------------------------------------------------------------------|
| `ErrorDataMaterializer` | Snapshot         | `v_ca_error_data_build` (View), `ca_error_data` & `ca_error_object` (Tabellen) |

So bleibt die Architektur symmetrisch: vier Klassen, vier klar abgegrenzte
Verantwortlichkeiten, jede mit eigenem Unit-Test plus einer Erweiterung des
bestehenden `GeopackageGenerationIntegrationTest`.

### Skelett für `ErrorDataMaterializer`

```csharp
namespace VsaCheckerAnalytics.Process.GeopackageGeneration;

/// <summary>
/// Materializes the final ca_error_data and ca_error_object tables from the
/// checker errors view by joining with the VSA feature classes.
/// </summary>
internal sealed class ErrorDataMaterializer
{
    private readonly SqliteConnection connection;
    private readonly ILogger logger;

    internal ErrorDataMaterializer(SqliteConnection connection, ILogger logger)
    {
        this.connection = connection;
        this.logger = logger;
    }

    /// <summary>
    /// Creates the v_ca_error_data_build view that maps v_checker_errors plus
    /// the VSA feature tables (knoten, leitung, …) onto the ca_error_data shape.
    /// </summary>
    internal void CreateBuildView(string viewName, string errorsViewName) { /* … */ }

    /// <summary>
    /// Populates ca_error_data and ca_error_object from the build view in a
    /// single transaction. Both target tables are truncated first.
    /// </summary>
    internal async Task MaterializeAsync(CancellationToken cancellationToken) { /* … */ }
}
```

Die `MaterializeAsync`-Methode entspricht etwa:

```csharp
await using var tx = await connection.BeginTransactionAsync(cancellationToken);

await ExecuteAsync("DELETE FROM ca_error_object", tx, cancellationToken);
await ExecuteAsync("DELETE FROM ca_error_data",   tx, cancellationToken);

await ExecuteAsync(@"
    INSERT INTO ca_error_data (
        tid, check_type, topic, class, errorid, error, detail,
        funktionhierarchisch, eigentuemer, status, category, model, module,
        wk, gep, recommendation, recommendation_detail)
    SELECT
        tid, check_type, topic, class, errorid, error, detail,
        funktionhierarchisch, eigentuemer, status, category, model, module,
        wk, gep, recommendation, recommendation_detail
    FROM v_ca_error_data_build", tx, cancellationToken);

await ExecuteAsync(@"
    INSERT INTO ca_error_object (tid, class, count_error, wk_max, gep_max)
    SELECT tid, class, COUNT(*), MAX(wk), MAX(gep)
    FROM ca_error_data
    GROUP BY tid, class", tx, cancellationToken);

await tx.CommitAsync(cancellationToken);
```

Beides läuft in einer einzigen Transaktion, atomar. Wenn die Build-View
intern Joins macht, die fehlschlagen (z. B. neue Klasse ohne Mapping), siehst
du das beim Build sofort.

### Test-Strategie

Analog zu den bestehenden Tests:

1. **Unit-Tests `ErrorDataMaterializerTest`** mit `:memory:`-SQLite, einer
   minimalen Seed-Datenmenge (eine Leitung, ein Knoten, ein Teileinzugsgebiet
   mit je 1–2 Befunden), und Assertions über die Zeilenzahlen und konkrete
   Werte in `ca_error_data` / `ca_error_object`. Empfohlene Test-Methoden:
   - `Materialize_PopulatesCaErrorData_WithEnrichedAttributes`
   - `Materialize_AggregatesCorrectly_IntoCaErrorObject`
   - `Materialize_HandlesUnknownClasses_AsNullEnrichment` (Catch-All-Fall)
   - `Materialize_IsIdempotent_OnReRun` (zweimal aufrufen, gleiches Ergebnis)
   - `Materialize_RollsBack_OnError` (mit einer kaputten Build-View prüfen,
     dass die Tabellen leer bleiben)

2. **Erweiterung des `GeopackageGenerationIntegrationTest`** um den
   Materialisierungs-Schritt nach den drei Views. Dort werden gegen die echten
   Test-Daten konkrete Zähler geprüft (z. B. `ca_error_object` enthält genau
   so viele Zeilen wie es eindeutige `(tid, class)`-Paare in `ca_error_data`
   gibt; die Summe der `count_error` entspricht `COUNT(*)` auf
   `ca_error_data`).

3. **Sprachpfade getrennt testen.** Der `ViewCreator` hat schon je einen Test
   für DE und FR — analog beim Materializer, falls der Sprach-Parameter durch
   die Pipeline durchgereicht wird (z. B. weil `cmsg_de` vs `cmsg_fr` aus der
   Matrix in `error` landet).

## Empfohlene Indizes

> Die Indizes auf `error_matrix` und den drei `checker_csv_*`-Tabellen werden
> bereits direkt im Code angelegt — `CsvImporter` und `ErrorMatrixImporter`
> bekommen die Index-Spalten als optionalen Parameter und legen den
> zusammengesetzten Index nach den Inserts an. Siehe
> `GeopackageGenerationIntegrationTest`. Hier stehen nur noch die Indizes,
> die der neue Materialisierungs-Schritt mitbringen muss.

```sql
CREATE INDEX IF NOT EXISTS ix_leitung_t_ili_tid
    ON leitung (T_Ili_Tid);
CREATE INDEX IF NOT EXISTS ix_knoten_t_ili_tid
    ON knoten  (T_Ili_Tid);
-- … analog für die anderen sk_*-Tabellen, teileinzugsgebiet,
--   ueberlauf_foerderaggregat

CREATE INDEX IF NOT EXISTS ix_ca_error_data_object_grouping
    ON ca_error_data (tid, class);

CREATE UNIQUE INDEX IF NOT EXISTS uq_ca_error_data_natural_key
    ON ca_error_data (tid, errorid, check_type);   -- siehe Duplikat-Diskussion
```

Der UNIQUE-Index ist gleichzeitig "Safety Net" gegen die Matrix-Duplikate —
fällt etwas durch, das laut Fachregel doppelt wäre, gibt's eine sprechende
Constraint-Violation statt eines stillen Doppel-Eintrags.

## Offene fachliche Fragen

1. **Semantik von `check_type`.** Im DSSMini-Vorbild stand dort `'vsa-fp'`
   als Quelltyp aus dem Checker (suggeriert "Modelltyp/Profil"). In den CSVs
   gibt es drei Source-Files T/A/FP. Wenn T/A/FP fachlich genau dem
   entspricht, perfekt — sonst Mapping nötig.
2. **`error_type`-Spalte fehlt.** Im Vorbild gab es `error_type` (Allgemein,
   GEP, Zustand, …). `ca_error_data` hat das nicht. Falls die Visualisierung
   später danach gruppiert (im DSSMini-File gab's `v_error_category_*`-Views),
   muss das ergänzt werden.
3. **Catch-All für unbekannte Klassen.** `Leitung_Text`, `Massnahme` etc. sind
   aktuell nicht abgedeckt. Eine kleine `WHERE Class NOT IN (...)`-Klausel am
   Ende der UNION mit NULL-Anreicherung wäre eine Versicherung, die nichts
   kostet.
