/*
 Navicat Premium Dump SQL

 Source Server         : Postgres WSL
 Source Server Type    : PostgreSQL
 Source Server Version : 160011 (160011)
 Source Host           : 172.31.250.64:5432
 Source Catalog        : ops
 Source Schema         : senado

 Target Server Type    : PostgreSQL
 Target Server Version : 160011 (160011)
 File Encoding         : 65001

 Date: 06/01/2026 20:55:19
*/


-- ----------------------------
-- Table structure for sf_cargo
-- ----------------------------
DROP TABLE IF EXISTS "senado"."sf_cargo";
CREATE TABLE "senado"."sf_cargo" (
  "id" int2 NOT NULL,
  "descricao" varchar(100) COLLATE "pg_catalog"."default" NOT NULL
)
;

-- ----------------------------
-- Table structure for sf_categoria
-- ----------------------------
DROP TABLE IF EXISTS "senado"."sf_categoria";
CREATE TABLE "senado"."sf_categoria" (
  "id" int2 NOT NULL,
  "descricao" varchar(100) COLLATE "pg_catalog"."default" NOT NULL
)
;

-- ----------------------------
-- Table structure for sf_despesa
-- ----------------------------
DROP TABLE IF EXISTS "senado"."sf_despesa";
CREATE TABLE "senado"."sf_despesa" (
  "id" numeric(20,0) NOT NULL,
  "id_sf_senador" int8 NOT NULL,
  "id_sf_despesa_tipo" int2,
  "id_fornecedor" int8,
  "ano_mes" int8,
  "ano" int4,
  "mes" int4,
  "documento" varchar(50) COLLATE "pg_catalog"."default",
  "data_emissao" date,
  "detalhamento" text COLLATE "pg_catalog"."default",
  "valor" numeric(10,2),
  "hash" bytea
)
;

-- ----------------------------
-- Table structure for sf_despesa_resumo_mensal
-- ----------------------------
DROP TABLE IF EXISTS "senado"."sf_despesa_resumo_mensal";
CREATE TABLE "senado"."sf_despesa_resumo_mensal" (
  "ano" int8 NOT NULL,
  "mes" int8 NOT NULL,
  "valor" numeric(10,2)
)
;

-- ----------------------------
-- Table structure for sf_despesa_tipo
-- ----------------------------
DROP TABLE IF EXISTS "senado"."sf_despesa_tipo";
CREATE TABLE "senado"."sf_despesa_tipo" (
  "id" int2 NOT NULL,
  "descricao" varchar(255) COLLATE "pg_catalog"."default" NOT NULL
)
;

-- ----------------------------
-- Table structure for sf_funcao
-- ----------------------------
DROP TABLE IF EXISTS "senado"."sf_funcao";
CREATE TABLE "senado"."sf_funcao" (
  "id" int2 NOT NULL,
  "descricao" char(5) COLLATE "pg_catalog"."default" NOT NULL
)
;

-- ----------------------------
-- Table structure for sf_legislatura
-- ----------------------------
DROP TABLE IF EXISTS "senado"."sf_legislatura";
CREATE TABLE "senado"."sf_legislatura" (
  "id" int2 NOT NULL,
  "inicio" date NOT NULL,
  "final" date NOT NULL
)
;

-- ----------------------------
-- Table structure for sf_lotacao
-- ----------------------------
DROP TABLE IF EXISTS "senado"."sf_lotacao";
CREATE TABLE "senado"."sf_lotacao" (
  "id" int4 NOT NULL,
  "id_senador" int8,
  "descricao" varchar(100) COLLATE "pg_catalog"."default" NOT NULL
)
;

-- ----------------------------
-- Table structure for sf_mandato
-- ----------------------------
DROP TABLE IF EXISTS "senado"."sf_mandato";
CREATE TABLE "senado"."sf_mandato" (
  "id" int4 NOT NULL,
  "id_sf_senador" int4 NOT NULL,
  "id_estado" int2,
  "id_partido" int2,
  "participacao" varchar(50) COLLATE "pg_catalog"."default" NOT NULL,
  "exerceu" int2
)
;

