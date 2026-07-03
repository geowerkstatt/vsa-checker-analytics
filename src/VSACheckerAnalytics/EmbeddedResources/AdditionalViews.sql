CREATE VIEW v_vsa_knoten AS SELECT
    k.T_Id AS fid,
    k.T_Ili_Tid AS tid,
    k.ara_nr AS wwtp_no,
    k.baujahr AS year_of_construction,
    k.baulicherzustand AS structure_condition,
    k.bemerkung AS remark,
    k.bezeichnung AS identifier,
    k.deckelkote AS cover_level,
    k.dimension1,
    k.dimension2,
    k.finanzierung AS financing,
    k.funktion AS function,
    k.funktionhierarchisch AS function_hierarchic,
    k.lagegenauigkeit AS positional_accuracy,
    k.nutzungsart_geplant AS usage_planned,
    k.nutzungsart_ist AS usage_current,
    k.obj_id_abwasserbauwerk AS fk_wastewater_structure,
    k.obj_id_deckel AS fk_cover,
    k.rueckstaukote_ist AS backflow_level,
    k.sanierungsbedarf AS renovation_necessity,
    k.sohlenkote AS bottom_level,
    k.astatus AS status,
    k.symbolori,
    k.zugaenglichkeit AS accessibility,
    k.zustandserhebung_jahr AS condition_survey_year,
    o1.bezeichnung AS operator,
    o1.organisationstyp AS operator_org_type,
    o2.bezeichnung AS owner,
    o2.organisationstyp AS owner_org_type,
    o3.bezeichnung AS data_owner,
    o3.organisationstyp AS data_owner_org_type,
    o4.bezeichnung AS data_provider,
    o4.organisationstyp AS data_provider_org_type,
    k.letzte_aenderung AS last_modification,
    kl.lage AS geom,
    CASE WHEN k.nutzungsart_ist = 'Mischabwasser' THEN '102,0,102'
         WHEN k.nutzungsart_ist IN ('Niederschlagsabwasser', 'Reinabwasser', 'Bachwasser') THEN '0,0,255'
         WHEN k.nutzungsart_ist IN ('Schmutzabwasser', 'Industrieabwasser') THEN '255,0,0'
         WHEN k.nutzungsart_ist = 'entlastetes_Mischabwasser' THEN '0,255,0'
         WHEN k.nutzungsart_ist = 'andere' THEN '255,127,0'
         WHEN k.nutzungsart_ist = 'unbekannt' THEN '165,165,165'
         ELSE '165,165,165'
         END AS color_usage_current
  FROM
    knoten k
    JOIN knoten_lage kl ON k.t_id = kl.t_id
    LEFT JOIN organisation o1 ON k.betreiberref = o1.t_id
    LEFT JOIN organisation o2 ON k.eigentuemerref = o2.t_id
    LEFT JOIN organisation o3 ON k.datenherrref = o3.t_id
    LEFT JOIN organisation o4 ON k.datenlieferantref = o4.t_id;
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_vsa_knoten', 'features', 'v_vsa_knoten', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_vsa_knoten', 'geom', 'POINT', 2056, 0, 0);

CREATE VIEW v_vsa_knoten_abwasserknoten AS SELECT
    k.T_Id AS fid,
    k.T_Ili_Tid AS tid,
    k.ara_nr AS wwtp_no,
    k.baujahr AS year_of_construction,
    k.baulicherzustand AS structure_condition,
    k.bemerkung AS remark,
    k.bezeichnung AS identifier,
    k.deckelkote AS cover_level,
    k.dimension1,
    k.dimension2,
    k.finanzierung AS financing,
    k.funktion AS function,
    k.funktionhierarchisch AS function_hierarchic,
    k.lagegenauigkeit AS positional_accuracy,
    k.nutzungsart_geplant AS usage_planned,
    k.nutzungsart_ist AS usage_current,
    k.obj_id_abwasserbauwerk AS fk_wastewater_structure,
    k.obj_id_deckel AS fk_cover,
    k.rueckstaukote_ist AS backflow_level,
    k.sanierungsbedarf AS renovation_necessity,
    k.sohlenkote AS bottom_level,
    k.astatus AS status,
    k.symbolori,
    k.zugaenglichkeit AS accessibility,
    k.zustandserhebung_jahr AS condition_survey_year,
    o1.bezeichnung AS operator,
    o1.organisationstyp AS operator_org_type,
    o2.bezeichnung AS owner,
    o2.organisationstyp AS owner_org_type,
    o3.bezeichnung AS data_owner,
    o3.organisationstyp AS data_owner_org_type,
    o4.bezeichnung AS data_provider,
    o4.organisationstyp AS data_provider_org_type,
    k.letzte_aenderung AS last_modification,
    kl.lage AS geom
  FROM
    knoten k
    JOIN knoten_lage kl ON k.t_id = kl.t_id
    LEFT JOIN organisation o1 ON k.betreiberref = o1.t_id
    LEFT JOIN organisation o2 ON k.eigentuemerref = o2.t_id
    LEFT JOIN organisation o3 ON k.datenherrref = o3.t_id
    LEFT JOIN organisation o4 ON k.datenlieferantref = o4.t_id
WHERE funktion IN ('andere', 'Leitungsknoten', 'seitlicherZugang', 'unbekannt');
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_vsa_knoten_abwasserknoten', 'features', 'v_vsa_knoten_abwasserknoten', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_vsa_knoten_abwasserknoten', 'geom', 'POINT', 2056, 0, 0);

