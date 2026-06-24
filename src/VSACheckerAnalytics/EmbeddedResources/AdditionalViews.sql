CREATE VIEW v_vsa_knoten AS SELECT
    k.T_Id AS fid,
    k.T_Ili_Tid AS tid,
    k.ara_nr,
    k.baujahr,
    k.baulicherzustand,
    k.bemerkung,
    k.bezeichnung,
    k.deckelkote,
    k.dimension1,
    k.dimension2,
    k.finanzierung,
    k.funktion,
    k.funktionhierarchisch,
    k.lagegenauigkeit,
    k.nutzungsart_geplant,
    k.nutzungsart_ist,
    k.obj_id_abwasserbauwerk,
    k.obj_id_deckel,
    k.rueckstaukote_ist,
    k.sanierungsbedarf,
    k.sohlenkote,
    k.astatus AS status,
    k.symbolori,
    k.zugaenglichkeit,
    k.zustandserhebung_jahr,
    o1.bezeichnung AS betreiber,
    o1.organisationstyp AS betreiber_orgtyp,
    o2.bezeichnung AS eigentuemer,
    o2.organisationstyp AS eigentuemer_orgtyp,
    o3.bezeichnung AS datenherr,
    o3.organisationstyp AS datenherr_orgtyp,
    o4.bezeichnung AS datenlieferant,
    o4.organisationstyp AS datenlieferant_orgtyp,
    k.letzte_aenderung,
    kl.lage AS geom,
    CASE WHEN k.nutzungsart_ist = 'Mischabwasser' THEN '102,0,102'
         WHEN k.nutzungsart_ist IN ('Niederschlagsabwasser', 'Reinabwasser', 'Bachwasser') THEN '0,0,255'
         WHEN k.nutzungsart_ist IN ('Schmutzabwasser', 'Industrieabwasser') THEN '255,0,0'
         WHEN k.nutzungsart_ist = 'entlastetes_Mischabwasser' THEN '0,255,0'
         WHEN k.nutzungsart_ist = 'andere' THEN '255,127,0'
         WHEN k.nutzungsart_ist = 'unbekannt' THEN '165,165,165'
         ELSE '165,165,165'
         END AS color_nutzungsart_ist
  FROM 
    knoten k
    JOIN knoten_lage kl ON k.t_id = kl.t_id
    LEFT JOIN organisation o1 ON k.betreiberref = o1.t_id
    LEFT JOIN organisation o2 ON k.eigentuemerref = o2.t_id
    LEFT JOIN organisation o3 ON k.datenherrref = o3.t_id
    LEFT JOIN organisation o4 ON k.datenlieferantref = o4.t_id;

CREATE VIEW v_vsa_knoten_abwasserknoten AS SELECT
    k.T_Id AS fid,
    k.T_Ili_Tid AS tid,
    k.ara_nr,
    k.baujahr,
    k.baulicherzustand,
    k.bemerkung,
    k.bezeichnung,
    k.deckelkote,
    k.dimension1,
    k.dimension2,
    k.finanzierung,
    k.funktion,
    k.funktionhierarchisch,
    k.lagegenauigkeit,
    k.nutzungsart_geplant,
    k.nutzungsart_ist,
    k.obj_id_abwasserbauwerk,
    k.obj_id_deckel,
    k.rueckstaukote_ist,
    k.sanierungsbedarf,
    k.sohlenkote,
    k.astatus AS status,
    k.symbolori,
    k.zugaenglichkeit,
    k.zustandserhebung_jahr,
    o1.bezeichnung AS betreiber,
    o1.organisationstyp AS betreiber_orgtyp,
    o2.bezeichnung AS eigentuemer,
    o2.organisationstyp AS eigentuemer_orgtyp,
    o3.bezeichnung AS datenherr,
    o3.organisationstyp AS datenherr_orgtyp,
    o4.bezeichnung AS datenlieferant,
    o4.organisationstyp AS datenlieferant_orgtyp,
    k.letzte_aenderung,
    kl.lage AS geom
  FROM 
    knoten k
    JOIN knoten_lage kl ON k.t_id = kl.t_id
    LEFT JOIN organisation o1 ON k.betreiberref = o1.t_id
    LEFT JOIN organisation o2 ON k.eigentuemerref = o2.t_id
    LEFT JOIN organisation o3 ON k.datenherrref = o3.t_id
    LEFT JOIN organisation o4 ON k.datenlieferantref = o4.t_id
WHERE funktion IN ('andere', 'Leitungsknoten', 'seitlicherZugang', 'unbekannt');

