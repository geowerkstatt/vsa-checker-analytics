-- ============================================================
-- Registrierung der Statistik-Views in gpkg_contents
-- (data_type='attributes', wie bei den bestehenden Sachdaten-Tabellen
-- dieses GeoPackages, z.B. 'organisation'). Ohne diesen Eintrag werden
-- die Views von QGIS/GDAL nicht im GeoPackage-Layerbaum angezeigt.
-- INSERT OR REPLACE macht das Skript erneut ausfuehrbar.
-- ============================================================

INSERT OR REPLACE INTO gpkg_contents (table_name, data_type, identifier, description)
VALUES ('v_statistics_alr', 'attributes', 'v_statistics_alr', 'Statistik je Attribut - Quelltabelle alr');
INSERT OR REPLACE INTO gpkg_contents (table_name, data_type, identifier, description)
VALUES ('v_statistics_knoten', 'attributes', 'v_statistics_knoten', 'Statistik je Attribut - Quelltabelle knoten');
INSERT OR REPLACE INTO gpkg_contents (table_name, data_type, identifier, description)
VALUES ('v_statistics_leitung', 'attributes', 'v_statistics_leitung', 'Statistik je Attribut - Quelltabelle leitung');
INSERT OR REPLACE INTO gpkg_contents (table_name, data_type, identifier, description)
VALUES ('v_statistics_massnahme', 'attributes', 'v_statistics_massnahme', 'Statistik je Attribut - Quelltabelle massnahme');
INSERT OR REPLACE INTO gpkg_contents (table_name, data_type, identifier, description)
VALUES ('v_statistics_organisation', 'attributes', 'v_statistics_organisation', 'Statistik je Attribut - Quelltabelle organisation');
INSERT OR REPLACE INTO gpkg_contents (table_name, data_type, identifier, description)
VALUES ('v_statistics_rohrprofil', 'attributes', 'v_statistics_rohrprofil', 'Statistik je Attribut - Quelltabelle rohrprofil');
INSERT OR REPLACE INTO gpkg_contents (table_name, data_type, identifier, description)
VALUES ('v_statistics_rohrprofil_geometrie', 'attributes', 'v_statistics_rohrprofil_geometrie', 'Statistik je Attribut - Quelltabelle rohrprofil_geometrie');
INSERT OR REPLACE INTO gpkg_contents (table_name, data_type, identifier, description)
VALUES ('v_statistics_teileinzugsgebiet', 'attributes', 'v_statistics_teileinzugsgebiet', 'Statistik je Attribut - Quelltabelle teileinzugsgebiet');
INSERT OR REPLACE INTO gpkg_contents (table_name, data_type, identifier, description)
VALUES ('v_statistics_ueberlauf_foerderaggregat', 'attributes', 'v_statistics_ueberlauf_foerderaggregat', 'Statistik je Attribut - Quelltabelle ueberlauf_foerderaggregat');
INSERT OR REPLACE INTO gpkg_contents (table_name, data_type, identifier, description)
VALUES ('v_statistics_bauwerkskomponente', 'attributes', 'v_statistics_bauwerkskomponente', 'Statistik je Attribut - Quelltabelle bauwerkskomponente');
INSERT OR REPLACE INTO gpkg_contents (table_name, data_type, identifier, description)
VALUES ('v_statistics_sk_autonome_messstelle', 'attributes', 'v_statistics_sk_autonome_messstelle', 'Statistik je Attribut - Quelltabelle sk_autonome_messstelle');
INSERT OR REPLACE INTO gpkg_contents (table_name, data_type, identifier, description)
VALUES ('v_statistics_sk_duekeroberhaupt', 'attributes', 'v_statistics_sk_duekeroberhaupt', 'Statistik je Attribut - Quelltabelle sk_duekeroberhaupt');
INSERT OR REPLACE INTO gpkg_contents (table_name, data_type, identifier, description)
VALUES ('v_statistics_sk_einleitstelle', 'attributes', 'v_statistics_sk_einleitstelle', 'Statistik je Attribut - Quelltabelle sk_einleitstelle');
INSERT OR REPLACE INTO gpkg_contents (table_name, data_type, identifier, description)
VALUES ('v_statistics_sk_pumpwerk', 'attributes', 'v_statistics_sk_pumpwerk', 'Statistik je Attribut - Quelltabelle sk_pumpwerk');
INSERT OR REPLACE INTO gpkg_contents (table_name, data_type, identifier, description)
VALUES ('v_statistics_sk_regenrueckhaltebecken_kanal', 'attributes', 'v_statistics_sk_regenrueckhaltebecken_kanal', 'Statistik je Attribut - Quelltabelle sk_regenrueckhaltebecken_kanal');
INSERT OR REPLACE INTO gpkg_contents (table_name, data_type, identifier, description)
VALUES ('v_statistics_sk_regenueberlauf', 'attributes', 'v_statistics_sk_regenueberlauf', 'Statistik je Attribut - Quelltabelle sk_regenueberlauf');
INSERT OR REPLACE INTO gpkg_contents (table_name, data_type, identifier, description)
VALUES ('v_statistics_sk_regenueberlaufbecken', 'attributes', 'v_statistics_sk_regenueberlaufbecken', 'Statistik je Attribut - Quelltabelle sk_regenueberlaufbecken');
INSERT OR REPLACE INTO gpkg_contents (table_name, data_type, identifier, description)
VALUES ('v_statistics_sk_trennbauwerk', 'attributes', 'v_statistics_sk_trennbauwerk', 'Statistik je Attribut - Quelltabelle sk_trennbauwerk');
INSERT OR REPLACE INTO gpkg_contents (table_name, data_type, identifier, description)
VALUES ('v_statistics_sk_uebrige', 'attributes', 'v_statistics_sk_uebrige', 'Statistik je Attribut - Quelltabelle sk_uebrige');
INSERT OR REPLACE INTO gpkg_contents (table_name, data_type, identifier, description)
VALUES ('v_statistics_kennlinie_stuetzpunkt', 'attributes', 'v_statistics_kennlinie_stuetzpunkt', 'Statistik je Attribut - Quelltabelle kennlinie_stuetzpunkt');
INSERT OR REPLACE INTO gpkg_contents (table_name, data_type, identifier, description)
VALUES ('v_statistics_attribute', 'attributes', 'v_statistics_attribute', 'Statistik je Attribut - Gesamtuebersicht aller Tabellen, sortiert in Tabellen- und Attributreihenfolge');
