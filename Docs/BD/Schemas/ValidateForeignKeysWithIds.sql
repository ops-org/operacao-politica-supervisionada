-- SQL Commands to Identify Invalid Foreign Key Constraints WITH ORPHANED RECORD IDs
-- This script checks for data type mismatches and shows orphaned record IDs

-- ========================================
-- FORNECEDOR SCHEMA VALIDATIONS
-- ========================================

-- Check data type compatibility for fornecedor_socio_faixa_etaria
SELECT 
    'fornecedor_socio_faixa_etaria' as constraint_name,
    'id_fornecedor_faixa_etaria' as column_name,
    'fornecedor_socio' as table_name,
    'fornecedor_faixa_etaria' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(f.id_fornecedor_faixa_etaria) != pg_typeof(fa.id) THEN 'DATA TYPE MISMATCH'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(f.id_fornecedor_faixa_etaria) as column_type,
    pg_typeof(fa.id) as referenced_type
FROM fornecedor.fornecedor_socio f
CROSS JOIN (SELECT id FROM fornecedor.fornecedor_faixa_etaria LIMIT 1) fa
GROUP BY validation_status, pg_typeof(f.id_fornecedor_faixa_etaria), pg_typeof(fa.id);

-- Show orphaned records for fornecedor_socio_faixa_etaria
SELECT 
    'ORPHANED RECORDS' as issue_type,
    'fornecedor_socio_faixa_etaria' as constraint_name,
    f.id as record_id,
    f.id_fornecedor_faixa_etaria as invalid_fk_value,
    'id_fornecedor_faixa_etaria' as column_name
FROM fornecedor.fornecedor_socio f
WHERE f.id_fornecedor_faixa_etaria IS NOT NULL 
  AND NOT EXISTS (
    SELECT 1 FROM fornecedor.fornecedor_faixa_etaria fa WHERE fa.id = f.id_fornecedor_faixa_etaria
);

-- Check data type compatibility for fornecedor_socio_qualificacao
SELECT 
    'fornecedor_socio_qualificacao' as constraint_name,
    'id_fornecedor_socio_qualificacao' as column_name,
    'fornecedor_socio' as table_name,
    'fornecedor_socio_qualificacao' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(f.id_fornecedor_socio_qualificacao) != pg_typeof(fsq.id) THEN 'DATA TYPE MISMATCH'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(f.id_fornecedor_socio_qualificacao) as column_type,
    pg_typeof(fsq.id) as referenced_type
FROM fornecedor.fornecedor_socio f
CROSS JOIN (SELECT id FROM fornecedor.fornecedor_socio_qualificacao LIMIT 1) fsq
GROUP BY validation_status, pg_typeof(f.id_fornecedor_socio_qualificacao), pg_typeof(fsq.id);

-- Show orphaned records for fornecedor_socio_qualificacao
SELECT 
    'ORPHANED RECORDS' as issue_type,
    'fornecedor_socio_qualificacao' as constraint_name,
    f.id as record_id,
    f.id_fornecedor_socio_qualificacao as invalid_fk_value,
    'id_fornecedor_socio_qualificacao' as column_name
FROM fornecedor.fornecedor_socio f
WHERE f.id_fornecedor_socio_qualificacao IS NOT NULL 
  AND NOT EXISTS (
    SELECT 1 FROM fornecedor.fornecedor_socio_qualificacao fsq WHERE fsq.id = f.id_fornecedor_socio_qualificacao
);

-- Check data type compatibility for fornecedor_socio_representante_qualificacao
SELECT 
    'fornecedor_socio_representante_qualificacao' as constraint_name,
    'id_fornecedor_socio_representante_qualificacao' as column_name,
    'fornecedor_socio' as table_name,
    'fornecedor_socio_qualificacao' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(f.id_fornecedor_socio_representante_qualificacao) != pg_typeof(fsq.id) THEN 'DATA TYPE MISMATCH'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(f.id_fornecedor_socio_representante_qualificacao) as column_type,
    pg_typeof(fsq.id) as referenced_type