-- v_wk_knoten_detailgeometrie source

CREATE VIEW v_vsa_knoten_detailgeometrie AS SELECT 
    k.T_Id AS fid,
    k.T_Ili_Tid AS tid,
    k.dimension1,
    k.dimension2,
    k.finanzierung,
    k.funktion,
    k.funktionhierarchisch,
    k.lagegenauigkeit,
    k.nutzungsart_geplant,
    k.nutzungsart_ist,
    k.obj_id_abwasserbauwerk,
    k.obj_id_deckel,
    k.rueckstaukote_ist,
    k.sanierungsbedarf,
    k.sohlenkote,
    k.astatus AS status,
    k.zugaenglichkeit,
    k.zustandserhebung_jahr,
    o1.bezeichnung AS betreiber,
    o1.organisationstyp AS betreiber_orgtyp,
    o2.bezeichnung AS eigentuemer,
    o2.organisationstyp AS eigentuemer_orgtyp,
    o3.bezeichnung AS datenherr,
    o3.organisationstyp AS datenherr_orgtyp,
    o4.bezeichnung AS datenlieferant,
    o4.organisationstyp AS datenlieferant_orgtyp,
    k.detailgeometrie AS geom
  FROM 
    knoten k
    LEFT JOIN organisation o1 ON k.betreiberref = o1.t_id
    LEFT JOIN organisation o2 ON k.eigentuemerref = o2.t_id
    LEFT JOIN organisation o3 ON k.datenherrref = o3.t_id
    LEFT JOIN organisation o4 ON k.datenlieferantref = o4.t_id
WHERE 
    k.detailgeometrie IS NOT NULL;

-- v_wk_knoten_einleitstelle source

CREATE VIEW v_vsa_knoten_einleitstelle AS SELECT
    k.T_Id AS fid, 
    k.T_Ili_Tid AS tid,
    k.ara_nr,
    k.baujahr,
    k.baulicherzustand,
    k.bemerkung,
    k.bezeichnung,
    k.deckelkote,
    k.dimension1,
    k.dimension2,
    k.finanzierung,
    k.funktion,
    k.funktionhierarchisch,
    k.lagegenauigkeit,
    k.nutzungsart_geplant,
    k.nutzungsart_ist,
    k.obj_id_abwasserbauwerk,
    k.obj_id_deckel,
    k.rueckstaukote_ist,
    k.sanierungsbedarf,
    k.sohlenkote,
    k.astatus AS status,
    k.symbolori,
    k.zugaenglichkeit,
    k.zustandserhebung_jahr,
    o1.bezeichnung AS betreiber,
    o1.organisationstyp AS betreiber_orgtyp,
    o2.bezeichnung AS eigentuemer,
    o2.organisationstyp AS eigentuemer_orgtyp,
    o3.bezeichnung AS datenherr,
    o3.organisationstyp AS datenherr_orgtyp,
    o4.bezeichnung AS datenlieferant,
    o4.organisationstyp AS datenlieferant_orgtyp,
    k.letzte_aenderung,
    kl.lage AS geom
  FROM 
    knoten k
    JOIN knoten_lage kl ON k.t_id = kl.t_id
    LEFT JOIN organisation o1 ON k.betreiberref = o1.t_id
    LEFT JOIN organisation o2 ON k.eigentuemerref = o2.t_id
    LEFT JOIN organisation o3 ON k.datenherrref = o3.t_id
    LEFT JOIN organisation o4 ON k.datenlieferantref = o4.t_id
WHERE funktion IN ('Einleitstelle_gewaesserrelevant', 'Einleitstelle_nicht_gewaesserrelevant');

-- v_wk_knoten_messstelle source

