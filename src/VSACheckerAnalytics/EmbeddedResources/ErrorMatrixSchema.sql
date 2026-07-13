-- ============================================================
-- error_matrix schema (single source of truth).
--
-- Owns the full error_matrix table and its join index. Runs once,
-- before the igcheck XLSX import. Columns fall into two groups:
--   * the 16 columns the igcheck XLSX carries, inserted positionally
--     by ErrorMatrixImporter (see ErrorMatrixColumns).
--   * the 6 geowerkstatt-maintained columns the XLSX does not carry
--     (Italian and error_type variants), filled by ReaderErrorEnrichment.sql
--     for the reader 'base' rows.
--
-- error_matrix is never part of the GeoPackage template, so this
-- script always creates the table on a fresh pipeline copy.
-- ============================================================

DROP TABLE IF EXISTS error_matrix;

CREATE TABLE error_matrix (
    t_id                INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Columns carried by the igcheck XLSX (ErrorMatrixColumns, in order).
    cid                 TEXT,
    ccat                TEXT,
    cmsg_de             TEXT,
    cmsg_fr             TEXT,
    class_de            TEXT,
    class_fr            TEXT,
    checkmodel          TEXT,
    model               TEXT,
    prio_uc             TEXT,
    prio_gsp            TEXT,
    sub_project_gsp_de  TEXT,
    sub_project_gsp_fr  TEXT,
    required_action_de  TEXT,
    required_action_fr  TEXT,
    action_context_de   TEXT,
    action_context_fr   TEXT,

    -- geowerkstatt-maintained columns not carried by the XLSX.
    cmsg_it             TEXT,
    error_type_de       TEXT,
    error_type_fr       TEXT,
    error_type_it       TEXT,
    required_action_it  TEXT,
    action_context_it   TEXT
);

CREATE INDEX ix_error_matrix_cid_model_class_de
    ON error_matrix (cid, model, class_de);