FROM fornecedor.fornecedor_socio f
CROSS JOIN (SELECT id FROM fornecedor.fornecedor_socio_qualificacao LIMIT 1) fsq
GROUP BY validation_status, pg_typeof(f.id_fornecedor_socio_representante_qualificacao), pg_typeof(fsq.id);

-- Show orphaned records for fornecedor_socio_representante_qualificacao
SELECT 
    'ORPHANED RECORDS' as issue_type,
    'fornecedor_socio_representante_qualificacao' as constraint_name,
    f.id as record_id,
    f.id_fornecedor_socio_representante_qualificacao as invalid_fk_value,
    'id_fornecedor_socio_representante_qualificacao' as column_name
FROM fornecedor.fornecedor_socio f
WHERE f.id_fornecedor_socio_representante_qualificacao IS NOT NULL 
  AND NOT EXISTS (
    SELECT 1 FROM fornecedor.fornecedor_socio_qualificacao fsq WHERE fsq.id = f.id_fornecedor_socio_representante_qualificacao
);

-- Check forcecedor_cnpj_incorreto (note: table name typo) - fornecedor_incorreto
SELECT 
    'forcecedor_cnpj_incorreto_fornecedor_incorreto' as constraint_name,
    'id_fornecedor_incorreto' as column_name,
    'forcecedor_cnpj_incorreto' as table_name,
    'fornecedor' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(fci.id_fornecedor_incorreto) != pg_typeof(f.id) THEN 'DATA TYPE MISMATCH'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(fci.id_fornecedor_incorreto) as column_type,
    pg_typeof(f.id) as referenced_type
FROM fornecedor.forcecedor_cnpj_incorreto fci
CROSS JOIN (SELECT id FROM fornecedor.fornecedor LIMIT 1) f
GROUP BY validation_status, pg_typeof(fci.id_fornecedor_incorreto), pg_typeof(f.id);

-- Show orphaned records for forcecedor_cnpj_incorreto_fornecedor_incorreto
SELECT 
    'ORPHANED RECORDS' as issue_type,
    'forcecedor_cnpj_incorreto_fornecedor_incorreto' as constraint_name,
    fci.cnpj_incorreto as record_id,
    fci.id_fornecedor_incorreto as invalid_fk_value,
    'id_fornecedor_incorreto' as column_name
FROM fornecedor.forcecedor_cnpj_incorreto fci
WHERE fci.id_fornecedor_incorreto IS NOT NULL 
  AND NOT EXISTS (
    SELECT 1 FROM fornecedor.fornecedor f WHERE f.id = fci.id_fornecedor_incorreto
);

-- Check forcecedor_cnpj_incorreto - fornecedor_correto
SELECT 
    'forcecedor_cnpj_incorreto_fornecedor_correto' as constraint_name,
    'id_fornecedor_correto' as column_name,
    'forcecedor_cnpj_incorreto' as table_name,
    'fornecedor' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(fci.id_fornecedor_correto) != pg_typeof(f.id) THEN 'DATA TYPE MISMATCH'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(fci.id_fornecedor_correto) as column_type,
    pg_typeof(f.id) as referenced_type
FROM fornecedor.forcecedor_cnpj_incorreto fci
CROSS JOIN (SELECT id FROM fornecedor.fornecedor LIMIT 1) f
GROUP BY validation_status, pg_typeof(fci.id_fornecedor_correto), pg_typeof(f.id);

-- Show orphaned records for forcecedor_cnpj_incorreto_fornecedor_correto
SELECT 
    'ORPHANED RECORDS' as issue_type,
    'forcecedor_cnpj_incorreto_fornecedor_correto' as constraint_name,
    fci.cnpj_incorreto as record_id,
    fci.id_fornecedor_correto as invalid_fk_value,
    'id_fornecedor_correto' as column_name