-- v_wk_knoten_detailgeometrie source

CREATE VIEW v_vsa_knoten_detailgeometrie AS SELECT
    k.T_Id AS fid,
    k.T_Ili_Tid AS tid,
    k.dimension1,
    k.dimension2,
    k.finanzierung AS financing,
    k.funktion AS function,
    k.funktionhierarchisch AS function_hierarchic,
    k.lagegenauigkeit AS positional_accuracy,
    k.nutzungsart_geplant AS usage_planned,
    k.nutzungsart_ist AS usage_current,
    k.obj_id_abwasserbauwerk AS fk_wastewater_structure,
    k.obj_id_deckel AS fk_cover,
    k.rueckstaukote_ist AS backflow_level,
    k.sanierungsbedarf AS renovation_necessity,
    k.sohlenkote AS bottom_level,
    k.astatus AS status,
    k.zugaenglichkeit AS accessibility,
    k.zustandserhebung_jahr AS condition_survey_year,
    o1.bezeichnung AS operator,
    o1.organisationstyp AS operator_org_type,
    o2.bezeichnung AS owner,
    o2.organisationstyp AS owner_org_type,
    o3.bezeichnung AS data_owner,
    o3.organisationstyp AS data_owner_org_type,
    o4.bezeichnung AS data_provider,
    o4.organisationstyp AS data_provider_org_type,
    k.detailgeometrie AS geom
  FROM
    knoten k
    LEFT JOIN organisation o1 ON k.betreiberref = o1.t_id
    LEFT JOIN organisation o2 ON k.eigentuemerref = o2.t_id
    LEFT JOIN organisation o3 ON k.datenherrref = o3.t_id
    LEFT JOIN organisation o4 ON k.datenlieferantref = o4.t_id
WHERE
    k.detailgeometrie IS NOT NULL;
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_vsa_knoten_detailgeometrie', 'features', 'v_vsa_knoten_detailgeometrie', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_vsa_knoten_detailgeometrie', 'geom', 'POINT', 2056, 0, 0);

-- v_wk_knoten_einleitstelle source

CREATE VIEW v_vsa_knoten_einleitstelle AS SELECT
    k.T_Id AS fid,
    k.T_Ili_Tid AS tid,
    k.ara_nr AS wwtp_no,
    k.baujahr AS year_of_construction,
    k.baulicherzustand AS structure_condition,
    k.bemerkung AS remark,
    k.bezeichnung AS identifier,
    k.deckelkote AS cover_level,
    k.dimension1,
    k.dimension2,
    k.finanzierung AS financing,
    k.funktion AS function,
    k.funktionhierarchisch AS function_hierarchic,
    k.lagegenauigkeit AS positional_accuracy,
    k.nutzungsart_geplant AS usage_planned,
    k.nutzungsart_ist AS usage_current,
    k.obj_id_abwasserbauwerk AS fk_wastewater_structure,
    k.obj_id_deckel AS fk_cover,
    k.rueckstaukote_ist AS backflow_level,
    k.sanierungsbedarf AS renovation_necessity,
    k.sohlenkote AS bottom_level,
    k.astatus AS status,
    k.symbolori,
    k.zugaenglichkeit AS accessibility,
    k.zustandserhebung_jahr AS condition_survey_year,
    o1.bezeichnung AS operator,
    o1.organisationstyp AS operator_org_type,
    o2.bezeichnung AS owner,
    o2.organisationstyp AS owner_org_type,
    o3.bezeichnung AS data_owner,
    o3.organisationstyp AS data_owner_org_type,
    o4.bezeichnung AS data_provider,
    o4.organisationstyp AS data_provider_org_type,
    k.letzte_aenderung AS last_modification,
    kl.lage AS geom
  FROM
    knoten k
    JOIN knoten_lage kl ON k.t_id = kl.t_id
    LEFT JOIN organisation o1 ON k.betreiberref = o1.t_id
    LEFT JOIN organisation o2 ON k.eigentuemerref = o2.t_id
    LEFT JOIN organisation o3 ON k.datenherrref = o3.t_id
    LEFT JOIN organisation o4 ON k.datenlieferantref = o4.t_id
WHERE funktion IN ('Einleitstelle_gewaesserrelevant', 'Einleitstelle_nicht_gewaesserrelevant');
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_vsa_knoten_einleitstelle', 'features', 'v_vsa_knoten_einleitstelle', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_vsa_knoten_einleitstelle', 'geom', 'POINT', 2056, 0, 0);

-- v_wk_knoten_messstelle source

