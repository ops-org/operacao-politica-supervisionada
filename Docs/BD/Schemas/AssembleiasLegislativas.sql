/*
 Navicat Premium Dump SQL

 Source Server         : Postgres WSL
 Source Server Type    : PostgreSQL
 Source Server Version : 160011 (160011)
 Source Host           : 172.31.250.64:5432
 Source Catalog        : ops
 Source Schema         : assembleias

 Target Server Type    : PostgreSQL
 Target Server Version : 160011 (160011)
 File Encoding         : 65001

 Date: 06/01/2026 20:54:10
*/


-- ----------------------------
-- Table structure for cl_deputado
-- ----------------------------
DROP TABLE IF EXISTS "assembleias"."cl_deputado";
CREATE TABLE "assembleias"."cl_deputado" (
  "id" int4 NOT NULL,
  "matricula" int4,
  "gabinete" int4,
  "id_partido" int2 NOT NULL,
  "id_estado" int2 NOT NULL,
  "cpf" varchar(11) COLLATE "pg_catalog"."default",
  "cpf_parcial" varchar(6) COLLATE "pg_catalog"."default",
  "nome_parlamentar" varchar(255) COLLATE "pg_catalog"."default" NOT NULL,
  "nome_civil" varchar(255) COLLATE "pg_catalog"."default",
  "nome_importacao" varchar(255) COLLATE "pg_catalog"."default",
  "nascimento" date,
  "sexo" char(2) COLLATE "pg_catalog"."default",
  "email" varchar(100) COLLATE "pg_catalog"."default",
  "naturalidade" varchar(100) COLLATE "pg_catalog"."default",
  "escolaridade" varchar(100) COLLATE "pg_catalog"."default",
  "profissao" varchar(150) COLLATE "pg_catalog"."default",
  "telefone" varchar(100) COLLATE "pg_catalog"."default",
  "site" varchar(500) COLLATE "pg_catalog"."default",
  "perfil" varchar(100) COLLATE "pg_catalog"."default",
  "foto" varchar(200) COLLATE "pg_catalog"."default",
  "valor_total_ceap" numeric(12,2) NOT NULL,
  "valor_total_remuneracao" numeric(12,2) NOT NULL
)
;
COMMENT ON TABLE "assembleias"."cl_deputado" IS 'Câmara Legislativa - Deputados Estaduais';

-- ----------------------------
-- Table structure for cl_deputado_campeao_gasto
-- ----------------------------
DROP TABLE IF EXISTS "assembleias"."cl_deputado_campeao_gasto";
CREATE TABLE "assembleias"."cl_deputado_campeao_gasto" (
  "id_cl_deputado" int4 NOT NULL,
  "nome_parlamentar" varchar(100) COLLATE "pg_catalog"."default",
  "valor_total" numeric(10,2),
  "sigla_partido" varchar(20) COLLATE "pg_catalog"."default",
  "sigla_estado" varchar(2) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for cl_despesa
-- ----------------------------
DROP TABLE IF EXISTS "assembleias"."cl_despesa";
CREATE TABLE "assembleias"."cl_despesa" (
  "id" int8 NOT NULL,
  "id_cl_deputado" int8 NOT NULL,
  "id_cl_despesa_tipo" int8,
  "id_cl_despesa_especificacao" int8,
  "id_fornecedor" int8,
  "data_emissao" date,
  "ano_mes" int4,
  "numero_documento" varchar(50) COLLATE "pg_catalog"."default",
  "valor_liquido" numeric(10,2) NOT NULL,
  "favorecido" varchar(200) COLLATE "pg_catalog"."default",
  "observacao" varchar(8000) COLLATE "pg_catalog"."default",
  "hash" bytea
)
;

-- ----------------------------
-- Table structure for cl_despesa_especificacao
-- ----------------------------
DROP TABLE IF EXISTS "assembleias"."cl_despesa_especificacao";
CREATE TABLE "assembleias"."cl_despesa_especificacao" (
  "id" int4 NOT NULL,
  "id_cl_despesa_tipo" int4,
  "descricao" varchar(400) COLLATE "pg_catalog"."default" NOT NULL
)
;

-- ----------------------------
-- Table structure for cl_despesa_resumo_mensal
-- ----------------------------
DROP TABLE IF EXISTS "assembleias"."cl_despesa_resumo_mensal";
CREATE TABLE "assembleias"."cl_despesa_resumo_mensal" (
  "ano" int8 NOT NULL,
  "mes" int8 NOT NULL,
  "valor" numeric(10,2)
)
;

-- ----------------------------
-- Table structure for cl_despesa_tipo
-- ----------------------------
DROP TABLE IF EXISTS "assembleias"."cl_despesa_tipo";
CREATE TABLE "assembleias"."cl_despesa_tipo" (
  "id" int4 NOT NULL,
  "descricao" varchar(150) COLLATE "pg_catalog"."default" NOT NULL
)
;

-- ----------------------------
-- Indexes structure for table cl_deputado
-- ----------------------------
CREATE UNIQUE INDEX "cl_cpf" ON "assembleias"."cl_deputado" USING btree (
  "cpf" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE UNIQUE INDEX "cpf_parcial" ON "assembleias"."cl_deputado" USING btree (
  "cpf_parcial" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE UNIQUE INDEX "email" ON "assembleias"."cl_deputado" USING btree (
  "email" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE UNIQUE INDEX "id_estado_nome_civil" ON "assembleias"."cl_deputado" USING btree (
  "id_estado" "pg_catalog"."int2_ops" ASC NULLS LAST,
  "nome_civil" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table cl_deputado
-- ----------------------------
ALTER TABLE "assembleias"."cl_deputado" ADD CONSTRAINT "cl_deputado_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table cl_deputado_campeao_gasto
-- ----------------------------
ALTER TABLE "assembleias"."cl_deputado_campeao_gasto" ADD CONSTRAINT "cl_deputado_campeao_gasto_pkey" PRIMARY KEY ("id_cl_deputado");

-- ----------------------------
-- Indexes structure for table cl_despesa
-- ----------------------------
CREATE UNIQUE INDEX "id_cl_deputado_ano_mes_hash" ON "assembleias"."cl_despesa" USING btree (
  "id_cl_deputado" "pg_catalog"."int8_ops" ASC NULLS LAST,
  "ano_mes" "pg_catalog"."int4_ops" ASC NULLS LAST,
  "hash" "pg_catalog"."bytea_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table cl_despesa
-- ----------------------------
ALTER TABLE "assembleias"."cl_despesa" ADD CONSTRAINT "cl_despesa_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table cl_despesa_especificacao
-- ----------------------------
ALTER TABLE "assembleias"."cl_despesa_especificacao" ADD CONSTRAINT "cl_despesa_especificacao_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table cl_despesa_resumo_mensal
-- ----------------------------
ALTER TABLE "assembleias"."cl_despesa_resumo_mensal" ADD CONSTRAINT "cl_despesa_resumo_mensal_pkey" PRIMARY KEY ("ano", "mes");

-- ----------------------------
-- Primary Key structure for table cl_despesa_tipo
-- ----------------------------
ALTER TABLE "assembleias"."cl_despesa_tipo" ADD CONSTRAINT "cl_despesa_tipo_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Foreign Keys structure for table cl_deputado_campeao_gasto
-- ----------------------------
ALTER TABLE "assembleias"."cl_deputado_campeao_gasto" ADD CONSTRAINT "cl_deputado_campeao_gasto_id_cl_deputado_fkey" FOREIGN KEY ("id_cl_deputado") REFERENCES "public"."cl_deputado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;