FROM fornecedor.forcecedor_cnpj_incorreto fci
WHERE fci.id_fornecedor_correto IS NOT NULL 
  AND NOT EXISTS (
    SELECT 1 FROM fornecedor.fornecedor f WHERE f.id = fci.id_fornecedor_correto
);

-- ========================================
-- CAMARA FEDERAL SCHEMA VALIDATIONS
-- ========================================

-- Check cf_despesa_tipo
SELECT 
    'cf_despesa_tipo' as constraint_name,
    'id_cf_despesa_tipo' as column_name,
    'cf_despesa' as table_name,
    'cf_despesa_tipo' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(cd.id_cf_despesa_tipo) != pg_typeof(cdt.id) THEN 'DATA TYPE MISMATCH'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(cd.id_cf_despesa_tipo) as column_type,
    pg_typeof(cdt.id) as referenced_type
FROM camara.cf_despesa cd
CROSS JOIN (SELECT id FROM camara.cf_despesa_tipo LIMIT 1) cdt
GROUP BY validation_status, pg_typeof(cd.id_cf_despesa_tipo), pg_typeof(cdt.id);

-- Show orphaned records for cf_despesa_tipo
SELECT 
    'ORPHANED RECORDS' as issue_type,
    'cf_despesa_tipo' as constraint_name,
    cd.id as record_id,
    cd.id_cf_despesa_tipo as invalid_fk_value,
    'id_cf_despesa_tipo' as column_name
FROM camara.cf_despesa cd
WHERE cd.id_cf_despesa_tipo IS NOT NULL 
  AND NOT EXISTS (
    SELECT 1 FROM camara.cf_despesa_tipo cdt WHERE cdt.id = cd.id_cf_despesa_tipo
);

-- Check cf_despesa_especificacao
SELECT 
    'cf_despesa_especificacao' as constraint_name,
    'id_cf_especificacao' as column_name,
    'cf_despesa' as table_name,
    'cf_especificacao_tipo' as referenced_table,
    'id_cf_especificacao' as referenced_column,
    CASE 
        WHEN pg_typeof(cd.id_cf_especificacao) != pg_typeof(cet.id_cf_especificacao) THEN 'DATA TYPE MISMATCH'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(cd.id_cf_especificacao) as column_type,
    pg_typeof(cet.id_cf_especificacao) as referenced_type
FROM camara.cf_despesa cd
CROSS JOIN (SELECT id_cf_especificacao FROM camara.cf_especificacao_tipo LIMIT 1) cet
GROUP BY validation_status, pg_typeof(cd.id_cf_especificacao), pg_typeof(cet.id_cf_especificacao);

-- Show orphaned records for cf_despesa_especificacao
SELECT 
    'ORPHANED RECORDS' as issue_type,
    'cf_despesa_especificacao' as constraint_name,
    cd.id as record_id,
    cd.id_cf_especificacao as invalid_fk_value,
    'id_cf_especificacao' as column_name
FROM camara.cf_despesa cd
WHERE cd.id_cf_especificacao IS NOT NULL 
  AND NOT EXISTS (
    SELECT 1 FROM camara.cf_especificacao_tipo cet WHERE cet.id_cf_especificacao = cd.id_cf_especificacao
);

-- Check cf_despesa_fornecedor
SELECT 
    'cf_despesa_fornecedor' as constraint_name,
    'id_fornecedor' as column_name,
    'cf_despesa' as table_name,
    'fornecedor' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(cd.id_fornecedor) != pg_typeof(f.id) THEN 'DATA TYPE MISMATCH'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(cd.id_fornecedor) as column_type,
    pg_typeof(f.id) as referenced_type
FROM camara.cf_despesa cd
CROSS JOIN (SELECT id FROM fornecedor.fornecedor LIMIT 1) f
GROUP BY validation_status, pg_typeof(cd.id_fornecedor), pg_typeof(f.id);

-- Show orphaned records for cf_despesa_fornecedor
SELECT 
    'ORPHANED RECORDS' as issue_type,
    'cf_despesa_fornecedor' as constraint_name,
    cd.id as record_id,
    cd.id_fornecedor as invalid_fk_value,
    'id_fornecedor' as column_name
