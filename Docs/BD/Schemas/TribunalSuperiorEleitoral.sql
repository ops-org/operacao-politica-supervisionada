/*
 Navicat Premium Dump SQL

 Source Server         : Postgres WSL
 Source Server Type    : PostgreSQL
 Source Server Version : 160011 (160011)
 Source Host           : 172.31.250.64:5432
 Source Catalog        : ops
 Source Schema         : tse

 Target Server Type    : PostgreSQL
 Target Server Version : 160011 (160011)
 File Encoding         : 65001

 Date: 06/01/2026 20:55:49
*/


-- ----------------------------
-- Table structure for tse_eleicao
-- ----------------------------
DROP TABLE IF EXISTS "tse"."tse_eleicao";
CREATE TABLE "tse"."tse_eleicao" (
  "id" int8,
  "descricao" varchar(50) COLLATE "pg_catalog"."default",
  "data" date,
  "turno" varchar(50) COLLATE "pg_catalog"."default",
  "tipo" varchar(50) COLLATE "pg_catalog"."default",
  "abrangencia" varchar(50) COLLATE "pg_catalog"."default"
)
;
COMMENT ON TABLE "tse"."tse_eleicao" IS 'CD_ELEICAO, DS_ELEICAO, DT_ELEICAO, NM_TIPO_ELEICAO, NR_TURNO, TP_ABRANGENCIA';

-- ----------------------------
-- Table structure for tse_eleicao_candidato
-- ----------------------------
DROP TABLE IF EXISTS "tse"."tse_eleicao_candidato";
CREATE TABLE "tse"."tse_eleicao_candidato" (
  "id" int4 NOT NULL,
  "cpf" varchar(11) COLLATE "pg_catalog"."default",
  "nome" varchar(255) COLLATE "pg_catalog"."default",
  "email" varchar(255) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for tse_eleicao_candidatura
-- ----------------------------
DROP TABLE IF EXISTS "tse"."tse_eleicao_candidatura";
CREATE TABLE "tse"."tse_eleicao_candidatura" (
  "numero" int8 NOT NULL,
  "cargo" int2 NOT NULL,
  "ano" int8 NOT NULL,
  "id_eleicao_candidato" int4,
  "id_eleicao_candidato_vice" int4,
  "sigla_partido" varchar(50) COLLATE "pg_catalog"."default",
  "sigla_partido_vice" varchar(50) COLLATE "pg_catalog"."default",
  "nome_urna" varchar(255) COLLATE "pg_catalog"."default",
  "nome_urna_vice" varchar(255) COLLATE "pg_catalog"."default",
  "sequencia" varchar(50) COLLATE "pg_catalog"."default",
  "sequencia_vice" varchar(50) COLLATE "pg_catalog"."default",
  "sigla_estado" char(2) COLLATE "pg_catalog"."default" NOT NULL
)
;

-- ----------------------------
-- Table structure for tse_eleicao_cargo
-- ----------------------------
DROP TABLE IF EXISTS "tse"."tse_eleicao_cargo";
CREATE TABLE "tse"."tse_eleicao_cargo" (
  "id" int8 NOT NULL,
  "nome" varchar(50) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for tse_eleicao_doacao
-- ----------------------------
DROP TABLE IF EXISTS "tse"."tse_eleicao_doacao";
CREATE TABLE "tse"."tse_eleicao_doacao" (
  "id" int8 NOT NULL,
  "id_eleicao_cargo" int8,
  "id_eleicao_candidadto" int8,
  "ano_eleicao" numeric(4,0),
  "num_documento" varchar(50) COLLATE "pg_catalog"."default",
  "cnpj_cpf_doador" varchar(14) COLLATE "pg_catalog"."default",
  "raiz_cnpj_cpf_doador" varchar(14) COLLATE "pg_catalog"."default",
  "data_receita" date,
  "valor_receita" numeric(10,2)
)
;

-- ----------------------------
-- Indexes structure for table tse_eleicao_candidato
-- ----------------------------
CREATE UNIQUE INDEX "cpf_UNIQUE" ON "tse"."tse_eleicao_candidato" USING btree (
  "cpf" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table tse_eleicao_candidato
-- ----------------------------
ALTER TABLE "tse"."tse_eleicao_candidato" ADD CONSTRAINT "tse_eleicao_candidato_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table tse_eleicao_candidatura
-- ----------------------------
ALTER TABLE "tse"."tse_eleicao_candidatura" ADD CONSTRAINT "tse_eleicao_candidatura_pkey" PRIMARY KEY ("numero", "cargo", "ano", "sigla_estado");

-- ----------------------------
-- Indexes structure for table tse_eleicao_cargo
-- ----------------------------
CREATE UNIQUE INDEX "nome_UNIQUE" ON "tse"."tse_eleicao_cargo" USING btree (
  "nome" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table tse_eleicao_cargo
-- ----------------------------
ALTER TABLE "tse"."tse_eleicao_cargo" ADD CONSTRAINT "tse_eleicao_cargo_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table tse_eleicao_doacao
-- ----------------------------
ALTER TABLE "tse"."tse_eleicao_doacao" ADD CONSTRAINT "tse_eleicao_doacao_pkey" PRIMARY KEY ("id");
