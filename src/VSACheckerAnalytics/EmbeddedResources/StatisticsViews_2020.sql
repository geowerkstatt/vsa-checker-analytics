-- ============================================================
-- Statistik-Views: Objektanzahlen und NULL-Auswertung je Attribut
-- Tabellen (in dieser Reihenfolge): alr, knoten, leitung, massnahme, organisation, rohrprofil, rohrprofil_geometrie, teileinzugsgebiet, ueberlauf_foerderaggregat, bauwerkskomponente, sk_autonome_messstelle, sk_duekeroberhaupt, sk_einleitstelle, sk_pumpwerk, sk_regenrueckhaltebecken_kanal, sk_regenueberlauf, sk_regenueberlaufbecken, sk_trennbauwerk, sk_uebrige
-- Attribute je Tabelle alphabetisch sortiert (Spalte 'sortierung' fixiert die Reihenfolge).
-- T_Id / T_basket ausgeschlossen, t_ili_tid als OID benannt.
-- anzahl_paa / anzahl_saa und anzahl_null_paa / anzahl_null_saa nur fuer
-- knoten und leitung befuellt (Feld funktionhierarchisch, inkl. hierarchischer
-- Werte wie 'PAA.Hauptsammelkanal'), sonst NULL.
-- Benoetigt SQLite >= 3.30 (FILTER-Klausel), in GeoPackages Standard.
--
-- Aufbau: pro Quelltabelle eine Teil-View (SQLite begrenzt Compound-SELECTs
-- auf ca. 500 Terme), zusammengefuehrt und sortiert in v_statistics_attribute.
-- Verwendung: SELECT * FROM v_statistics_attribute;
-- ============================================================

-- Teil-Views je Quelltabelle
DROP VIEW IF EXISTS v_statistics_alr;
CREATE VIEW v_statistics_alr AS
SELECT
    1 AS sortierung,
    'alr' AS tabelle,
    'beseitigung_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "beseitigung_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "alr"
UNION ALL
SELECT
    2 AS sortierung,
    'alr' AS tabelle,
    'beseitigung_ist_unbekannt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "beseitigung_ist" = 'unbekannt') AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "alr"
UNION ALL
SELECT
    3 AS sortierung,
    'alr' AS tabelle,
    'bezeichnung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bezeichnung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "alr"
UNION ALL
SELECT
    4 AS sortierung,
    'alr' AS tabelle,
    'datenherrref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenherrref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "alr"
UNION ALL
SELECT
    5 AS sortierung,
    'alr' AS tabelle,
    'datenlieferantref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenlieferantref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "alr"
UNION ALL
SELECT
    6 AS sortierung,
    'alr' AS tabelle,
    'einwohnerwerte' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "einwohnerwerte" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "alr"
UNION ALL
SELECT
    7 AS sortierung,
    'alr' AS tabelle,
    'lage' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "lage" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "alr"
UNION ALL
SELECT
    8 AS sortierung,
    'alr' AS tabelle,
    'letzte_aenderung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "letzte_aenderung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "alr"
UNION ALL
SELECT
    9 AS sortierung,
    'alr' AS tabelle,
    'massnahmeref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "massnahmeref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "alr"
UNION ALL
SELECT
    10 AS sortierung,
    'alr' AS tabelle,
    'obj_id_entsorgung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_entsorgung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "alr"
UNION ALL
SELECT
    11 AS sortierung,
    'alr' AS tabelle,
    'obj_id_entsorgung_abwasserbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_entsorgung_abwasserbauwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "alr"
UNION ALL
SELECT
    12 AS sortierung,
    'alr' AS tabelle,
    'obj_id_entsorgung_einleitstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_entsorgung_einleitstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "alr"
UNION ALL
SELECT
    13 AS sortierung,
    'alr' AS tabelle,
    'obj_id_entsorgung_versickerungsanlage' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_entsorgung_versickerungsanlage" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "alr"
UNION ALL
SELECT
    14 AS sortierung,
    'alr' AS tabelle,
    'obj_id_gebaeudegruppe_entsorgung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_gebaeudegruppe_entsorgung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "alr"
UNION ALL
SELECT
    15 AS sortierung,
    'alr' AS tabelle,
    'OID' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "T_Ili_Tid" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "alr"
UNION ALL
SELECT
    16 AS sortierung,
    'alr' AS tabelle,
    'sanierungsbedarf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sanierungsbedarf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "alr"
UNION ALL
SELECT
    17 AS sortierung,
    'alr' AS tabelle,
    'sanierungsbedarf_unbekannt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sanierungsbedarf" = 'unbekannt') AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "alr"
UNION ALL
SELECT
    18 AS sortierung,
    'alr' AS tabelle,
    'sanierungsdatum' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sanierungsdatum" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "alr"
UNION ALL
SELECT
    19 AS sortierung,
    'alr' AS tabelle,
    'sanierungskonzept' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sanierungskonzept" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "alr"
;