FROM camara.cf_despesa cd
WHERE cd.id_fornecedor IS NOT NULL 
  AND NOT EXISTS (
    SELECT 1 FROM fornecedor.fornecedor f WHERE f.id = cd.id_fornecedor
);

-- Check cf_mandato_deputado
SELECT 
    'cf_mandato_deputado' as constraint_name,
    'id_cf_deputado' as column_name,
    'cf_mandato' as table_name,
    'cf_deputado' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(cm.id_cf_deputado) != pg_typeof(cd.id) THEN 'DATA TYPE MISMATCH'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(cm.id_cf_deputado) as column_type,
    pg_typeof(cd.id) as referenced_type
FROM camara.cf_mandato cm
CROSS JOIN (SELECT id FROM camara.cf_deputado LIMIT 1) cd
GROUP BY validation_status, pg_typeof(cm.id_cf_deputado), pg_typeof(cd.id);

-- Show orphaned records for cf_mandato_deputado
SELECT 
    'ORPHANED RECORDS' as issue_type,
    'cf_mandato_deputado' as constraint_name,
    cm.id as record_id,
    cm.id_cf_deputado as invalid_fk_value,
    'id_cf_deputado' as column_name
FROM camara.cf_mandato cm
WHERE cm.id_cf_deputado IS NOT NULL 
  AND NOT EXISTS (
    SELECT 1 FROM camara.cf_deputado cd WHERE cd.id = cm.id_cf_deputado
);

-- Check cf_senador_verba_gabinete_senador
SELECT 
    'cf_senador_verba_gabinete_senador' as constraint_name,
    'id_sf_senador' as column_name,
    'cf_senador_verba_gabinete' as table_name,
    'sf_senador' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(csvg.id_sf_senador) != pg_typeof(ss.id) THEN 'DATA TYPE MISMATCH'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(csvg.id_sf_senador) as column_type,
    pg_typeof(ss.id) as referenced_type
FROM camara.cf_senador_verba_gabinete csvg
CROSS JOIN (SELECT id FROM senado.sf_senador LIMIT 1) ss
GROUP BY validation_status, pg_typeof(csvg.id_sf_senador), pg_typeof(ss.id);

-- Show orphaned records for cf_senador_verba_gabinete_senador
SELECT 
    'ORPHANED RECORDS' as issue_type,
    'cf_senador_verba_gabinete_senador' as constraint_name,
    csvg.id_sf_senador as record_id,
    csvg.id_sf_senador as invalid_fk_value,
    'id_sf_senador' as column_name
FROM camara.cf_senador_verba_gabinete csvg
WHERE csvg.id_sf_senador IS NOT NULL 
  AND NOT EXISTS (
    SELECT 1 FROM senado.sf_senador ss WHERE ss.id = csvg.id_sf_senador
);

-- Check cf_sessao_presenca_deputado
SELECT 
    'cf_sessao_presenca_deputado' as constraint_name,
    'id_cf_deputado' as column_name,
    'cf_sessao_presenca' as table_name,
    'cf_deputado' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(csp.id_cf_deputado) != pg_typeof(cd.id) THEN 'DATA TYPE MISMATCH'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(csp.id_cf_deputado) as column_type,
    pg_typeof(cd.id) as referenced_type
FROM camara.cf_sessao_presenca csp
CROSS JOIN (SELECT id FROM camara.cf_deputado LIMIT 1) cd
GROUP BY validation_status, pg_typeof(csp.id_cf_deputado), pg_typeof(cd.id);

-- Show orphaned records for cf_sessao_presenca_deputado
SELECT 
    'ORPHANED RECORDS' as issue_type,
    'cf_sessao_presenca_deputado' as constraint_name,
    csp.id as record_id,
    csp.id_cf_deputado as invalid_fk_value,
    'id_cf_deputado' as column_name
