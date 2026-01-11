-- SQL Commands to Identify Invalid Foreign Key Constraints
-- This script checks for data type mismatches and orphaned records

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
        WHEN f.id_fornecedor_faixa_etaria IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM fornecedor.fornecedor_faixa_etaria fa WHERE fa.id = f.id_fornecedor_faixa_etaria
        ) THEN 'ORPHANED RECORDS'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(f.id_fornecedor_faixa_etaria) as column_type,
    pg_typeof(fa.id) as referenced_type
FROM fornecedor.fornecedor_socio f
CROSS JOIN (SELECT id FROM fornecedor.fornecedor_faixa_etaria LIMIT 1) fa
WHERE f.id_fornecedor_faixa_etaria IS NOT NULL
GROUP BY validation_status, pg_typeof(f.id_fornecedor_faixa_etaria), pg_typeof(fa.id);

-- Check data type compatibility for fornecedor_socio_qualificacao
SELECT 
    'fornecedor_socio_qualificacao' as constraint_name,
    'id_fornecedor_socio_qualificacao' as column_name,
    'fornecedor_socio' as table_name,
    'fornecedor_socio_qualificacao' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(f.id_fornecedor_socio_qualificacao) != pg_typeof(fsq.id) THEN 'DATA TYPE MISMATCH'
        WHEN f.id_fornecedor_socio_qualificacao IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM fornecedor.fornecedor_socio_qualificacao fsq WHERE fsq.id = f.id_fornecedor_socio_qualificacao
        ) THEN 'ORPHANED RECORDS'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(f.id_fornecedor_socio_qualificacao) as column_type,
    pg_typeof(fsq.id) as referenced_type
FROM fornecedor.fornecedor_socio f
CROSS JOIN (SELECT id FROM fornecedor.fornecedor_socio_qualificacao LIMIT 1) fsq
WHERE f.id_fornecedor_socio_qualificacao IS NOT NULL
GROUP BY validation_status, pg_typeof(f.id_fornecedor_socio_qualificacao), pg_typeof(fsq.id);

-- Check data type compatibility for fornecedor_socio_representante_qualificacao
SELECT 
    'fornecedor_socio_representante_qualificacao' as constraint_name,
    'id_fornecedor_socio_representante_qualificacao' as column_name,
    'fornecedor_socio' as table_name,
    'fornecedor_socio_qualificacao' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(f.id_fornecedor_socio_representante_qualificacao) != pg_typeof(fsq.id) THEN 'DATA TYPE MISMATCH'
        WHEN f.id_fornecedor_socio_representante_qualificacao IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM fornecedor.fornecedor_socio_qualificacao fsq WHERE fsq.id = f.id_fornecedor_socio_representante_qualificacao
        ) THEN 'ORPHANED RECORDS'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(f.id_fornecedor_socio_representante_qualificacao) as column_type,
    pg_typeof(fsq.id) as referenced_type
FROM fornecedor.fornecedor_socio f
CROSS JOIN (SELECT id FROM fornecedor.fornecedor_socio_qualificacao LIMIT 1) fsq
WHERE f.id_fornecedor_socio_representante_qualificacao IS NOT NULL
GROUP BY validation_status, pg_typeof(f.id_fornecedor_socio_representante_qualificacao), pg_typeof(fsq.id);

-- Check forcecedor_cnpj_incorreto (note: table name typo)
SELECT 
    'forcecedor_cnpj_incorreto_fornecedor_incorreto' as constraint_name,
    'id_fornecedor_incorreto' as column_name,
    'forcecedor_cnpj_incorreto' as table_name,
    'fornecedor' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(fci.id_fornecedor_incorreto) != pg_typeof(f.id) THEN 'DATA TYPE MISMATCH'
        WHEN fci.id_fornecedor_incorreto IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM fornecedor.fornecedor f WHERE f.id = fci.id_fornecedor_incorreto
        ) THEN 'ORPHANED RECORDS'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(fci.id_fornecedor_incorreto) as column_type,
    pg_typeof(f.id) as referenced_type
FROM fornecedor.forcecedor_cnpj_incorreto fci
CROSS JOIN (SELECT id FROM fornecedor.fornecedor LIMIT 1) f
WHERE fci.id_fornecedor_incorreto IS NOT NULL
GROUP BY validation_status, pg_typeof(fci.id_fornecedor_incorreto), pg_typeof(f.id);