DROP VIEW IF EXISTS v_statistics_knoten;
CREATE VIEW v_statistics_knoten AS
SELECT
    20 AS sortierung,
    'knoten' AS tabelle,
    'ara_nr' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "ara_nr" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "ara_nr" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "ara_nr" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    21 AS sortierung,
    'knoten' AS tabelle,
    'astatus' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "astatus" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "astatus" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "astatus" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    22 AS sortierung,
    'knoten' AS tabelle,
    'astatus_unbekannt' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "astatus" = 'unbekannt') AS anzahl_null,
    COUNT(*) FILTER (WHERE "astatus" = 'unbekannt' AND  funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "astatus" = 'unbekannt' AND  funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    23 AS sortierung,
    'knoten' AS tabelle,
    'baujahr' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "baujahr" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "baujahr" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "baujahr" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    24 AS sortierung,
    'knoten' AS tabelle,
    'baulicherzustand' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "baulicherzustand" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "baulicherzustand" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "baulicherzustand" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    25 AS sortierung,
    'knoten' AS tabelle,
    'baulicherzustand_unbekannt' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "baulicherzustand" = 'unbekannt') AS anzahl_null,
    COUNT(*) FILTER (WHERE "baulicherzustand" = 'unbekannt' AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "baulicherzustand" = 'unbekannt' AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    26 AS sortierung,
    'knoten' AS tabelle,
    'bemerkung' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bemerkung" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "bemerkung" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "bemerkung" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    27 AS sortierung,
    'knoten' AS tabelle,
    'betreiberref' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "betreiberref" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "betreiberref" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "betreiberref" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    28 AS sortierung,
    'knoten' AS tabelle,
    'bezeichnung' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bezeichnung" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "bezeichnung" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "bezeichnung" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    29 AS sortierung,
    'knoten' AS tabelle,
    'bezeichnung_unique' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bezeichnung" IS NOT NULL AND Bezeichnung IN (SELECT bezeichnung FROM knoten GROUP BY Bezeichnung HAVING count(*) > 1)) AS anzahl_null ,
    COUNT(*) FILTER (WHERE "bezeichnung" IS NOT NULL AND funktionhierarchisch LIKE 'PAA%' AND Bezeichnung IN (SELECT bezeichnung FROM knoten GROUP BY Bezeichnung HAVING count(*) > 1)) AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "bezeichnung" IS NOT NULL AND funktionhierarchisch LIKE 'SAA%' AND Bezeichnung IN (SELECT bezeichnung FROM knoten GROUP BY Bezeichnung HAVING count(*) > 1)) AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    30 AS sortierung,
    'knoten' AS tabelle,
    'datenherrref' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenherrref" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "datenherrref" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "datenherrref" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    31 AS sortierung,
    'knoten' AS tabelle,
    'datenlieferantref' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenlieferantref" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "datenlieferantref" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "datenlieferantref" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    32 AS sortierung,
    'knoten' AS tabelle,
    'deckelkote' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "deckelkote" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "deckelkote" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "deckelkote" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    33 AS sortierung,
    'knoten' AS tabelle,
    'detailgeometrie' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "detailgeometrie" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "detailgeometrie" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "detailgeometrie" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    34 AS sortierung,
    'knoten' AS tabelle,
    'dimension1' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "dimension1" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "dimension1" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "dimension1" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    35 AS sortierung,
    'knoten' AS tabelle,
    'dimension2' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "dimension2" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "dimension2" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "dimension2" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    36 AS sortierung,
    'knoten' AS tabelle,
    'dringlichkeitszahl' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    NULL AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    37 AS sortierung,
    'knoten' AS tabelle,
    'eigentuemerref' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "eigentuemerref" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "eigentuemerref" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "eigentuemerref" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    38 AS sortierung,
    'knoten' AS tabelle,
    'finanzierung' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "finanzierung" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "finanzierung" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "finanzierung" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    39 AS sortierung,
    'knoten' AS tabelle,
    'finanzierung_unbekannt' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "finanzierung" = 'unbekannt') AS anzahl_null,
    COUNT(*) FILTER (WHERE "finanzierung" = 'unbekannt' AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "finanzierung" = 'unbekannt' AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    40 AS sortierung,
    'knoten' AS tabelle,
    'funktion' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "funktion" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "funktion" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "funktion" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    41 AS sortierung,
    'knoten' AS tabelle,
    'funktion_unbek' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "funktion" = 'unbekannt') AS anzahl_null,
    COUNT(*) FILTER (WHERE "funktion" = 'unbekannt' AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "funktion" = 'unbekannt' AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    42 AS sortierung,
    'knoten' AS tabelle,
    'funktionhierarchisch' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "funktionhierarchisch" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "funktionhierarchisch" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "funktionhierarchisch" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    43 AS sortierung,
    'knoten' AS tabelle,
    'lage' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "lage" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "lage" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "lage" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM (SELECT k.*, kl.lage FROM "knoten" k LEFT JOIN "knoten_lage" kl ON kl."T_Id" = k."T_Id")
UNION ALL
SELECT
    44 AS sortierung,
    'knoten' AS tabelle,
    'lagegenauigkeit' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "lagegenauigkeit" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "lagegenauigkeit" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "lagegenauigkeit" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    45 AS sortierung,
    'knoten' AS tabelle,
    'letzte_aenderung' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "letzte_aenderung" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "letzte_aenderung" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "letzte_aenderung" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    46 AS sortierung,
    'knoten' AS tabelle,
    'nutzungsart_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "nutzungsart_geplant" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "nutzungsart_geplant" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "nutzungsart_geplant" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    47 AS sortierung,
    'knoten' AS tabelle,
    'nutzungsart_geplant_unbekannt' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "nutzungsart_geplant" = 'unbekannt') AS anzahl_null,
    COUNT(*) FILTER (WHERE "nutzungsart_geplant" = 'unbekannt' AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "nutzungsart_geplant" = 'unbekannt' AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    48 AS sortierung,
    'knoten' AS tabelle,
    'nutzungsart_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "nutzungsart_ist" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "nutzungsart_ist" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "nutzungsart_ist" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    49 AS sortierung,
    'knoten' AS tabelle,
    'nutzungsart_ist_unbekannt' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "nutzungsart_ist"  = 'unbekannt') AS anzahl_null,
    COUNT(*) FILTER (WHERE "nutzungsart_ist"  = 'unbekannt' AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "nutzungsart_ist"  = 'unbekannt' AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    50 AS sortierung,
    'knoten' AS tabelle,
    'obj_id_abwasserbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_abwasserbauwerk" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "obj_id_abwasserbauwerk" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "obj_id_abwasserbauwerk" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    51 AS sortierung,
    'knoten' AS tabelle,
    'obj_id_deckel' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_deckel" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "obj_id_deckel" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "obj_id_deckel" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    52 AS sortierung,
    'knoten' AS tabelle,
    'OID' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "T_Ili_Tid" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "T_Ili_Tid" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "T_Ili_Tid" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    53 AS sortierung,
    'knoten' AS tabelle,
    'rueckstaukote_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "rueckstaukote_ist" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "rueckstaukote_ist" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "rueckstaukote_ist" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    54 AS sortierung,
    'knoten' AS tabelle,
    'sanierungsbedarf' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sanierungsbedarf" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "sanierungsbedarf" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "sanierungsbedarf" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    55 AS sortierung,
    'knoten' AS tabelle,
    'sohlenkote' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sohlenkote" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "sohlenkote" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "sohlenkote" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    56 AS sortierung,
    'knoten' AS tabelle,
    'symbolori' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "symbolori" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "symbolori" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "symbolori" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    57 AS sortierung,
    'knoten' AS tabelle,
    'zugaenglichkeit' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "zugaenglichkeit" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "zugaenglichkeit" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "zugaenglichkeit" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    58 AS sortierung,
    'knoten' AS tabelle,
    'zustandserhebung_jahr' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "zustandserhebung_jahr" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "zustandserhebung_jahr" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "zustandserhebung_jahr" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    59 AS sortierung,
    'knoten' AS tabelle,
    'zustandserhebung_jahr_unbekannt' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "zustandserhebung_jahr"  = 'unbekannt') AS anzahl_null,
    COUNT(*) FILTER (WHERE "zustandserhebung_jahr"  = 'unbekannt' AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "zustandserhebung_jahr"  = 'unbekannt' AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    60 AS sortierung,
    'knoten' AS tabelle,
    'zustandsnote' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    NULL AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "knoten"
UNION ALL
SELECT
    61 AS sortierung,
    'knoten' AS tabelle,
    'zustandsnote_unbekannt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    NULL AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "knoten"
;

DROP VIEW IF EXISTS v_statistics_leitung;
CREATE VIEW v_statistics_leitung AS
SELECT
    62 AS sortierung,
    'leitung' AS tabelle,
    'astatus' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "astatus" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "astatus" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "astatus" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    63 AS sortierung,
    'leitung' AS tabelle,
    'astatus_unbekannt' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "astatus" = 'unbekannt') AS anzahl_null,
    COUNT(*) FILTER (WHERE "astatus" = 'unbekannt' AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "astatus" = 'unbekannt' AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    64 AS sortierung,
    'leitung' AS tabelle,
    'baujahr' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "baujahr" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "baujahr" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "baujahr" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    65 AS sortierung,
    'leitung' AS tabelle,
    'baujahr_1800' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "baujahr" = 1800) AS anzahl_null,
    COUNT(*) FILTER (WHERE "baujahr" = 1800 AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "baujahr" = 1800 AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    66 AS sortierung,
    'leitung' AS tabelle,
    'baulicherzustand' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "baulicherzustand" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "baulicherzustand" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "baulicherzustand" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    67 AS sortierung,
    'leitung' AS tabelle,
    'baulicherzustand_unbekannt' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "baulicherzustand" = 'unbekannt') AS anzahl_null,
    COUNT(*) FILTER (WHERE "baulicherzustand" = 'unbekannt' AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "baulicherzustand"  = 'unbekannt' AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    68 AS sortierung,
    'leitung' AS tabelle,
    'bemerkung' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bemerkung" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "bemerkung" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "bemerkung" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    69 AS sortierung,
    'leitung' AS tabelle,
    'betreiberref' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "betreiberref" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "betreiberref" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "betreiberref" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    70 AS sortierung,
    'leitung' AS tabelle,
    'bezeichnung' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bezeichnung" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "bezeichnung" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "bezeichnung" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    71 AS sortierung,
    'leitung' AS tabelle,
    'bezeichnung_unique' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bezeichnung" IS NOT NULL AND Bezeichnung IN (SELECT bezeichnung FROM leitung GROUP BY Bezeichnung HAVING count(*) > 1)) AS anzahl_null ,
    COUNT(*) FILTER (WHERE "bezeichnung" IS NOT NULL AND funktionhierarchisch LIKE 'PAA%' AND Bezeichnung IN (SELECT bezeichnung FROM leitung GROUP BY Bezeichnung HAVING count(*) > 1)) AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "bezeichnung" IS NOT NULL AND funktionhierarchisch LIKE 'SAA%' AND Bezeichnung IN (SELECT bezeichnung FROM leitung GROUP BY Bezeichnung HAVING count(*) > 1)) AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    72 AS sortierung,
    'leitung' AS tabelle,
    'datenherrref' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenherrref" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "datenherrref" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "datenherrref" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    73 AS sortierung,
    'leitung' AS tabelle,
    'datenlieferantref' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenlieferantref" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "datenlieferantref" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "datenlieferantref" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    74 AS sortierung,
    'leitung' AS tabelle,
    'dringlichkeitszahl' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    NULL AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    75 AS sortierung,
    'leitung' AS tabelle,
    'eigentuemerref' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "eigentuemerref" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "eigentuemerref" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "eigentuemerref" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    76 AS sortierung,
    'leitung' AS tabelle,
    'finanzierung' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "finanzierung" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "finanzierung" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "finanzierung" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    77 AS sortierung,
    'leitung' AS tabelle,
    'finanzierung_unbekannt' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "finanzierung"  = 'unbekannt') AS anzahl_null,
    COUNT(*) FILTER (WHERE "finanzierung"  = 'unbekannt' AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "finanzierung"  = 'unbekannt' AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    78 AS sortierung,
    'leitung' AS tabelle,
    'funktionhierarchisch' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "funktionhierarchisch" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "funktionhierarchisch" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "funktionhierarchisch" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    79 AS sortierung,
    'leitung' AS tabelle,
    'funktionhierarchisch_unbekannt' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "funktionhierarchisch"  = 'unbekannt') AS anzahl_null,
    COUNT(*) FILTER (WHERE "funktionhierarchisch"  = 'unbekannt' AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "funktionhierarchisch"  = 'unbekannt' AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    80 AS sortierung,
    'leitung' AS tabelle,
    'funktionhydraulisch' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "funktionhydraulisch" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "funktionhydraulisch" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "funktionhydraulisch" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    81 AS sortierung,
    'leitung' AS tabelle,
    'funktionhydraulisch_unbekannt' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "funktionhydraulisch"  = 'unbekannt') AS anzahl_null,
    COUNT(*) FILTER (WHERE "funktionhydraulisch"  = 'unbekannt' AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "funktionhydraulisch"  = 'unbekannt' AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    82 AS sortierung,
    'leitung' AS tabelle,
    'hoehengenauigkeit_nach' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hoehengenauigkeit_nach" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "hoehengenauigkeit_nach" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "hoehengenauigkeit_nach" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    83 AS sortierung,
    'leitung' AS tabelle,
    'hoehengenauigkeit_von' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hoehengenauigkeit_von" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "hoehengenauigkeit_von" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "hoehengenauigkeit_von" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    84 AS sortierung,
    'leitung' AS tabelle,
    'hydr_belastung_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hydr_belastung_ist" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "hydr_belastung_ist" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "hydr_belastung_ist" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    85 AS sortierung,
    'leitung' AS tabelle,
    'knoten_nachref' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "knoten_nachref" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "knoten_nachref" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "knoten_nachref" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    86 AS sortierung,
    'leitung' AS tabelle,
    'knoten_vonref' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "knoten_vonref" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "knoten_vonref" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "knoten_vonref" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    87 AS sortierung,
    'leitung' AS tabelle,
    'kote_nach' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "kote_nach" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "kote_nach" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "kote_nach" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    88 AS sortierung,
    'leitung' AS tabelle,
    'kote_von' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "kote_von" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "kote_von" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "kote_von" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    89 AS sortierung,
    'leitung' AS tabelle,
    'laengeeffektiv' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "laengeeffektiv" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "laengeeffektiv" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "laengeeffektiv" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    90 AS sortierung,
    'leitung' AS tabelle,
    'lagebestimmung' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "lagebestimmung" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "lagebestimmung" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "lagebestimmung" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    91 AS sortierung,
    'leitung' AS tabelle,
    'leckschutz' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "leckschutz" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "leckschutz" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "leckschutz" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    92 AS sortierung,
    'leitung' AS tabelle,
    'leckschutz_unbekannt' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "leckschutz"  = 'unbekannt') AS anzahl_null,
    COUNT(*) FILTER (WHERE "leckschutz"  = 'unbekannt' AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "leckschutz"  = 'unbekannt' AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    93 AS sortierung,
    'leitung' AS tabelle,
    'leitung_nachref' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "leitung_nachref" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "leitung_nachref" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "leitung_nachref" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    94 AS sortierung,
    'leitung' AS tabelle,
    'letzte_aenderung' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "letzte_aenderung" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "letzte_aenderung" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "letzte_aenderung" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    95 AS sortierung,
    'leitung' AS tabelle,
    'lichte_breite' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "lichte_breite" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "lichte_breite" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "lichte_breite" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    96 AS sortierung,
    'leitung' AS tabelle,
    'lichte_hoehe' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "lichte_hoehe" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "lichte_hoehe" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "lichte_hoehe" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    97 AS sortierung,
    'leitung' AS tabelle,
    'material' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "material" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "material" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "material" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    98 AS sortierung,
    'leitung' AS tabelle,
    'material_unbekannt' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "material" = 'unbekannt') AS anzahl_null,
    COUNT(*) FILTER (WHERE "material"  = 'unbekannt' AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "material"  = 'unbekannt' AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    99 AS sortierung,
    'leitung' AS tabelle,
    'nutzungsart_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "nutzungsart_geplant" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "nutzungsart_geplant" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "nutzungsart_geplant" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    100 AS sortierung,
    'leitung' AS tabelle,
    'nutzungsart_geplant_unbekannt' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "nutzungsart_geplant"  = 'unbekannt') AS anzahl_null,
    COUNT(*) FILTER (WHERE "nutzungsart_geplant"  = 'unbekannt' AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "nutzungsart_geplant"  = 'unbekannt' AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    101 AS sortierung,
    'leitung' AS tabelle,
    'nutzungsart_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "nutzungsart_ist" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "nutzungsart_ist" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "nutzungsart_ist" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    102 AS sortierung,
    'leitung' AS tabelle,
    'nutzungsart_ist_unbekannt' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "nutzungsart_ist"  = 'unbekannt') AS anzahl_null,
    COUNT(*) FILTER (WHERE "nutzungsart_ist"  = 'unbekannt' AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "nutzungsart_ist"  = 'unbekannt' AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    103 AS sortierung,
    'leitung' AS tabelle,
    'obj_id_abwasserbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_abwasserbauwerk" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "obj_id_abwasserbauwerk" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "obj_id_abwasserbauwerk" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    104 AS sortierung,
    'leitung' AS tabelle,
    'obj_id_nachhaltungspunkt' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_nachhaltungspunkt" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "obj_id_nachhaltungspunkt" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "obj_id_nachhaltungspunkt" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    105 AS sortierung,
    'leitung' AS tabelle,
    'obj_id_vonhaltungspunkt' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_vonhaltungspunkt" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "obj_id_vonhaltungspunkt" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "obj_id_vonhaltungspunkt" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    106 AS sortierung,
    'leitung' AS tabelle,
    'OID' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "T_Ili_Tid" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "T_Ili_Tid" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "T_Ili_Tid" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    107 AS sortierung,
    'leitung' AS tabelle,
    'profiltyp' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "profiltyp" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "profiltyp" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "profiltyp" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    108 AS sortierung,
    'leitung' AS tabelle,
    'profiltyp_unbekannt' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "profiltyp" = 'unbekannt') AS anzahl_null,
    COUNT(*) FILTER (WHERE "profiltyp" = 'unbekannt' AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "profiltyp" = 'unbekannt' AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    109 AS sortierung,
    'leitung' AS tabelle,
    'reliner_art' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "reliner_art" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "reliner_art" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "reliner_art" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    110 AS sortierung,
    'leitung' AS tabelle,
    'reliner_nennweite' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "reliner_nennweite" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "reliner_nennweite" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "reliner_nennweite" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    111 AS sortierung,
    'leitung' AS tabelle,
    'rohrprofilref' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "rohrprofilref" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "rohrprofilref" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "rohrprofilref" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    112 AS sortierung,
    'leitung' AS tabelle,
    'sanierungsbedarf' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sanierungsbedarf" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "sanierungsbedarf" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "sanierungsbedarf" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    113 AS sortierung,
    'leitung' AS tabelle,
    'verlauf' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "verlauf" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "verlauf" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "verlauf" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    114 AS sortierung,
    'leitung' AS tabelle,
    'wandrauhigkeit' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "wandrauhigkeit" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "wandrauhigkeit" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "wandrauhigkeit" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    115 AS sortierung,
    'leitung' AS tabelle,
    'wbw_basisjahr' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "wbw_basisjahr" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "wbw_basisjahr" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "wbw_basisjahr" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    116 AS sortierung,
    'leitung' AS tabelle,
    'wbw_bauart' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "wbw_bauart" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "wbw_bauart" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "wbw_bauart" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    117 AS sortierung,
    'leitung' AS tabelle,
    'wiederbeschaffungswert' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "wiederbeschaffungswert" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "wiederbeschaffungswert" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "wiederbeschaffungswert" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    118 AS sortierung,
    'leitung' AS tabelle,
    'zustandserhebung_jahr' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "zustandserhebung_jahr" IS NULL) AS anzahl_null,
    COUNT(*) FILTER (WHERE "zustandserhebung_jahr" IS NULL AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "zustandserhebung_jahr" IS NULL AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    119 AS sortierung,
    'leitung' AS tabelle,
    'zustandserhebung_jahr_1800' AS attribut,
    COUNT(*) AS anzahl_total,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'PAA%') AS anzahl_paa,
    COUNT(*) FILTER (WHERE funktionhierarchisch LIKE 'SAA%') AS anzahl_saa,
    COUNT(*) FILTER (WHERE "zustandserhebung_jahr" = 1800) AS anzahl_null,
    COUNT(*) FILTER (WHERE "zustandserhebung_jahr" = 1800 AND funktionhierarchisch LIKE 'PAA%') AS anzahl_null_paa,
    COUNT(*) FILTER (WHERE "zustandserhebung_jahr" = 1800 AND funktionhierarchisch LIKE 'SAA%') AS anzahl_null_saa