CREATE VIEW v_vsa_knoten_messstelle AS SELECT
    k.T_Id AS fid,
    k.T_Ili_Tid AS tid,
    k.ara_nr AS wwtp_no,
    k.baujahr AS year_of_construction,
    k.baulicherzustand AS structure_condition,
    k.bemerkung AS remark,
    k.bezeichnung AS identifier,
    k.deckelkote AS cover_level,
    k.dimension1,
    k.dimension2,
    k.finanzierung AS financing,
    k.funktion AS function,
    k.funktionhierarchisch AS function_hierarchic,
    k.lagegenauigkeit AS positional_accuracy,
    k.nutzungsart_geplant AS usage_planned,
    k.nutzungsart_ist AS usage_current,
    k.obj_id_abwasserbauwerk AS fk_wastewater_structure,
    k.obj_id_deckel AS fk_cover,
    k.rueckstaukote_ist AS backflow_level,
    k.sanierungsbedarf AS renovation_necessity,
    k.sohlenkote AS bottom_level,
    k.astatus AS status,
    k.symbolori,
    k.zugaenglichkeit AS accessibility,
    k.zustandserhebung_jahr AS condition_survey_year,
    o1.bezeichnung AS operator,
    o1.organisationstyp AS operator_org_type,
    o2.bezeichnung AS owner,
    o2.organisationstyp AS owner_org_type,
    o3.bezeichnung AS data_owner,
    o3.organisationstyp AS data_owner_org_type,
    o4.bezeichnung AS data_provider,
    o4.organisationstyp AS data_provider_org_type,
    k.letzte_aenderung AS last_modification,
    kl.lage AS geom
  FROM
    knoten k
    JOIN knoten_lage kl ON k.t_id = kl.t_id
    LEFT JOIN organisation o1 ON k.betreiberref = o1.t_id
    LEFT JOIN organisation o2 ON k.eigentuemerref = o2.t_id
    LEFT JOIN organisation o3 ON k.datenherrref = o3.t_id
    LEFT JOIN organisation o4 ON k.datenlieferantref = o4.t_id
WHERE funktion IN ('Messstelle');
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_vsa_knoten_messstelle', 'features', 'v_vsa_knoten_messstelle', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_vsa_knoten_messstelle', 'geom', 'POINT', 2056, 0, 0);

-- v_wk_knoten_normschacht source

CREATE VIEW v_vsa_knoten_normschacht AS SELECT
    k.T_Id AS fid,
    k.T_Ili_Tid AS tid,
    k.ara_nr AS wwtp_no,
    k.baujahr AS year_of_construction,
    k.baulicherzustand AS structure_condition,
    k.bemerkung AS remark,
    k.bezeichnung AS identifier,
    k.deckelkote AS cover_level,
    k.dimension1,
    k.dimension2,
    k.finanzierung AS financing,
    k.funktion AS function,
    k.funktionhierarchisch AS function_hierarchic,
    k.lagegenauigkeit AS positional_accuracy,
    k.nutzungsart_geplant AS usage_planned,
    k.nutzungsart_ist AS usage_current,
    k.obj_id_abwasserbauwerk AS fk_wastewater_structure,
    k.obj_id_deckel AS fk_cover,
    k.rueckstaukote_ist AS backflow_level,
    k.sanierungsbedarf AS renovation_necessity,
    k.sohlenkote AS bottom_level,
    k.astatus AS status,
    k.symbolori,
    k.zugaenglichkeit AS accessibility,
    k.zustandserhebung_jahr AS condition_survey_year,
    o1.bezeichnung AS operator,
    o1.organisationstyp AS operator_org_type,
    o2.bezeichnung AS owner,
    o2.organisationstyp AS owner_org_type,
    o3.bezeichnung AS data_owner,
    o3.organisationstyp AS data_owner_org_type,
    o4.bezeichnung AS data_provider,
    o4.organisationstyp AS data_provider_org_type,
    k.letzte_aenderung AS last_modification,
    kl.lage AS geom,
    CASE WHEN k.nutzungsart_ist = 'Mischabwasser' THEN '102,0,102'
         WHEN k.nutzungsart_ist IN ('Niederschlagsabwasser', 'Reinabwasser', 'Bachwasser') THEN '0,0,255'
         WHEN k.nutzungsart_ist IN ('Schmutzabwasser', 'Industrieabwasser') THEN '255,0,0'
         WHEN k.nutzungsart_ist = 'entlastetes_Mischabwasser' THEN '0,255,0'
         WHEN k.nutzungsart_ist = 'andere' THEN '255,127,0'
         WHEN k.nutzungsart_ist = 'unbekannt' THEN '165,165,165'
         ELSE '165,165,165'
         END AS color_usage_current
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
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_vsa_knoten_normschacht', 'features', 'v_vsa_knoten_normschacht', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_vsa_knoten_normschacht', 'geom', 'POINT', 2056, 0, 0);

-- v_wk_knoten_spezialbauwerk source

CREATE VIEW v_vsa_knoten_spezialbauwerk AS SELECT
    k.T_Id AS fid,
    k.T_Ili_Tid AS tid,
    k.ara_nr AS wwtp_no,
    k.baujahr AS year_of_construction,
    k.baulicherzustand AS structure_condition,
    k.bemerkung AS remark,
    k.bezeichnung AS identifier,
    k.deckelkote AS cover_level,
    k.dimension1,
    k.dimension2,
    k.finanzierung AS financing,
    k.funktion AS function,
    k.funktionhierarchisch AS function_hierarchic,
    k.lagegenauigkeit AS positional_accuracy,
    k.nutzungsart_geplant AS usage_planned,
    k.nutzungsart_ist AS usage_current,
    k.obj_id_abwasserbauwerk AS fk_wastewater_structure,
    k.obj_id_deckel AS fk_cover,
    k.rueckstaukote_ist AS backflow_level,
    k.sanierungsbedarf AS renovation_necessity,
    k.sohlenkote AS bottom_level,
    k.astatus AS status,
    k.symbolori,
    k.zugaenglichkeit AS accessibility,
    k.zustandserhebung_jahr AS condition_survey_year,
    o1.bezeichnung AS operator,
    o1.organisationstyp AS operator_org_type,
    o2.bezeichnung AS owner,
    o2.organisationstyp AS owner_org_type,
    o3.bezeichnung AS data_owner,
    o3.organisationstyp AS data_owner_org_type,
    o4.bezeichnung AS data_provider,
    o4.organisationstyp AS data_provider_org_type,
    k.letzte_aenderung AS last_modification,
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
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_vsa_knoten_spezialbauwerk', 'features', 'v_vsa_knoten_spezialbauwerk', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_vsa_knoten_spezialbauwerk', 'geom', 'POINT', 2056, 0, 0);