CREATE VIEW v_vsa_knoten_messstelle AS SELECT 
    k.T_Id AS fid,
    k.T_Ili_Tid AS tid,
    k.ara_nr,
    k.baujahr,
    k.baulicherzustand,
    k.bemerkung,
    k.bezeichnung,
    k.deckelkote,
    k.dimension1,
    k.dimension2,
    k.finanzierung,
    k.funktion,
    k.funktionhierarchisch,
    k.lagegenauigkeit,
    k.nutzungsart_geplant,
    k.nutzungsart_ist,
    k.obj_id_abwasserbauwerk,
    k.obj_id_deckel,
    k.rueckstaukote_ist,
    k.sanierungsbedarf,
    k.sohlenkote,
    k.astatus AS status,
    k.symbolori,
    k.zugaenglichkeit,
    k.zustandserhebung_jahr,
    o1.bezeichnung AS betreiber,
    o1.organisationstyp AS betreiber_orgtyp,
    o2.bezeichnung AS eigentuemer,
    o2.organisationstyp AS eigentuemer_orgtyp,
    o3.bezeichnung AS datenherr,
    o3.organisationstyp AS datenherr_orgtyp,
    o4.bezeichnung AS datenlieferant,
    o4.organisationstyp AS datenlieferant_orgtyp,
    k.letzte_aenderung,
    kl.lage AS geom
  FROM 
    knoten k
    JOIN knoten_lage kl ON k.t_id = kl.t_id
    LEFT JOIN organisation o1 ON k.betreiberref = o1.t_id
    LEFT JOIN organisation o2 ON k.eigentuemerref = o2.t_id
    LEFT JOIN organisation o3 ON k.datenherrref = o3.t_id
    LEFT JOIN organisation o4 ON k.datenlieferantref = o4.t_id
WHERE funktion IN ('Messstelle');

-- v_wk_knoten_normschacht source

CREATE VIEW v_vsa_knoten_normschacht AS SELECT
    k.T_Id AS fid,
    k.T_Ili_Tid AS tid,
    k.ara_nr,
    k.baujahr,
    k.baulicherzustand,
    k.bemerkung,
    k.bezeichnung,
    k.deckelkote,
    k.dimension1,
    k.dimension2,
    k.finanzierung,
    k.funktion,
    k.funktionhierarchisch,
    k.lagegenauigkeit,
    k.nutzungsart_geplant,
    k.nutzungsart_ist,
    k.obj_id_abwasserbauwerk,
    k.obj_id_deckel,
    k.rueckstaukote_ist,
    k.sanierungsbedarf,
    k.sohlenkote,
    k.astatus AS status,
    k.symbolori,
    k.zugaenglichkeit,
    k.zustandserhebung_jahr,
    o1.bezeichnung AS betreiber,
    o1.organisationstyp AS betreiber_orgtyp,
    o2.bezeichnung AS eigentuemer,
    o2.organisationstyp AS eigentuemer_orgtyp,
    o3.bezeichnung AS datenherr,
    o3.organisationstyp AS datenherr_orgtyp,
    o4.bezeichnung AS datenlieferant,
    o4.organisationstyp AS datenlieferant_orgtyp,
    k.letzte_aenderung,
    kl.lage AS geom,
    CASE WHEN k.nutzungsart_ist = 'Mischabwasser' THEN '102,0,102'
         WHEN k.nutzungsart_ist IN ('Niederschlagsabwasser', 'Reinabwasser', 'Bachwasser') THEN '0,0,255'
         WHEN k.nutzungsart_ist IN ('Schmutzabwasser', 'Industrieabwasser') THEN '255,0,0'
         WHEN k.nutzungsart_ist = 'entlastetes_Mischabwasser' THEN '0,255,0'
         WHEN k.nutzungsart_ist = 'andere' THEN '255,127,0'
         WHEN k.nutzungsart_ist = 'unbekannt' THEN '165,165,165'
         ELSE '165,165,165'
         END AS color_nutzungsart_ist
  FROM 
    knoten k
    JOIN knoten_lage kl ON k.t_id = kl.t_id
    LEFT JOIN organisation o1 ON k.betreiberref = o1.t_id
    LEFT JOIN organisation o2 ON k.eigentuemerref = o2.t_id
    LEFT JOIN organisation o3 ON k.datenherrref = o3.t_id
    LEFT JOIN organisation o4 ON k.datenlieferantref = o4.t_id
WHERE funktion IN ('Dachwasserschacht', 'Einlaufschacht', 'Entwaesserungsrinne', 'Geleiseschacht', 'Schlammsammler',
                    'Be_Entlueftung', 'Kontrollschacht', 'Oelabscheider',
                    'Schwimmstoffabscheider', 'Spuelschacht', 'Kontroll_Einsteigschacht');

-- v_wk_knoten_spezialbauwerk source