FROM camara.cf_sessao_presenca csp
WHERE csp.id_cf_deputado IS NOT NULL 
  AND NOT EXISTS (
    SELECT 1 FROM camara.cf_deputado cd WHERE cd.id = csp.id_cf_deputado
);

-- ========================================
-- SENADO FEDERAL SCHEMA VALIDATIONS
-- ========================================

-- Check sf_lotacao_senador
SELECT 
    'sf_lotacao_senador' as constraint_name,
    'id_senador' as column_name,
    'sf_lotacao' as table_name,
    'sf_senador' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(sl.id_senador) != pg_typeof(ss.id) THEN 'DATA TYPE MISMATCH'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(sl.id_senador) as column_type,
    pg_typeof(ss.id) as referenced_type
FROM senado.sf_lotacao sl
CROSS JOIN (SELECT id FROM senado.sf_senador LIMIT 1) ss
GROUP BY validation_status, pg_typeof(sl.id_senador), pg_typeof(ss.id);

-- Show orphaned records for sf_lotacao_senador
SELECT 
    'ORPHANED RECORDS' as issue_type,
    'sf_lotacao_senador' as constraint_name,
    sl.id as record_id,
    sl.id_senador as invalid_fk_value,
    'id_senador' as column_name
FROM senado.sf_lotacao sl
WHERE sl.id_senador IS NOT NULL 
  AND NOT EXISTS (
    SELECT 1 FROM senado.sf_senador ss WHERE ss.id = sl.id_senador
);

-- Check sf_mandato_exercicio_senador
SELECT 
    'sf_mandato_exercicio_senador' as constraint_name,
    'id_sf_senador' as column_name,
    'sf_mandato_exercicio' as table_name,
    'sf_senador' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(sme.id_sf_senador) != pg_typeof(ss.id) THEN 'DATA TYPE MISMATCH'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(sme.id_sf_senador) as column_type,
    pg_typeof(ss.id) as referenced_type
FROM senado.sf_mandato_exercicio sme
CROSS JOIN (SELECT id FROM senado.sf_senador LIMIT 1) ss
GROUP BY validation_status, pg_typeof(sme.id_sf_senador), pg_typeof(ss.id);

-- Show orphaned records for sf_mandato_exercicio_senador
SELECT 
    'ORPHANED RECORDS' as issue_type,
    'sf_mandato_exercicio_senador' as constraint_name,
    sme.id as record_id,
    sme.id_sf_senador as invalid_fk_value,
    'id_sf_senador' as column_name
FROM senado.sf_mandato_exercicio sme
WHERE sme.id_sf_senador IS NOT NULL 
  AND NOT EXISTS (
    SELECT 1 FROM senado.sf_senador ss WHERE ss.id = sme.id_sf_senador
);

-- Check sf_mandato_exercicio_mandato
SELECT 
    'sf_mandato_exercicio_mandato' as constraint_name,
    'id_sf_mandato' as column_name,
    'sf_mandato_exercicio' as table_name,
    'sf_mandato' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(sme.id_sf_mandato) != pg_typeof(sm.id) THEN 'DATA TYPE MISMATCH'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(sme.id_sf_mandato) as column_type,
    pg_typeof(sm.id) as referenced_type
FROM senado.sf_mandato_exercicio sme
CROSS JOIN (SELECT id FROM senado.sf_mandato LIMIT 1) sm
GROUP BY validation_status, pg_typeof(sme.id_sf_mandato), pg_typeof(sm.id);

-- Show orphaned records for sf_mandato_exercicio_mandato
SELECT 
    'ORPHANED RECORDS' as issue_type,
    'sf_mandato_exercicio_mandato' as constraint_name,
    sme.id as record_id,
    sme.id_sf_mandato as invalid_fk_value,
    'id_sf_mandato' as column_name
FROM senado.sf_mandato_exercicio sme
WHERE sme.id_sf_mandato IS NOT NULL 
  AND NOT EXISTS (
    SELECT 1 FROM senado.sf_mandato sm WHERE sm.id = sme.id_sf_mandato
);

