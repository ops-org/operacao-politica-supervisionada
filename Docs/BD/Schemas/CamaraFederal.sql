/*
 Navicat Premium Dump SQL

 Source Server         : Postgres WSL
 Source Server Type    : PostgreSQL
 Source Server Version : 160011 (160011)
 Source Host           : 172.31.250.64:5432
 Source Catalog        : ops
 Source Schema         : camara

 Target Server Type    : PostgreSQL
 Target Server Version : 160011 (160011)
 File Encoding         : 65001

 Date: 06/01/2026 20:54:23
*/


-- ----------------------------
-- Table structure for cf_deputado
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_deputado";
CREATE TABLE "camara"."cf_deputado" (
  "id" int4 NOT NULL,
  "id_deputado" int4,
  "id_partido" int2 NOT NULL,
  "id_estado" int2,
  "id_cf_gabinete" int4,
  "cpf" varchar(15) COLLATE "pg_catalog"."default",
  "nome_parlamentar" varchar(100) COLLATE "pg_catalog"."default",
  "nome_civil" varchar(100) COLLATE "pg_catalog"."default",
  "nome_importacao_presenca" varchar(100) COLLATE "pg_catalog"."default",
  "sexo" varchar(2) COLLATE "pg_catalog"."default",
  "email" varchar(100) COLLATE "pg_catalog"."default",
  "nascimento" date,
  "falecimento" date,
  "id_estado_nascimento" int2,
  "municipio" varchar(500) COLLATE "pg_catalog"."default",
  "website" varchar(255) COLLATE "pg_catalog"."default",
  "profissao" varchar(255) COLLATE "pg_catalog"."default",
  "escolaridade" varchar(100) COLLATE "pg_catalog"."default",
  "condicao" varchar(50) COLLATE "pg_catalog"."default",
  "situacao" varchar(20) COLLATE "pg_catalog"."default",
  "passaporte_diplomatico" int2,
  "processado" int2 NOT NULL,
  "valor_total_ceap" numeric(16,2) NOT NULL,
  "secretarios_ativos" int2,
  "valor_mensal_secretarios" numeric(16,2) NOT NULL,
  "valor_total_remuneracao" numeric(16,2) NOT NULL,
  "valor_total_salario" numeric(16,2) NOT NULL,
  "valor_total_auxilio_moradia" numeric(16,2) NOT NULL
)
;
COMMENT ON COLUMN "camara"."cf_deputado"."id" IS 'ideDeputado';
COMMENT ON COLUMN "camara"."cf_deputado"."id_deputado" IS 'nuDeputadoId';
COMMENT ON COLUMN "camara"."cf_deputado"."id_cf_gabinete" IS 'Usado para importação dos secretarios parlamentares';
COMMENT ON COLUMN "camara"."cf_deputado"."valor_total_ceap" IS 'Valor acumulado gasto com a cota parlamentar em todas as legislaturas';
COMMENT ON COLUMN "camara"."cf_deputado"."secretarios_ativos" IS 'Quantidade de secretarios';
COMMENT ON COLUMN "camara"."cf_deputado"."valor_total_remuneracao" IS 'Renomear para valor_total_gabinete';
COMMENT ON COLUMN "camara"."cf_deputado"."valor_total_salario" IS 'Renomear para valor_total_remuneracao';

-- ----------------------------
-- Table structure for cf_deputado_auxilio_moradia
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_deputado_auxilio_moradia";
CREATE TABLE "camara"."cf_deputado_auxilio_moradia" (
  "id_cf_deputado" int4 NOT NULL,
  "ano" int4,
  "mes" int4,
  "valor" numeric(10,2)
)
;