CREATE VIEW v_vsa_knoten_spezialbauwerk AS SELECT
    k.T_Id AS fid,
    k.T_Ili_Tid AS tid,
    k.ara_nr,
    k.baujahr,
    k.baulicherzustand,
    k.bemerkung,
    k.bezeichnung,
    k.deckelkote,
    k.dimension1,
    k.dimension2,
    k.finanzierung,
    k.funktion,
    k.funktionhierarchisch,
    k.lagegenauigkeit,
    k.nutzungsart_geplant,
    k.nutzungsart_ist,
    k.obj_id_abwasserbauwerk,
    k.obj_id_deckel,
    k.rueckstaukote_ist,
    k.sanierungsbedarf,
    k.sohlenkote,
    k.astatus AS status,
    k.symbolori,
    k.zugaenglichkeit,
    k.zustandserhebung_jahr,
    o1.bezeichnung AS betreiber,
    o1.organisationstyp AS betreiber_orgtyp,
    o2.bezeichnung AS eigentuemer,
    o2.organisationstyp AS eigentuemer_orgtyp,
    o3.bezeichnung AS datenherr,
    o3.organisationstyp AS datenherr_orgtyp,
    o4.bezeichnung AS datenlieferant,
    o4.organisationstyp AS datenlieferant_orgtyp,
    k.letzte_aenderung,
    kl.lage AS geom
  FROM 
    knoten k
    JOIN knoten_lage kl ON k.t_id = kl.t_id
    LEFT JOIN organisation o1 ON k.betreiberref = o1.t_id
    LEFT JOIN organisation o2 ON k.eigentuemerref = o2.t_id
    LEFT JOIN organisation o3 ON k.datenherrref = o3.t_id
    LEFT JOIN organisation o4 ON k.datenlieferantref = o4.t_id
WHERE funktion IN ('abflussloseGrube', 'Absturzbauwerk', 'Abwasserfaulraum',
                                  'Duekerkammer', 'Duekeroberhaupt', 'Faulgrube', 'Gelaendemulde', 'Geschiebefang',
                                  'Guellegrube', 'Klaergrube', 'Regenbecken_Durchlaufbecken', 'Regenbecken_Fangbecken',
                                  'Regenbecken_Fangkanal', 'Regenbecken_Regenklaerbecken',
                                  'Regenbecken_Regenrueckhaltebecken', 'Regenbecken_Regenrueckhaltekanal',
                                  'Regenbecken_Stauraumkanal', 'Regenbecken_Verbundbecken', 'Wirbelfallschacht', 
                                  'Pumpwerk', 'Trennbauwerk', 'Regenueberlauf');

-- v_wk_knoten_text source

CREATE VIEW v_vsa_knoten_text AS SELECT 
    k.T_Id AS fid,
    k.T_Ili_Tid AS tid,
    k.nutzungsart_ist,
    k.funktion,
    k.finanzierung,
    k.funktionhierarchisch,
    k.baulicherzustand,
    k.astatus AS status,
    k.bezeichnung,
    kt.textpos AS geom,
    [REPLACE](kt.textinhalt, '#x', '\n') AS textinhalt,
    kt.textori - 90 AS textori,
    kt.texthali,
    kt.textvali,
    CASE WHEN k.nutzungsart_ist = 'Mischabwasser' THEN '102,0,102' WHEN k.nutzungsart_ist IN ('Niederschlagsabwasser', 'Reinabwasser', 'Bachwasser') THEN '0,0,255' WHEN k.nutzungsart_ist IN ('Schmutzabwasser', 'Industrieabwasser') THEN '255,0,0' WHEN k.nutzungsart_ist = 'entlastetes_Mischabwasser' THEN '0,255,0' WHEN k.nutzungsart_ist = 'andere' THEN '255,127,0' WHEN k.nutzungsart_ist = 'unbekannt' THEN '165,165,165' ELSE '165,165,165' END AS color_nutzungsart_ist
FROM knoten k
   JOIN knoten_text kt ON kt.knotenref = k.t_id;

-- v_wk_knoten_versickerungsanlage source

CREATE VIEW v_vsa_knoten_versickerungsanlage AS SELECT
    k.T_Id AS fid,
    k.T_Ili_Tid AS tid,
    k.ara_nr,
    k.baujahr,
    k.baulicherzustand,
    k.bemerkung,
    k.bezeichnung,
    k.deckelkote,
    k.dimension1,
    k.dimension2,
    k.finanzierung,
    k.funktion,
    k.funktionhierarchisch,
    k.lagegenauigkeit,
    k.nutzungsart_geplant,
    k.nutzungsart_ist,
    k.obj_id_abwasserbauwerk,
    k.obj_id_deckel,
    k.rueckstaukote_ist,
    k.sanierungsbedarf,
    k.sohlenkote,
    k.astatus AS status,
    k.symbolori,
    k.zugaenglichkeit,
    k.zustandserhebung_jahr,
    o1.bezeichnung AS betreiber,
    o1.organisationstyp AS betreiber_orgtyp,
    o2.bezeichnung AS eigentuemer,
    o2.organisationstyp AS eigentuemer_orgtyp,
    o3.bezeichnung AS datenherr,
    o3.organisationstyp AS datenherr_orgtyp,
    o4.bezeichnung AS datenlieferant,
    o4.organisationstyp AS datenlieferant_orgtyp,
    k.letzte_aenderung,
    kl.lage AS geom
  FROM 
    knoten k
    JOIN knoten_lage kl ON k.t_id = kl.t_id
    LEFT JOIN organisation o1 ON k.betreiberref = o1.t_id
    LEFT JOIN organisation o2 ON k.eigentuemerref = o2.t_id
    LEFT JOIN organisation o3 ON k.datenherrref = o3.t_id
    LEFT JOIN organisation o4 ON k.datenlieferantref = o4.t_id
