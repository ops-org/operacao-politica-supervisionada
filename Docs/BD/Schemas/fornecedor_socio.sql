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

 Date: 08/01/2026 22:55:51
*/


-- ----------------------------
-- Table structure for fornecedor_socio
-- ----------------------------
DROP TABLE IF EXISTS "fornecedor"."fornecedor_socio";
CREATE TABLE "fornecedor"."fornecedor_socio" (
  "id" int4 NOT NULL DEFAULT nextval('"fornecedor".fornecedor_socio_id_seq'::regclass),
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
-- Foreign Keys structure for table fornecedor_socio
-- ----------------------------
ALTER TABLE "fornecedor"."fornecedor_socio" ADD CONSTRAINT "fk_fornecedor_socio_faixa_etaria" FOREIGN KEY ("id_fornecedor_faixa_etaria") REFERENCES "fornecedor"."fornecedor_faixa_etaria" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;
ALTER TABLE "fornecedor"."fornecedor_socio" ADD CONSTRAINT "fk_fornecedor_socio_fornecedor" FOREIGN KEY ("id_fornecedor") REFERENCES "fornecedor"."fornecedor" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;
ALTER TABLE "fornecedor"."fornecedor_socio" ADD CONSTRAINT "fk_fornecedor_socio_qualificacao" FOREIGN KEY ("id_fornecedor_socio_qualificacao") REFERENCES "fornecedor"."fornecedor_socio_qualificacao" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;
ALTER TABLE "fornecedor"."fornecedor_socio" ADD CONSTRAINT "fk_fornecedor_socio_representante_qualificacao" FOREIGN KEY ("id_fornecedor_socio_representante_qualificacao") REFERENCES "fornecedor"."fornecedor_socio_qualificacao" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;