-- ----------------------------
-- Table structure for cf_deputado_campeao_gasto
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_deputado_campeao_gasto";
CREATE TABLE "camara"."cf_deputado_campeao_gasto" (
  "id_cf_deputado" int4 NOT NULL,
  "nome_parlamentar" varchar(100) COLLATE "pg_catalog"."default",
  "valor_total" numeric(10,2),
  "sigla_partido" varchar(20) COLLATE "pg_catalog"."default",
  "sigla_estado" varchar(2) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for cf_deputado_cota_parlamentar
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_deputado_cota_parlamentar";
CREATE TABLE "camara"."cf_deputado_cota_parlamentar" (
  "id_cf_deputado" int4 NOT NULL,
  "ano" int2 NOT NULL,
  "mes" int2 NOT NULL,
  "valor" numeric(10,2) NOT NULL,
  "percentual" numeric(10,2)
)
;

-- ----------------------------
-- Table structure for cf_deputado_gabinete
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_deputado_gabinete";
CREATE TABLE "camara"."cf_deputado_gabinete" (
  "id" int4 NOT NULL,
  "nome" varchar(500) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for cf_deputado_imovel_funcional
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_deputado_imovel_funcional";
CREATE TABLE "camara"."cf_deputado_imovel_funcional" (
  "id_cf_deputado" int4 NOT NULL,
  "uso_de" date NOT NULL,
  "uso_ate" date,
  "total_dias" int2
)
;

-- ----------------------------
-- Table structure for cf_deputado_missao_oficial
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_deputado_missao_oficial";
CREATE TABLE "camara"."cf_deputado_missao_oficial" (
  "id_cf_deputado" int4 NOT NULL,
  "periodo" varchar(50) COLLATE "pg_catalog"."default" NOT NULL,
  "assunto" varchar(4000) COLLATE "pg_catalog"."default" NOT NULL,
  "destino" varchar(255) COLLATE "pg_catalog"."default",
  "passagens" numeric(10,2),
  "diarias" numeric(10,2),
  "relatorio" varchar(255) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for cf_deputado_remuneracao
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_deputado_remuneracao";
CREATE TABLE "camara"."cf_deputado_remuneracao" (
  "id_cf_deputado" int8 NOT NULL,
  "ano" int2 NOT NULL,
  "mes" int2 NOT NULL,
  "valor" numeric(10,2) NOT NULL
)
;

-- ----------------------------
-- Table structure for cf_deputado_verba_gabinete
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_deputado_verba_gabinete";
CREATE TABLE "camara"."cf_deputado_verba_gabinete" (
  "id_cf_deputado" int4 NOT NULL,
  "ano" int2 NOT NULL,
  "mes" int2 NOT NULL,
  "valor" numeric(10,2) NOT NULL,
  "percentual" numeric(10,2)
)
;

-- ----------------------------
-- Table structure for cf_despesa
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_despesa";
CREATE TABLE "camara"."cf_despesa" (
  "id" numeric(20,0) NOT NULL,
  "ano" int4 NOT NULL,
  "mes" int2 NOT NULL,
  "id_cf_legislatura" int2 NOT NULL,
  "id_cf_deputado" int4 NOT NULL,
  "id_cf_mandato" int4,
  "id_cf_despesa_tipo" int4 NOT NULL,
  "id_cf_especificacao" int2,
  "id_fornecedor" int4 NOT NULL,
  "id_documento" numeric(20,0),
  "id_passageiro" int4,
  "id_trecho_viagem" int4,
  "data_emissao" date,
  "valor_documento" numeric(10,2),
  "valor_glosa" numeric(10,2) NOT NULL,
  "valor_liquido" numeric(10,2) NOT NULL,
  "valor_restituicao" numeric(10,2),
  "tipo_documento" int2 NOT NULL,
  "tipo_link" int2 NOT NULL,
  "numero_documento" varchar(100) COLLATE "pg_catalog"."default",
  "hash" bytea
)
;
COMMENT ON COLUMN "camara"."cf_despesa"."tipo_documento" IS '0: Nota Fiscal; 1: Recibo; 2: Despesa no Exterior';
COMMENT ON COLUMN "camara"."cf_despesa"."tipo_link" IS '0: Sem Arquivo; 1: Recibo; 2: Nota Fiscal Eletronica';

-- ----------------------------
-- Table structure for cf_despesa_resumo_mensal
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_despesa_resumo_mensal";
CREATE TABLE "camara"."cf_despesa_resumo_mensal" (
  "ano" int8 NOT NULL,
  "mes" int8 NOT NULL,
  "valor" numeric(10,2)
)
;

-- ----------------------------
-- Table structure for cf_despesa_tipo
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_despesa_tipo";
CREATE TABLE "camara"."cf_despesa_tipo" (
  "id" int4 NOT NULL,
  "descricao" varchar(100) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for cf_especificacao_tipo
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_especificacao_tipo";
CREATE TABLE "camara"."cf_especificacao_tipo" (
  "id_cf_despesa_tipo" int4 NOT NULL,
  "id_cf_especificacao" int2 NOT NULL,
  "descricao" varchar(100) COLLATE "pg_catalog"."default" NOT NULL
)
;

-- ----------------------------
-- Table structure for cf_funcionario
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_funcionario";
CREATE TABLE "camara"."cf_funcionario" (
  "id" int4 NOT NULL,
  "chave" varchar(45) COLLATE "pg_catalog"."default" NOT NULL,
  "nome" varchar(100) COLLATE "pg_catalog"."default" NOT NULL,
  "processado" int2 NOT NULL,
  "controle" char(10) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for cf_funcionario_area_atuacao
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_funcionario_area_atuacao";
CREATE TABLE "camara"."cf_funcionario_area_atuacao" (
  "id" int2 NOT NULL,
  "nome" varchar(50) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for cf_funcionario_cargo
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_funcionario_cargo";
CREATE TABLE "camara"."cf_funcionario_cargo" (
  "id" int2 NOT NULL,
  "nome" varchar(100) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for cf_funcionario_contratacao
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_funcionario_contratacao";
CREATE TABLE "camara"."cf_funcionario_contratacao" (
  "id" int4 NOT NULL,
  "id_cf_deputado" int4,
  "id_cf_funcionario" int4 NOT NULL,
  "id_cf_funcionario_grupo_funcional" int2,
  "id_cf_funcionario_cargo" int2,
  "id_cf_funcionario_nivel" int2,
  "id_cf_funcionario_funcao_comissionada" int4,
  "id_cf_funcionario_area_atuacao" int2,
  "id_cf_funcionario_local_trabalho" int2,
  "id_cf_funcionario_situacao" int2,
  "periodo_de" date,
  "periodo_ate" date
)
;

-- ----------------------------
-- Table structure for cf_funcionario_funcao_comissionada
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_funcionario_funcao_comissionada";
CREATE TABLE "camara"."cf_funcionario_funcao_comissionada" (
  "id" int4 NOT NULL,
  "nome" varchar(255) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for cf_funcionario_grupo_funcional
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_funcionario_grupo_funcional";
CREATE TABLE "camara"."cf_funcionario_grupo_funcional" (
  "id" int2 NOT NULL,
  "nome" varchar(50) COLLATE "pg_catalog"."default" NOT NULL
)
;

-- ----------------------------
-- Table structure for cf_funcionario_local_trabalho
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_funcionario_local_trabalho";
CREATE TABLE "camara"."cf_funcionario_local_trabalho" (
  "id" int2 NOT NULL,
  "nome" varchar(50) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for cf_funcionario_nivel
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_funcionario_nivel";
CREATE TABLE "camara"."cf_funcionario_nivel" (
  "id" int2 NOT NULL,
  "nome" varchar(50) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for cf_funcionario_remuneracao
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_funcionario_remuneracao";
CREATE TABLE "camara"."cf_funcionario_remuneracao" (
  "id" int4 NOT NULL,
  "id_cf_funcionario" int4 NOT NULL,
  "id_cf_funcionario_contratacao" int4,
  "id_cf_deputado" int4,
  "referencia" date NOT NULL,
  "tipo" int2,
  "remuneracao_fixa" numeric(10,2),
  "vantagens_natureza_pessoal" numeric(10,2),
  "funcao_ou_cargo_em_comissao" numeric(10,2),
  "gratificacao_natalina" numeric(10,2),
  "ferias" numeric(10,2),
  "outras_remuneracoes" numeric(10,2),
  "abono_permanencia" numeric(10,2),
  "valor_bruto" numeric(10,2),
  "redutor_constitucional" numeric(10,2),
  "contribuicao_previdenciaria" numeric(10,2),
  "imposto_renda" numeric(10,2),
  "valor_liquido" numeric(10,2),
  "valor_diarias" numeric(10,2),
  "valor_auxilios" numeric(10,2),
  "valor_vantagens" numeric(10,2),
  "valor_outros" numeric(10,2),
  "valor_total" numeric(10,2),
  "nivel" varchar(5) COLLATE "pg_catalog"."default",
  "contratacao" date
)
;
COMMENT ON COLUMN "camara"."cf_funcionario_remuneracao"."valor_total" IS 'valor_bruto + valor_outros';

-- ----------------------------
-- Table structure for cf_funcionario_situacao
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_funcionario_situacao";
CREATE TABLE "camara"."cf_funcionario_situacao" (
  "id" int2 NOT NULL,
  "nome" varchar(50) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for cf_funcionario_tipo_folha
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_funcionario_tipo_folha";
CREATE TABLE "camara"."cf_funcionario_tipo_folha" (
  "id" int2 NOT NULL,
  "nome" varchar(50) COLLATE "pg_catalog"."default" NOT NULL
)
;

-- ----------------------------
-- Table structure for cf_gabinete
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_gabinete";
CREATE TABLE "camara"."cf_gabinete" (
  "id" int4 NOT NULL,
  "nome" varchar(50) COLLATE "pg_catalog"."default",
  "predio" varchar(50) COLLATE "pg_catalog"."default",
  "andar" int2,
  "sala" varchar(50) COLLATE "pg_catalog"."default",
  "telefone" varchar(20) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for cf_legislatura
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_legislatura";
CREATE TABLE "camara"."cf_legislatura" (
  "id" int2 NOT NULL,
  "ano" int4,
  "inicio" int4,
  "final" int4
)
;

-- ----------------------------
-- Table structure for cf_mandato
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_mandato";
CREATE TABLE "camara"."cf_mandato" (
  "id" int4 NOT NULL,
  "id_cf_deputado" int4 NOT NULL,
  "id_legislatura" int2,
  "id_carteira_parlamantar" int4,
  "id_estado" int2,
  "id_partido" int2,
  "condicao" varchar(10) COLLATE "pg_catalog"."default",
  "valor_total_ceap" numeric(26,2)
)
;

-- ----------------------------
-- Table structure for cf_secretario
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_secretario";
CREATE TABLE "camara"."cf_secretario" (
  "id" int4 NOT NULL,
  "id_cf_deputado" int4 NOT NULL,
  "nome" varchar(100) COLLATE "pg_catalog"."default",
  "periodo" varchar(100) COLLATE "pg_catalog"."default",
  "cargo" varchar(45) COLLATE "pg_catalog"."default",
  "valor_bruto" numeric(10,2),
  "valor_liquido" numeric(10,2),
  "valor_outros" numeric(10,2),
  "link" varchar(255) COLLATE "pg_catalog"."default",
  "referencia" varchar(255) COLLATE "pg_catalog"."default",
  "em_exercicio" int2
)
;

-- ----------------------------
-- Table structure for cf_secretario_historico
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_secretario_historico";
CREATE TABLE "camara"."cf_secretario_historico" (
  "id_cf_deputado" int4 NOT NULL,
  "nome" varchar(100) COLLATE "pg_catalog"."default",
  "cargo" varchar(45) COLLATE "pg_catalog"."default",
  "periodo" varchar(255) COLLATE "pg_catalog"."default",
  "valor_bruto" numeric(10,2),
  "valor_liquido" numeric(10,2),
  "valor_outros" numeric(10,2),
  "link" varchar(255) COLLATE "pg_catalog"."default",
  "referencia" varchar(255) COLLATE "pg_catalog"."default",
  "ano_mes" int8
)
;

-- ----------------------------
-- Table structure for cf_senador_verba_gabinete
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_senador_verba_gabinete";
CREATE TABLE "camara"."cf_senador_verba_gabinete" (
  "id_sf_senador" int4 NOT NULL,
  "ano" int2 NOT NULL,
  "mes" int2 NOT NULL,
  "valor" numeric(10,2) NOT NULL,
  "percentual" numeric(10,2)
)
;

-- ----------------------------
-- Table structure for cf_sessao
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_sessao";
CREATE TABLE "camara"."cf_sessao" (
  "id" int4 NOT NULL,
  "id_legislatura" int2 NOT NULL,
  "data" date NOT NULL,
  "inicio" timestamp(6) NOT NULL,
  "tipo" int2 NOT NULL,
  "numero" varchar(45) COLLATE "pg_catalog"."default",
  "presencas" int4 NOT NULL,
  "ausencias" int4 NOT NULL,
  "ausencias_justificadas" int4 NOT NULL,
  "checksum" varchar(100) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for cf_sessao_presenca
-- ----------------------------
DROP TABLE IF EXISTS "camara"."cf_sessao_presenca";
CREATE TABLE "camara"."cf_sessao_presenca" (
  "id" int8 NOT NULL,
  "id_cf_sessao" int4 NOT NULL,
  "id_cf_deputado" int4 NOT NULL,
  "presente" char(1) COLLATE "pg_catalog"."default" NOT NULL,
  "justificativa" varchar(100) COLLATE "pg_catalog"."default",
  "presenca_externa" char(1) COLLATE "pg_catalog"."default" NOT NULL
)
;

-- ----------------------------
-- Indexes structure for table cf_deputado
-- ----------------------------
CREATE INDEX "id_cf_gabinete" ON "camara"."cf_deputado" USING btree (
  "id_cf_gabinete" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "id_deputado" ON "camara"."cf_deputado" USING btree (
  "id_deputado" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "id_estado" ON "camara"."cf_deputado" USING btree (
  "id_estado" "pg_catalog"."int2_ops" ASC NULLS LAST
);
CREATE INDEX "id_estado_nascimento" ON "camara"."cf_deputado" USING btree (
  "id_estado_nascimento" "pg_catalog"."int2_ops" ASC NULLS LAST
);
CREATE INDEX "id_partido" ON "camara"."cf_deputado" USING btree (
  "id_partido" "pg_catalog"."int2_ops" ASC NULLS LAST
);
CREATE INDEX "nome_parlamentar" ON "camara"."cf_deputado" USING btree (
  "nome_parlamentar" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE INDEX "quantidade_secretarios" ON "camara"."cf_deputado" USING btree (
  "secretarios_ativos" "pg_catalog"."int2_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table cf_deputado
-- ----------------------------
ALTER TABLE "camara"."cf_deputado" ADD CONSTRAINT "cf_deputado_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table cf_deputado_auxilio_moradia
-- ----------------------------
CREATE UNIQUE INDEX "id_cf_deputado" ON "camara"."cf_deputado_auxilio_moradia" USING btree (
  "id_cf_deputado" "pg_catalog"."int4_ops" ASC NULLS LAST,
  "ano" "pg_catalog"."int4_ops" ASC NULLS LAST,
  "mes" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table cf_deputado_campeao_gasto
-- ----------------------------
ALTER TABLE "camara"."cf_deputado_campeao_gasto" ADD CONSTRAINT "cf_deputado_campeao_gasto_pkey" PRIMARY KEY ("id_cf_deputado");

-- ----------------------------
-- Indexes structure for table cf_deputado_cota_parlamentar
-- ----------------------------
CREATE INDEX "id_cl_deputado" ON "camara"."cf_deputado_cota_parlamentar" USING btree (
  "id_cf_deputado" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE UNIQUE INDEX "id_cl_deputado_ano_mes" ON "camara"."cf_deputado_cota_parlamentar" USING btree (
  "id_cf_deputado" "pg_catalog"."int4_ops" ASC NULLS LAST,
  "ano" "pg_catalog"."int2_ops" ASC NULLS LAST,
  "mes" "pg_catalog"."int2_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table cf_deputado_gabinete
-- ----------------------------
ALTER TABLE "camara"."cf_deputado_gabinete" ADD CONSTRAINT "cf_deputado_gabinete_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table cf_despesa
-- ----------------------------
CREATE INDEX "ano" ON "camara"."cf_despesa" USING btree (
  "ano" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "id_cf_despesa_tipo" ON "camara"."cf_despesa" USING btree (
  "id_cf_despesa_tipo" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "id_cf_especificacao" ON "camara"."cf_despesa" USING btree (
  "id_cf_especificacao" "pg_catalog"."int2_ops" ASC NULLS LAST
);
CREATE INDEX "id_cf_mandato" ON "camara"."cf_despesa" USING btree (
  "id_cf_mandato" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "id_legislatura" ON "camara"."cf_despesa" USING btree (
  "id_cf_legislatura" "pg_catalog"."int2_ops" ASC NULLS LAST
);
CREATE INDEX "idx_id_cf_deputado" ON "camara"."cf_despesa" USING btree (
  "id_cf_deputado" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "idx_id_fornecedor" ON "camara"."cf_despesa" USING btree (
  "id_fornecedor" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "mes" ON "camara"."cf_despesa" USING btree (
  "mes" "pg_catalog"."int2_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table cf_despesa
-- ----------------------------
ALTER TABLE "camara"."cf_despesa" ADD CONSTRAINT "cf_despesa_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table cf_despesa_resumo_mensal
-- ----------------------------
ALTER TABLE "camara"."cf_despesa_resumo_mensal" ADD CONSTRAINT "cf_despesa_resumo_mensal_pkey" PRIMARY KEY ("ano", "mes");

-- ----------------------------
-- Indexes structure for table cf_despesa_tipo
-- ----------------------------
CREATE INDEX "descricao" ON "camara"."cf_despesa_tipo" USING btree (
  "descricao" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table cf_despesa_tipo
-- ----------------------------
ALTER TABLE "camara"."cf_despesa_tipo" ADD CONSTRAINT "cf_despesa_tipo_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table cf_especificacao_tipo
-- ----------------------------
ALTER TABLE "camara"."cf_especificacao_tipo" ADD CONSTRAINT "cf_especificacao_tipo_pkey" PRIMARY KEY ("id_cf_despesa_tipo", "id_cf_especificacao");

-- ----------------------------
-- Indexes structure for table cf_funcionario
-- ----------------------------
CREATE UNIQUE INDEX "chave" ON "camara"."cf_funcionario" USING btree (
  "chave" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE INDEX "idx_nome" ON "camara"."cf_funcionario" USING btree (
  "nome" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table cf_funcionario
-- ----------------------------
ALTER TABLE "camara"."cf_funcionario" ADD CONSTRAINT "cf_funcionario_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table cf_funcionario_area_atuacao
-- ----------------------------
CREATE UNIQUE INDEX "nome" ON "camara"."cf_funcionario_area_atuacao" USING btree (
  "nome" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table cf_funcionario_area_atuacao
-- ----------------------------
ALTER TABLE "camara"."cf_funcionario_area_atuacao" ADD CONSTRAINT "cf_funcionario_area_atuacao_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table cf_funcionario_cargo
-- ----------------------------
ALTER TABLE "camara"."cf_funcionario_cargo" ADD CONSTRAINT "cf_funcionario_cargo_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table cf_funcionario_contratacao
-- ----------------------------
CREATE INDEX "FK_cf_funcionario_contratacao_cf_deputado" ON "camara"."cf_funcionario_contratacao" USING btree (
  "id_cf_deputado" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "FK_cf_funcionario_contratacao_cf_funcionario_area_atuacao" ON "camara"."cf_funcionario_contratacao" USING btree (
  "id_cf_funcionario_area_atuacao" "pg_catalog"."int2_ops" ASC NULLS LAST
);
CREATE INDEX "FK_cf_funcionario_contratacao_cf_funcionario_cargo" ON "camara"."cf_funcionario_contratacao" USING btree (
  "id_cf_funcionario_cargo" "pg_catalog"."int2_ops" ASC NULLS LAST
);
CREATE INDEX "FK_cf_funcionario_contratacao_cf_funcionario_funcao_comissionad" ON "camara"."cf_funcionario_contratacao" USING btree (
  "id_cf_funcionario_funcao_comissionada" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "FK_cf_funcionario_contratacao_cf_funcionario_grupo_funcional" ON "camara"."cf_funcionario_contratacao" USING btree (
  "id_cf_funcionario_grupo_funcional" "pg_catalog"."int2_ops" ASC NULLS LAST
);
CREATE INDEX "FK_cf_funcionario_contratacao_cf_funcionario_local_trabalho" ON "camara"."cf_funcionario_contratacao" USING btree (
  "id_cf_funcionario_local_trabalho" "pg_catalog"."int2_ops" ASC NULLS LAST
);
CREATE INDEX "FK_cf_funcionario_contratacao_cf_funcionario_nivel" ON "camara"."cf_funcionario_contratacao" USING btree (
  "id_cf_funcionario_nivel" "pg_catalog"."int2_ops" ASC NULLS LAST
);
CREATE INDEX "FK_cf_funcionario_contratacao_cf_funcionario_situacao" ON "camara"."cf_funcionario_contratacao" USING btree (
  "id_cf_funcionario_situacao" "pg_catalog"."int2_ops" ASC NULLS LAST
);
CREATE UNIQUE INDEX "cf_secretario_contratacao_unique" ON "camara"."cf_funcionario_contratacao" USING btree (
  "id_cf_funcionario" "pg_catalog"."int4_ops" ASC NULLS LAST,
  "id_cf_deputado" "pg_catalog"."int4_ops" ASC NULLS LAST,
  "periodo_de" "pg_catalog"."date_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table cf_funcionario_contratacao
-- ----------------------------
ALTER TABLE "camara"."cf_funcionario_contratacao" ADD CONSTRAINT "cf_funcionario_contratacao_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table cf_funcionario_funcao_comissionada
-- ----------------------------
ALTER TABLE "camara"."cf_funcionario_funcao_comissionada" ADD CONSTRAINT "cf_funcionario_funcao_comissionada_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table cf_funcionario_grupo_funcional
-- ----------------------------
ALTER TABLE "camara"."cf_funcionario_grupo_funcional" ADD CONSTRAINT "cf_funcionario_grupo_funcional_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table cf_funcionario_local_trabalho
-- ----------------------------
ALTER TABLE "camara"."cf_funcionario_local_trabalho" ADD CONSTRAINT "cf_funcionario_local_trabalho_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table cf_funcionario_nivel
-- ----------------------------
ALTER TABLE "camara"."cf_funcionario_nivel" ADD CONSTRAINT "cf_funcionario_nivel_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table cf_funcionario_remuneracao
-- ----------------------------
CREATE INDEX "FK_cf_funcionario_remuneracao_cf_deputado" ON "camara"."cf_funcionario_remuneracao" USING btree (
  "id_cf_deputado" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "FK_cf_funcionario_remuneracao_cf_funcionario_contratacao" ON "camara"."cf_funcionario_remuneracao" USING btree (
  "id_cf_funcionario_contratacao" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE UNIQUE INDEX "id_cf_secretario" ON "camara"."cf_funcionario_remuneracao" USING btree (
  "id_cf_funcionario" "pg_catalog"."int4_ops" ASC NULLS LAST,
  "referencia" "pg_catalog"."date_ops" ASC NULLS LAST,
  "tipo" "pg_catalog"."int2_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table cf_funcionario_remuneracao
-- ----------------------------
ALTER TABLE "camara"."cf_funcionario_remuneracao" ADD CONSTRAINT "cf_funcionario_remuneracao_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table cf_funcionario_situacao
-- ----------------------------
ALTER TABLE "camara"."cf_funcionario_situacao" ADD CONSTRAINT "cf_funcionario_situacao_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table cf_funcionario_tipo_folha
-- ----------------------------
ALTER TABLE "camara"."cf_funcionario_tipo_folha" ADD CONSTRAINT "cf_funcionario_tipo_folha_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table cf_gabinete
-- ----------------------------
ALTER TABLE "camara"."cf_gabinete" ADD CONSTRAINT "cf_gabinete_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table cf_legislatura
-- ----------------------------
ALTER TABLE "camara"."cf_legislatura" ADD CONSTRAINT "cf_legislatura_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table cf_mandato
-- ----------------------------
CREATE INDEX "id_carteira_parlamantar" ON "camara"."cf_mandato" USING btree (
  "id_carteira_parlamantar" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE UNIQUE INDEX "id_cf_deputado_id_legislatura" ON "camara"."cf_mandato" USING btree (
  "id_cf_deputado" "pg_catalog"."int4_ops" ASC NULLS LAST,
  "id_legislatura" "pg_catalog"."int2_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table cf_mandato
-- ----------------------------
ALTER TABLE "camara"."cf_mandato" ADD CONSTRAINT "cf_mandato_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table cf_secretario
-- ----------------------------
CREATE INDEX "idx_link" ON "camara"."cf_secretario" USING btree (
  "link" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table cf_secretario
-- ----------------------------
ALTER TABLE "camara"."cf_secretario" ADD CONSTRAINT "cf_secretario_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table cf_senador_verba_gabinete
-- ----------------------------
CREATE INDEX "id_sf_senador" ON "camara"."cf_senador_verba_gabinete" USING btree (
  "id_sf_senador" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE UNIQUE INDEX "id_sf_senador_ano_mes" ON "camara"."cf_senador_verba_gabinete" USING btree (
  "id_sf_senador" "pg_catalog"."int4_ops" ASC NULLS LAST,
  "ano" "pg_catalog"."int2_ops" ASC NULLS LAST,
  "mes" "pg_catalog"."int2_ops" ASC NULLS LAST
);

-- ----------------------------
-- Indexes structure for table cf_sessao
-- ----------------------------
CREATE INDEX "data" ON "camara"."cf_sessao" USING btree (
  "data" "pg_catalog"."date_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table cf_sessao
-- ----------------------------
ALTER TABLE "camara"."cf_sessao" ADD CONSTRAINT "cf_sessao_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table cf_sessao_presenca
-- ----------------------------
CREATE INDEX "id_cf_sessao" ON "camara"."cf_sessao_presenca" USING btree (
  "id_cf_sessao" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table cf_sessao_presenca
-- ----------------------------
ALTER TABLE "camara"."cf_sessao_presenca" ADD CONSTRAINT "cf_sessao_presenca_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Foreign Keys structure for table cf_deputado_campeao_gasto
-- ----------------------------
ALTER TABLE "camara"."cf_deputado_campeao_gasto" ADD CONSTRAINT "cf_deputado_campeao_gasto_id_cf_deputado_fkey" FOREIGN KEY ("id_cf_deputado") REFERENCES "public"."cf_deputado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table cf_mandato
-- ----------------------------
ALTER TABLE "camara"."cf_mandato" ADD CONSTRAINT "cf_mandato_id_estado_fkey" FOREIGN KEY ("id_estado") REFERENCES "public"."estado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;
ALTER TABLE "camara"."cf_mandato" ADD CONSTRAINT "cf_mandato_id_legislatura_fkey" FOREIGN KEY ("id_legislatura") REFERENCES "public"."cf_legislatura" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;
ALTER TABLE "camara"."cf_mandato" ADD CONSTRAINT "cf_mandato_id_partido_fkey" FOREIGN KEY ("id_partido") REFERENCES "public"."partido" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table cf_sessao_presenca
-- ----------------------------
ALTER TABLE "camara"."cf_sessao_presenca" ADD CONSTRAINT "cf_sessao_presenca_id_cf_sessao_fkey" FOREIGN KEY ("id_cf_sessao") REFERENCES "public"."cf_sessao" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;