-- ----------------------------
-- Table structure for sf_mandato_exercicio
-- ----------------------------
DROP TABLE IF EXISTS "senado"."sf_mandato_exercicio";
CREATE TABLE "senado"."sf_mandato_exercicio" (
  "id" int4 NOT NULL,
  "id_sf_senador" int4 NOT NULL,
  "id_sf_mandato" int4 NOT NULL,
  "id_sf_motivo_afastamento" char(5) COLLATE "pg_catalog"."default",
  "inicio" date,
  "final" date
)
;

-- ----------------------------
-- Table structure for sf_mandato_legislatura
-- ----------------------------
DROP TABLE IF EXISTS "senado"."sf_mandato_legislatura";
CREATE TABLE "senado"."sf_mandato_legislatura" (
  "id_sf_mandato" int4 NOT NULL,
  "id_sf_legislatura" int2 NOT NULL,
  "id_sf_senador" int8 NOT NULL
)
;

-- ----------------------------
-- Table structure for sf_motivo_afastamento
-- ----------------------------
DROP TABLE IF EXISTS "senado"."sf_motivo_afastamento";
CREATE TABLE "senado"."sf_motivo_afastamento" (
  "id" char(5) COLLATE "pg_catalog"."default" NOT NULL,
  "descricao" varchar(100) COLLATE "pg_catalog"."default" NOT NULL
)
;

-- ----------------------------
-- Table structure for sf_referencia_cargo
-- ----------------------------
DROP TABLE IF EXISTS "senado"."sf_referencia_cargo";
CREATE TABLE "senado"."sf_referencia_cargo" (
  "id" int2 NOT NULL,
  "descricao" varchar(100) COLLATE "pg_catalog"."default" NOT NULL
)
;

-- ----------------------------
-- Table structure for sf_remuneracao
-- ----------------------------
DROP TABLE IF EXISTS "senado"."sf_remuneracao";
CREATE TABLE "senado"."sf_remuneracao" (
  "id" int8 NOT NULL,
  "id_vinculo" int2 NOT NULL,
  "id_categoria" int2 NOT NULL,
  "id_cargo" int2,
  "id_referencia_cargo" int2,
  "id_simbolo_funcao" int2,
  "id_lotacao" int4 NOT NULL,
  "id_tipo_folha" int2 NOT NULL,
  "ano_mes" int4,
  "admissao" int4 NOT NULL,
  "remun_basica" numeric(10,2),
  "vant_pessoais" numeric(10,2),
  "func_comissionada" numeric(10,2),
  "grat_natalina" numeric(10,2),
  "horas_extras" numeric(10,2),
  "outras_eventuais" numeric(10,2),
  "abono_permanencia" numeric(10,2),
  "reversao_teto_const" numeric(10,2),
  "imposto_renda" numeric(10,2),
  "previdencia" numeric(10,2),
  "faltas" numeric(10,2),
  "rem_liquida" numeric(10,2),
  "diarias" numeric(10,2),
  "auxilios" numeric(10,2),
  "vant_indenizatorias" numeric(10,2),
  "custo_total" numeric(10,2)
)
;

-- ----------------------------
-- Table structure for sf_secretario
-- ----------------------------
DROP TABLE IF EXISTS "senado"."sf_secretario";
CREATE TABLE "senado"."sf_secretario" (
  "id" int8,
  "id_senador" int8,
  "nome" varchar(255) COLLATE "pg_catalog"."default",
  "id_funcao" int2,
  "id_cargo" int2,
  "id_vinculo" int2,
  "id_categoria" int2,
  "id_referencia_cargo" int2,
  "id_especialidade" int2,
  "id_lotacao" int4,
  "admissao" int4,
  "situacao" int2
)
;