-- Check sf_mandato_exercicio_motivo_afastamento
SELECT 
    'sf_mandato_exercicio_motivo_afastamento' as constraint_name,
    'id_sf_motivo_afastamento' as column_name,
    'sf_mandato_exercicio' as table_name,
    'sf_motivo_afastamento' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(sme.id_sf_motivo_afastamento) != pg_typeof(sma.id) THEN 'DATA TYPE MISMATCH'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(sme.id_sf_motivo_afastamento) as column_type,
    pg_typeof(sma.id) as referenced_type
FROM senado.sf_mandato_exercicio sme
CROSS JOIN (SELECT id FROM senado.sf_motivo_afastamento LIMIT 1) sma
GROUP BY validation_status, pg_typeof(sme.id_sf_motivo_afastamento), pg_typeof(sma.id);

-- Show orphaned records for sf_mandato_exercicio_motivo_afastamento
SELECT 
    'ORPHANED RECORDS' as issue_type,
    'sf_mandato_exercicio_motivo_afastamento' as constraint_name,
    sme.id as record_id,
    sme.id_sf_motivo_afastamento as invalid_fk_value,
    'id_sf_motivo_afastamento' as column_name
FROM senado.sf_mandato_exercicio sme
WHERE sme.id_sf_motivo_afastamento IS NOT NULL 
  AND NOT EXISTS (
    SELECT 1 FROM senado.sf_motivo_afastamento sma WHERE sma.id = sme.id_sf_motivo_afastamento
);

-- Check sf_mandato_legislatura_mandato
SELECT 
    'sf_mandato_legislatura_mandato' as constraint_name,
    'id_sf_mandato' as column_name,
    'sf_mandato_legislatura' as table_name,
    'sf_mandato' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(sml.id_sf_mandato) != pg_typeof(sm.id) THEN 'DATA TYPE MISMATCH'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(sml.id_sf_mandato) as column_type,
    pg_typeof(sm.id) as referenced_type
FROM senado.sf_mandato_legislatura sml
CROSS JOIN (SELECT id FROM senado.sf_mandato LIMIT 1) sm
GROUP BY validation_status, pg_typeof(sml.id_sf_mandato), pg_typeof(sm.id);

-- Show orphaned records for sf_mandato_legislatura_mandato
SELECT 
    'ORPHANED RECORDS' as issue_type,
    'sf_mandato_legislatura_mandato' as constraint_name,
    sml.id_sf_mandato as record_id,
    sml.id_sf_mandato as invalid_fk_value,
    'id_sf_mandato' as column_name
FROM senado.sf_mandato_legislatura sml
WHERE sml.id_sf_mandato IS NOT NULL 
  AND NOT EXISTS (
    SELECT 1 FROM senado.sf_mandato sm WHERE sm.id = sml.id_sf_mandato
);

-- Check sf_mandato_legislatura_legislatura
SELECT 
    'sf_mandato_legislatura_legislatura' as constraint_name,
    'id_sf_legislatura' as column_name,
    'sf_mandato_legislatura' as table_name,
    'sf_legislatura' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(sml.id_sf_legislatura) != pg_typeof(sl.id) THEN 'DATA TYPE MISMATCH'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(sml.id_sf_legislatura) as column_type,
    pg_typeof(sl.id) as referenced_type
FROM senado.sf_mandato_legislatura sml
CROSS JOIN (SELECT id FROM senado.sf_legislatura LIMIT 1) sl
GROUP BY validation_status, pg_typeof(sml.id_sf_legislatura), pg_typeof(sl.id);

-- Show orphaned records for sf_mandato_legislatura_legislatura
SELECT 
    'ORPHANED RECORDS' as issue_type,
    'sf_mandato_legislatura_legislatura' as constraint_name,
    sml.id_sf_mandato as record_id,
    sml.id_sf_legislatura as invalid_fk_value,
    'id_sf_legislatura' as column_name
FROM senado.sf_mandato_legislatura sml
WHERE sml.id_sf_legislatura IS NOT NULL 
  AND NOT EXISTS (
    SELECT 1 FROM senado.sf_legislatura sl WHERE sl.id = sml.id_sf_legislatura
);