WHERE funktion IN ('Versickerungsanlage');

-- v_wk_leitung source

CREATE VIEW v_vsa_leitung AS SELECT
    h.T_Id AS fid,
    h.T_Ili_Tid AS tid,
    h.baujahr,
    h.baulicherzustand,
    h.bemerkung,
    h.bezeichnung,
    h.finanzierung,
    h.funktionhierarchisch,
    h.funktionhydraulisch,
    h.hoehengenauigkeit_nach,
    h.hoehengenauigkeit_von,
    h.hydr_belastung_ist,
    h.kote_nach,
    h.kote_von,
    h.laengeeffektiv,
    h.lagebestimmung,
    h.leckschutz,
    h.lichte_breite,
    h.lichte_hoehe,
    h.material,
    h.nutzungsart_geplant,
    h.nutzungsart_ist,
    h.obj_id_abwasserbauwerk,
    h.obj_id_nachhaltungspunkt,
    h.obj_id_vonhaltungspunkt,
    h.profiltyp,
    h.reliner_art,
    h.reliner_nennweite,
    h.sanierungsbedarf,
    h.astatus AS status,
    h.wandrauhigkeit,
    h.wbw_basisjahr,
    h.wbw_bauart,
    h.wiederbeschaffungswert,
    h.zustandserhebung_jahr,
    o1.bezeichnung AS betreiber,
    o1.organisationstyp AS betreiber_orgtyp,
    o2.bezeichnung AS eigentuemer,
    o2.organisationstyp AS eigentuemer_orgtyp,
    h.knoten_nachref,
    h.knoten_vonref,
    h.leitung_nachref,
    h.rohrprofilref,
    o3.bezeichnung AS datenherr,
    o3.organisationstyp AS datenherr_orgtyp,
    o4.bezeichnung AS datenlieferant,
    o4.organisationstyp AS datenlieferant_orgtyp,
    h.letzte_aenderung,
    h.verlauf AS geom
FROM 
    leitung h
    LEFT JOIN organisation o1 ON h.betreiberref = o1.t_id
    LEFT JOIN organisation o2 ON h.eigentuemerref = o2.t_id
    LEFT JOIN organisation o3 ON h.datenherrref = o3.t_id
    LEFT JOIN organisation o4 ON h.datenlieferantref = o4.t_id;

-- v_wk_leitung_text source

CREATE VIEW v_vsa_leitung_text AS SELECT 
    h.T_Id AS fid,
    h.T_Ili_Tid AS tid,
    h.nutzungsart_ist,
    h.profiltyp,
    h.funktionhierarchisch,
    h.funktionhydraulisch,
    h.finanzierung,
    h.baulicherzustand,
    h.astatus AS status,
    h.bezeichnung,
    h.lichte_hoehe,
    h.material,
    ht.textpos AS geom,
    [REPLACE](ht.textinhalt, '#x', '\n') AS textinhalt,
    ht.textori - 90 AS textori,
    ht.texthali,
    ht.textvali,
    CASE WHEN h.nutzungsart_ist = 'Mischabwasser' THEN '102,0,102' WHEN h.nutzungsart_ist IN ('Niederschlagsabwasser', 'Reinabwasser', 'Bachwasser') THEN '0,0,255' WHEN h.nutzungsart_ist IN ('Schmutzabwasser', 'Industrieabwasser') THEN '255,0,0' WHEN h.nutzungsart_ist = 'entlastetes_Mischabwasser' THEN '0,255,0' WHEN h.nutzungsart_ist = 'andere' THEN '255,127,0' WHEN h.nutzungsart_ist = 'unbekannt' THEN '165,165,165' ELSE '165,165,165' END AS color_nutzungsart_ist
