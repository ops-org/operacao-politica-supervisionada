/*
 Navicat Premium Dump SQL

 Source Server         : Postgres WSL
 Source Server Type    : PostgreSQL
 Source Server Version : 160011 (160011)
 Source Host           : 172.31.250.64:5432
 Source Catalog        : ops
 Source Schema         : fornecedor

 Target Server Type    : PostgreSQL
 Target Server Version : 160011 (160011)
 File Encoding         : 65001

 Date: 06/01/2026 20:54:54
*/


-- ----------------------------
-- Table structure for forcecedor_cnpj_incorreto
-- ----------------------------
DROP TABLE IF EXISTS "fornecedor"."forcecedor_cnpj_incorreto";
CREATE TABLE "fornecedor"."forcecedor_cnpj_incorreto" (
  "cnpj_incorreto" char(15) COLLATE "pg_catalog"."default" NOT NULL,
  "id_fornecedor_incorreto" int8,
  "nome" varchar(255) COLLATE "pg_catalog"."default",
  "id_fornecedor_correto" int8,
  "cnpj_correto" char(15) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for fornecedor
-- ----------------------------
DROP TABLE IF EXISTS "fornecedor"."fornecedor";
CREATE TABLE "fornecedor"."fornecedor" (
  "id" int8 NOT NULL,
  "cnpj_cpf" varchar(15) COLLATE "pg_catalog"."default",
  "nome" varchar(255) COLLATE "pg_catalog"."default" NOT NULL,
  "categoria" char(2) COLLATE "pg_catalog"."default" NOT NULL,
  "doador" int2 NOT NULL,
  "controle" int2,
  "mensagem" varchar(8000) COLLATE "pg_catalog"."default",
  "valor_total_ceap_camara" numeric(16,2),
  "valor_total_ceap_senado" numeric(16,2),
  "valor_total_ceap_assembleias" numeric(16,2)
)
;
COMMENT ON COLUMN "fornecedor"."fornecedor"."categoria" IS 'Pessoa Fisica ou Pessoa Juridica';

-- ----------------------------
-- Table structure for fornecedor_atividade
-- ----------------------------
DROP TABLE IF EXISTS "fornecedor"."fornecedor_atividade";
CREATE TABLE "fornecedor"."fornecedor_atividade" (
  "id" int8 NOT NULL,
  "codigo" varchar(15) COLLATE "pg_catalog"."default" NOT NULL,
  "descricao" varchar(255) COLLATE "pg_catalog"."default" NOT NULL
)
;

-- ----------------------------
-- Table structure for fornecedor_atividade_secundaria
-- ----------------------------
DROP TABLE IF EXISTS "fornecedor"."fornecedor_atividade_secundaria";
CREATE TABLE "fornecedor"."fornecedor_atividade_secundaria" (
  "id_fornecedor" int8 NOT NULL,
  "id_fornecedor_atividade" int8 NOT NULL
)
;

-- ----------------------------
-- Table structure for fornecedor_faixa_etaria
-- ----------------------------
DROP TABLE IF EXISTS "fornecedor"."fornecedor_faixa_etaria";
CREATE TABLE "fornecedor"."fornecedor_faixa_etaria" (
  "id" int2 NOT NULL,
  "nome" varchar(50) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for fornecedor_info
-- ----------------------------
DROP TABLE IF EXISTS "fornecedor"."fornecedor_info";
CREATE TABLE "fornecedor"."fornecedor_info" (
  "id_fornecedor" int8 NOT NULL,
  "cnpj" varchar(14) COLLATE "pg_catalog"."default" NOT NULL,
  "cnpj_radical" char(14) COLLATE "pg_catalog"."default",
  "tipo" char(20) COLLATE "pg_catalog"."default",
  "nome" varchar(255) COLLATE "pg_catalog"."default",
  "data_de_abertura" date,
  "nome_fantasia" varchar(255) COLLATE "pg_catalog"."default",
  "id_fornecedor_atividade_principal" int4,
  "id_fornecedor_natureza_juridica" int4,
  "logradouro_tipo" varchar(20) COLLATE "pg_catalog"."default",
  "logradouro" varchar(100) COLLATE "pg_catalog"."default",
  "numero" varchar(100) COLLATE "pg_catalog"."default",
  "complemento" varchar(150) COLLATE "pg_catalog"."default",
  "cep" varchar(20) COLLATE "pg_catalog"."default",
  "bairro" varchar(100) COLLATE "pg_catalog"."default",
  "municipio" varchar(100) COLLATE "pg_catalog"."default",
  "estado" varchar(4) COLLATE "pg_catalog"."default",
  "endereco_eletronico" varchar(100) COLLATE "pg_catalog"."default",
  "telefone1" varchar(100) COLLATE "pg_catalog"."default",
  "telefone2" varchar(100) COLLATE "pg_catalog"."default",
  "fax" varchar(100) COLLATE "pg_catalog"."default",
  "ente_federativo_responsavel" varchar(100) COLLATE "pg_catalog"."default",
  "situacao_cadastral" varchar(100) COLLATE "pg_catalog"."default",
  "data_da_situacao_cadastral" date,
  "motivo_situacao_cadastral" varchar(100) COLLATE "pg_catalog"."default",
  "situacao_especial" varchar(100) COLLATE "pg_catalog"."default",
  "data_situacao_especial" date,
  "capital_social" numeric(20,2),
  "porte" varchar(50) COLLATE "pg_catalog"."default",
  "opcao_pelo_mei" int2,
  "data_opcao_pelo_mei" date,
  "data_exclusao_do_mei" date,
  "opcao_pelo_simples" int2,
  "data_opcao_pelo_simples" date,
  "data_exclusao_do_simples" date,
  "codigo_municipio" varchar(50) COLLATE "pg_catalog"."default",
  "codigo_municipio_ibge" varchar(50) COLLATE "pg_catalog"."default",
  "nome_cidade_no_exterior" varchar(100) COLLATE "pg_catalog"."default",
  "obtido_em" date,
  "ip_colaborador" varchar(15) COLLATE "pg_catalog"."default",
  "pais" varchar(15) COLLATE "pg_catalog"."default",
  "nome_pais" varchar(100) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for fornecedor_natureza_juridica
-- ----------------------------
DROP TABLE IF EXISTS "fornecedor"."fornecedor_natureza_juridica";
CREATE TABLE "fornecedor"."fornecedor_natureza_juridica" (
  "id" int8 NOT NULL,
  "codigo" varchar(10) COLLATE "pg_catalog"."default" NOT NULL,
  "descricao" varchar(100) COLLATE "pg_catalog"."default" NOT NULL
)
;

-- ----------------------------
-- Table structure for fornecedor_socio
-- ----------------------------
DROP TABLE IF EXISTS "fornecedor"."fornecedor_socio";
CREATE TABLE "fornecedor"."fornecedor_socio" (
  "id" int8 NOT NULL,
  "id_fornecedor" int8 NOT NULL,
  "nome" varchar(255) COLLATE "pg_catalog"."default",
  "cnpj_cpf" varchar(15) COLLATE "pg_catalog"."default",
  "pais_origem" varchar(255) COLLATE "pg_catalog"."default",
  "data_entrada_sociedade" date,
  "id_fornecedor_faixa_etaria" int2,
  "id_fornecedor_socio_qualificacao" int8,
  "nome_representante" varchar(255) COLLATE "pg_catalog"."default",
  "cpf_representante" varchar(15) COLLATE "pg_catalog"."default",
  "id_fornecedor_socio_representante_qualificacao" int8
)
;

-- ----------------------------
-- Table structure for fornecedor_socio_qualificacao
-- ----------------------------
DROP TABLE IF EXISTS "fornecedor"."fornecedor_socio_qualificacao";
CREATE TABLE "fornecedor"."fornecedor_socio_qualificacao" (
  "id" int8 NOT NULL,
  "descricao" varchar(100) COLLATE "pg_catalog"."default" NOT NULL
)
;

-- ----------------------------
-- Primary Key structure for table forcecedor_cnpj_incorreto
-- ----------------------------
ALTER TABLE "fornecedor"."forcecedor_cnpj_incorreto" ADD CONSTRAINT "forcecedor_cnpj_incorreto_pkey" PRIMARY KEY ("cnpj_incorreto");

-- ----------------------------
-- Indexes structure for table fornecedor
-- ----------------------------
CREATE INDEX "idx_cnpj_cpf" ON "fornecedor"."fornecedor" USING btree (
  "cnpj_cpf" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table fornecedor
-- ----------------------------
ALTER TABLE "fornecedor"."fornecedor" ADD CONSTRAINT "fornecedor_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table fornecedor_atividade
-- ----------------------------
ALTER TABLE "fornecedor"."fornecedor_atividade" ADD CONSTRAINT "fornecedor_atividade_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table fornecedor_atividade_secundaria
-- ----------------------------
ALTER TABLE "fornecedor"."fornecedor_atividade_secundaria" ADD CONSTRAINT "fornecedor_atividade_secundaria_pkey" PRIMARY KEY ("id_fornecedor", "id_fornecedor_atividade");

-- ----------------------------
-- Primary Key structure for table fornecedor_faixa_etaria
-- ----------------------------
ALTER TABLE "fornecedor"."fornecedor_faixa_etaria" ADD CONSTRAINT "fornecedor_faixa_etaria_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table fornecedor_info
-- ----------------------------
CREATE INDEX "id_fornecedor_atividade_principal" ON "fornecedor"."fornecedor_info" USING btree (
  "id_fornecedor_atividade_principal" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "id_fornecedor_natureza_juridica" ON "fornecedor"."fornecedor_info" USING btree (
  "id_fornecedor_natureza_juridica" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE UNIQUE INDEX "idx_cnpj" ON "fornecedor"."fornecedor_info" USING btree (
  "cnpj" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE INDEX "nome_fantasia" ON "fornecedor"."fornecedor_info" USING btree (
  "nome_fantasia" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table fornecedor_info
-- ----------------------------
ALTER TABLE "fornecedor"."fornecedor_info" ADD CONSTRAINT "fornecedor_info_pkey" PRIMARY KEY ("id_fornecedor");

-- ----------------------------
-- Primary Key structure for table fornecedor_natureza_juridica
-- ----------------------------
ALTER TABLE "fornecedor"."fornecedor_natureza_juridica" ADD CONSTRAINT "fornecedor_natureza_juridica_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table fornecedor_socio
-- ----------------------------
CREATE INDEX "id_fornecedor" ON "fornecedor"."fornecedor_socio" USING btree (
  "id_fornecedor" "pg_catalog"."int8_ops" ASC NULLS LAST
);
CREATE INDEX "id_fornecedor_socio_qualificacao" ON "fornecedor"."fornecedor_socio" USING btree (
  "id_fornecedor_socio_qualificacao" "pg_catalog"."int8_ops" ASC NULLS LAST
);
CREATE INDEX "id_fornecedor_socio_representante_qualificacao" ON "fornecedor"."fornecedor_socio" USING btree (
  "id_fornecedor_socio_representante_qualificacao" "pg_catalog"."int8_ops" ASC NULLS LAST
);
CREATE INDEX "nome_representante" ON "fornecedor"."fornecedor_socio" USING btree (
  "nome_representante" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table fornecedor_socio
-- ----------------------------
ALTER TABLE "fornecedor"."fornecedor_socio" ADD CONSTRAINT "fornecedor_socio_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table fornecedor_socio_qualificacao
-- ----------------------------
ALTER TABLE "fornecedor"."fornecedor_socio_qualificacao" ADD CONSTRAINT "fornecedor_socio_qualificacao_pkey" PRIMARY KEY ("id");