-- v_wk_knoten_text source

CREATE VIEW v_vsa_knoten_text AS SELECT
    k.T_Id AS fid,
    k.T_Ili_Tid AS tid,
    k.nutzungsart_ist AS usage_current,
    k.funktion AS function,
    k.finanzierung AS financing,
    k.funktionhierarchisch AS function_hierarchic,
    k.baulicherzustand AS structure_condition,
    k.astatus AS status,
    k.bezeichnung AS identifier,
    kt.textpos AS geom,
    [REPLACE](kt.textinhalt, '#x', '\n') AS text,
    kt.textori - 90 AS textori,
    kt.texthali,
    kt.textvali,
    CASE WHEN k.nutzungsart_ist = 'Mischabwasser' THEN '102,0,102' WHEN k.nutzungsart_ist IN ('Niederschlagsabwasser', 'Reinabwasser', 'Bachwasser') THEN '0,0,255' WHEN k.nutzungsart_ist IN ('Schmutzabwasser', 'Industrieabwasser') THEN '255,0,0' WHEN k.nutzungsart_ist = 'entlastetes_Mischabwasser' THEN '0,255,0' WHEN k.nutzungsart_ist = 'andere' THEN '255,127,0' WHEN k.nutzungsart_ist = 'unbekannt' THEN '165,165,165' ELSE '165,165,165' END AS color_usage_current
FROM knoten k
   JOIN knoten_text kt ON kt.knotenref = k.t_id;
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_vsa_knoten_text', 'features', 'v_vsa_knoten_text', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_vsa_knoten_text', 'geom', 'POINT', 2056, 0, 0);

-- v_wk_knoten_versickerungsanlage source

CREATE VIEW v_vsa_knoten_versickerungsanlage AS SELECT
    k.T_Id AS fid,
    k.T_Ili_Tid AS tid,
    k.ara_nr AS wwtp_no,
    k.baujahr AS year_of_construction,
    k.baulicherzustand AS structure_condition,
    k.bemerkung AS remark,
    k.bezeichnung AS identifier,
    k.deckelkote AS cover_level,
    k.dimension1,
    k.dimension2,
    k.finanzierung AS financing,
    k.funktion AS function,
    k.funktionhierarchisch AS function_hierarchic,
    k.lagegenauigkeit AS positional_accuracy,
    k.nutzungsart_geplant AS usage_planned,
    k.nutzungsart_ist AS usage_current,
    k.obj_id_abwasserbauwerk AS fk_wastewater_structure,
    k.obj_id_deckel AS fk_cover,
    k.rueckstaukote_ist AS backflow_level,
    k.sanierungsbedarf AS renovation_necessity,
    k.sohlenkote AS bottom_level,
    k.astatus AS status,
    k.symbolori,
    k.zugaenglichkeit AS accessibility,
    k.zustandserhebung_jahr AS condition_survey_year,
    o1.bezeichnung AS operator,
    o1.organisationstyp AS operator_org_type,
    o2.bezeichnung AS owner,
    o2.organisationstyp AS owner_org_type,
    o3.bezeichnung AS data_owner,
    o3.organisationstyp AS data_owner_org_type,
    o4.bezeichnung AS data_provider,
    o4.organisationstyp AS data_provider_org_type,
    k.letzte_aenderung AS last_modification,
    kl.lage AS geom
  FROM
    knoten k
    JOIN knoten_lage kl ON k.t_id = kl.t_id
    LEFT JOIN organisation o1 ON k.betreiberref = o1.t_id
    LEFT JOIN organisation o2 ON k.eigentuemerref = o2.t_id
    LEFT JOIN organisation o3 ON k.datenherrref = o3.t_id
    LEFT JOIN organisation o4 ON k.datenlieferantref = o4.t_id
WHERE funktion IN ('Versickerungsanlage');
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_vsa_knoten_versickerungsanlage', 'features', 'v_vsa_knoten_versickerungsanlage', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_vsa_knoten_versickerungsanlage', 'geom', 'POINT', 2056, 0, 0);

-- v_wk_leitung source

