/*
 Navicat Premium Dump SQL

 Source Server         : Postgres WSL
 Source Server Type    : PostgreSQL
 Source Server Version : 160011 (160011)
 Source Host           : 172.31.250.64:5432
 Source Catalog        : ops
 Source Schema         : public

 Target Server Type    : PostgreSQL
 Target Server Version : 160011 (160011)
 File Encoding         : 65001

 Date: 08/01/2026 13:56:28
*/


-- ----------------------------
-- Table structure for estado
-- ----------------------------
DROP TABLE IF EXISTS "public"."estado";
CREATE TABLE "public"."estado" (
  "id" int2 NOT NULL,
  "sigla" char(2) COLLATE "pg_catalog"."default" NOT NULL,
  "nome" varchar(30) COLLATE "pg_catalog"."default" NOT NULL,
  "regiao" varchar(30) COLLATE "pg_catalog"."default" NOT NULL
)
;
COMMENT ON COLUMN "public"."estado"."id" IS 'Código no IBGE';
COMMENT ON TABLE "public"."estado" IS 'https://ibge.gov.br/explica/codigos-dos-municipios.php';

-- ----------------------------
-- Table structure for importacao
-- ----------------------------
DROP TABLE IF EXISTS "public"."importacao";
CREATE TABLE "public"."importacao" (
  "id" int2 NOT NULL,
  "chave" varchar(50) COLLATE "pg_catalog"."default" NOT NULL,
  "url" varchar(255) COLLATE "pg_catalog"."default" NOT NULL,
  "info" varchar(255) COLLATE "pg_catalog"."default" NOT NULL,
  "parlamentar_inicio" timestamp(6),
  "parlamentar_fim" timestamp(6),
  "despesas_inicio" timestamp(6),
  "despesas_fim" timestamp(6),
  "primeira_despesa" date,
  "ultima_despesa" date,
  "id_estado" int2
)
;