FROM "leitung"
UNION ALL
SELECT
    120 AS sortierung,
    'leitung' AS tabelle,
    'zustandsnote' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    NULL AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "leitung"
;

DROP VIEW IF EXISTS v_statistics_massnahme;
CREATE VIEW v_statistics_massnahme AS
SELECT
    121 AS sortierung,
    'massnahme' AS tabelle,
    'astatus' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "astatus" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "massnahme"
UNION ALL
SELECT
    122 AS sortierung,
    'massnahme' AS tabelle,
    'astatus_unbekannt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "astatus" = 'unbekannt') AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "massnahme"
UNION ALL
SELECT
    123 AS sortierung,
    'massnahme' AS tabelle,
    'bemerkung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bemerkung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "massnahme"
UNION ALL
SELECT
    124 AS sortierung,
    'massnahme' AS tabelle,
    'beschreibung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "beschreibung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "massnahme"
UNION ALL
SELECT
    125 AS sortierung,
    'massnahme' AS tabelle,
    'bezeichnung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bezeichnung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "massnahme"
UNION ALL
SELECT
    126 AS sortierung,
    'massnahme' AS tabelle,
    'datenherrref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenherrref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "massnahme"
UNION ALL
SELECT
    127 AS sortierung,
    'massnahme' AS tabelle,
    'datenlieferantref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenlieferantref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "massnahme"
UNION ALL
SELECT
    128 AS sortierung,
    'massnahme' AS tabelle,
    'datum_eingang' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datum_eingang" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "massnahme"
UNION ALL
SELECT
    129 AS sortierung,
    'massnahme' AS tabelle,
    'gesamtkosten' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "gesamtkosten" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "massnahme"
UNION ALL
SELECT
    130 AS sortierung,
    'massnahme' AS tabelle,
    'handlungsbedarf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "handlungsbedarf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "massnahme"
UNION ALL
SELECT
    131 AS sortierung,
    'massnahme' AS tabelle,
    'jahr_umsetzung_effektiv' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "jahr_umsetzung_effektiv" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "massnahme"
UNION ALL
SELECT
    132 AS sortierung,
    'massnahme' AS tabelle,
    'jahr_umsetzung_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "jahr_umsetzung_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "massnahme"
UNION ALL
SELECT
    133 AS sortierung,
    'massnahme' AS tabelle,
    'kategorie' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "kategorie" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "massnahme"
UNION ALL
SELECT
    134 AS sortierung,
    'massnahme' AS tabelle,
    'kategorie' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "kategorie" = 'unbekannt') AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "massnahme"
UNION ALL
SELECT
    135 AS sortierung,
    'massnahme' AS tabelle,
    'letzte_aenderung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "letzte_aenderung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "massnahme"
UNION ALL
SELECT
    136 AS sortierung,
    'massnahme' AS tabelle,
    'linie' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "linie" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "massnahme"
UNION ALL
SELECT
    137 AS sortierung,
    'massnahme' AS tabelle,
    'obj_id_erhaltungsereignis_abwasserbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_erhaltungsereignis_abwasserbauwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "massnahme"
UNION ALL
SELECT
    138 AS sortierung,
    'massnahme' AS tabelle,
    'OID' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "T_Ili_Tid" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "massnahme"
UNION ALL
SELECT
    139 AS sortierung,
    'massnahme' AS tabelle,
    'prioritaet' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "prioritaet" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "massnahme"
UNION ALL
SELECT
    140 AS sortierung,
    'massnahme' AS tabelle,
    'traegerschaftref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "traegerschaftref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "massnahme"
UNION ALL
SELECT
    141 AS sortierung,
    'massnahme' AS tabelle,
    'verantwortlich_ausloesungref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "verantwortlich_ausloesungref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "massnahme"
UNION ALL
SELECT
    142 AS sortierung,
    'massnahme' AS tabelle,
    'verweis' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "verweis" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "massnahme"
;

DROP VIEW IF EXISTS v_statistics_organisation;
CREATE VIEW v_statistics_organisation AS
SELECT
    143 AS sortierung,
    'organisation' AS tabelle,
    'astatus' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "astatus" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "organisation"
UNION ALL
SELECT
    144 AS sortierung,
    'organisation' AS tabelle,
    'auid' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "auid" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "organisation"
UNION ALL
SELECT
    145 AS sortierung,
    'organisation' AS tabelle,
    'bemerkung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bemerkung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "organisation"
UNION ALL
SELECT
    146 AS sortierung,
    'organisation' AS tabelle,
    'bezeichnung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bezeichnung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "organisation"
UNION ALL
SELECT
    147 AS sortierung,
    'organisation' AS tabelle,
    'gemeindenummer' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "gemeindenummer" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "organisation"
UNION ALL
SELECT
    148 AS sortierung,
    'organisation' AS tabelle,
    'kurzbezeichnung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "kurzbezeichnung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "organisation"
UNION ALL
SELECT
    149 AS sortierung,
    'organisation' AS tabelle,
    'letzte_aenderung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "letzte_aenderung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "organisation"
UNION ALL
SELECT
    150 AS sortierung,
    'organisation' AS tabelle,
    'OID' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "T_Ili_Tid" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "organisation"
UNION ALL
SELECT
    151 AS sortierung,
    'organisation' AS tabelle,
    'organisationstyp' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "organisationstyp" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "organisation"
;

DROP VIEW IF EXISTS v_statistics_rohrprofil;
CREATE VIEW v_statistics_rohrprofil AS
SELECT
    152 AS sortierung,
    'rohrprofil' AS tabelle,
    'bemerkung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bemerkung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "rohrprofil"
UNION ALL
SELECT
    153 AS sortierung,
    'rohrprofil' AS tabelle,
    'bezeichnung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bezeichnung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "rohrprofil"
UNION ALL
SELECT
    154 AS sortierung,
    'rohrprofil' AS tabelle,
    'datenherrref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenherrref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "rohrprofil"
UNION ALL
SELECT
    155 AS sortierung,
    'rohrprofil' AS tabelle,
    'datenlieferantref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenlieferantref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "rohrprofil"
UNION ALL
SELECT
    156 AS sortierung,
    'rohrprofil' AS tabelle,
    'letzte_aenderung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "letzte_aenderung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "rohrprofil"
UNION ALL
SELECT
    157 AS sortierung,
    'rohrprofil' AS tabelle,
    'OID' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "T_Ili_Tid" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "rohrprofil"
;

DROP VIEW IF EXISTS v_statistics_rohrprofil_geometrie;
CREATE VIEW v_statistics_rohrprofil_geometrie AS
SELECT
    158 AS sortierung,
    'rohrprofil_geometrie' AS tabelle,
    'datenherrref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenherrref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "rohrprofil_geometrie"
UNION ALL
SELECT
    159 AS sortierung,
    'rohrprofil_geometrie' AS tabelle,
    'datenlieferantref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenlieferantref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "rohrprofil_geometrie"
UNION ALL
SELECT
    160 AS sortierung,
    'rohrprofil_geometrie' AS tabelle,
    'letzte_aenderung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "letzte_aenderung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "rohrprofil_geometrie"
UNION ALL
SELECT
    161 AS sortierung,
    'rohrprofil_geometrie' AS tabelle,
    'OID' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "T_Ili_Tid" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "rohrprofil_geometrie"
UNION ALL
SELECT
    162 AS sortierung,
    'rohrprofil_geometrie' AS tabelle,
    'reihenfolge' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "reihenfolge" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "rohrprofil_geometrie"
UNION ALL
SELECT
    163 AS sortierung,
    'rohrprofil_geometrie' AS tabelle,
    'rohrprofilref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "rohrprofilref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "rohrprofil_geometrie"
UNION ALL
SELECT
    164 AS sortierung,
    'rohrprofil_geometrie' AS tabelle,
    'x' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "x" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "rohrprofil_geometrie"
UNION ALL
SELECT
    165 AS sortierung,
    'rohrprofil_geometrie' AS tabelle,
    'y' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "y" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "rohrprofil_geometrie"
;