FROM leitung h
   JOIN leitung_text ht ON ht.leitungref = h.t_id;

-- v_wk_ueberlauf_foerderaggregat source

CREATE VIEW v_vsa_ueberlauf_foerderaggregat AS SELECT
    ua.T_Id AS fid,
    ua.T_Ili_Tid AS tid,
    ua.art,
    ua.bezeichnung,
    ua.knotenref,
    ua.knoten_nachref,
    ua.datenherrref,
    ua.datenlieferantref,
    ua.letzte_aenderung,
    k.ara_nr,
    k.baujahr,
    k.baulicherzustand,
    k.bemerkung,
    k.bezeichnung AS bezeichnung_knoten,
    k.deckelkote,
    k.dimension1,
    k.dimension2,
    k.finanzierung,
    k.funktion,
    k.funktionhierarchisch,
    k.lagegenauigkeit,
    k.nutzungsart_geplant,
    k.nutzungsart_ist,
    k.obj_id_abwasserbauwerk,
    k.obj_id_deckel,
    k.rueckstaukote_ist,
    k.sanierungsbedarf,
    k.sohlenkote,
    k.astatus AS status,
    k.symbolori,
    k.zugaenglichkeit,
    k.zustandserhebung_jahr,
    o1.bezeichnung AS betreiber,
    o1.organisationstyp AS betreiber_orgtyp,
    o2.bezeichnung AS eigentuemer,
    o2.organisationstyp AS eigentuemer_orgtyp,
    o3.bezeichnung AS datenherr,
    o3.organisationstyp AS datenherr_orgtyp,
    o4.bezeichnung AS datenlieferant,
    o4.organisationstyp AS datenlieferant_orgtyp,
    k.letzte_aenderung AS letzte_aenderung_knoten,
    kl.lage AS geom,
    CASE WHEN k.nutzungsart_ist = 'Mischabwasser' THEN '102,0,102'
         WHEN k.nutzungsart_ist IN ('Niederschlagsabwasser', 'Reinabwasser', 'Bachwasser') THEN '0,0,255'
         WHEN k.nutzungsart_ist IN ('Schmutzabwasser', 'Industrieabwasser') THEN '255,0,0'
         WHEN k.nutzungsart_ist = 'entlastetes_Mischabwasser' THEN '0,255,0'
         WHEN k.nutzungsart_ist = 'andere' THEN '255,127,0'
         WHEN k.nutzungsart_ist = 'unbekannt' THEN '165,165,165'
         ELSE '165,165,165'
         END AS color_nutzungsart_ist
  FROM
    ueberlauf_foerderaggregat ua
    JOIN knoten k ON ua.knotenref = k.t_id
    JOIN knoten_lage kl ON k.t_id = kl.t_id
    LEFT JOIN organisation o1 ON k.betreiberref = o1.t_id
    LEFT JOIN organisation o2 ON k.eigentuemerref = o2.t_id
    LEFT JOIN organisation o3 ON k.datenherrref = o3.t_id
    LEFT JOIN organisation o4 ON k.datenlieferantref = o4.t_id;

CREATE VIEW v_errorlist_ueberlauf_foerderaggregat_data AS
    SELECT *
      FROM ca_error_data e
     WHERE e.class LIKE 'Ueberlauf_Foerderaggregat';


CREATE VIEW v_errorlist_error_teileinzugsgebiet_data AS
    SELECT *
      FROM ca_error_data e
     WHERE e.class LIKE 'Teileinzugsgebiet';

CREATE VIEW v_errorlist_error_knoten_data AS
    SELECT *
      FROM ca_error_data e
     WHERE e.class LIKE 'Knoten';

CREATE VIEW v_errorlist_error_haltung_data AS
    SELECT *
      FROM ca_error_data e
     WHERE e.class LIKE 'Leitung';


CREATE VIEW v_error_ueberlauf_foerderaggregat AS SELECT 
u.T_Id AS fid, 
e.tid,
           e.class,
           e.count_error,
           e.wk_max,
           e.gep_max,
           n.funktion,
           n.funktionhierarchisch,
           o.bezeichnung AS eigentuemer,
           k.lage AS geom
      FROM ca_error_object e
           JOIN ueberlauf_foerderaggregat u ON e.tid = u.t_ili_tid
           JOIN knoten n ON u.knotenref = n.t_id
           JOIN knoten_lage k ON n.t_id = k.t_id
           LEFT JOIN organisation o ON n.eigentuemerref = o.t_id
     WHERE e.class LIKE 'Ueberlauf_Foerderaggregat';

