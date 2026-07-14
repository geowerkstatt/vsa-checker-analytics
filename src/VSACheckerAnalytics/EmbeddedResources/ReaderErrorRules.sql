-- ============================================================
-- Reader error rules setup.
--
-- Runs after error_matrix has been populated (igcheck rows from the
-- XLSX and the category-level 'base' rows from the embedded
-- errorMatrixBaseError.xlsx). It creates and seeds the
-- reader_error_rules override / suppression table, the
-- geowerkstatt-maintained knowledge that does not travel via the XLSX.
-- ============================================================

-- ── reader_error_rules: attribute / condition overrides + suppression ────────

DROP TABLE IF EXISTS reader_error_rules;

CREATE TABLE reader_error_rules (
    rule_id                   INTEGER PRIMARY KEY AUTOINCREMENT,
    error_id                  INTEGER NOT NULL,
    attr_name                 TEXT,
    condition_col             TEXT,
    condition_val             TEXT,
    msg_template_de           TEXT,
    msg_template_fr           TEXT,
    msg_template_it           TEXT,
    msg_template_en           TEXT,
    error_type_de             TEXT,
    error_type_fr             TEXT,
    error_type_it             TEXT,
    wk                        INTEGER,
    gep                       INTEGER,
    recommendation_de         TEXT,
    recommendation_fr         TEXT,
    recommendation_it         TEXT,
    recommendation_detail_de  TEXT,
    recommendation_detail_fr  TEXT,
    recommendation_detail_it  TEXT,
    rule_note                 TEXT,
    suppress                  INTEGER NOT NULL DEFAULT 0,
    UNIQUE (error_id, attr_name, condition_col, condition_val)
);

INSERT INTO reader_error_rules
    (error_id, attr_name, condition_col, condition_val,
     msg_template_de, msg_template_fr, msg_template_it, msg_template_en,
     wk, gep,
     recommendation_de,       recommendation_fr,          recommendation_it,
     recommendation_detail_de, recommendation_detail_fr,   recommendation_detail_it,
     rule_note, suppress)
VALUES
    -- Attr-specific override: BetreiberRef
    (11, 'BetreiberRef', NULL, NULL,
     'Pflichtattribut BetreiberRef fehlt',
     'Attribut obligatoire BetreiberRef manquant',
     'Attributo obbligatorio BetreiberRef mancante',
     'Mandatory attribute BetreiberRef missing',
     2, 2,
     'Daten erheben',   'Saisir les données',   'Rilevare i dati',
     'Betreiberorganisation erfassen und referenzieren',
     'Enregistrer et référencer l''organisation exploitante',
     'Registrare e referenziare l''organizzazione esercente',
     'Attr-Override: BetreiberRef Prio 2', 0),

    -- Condition override: FunktionHierarchisch missing on SAA object
    (11, 'FunktionHierarchisch', 'funktionhierarchisch', 'SAA',
     'Pflichtattribut FunktionHierarchisch fehlt (SAA-Pflichtfeld)',
     'Attribut obligatoire FunktionHierarchisch manquant (champ obligatoire SAA)',
     'Attributo obbligatorio FunktionHierarchisch mancante (campo obbligatorio SAA)',
     'Mandatory attribute FunktionHierarchisch missing (SAA required field)',
     1, 1,
     'Daten erheben',   'Saisir les données',   'Rilevare i dati',
     'FunktionHierarchisch für SAA-Lieferung zwingend setzen',
     'Définir obligatoirement FunktionHierarchisch pour la livraison SAA',
     'Impostare obbligatoriamente FunktionHierarchisch per la consegna SAA',
     'Condition: FunktionHierarchisch fehlt + funktionhierarchisch=SAA', 0),

    -- Attr-specific override: OBJ_ID exact-name
    (12, 'OBJ_ID', NULL, NULL,
     'OBJ_ID zu lang: {N} Zeichen (Max. {MAX}). Muss <= {MAX} Zeichen sein.',
     'OBJ_ID trop long : {N} caractères (max. {MAX}). Doit être <= {MAX} caractères.',
     'OBJ_ID troppo lungo: {N} caratteri (max. {MAX}). Deve essere <= {MAX} caratteri.',
     'OBJ_ID too long: {N} characters (max. {MAX}). Must be <= {MAX} characters.',
     2, 2,
     'Daten prüfen / bereinigen', 'Vérifier / corriger les données', 'Verificare / correggere i dati',
     'OBJ_ID auf maximal {MAX} Zeichen kürzen.',
     'Réduire OBJ_ID à {MAX} caractères maximum.',
     'Ridurre OBJ_ID a un massimo di {MAX} caratteri.',
     'Attr-Override: OBJ_ID Prio 2', 0),

    -- Suppression rules: reader error 12 on OBJ_ID_* (covered by igcheck 1021)
    (12, 'OBJ_ID_Abwasserbauwerk',   NULL, NULL,
     NULL, NULL, NULL, NULL, NULL, NULL,
     NULL, NULL, NULL,  NULL, NULL, NULL,
     'Suppress: covered by igcheck 1021', 1),

    (12, 'OBJ_ID_nachHaltungspunkt', NULL, NULL,
     NULL, NULL, NULL, NULL, NULL, NULL,
     NULL, NULL, NULL,  NULL, NULL, NULL,
     'Suppress: covered by igcheck 1021', 1),

    (12, 'OBJ_ID_vonHaltungspunkt',  NULL, NULL,
     NULL, NULL, NULL, NULL, NULL, NULL,
     NULL, NULL, NULL,  NULL, NULL, NULL,
     'Suppress: covered by igcheck 1021', 1);