DROP VIEW IF EXISTS v_statistics_teileinzugsgebiet;
CREATE VIEW v_statistics_teileinzugsgebiet AS
SELECT
    166 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'abflussbegrenzung_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "abflussbegrenzung_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    167 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'abflussbegrenzung_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "abflussbegrenzung_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    168 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'abflussbeiwert_rw_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "abflussbeiwert_rw_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    169 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'abflussbeiwert_rw_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "abflussbeiwert_rw_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    170 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'abflussbeiwert_sw_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "abflussbeiwert_sw_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    171 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'abflussbeiwert_sw_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "abflussbeiwert_sw_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    172 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'befestigungsgrad_rw_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "befestigungsgrad_rw_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    173 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'befestigungsgrad_rw_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "befestigungsgrad_rw_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    174 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'befestigungsgrad_sw_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "befestigungsgrad_sw_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    175 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'befestigungsgrad_sw_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "befestigungsgrad_sw_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    176 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'bemerkung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bemerkung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    177 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'bezeichnung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bezeichnung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    178 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'datenherrref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenherrref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    179 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'datenlieferantref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenlieferantref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    180 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'direkteinleitung_in_gewaesser_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "direkteinleitung_in_gewaesser_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    181 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'direkteinleitung_in_gewaesser_geplant_unbekannt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "direkteinleitung_in_gewaesser_geplant" = 'unbekannt') AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    182 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'direkteinleitung_in_gewaesser_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "direkteinleitung_in_gewaesser_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    183 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'einwohnerdichte_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "einwohnerdichte_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    184 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'einwohnerdichte_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "einwohnerdichte_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    185 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'entwaesserungssystem_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "entwaesserungssystem_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    186 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'entwaesserungssystem_geplant_unbekannt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "entwaesserungssystem_geplant" = 'unbekannt') AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    187 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'entwaesserungssystem_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "entwaesserungssystem_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    188 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'flaeche' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    189 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'fremdwasseranfall_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "fremdwasseranfall_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    190 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'fremdwasseranfall_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "fremdwasseranfall_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    191 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'knoten_rw_geplantref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "knoten_rw_geplantref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    192 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'knoten_rw_istref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "knoten_rw_istref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    193 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'knoten_sw_geplantref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "knoten_sw_geplantref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    194 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'knoten_sw_istref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "knoten_sw_istref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    195 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'letzte_aenderung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "letzte_aenderung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    196 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'OID' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "T_Ili_Tid" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    197 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'perimeter' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "perimeter" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    198 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'retention_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "retention_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    199 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'retention_geplant_unbekannt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "retention_geplant" = 'unbekannt') AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    200 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'retention_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "retention_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    201 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_rw_geplantref_sk_autonome_messstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_rw_geplantref_sk_autonome_messstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    202 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_rw_geplantref_sk_duekeroberhaupt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_rw_geplantref_sk_duekeroberhaupt" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    203 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_rw_geplantref_sk_einleitstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_rw_geplantref_sk_einleitstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    204 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_rw_geplantref_sk_pumpwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_rw_geplantref_sk_pumpwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    205 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_rw_geplantref_sk_regenrueckhaltebecken_kanal' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_rw_geplantref_sk_regenrueckhaltebecken_kanal" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    206 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_rw_geplantref_sk_regenueberlauf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_rw_geplantref_sk_regenueberlauf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    207 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_rw_geplantref_sk_regenueberlaufbecken' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_rw_geplantref_sk_regenueberlaufbecken" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    208 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_rw_geplantref_sk_trennbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_rw_geplantref_sk_trennbauwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    209 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_rw_geplantref_sk_uebrige' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_rw_geplantref_sk_uebrige" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    210 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_rw_istref_sk_autonome_messstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_rw_istref_sk_autonome_messstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    211 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_rw_istref_sk_duekeroberhaupt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_rw_istref_sk_duekeroberhaupt" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    212 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_rw_istref_sk_einleitstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_rw_istref_sk_einleitstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    213 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_rw_istref_sk_pumpwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_rw_istref_sk_pumpwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    214 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_rw_istref_sk_regenrueckhaltebecken_kanal' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_rw_istref_sk_regenrueckhaltebecken_kanal" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    215 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_rw_istref_sk_regenueberlauf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_rw_istref_sk_regenueberlauf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    216 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_rw_istref_sk_regenueberlaufbecken' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_rw_istref_sk_regenueberlaufbecken" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    217 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_rw_istref_sk_trennbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_rw_istref_sk_trennbauwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    218 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_rw_istref_sk_uebrige' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_rw_istref_sk_uebrige" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    219 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_sw_geplantref_sk_autonome_messstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_sw_geplantref_sk_autonome_messstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    220 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_sw_geplantref_sk_duekeroberhaupt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_sw_geplantref_sk_duekeroberhaupt" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    221 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_sw_geplantref_sk_einleitstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_sw_geplantref_sk_einleitstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    222 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_sw_geplantref_sk_pumpwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_sw_geplantref_sk_pumpwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    223 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_sw_geplantref_sk_regenrueckhaltebecken_kanal' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_sw_geplantref_sk_regenrueckhaltebecken_kanal" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    224 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_sw_geplantref_sk_regenueberlauf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_sw_geplantref_sk_regenueberlauf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    225 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_sw_geplantref_sk_regenueberlaufbecken' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_sw_geplantref_sk_regenueberlaufbecken" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    226 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_sw_geplantref_sk_trennbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_sw_geplantref_sk_trennbauwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    227 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_sw_geplantref_sk_uebrige' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_sw_geplantref_sk_uebrige" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    228 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_sw_istref_sk_autonome_messstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_sw_istref_sk_autonome_messstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    229 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_sw_istref_sk_duekeroberhaupt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_sw_istref_sk_duekeroberhaupt" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    230 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_sw_istref_sk_einleitstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_sw_istref_sk_einleitstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    231 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_sw_istref_sk_pumpwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_sw_istref_sk_pumpwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    232 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_sw_istref_sk_regenrueckhaltebecken_kanal' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_sw_istref_sk_regenrueckhaltebecken_kanal" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    233 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_sw_istref_sk_regenueberlauf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_sw_istref_sk_regenueberlauf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    234 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_sw_istref_sk_regenueberlaufbecken' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_sw_istref_sk_regenueberlaufbecken" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    235 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_sw_istref_sk_trennbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_sw_istref_sk_trennbauwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    236 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'sbw_sw_istref_sk_uebrige' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sbw_sw_istref_sk_uebrige" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    237 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'schmutzabwasseranfall_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "schmutzabwasseranfall_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    238 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'schmutzabwasseranfall_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "schmutzabwasseranfall_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    239 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'versickerung_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "versickerung_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    240 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'versickerung_geplant_unbekannt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "versickerung_geplant" = 'unbekannt') AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
UNION ALL
SELECT
    241 AS sortierung,
    'teileinzugsgebiet' AS tabelle,
    'versickerung_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "versickerung_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "teileinzugsgebiet"
;

DROP VIEW IF EXISTS v_statistics_ueberlauf_foerderaggregat;
CREATE VIEW v_statistics_ueberlauf_foerderaggregat AS
SELECT
    242 AS sortierung,
    'ueberlauf_foerderaggregat' AS tabelle,
    'art' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "art" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "ueberlauf_foerderaggregat"
UNION ALL
SELECT
    243 AS sortierung,
    'ueberlauf_foerderaggregat' AS tabelle,
    'bezeichnung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bezeichnung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "ueberlauf_foerderaggregat"
UNION ALL
SELECT
    244 AS sortierung,
    'ueberlauf_foerderaggregat' AS tabelle,
    'datenherrref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenherrref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "ueberlauf_foerderaggregat"
UNION ALL
SELECT
    245 AS sortierung,
    'ueberlauf_foerderaggregat' AS tabelle,
    'datenlieferantref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenlieferantref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "ueberlauf_foerderaggregat"
UNION ALL
SELECT
    246 AS sortierung,
    'ueberlauf_foerderaggregat' AS tabelle,
    'knoten_nachref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "knoten_nachref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "ueberlauf_foerderaggregat"
UNION ALL
SELECT
    247 AS sortierung,
    'ueberlauf_foerderaggregat' AS tabelle,
    'knotenref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "knotenref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "ueberlauf_foerderaggregat"
UNION ALL
SELECT
    248 AS sortierung,
    'ueberlauf_foerderaggregat' AS tabelle,
    'letzte_aenderung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "letzte_aenderung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "ueberlauf_foerderaggregat"
UNION ALL
SELECT
    249 AS sortierung,
    'ueberlauf_foerderaggregat' AS tabelle,
    'OID' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "T_Ili_Tid" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "ueberlauf_foerderaggregat"
;

DROP VIEW IF EXISTS v_statistics_bauwerkskomponente;
CREATE VIEW v_statistics_bauwerkskomponente AS
SELECT
    250 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'art' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "art" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    251 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'beckenentleerung_art' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "beckenentleerung_art" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    252 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'beckenentleerung_leistung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "beckenentleerung_leistung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    253 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'beckenreinigung_art' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "beckenreinigung_art" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    254 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'bemerkung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bemerkung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    255 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'datenherrref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenherrref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    256 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'datenlieferantref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenlieferantref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    257 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'drosselorgan_art' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "drosselorgan_art" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    258 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'drosselorgan_oeffnung_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "drosselorgan_oeffnung_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    259 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'drosselorgan_oeffnung_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "drosselorgan_oeffnung_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    260 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'feststoffrueckhalt_anspringkote' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "feststoffrueckhalt_anspringkote" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    261 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'feststoffrueckhalt_art' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "feststoffrueckhalt_art" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    262 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'feststoffrueckhalt_dimensionierungswert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "feststoffrueckhalt_dimensionierungswert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    263 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'foerderaggregat_bauart' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "foerderaggregat_bauart" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    264 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'foerderaggregat_foerderstrommax_einzeln' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "foerderaggregat_foerderstrommax_einzeln" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    265 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'foerderaggregat_foerderstrommin_einzeln' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "foerderaggregat_foerderstrommin_einzeln" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    266 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'letzte_aenderung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "letzte_aenderung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    267 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'messgeraet_art' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "messgeraet_art" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    268 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'messgeraet_messart' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "messgeraet_messart" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    269 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'messgeraet_staukoerper' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "messgeraet_staukoerper" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    270 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'messgeraet_zweck' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "messgeraet_zweck" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    271 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'notentlastung_einleitstelleref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "notentlastung_einleitstelleref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    272 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'notentlastung_kote' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "notentlastung_kote" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    273 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'obj_id_absperr_drosselorgan' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_absperr_drosselorgan" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    274 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'obj_id_beckenentleerung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_beckenentleerung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    275 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'obj_id_beckenreinigung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_beckenreinigung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    276 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'obj_id_feststoffrueckhalt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_feststoffrueckhalt" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    277 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'obj_id_messgeraet' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_messgeraet" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    278 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'obj_id_messstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_messstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    279 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'obj_id_rueckstausicherung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_rueckstausicherung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    280 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'obj_id_ueberlauf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_ueberlauf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    281 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'OID' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "T_Ili_Tid" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    282 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'rueckstausicherung_art' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "rueckstausicherung_art" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    283 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'stammkarteref_sk_autonome_messstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "stammkarteref_sk_autonome_messstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    284 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'stammkarteref_sk_duekeroberhaupt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "stammkarteref_sk_duekeroberhaupt" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    285 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'stammkarteref_sk_einleitstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "stammkarteref_sk_einleitstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    286 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'stammkarteref_sk_pumpwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "stammkarteref_sk_pumpwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    287 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'stammkarteref_sk_regenrueckhaltebecken_kanal' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "stammkarteref_sk_regenrueckhaltebecken_kanal" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    288 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'stammkarteref_sk_regenueberlauf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "stammkarteref_sk_regenueberlauf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    289 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'stammkarteref_sk_regenueberlaufbecken' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "stammkarteref_sk_regenueberlaufbecken" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    290 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'stammkarteref_sk_trennbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "stammkarteref_sk_trennbauwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    291 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'stammkarteref_sk_uebrige' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "stammkarteref_sk_uebrige" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    292 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'ueberlauf_hydrueberfalllaenge' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "ueberlauf_hydrueberfalllaenge" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    293 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'ueberlauf_kotemax' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "ueberlauf_kotemax" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
UNION ALL
SELECT
    294 AS sortierung,
    'bauwerkskomponente' AS tabelle,
    'ueberlauf_kotemin' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "ueberlauf_kotemin" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "bauwerkskomponente"
;