CREATE VIEW v_error_teileinzugsgebiet AS SELECT
t.T_Id AS fid, 
 e.tid,
           e.class,
           e.count_error,
           e.wk_max,
           e.gep_max,
           t.perimeter AS geom
      FROM ca_error_object e
           JOIN teileinzugsgebiet t ON e.tid = t.t_ili_tid
     WHERE e.class LIKE 'Teileinzugsgebiet';

-- v_error_sk_trennbauwerk source

CREATE VIEW v_error_sk_trennbauwerk AS SELECT 
sk.T_Id AS fid,
e.tid,
           e.class,
           e.count_error,
           e.wk_max,
           e.gep_max,
           o.bezeichnung AS eigentuemer,
           k.lage AS geom
      FROM ca_error_object e
          JOIN sk_trennbauwerk sk ON e.tid = sk.t_ili_tid
          JOIN knoten n ON sk.paa_knotenref = n.t_id
          JOIN knoten_lage k ON n.t_id = k.t_id
          LEFT JOIN organisation o ON n.eigentuemerref = o.t_id
     WHERE e.class LIKE 'SK_Trennbauwerk';

-- v_error_sk_regenrueckhaltebecken_kanal source

CREATE VIEW v_error_sk_regenrueckhaltebecken_kanal AS SELECT
sk.T_Id AS fid,
e.tid,
           e.class,
           e.count_error,
           e.wk_max,
           e.gep_max,
           o.bezeichnung AS eigentuemer,
           k.lage AS geom
      FROM ca_error_object e
          JOIN sk_regenrueckhaltebecken_kanal sk ON e.tid = sk.t_ili_tid
          JOIN knoten n ON sk.paa_knotenref = n.t_id
          JOIN knoten_lage k ON n.t_id = k.t_id
          LEFT JOIN organisation o ON n.eigentuemerref = o.t_id
     WHERE e.class LIKE 'SK_regenrueckhaltebecken_kanal';

CREATE VIEW v_error_sk_regenueberlaufbecken AS SELECT 
sk.T_Id AS fid,
e.tid,
           e.class,
           e.count_error,
           e.wk_max,
           e.gep_max,
           o.bezeichnung AS eigentuemer,
           k.lage AS geom
      FROM ca_error_object e
          JOIN sk_regenueberlaufbecken sk ON e.tid = sk.t_ili_tid
          JOIN knoten n ON sk.paa_knotenref = n.t_id
          JOIN knoten_lage k ON n.t_id = k.t_id
          LEFT JOIN organisation o ON n.eigentuemerref = o.t_id
     WHERE e.class LIKE 'SK_Regenueberlaufbecken';

CREATE VIEW v_error_sk_regenueberlauf AS SELECT 
sk.T_Id AS fid,
e.tid,
           e.class,
           e.count_error,
           e.wk_max,
           e.gep_max,
           o.bezeichnung AS eigentuemer,
           k.lage AS geom
      FROM ca_error_object e
          JOIN sk_regenueberlauf sk ON e.tid = sk.t_ili_tid
          JOIN knoten n ON sk.paa_knotenref = n.t_id
          JOIN knoten_lage k ON n.t_id = k.t_id
          LEFT JOIN organisation o ON n.eigentuemerref = o.t_id
     WHERE e.class LIKE 'SK_Regenueberlauf';

CREATE VIEW v_error_sk_pumpwerk AS SELECT
sk.T_Id AS fid,
 e.tid,
           e.class,
           e.count_error,
           e.wk_max,
           e.gep_max,
           o.bezeichnung AS eigentuemer,
           k.lage AS geom
      FROM ca_error_object e
          JOIN sk_pumpwerk sk ON e.tid = sk.t_ili_tid
          JOIN knoten n ON sk.paa_knotenref = n.t_id
          JOIN knoten_lage k ON n.t_id = k.t_id
          LEFT JOIN organisation o ON n.eigentuemerref = o.t_id
     WHERE e.class LIKE 'SK_Pumpwerk';

CREATE VIEW v_error_sk_einleitstelle AS SELECT 
sk.T_Id AS fid,
e.tid,
           e.class,
           e.count_error,
           e.wk_max,
           e.gep_max,
           o.bezeichnung AS eigentuemer,
           k.lage AS geom
      FROM ca_error_object e
          JOIN sk_einleitstelle sk ON e.tid = sk.t_ili_tid
          JOIN knoten n ON sk.paa_knotenref = n.t_id
          JOIN knoten_lage k ON n.t_id = k.t_id
          LEFT JOIN organisation o ON n.eigentuemerref = o.t_id
     WHERE e.class LIKE 'SK_Einleitstelle';