SELECT 
    'forcecedor_cnpj_incorreto_fornecedor_correto' as constraint_name,
    'id_fornecedor_correto' as column_name,
    'forcecedor_cnpj_incorreto' as table_name,
    'fornecedor' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(fci.id_fornecedor_correto) != pg_typeof(f.id) THEN 'DATA TYPE MISMATCH'
        WHEN fci.id_fornecedor_correto IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM fornecedor.fornecedor f WHERE f.id = fci.id_fornecedor_correto
        ) THEN 'ORPHANED RECORDS'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(fci.id_fornecedor_correto) as column_type,
    pg_typeof(f.id) as referenced_type
FROM fornecedor.forcecedor_cnpj_incorreto fci
CROSS JOIN (SELECT id FROM fornecedor.fornecedor LIMIT 1) f
WHERE fci.id_fornecedor_correto IS NOT NULL
GROUP BY validation_status, pg_typeof(fci.id_fornecedor_correto), pg_typeof(f.id);

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
        WHEN cd.id_cf_despesa_tipo IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM camara.cf_despesa_tipo cdt WHERE cdt.id = cd.id_cf_despesa_tipo
        ) THEN 'ORPHANED RECORDS'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(cd.id_cf_despesa_tipo) as column_type,
    pg_typeof(cdt.id) as referenced_type
FROM camara.cf_despesa cd
CROSS JOIN (SELECT id FROM camara.cf_despesa_tipo LIMIT 1) cdt
WHERE cd.id_cf_despesa_tipo IS NOT NULL
GROUP BY validation_status, pg_typeof(cd.id_cf_despesa_tipo), pg_typeof(cdt.id);

-- Check cf_despesa_especificacao
SELECT 
    'cf_despesa_especificacao' as constraint_name,
    'id_cf_especificacao' as column_name,
    'cf_despesa' as table_name,
    'cf_especificacao_tipo' as referenced_table,
    'id_cf_especificacao' as referenced_column,
    CASE 
        WHEN pg_typeof(cd.id_cf_especificacao) != pg_typeof(cet.id_cf_especificacao) THEN 'DATA TYPE MISMATCH'
        WHEN cd.id_cf_especificacao IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM camara.cf_especificacao_tipo cet WHERE cet.id_cf_especificacao = cd.id_cf_especificacao
        ) THEN 'ORPHANED RECORDS'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(cd.id_cf_especificacao) as column_type,
    pg_typeof(cet.id_cf_especificacao) as referenced_type
FROM camara.cf_despesa cd
CROSS JOIN (SELECT id_cf_especificacao FROM camara.cf_especificacao_tipo LIMIT 1) cet
WHERE cd.id_cf_especificacao IS NOT NULL
GROUP BY validation_status, pg_typeof(cd.id_cf_especificacao), pg_typeof(cet.id_cf_especificacao);

-- Check cf_despesa_fornecedor
SELECT 
    'cf_despesa_fornecedor' as constraint_name,
    'id_fornecedor' as column_name,
    'cf_despesa' as table_name,
    'fornecedor' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(cd.id_fornecedor) != pg_typeof(f.id) THEN 'DATA TYPE MISMATCH'
        WHEN cd.id_fornecedor IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM fornecedor.fornecedor f WHERE f.id = cd.id_fornecedor
        ) THEN 'ORPHANED RECORDS'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(cd.id_fornecedor) as column_type,
    pg_typeof(f.id) as referenced_type
FROM camara.cf_despesa cd
CROSS JOIN (SELECT id FROM fornecedor.fornecedor LIMIT 1) f
WHERE cd.id_fornecedor IS NOT NULL
GROUP BY validation_status, pg_typeof(cd.id_fornecedor), pg_typeof(f.id);

-- Check cf_mandato_deputado
SELECT 
    'cf_mandato_deputado' as constraint_name,
    'id_cf_deputado' as column_name,
    'cf_mandato' as table_name,
    'cf_deputado' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(cm.id_cf_deputado) != pg_typeof(cd.id) THEN 'DATA TYPE MISMATCH'
        WHEN cm.id_cf_deputado IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM camara.cf_deputado cd WHERE cd.id = cm.id_cf_deputado
        ) THEN 'ORPHANED RECORDS'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(cm.id_cf_deputado) as column_type,
    pg_typeof(cd.id) as referenced_type