DROP VIEW IF EXISTS v_statistics_sk_autonome_messstelle;
CREATE VIEW v_statistics_sk_autonome_messstelle AS
SELECT
    295 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'akten' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "akten" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    296 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'bemerkung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bemerkung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    297 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'bueroref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bueroref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    298 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'datenherrref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenherrref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    299 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'datenlieferantref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenlieferantref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    300 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'hauptbauwerkref_sk_autonome_messstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_autonome_messstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    301 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'hauptbauwerkref_sk_duekeroberhaupt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_duekeroberhaupt" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    302 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'hauptbauwerkref_sk_einleitstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_einleitstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    303 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'hauptbauwerkref_sk_pumpwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_pumpwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    304 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'hauptbauwerkref_sk_regenrueckhaltebecken_kanal' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_regenrueckhaltebecken_kanal" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    305 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'hauptbauwerkref_sk_regenueberlauf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_regenueberlauf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    306 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'hauptbauwerkref_sk_regenueberlaufbecken' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_regenueberlaufbecken" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    307 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'hauptbauwerkref_sk_trennbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_trennbauwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    308 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'hauptbauwerkref_sk_uebrige' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_uebrige" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    309 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'informationsquelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "informationsquelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    310 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'letzte_aenderung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "letzte_aenderung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    311 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'naechstes_sbwref_sk_autonome_messstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_autonome_messstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    312 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'naechstes_sbwref_sk_duekeroberhaupt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_duekeroberhaupt" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    313 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'naechstes_sbwref_sk_einleitstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_einleitstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    314 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'naechstes_sbwref_sk_pumpwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_pumpwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    315 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'naechstes_sbwref_sk_regenrueckhaltebecken_kanal' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_regenrueckhaltebecken_kanal" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    316 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'naechstes_sbwref_sk_regenueberlauf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_regenueberlauf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    317 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'naechstes_sbwref_sk_regenueberlaufbecken' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_regenueberlaufbecken" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    318 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'naechstes_sbwref_sk_trennbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_trennbauwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    319 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'naechstes_sbwref_sk_uebrige' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_uebrige" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    320 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'OID' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "T_Ili_Tid" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    321 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'paa_knotenref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "paa_knotenref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    322 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'sachbearbeiter' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sachbearbeiter" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    323 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'standortgemeinderef' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "standortgemeinderef" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    324 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'standortname' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "standortname" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    325 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'steuerung_fernwirkung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "steuerung_fernwirkung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    326 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'wbw_basisjahr' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "wbw_basisjahr" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
UNION ALL
SELECT
    327 AS sortierung,
    'sk_autonome_messstelle' AS tabelle,
    'wiederbeschaffungswert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "wiederbeschaffungswert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_autonome_messstelle"
;

DROP VIEW IF EXISTS v_statistics_sk_duekeroberhaupt;
CREATE VIEW v_statistics_sk_duekeroberhaupt AS
SELECT
    328 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'akten' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "akten" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    329 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'bemerkung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bemerkung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    330 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'bueroref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bueroref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    331 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'datenherrref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenherrref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    332 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'datenlieferantref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenlieferantref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    333 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'hauptbauwerkref_sk_autonome_messstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_autonome_messstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    334 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'hauptbauwerkref_sk_duekeroberhaupt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_duekeroberhaupt" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    335 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'hauptbauwerkref_sk_einleitstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_einleitstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    336 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'hauptbauwerkref_sk_pumpwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_pumpwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    337 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'hauptbauwerkref_sk_regenrueckhaltebecken_kanal' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_regenrueckhaltebecken_kanal" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    338 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'hauptbauwerkref_sk_regenueberlauf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_regenueberlauf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    339 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'hauptbauwerkref_sk_regenueberlaufbecken' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_regenueberlaufbecken" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    340 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'hauptbauwerkref_sk_trennbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_trennbauwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    341 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'hauptbauwerkref_sk_uebrige' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_uebrige" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    342 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'hydr_kennwerte_bezeichnung_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hydr_kennwerte_bezeichnung_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    343 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'hydr_kennwerte_bezeichnung_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hydr_kennwerte_bezeichnung_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    344 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'hydr_kennwerte_bezeichnung_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hydr_kennwerte_bezeichnung_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    345 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'informationsquelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "informationsquelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    346 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'letzte_aenderung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "letzte_aenderung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    347 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'naechstes_sbwref_sk_autonome_messstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_autonome_messstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    348 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'naechstes_sbwref_sk_duekeroberhaupt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_duekeroberhaupt" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    349 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'naechstes_sbwref_sk_einleitstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_einleitstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    350 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'naechstes_sbwref_sk_pumpwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_pumpwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    351 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'naechstes_sbwref_sk_regenrueckhaltebecken_kanal' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_regenrueckhaltebecken_kanal" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    352 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'naechstes_sbwref_sk_regenueberlauf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_regenueberlauf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    353 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'naechstes_sbwref_sk_regenueberlaufbecken' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_regenueberlaufbecken" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    354 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'naechstes_sbwref_sk_trennbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_trennbauwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    355 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'naechstes_sbwref_sk_uebrige' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_uebrige" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    356 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'obj_id_hydr_kennwerte_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_hydr_kennwerte_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    357 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'obj_id_hydr_kennwerte_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_hydr_kennwerte_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    358 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'obj_id_hydr_kennwerte_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_hydr_kennwerte_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    359 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'OID' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "T_Ili_Tid" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    360 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'paa_knotenref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "paa_knotenref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    361 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'sachbearbeiter' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sachbearbeiter" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    362 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'standortgemeinderef' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "standortgemeinderef" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    363 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'standortname' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "standortname" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    364 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'steuerung_fernwirkung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "steuerung_fernwirkung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    365 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'wbw_basisjahr' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "wbw_basisjahr" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
UNION ALL
SELECT
    366 AS sortierung,
    'sk_duekeroberhaupt' AS tabelle,
    'wiederbeschaffungswert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "wiederbeschaffungswert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_duekeroberhaupt"
;

DROP VIEW IF EXISTS v_statistics_sk_einleitstelle;
CREATE VIEW v_statistics_sk_einleitstelle AS
SELECT
    367 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'akten' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "akten" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    368 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'ausfuehrende_firmaref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "ausfuehrende_firmaref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    369 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'ausfuehrender' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "ausfuehrender" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    370 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'auslaufrohr_lichte_hoehe' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "auslaufrohr_lichte_hoehe" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    371 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'bemerkung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bemerkung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    372 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'biol_oekol_gesamtbeurteilung_bemerkung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "biol_oekol_gesamtbeurteilung_bemerkung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    373 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'biol_oekol_gesamtbeurteilung_bezeichnung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "biol_oekol_gesamtbeurteilung_bezeichnung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    374 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'bueroref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bueroref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    375 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'datenherrref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenherrref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    376 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'datenlieferantref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenlieferantref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    377 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'datum_letzte_untersuchung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datum_letzte_untersuchung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    378 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'datum_untersuchung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datum_untersuchung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    379 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'einfluss_aeusserer_aspekt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "einfluss_aeusserer_aspekt" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    380 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'einfluss_hilfsindikatoren' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "einfluss_hilfsindikatoren" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    381 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'einfluss_makroinvertebraten' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "einfluss_makroinvertebraten" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    382 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'einfluss_wasserpflanzen' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "einfluss_wasserpflanzen" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    383 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'gewaesserart' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "gewaesserart" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    384 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'gewaesserspezifische_entlastungsfracht_nh4_n_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "gewaesserspezifische_entlastungsfracht_nh4_n_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    385 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'gewaesserspezifische_entlastungsfracht_nh4_n_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "gewaesserspezifische_entlastungsfracht_nh4_n_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    386 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'gewaesserspezifische_entlastungsfracht_nh4_n_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "gewaesserspezifische_entlastungsfracht_nh4_n_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    387 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'handlungsbedarf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "handlungsbedarf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    388 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'hauptbauwerkref_sk_autonome_messstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_autonome_messstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    389 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'hauptbauwerkref_sk_duekeroberhaupt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_duekeroberhaupt" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    390 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'hauptbauwerkref_sk_einleitstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_einleitstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    391 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'hauptbauwerkref_sk_pumpwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_pumpwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    392 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'hauptbauwerkref_sk_regenrueckhaltebecken_kanal' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_regenrueckhaltebecken_kanal" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    393 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'hauptbauwerkref_sk_regenueberlauf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_regenueberlauf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    394 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'hauptbauwerkref_sk_regenueberlaufbecken' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_regenueberlaufbecken" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    395 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'hauptbauwerkref_sk_trennbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_trennbauwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    396 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'hauptbauwerkref_sk_uebrige' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_uebrige" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    397 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'immissionsorientierte_berechnung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "immissionsorientierte_berechnung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    398 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'informationsquelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "informationsquelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    399 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'letzte_aenderung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "letzte_aenderung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    400 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'naechstes_sbwref_sk_autonome_messstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_autonome_messstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    401 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'naechstes_sbwref_sk_duekeroberhaupt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_duekeroberhaupt" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    402 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'naechstes_sbwref_sk_einleitstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_einleitstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    403 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'naechstes_sbwref_sk_pumpwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_pumpwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    404 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'naechstes_sbwref_sk_regenrueckhaltebecken_kanal' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_regenrueckhaltebecken_kanal" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    405 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'naechstes_sbwref_sk_regenueberlauf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_regenueberlauf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    406 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'naechstes_sbwref_sk_regenueberlaufbecken' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_regenueberlaufbecken" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    407 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'naechstes_sbwref_sk_trennbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_trennbauwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    408 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'naechstes_sbwref_sk_uebrige' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_uebrige" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    409 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'oberflaechengewaesser' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "oberflaechengewaesser" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    410 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'obj_id_biol_oekol_gesamtbeurteilung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_biol_oekol_gesamtbeurteilung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    411 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'obj_id_erhaltungsereignis_abwasserbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_erhaltungsereignis_abwasserbauwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    412 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'OID' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "T_Ili_Tid" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    413 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'paa_knotenref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "paa_knotenref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    414 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'q347' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "q347" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    415 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'relevantes_gefaelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "relevantes_gefaelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    416 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'relevanzmatrix' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "relevanzmatrix" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    417 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'sachbearbeiter' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sachbearbeiter" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    418 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'standortgemeinderef' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "standortgemeinderef" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    419 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'standortname' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "standortname" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    420 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'steuerung_fernwirkung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "steuerung_fernwirkung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    421 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'vergleich_letzte_untersuchung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "vergleich_letzte_untersuchung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    422 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'wasserspiegel_hydraulik' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "wasserspiegel_hydraulik" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    423 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'wbw_basisjahr' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "wbw_basisjahr" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
UNION ALL
SELECT
    424 AS sortierung,
    'sk_einleitstelle' AS tabelle,
    'wiederbeschaffungswert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "wiederbeschaffungswert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_einleitstelle"
;