CREATE VIEW v_vsa_leitung AS SELECT
    h.T_Id AS fid,
    h.T_Ili_Tid AS tid,
    h.baujahr AS year_of_construction,
    h.baulicherzustand AS structure_condition,
    h.bemerkung AS remark,
    h.bezeichnung AS identifier,
    h.finanzierung AS financing,
    h.funktionhierarchisch AS function_hierarchic,
    h.funktionhydraulisch AS function_hydraulic,
    h.hoehengenauigkeit_nach AS elevation_accuracy_to,
    h.hoehengenauigkeit_von AS elevation_accuracy_from,
    h.hydr_belastung_ist AS hydraulic_load_current,
    h.kote_nach AS level_to,
    h.kote_von AS level_from,
    h.laengeeffektiv AS length_effective,
    h.lagebestimmung AS horizontal_positioning,
    h.leckschutz AS leak_protection,
    h.lichte_breite AS clear_width,
    h.lichte_hoehe AS clear_height,
    h.material,
    h.nutzungsart_geplant AS usage_planned,
    h.nutzungsart_ist AS usage_current,
    h.obj_id_abwasserbauwerk AS fk_wastewater_structure,
    h.obj_id_nachhaltungspunkt AS fk_reach_point_to,
    h.obj_id_vonhaltungspunkt AS fk_reach_point_from,
    h.profiltyp AS profile_type,
    h.reliner_art AS relining_kind,
    h.reliner_nennweite AS reliner_nominal_size,
    h.sanierungsbedarf AS renovation_necessity,
    h.astatus AS status,
    h.wandrauhigkeit AS wall_roughness,
    h.wbw_basisjahr AS rv_base_year,
    h.wbw_bauart AS rv_construction_type,
    h.wiederbeschaffungswert AS replacement_value,
    h.zustandserhebung_jahr AS condition_survey_year,
    o1.bezeichnung AS operator,
    o1.organisationstyp AS operator_org_type,
    o2.bezeichnung AS owner,
    o2.organisationstyp AS owner_org_type,
    h.knoten_nachref AS fk_node_to,
    h.knoten_vonref AS fk_node_from,
    h.leitung_nachref AS fk_reach_to,
    h.rohrprofilref AS fk_pipe_profile,
    o3.bezeichnung AS data_owner,
    o3.organisationstyp AS data_owner_org_type,
    o4.bezeichnung AS data_provider,
    o4.organisationstyp AS data_provider_org_type,
    h.letzte_aenderung AS last_modification,
    h.verlauf AS geom
FROM
    leitung h
    LEFT JOIN organisation o1 ON h.betreiberref = o1.t_id
    LEFT JOIN organisation o2 ON h.eigentuemerref = o2.t_id
    LEFT JOIN organisation o3 ON h.datenherrref = o3.t_id
    LEFT JOIN organisation o4 ON h.datenlieferantref = o4.t_id;
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_vsa_leitung', 'features', 'v_vsa_leitung', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_vsa_leitung', 'geom', 'LINESTRING', 2056, 0, 0);

-- v_wk_leitung_text source

CREATE VIEW v_vsa_leitung_text AS SELECT
    h.T_Id AS fid,
    h.T_Ili_Tid AS tid,
    h.nutzungsart_ist AS usage_current,
    h.profiltyp AS profile_type,
    h.funktionhierarchisch AS function_hierarchic,
    h.funktionhydraulisch AS function_hydraulic,
    h.finanzierung AS financing,
    h.baulicherzustand AS structure_condition,
    h.astatus AS status,
    h.bezeichnung AS identifier,
    h.lichte_hoehe AS clear_height,
    h.material,
    ht.textpos AS geom,
    [REPLACE](ht.textinhalt, '#x', '\n') AS text,
    ht.textori - 90 AS textori,
    ht.texthali,
    ht.textvali,
    CASE WHEN h.nutzungsart_ist = 'Mischabwasser' THEN '102,0,102' WHEN h.nutzungsart_ist IN ('Niederschlagsabwasser', 'Reinabwasser', 'Bachwasser') THEN '0,0,255' WHEN h.nutzungsart_ist IN ('Schmutzabwasser', 'Industrieabwasser') THEN '255,0,0' WHEN h.nutzungsart_ist = 'entlastetes_Mischabwasser' THEN '0,255,0' WHEN h.nutzungsart_ist = 'andere' THEN '255,127,0' WHEN h.nutzungsart_ist = 'unbekannt' THEN '165,165,165' ELSE '165,165,165' END AS color_usage_current
FROM leitung h
   JOIN leitung_text ht ON ht.leitungref = h.t_id;
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_vsa_leitung_text', 'features', 'v_vsa_leitung_text', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_vsa_leitung_text', 'geom', 'POINT', 2056, 0, 0);

-- v_wk_ueberlauf_foerderaggregat source

CREATE VIEW v_vsa_ueberlauf_foerderaggregat AS SELECT
    ua.T_Id AS fid,
    ua.T_Ili_Tid AS tid,
    ua.art AS type,
    ua.bezeichnung AS identifier,
    ua.knotenref AS fk_wastewater_node,
    ua.knoten_nachref AS fk_node_to,
    ua.datenherrref AS fk_dataowner,
    ua.datenlieferantref AS fk_provider,
    ua.letzte_aenderung AS last_modification,
    k.ara_nr AS wwtp_no,
    k.baujahr AS year_of_construction,
    k.baulicherzustand AS structure_condition,
    k.bemerkung AS remark,
    k.bezeichnung AS node_identifier,
    k.deckelkote AS cover_level,
    k.dimension1,
    k.dimension2,
    k.finanzierung AS financing,
    k.funktion AS function,
    k.funktionhierarchisch AS function_hierarchic,
    k.lagegenauigkeit AS positional_accuracy,
    k.nutzungsart_geplant AS usage_planned,
    k.nutzungsart_ist AS usage_current,
    k.obj_id_abwasserbauwerk AS fk_wastewater_structure,
    k.obj_id_deckel AS fk_cover,
    k.rueckstaukote_ist AS backflow_level,
    k.sanierungsbedarf AS renovation_necessity,
    k.sohlenkote AS bottom_level,
    k.astatus AS status,
    k.symbolori,
    k.zugaenglichkeit AS accessibility,
    k.zustandserhebung_jahr AS condition_survey_year,
    o1.bezeichnung AS operator,
    o1.organisationstyp AS operator_org_type,
    o2.bezeichnung AS owner,
    o2.organisationstyp AS owner_org_type,
    o3.bezeichnung AS data_owner,
    o3.organisationstyp AS data_owner_org_type,
    o4.bezeichnung AS data_provider,
    o4.organisationstyp AS data_provider_org_type,
    k.letzte_aenderung AS node_last_modification,
    kl.lage AS geom,
    CASE WHEN k.nutzungsart_ist = 'Mischabwasser' THEN '102,0,102'
         WHEN k.nutzungsart_ist IN ('Niederschlagsabwasser', 'Reinabwasser', 'Bachwasser') THEN '0,0,255'
         WHEN k.nutzungsart_ist IN ('Schmutzabwasser', 'Industrieabwasser') THEN '255,0,0'
         WHEN k.nutzungsart_ist = 'entlastetes_Mischabwasser' THEN '0,255,0'
         WHEN k.nutzungsart_ist = 'andere' THEN '255,127,0'
         WHEN k.nutzungsart_ist = 'unbekannt' THEN '165,165,165'
         ELSE '165,165,165'
         END AS color_usage_current
  FROM
    ueberlauf_foerderaggregat ua
    JOIN knoten k ON ua.knotenref = k.t_id
    JOIN knoten_lage kl ON k.t_id = kl.t_id
    LEFT JOIN organisation o1 ON k.betreiberref = o1.t_id
    LEFT JOIN organisation o2 ON k.eigentuemerref = o2.t_id
    LEFT JOIN organisation o3 ON k.datenherrref = o3.t_id
    LEFT JOIN organisation o4 ON k.datenlieferantref = o4.t_id;
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_vsa_ueberlauf_foerderaggregat', 'features', 'v_vsa_ueberlauf_foerderaggregat', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_vsa_ueberlauf_foerderaggregat', 'geom', 'POINT', 2056, 0, 0);

