-- Foreign Key Management Script for OPS Database
-- This script contains commands to disable and re-enable all foreign keys

-- ========================================
-- DISABLE ALL FOREIGN KEYS
-- ========================================

-- Disable fornecedor schema foreign keys
ALTER TABLE "fornecedor"."fornecedor_atividade_secundaria" DROP CONSTRAINT IF EXISTS "fk_fornecedor_atividade_secundaria_fornecedor";
ALTER TABLE "fornecedor"."fornecedor_atividade_secundaria" DROP CONSTRAINT IF EXISTS "fk_fornecedor_atividade_secundaria_atividade";

ALTER TABLE "fornecedor"."fornecedor_info" DROP CONSTRAINT IF EXISTS "fk_fornecedor_info_fornecedor";
ALTER TABLE "fornecedor"."fornecedor_info" DROP CONSTRAINT IF EXISTS "fk_fornecedor_info_atividade_principal";
ALTER TABLE "fornecedor"."fornecedor_info" DROP CONSTRAINT IF EXISTS "fk_fornecedor_info_natureza_juridica";

ALTER TABLE "fornecedor"."fornecedor_socio" DROP CONSTRAINT IF EXISTS "fk_fornecedor_socio_fornecedor";
ALTER TABLE "fornecedor"."fornecedor_socio" DROP CONSTRAINT IF EXISTS "fk_fornecedor_socio_faixa_etaria";
ALTER TABLE "fornecedor"."fornecedor_socio" DROP CONSTRAINT IF EXISTS "fk_fornecedor_socio_qualificacao";
ALTER TABLE "fornecedor"."fornecedor_socio" DROP CONSTRAINT IF EXISTS "fk_fornecedor_socio_representante_qualificacao";

-- Disable other potential foreign keys (add more as needed)
-- Example for other schemas:
-- ALTER TABLE "other_schema"."table_name" DROP CONSTRAINT IF EXISTS "constraint_name";

-- ========================================
-- ENABLE/RECREATE ALL FOREIGN KEYS
-- ========================================

-- Recreate fornecedor schema foreign keys
ALTER TABLE "fornecedor"."fornecedor_atividade_secundaria" 
ADD CONSTRAINT "fk_fornecedor_atividade_secundaria_fornecedor" 
FOREIGN KEY ("id_fornecedor") REFERENCES "fornecedor"."fornecedor" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "fornecedor"."fornecedor_atividade_secundaria" 
ADD CONSTRAINT "fk_fornecedor_atividade_secundaria_atividade" 
FOREIGN KEY ("id_fornecedor_atividade") REFERENCES "fornecedor"."fornecedor_atividade" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "fornecedor"."fornecedor_info" 
ADD CONSTRAINT "fk_fornecedor_info_fornecedor" 
FOREIGN KEY ("id_fornecedor") REFERENCES "fornecedor"."fornecedor" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "fornecedor"."fornecedor_info" 
ADD CONSTRAINT "fk_fornecedor_info_atividade_principal" 
FOREIGN KEY ("id_fornecedor_atividade_principal") REFERENCES "fornecedor"."fornecedor_atividade" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "fornecedor"."fornecedor_info" 
ADD CONSTRAINT "fk_fornecedor_info_natureza_juridica" 
FOREIGN KEY ("id_fornecedor_natureza_juridica") REFERENCES "fornecedor"."fornecedor_natureza_juridica" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "fornecedor"."fornecedor_socio" 
ADD CONSTRAINT "fk_fornecedor_socio_fornecedor" 
FOREIGN KEY ("id_fornecedor") REFERENCES "fornecedor"."fornecedor" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "fornecedor"."fornecedor_socio" 
ADD CONSTRAINT "fk_fornecedor_socio_faixa_etaria" 
FOREIGN KEY ("id_fornecedor_faixa_etaria") REFERENCES "fornecedor"."fornecedor_faixa_etaria" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "fornecedor"."fornecedor_socio" 
ADD CONSTRAINT "fk_fornecedor_socio_qualificacao" 
FOREIGN KEY ("id_fornecedor_socio_qualificacao") REFERENCES "fornecedor"."fornecedor_socio_qualificacao" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "fornecedor"."fornecedor_socio" 
ADD CONSTRAINT "fk_fornecedor_socio_representante_qualificacao" 
FOREIGN KEY ("id_fornecedor_socio_representante_qualificacao") REFERENCES "fornecedor"."fornecedor_socio_qualificacao" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- ========================================
-- UTILITY QUERIES
-- ========================================

-- Query to check current foreign key status
SELECT 
    tc.table_name, 
    tc.constraint_name, 
    tc.constraint_type,
    kcu.column_name,
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name 
FROM 
    information_schema.table_constraints AS tc 
    JOIN information_schema.key_column_usage AS kcu
      ON tc.constraint_name = kcu.constraint_name
      AND tc.table_schema = kcu.table_schema
    JOIN information_schema.constraint_column_usage AS ccu
      ON ccu.constraint_name = tc.constraint_name
      AND ccu.table_schema = tc.table_schema
WHERE 
    tc.constraint_type = 'FOREIGN KEY' 
    AND tc.table_schema = 'fornecedor'
ORDER BY 
    tc.table_name, tc.constraint_name;

-- Query to check all foreign keys in all schemas
SELECT 
    tc.table_schema,
    tc.table_name, 
    tc.constraint_name, 
    tc.constraint_type,
    kcu.column_name,
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name 
FROM 
    information_schema.table_constraints AS tc 
    JOIN information_schema.key_column_usage AS kcu
      ON tc.constraint_name = kcu.constraint_name
      AND tc.table_schema = kcu.table_schema
    JOIN information_schema.constraint_column_usage AS ccu
      ON ccu.constraint_name = tc.constraint_name
      AND ccu.table_schema = tc.table_schema
WHERE 
    tc.constraint_type = 'FOREIGN KEY' 
ORDER BY 
    tc.table_schema, tc.table_name, tc.constraint_name;