DROP VIEW IF EXISTS v_statistics_sk_pumpwerk;
CREATE VIEW v_statistics_sk_pumpwerk AS
SELECT
    425 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'aggregatezahl' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "aggregatezahl" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    426 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'akten' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "akten" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    427 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'bemerkung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bemerkung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    428 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'bueroref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bueroref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    429 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'datenherrref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenherrref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    430 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'datenlieferantref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenlieferantref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    431 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'einwohner_dim_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "einwohner_dim_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    432 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'einwohner_dim_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "einwohner_dim_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    433 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'flaeche_bef_dim_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_bef_dim_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    434 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'flaeche_bef_dim_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_bef_dim_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    435 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'flaeche_dim_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_dim_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    436 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'flaeche_dim_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_dim_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    437 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'flaeche_red_dim_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_red_dim_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    438 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'flaeche_red_dim_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_red_dim_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    439 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'foerderaggregat_nutzungsart_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "foerderaggregat_nutzungsart_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    440 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'foerderhoehe_geodaetisch' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "foerderhoehe_geodaetisch" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    441 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'foerderstrommax' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "foerderstrommax" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    442 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'foerderstrommin' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "foerderstrommin" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    443 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'fremdwasseranfall_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "fremdwasseranfall_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    444 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'fremdwasseranfall_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "fremdwasseranfall_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    445 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'hauptbauwerkref_sk_autonome_messstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_autonome_messstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    446 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'hauptbauwerkref_sk_duekeroberhaupt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_duekeroberhaupt" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    447 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'hauptbauwerkref_sk_einleitstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_einleitstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    448 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'hauptbauwerkref_sk_pumpwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_pumpwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    449 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'hauptbauwerkref_sk_regenrueckhaltebecken_kanal' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_regenrueckhaltebecken_kanal" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    450 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'hauptbauwerkref_sk_regenueberlauf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_regenueberlauf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    451 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'hauptbauwerkref_sk_regenueberlaufbecken' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_regenueberlaufbecken" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    452 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'hauptbauwerkref_sk_trennbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_trennbauwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    453 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'hauptbauwerkref_sk_uebrige' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_uebrige" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    454 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'hydr_kennwerte_bezeichnung_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hydr_kennwerte_bezeichnung_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    455 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'hydr_kennwerte_bezeichnung_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hydr_kennwerte_bezeichnung_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    456 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'hydr_kennwerte_bezeichnung_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hydr_kennwerte_bezeichnung_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    457 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'informationsquelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "informationsquelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    458 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'letzte_aenderung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "letzte_aenderung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    459 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'naechstes_sbwref_sk_autonome_messstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_autonome_messstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    460 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'naechstes_sbwref_sk_duekeroberhaupt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_duekeroberhaupt" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    461 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'naechstes_sbwref_sk_einleitstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_einleitstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    462 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'naechstes_sbwref_sk_pumpwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_pumpwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    463 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'naechstes_sbwref_sk_regenrueckhaltebecken_kanal' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_regenrueckhaltebecken_kanal" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    464 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'naechstes_sbwref_sk_regenueberlauf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_regenueberlauf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    465 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'naechstes_sbwref_sk_regenueberlaufbecken' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_regenueberlaufbecken" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    466 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'naechstes_sbwref_sk_trennbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_trennbauwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    467 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'naechstes_sbwref_sk_uebrige' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_uebrige" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    468 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'obj_id_gesamteinzugsgebiet_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_gesamteinzugsgebiet_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    469 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'obj_id_gesamteinzugsgebiet_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_gesamteinzugsgebiet_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    470 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'obj_id_hydr_kennwerte_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_hydr_kennwerte_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    471 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'obj_id_hydr_kennwerte_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_hydr_kennwerte_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    472 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'obj_id_hydr_kennwerte_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_hydr_kennwerte_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    473 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'OID' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "T_Ili_Tid" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    474 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'paa_knotenref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "paa_knotenref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    475 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'pumpenregime' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "pumpenregime" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    476 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'sachbearbeiter' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sachbearbeiter" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    477 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'schmutzabwasseranfall_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "schmutzabwasseranfall_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    478 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'schmutzabwasseranfall_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "schmutzabwasseranfall_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    479 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'standortgemeinderef' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "standortgemeinderef" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    480 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'standortname' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "standortname" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    481 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'stauraum' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "stauraum" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    482 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'steuerung_fernwirkung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "steuerung_fernwirkung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    483 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'volumen_pumpensumpf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "volumen_pumpensumpf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    484 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'wbw_basisjahr' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "wbw_basisjahr" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
UNION ALL
SELECT
    485 AS sortierung,
    'sk_pumpwerk' AS tabelle,
    'wiederbeschaffungswert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "wiederbeschaffungswert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_pumpwerk"
;

DROP VIEW IF EXISTS v_statistics_sk_regenrueckhaltebecken_kanal;
CREATE VIEW v_statistics_sk_regenrueckhaltebecken_kanal AS
SELECT
    486 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'akten' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "akten" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    487 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'bemerkung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bemerkung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    488 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'bueroref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bueroref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    489 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'datenherrref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenherrref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    490 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'datenlieferantref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenlieferantref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    491 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'einwohner_dim_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "einwohner_dim_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    492 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'einwohner_dim_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "einwohner_dim_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    493 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'flaeche_bef_dim_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_bef_dim_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    494 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'flaeche_bef_dim_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_bef_dim_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    495 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'flaeche_dim_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_dim_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    496 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'flaeche_dim_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_dim_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    497 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'flaeche_red_dim_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_red_dim_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    498 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'flaeche_red_dim_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_red_dim_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    499 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'fremdwasseranfall_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "fremdwasseranfall_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    500 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'fremdwasseranfall_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "fremdwasseranfall_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    501 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'gesamteinzugsgebiet_bezeichnung_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "gesamteinzugsgebiet_bezeichnung_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    502 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'gesamteinzugsgebiet_bezeichnung_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "gesamteinzugsgebiet_bezeichnung_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    503 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'gesamteinzugsgebiet_bezeichnung_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "gesamteinzugsgebiet_bezeichnung_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    504 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'hauptbauwerkref_sk_autonome_messstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_autonome_messstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    505 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'hauptbauwerkref_sk_duekeroberhaupt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_duekeroberhaupt" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    506 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'hauptbauwerkref_sk_einleitstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_einleitstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    507 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'hauptbauwerkref_sk_pumpwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_pumpwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    508 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'hauptbauwerkref_sk_regenrueckhaltebecken_kanal' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_regenrueckhaltebecken_kanal" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    509 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'hauptbauwerkref_sk_regenueberlauf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_regenueberlauf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    510 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'hauptbauwerkref_sk_regenueberlaufbecken' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_regenueberlaufbecken" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    511 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'hauptbauwerkref_sk_trennbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_trennbauwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    512 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'hauptbauwerkref_sk_uebrige' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_uebrige" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    513 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'hydr_kennwerte_bezeichnung_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hydr_kennwerte_bezeichnung_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    514 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'hydr_kennwerte_bezeichnung_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hydr_kennwerte_bezeichnung_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    515 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'hydr_kennwerte_bezeichnung_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hydr_kennwerte_bezeichnung_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    516 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'informationsquelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "informationsquelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    517 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'letzte_aenderung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "letzte_aenderung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    518 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'naechstes_sbwref_sk_autonome_messstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_autonome_messstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    519 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'naechstes_sbwref_sk_duekeroberhaupt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_duekeroberhaupt" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    520 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'naechstes_sbwref_sk_einleitstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_einleitstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    521 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'naechstes_sbwref_sk_pumpwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_pumpwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    522 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'naechstes_sbwref_sk_regenrueckhaltebecken_kanal' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_regenrueckhaltebecken_kanal" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    523 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'naechstes_sbwref_sk_regenueberlauf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_regenueberlauf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    524 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'naechstes_sbwref_sk_regenueberlaufbecken' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_regenueberlaufbecken" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    525 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'naechstes_sbwref_sk_trennbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_trennbauwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    526 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'naechstes_sbwref_sk_uebrige' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_uebrige" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    527 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'notueberlauf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "notueberlauf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    528 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'nutzinhalt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "nutzinhalt" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    529 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'obj_id_gesamteinzugsgebiet_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_gesamteinzugsgebiet_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    530 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'obj_id_gesamteinzugsgebiet_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_gesamteinzugsgebiet_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    531 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'obj_id_gesamteinzugsgebiet_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_gesamteinzugsgebiet_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    532 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'obj_id_hydr_kennwerte_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_hydr_kennwerte_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    533 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'obj_id_hydr_kennwerte_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_hydr_kennwerte_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    534 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'obj_id_hydr_kennwerte_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_hydr_kennwerte_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    535 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'OID' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "T_Ili_Tid" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    536 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'paa_knotenref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "paa_knotenref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    537 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'qab_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "qab_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    538 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'qab_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "qab_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    539 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'qab_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "qab_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    540 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'regenbecken_anordnung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "regenbecken_anordnung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    541 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'sachbearbeiter' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sachbearbeiter" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    542 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'schmutzabwasseranfall_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "schmutzabwasseranfall_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    543 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'schmutzabwasseranfall_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "schmutzabwasseranfall_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    544 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'standortgemeinderef' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "standortgemeinderef" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    545 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'standortname' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "standortname" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    546 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'stauraum' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "stauraum" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    547 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'steuerung_fernwirkung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "steuerung_fernwirkung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    548 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'wbw_basisjahr' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "wbw_basisjahr" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
UNION ALL
SELECT
    549 AS sortierung,
    'sk_regenrueckhaltebecken_kanal' AS tabelle,
    'wiederbeschaffungswert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "wiederbeschaffungswert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenrueckhaltebecken_kanal"
;

DROP VIEW IF EXISTS v_statistics_sk_regenueberlauf;
CREATE VIEW v_statistics_sk_regenueberlauf AS
SELECT
    550 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'akten' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "akten" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    551 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'bemerkung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bemerkung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    552 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'bueroref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bueroref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    553 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'datenherrref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenherrref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    554 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'datenlieferantref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenlieferantref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    555 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'einleitstelleref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "einleitstelleref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    556 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'einwohner_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "einwohner_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    557 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'einwohner_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "einwohner_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    558 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'entlastungsanteil_nh4_n_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "entlastungsanteil_nh4_n_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    559 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'entlastungsanteil_nh4_n_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "entlastungsanteil_nh4_n_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    560 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'entlastungsanteil_nh4_n_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "entlastungsanteil_nh4_n_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    561 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'entlastungsfracht_nh4_n_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "entlastungsfracht_nh4_n_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    562 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'entlastungsfracht_nh4_n_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "entlastungsfracht_nh4_n_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    563 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'entlastungsfracht_nh4_n_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "entlastungsfracht_nh4_n_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    564 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'flaeche_bef_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_bef_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    565 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'flaeche_bef_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_bef_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    566 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'flaeche_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    567 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'flaeche_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    568 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'flaeche_red_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_red_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    569 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'flaeche_red_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_red_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    570 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'fremdwasseranfall_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "fremdwasseranfall_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    571 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'fremdwasseranfall_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "fremdwasseranfall_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    572 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'gesamteinzugsgebiet_bezeichnung_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "gesamteinzugsgebiet_bezeichnung_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    573 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'gesamteinzugsgebiet_bezeichnung_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "gesamteinzugsgebiet_bezeichnung_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    574 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'gesamteinzugsgebiet_bezeichnung_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "gesamteinzugsgebiet_bezeichnung_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    575 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'hauptbauwerkref_sk_autonome_messstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_autonome_messstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    576 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'hauptbauwerkref_sk_duekeroberhaupt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_duekeroberhaupt" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    577 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'hauptbauwerkref_sk_einleitstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_einleitstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    578 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'hauptbauwerkref_sk_pumpwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_pumpwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    579 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'hauptbauwerkref_sk_regenrueckhaltebecken_kanal' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_regenrueckhaltebecken_kanal" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    580 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'hauptbauwerkref_sk_regenueberlauf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_regenueberlauf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    581 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'hauptbauwerkref_sk_regenueberlaufbecken' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_regenueberlaufbecken" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    582 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'hauptbauwerkref_sk_trennbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_trennbauwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    583 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'hauptbauwerkref_sk_uebrige' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_uebrige" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    584 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'hydr_kennwerte_bezeichnung_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hydr_kennwerte_bezeichnung_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    585 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'hydr_kennwerte_bezeichnung_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hydr_kennwerte_bezeichnung_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    586 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'hydr_kennwerte_bezeichnung_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hydr_kennwerte_bezeichnung_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    587 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'informationsquelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "informationsquelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    588 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'letzte_aenderung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "letzte_aenderung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    589 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'mehrbelastung_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "mehrbelastung_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    590 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'mehrbelastung_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "mehrbelastung_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    591 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'mehrbelastung_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "mehrbelastung_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    592 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'naechstes_sbwref_sk_autonome_messstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_autonome_messstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    593 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'naechstes_sbwref_sk_duekeroberhaupt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_duekeroberhaupt" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    594 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'naechstes_sbwref_sk_einleitstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_einleitstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    595 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'naechstes_sbwref_sk_pumpwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_pumpwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    596 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'naechstes_sbwref_sk_regenrueckhaltebecken_kanal' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_regenrueckhaltebecken_kanal" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    597 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'naechstes_sbwref_sk_regenueberlauf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_regenueberlauf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    598 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'naechstes_sbwref_sk_regenueberlaufbecken' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_regenueberlaufbecken" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    599 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'naechstes_sbwref_sk_trennbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_trennbauwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    600 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'naechstes_sbwref_sk_uebrige' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_uebrige" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    601 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'obj_id_gesamteinzugsgebiet_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_gesamteinzugsgebiet_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    602 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'obj_id_gesamteinzugsgebiet_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_gesamteinzugsgebiet_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    603 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'obj_id_gesamteinzugsgebiet_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_gesamteinzugsgebiet_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    604 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'obj_id_hydr_kennwerte_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_hydr_kennwerte_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    605 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'obj_id_hydr_kennwerte_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_hydr_kennwerte_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    606 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'obj_id_hydr_kennwerte_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_hydr_kennwerte_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    607 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'OID' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "T_Ili_Tid" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    608 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'paa_knotenref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "paa_knotenref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    609 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'qan_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "qan_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    610 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'qan_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "qan_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    611 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'qan_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "qan_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    612 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'sachbearbeiter' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sachbearbeiter" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    613 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'schmutzabwasseranfall_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "schmutzabwasseranfall_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    614 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'schmutzabwasseranfall_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "schmutzabwasseranfall_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    615 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'springt_an' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "springt_an" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    616 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'standortgemeinderef' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "standortgemeinderef" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    617 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'standortname' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "standortname" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    618 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'stauraum' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "stauraum" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    619 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'steuerung_fernwirkung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "steuerung_fernwirkung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    620 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'ueberlauf_bemerkung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "ueberlauf_bemerkung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    621 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'ueberlaufdauer_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "ueberlaufdauer_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    622 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'ueberlaufdauer_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "ueberlaufdauer_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    623 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'ueberlaufdauer_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "ueberlaufdauer_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    624 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'ueberlaufhaeufigkeit_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "ueberlaufhaeufigkeit_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    625 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'ueberlaufhaeufigkeit_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "ueberlaufhaeufigkeit_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    626 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'ueberlaufhaeufigkeit_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "ueberlaufhaeufigkeit_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    627 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'ueberlaufmenge_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "ueberlaufmenge_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    628 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'ueberlaufmenge_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "ueberlaufmenge_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    629 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'ueberlaufmenge_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "ueberlaufmenge_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    630 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'wbw_basisjahr' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "wbw_basisjahr" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    631 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'wehr_art' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "wehr_art" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