FROM camara.cf_mandato cm
CROSS JOIN (SELECT id FROM camara.cf_deputado LIMIT 1) cd
WHERE cm.id_cf_deputado IS NOT NULL
GROUP BY validation_status, pg_typeof(cm.id_cf_deputado), pg_typeof(cd.id);

-- Check cf_senador_verba_gabinete_senador
SELECT 
    'cf_senador_verba_gabinete_senador' as constraint_name,
    'id_sf_senador' as column_name,
    'cf_senador_verba_gabinete' as table_name,
    'sf_senador' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(csvg.id_sf_senador) != pg_typeof(ss.id) THEN 'DATA TYPE MISMATCH'
        WHEN csvg.id_sf_senador IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM senado.sf_senador ss WHERE ss.id = csvg.id_sf_senador
        ) THEN 'ORPHANED RECORDS'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(csvg.id_sf_senador) as column_type,
    pg_typeof(ss.id) as referenced_type
FROM camara.cf_senador_verba_gabinete csvg
CROSS JOIN (SELECT id FROM senado.sf_senador LIMIT 1) ss
WHERE csvg.id_sf_senador IS NOT NULL
GROUP BY validation_status, pg_typeof(csvg.id_sf_senador), pg_typeof(ss.id);

-- Check cf_sessao_presenca_deputado
SELECT 
    'cf_sessao_presenca_deputado' as constraint_name,
    'id_cf_deputado' as column_name,
    'cf_sessao_presenca' as table_name,
    'cf_deputado' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(csp.id_cf_deputado) != pg_typeof(cd.id) THEN 'DATA TYPE MISMATCH'
        WHEN csp.id_cf_deputado IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM camara.cf_deputado cd WHERE cd.id = csp.id_cf_deputado
        ) THEN 'ORPHANED RECORDS'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(csp.id_cf_deputado) as column_type,
    pg_typeof(cd.id) as referenced_type
FROM camara.cf_sessao_presenca csp
CROSS JOIN (SELECT id FROM camara.cf_deputado LIMIT 1) cd
WHERE csp.id_cf_deputado IS NOT NULL
GROUP BY validation_status, pg_typeof(csp.id_cf_deputado), pg_typeof(cd.id);

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
        WHEN sl.id_senador IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM senado.sf_senador ss WHERE ss.id = sl.id_senador
        ) THEN 'ORPHANED RECORDS'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(sl.id_senador) as column_type,
    pg_typeof(ss.id) as referenced_type
FROM senado.sf_lotacao sl
CROSS JOIN (SELECT id FROM senado.sf_senador LIMIT 1) ss
WHERE sl.id_senador IS NOT NULL
GROUP BY validation_status, pg_typeof(sl.id_senador), pg_typeof(ss.id);

-- Check sf_mandato_exercicio_senador
SELECT 
    'sf_mandato_exercicio_senador' as constraint_name,
    'id_sf_senador' as column_name,
    'sf_mandato_exercicio' as table_name,
    'sf_senador' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(sme.id_sf_senador) != pg_typeof(ss.id) THEN 'DATA TYPE MISMATCH'
        WHEN sme.id_sf_senador IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM senado.sf_senador ss WHERE ss.id = sme.id_sf_senador
        ) THEN 'ORPHANED RECORDS'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(sme.id_sf_senador) as column_type,
    pg_typeof(ss.id) as referenced_type
FROM senado.sf_mandato_exercicio sme
CROSS JOIN (SELECT id FROM senado.sf_senador LIMIT 1) ss
WHERE sme.id_sf_senador IS NOT NULL
GROUP BY validation_status, pg_typeof(sme.id_sf_senador), pg_typeof(ss.id);

-- Check sf_mandato_exercicio_mandato
SELECT 
    'sf_mandato_exercicio_mandato' as constraint_name,
    'id_sf_mandato' as column_name,
    'sf_mandato_exercicio' as table_name,
    'sf_mandato' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(sme.id_sf_mandato) != pg_typeof(sm.id) THEN 'DATA TYPE MISMATCH'
        WHEN sme.id_sf_mandato IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM senado.sf_mandato sm WHERE sm.id = sme.id_sf_mandato
        ) THEN 'ORPHANED RECORDS'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(sme.id_sf_mandato) as column_type,
    pg_typeof(sm.id) as referenced_type