-- Check sf_mandato_legislatura_senador
SELECT 
    'sf_mandato_legislatura_senador' as constraint_name,
    'id_sf_senador' as column_name,
    'sf_mandato_legislatura' as table_name,
    'sf_senador' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(sml.id_sf_senador) != pg_typeof(ss.id) THEN 'DATA TYPE MISMATCH'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(sml.id_sf_senador) as column_type,
    pg_typeof(ss.id) as referenced_type
FROM senado.sf_mandato_legislatura sml
CROSS JOIN (SELECT id FROM senado.sf_senador LIMIT 1) ss
GROUP BY validation_status, pg_typeof(sml.id_sf_senador), pg_typeof(ss.id);

-- Show orphaned records for sf_mandato_legislatura_senador
SELECT 
    'ORPHANED RECORDS' as issue_type,
    'sf_mandato_legislatura_senador' as constraint_name,
    sml.id_sf_mandato as record_id,
    sml.id_sf_senador as invalid_fk_value,
    'id_sf_senador' as column_name
FROM senado.sf_mandato_legislatura sml
WHERE sml.id_sf_senador IS NOT NULL 
  AND NOT EXISTS (
    SELECT 1 FROM senado.sf_senador ss WHERE ss.id = sml.id_sf_senador
);

-- ========================================
-- ASSEMBLEIAS LEGISLATIVAS SCHEMA VALIDATIONS
-- ========================================

-- Check cl_despesa_deputado
SELECT 
    'cl_despesa_deputado' as constraint_name,
    'id_cl_deputado' as column_name,
    'cl_despesa' as table_name,
    'cl_deputado' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(cd.id_cl_deputado) != pg_typeof(cld.id) THEN 'DATA TYPE MISMATCH'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(cd.id_cl_deputado) as column_type,
    pg_typeof(cld.id) as referenced_type
FROM assembleias.cl_despesa cd
CROSS JOIN (SELECT id FROM assembleias.cl_deputado LIMIT 1) cld
GROUP BY validation_status, pg_typeof(cd.id_cl_deputado), pg_typeof(cld.id);

-- Show orphaned records for cl_despesa_deputado
SELECT 
    'ORPHANED RECORDS' as issue_type,
    'cl_despesa_deputado' as constraint_name,
    cd.id as record_id,
    cd.id_cl_deputado as invalid_fk_value,
    'id_cl_deputado' as column_name
FROM assembleias.cl_despesa cd
WHERE cd.id_cl_deputado IS NOT NULL 
  AND NOT EXISTS (
    SELECT 1 FROM assembleias.cl_deputado cld WHERE cld.id = cd.id_cl_deputado
);

-- ========================================
-- TSE SCHEMA VALIDATIONS
-- ========================================

-- Check tse_eleicao_candidatura_candidato
SELECT 
    'tse_eleicao_candidatura_candidato' as constraint_name,
    'id_eleicao_candidato' as column_name,
    'tse_eleicao_candidatura' as table_name,
    'tse_eleicao_candidato' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(tec.id_eleicao_candidato) != pg_typeof(tec2.id) THEN 'DATA TYPE MISMATCH'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(tec.id_eleicao_candidato) as column_type,
    pg_typeof(tec2.id) as referenced_type
FROM tse.tse_eleicao_candidatura tec
CROSS JOIN (SELECT id FROM tse.tse_eleicao_candidato LIMIT 1) tec2
GROUP BY validation_status, pg_typeof(tec.id_eleicao_candidato), pg_typeof(tec2.id);

-- Show orphaned records for tse_eleicao_candidatura_candidato
SELECT 
    'ORPHANED RECORDS' as issue_type,
    'tse_eleicao_candidatura_candidato' as constraint_name,
    tec.numero || '-' || tec.cargo || '-' || tec.ano || '-' || tec.sigla_estado as record_id,
    tec.id_eleicao_candidato as invalid_fk_value,
    'id_eleicao_candidato' as column_name