-- ----------------------------
-- Table structure for sf_secretario_completo
-- ----------------------------
DROP TABLE IF EXISTS "senado"."sf_secretario_completo";
CREATE TABLE "senado"."sf_secretario_completo" (
  "id" int8,
  "id_senador" int8,
  "nome" varchar(255) COLLATE "pg_catalog"."default",
  "funcao" varchar(10) COLLATE "pg_catalog"."default",
  "nome_funcao" varchar(255) COLLATE "pg_catalog"."default",
  "vinculo" varchar(255) COLLATE "pg_catalog"."default",
  "situacao" varchar(255) COLLATE "pg_catalog"."default",
  "admissao" int4,
  "cargo" varchar(100) COLLATE "pg_catalog"."default",
  "padrao" varchar(10) COLLATE "pg_catalog"."default",
  "especialidade" varchar(255) COLLATE "pg_catalog"."default",
  "lotacao" varchar(255) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for sf_senador
-- ----------------------------
DROP TABLE IF EXISTS "senado"."sf_senador";
CREATE TABLE "senado"."sf_senador" (
  "id" int4 NOT NULL,
  "codigo" int4,
  "nome" varchar(255) COLLATE "pg_catalog"."default",
  "nome_completo" varchar(255) COLLATE "pg_catalog"."default",
  "sexo" char(1) COLLATE "pg_catalog"."default",
  "nascimento" date,
  "naturalidade" varchar(50) COLLATE "pg_catalog"."default",
  "id_estado_naturalidade" int2,
  "profissao" varchar(100) COLLATE "pg_catalog"."default",
  "id_partido" int2 NOT NULL,
  "id_estado" int2,
  "email" varchar(100) COLLATE "pg_catalog"."default",
  "site" varchar(100) COLLATE "pg_catalog"."default",
  "ativo" char(1) COLLATE "pg_catalog"."default",
  "nome_importacao" varchar(255) COLLATE "pg_catalog"."default",
  "valor_total_ceaps" numeric(16,2) NOT NULL,
  "valor_total_remuneracao" numeric(16,2) NOT NULL,
  "hash" bytea
)
;

-- ----------------------------
-- Table structure for sf_senador_campeao_gasto
-- ----------------------------
DROP TABLE IF EXISTS "senado"."sf_senador_campeao_gasto";
CREATE TABLE "senado"."sf_senador_campeao_gasto" (
  "id_sf_senador" int4 NOT NULL,
  "nome_parlamentar" varchar(100) COLLATE "pg_catalog"."default",
  "valor_total" numeric(10,2),
  "sigla_partido" varchar(20) COLLATE "pg_catalog"."default",
  "sigla_estado" varchar(2) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for sf_senador_historico_academico
-- ----------------------------
DROP TABLE IF EXISTS "senado"."sf_senador_historico_academico";
CREATE TABLE "senado"."sf_senador_historico_academico" (
  "id_sf_senador" int4 NOT NULL,
  "nome_curso" varchar(255) COLLATE "pg_catalog"."default" NOT NULL,
  "grau_instrucao" varchar(50) COLLATE "pg_catalog"."default" NOT NULL,
  "estabelecimento" varchar(255) COLLATE "pg_catalog"."default",
  "local" varchar(255) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for sf_senador_partido
-- ----------------------------
DROP TABLE IF EXISTS "senado"."sf_senador_partido";
CREATE TABLE "senado"."sf_senador_partido" (
  "id" int4 NOT NULL,
  "id_sf_senador" int4 NOT NULL,
  "id_partido" int2 NOT NULL,
  "filiacao" date,
  "desfiliacao" date
)
;

-- ----------------------------
-- Table structure for sf_senador_profissao
-- ----------------------------
DROP TABLE IF EXISTS "senado"."sf_senador_profissao";
CREATE TABLE "senado"."sf_senador_profissao" (
  "id_sf_senador" int4 NOT NULL,
  "id_profissao" int4 NOT NULL
)
;

-- ----------------------------
-- Table structure for sf_situacao
-- ----------------------------
DROP TABLE IF EXISTS "senado"."sf_situacao";
CREATE TABLE "senado"."sf_situacao" (
  "id" int2 NOT NULL,
  "descricao" varchar(50) COLLATE "pg_catalog"."default" NOT NULL
)
;

-- ----------------------------
-- Table structure for sf_tipo_folha
-- ----------------------------
DROP TABLE IF EXISTS "senado"."sf_tipo_folha";
CREATE TABLE "senado"."sf_tipo_folha" (
  "id" int2 NOT NULL,
  "descricao" varchar(100) COLLATE "pg_catalog"."default" NOT NULL
)
;

-- ----------------------------
-- Table structure for sf_vinculo
-- ----------------------------
DROP TABLE IF EXISTS "senado"."sf_vinculo";
CREATE TABLE "senado"."sf_vinculo" (
  "id" int2 NOT NULL,
  "descricao" varchar(100) COLLATE "pg_catalog"."default" NOT NULL
)
;

-- ----------------------------
-- Primary Key structure for table sf_cargo
-- ----------------------------
ALTER TABLE "senado"."sf_cargo" ADD CONSTRAINT "sf_cargo_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table sf_categoria
-- ----------------------------
ALTER TABLE "senado"."sf_categoria" ADD CONSTRAINT "sf_categoria_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table sf_despesa
-- ----------------------------
CREATE INDEX "ano_mes" ON "senado"."sf_despesa" USING btree (
  "ano_mes" "pg_catalog"."int8_ops" ASC NULLS LAST
);
CREATE INDEX "id_sf_despesa_tipo" ON "senado"."sf_despesa" USING btree (
  "id_sf_despesa_tipo" "pg_catalog"."int2_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table sf_despesa
-- ----------------------------
ALTER TABLE "senado"."sf_despesa" ADD CONSTRAINT "sf_despesa_pkey" PRIMARY KEY ("id_sf_senador", "id");

-- ----------------------------
-- Primary Key structure for table sf_despesa_resumo_mensal
-- ----------------------------
ALTER TABLE "senado"."sf_despesa_resumo_mensal" ADD CONSTRAINT "sf_despesa_resumo_mensal_pkey" PRIMARY KEY ("ano", "mes");

-- ----------------------------
-- Indexes structure for table sf_despesa_tipo
-- ----------------------------
CREATE UNIQUE INDEX "descricao_UNIQUE" ON "senado"."sf_despesa_tipo" USING btree (
  "descricao" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table sf_despesa_tipo
-- ----------------------------
ALTER TABLE "senado"."sf_despesa_tipo" ADD CONSTRAINT "sf_despesa_tipo_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table sf_funcao
-- ----------------------------
ALTER TABLE "senado"."sf_funcao" ADD CONSTRAINT "sf_funcao_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table sf_legislatura
-- ----------------------------
ALTER TABLE "senado"."sf_legislatura" ADD CONSTRAINT "sf_legislatura_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table sf_lotacao
-- ----------------------------
CREATE INDEX "id_senador" ON "senado"."sf_lotacao" USING btree (
  "id_senador" "pg_catalog"."int8_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table sf_lotacao
-- ----------------------------
ALTER TABLE "senado"."sf_lotacao" ADD CONSTRAINT "sf_lotacao_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table sf_mandato
-- ----------------------------
CREATE INDEX "FK_sf_mandato_estado" ON "senado"."sf_mandato" USING btree (
  "id_estado" "pg_catalog"."int2_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table sf_mandato
-- ----------------------------
ALTER TABLE "senado"."sf_mandato" ADD CONSTRAINT "sf_mandato_pkey" PRIMARY KEY ("id", "id_sf_senador");

-- ----------------------------
-- Indexes structure for table sf_mandato_exercicio
-- ----------------------------
CREATE INDEX "FK_sf_mandato_exercicio_sf_mandato" ON "senado"."sf_mandato_exercicio" USING btree (
  "id_sf_mandato" "pg_catalog"."int4_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table sf_mandato_exercicio
-- ----------------------------
ALTER TABLE "senado"."sf_mandato_exercicio" ADD CONSTRAINT "sf_mandato_exercicio_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table sf_mandato_legislatura
-- ----------------------------
CREATE INDEX "FK__sf_legislatura" ON "senado"."sf_mandato_legislatura" USING btree (
  "id_sf_legislatura" "pg_catalog"."int2_ops" ASC NULLS LAST
);
CREATE INDEX "FK__sf_mandato" ON "senado"."sf_mandato_legislatura" USING btree (
  "id_sf_mandato" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE UNIQUE INDEX "id_sf_mandato_id_sf_legislatura" ON "senado"."sf_mandato_legislatura" USING btree (
  "id_sf_mandato" "pg_catalog"."int4_ops" ASC NULLS LAST,
  "id_sf_legislatura" "pg_catalog"."int2_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table sf_motivo_afastamento
-- ----------------------------
ALTER TABLE "senado"."sf_motivo_afastamento" ADD CONSTRAINT "sf_motivo_afastamento_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table sf_referencia_cargo
-- ----------------------------
ALTER TABLE "senado"."sf_referencia_cargo" ADD CONSTRAINT "sf_referencia_cargo_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table sf_remuneracao
-- ----------------------------
CREATE INDEX "sfm_ano_mes" ON "senado"."sf_remuneracao" USING btree (
  "ano_mes" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "sfm_id_cargo" ON "senado"."sf_remuneracao" USING btree (
  "id_cargo" "pg_catalog"."int2_ops" ASC NULLS LAST
);
CREATE INDEX "sfm_id_categoria" ON "senado"."sf_remuneracao" USING btree (
  "id_categoria" "pg_catalog"."int2_ops" ASC NULLS LAST
);
CREATE INDEX "sfm_id_lotacao" ON "senado"."sf_remuneracao" USING btree (
  "id_lotacao" "pg_catalog"."int4_ops" ASC NULLS LAST
);
CREATE INDEX "sfm_id_referencia_cargo" ON "senado"."sf_remuneracao" USING btree (
  "id_referencia_cargo" "pg_catalog"."int2_ops" ASC NULLS LAST
);
CREATE INDEX "sfm_id_simbolo_funcao" ON "senado"."sf_remuneracao" USING btree (
  "id_simbolo_funcao" "pg_catalog"."int2_ops" ASC NULLS LAST
);
CREATE INDEX "sfm_id_tipo_folha" ON "senado"."sf_remuneracao" USING btree (
  "id_tipo_folha" "pg_catalog"."int2_ops" ASC NULLS LAST
);
CREATE INDEX "sfm_id_vinculo" ON "senado"."sf_remuneracao" USING btree (
  "id_vinculo" "pg_catalog"."int2_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table sf_remuneracao
-- ----------------------------
ALTER TABLE "senado"."sf_remuneracao" ADD CONSTRAINT "sf_remuneracao_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Indexes structure for table sf_senador
-- ----------------------------
CREATE INDEX "nome_completo" ON "senado"."sf_senador" USING btree (
  "nome_completo" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table sf_senador
-- ----------------------------
ALTER TABLE "senado"."sf_senador" ADD CONSTRAINT "sf_senador_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table sf_senador_campeao_gasto
-- ----------------------------
ALTER TABLE "senado"."sf_senador_campeao_gasto" ADD CONSTRAINT "sf_senador_campeao_gasto_pkey" PRIMARY KEY ("id_sf_senador");

-- ----------------------------
-- Indexes structure for table sf_senador_historico_academico
-- ----------------------------
CREATE UNIQUE INDEX "unique" ON "senado"."sf_senador_historico_academico" USING btree (
  "id_sf_senador" "pg_catalog"."int4_ops" ASC NULLS LAST,
  "nome_curso" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST,
  "grau_instrucao" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Indexes structure for table sf_senador_partido
-- ----------------------------
CREATE UNIQUE INDEX "id_sf_senador_id_partido_filiacao" ON "senado"."sf_senador_partido" USING btree (
  "id_sf_senador" "pg_catalog"."int4_ops" ASC NULLS LAST,
  "id_partido" "pg_catalog"."int2_ops" ASC NULLS LAST,
  "filiacao" "pg_catalog"."date_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table sf_senador_partido
-- ----------------------------
ALTER TABLE "senado"."sf_senador_partido" ADD CONSTRAINT "sf_senador_partido_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table sf_senador_profissao
-- ----------------------------
ALTER TABLE "senado"."sf_senador_profissao" ADD CONSTRAINT "sf_senador_profissao_pkey" PRIMARY KEY ("id_sf_senador", "id_profissao");

-- ----------------------------
-- Primary Key structure for table sf_situacao
-- ----------------------------
ALTER TABLE "senado"."sf_situacao" ADD CONSTRAINT "sf_situacao_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table sf_tipo_folha
-- ----------------------------
ALTER TABLE "senado"."sf_tipo_folha" ADD CONSTRAINT "sf_tipo_folha_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table sf_vinculo
-- ----------------------------
ALTER TABLE "senado"."sf_vinculo" ADD CONSTRAINT "sf_vinculo_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Foreign Keys structure for table sf_mandato
-- ----------------------------
ALTER TABLE "senado"."sf_mandato" ADD CONSTRAINT "sf_mandato_id_estado_fkey" FOREIGN KEY ("id_estado") REFERENCES "public"."estado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;
ALTER TABLE "senado"."sf_mandato" ADD CONSTRAINT "sf_mandato_id_partido_fkey" FOREIGN KEY ("id_partido") REFERENCES "public"."partido" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;
ALTER TABLE "senado"."sf_mandato" ADD CONSTRAINT "sf_mandato_id_sf_senador_fkey" FOREIGN KEY ("id_sf_senador") REFERENCES "public"."sf_senador" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table sf_mandato_exercicio
-- ----------------------------
ALTER TABLE "senado"."sf_mandato_exercicio" ADD CONSTRAINT "sf_mandato_exercicio_id_sf_senador_fkey" FOREIGN KEY ("id_sf_senador") REFERENCES "public"."sf_senador" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table sf_mandato_legislatura
-- ----------------------------
ALTER TABLE "senado"."sf_mandato_legislatura" ADD CONSTRAINT "sf_mandato_legislatura_id_sf_legislatura_fkey" FOREIGN KEY ("id_sf_legislatura") REFERENCES "public"."sf_legislatura" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table sf_remuneracao
-- ----------------------------
ALTER TABLE "senado"."sf_remuneracao" ADD CONSTRAINT "sf_remuneracao_id_cargo_fkey" FOREIGN KEY ("id_cargo") REFERENCES "public"."sf_cargo" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;
ALTER TABLE "senado"."sf_remuneracao" ADD CONSTRAINT "sf_remuneracao_id_categoria_fkey" FOREIGN KEY ("id_categoria") REFERENCES "public"."sf_categoria" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;
ALTER TABLE "senado"."sf_remuneracao" ADD CONSTRAINT "sf_remuneracao_id_lotacao_fkey" FOREIGN KEY ("id_lotacao") REFERENCES "public"."sf_lotacao" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;
ALTER TABLE "senado"."sf_remuneracao" ADD CONSTRAINT "sf_remuneracao_id_referencia_cargo_fkey" FOREIGN KEY ("id_referencia_cargo") REFERENCES "public"."sf_referencia_cargo" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;
ALTER TABLE "senado"."sf_remuneracao" ADD CONSTRAINT "sf_remuneracao_id_simbolo_funcao_fkey" FOREIGN KEY ("id_simbolo_funcao") REFERENCES "public"."sf_funcao" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;
ALTER TABLE "senado"."sf_remuneracao" ADD CONSTRAINT "sf_remuneracao_id_tipo_folha_fkey" FOREIGN KEY ("id_tipo_folha") REFERENCES "public"."sf_tipo_folha" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;
ALTER TABLE "senado"."sf_remuneracao" ADD CONSTRAINT "sf_remuneracao_id_vinculo_fkey" FOREIGN KEY ("id_vinculo") REFERENCES "public"."sf_vinculo" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table sf_senador_partido
-- ----------------------------
ALTER TABLE "senado"."sf_senador_partido" ADD CONSTRAINT "sf_senador_partido_id_partido_fkey" FOREIGN KEY ("id_partido") REFERENCES "public"."partido" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;
ALTER TABLE "senado"."sf_senador_partido" ADD CONSTRAINT "sf_senador_partido_id_sf_senador_fkey" FOREIGN KEY ("id_sf_senador") REFERENCES "public"."sf_senador" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- ----------------------------
-- Foreign Keys structure for table sf_senador_profissao
-- ----------------------------
ALTER TABLE "senado"."sf_senador_profissao" ADD CONSTRAINT "sf_senador_profissao_id_sf_senador_fkey" FOREIGN KEY ("id_sf_senador") REFERENCES "public"."sf_senador" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;