UNION ALL
SELECT
    632 AS sortierung,
    'sk_regenueberlauf' AS tabelle,
    'wiederbeschaffungswert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "wiederbeschaffungswert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlauf"
;

DROP VIEW IF EXISTS v_statistics_sk_regenueberlaufbecken;
CREATE VIEW v_statistics_sk_regenueberlaufbecken AS
SELECT
    633 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'akten' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "akten" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    634 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'bemerkung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bemerkung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    635 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'bueroref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bueroref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    636 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'datenherrref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenherrref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    637 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'datenlieferantref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenlieferantref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    638 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'einleitstelleref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "einleitstelleref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    639 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'einwohner_dim_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "einwohner_dim_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    640 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'einwohner_dim_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "einwohner_dim_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    641 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'einwohner_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "einwohner_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    642 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'einwohner_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "einwohner_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    643 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'entlastungsanteil_nh4_n_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "entlastungsanteil_nh4_n_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    644 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'entlastungsanteil_nh4_n_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "entlastungsanteil_nh4_n_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    645 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'entlastungsanteil_nh4_n_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "entlastungsanteil_nh4_n_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    646 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'entlastungsfracht_nh4_n_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "entlastungsfracht_nh4_n_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    647 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'entlastungsfracht_nh4_n_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "entlastungsfracht_nh4_n_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    648 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'entlastungsfracht_nh4_n_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "entlastungsfracht_nh4_n_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    649 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'flaeche_bef_dim_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_bef_dim_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    650 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'flaeche_bef_dim_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_bef_dim_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    651 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'flaeche_bef_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_bef_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    652 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'flaeche_bef_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_bef_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    653 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'flaeche_dim_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_dim_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    654 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'flaeche_dim_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_dim_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    655 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'flaeche_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    656 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'flaeche_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    657 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'flaeche_red_dim_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_red_dim_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    658 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'flaeche_red_dim_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_red_dim_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    659 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'flaeche_red_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_red_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    660 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'flaeche_red_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "flaeche_red_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    661 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'fremdwasseranfall_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "fremdwasseranfall_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    662 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'fremdwasseranfall_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "fremdwasseranfall_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    663 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'gesamteinzugsgebiet_bezeichnung_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "gesamteinzugsgebiet_bezeichnung_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    664 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'gesamteinzugsgebiet_bezeichnung_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "gesamteinzugsgebiet_bezeichnung_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    665 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'gesamteinzugsgebiet_bezeichnung_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "gesamteinzugsgebiet_bezeichnung_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    666 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'hauptbauwerkref_sk_autonome_messstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_autonome_messstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    667 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'hauptbauwerkref_sk_duekeroberhaupt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_duekeroberhaupt" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    668 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'hauptbauwerkref_sk_einleitstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_einleitstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    669 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'hauptbauwerkref_sk_pumpwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_pumpwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    670 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'hauptbauwerkref_sk_regenrueckhaltebecken_kanal' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_regenrueckhaltebecken_kanal" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    671 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'hauptbauwerkref_sk_regenueberlauf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_regenueberlauf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    672 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'hauptbauwerkref_sk_regenueberlaufbecken' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_regenueberlaufbecken" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    673 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'hauptbauwerkref_sk_trennbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_trennbauwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    674 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'hauptbauwerkref_sk_uebrige' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_uebrige" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    675 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'hydr_kennwerte_bezeichnung_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hydr_kennwerte_bezeichnung_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    676 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'hydr_kennwerte_bezeichnung_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hydr_kennwerte_bezeichnung_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    677 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'hydr_kennwerte_bezeichnung_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hydr_kennwerte_bezeichnung_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    678 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'informationsquelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "informationsquelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    679 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'letzte_aenderung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "letzte_aenderung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    680 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'mehrbelastung_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "mehrbelastung_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    681 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'mehrbelastung_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "mehrbelastung_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    682 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'mehrbelastung_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "mehrbelastung_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    683 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'naechstes_sbwref_sk_autonome_messstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_autonome_messstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    684 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'naechstes_sbwref_sk_duekeroberhaupt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_duekeroberhaupt" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    685 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'naechstes_sbwref_sk_einleitstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_einleitstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    686 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'naechstes_sbwref_sk_pumpwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_pumpwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    687 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'naechstes_sbwref_sk_regenrueckhaltebecken_kanal' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_regenrueckhaltebecken_kanal" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    688 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'naechstes_sbwref_sk_regenueberlauf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_regenueberlauf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    689 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'naechstes_sbwref_sk_regenueberlaufbecken' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_regenueberlaufbecken" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    690 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'naechstes_sbwref_sk_trennbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_trennbauwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    691 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'naechstes_sbwref_sk_uebrige' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_uebrige" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    692 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'nutzinhalt_fangteil' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "nutzinhalt_fangteil" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    693 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'nutzinhalt_klaerteil' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "nutzinhalt_klaerteil" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    694 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'obj_id_gesamteinzugsgebiet_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_gesamteinzugsgebiet_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    695 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'obj_id_gesamteinzugsgebiet_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_gesamteinzugsgebiet_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    696 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'obj_id_gesamteinzugsgebiet_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_gesamteinzugsgebiet_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    697 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'obj_id_hydr_kennwerte_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_hydr_kennwerte_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    698 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'obj_id_hydr_kennwerte_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_hydr_kennwerte_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    699 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'obj_id_hydr_kennwerte_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_hydr_kennwerte_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    700 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'OID' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "T_Ili_Tid" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    701 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'paa_knotenref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "paa_knotenref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    702 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'qan_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "qan_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    703 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'qan_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "qan_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    704 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'qan_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "qan_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    705 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'regenbecken_anordnung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "regenbecken_anordnung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    706 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'sachbearbeiter' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sachbearbeiter" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    707 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'schmutzabwasseranfall_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "schmutzabwasseranfall_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    708 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'schmutzabwasseranfall_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "schmutzabwasseranfall_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    709 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'standortgemeinderef' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "standortgemeinderef" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    710 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'standortname' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "standortname" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    711 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'stauraum' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "stauraum" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    712 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'steuerung_fernwirkung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "steuerung_fernwirkung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    713 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'ueberlauf_bemerkung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "ueberlauf_bemerkung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    714 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'ueberlaufdauer_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "ueberlaufdauer_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    715 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'ueberlaufdauer_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "ueberlaufdauer_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    716 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'ueberlaufdauer_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "ueberlaufdauer_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    717 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'ueberlaufhaeufigkeit_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "ueberlaufhaeufigkeit_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    718 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'ueberlaufhaeufigkeit_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "ueberlaufhaeufigkeit_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    719 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'ueberlaufhaeufigkeit_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "ueberlaufhaeufigkeit_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    720 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'ueberlaufmenge_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "ueberlaufmenge_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    721 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'ueberlaufmenge_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "ueberlaufmenge_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    722 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'ueberlaufmenge_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "ueberlaufmenge_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    723 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'wbw_basisjahr' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "wbw_basisjahr" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
UNION ALL
SELECT
    724 AS sortierung,
    'sk_regenueberlaufbecken' AS tabelle,
    'wiederbeschaffungswert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "wiederbeschaffungswert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_regenueberlaufbecken"
;

DROP VIEW IF EXISTS v_statistics_sk_trennbauwerk;
CREATE VIEW v_statistics_sk_trennbauwerk AS
SELECT
    725 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'akten' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "akten" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    726 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'art' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "art" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    727 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'bemerkung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bemerkung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    728 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'bueroref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bueroref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    729 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'datenherrref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenherrref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    730 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'datenlieferantref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenlieferantref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    731 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'hauptbauwerkref_sk_autonome_messstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_autonome_messstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    732 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'hauptbauwerkref_sk_duekeroberhaupt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_duekeroberhaupt" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    733 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'hauptbauwerkref_sk_einleitstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_einleitstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    734 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'hauptbauwerkref_sk_pumpwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_pumpwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    735 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'hauptbauwerkref_sk_regenrueckhaltebecken_kanal' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_regenrueckhaltebecken_kanal" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    736 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'hauptbauwerkref_sk_regenueberlauf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_regenueberlauf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    737 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'hauptbauwerkref_sk_regenueberlaufbecken' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_regenueberlaufbecken" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    738 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'hauptbauwerkref_sk_trennbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_trennbauwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    739 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'hauptbauwerkref_sk_uebrige' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_uebrige" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    740 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'hydr_kennwerte_bezeichnung_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hydr_kennwerte_bezeichnung_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    741 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'hydr_kennwerte_bezeichnung_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hydr_kennwerte_bezeichnung_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    742 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'hydr_kennwerte_bezeichnung_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hydr_kennwerte_bezeichnung_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    743 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'informationsquelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "informationsquelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    744 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'letzte_aenderung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "letzte_aenderung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    745 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'mehrbelastung_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "mehrbelastung_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    746 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'mehrbelastung_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "mehrbelastung_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    747 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'mehrbelastung_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "mehrbelastung_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    748 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'naechstes_sbwref_sk_autonome_messstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_autonome_messstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    749 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'naechstes_sbwref_sk_duekeroberhaupt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_duekeroberhaupt" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    750 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'naechstes_sbwref_sk_einleitstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_einleitstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    751 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'naechstes_sbwref_sk_pumpwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_pumpwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    752 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'naechstes_sbwref_sk_regenrueckhaltebecken_kanal' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_regenrueckhaltebecken_kanal" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    753 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'naechstes_sbwref_sk_regenueberlauf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_regenueberlauf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    754 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'naechstes_sbwref_sk_regenueberlaufbecken' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_regenueberlaufbecken" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    755 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'naechstes_sbwref_sk_trennbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_trennbauwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    756 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'naechstes_sbwref_sk_uebrige' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_uebrige" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    757 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'obj_id_hydr_kennwerte_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_hydr_kennwerte_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    758 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'obj_id_hydr_kennwerte_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_hydr_kennwerte_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    759 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'obj_id_hydr_kennwerte_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_hydr_kennwerte_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    760 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'OID' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "T_Ili_Tid" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    761 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'paa_knotenref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "paa_knotenref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    762 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'primaerrichtungref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "primaerrichtungref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    763 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'qan_geplant' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "qan_geplant" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    764 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'qan_ist' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "qan_ist" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    765 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'qan_ist_optimiert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "qan_ist_optimiert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    766 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'sachbearbeiter' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sachbearbeiter" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    767 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'standortgemeinderef' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "standortgemeinderef" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    768 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'standortname' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "standortname" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    769 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'stauraum' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "stauraum" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    770 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'steuerung_fernwirkung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "steuerung_fernwirkung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    771 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'wbw_basisjahr' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "wbw_basisjahr" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