CREATE VIEW v_errorlist_ueberlauf_foerderaggregat_data AS
    SELECT *
      FROM ca_error_data e
     WHERE e.class LIKE 'Ueberlauf_Foerderaggregat';
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_errorlist_ueberlauf_foerderaggregat_data', 'attributes', 'v_errorlist_ueberlauf_foerderaggregat_data', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);


CREATE VIEW v_errorlist_error_teileinzugsgebiet_data AS
    SELECT *
      FROM ca_error_data e
     WHERE e.class LIKE 'Teileinzugsgebiet';
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_errorlist_error_teileinzugsgebiet_data', 'attributes', 'v_errorlist_error_teileinzugsgebiet_data', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);

CREATE VIEW v_errorlist_error_knoten_data AS
    SELECT *
      FROM ca_error_data e
     WHERE e.class LIKE 'Knoten';
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_errorlist_error_knoten_data', 'attributes', 'v_errorlist_error_knoten_data', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);

CREATE VIEW v_errorlist_error_haltung_data AS
    SELECT *
      FROM ca_error_data e
     WHERE e.class LIKE 'Leitung';
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_errorlist_error_haltung_data', 'attributes', 'v_errorlist_error_haltung_data', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);


CREATE VIEW v_error_ueberlauf_foerderaggregat AS SELECT
u.T_Id AS fid,
e.tid AS tid,
           e.class AS class,
           e.count_error AS error_count,
           e.wk_max AS wk_max,
           e.gep_max AS gep_max,
           n.funktion AS function,
           n.funktionhierarchisch AS function_hierarchic,
           o.bezeichnung AS owner,
           k.lage AS geom
      FROM ca_error_object e
           JOIN ueberlauf_foerderaggregat u ON e.tid = u.t_ili_tid
           JOIN knoten n ON u.knotenref = n.t_id
           JOIN knoten_lage k ON n.t_id = k.t_id
           LEFT JOIN organisation o ON n.eigentuemerref = o.t_id
     WHERE e.class LIKE 'Ueberlauf_Foerderaggregat';
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_error_ueberlauf_foerderaggregat', 'features', 'v_error_ueberlauf_foerderaggregat', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_error_ueberlauf_foerderaggregat', 'geom', 'POINT', 2056, 0, 0);

CREATE VIEW v_error_teileinzugsgebiet AS SELECT
t.T_Id AS fid,
 e.tid AS tid,
           e.class AS class,
           e.count_error AS error_count,
           e.wk_max AS wk_max,
           e.gep_max AS gep_max,
           t.perimeter AS geom
      FROM ca_error_object e
           JOIN teileinzugsgebiet t ON e.tid = t.t_ili_tid
     WHERE e.class LIKE 'Teileinzugsgebiet';
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_error_teileinzugsgebiet', 'features', 'v_error_teileinzugsgebiet', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_error_teileinzugsgebiet', 'geom', 'CURVEPOLYGON', 2056, 0, 0);

-- v_error_sk_trennbauwerk source

CREATE VIEW v_error_sk_trennbauwerk AS SELECT
sk.T_Id AS fid,
e.tid AS tid,
           e.class AS class,
           e.count_error AS error_count,
           e.wk_max AS wk_max,
           e.gep_max AS gep_max,
           o.bezeichnung AS owner,
           k.lage AS geom
      FROM ca_error_object e
          JOIN sk_trennbauwerk sk ON e.tid = sk.t_ili_tid
          JOIN knoten n ON sk.paa_knotenref = n.t_id
          JOIN knoten_lage k ON n.t_id = k.t_id
          LEFT JOIN organisation o ON n.eigentuemerref = o.t_id
     WHERE e.class LIKE 'SK_Trennbauwerk';
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_error_sk_trennbauwerk', 'features', 'v_error_sk_trennbauwerk', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_error_sk_trennbauwerk', 'geom', 'POINT', 2056, 0, 0);