FROM senado.sf_mandato_exercicio sme
CROSS JOIN (SELECT id FROM senado.sf_mandato LIMIT 1) sm
WHERE sme.id_sf_mandato IS NOT NULL
GROUP BY validation_status, pg_typeof(sme.id_sf_mandato), pg_typeof(sm.id);

-- Check sf_mandato_exercicio_motivo_afastamento
SELECT 
    'sf_mandato_exercicio_motivo_afastamento' as constraint_name,
    'id_sf_motivo_afastamento' as column_name,
    'sf_mandato_exercicio' as table_name,
    'sf_motivo_afastamento' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(sme.id_sf_motivo_afastamento) != pg_typeof(sma.id) THEN 'DATA TYPE MISMATCH'
        WHEN sme.id_sf_motivo_afastamento IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM senado.sf_motivo_afastamento sma WHERE sma.id = sme.id_sf_motivo_afastamento
        ) THEN 'ORPHANED RECORDS'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(sme.id_sf_motivo_afastamento) as column_type,
    pg_typeof(sma.id) as referenced_type
FROM senado.sf_mandato_exercicio sme
CROSS JOIN (SELECT id FROM senado.sf_motivo_afastamento LIMIT 1) sma
WHERE sme.id_sf_motivo_afastamento IS NOT NULL
GROUP BY validation_status, pg_typeof(sme.id_sf_motivo_afastamento), pg_typeof(sma.id);

-- Check sf_mandato_legislatura_mandato
SELECT 
    'sf_mandato_legislatura_mandato' as constraint_name,
    'id_sf_mandato' as column_name,
    'sf_mandato_legislatura' as table_name,
    'sf_mandato' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(sml.id_sf_mandato) != pg_typeof(sm.id) THEN 'DATA TYPE MISMATCH'
        WHEN sml.id_sf_mandato IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM senado.sf_mandato sm WHERE sm.id = sml.id_sf_mandato
        ) THEN 'ORPHANED RECORDS'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(sml.id_sf_mandato) as column_type,
    pg_typeof(sm.id) as referenced_type
FROM senado.sf_mandato_legislatura sml
CROSS JOIN (SELECT id FROM senado.sf_mandato LIMIT 1) sm
WHERE sml.id_sf_mandato IS NOT NULL
GROUP BY validation_status, pg_typeof(sml.id_sf_mandato), pg_typeof(sm.id);

-- Check sf_mandato_legislatura_legislatura
SELECT 
    'sf_mandato_legislatura_legislatura' as constraint_name,
    'id_sf_legislatura' as column_name,
    'sf_mandato_legislatura' as table_name,
    'sf_legislatura' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(sml.id_sf_legislatura) != pg_typeof(sl.id) THEN 'DATA TYPE MISMATCH'
        WHEN sml.id_sf_legislatura IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM senado.sf_legislatura sl WHERE sl.id = sml.id_sf_legislatura
        ) THEN 'ORPHANED RECORDS'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(sml.id_sf_legislatura) as column_type,
    pg_typeof(sl.id) as referenced_type
FROM senado.sf_mandato_legislatura sml
CROSS JOIN (SELECT id FROM senado.sf_legislatura LIMIT 1) sl
WHERE sml.id_sf_legislatura IS NOT NULL
GROUP BY validation_status, pg_typeof(sml.id_sf_legislatura), pg_typeof(sl.id);

-- Check sf_mandato_legislatura_senador
SELECT 
    'sf_mandato_legislatura_senador' as constraint_name,
    'id_sf_senador' as column_name,
    'sf_mandato_legislatura' as table_name,
    'sf_senador' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(sml.id_sf_senador) != pg_typeof(ss.id) THEN 'DATA TYPE MISMATCH'
        WHEN sml.id_sf_senador IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM senado.sf_senador ss WHERE ss.id = sml.id_sf_senador
        ) THEN 'ORPHANED RECORDS'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(sml.id_sf_senador) as column_type,
    pg_typeof(ss.id) as referenced_type
FROM senado.sf_mandato_legislatura sml
CROSS JOIN (SELECT id FROM senado.sf_senador LIMIT 1) ss
WHERE sml.id_sf_senador IS NOT NULL
GROUP BY validation_status, pg_typeof(sml.id_sf_senador), pg_typeof(ss.id);

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
        WHEN cd.id_cl_deputado IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM assembleias.cl_deputado cld WHERE cld.id = cd.id_cl_deputado
        ) THEN 'ORPHANED RECORDS'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(cd.id_cl_deputado) as column_type,
    pg_typeof(cld.id) as referenced_type