-- ----------------------------
-- Table structure for partido
-- ----------------------------
DROP TABLE IF EXISTS "public"."partido";
CREATE TABLE "public"."partido" (
  "id" int2 NOT NULL,
  "legenda" int2,
  "sigla" varchar(20) COLLATE "pg_catalog"."default" NOT NULL,
  "nome" varchar(100) COLLATE "pg_catalog"."default",
  "imagem" varchar(100) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for partido_historico
-- ----------------------------
DROP TABLE IF EXISTS "public"."partido_historico";
CREATE TABLE "public"."partido_historico" (
  "id" int2 NOT NULL,
  "legenda" int2,
  "sigla" varchar(20) COLLATE "pg_catalog"."default",
  "nome" varchar(100) COLLATE "pg_catalog"."default",
  "sede" char(2) COLLATE "pg_catalog"."default",
  "fundacao" date,
  "registro_solicitacao" date,
  "registro_provisorio" date,
  "registro_definitivo" date,
  "extincao" date,
  "motivo" varchar(500) COLLATE "pg_catalog"."default"
)
;
COMMENT ON COLUMN "public"."partido_historico"."fundacao" IS 'Fundação';
COMMENT ON COLUMN "public"."partido_historico"."registro_solicitacao" IS 'Solitação de habilitação ou registro';
COMMENT ON COLUMN "public"."partido_historico"."registro_provisorio" IS 'Registro provisório';
COMMENT ON COLUMN "public"."partido_historico"."registro_definitivo" IS 'Registro definitivo';
COMMENT ON COLUMN "public"."partido_historico"."extincao" IS 'Extinção';
COMMENT ON TABLE "public"."partido_historico" IS 'https://www.partidosdobrasil.com/';

-- ----------------------------
-- Table structure for pessoa
-- ----------------------------
DROP TABLE IF EXISTS "public"."pessoa";
CREATE TABLE "public"."pessoa" (
  "id" int8 NOT NULL,
  "cpf" varchar(15) COLLATE "pg_catalog"."default",
  "nome" varchar(100) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for pessoa_new
-- ----------------------------
DROP TABLE IF EXISTS "public"."pessoa_new";
CREATE TABLE "public"."pessoa_new" (
  "id" int8 NOT NULL,
  "cpf" varchar(15) COLLATE "pg_catalog"."default" NOT NULL,
  "cpf_parcial" varchar(6) COLLATE "pg_catalog"."default",
  "nome" varchar(100) COLLATE "pg_catalog"."default",
  "data_nascimento" date,
  "id_nacionalidade" int2,
  "id_estado_nascimento" int4,
  "municipio_nascimento" varchar(100) COLLATE "pg_catalog"."default",
  "id_genero" int2,
  "id_etnia" int2,
  "id_estado_civil" int2,
  "id_grau_instrucao" int2,
  "id_ocupacao" int4
)
;

-- ----------------------------
-- Table structure for profissao
-- ----------------------------
DROP TABLE IF EXISTS "public"."profissao";
CREATE TABLE "public"."profissao" (
  "id" int4 NOT NULL,
  "descricao" varchar(100) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for trecho_viagem
-- ----------------------------
DROP TABLE IF EXISTS "public"."trecho_viagem";
CREATE TABLE "public"."trecho_viagem" (
  "id" int4 NOT NULL,
  "descricao" varchar(200) COLLATE "pg_catalog"."default" NOT NULL
)
;

-- ----------------------------
-- Indexes structure for table estado
-- ----------------------------
CREATE UNIQUE INDEX "sigla" ON "public"."estado" USING btree (
  "sigla" COLLATE "pg_catalog"."default" "pg_catalog"."bpchar_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table estado
-- ----------------------------
ALTER TABLE "public"."estado" ADD CONSTRAINT "estado_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table partido
-- ----------------------------
CREATE INDEX "legenda" ON "public"."partido" USING btree (
  "legenda" "pg_catalog"."int2_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table partido
-- ----------------------------
ALTER TABLE "public"."partido" ADD CONSTRAINT "partido_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table partido_historico
-- ----------------------------
ALTER TABLE "public"."partido_historico" ADD CONSTRAINT "partido_historico_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table pessoa
-- ----------------------------
ALTER TABLE "public"."pessoa" ADD CONSTRAINT "pessoa_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table pessoa_new
-- ----------------------------
CREATE UNIQUE INDEX "cpf" ON "public"."pessoa_new" USING btree (
  "cpf" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table pessoa_new
-- ----------------------------
ALTER TABLE "public"."pessoa_new" ADD CONSTRAINT "pessoa_new_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table profissao
-- ----------------------------
ALTER TABLE "public"."profissao" ADD CONSTRAINT "profissao_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table trecho_viagem
-- ----------------------------
ALTER TABLE "public"."trecho_viagem" ADD CONSTRAINT "trecho_viagem_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Foreign Keys structure for table importacao
-- ----------------------------
ALTER TABLE "public"."importacao" ADD CONSTRAINT "importacao_id_estado_fkey" FOREIGN KEY ("id_estado") REFERENCES "public"."estado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table pessoa_new
-- ----------------------------
ALTER TABLE "public"."pessoa_new" ADD CONSTRAINT "fk_pessoa_new_estado_civil" FOREIGN KEY ("id_estado_civil") REFERENCES "public"."estado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;
ALTER TABLE "public"."pessoa_new" ADD CONSTRAINT "fk_pessoa_new_estado_nascimento" FOREIGN KEY ("id_estado_nascimento") REFERENCES "public"."estado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;
ALTER TABLE "public"."pessoa_new" ADD CONSTRAINT "fk_pessoa_new_etnia" FOREIGN KEY ("id_etnia") REFERENCES "public"."estado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;
ALTER TABLE "public"."pessoa_new" ADD CONSTRAINT "fk_pessoa_new_genero" FOREIGN KEY ("id_genero") REFERENCES "public"."estado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;
ALTER TABLE "public"."pessoa_new" ADD CONSTRAINT "fk_pessoa_new_grau_instrucao" FOREIGN KEY ("id_grau_instrucao") REFERENCES "public"."estado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;
ALTER TABLE "public"."pessoa_new" ADD CONSTRAINT "fk_pessoa_new_nacionalidade" FOREIGN KEY ("id_nacionalidade") REFERENCES "public"."estado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;
ALTER TABLE "public"."pessoa_new" ADD CONSTRAINT "fk_pessoa_new_ocupacao" FOREIGN KEY ("id_ocupacao") REFERENCES "public"."profissao" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;