-- v_error_sk_regenrueckhaltebecken_kanal source

CREATE VIEW v_error_sk_regenrueckhaltebecken_kanal AS SELECT
sk.T_Id AS fid,
e.tid AS tid,
           e.class AS class,
           e.count_error AS error_count,
           e.wk_max AS wk_max,
           e.gep_max AS gep_max,
           o.bezeichnung AS owner,
           k.lage AS geom
      FROM ca_error_object e
          JOIN sk_regenrueckhaltebecken_kanal sk ON e.tid = sk.t_ili_tid
          JOIN knoten n ON sk.paa_knotenref = n.t_id
          JOIN knoten_lage k ON n.t_id = k.t_id
          LEFT JOIN organisation o ON n.eigentuemerref = o.t_id
     WHERE e.class LIKE 'SK_regenrueckhaltebecken_kanal';
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_error_sk_regenrueckhaltebecken_kanal', 'features', 'v_error_sk_regenrueckhaltebecken_kanal', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_error_sk_regenrueckhaltebecken_kanal', 'geom', 'POINT', 2056, 0, 0);

CREATE VIEW v_error_sk_regenueberlaufbecken AS SELECT
sk.T_Id AS fid,
e.tid AS tid,
           e.class AS class,
           e.count_error AS error_count,
           e.wk_max AS wk_max,
           e.gep_max AS gep_max,
           o.bezeichnung AS owner,
           k.lage AS geom
      FROM ca_error_object e
          JOIN sk_regenueberlaufbecken sk ON e.tid = sk.t_ili_tid
          JOIN knoten n ON sk.paa_knotenref = n.t_id
          JOIN knoten_lage k ON n.t_id = k.t_id
          LEFT JOIN organisation o ON n.eigentuemerref = o.t_id
     WHERE e.class LIKE 'SK_Regenueberlaufbecken';
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_error_sk_regenueberlaufbecken', 'features', 'v_error_sk_regenueberlaufbecken', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_error_sk_regenueberlaufbecken', 'geom', 'POINT', 2056, 0, 0);

CREATE VIEW v_error_sk_regenueberlauf AS SELECT
sk.T_Id AS fid,
e.tid AS tid,
           e.class AS class,
           e.count_error AS error_count,
           e.wk_max AS wk_max,
           e.gep_max AS gep_max,
           o.bezeichnung AS owner,
           k.lage AS geom
      FROM ca_error_object e
          JOIN sk_regenueberlauf sk ON e.tid = sk.t_ili_tid
          JOIN knoten n ON sk.paa_knotenref = n.t_id
          JOIN knoten_lage k ON n.t_id = k.t_id
          LEFT JOIN organisation o ON n.eigentuemerref = o.t_id
     WHERE e.class LIKE 'SK_Regenueberlauf';
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_error_sk_regenueberlauf', 'features', 'v_error_sk_regenueberlauf', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_error_sk_regenueberlauf', 'geom', 'POINT', 2056, 0, 0);

CREATE VIEW v_error_sk_pumpwerk AS SELECT
sk.T_Id AS fid,
 e.tid AS tid,
           e.class AS class,
           e.count_error AS error_count,
           e.wk_max AS wk_max,
           e.gep_max AS gep_max,
           o.bezeichnung AS owner,
           k.lage AS geom
      FROM ca_error_object e
          JOIN sk_pumpwerk sk ON e.tid = sk.t_ili_tid
          JOIN knoten n ON sk.paa_knotenref = n.t_id
          JOIN knoten_lage k ON n.t_id = k.t_id
          LEFT JOIN organisation o ON n.eigentuemerref = o.t_id
     WHERE e.class LIKE 'SK_Pumpwerk';
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_error_sk_pumpwerk', 'features', 'v_error_sk_pumpwerk', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_error_sk_pumpwerk', 'geom', 'POINT', 2056, 0, 0);

CREATE VIEW v_error_sk_einleitstelle AS SELECT
sk.T_Id AS fid,
e.tid AS tid,
           e.class AS class,
           e.count_error AS error_count,
           e.wk_max AS wk_max,
           e.gep_max AS gep_max,
           o.bezeichnung AS owner,
           k.lage AS geom
      FROM ca_error_object e
          JOIN sk_einleitstelle sk ON e.tid = sk.t_ili_tid
          JOIN knoten n ON sk.paa_knotenref = n.t_id
          JOIN knoten_lage k ON n.t_id = k.t_id
          LEFT JOIN organisation o ON n.eigentuemerref = o.t_id
     WHERE e.class LIKE 'SK_Einleitstelle';
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_error_sk_einleitstelle', 'features', 'v_error_sk_einleitstelle', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_error_sk_einleitstelle', 'geom', 'POINT', 2056, 0, 0);

CREATE VIEW v_error_recommendation_teileinzugsgebiet AS SELECT
t.T_Id AS fid,
t.t_ili_tid AS tid,
           e.recommendation AS recommendation,
           e.recommendation_detail AS recommendation_detail,
           t.perimeter AS geom
      FROM ca_error_data e
           JOIN
           teileinzugsgebiet t ON e.tid = t.t_ili_tid
     WHERE e.class LIKE 'Teileinzugsgebiet'
     GROUP BY t.t_ili_tid,
              e.recommendation;
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_error_recommendation_teileinzugsgebiet', 'features', 'v_error_recommendation_teileinzugsgebiet', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_error_recommendation_teileinzugsgebiet', 'geom', 'CURVEPOLYGON', 2056, 0, 0);