FROM assembleias.cl_despesa cd
CROSS JOIN (SELECT id FROM assembleias.cl_deputado LIMIT 1) cld
WHERE cd.id_cl_deputado IS NOT NULL
GROUP BY validation_status, pg_typeof(cd.id_cl_deputado), pg_typeof(cld.id);

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
        WHEN pg_typeof(tec.id_eleicao_candidato) != pg_typeof(tec.id) THEN 'DATA TYPE MISMATCH'
        WHEN tec.id_eleicao_candidato IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM tse.tse_eleicao_candidato tec WHERE tec.id = tec.id_eleicao_candidato
        ) THEN 'ORPHANED RECORDS'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(tec.id_eleicao_candidato) as column_type,
    pg_typeof(tec.id) as referenced_type
FROM tse.tse_eleicao_candidatura tec
CROSS JOIN (SELECT id FROM tse.tse_eleicao_candidato LIMIT 1) tec
WHERE tec.id_eleicao_candidato IS NOT NULL
GROUP BY validation_status, pg_typeof(tec.id_eleicao_candidato), pg_typeof(tec.id);

-- Check tse_eleicao_candidatura_candidato_vice
SELECT 
    'tse_eleicao_candidatura_candidato_vice' as constraint_name,
    'id_eleicao_candidato_vice' as column_name,
    'tse_eleicao_candidatura' as table_name,
    'tse_eleicao_candidato' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(tec.id_eleicao_candidato_vice) != pg_typeof(tec.id) THEN 'DATA TYPE MISMATCH'
        WHEN tec.id_eleicao_candidato_vice IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM tse.tse_eleicao_candidato tec WHERE tec.id = tec.id_eleicao_candidato_vice
        ) THEN 'ORPHANED RECORDS'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(tec.id_eleicao_candidato_vice) as column_type,
    pg_typeof(tec.id) as referenced_type
FROM tse.tse_eleicao_candidatura tec
CROSS JOIN (SELECT id FROM tse.tse_eleicao_candidato LIMIT 1) tec
WHERE tec.id_eleicao_candidato_vice IS NOT NULL
GROUP BY validation_status, pg_typeof(tec.id_eleicao_candidato_vice), pg_typeof(tec.id);

-- Check tse_eleicao_candidatura_cargo
SELECT 
    'tse_eleicao_candidatura_cargo' as constraint_name,
    'cargo' as column_name,
    'tse_eleicao_candidatura' as table_name,
    'tse_eleicao_cargo' as referenced_table,
    'id' as referenced_column,
    CASE 
        WHEN pg_typeof(tec.cargo) != pg_typeof(tec2.id) THEN 'DATA TYPE MISMATCH'
        WHEN tec.cargo IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM tse.tse_eleicao_cargo tec2 WHERE tec2.id = tec.cargo
        ) THEN 'ORPHANED RECORDS'
        ELSE 'OK'
    END as validation_status,
    pg_typeof(tec.cargo) as column_type,
    pg_typeof(tec2.id) as referenced_type
FROM tse.tse_eleicao_candidatura tec
CROSS JOIN (SELECT id FROM tse.tse_eleicao_cargo LIMIT 1) tec2
WHERE tec.cargo IS NOT NULL
GROUP BY validation_status, pg_typeof(tec.cargo), pg_typeof(tec2.id);

-- ========================================
-- SUMMARY REPORT
-- ========================================

-- Get all validation results in one summary
SELECT 
    constraint_name,
    column_name,
    table_name,
    referenced_table,
    referenced_column,
    validation_status,
    column_type,
    referenced_type
FROM (
    -- All the above queries combined with UNION ALL
    -- This is a placeholder - you would need to run each query individually
    SELECT 'placeholder' as constraint_name, 'placeholder' as column_name, 
           'placeholder' as table_name, 'placeholder' as referenced_table,
           'placeholder' as referenced_column, 'RUN_QUERIES_INDIVIDUALLY' as validation_status,
           'placeholder' as column_type, 'placeholder' as referenced_type
) summary
WHERE validation_status != 'OK';