UNION ALL
SELECT
    772 AS sortierung,
    'sk_trennbauwerk' AS tabelle,
    'wiederbeschaffungswert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "wiederbeschaffungswert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_trennbauwerk"
;

DROP VIEW IF EXISTS v_statistics_sk_uebrige;
CREATE VIEW v_statistics_sk_uebrige AS
SELECT
    773 AS sortierung,
    'sk_uebrige' AS tabelle,
    'akten' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "akten" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    774 AS sortierung,
    'sk_uebrige' AS tabelle,
    'bemerkung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bemerkung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    775 AS sortierung,
    'sk_uebrige' AS tabelle,
    'beschrieb' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "beschrieb" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    776 AS sortierung,
    'sk_uebrige' AS tabelle,
    'bueroref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "bueroref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    777 AS sortierung,
    'sk_uebrige' AS tabelle,
    'datenherrref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenherrref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    778 AS sortierung,
    'sk_uebrige' AS tabelle,
    'datenlieferantref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "datenlieferantref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    779 AS sortierung,
    'sk_uebrige' AS tabelle,
    'hauptbauwerkref_sk_autonome_messstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_autonome_messstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    780 AS sortierung,
    'sk_uebrige' AS tabelle,
    'hauptbauwerkref_sk_duekeroberhaupt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_duekeroberhaupt" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    781 AS sortierung,
    'sk_uebrige' AS tabelle,
    'hauptbauwerkref_sk_einleitstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_einleitstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    782 AS sortierung,
    'sk_uebrige' AS tabelle,
    'hauptbauwerkref_sk_pumpwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_pumpwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    783 AS sortierung,
    'sk_uebrige' AS tabelle,
    'hauptbauwerkref_sk_regenrueckhaltebecken_kanal' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_regenrueckhaltebecken_kanal" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    784 AS sortierung,
    'sk_uebrige' AS tabelle,
    'hauptbauwerkref_sk_regenueberlauf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_regenueberlauf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    785 AS sortierung,
    'sk_uebrige' AS tabelle,
    'hauptbauwerkref_sk_regenueberlaufbecken' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_regenueberlaufbecken" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    786 AS sortierung,
    'sk_uebrige' AS tabelle,
    'hauptbauwerkref_sk_trennbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_trennbauwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    787 AS sortierung,
    'sk_uebrige' AS tabelle,
    'hauptbauwerkref_sk_uebrige' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hauptbauwerkref_sk_uebrige" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    788 AS sortierung,
    'sk_uebrige' AS tabelle,
    'informationsquelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "informationsquelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    789 AS sortierung,
    'sk_uebrige' AS tabelle,
    'letzte_aenderung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "letzte_aenderung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    790 AS sortierung,
    'sk_uebrige' AS tabelle,
    'naechstes_sbwref_sk_autonome_messstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_autonome_messstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    791 AS sortierung,
    'sk_uebrige' AS tabelle,
    'naechstes_sbwref_sk_duekeroberhaupt' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_duekeroberhaupt" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    792 AS sortierung,
    'sk_uebrige' AS tabelle,
    'naechstes_sbwref_sk_einleitstelle' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_einleitstelle" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    793 AS sortierung,
    'sk_uebrige' AS tabelle,
    'naechstes_sbwref_sk_pumpwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_pumpwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    794 AS sortierung,
    'sk_uebrige' AS tabelle,
    'naechstes_sbwref_sk_regenrueckhaltebecken_kanal' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_regenrueckhaltebecken_kanal" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    795 AS sortierung,
    'sk_uebrige' AS tabelle,
    'naechstes_sbwref_sk_regenueberlauf' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_regenueberlauf" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    796 AS sortierung,
    'sk_uebrige' AS tabelle,
    'naechstes_sbwref_sk_regenueberlaufbecken' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_regenueberlaufbecken" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    797 AS sortierung,
    'sk_uebrige' AS tabelle,
    'naechstes_sbwref_sk_trennbauwerk' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_trennbauwerk" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    798 AS sortierung,
    'sk_uebrige' AS tabelle,
    'naechstes_sbwref_sk_uebrige' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "naechstes_sbwref_sk_uebrige" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    799 AS sortierung,
    'sk_uebrige' AS tabelle,
    'OID' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "T_Ili_Tid" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    800 AS sortierung,
    'sk_uebrige' AS tabelle,
    'paa_knotenref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "paa_knotenref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    801 AS sortierung,
    'sk_uebrige' AS tabelle,
    'sachbearbeiter' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sachbearbeiter" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    802 AS sortierung,
    'sk_uebrige' AS tabelle,
    'standortgemeinderef' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "standortgemeinderef" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    803 AS sortierung,
    'sk_uebrige' AS tabelle,
    'standortname' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "standortname" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    804 AS sortierung,
    'sk_uebrige' AS tabelle,
    'steuerung_fernwirkung' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "steuerung_fernwirkung" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    805 AS sortierung,
    'sk_uebrige' AS tabelle,
    'wbw_basisjahr' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "wbw_basisjahr" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
UNION ALL
SELECT
    806 AS sortierung,
    'sk_uebrige' AS tabelle,
    'wiederbeschaffungswert' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "wiederbeschaffungswert" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "sk_uebrige"
;

DROP VIEW IF EXISTS v_statistics_kennlinie_stuetzpunkt;
CREATE VIEW v_statistics_kennlinie_stuetzpunkt AS
SELECT
    807 AS sortierung,
    'kennlinie_stuetzpunkt' AS tabelle,
    'abfluss' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "abfluss" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "kennlinie_stuetzpunkt"
UNION ALL
SELECT
    808 AS sortierung,
    'kennlinie_stuetzpunkt' AS tabelle,
    'hoehe' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "hoehe" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "kennlinie_stuetzpunkt"
UNION ALL
SELECT
    809 AS sortierung,
    'kennlinie_stuetzpunkt' AS tabelle,
    'obj_id_hq_relation' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "obj_id_hq_relation" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "kennlinie_stuetzpunkt"
UNION ALL
SELECT
    810 AS sortierung,
    'kennlinie_stuetzpunkt' AS tabelle,
    'OID' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "T_Ili_Tid" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "kennlinie_stuetzpunkt"
UNION ALL
SELECT
    811 AS sortierung,
    'kennlinie_stuetzpunkt' AS tabelle,
    'sk_duekeroberhauptref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sk_duekeroberhauptref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "kennlinie_stuetzpunkt"
UNION ALL
SELECT
    812 AS sortierung,
    'kennlinie_stuetzpunkt' AS tabelle,
    'sk_pumpwerkref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sk_pumpwerkref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "kennlinie_stuetzpunkt"
UNION ALL
SELECT
    813 AS sortierung,
    'kennlinie_stuetzpunkt' AS tabelle,
    'sk_regenrueckhaltebecken_kanalref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sk_regenrueckhaltebecken_kanalref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "kennlinie_stuetzpunkt"
UNION ALL
SELECT
    814 AS sortierung,
    'kennlinie_stuetzpunkt' AS tabelle,
    'sk_regenueberlaufref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sk_regenueberlaufref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "kennlinie_stuetzpunkt"
UNION ALL
SELECT
    815 AS sortierung,
    'kennlinie_stuetzpunkt' AS tabelle,
    'sk_regenueberlaufbeckenref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sk_regenueberlaufbeckenref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "kennlinie_stuetzpunkt"
UNION ALL
SELECT
    816 AS sortierung,
    'kennlinie_stuetzpunkt' AS tabelle,
    'sk_trennbauwerkref' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "sk_trennbauwerkref" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "kennlinie_stuetzpunkt"
UNION ALL
SELECT
    817 AS sortierung,
    'kennlinie_stuetzpunkt' AS tabelle,
    'astatus' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "astatus" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "kennlinie_stuetzpunkt"
UNION ALL
SELECT
    818 AS sortierung,
    'kennlinie_stuetzpunkt' AS tabelle,
    'zufluss' AS attribut,
    COUNT(*) AS anzahl_total,
    NULL AS anzahl_paa,
    NULL AS anzahl_saa,
    COUNT(*) FILTER (WHERE "zufluss" IS NULL) AS anzahl_null,
    NULL AS anzahl_null_paa,
    NULL AS anzahl_null_saa
FROM "kennlinie_stuetzpunkt"
;

-- Gesamt-View in Tabellen- und Attributreihenfolge
DROP VIEW IF EXISTS v_statistics_attribute;
CREATE VIEW v_statistics_attribute AS
SELECT sortierung, tabelle, attribut, anzahl_total, anzahl_paa, anzahl_saa, anzahl_null, anzahl_null_paa, anzahl_null_saa
FROM
(
    SELECT * FROM v_statistics_alr
    UNION ALL
    SELECT * FROM v_statistics_knoten
    UNION ALL
    SELECT * FROM v_statistics_leitung
    UNION ALL
    SELECT * FROM v_statistics_massnahme
    UNION ALL
    SELECT * FROM v_statistics_organisation
    UNION ALL
    SELECT * FROM v_statistics_rohrprofil
    UNION ALL
    SELECT * FROM v_statistics_rohrprofil_geometrie
    UNION ALL
    SELECT * FROM v_statistics_teileinzugsgebiet
    UNION ALL
    SELECT * FROM v_statistics_ueberlauf_foerderaggregat
    UNION ALL
    SELECT * FROM v_statistics_bauwerkskomponente
    UNION ALL
    SELECT * FROM v_statistics_sk_autonome_messstelle
    UNION ALL
    SELECT * FROM v_statistics_sk_duekeroberhaupt
    UNION ALL
    SELECT * FROM v_statistics_sk_einleitstelle
    UNION ALL
    SELECT * FROM v_statistics_sk_pumpwerk
    UNION ALL
    SELECT * FROM v_statistics_sk_regenrueckhaltebecken_kanal
    UNION ALL
    SELECT * FROM v_statistics_sk_regenueberlauf
    UNION ALL
    SELECT * FROM v_statistics_sk_regenueberlaufbecken
    UNION ALL
    SELECT * FROM v_statistics_sk_trennbauwerk
    UNION ALL
    SELECT * FROM v_statistics_sk_uebrige
    UNION ALL
    SELECT * FROM v_statistics_kennlinie_stuetzpunkt
)
ORDER BY sortierung;