CREATE VIEW v_error_recommendation_knoten AS SELECT
n.T_Id AS fid,
n.t_ili_tid AS tid,
           e.recommendation AS recommendation,
           e.recommendation_detail AS recommendation_detail,
           k.lage AS geom
      FROM ca_error_data e
           JOIN knoten n ON e.tid = n.t_ili_tid
           JOIN knoten_lage k ON n.t_id = k.t_id
     WHERE e.class LIKE 'Knoten'
     GROUP BY n.t_ili_tid,
              e.recommendation;
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_error_recommendation_knoten', 'features', 'v_error_recommendation_knoten', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_error_recommendation_knoten', 'geom', 'POINT', 2056, 0, 0);

CREATE VIEW v_error_recommendation_haltung AS SELECT
h.T_Id AS fid,
h.t_ili_tid AS tid,
           e.recommendation AS recommendation,
           e.recommendation_detail AS recommendation_detail,
           h.verlauf AS geom
      FROM ca_error_data e
           JOIN
           leitung h ON e.tid = h.t_ili_tid
     WHERE e.class LIKE 'leitung'
     GROUP BY h.t_ili_tid,
              e.recommendation;
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_error_recommendation_haltung', 'features', 'v_error_recommendation_haltung', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_error_recommendation_haltung', 'geom', 'LINESTRING', 2056, 0, 0);

CREATE VIEW v_error_knoten AS SELECT
n.T_Id AS fid,
e.tid AS tid,
           e.class AS class,
           e.count_error AS error_count,
           e.wk_max AS wk_max,
           e.gep_max AS gep_max,
           n.funktion AS function,
           n.funktionhierarchisch AS function_hierarchic,
           o.bezeichnung AS owner,
           k.lage AS geom
      FROM ca_error_object e
           JOIN knoten n ON e.tid = n.t_ili_tid
           JOIN knoten_lage k ON n.t_id = k.t_id
           LEFT JOIN organisation o ON n.eigentuemerref = o.t_id
     WHERE e.class LIKE 'Knoten';
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_error_knoten', 'features', 'v_error_knoten', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_error_knoten', 'geom', 'POINT', 2056, 0, 0);

CREATE VIEW v_error_haltung AS SELECT
h.T_Id AS fid,
e.tid AS tid,
           e.class AS class,
           e.count_error AS error_count,
           e.wk_max AS wk_max,
           e.gep_max AS gep_max,
           h.funktionhierarchisch AS function_hierarchic,
           o.bezeichnung AS owner,
           h.verlauf AS geom
      FROM ca_error_object e
           JOIN leitung h ON e.tid = h.t_ili_tid
           LEFT JOIN organisation o ON h.eigentuemerref = o.t_id
     WHERE e.class LIKE 'Leitung';
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_error_haltung', 'features', 'v_error_haltung', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_error_haltung', 'geom', 'LINESTRING', 2056, 0, 0);

CREATE VIEW v_error_error_knoten AS SELECT
n.T_Id AS fid,
n.t_ili_tid AS tid,
           e.error AS error,
           k.lage AS geom
      FROM ca_error_data e
           JOIN
           knoten n ON e.tid = n.t_ili_tid
           JOIN
           knoten_lage k ON n.t_id = k.t_id
     WHERE e.class LIKE 'Knoten'
     GROUP BY n.t_ili_tid,
              e.error;
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_error_error_knoten', 'features', 'v_error_error_knoten', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_error_error_knoten', 'geom', 'POINT', 2056, 0, 0);

CREATE VIEW v_error_error_haltung AS SELECT
h.T_Id AS fid,
h.t_ili_tid AS tid,
           e.error AS error,
           h.verlauf AS geom
      FROM ca_error_data e
           JOIN
           leitung h ON e.tid = h.t_ili_tid
     WHERE e.class LIKE 'leitung'
     GROUP BY h.t_ili_tid,
              e.error;
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_error_error_haltung', 'features', 'v_error_error_haltung', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_error_error_haltung', 'geom', 'LINESTRING', 2056, 0, 0);

CREATE VIEW v_error_category_knoten AS SELECT
n.T_Id AS fid,
n.t_ili_tid AS tid,
           e.category AS category,
           k.lage AS geom
      FROM ca_error_data e
           JOIN
           knoten n ON e.tid = n.t_ili_tid
           JOIN
           knoten_lage k ON n.t_id = k.t_id
     WHERE e.class LIKE 'Knoten'
     GROUP BY n.t_ili_tid,
              e.category;
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_error_category_knoten', 'features', 'v_error_category_knoten', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_error_category_knoten', 'geom', 'POINT', 2056, 0, 0);

-- v_error_category_haltung source

CREATE VIEW v_error_category_haltung AS SELECT
h.T_Id AS fid,
h.t_ili_tid AS tid,
           e.category AS category,
           h.verlauf AS geom
      FROM ca_error_data e
           JOIN
           leitung h ON e.tid = h.t_ili_tid
     WHERE e.class LIKE 'leitung'
     GROUP BY h.t_ili_tid,
              e.category;
INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, min_x, min_y, max_x, max_y, srs_id) VALUES ('v_error_category_haltung', 'features', 'v_error_category_haltung', NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), NULL, NULL, NULL, NULL, NULL);
INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m) VALUES ('v_error_category_haltung', 'geom', 'LINESTRING', 2056, 0, 0);