CREATE VIEW v_error_recommendation_teileinzugsgebiet AS SELECT 
t.T_Id AS fid, 
t.t_ili_tid AS tid,
           e.recommendation,
           e.recommendation_detail,
           t.perimeter AS geom
      FROM ca_error_data e
           JOIN
           teileinzugsgebiet t ON e.tid = t.t_ili_tid
     WHERE e.class LIKE 'Teileinzugsgebiet'
     GROUP BY t.t_ili_tid,
              e.recommendation;

CREATE VIEW v_error_recommendation_knoten AS SELECT 
n.T_Id AS fid, 
n.t_ili_tid AS tid,
           e.recommendation,
           e.recommendation_detail,
           k.lage AS geom
      FROM ca_error_data e
           JOIN knoten n ON e.tid = n.t_ili_tid
           JOIN knoten_lage k ON n.t_id = k.t_id
     WHERE e.class LIKE 'Knoten'
     GROUP BY n.t_ili_tid,
              e.recommendation;

CREATE VIEW v_error_recommendation_haltung AS SELECT 
h.T_Id AS fid, 
h.t_ili_tid AS tid,
           e.recommendation,
           e.recommendation_detail,
           h.verlauf AS geom
      FROM ca_error_data e
           JOIN
           leitung h ON e.tid = h.t_ili_tid
     WHERE e.class LIKE 'leitung'
     GROUP BY h.t_ili_tid,
              e.recommendation;

CREATE VIEW v_error_knoten AS SELECT 
n.T_Id AS fid, 
e.tid,
           e.class,
           e.count_error,
           e.wk_max,
           e.gep_max,
           n.funktion,
           n.funktionhierarchisch,
           o.bezeichnung AS eigentuemer,
           k.lage AS geom
      FROM ca_error_object e
           JOIN knoten n ON e.tid = n.t_ili_tid
           JOIN knoten_lage k ON n.t_id = k.t_id
           LEFT JOIN organisation o ON n.eigentuemerref = o.t_id
     WHERE e.class LIKE 'Knoten';

CREATE VIEW v_error_haltung AS SELECT 
h.T_Id AS fid, 
e.tid,
           e.class,
           e.count_error,
           e.wk_max,
           e.gep_max,
           h.funktionhierarchisch,
           o.bezeichnung AS eigentuemer,
           h.verlauf AS geom
      FROM ca_error_object e
           JOIN leitung h ON e.tid = h.t_ili_tid
           LEFT JOIN organisation o ON h.eigentuemerref = o.t_id
     WHERE e.class LIKE 'Leitung';

CREATE VIEW v_error_error_knoten AS SELECT 
n.T_Id AS fid, 
n.t_ili_tid AS tid,
           e.error,
           k.lage AS geom
      FROM ca_error_data e
           JOIN
           knoten n ON e.tid = n.t_ili_tid
           JOIN
           knoten_lage k ON n.t_id = k.t_id
     WHERE e.class LIKE 'Knoten'
     GROUP BY n.t_ili_tid,
              e.error;

CREATE VIEW v_error_error_haltung AS SELECT
h.T_Id AS fid,  
h.t_ili_tid AS tid,
           e.error,
           h.verlauf AS geom
      FROM ca_error_data e
           JOIN
           leitung h ON e.tid = h.t_ili_tid
     WHERE e.class LIKE 'leitung'
     GROUP BY h.t_ili_tid,
              e.error;

CREATE VIEW v_error_category_knoten AS SELECT
n.T_Id AS fid, 
n.t_ili_tid AS tid,
           e.category,
           k.lage AS geom
      FROM ca_error_data e
           JOIN
           knoten n ON e.tid = n.t_ili_tid
           JOIN
           knoten_lage k ON n.t_id = k.t_id
     WHERE e.class LIKE 'Knoten'
     GROUP BY n.t_ili_tid,
              e.category;

-- v_error_category_haltung source

CREATE VIEW v_error_category_haltung AS SELECT
h.T_Id AS fid, 
h.t_ili_tid AS tid,
           e.category,
           h.verlauf AS geom
      FROM ca_error_data e
           JOIN
           leitung h ON e.tid = h.t_ili_tid
     WHERE e.class LIKE 'leitung'
     GROUP BY h.t_ili_tid,
              e.category;