FROM tse.tse_eleicao_candidatura tec
WHERE tec.id_eleicao_candidato IS NOT NULL 
  AND NOT EXISTS (
    SELECT 1 FROM tse.tse_eleicao_candidato tec2 WHERE tec2.id = tec.id_eleicao_candidato
);

-- Check tse_eleicao_candidatura_candidato_vice
SELECT 
    'tse_eleicao_candidatura_candidato_vice' as constraint_name,
    'id_eleicao_candidato_vice' as column_name,
    'tse_eleicao_candidatura' as table_name,
    'tse_eleicao_candidato' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(tec.id_eleicao_candidato_vice) != pg_typeof(tec2.id) THEN 'DATA TYPE MISMATCH'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(tec.id_eleicao_candidato_vice) as column_type,
    pg_typeof(tec2.id) as referenced_type
FROM tse.tse_eleicao_candidatura tec
CROSS JOIN (SELECT id FROM tse.tse_eleicao_candidato LIMIT 1) tec2
GROUP BY validation_status, pg_typeof(tec.id_eleicao_candidato_vice), pg_typeof(tec2.id);

-- Show orphaned records for tse_eleicao_candidatura_candidato_vice
SELECT 
    'ORPHANED RECORDS' as issue_type,
    'tse_eleicao_candidatura_candidato_vice' as constraint_name,
    tec.numero || '-' || tec.cargo || '-' || tec.ano || '-' || tec.sigla_estado as record_id,
    tec.id_eleicao_candidato_vice as invalid_fk_value,
    'id_eleicao_candidato_vice' as column_name
FROM tse.tse_eleicao_candidatura tec
WHERE tec.id_eleicao_candidato_vice IS NOT NULL 
  AND NOT EXISTS (
    SELECT 1 FROM tse.tse_eleicao_candidato tec2 WHERE tec2.id = tec.id_eleicao_candidato_vice
);

-- Check tse_eleicao_candidatura_cargo
SELECT 
    'tse_eleicao_candidatura_cargo' as constraint_name,
    'cargo' as column_name,
    'tse_eleicao_candidatura' as table_name,
    'tse_eleicao_cargo' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(tec.cargo) != pg_typeof(tec2.id) THEN 'DATA TYPE MISMATCH'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(tec.cargo) as column_type,
    pg_typeof(tec2.id) as referenced_type
FROM tse.tse_eleicao_candidatura tec
CROSS JOIN (SELECT id FROM tse.tse_eleicao_cargo LIMIT 1) tec2
GROUP BY validation_status, pg_typeof(tec.cargo), pg_typeof(tec2.id);

-- Show orphaned records for tse_eleicao_candidatura_cargo
SELECT 
    'ORPHANED RECORDS' as issue_type,
    'tse_eleicao_candidatura_cargo' as constraint_name,
    tec.numero || '-' || tec.cargo || '-' || tec.ano || '-' || tec.sigla_estado as record_id,
    tec.cargo as invalid_fk_value,
    'cargo' as column_name
FROM tse.tse_eleicao_candidatura tec
WHERE tec.cargo IS NOT NULL 
  AND NOT EXISTS (
    SELECT 1 FROM tse.tse_eleicao_cargo tec2 WHERE tec2.id = tec.cargo
);

-- ========================================
-- SUMMARY OF ALL ORPHANED RECORDS
-- ========================================

SELECT 
    'ALL ORPHANED RECORDS SUMMARY' as report_type,
    constraint_name,
    COUNT(*) as orphaned_count,
    STRING_AGG(CAST(record_id AS TEXT), ', ' ORDER BY record_id) as orphaned_ids
FROM (
    -- Combine all orphaned record queries here
    -- This is a placeholder - run individual queries above
    SELECT 'placeholder' as constraint_name, 1 as record_id
    WHERE 1=0
) all_orphans
GROUP BY constraint_name
ORDER BY orphaned_count DESC;
