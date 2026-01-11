/*
 Navicat Premium Dump SQL

 Source Server         : Postgres WSL
 Source Server Type    : PostgreSQL
 Source Server Version : 160011 (160011)
 Source Host           : 172.31.250.64:5432
 Source Catalog        : ops
 Source Schema         : temp

 Target Server Type    : PostgreSQL
 Target Server Version : 160011 (160011)
 File Encoding         : 65001

 Date: 08/01/2026 15:08:54
*/


-- ----------------------------
-- Table structure for arquivo_cheksum
-- ----------------------------
DROP TABLE IF EXISTS "temp"."arquivo_cheksum";
CREATE TABLE "temp"."arquivo_cheksum" (
  "id" int8 NOT NULL,
  "nome" varchar(255) COLLATE "pg_catalog"."default" NOT NULL,
  "checksum" varchar(255) COLLATE "pg_catalog"."default",
  "tamanho_bytes" int8,
  "criacao" timestamp(6) NOT NULL,
  "atualizacao" timestamp(6) NOT NULL,
  "verificacao" timestamp(6) NOT NULL,
  "valor_total" numeric(20,6) NOT NULL,
  "divergencia" numeric(20,6) NOT NULL,
  "revisado" varchar(1) COLLATE "pg_catalog"."default" NOT NULL
)
;

-- ----------------------------
-- Table structure for cf_deputado_funcionario_temp
-- ----------------------------
DROP TABLE IF EXISTS "temp"."cf_deputado_funcionario_temp";
CREATE TABLE "temp"."cf_deputado_funcionario_temp" (
  "id_cf_deputado" int4,
  "chave" varchar(45) COLLATE "pg_catalog"."default" NOT NULL,
  "nome" varchar(100) COLLATE "pg_catalog"."default" NOT NULL,
  "grupo_funcional" varchar(100) COLLATE "pg_catalog"."default" NOT NULL,
  "nivel" varchar(100) COLLATE "pg_catalog"."default",
  "periodo_de" date NOT NULL,
  "periodo_ate" date
)
;

-- ----------------------------
-- Table structure for cf_despesa_temp
-- ----------------------------
DROP TABLE IF EXISTS "temp"."cf_despesa_temp";
CREATE TABLE "temp"."cf_despesa_temp" (
  "id" int8 NOT NULL,
  "idDeputado" int8,
  "nomeParlamentar" varchar(100) COLLATE "pg_catalog"."default",
  "numeroCarteiraParlamentar" int4,
  "legislatura" int4,
  "siglaUF" varchar(2) COLLATE "pg_catalog"."default",
  "siglaPartido" varchar(20) COLLATE "pg_catalog"."default",
  "codigoLegislatura" int4,
  "numeroSubCota" int4,
  "descricao" varchar(100) COLLATE "pg_catalog"."default",
  "numeroEspecificacaoSubCota" int4,
  "descricaoEspecificacao" varchar(100) COLLATE "pg_catalog"."default",
  "fornecedor" varchar(255) COLLATE "pg_catalog"."default",
  "cnpjCPF" varchar(14) COLLATE "pg_catalog"."default",
  "numero" varchar(50) COLLATE "pg_catalog"."default",
  "tipoDocumento" varchar(10) COLLATE "pg_catalog"."default",
  "dataEmissao" date,
  "valorDocumento" numeric(10,2),
  "valorGlosa" numeric(10,2),
  "valorLiquido" numeric(10,2),
  "mes" numeric(2,0),
  "ano" numeric(4,0),
  "parcela" numeric(3,0),
  "passageiro" varchar(100) COLLATE "pg_catalog"."default",
  "trecho" varchar(100) COLLATE "pg_catalog"."default",
  "lote" int4,
  "ressarcimento" int4,
  "idDocumento" varchar(20) COLLATE "pg_catalog"."default",
  "restituicao" numeric(10,2),
  "datPagamentoRestituicao" date,
  "numeroDeputadoID" int4,
  "cpf" varchar(15) COLLATE "pg_catalog"."default",
  "urlDocumento" varchar(255) COLLATE "pg_catalog"."default",
  "hash" bytea
)
;

-- ----------------------------
-- Table structure for cf_funcionario_temp
-- ----------------------------
DROP TABLE IF EXISTS "temp"."cf_funcionario_temp";
CREATE TABLE "temp"."cf_funcionario_temp" (
  "chave" varchar(50) COLLATE "pg_catalog"."default" NOT NULL,
  "nome" varchar(255) COLLATE "pg_catalog"."default",
  "categoria_funcional" varchar(255) COLLATE "pg_catalog"."default",
  "cargo" varchar(255) COLLATE "pg_catalog"."default",
  "nivel" varchar(255) COLLATE "pg_catalog"."default",
  "funcao_comissionada" varchar(255) COLLATE "pg_catalog"."default",
  "area_atuacao" varchar(255) COLLATE "pg_catalog"."default",
  "local_trabalho" varchar(255) COLLATE "pg_catalog"."default",
  "situacao" varchar(255) COLLATE "pg_catalog"."default",
  "data_designacao_funcao" date
)
;
COMMENT ON COLUMN "temp"."cf_funcionario_temp"."categoria_funcional" IS 'Categoria funcional';
COMMENT ON COLUMN "temp"."cf_funcionario_temp"."cargo" IS 'Cargo';
COMMENT ON COLUMN "temp"."cf_funcionario_temp"."nivel" IS 'Nível';
COMMENT ON COLUMN "temp"."cf_funcionario_temp"."funcao_comissionada" IS 'Função comissionada';
COMMENT ON COLUMN "temp"."cf_funcionario_temp"."area_atuacao" IS 'Área de atuação';
COMMENT ON COLUMN "temp"."cf_funcionario_temp"."local_trabalho" IS 'Local de trabalho';
COMMENT ON COLUMN "temp"."cf_funcionario_temp"."situacao" IS 'Situação';
COMMENT ON COLUMN "temp"."cf_funcionario_temp"."data_designacao_funcao" IS 'Data da designação da função';

-- ----------------------------
-- Table structure for cf_mandato_temp
-- ----------------------------
DROP TABLE IF EXISTS "temp"."cf_mandato_temp";
CREATE TABLE "temp"."cf_mandato_temp" (
  "ideCadastro" int4,
  "numLegislatura" int4,
  "nomeParlamentar" varchar(45) COLLATE "pg_catalog"."default",
  "Sexo" varchar(45) COLLATE "pg_catalog"."default",
  "Profissao" varchar(255) COLLATE "pg_catalog"."default",
  "LegendaPartidoEleito" char(10) COLLATE "pg_catalog"."default",
  "UFEleito" char(2) COLLATE "pg_catalog"."default",
  "Condicao" varchar(45) COLLATE "pg_catalog"."default",
  "SituacaoMandato" varchar(45) COLLATE "pg_catalog"."default",
  "Matricula" int4,
  "Gabinete" varchar(45) COLLATE "pg_catalog"."default",
  "Anexo" varchar(45) COLLATE "pg_catalog"."default",
  "Fone" varchar(45) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for cf_remuneracao_temp
-- ----------------------------
DROP TABLE IF EXISTS "temp"."cf_remuneracao_temp";
CREATE TABLE "temp"."cf_remuneracao_temp" (
  "id" int4,
  "ano_mes" int4,
  "cargo" varchar(255) COLLATE "pg_catalog"."default",
  "grupo_funcional" varchar(255) COLLATE "pg_catalog"."default",
  "tipo_folha" varchar(255) COLLATE "pg_catalog"."default",
  "admissao" int4,
  "remun_basica" numeric(10,2),
  "vant_pessoais" numeric(10,2),
  "func_comissionada" numeric(10,2),
  "grat_natalina" numeric(10,2),
  "ferias" numeric(10,2),
  "outras_eventuais" numeric(10,2),
  "abono_permanencia" numeric(10,2),
  "reversao_teto_const" numeric(10,2),
  "previdencia" numeric(10,2),
  "imposto_renda" numeric(10,2),
  "rem_liquida" numeric(10,2),
  "diarias" numeric(10,2),
  "auxilios" numeric(10,2),
  "vant_indenizatorias" numeric(10,2)
)
;
COMMENT ON COLUMN "temp"."cf_remuneracao_temp"."cargo" IS 'Cargo Individualizado do Servidor';
COMMENT ON COLUMN "temp"."cf_remuneracao_temp"."grupo_funcional" IS 'Grupo Funcional';
COMMENT ON COLUMN "temp"."cf_remuneracao_temp"."tipo_folha" IS 'Folha de Pagamento';
COMMENT ON COLUMN "temp"."cf_remuneracao_temp"."admissao" IS 'Ano Ingresso';
COMMENT ON COLUMN "temp"."cf_remuneracao_temp"."remun_basica" IS 'Remuneração Fixa';
COMMENT ON COLUMN "temp"."cf_remuneracao_temp"."vant_pessoais" IS 'Vantagens de Natureza Pessoal';
COMMENT ON COLUMN "temp"."cf_remuneracao_temp"."func_comissionada" IS 'Função ou Cargo em Comissão';
COMMENT ON COLUMN "temp"."cf_remuneracao_temp"."grat_natalina" IS 'Gratificação Natalina';
COMMENT ON COLUMN "temp"."cf_remuneracao_temp"."ferias" IS 'Férias (1/3 Constitucional)';
COMMENT ON COLUMN "temp"."cf_remuneracao_temp"."outras_eventuais" IS 'Outras Remunerações Eventuais/Provisórias(*)';
COMMENT ON COLUMN "temp"."cf_remuneracao_temp"."abono_permanencia" IS 'Abono de Permanência';
COMMENT ON COLUMN "temp"."cf_remuneracao_temp"."reversao_teto_const" IS 'Redutor Constitucional';
COMMENT ON COLUMN "temp"."cf_remuneracao_temp"."previdencia" IS 'Constribuição Previdenciária';
COMMENT ON COLUMN "temp"."cf_remuneracao_temp"."imposto_renda" IS 'Imposto de Renda';
COMMENT ON COLUMN "temp"."cf_remuneracao_temp"."rem_liquida" IS 'Remuneração Após Descontos Obrigatórios';
COMMENT ON COLUMN "temp"."cf_remuneracao_temp"."diarias" IS 'Diárias';
COMMENT ON COLUMN "temp"."cf_remuneracao_temp"."auxilios" IS 'Auxílios';
COMMENT ON COLUMN "temp"."cf_remuneracao_temp"."vant_indenizatorias" IS 'Vantagens Indenizatórias';

-- ----------------------------
-- Table structure for cf_secretario_remuneracao_temp
-- ----------------------------
DROP TABLE IF EXISTS "temp"."cf_secretario_remuneracao_temp";
CREATE TABLE "temp"."cf_secretario_remuneracao_temp" (
  "id_cf_secretario" varchar(255) COLLATE "pg_catalog"."default" NOT NULL,
  "referencia" varchar(255) COLLATE "pg_catalog"."default",
  "descricao" varchar(255) COLLATE "pg_catalog"."default",
  "remuneracao_fixa" numeric(10,2),
  "vantagens_natureza_pessoal" numeric(10,2),
  "funcao_ou_cargo_em_comissao" numeric(10,2),
  "gratificacao_natalina" numeric(10,2),
  "ferias" numeric(10,2),
  "outras_remuneracoes" numeric(10,2),
  "valor_bruto" numeric(10,2),
  "abono_permanencia" numeric(10,2),
  "redutor_constitucional" numeric(10,2),
  "contribuicao_previdenciaria" numeric(10,2),
  "imposto_renda" numeric(10,2),
  "valor_liquido" numeric(10,2),
  "valor_diarias" numeric(10,2),
  "valor_auxilios" numeric(10,2),
  "valor_vantagens" numeric(10,2),
  "valor_outros" numeric(10,2)
)
;

-- ----------------------------
-- Table structure for cl_deputado_de_para
-- ----------------------------
DROP TABLE IF EXISTS "temp"."cl_deputado_de_para";
CREATE TABLE "temp"."cl_deputado_de_para" (
  "id" int8 NOT NULL,
  "nome" varchar(100) COLLATE "pg_catalog"."default" NOT NULL,
  "id_estado" int2 NOT NULL
)
;

-- ----------------------------
-- Table structure for cl_despesa
-- ----------------------------
DROP TABLE IF EXISTS "temp"."cl_despesa";
CREATE TABLE "temp"."cl_despesa" (
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
DROP TABLE IF EXISTS "temp"."cl_despesa_especificacao";
CREATE TABLE "temp"."cl_despesa_especificacao" (
  "id" int4 NOT NULL,
  "id_cl_despesa_tipo" int4,
  "descricao" varchar(200) COLLATE "pg_catalog"."default" NOT NULL
)
;

-- ----------------------------
-- Table structure for cl_despesa_resumo_temp
-- ----------------------------
DROP TABLE IF EXISTS "temp"."cl_despesa_resumo_temp";
CREATE TABLE "temp"."cl_despesa_resumo_temp" (
  "id_deputado" int8,
  "ano" int4,
  "mes" int4,
  "valor" numeric(10,2),
  "valor_ajustado" numeric(10,2)
)
;

-- ----------------------------
-- Table structure for cl_despesa_temp
-- ----------------------------
DROP TABLE IF EXISTS "temp"."cl_despesa_temp";
CREATE TABLE "temp"."cl_despesa_temp" (
  "id" int4 NOT NULL,
  "id_cl_deputado" int4,
  "nome" varchar(255) COLLATE "pg_catalog"."default",
  "nome_civil" varchar(255) COLLATE "pg_catalog"."default",
  "cpf" varchar(50) COLLATE "pg_catalog"."default",
  "empresa" varchar(255) COLLATE "pg_catalog"."default",
  "cnpj_cpf" varchar(20) COLLATE "pg_catalog"."default",
  "data_emissao" date,
  "tipo_verba" varchar(100) COLLATE "pg_catalog"."default",
  "despesa_tipo" varchar(400) COLLATE "pg_catalog"."default",
  "documento" varchar(100) COLLATE "pg_catalog"."default",
  "observacao" varchar(8000) COLLATE "pg_catalog"."default",
  "valor" numeric(10,2) NOT NULL,
  "ano" int4 NOT NULL,
  "mes" int4,
  "favorecido" varchar(200) COLLATE "pg_catalog"."default",
  "hash" bytea
)
;

-- ----------------------------
-- Table structure for cl_empenho_temp
-- ----------------------------
DROP TABLE IF EXISTS "temp"."cl_empenho_temp";
CREATE TABLE "temp"."cl_empenho_temp" (
  "id" int8 NOT NULL,
  "nome_favorecido" varchar(100) COLLATE "pg_catalog"."default" NOT NULL,
  "id_cl_deputado" int8 NOT NULL,
  "nome_deputado" varchar(100) COLLATE "pg_catalog"."default" NOT NULL,
  "cnpj_cpf" varchar(100) COLLATE "pg_catalog"."default" NOT NULL,
  "numero_empenho" varchar(100) COLLATE "pg_catalog"."default" NOT NULL,
  "data" date NOT NULL,
  "competencia" date NOT NULL,
  "valor_empenhado" numeric(10,2) NOT NULL,
  "valor_pago" numeric(10,2) NOT NULL
)
;

-- ----------------------------
-- Table structure for fornecedor_cnpj_incorreto
-- ----------------------------
DROP TABLE IF EXISTS "temp"."fornecedor_cnpj_incorreto";
CREATE TABLE "temp"."fornecedor_cnpj_incorreto" (
  "cnpj_incorreto" char(15) COLLATE "pg_catalog"."default" NOT NULL,
  "id_fornecedor_incorreto" int8,
  "nome" varchar(255) COLLATE "pg_catalog"."default",
  "id_fornecedor_correto" int8,
  "cnpj_correto" char(15) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for fornecedor_correcao
-- ----------------------------
DROP TABLE IF EXISTS "temp"."fornecedor_correcao";
CREATE TABLE "temp"."fornecedor_correcao" (
  "cnpj_cpf" varchar(15) COLLATE "pg_catalog"."default",
  "nome" varchar(255) COLLATE "pg_catalog"."default",
  "cnpj_cpf_correto" char(15) COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for sf_despesa_temp
-- ----------------------------
DROP TABLE IF EXISTS "temp"."sf_despesa_temp";
CREATE TABLE "temp"."sf_despesa_temp" (
  "ano" numeric(4,0),
  "mes" numeric(2,0),
  "senador" varchar(255) COLLATE "pg_catalog"."default",
  "tipo_despesa" varchar(255) COLLATE "pg_catalog"."default",
  "cnpj_cpf" varchar(14) COLLATE "pg_catalog"."default",
  "fornecedor" varchar(255) COLLATE "pg_catalog"."default",
  "documento" varchar(50) COLLATE "pg_catalog"."default",
  "data" timestamp(6),
  "detalhamento" text COLLATE "pg_catalog"."default",
  "valor_reembolsado" numeric(10,2),
  "cod_documento" numeric(20,0),
  "hash" bytea
)
;

-- ----------------------------
-- Table structure for sf_remuneracao_temp
-- ----------------------------
DROP TABLE IF EXISTS "temp"."sf_remuneracao_temp";
CREATE TABLE "temp"."sf_remuneracao_temp" (
  "ano_mes" int4,
  "vinculo" varchar(255) COLLATE "pg_catalog"."default",
  "categoria" varchar(255) COLLATE "pg_catalog"."default",
  "cargo" varchar(255) COLLATE "pg_catalog"."default",
  "referencia_cargo" varchar(255) COLLATE "pg_catalog"."default",
  "simbolo_funcao" varchar(255) COLLATE "pg_catalog"."default",
  "lotacao_exercicio" varchar(255) COLLATE "pg_catalog"."default",
  "tipo_folha" varchar(255) COLLATE "pg_catalog"."default",
  "admissao" int4,
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
  "vant_indenizatorias" numeric(10,2)
)
;

-- ----------------------------
-- Table structure for sf_secretario_temp
-- ----------------------------
DROP TABLE IF EXISTS "temp"."sf_secretario_temp";
CREATE TABLE "temp"."sf_secretario_temp" (
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
-- Table structure for tse_candidato
-- ----------------------------
DROP TABLE IF EXISTS "temp"."tse_candidato";
CREATE TABLE "temp"."tse_candidato" (
  "DT_GERACAO" text COLLATE "pg_catalog"."default",
  "HH_GERACAO" text COLLATE "pg_catalog"."default",
  "ANO_ELEICAO" text COLLATE "pg_catalog"."default",
  "CD_TIPO_ELEICAO" text COLLATE "pg_catalog"."default",
  "NM_TIPO_ELEICAO" text COLLATE "pg_catalog"."default",
  "NR_TURNO" text COLLATE "pg_catalog"."default",
  "CD_ELEICAO" text COLLATE "pg_catalog"."default",
  "DS_ELEICAO" text COLLATE "pg_catalog"."default",
  "DT_ELEICAO" text COLLATE "pg_catalog"."default",
  "TP_ABRANGENCIA" text COLLATE "pg_catalog"."default",
  "SG_UF" text COLLATE "pg_catalog"."default",
  "SG_UE" text COLLATE "pg_catalog"."default",
  "NM_UE" text COLLATE "pg_catalog"."default",
  "CD_CARGO" text COLLATE "pg_catalog"."default",
  "DS_CARGO" text COLLATE "pg_catalog"."default",
  "SQ_CANDIDATO" text COLLATE "pg_catalog"."default",
  "NR_CANDIDATO" text COLLATE "pg_catalog"."default",
  "NM_CANDIDATO" text COLLATE "pg_catalog"."default",
  "NM_URNA_CANDIDATO" text COLLATE "pg_catalog"."default",
  "NM_SOCIAL_CANDIDATO" text COLLATE "pg_catalog"."default",
  "NR_CPF_CANDIDATO" text COLLATE "pg_catalog"."default",
  "NM_EMAIL" text COLLATE "pg_catalog"."default",
  "CD_SITUACAO_CANDIDATURA" text COLLATE "pg_catalog"."default",
  "DS_SITUACAO_CANDIDATURA" text COLLATE "pg_catalog"."default",
  "CD_DETALHE_SITUACAO_CAND" text COLLATE "pg_catalog"."default",
  "DS_DETALHE_SITUACAO_CAND" text COLLATE "pg_catalog"."default",
  "TP_AGREMIACAO" text COLLATE "pg_catalog"."default",
  "NR_PARTIDO" text COLLATE "pg_catalog"."default",
  "SG_PARTIDO" text COLLATE "pg_catalog"."default",
  "NM_PARTIDO" text COLLATE "pg_catalog"."default",
  "SQ_COLIGACAO" text COLLATE "pg_catalog"."default",
  "NM_COLIGACAO" text COLLATE "pg_catalog"."default",
  "DS_COMPOSICAO_COLIGACAO" text COLLATE "pg_catalog"."default",
  "CD_NACIONALIDADE" text COLLATE "pg_catalog"."default",
  "DS_NACIONALIDADE" text COLLATE "pg_catalog"."default",
  "SG_UF_NASCIMENTO" text COLLATE "pg_catalog"."default",
  "CD_MUNICIPIO_NASCIMENTO" text COLLATE "pg_catalog"."default",
  "NM_MUNICIPIO_NASCIMENTO" text COLLATE "pg_catalog"."default",
  "DT_NASCIMENTO" text COLLATE "pg_catalog"."default",
  "NR_IDADE_DATA_POSSE" text COLLATE "pg_catalog"."default",
  "NR_TITULO_ELEITORAL_CANDIDATO" text COLLATE "pg_catalog"."default",
  "CD_GENERO" text COLLATE "pg_catalog"."default",
  "DS_GENERO" text COLLATE "pg_catalog"."default",
  "CD_GRAU_INSTRUCAO" text COLLATE "pg_catalog"."default",
  "DS_GRAU_INSTRUCAO" text COLLATE "pg_catalog"."default",
  "CD_ESTADO_CIVIL" text COLLATE "pg_catalog"."default",
  "DS_ESTADO_CIVIL" text COLLATE "pg_catalog"."default",
  "CD_COR_RACA" text COLLATE "pg_catalog"."default",
  "DS_COR_RACA" text COLLATE "pg_catalog"."default",
  "CD_OCUPACAO" text COLLATE "pg_catalog"."default",
  "DS_OCUPACAO" text COLLATE "pg_catalog"."default",
  "VR_DESPESA_MAX_CAMPANHA" text COLLATE "pg_catalog"."default",
  "CD_SIT_TOT_TURNO" text COLLATE "pg_catalog"."default",
  "DS_SIT_TOT_TURNO" text COLLATE "pg_catalog"."default",
  "ST_REELEICAO" text COLLATE "pg_catalog"."default",
  "ST_DECLARAR_BENS" text COLLATE "pg_catalog"."default",
  "NR_PROTOCOLO_CANDIDATURA" text COLLATE "pg_catalog"."default",
  "NR_PROCESSO" text COLLATE "pg_catalog"."default",
  "CD_SITUACAO_CANDIDATO_PLEITO" text COLLATE "pg_catalog"."default",
  "DS_SITUACAO_CANDIDATO_PLEITO" text COLLATE "pg_catalog"."default",
  "CD_SITUACAO_CANDIDATO_URNA" text COLLATE "pg_catalog"."default",
  "DS_SITUACAO_CANDIDATO_URNA" text COLLATE "pg_catalog"."default",
  "ST_CANDIDATO_INSERIDO_URNA" text COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for tse_despesa_contratada
-- ----------------------------
DROP TABLE IF EXISTS "temp"."tse_despesa_contratada";
CREATE TABLE "temp"."tse_despesa_contratada" (
  "DT_GERACAO" text COLLATE "pg_catalog"."default",
  "HH_GERACAO" text COLLATE "pg_catalog"."default",
  "ANO_ELEICAO" text COLLATE "pg_catalog"."default",
  "CD_TIPO_ELEICAO" text COLLATE "pg_catalog"."default",
  "NM_TIPO_ELEICAO" text COLLATE "pg_catalog"."default",
  "CD_ELEICAO" text COLLATE "pg_catalog"."default",
  "DS_ELEICAO" text COLLATE "pg_catalog"."default",
  "DT_ELEICAO" text COLLATE "pg_catalog"."default",
  "ST_TURNO" text COLLATE "pg_catalog"."default",
  "TP_PRESTACAO_CONTAS" text COLLATE "pg_catalog"."default",
  "DT_PRESTACAO_CONTAS" text COLLATE "pg_catalog"."default",
  "SQ_PRESTADOR_CONTAS" text COLLATE "pg_catalog"."default",
  "SG_UF" text COLLATE "pg_catalog"."default",
  "SG_UE" text COLLATE "pg_catalog"."default",
  "NM_UE" text COLLATE "pg_catalog"."default",
  "NR_CNPJ_PRESTADOR_CONTA" text COLLATE "pg_catalog"."default",
  "CD_CARGO" text COLLATE "pg_catalog"."default",
  "DS_CARGO" text COLLATE "pg_catalog"."default",
  "SQ_CANDIDATO" text COLLATE "pg_catalog"."default",
  "NR_CANDIDATO" text COLLATE "pg_catalog"."default",
  "NM_CANDIDATO" text COLLATE "pg_catalog"."default",
  "NR_CPF_CANDIDATO" text COLLATE "pg_catalog"."default",
  "NR_CPF_VICE_CANDIDATO" text COLLATE "pg_catalog"."default",
  "NR_PARTIDO" text COLLATE "pg_catalog"."default",
  "SG_PARTIDO" text COLLATE "pg_catalog"."default",
  "NM_PARTIDO" text COLLATE "pg_catalog"."default",
  "CD_TIPO_FORNECEDOR" text COLLATE "pg_catalog"."default",
  "DS_TIPO_FORNECEDOR" text COLLATE "pg_catalog"."default",
  "CD_CNAE_FORNECEDOR" text COLLATE "pg_catalog"."default",
  "DS_CNAE_FORNECEDOR" text COLLATE "pg_catalog"."default",
  "NR_CPF_CNPJ_FORNECEDOR" text COLLATE "pg_catalog"."default",
  "NM_FORNECEDOR" text COLLATE "pg_catalog"."default",
  "NM_FORNECEDOR_RFB" text COLLATE "pg_catalog"."default",
  "CD_ESFERA_PART_FORNECEDOR" text COLLATE "pg_catalog"."default",
  "DS_ESFERA_PART_FORNECEDOR" text COLLATE "pg_catalog"."default",
  "SG_UF_FORNECEDOR" text COLLATE "pg_catalog"."default",
  "CD_MUNICIPIO_FORNECEDOR" text COLLATE "pg_catalog"."default",
  "NM_MUNICIPIO_FORNECEDOR" text COLLATE "pg_catalog"."default",
  "SQ_CANDIDATO_FORNECEDOR" text COLLATE "pg_catalog"."default",
  "NR_CANDIDATO_FORNECEDOR" text COLLATE "pg_catalog"."default",
  "CD_CARGO_FORNECEDOR" text COLLATE "pg_catalog"."default",
  "DS_CARGO_FORNECEDOR" text COLLATE "pg_catalog"."default",
  "NR_PARTIDO_FORNECEDOR" text COLLATE "pg_catalog"."default",
  "SG_PARTIDO_FORNECEDOR" text COLLATE "pg_catalog"."default",
  "NM_PARTIDO_FORNECEDOR" text COLLATE "pg_catalog"."default",
  "DS_TIPO_DOCUMENTO" text COLLATE "pg_catalog"."default",
  "NR_DOCUMENTO" text COLLATE "pg_catalog"."default",
  "CD_ORIGEM_DESPESA" text COLLATE "pg_catalog"."default",
  "DS_ORIGEM_DESPESA" text COLLATE "pg_catalog"."default",
  "SQ_DESPESA" text COLLATE "pg_catalog"."default",
  "DT_DESPESA" text COLLATE "pg_catalog"."default",
  "DS_DESPESA" text COLLATE "pg_catalog"."default",
  "VR_DESPESA_CONTRATADA" text COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for tse_despesa_paga
-- ----------------------------
DROP TABLE IF EXISTS "temp"."tse_despesa_paga";
CREATE TABLE "temp"."tse_despesa_paga" (
  "DT_GERACAO" text COLLATE "pg_catalog"."default",
  "HH_GERACAO" text COLLATE "pg_catalog"."default",
  "ANO_ELEICAO" text COLLATE "pg_catalog"."default",
  "CD_TIPO_ELEICAO" text COLLATE "pg_catalog"."default",
  "NM_TIPO_ELEICAO" text COLLATE "pg_catalog"."default",
  "CD_ELEICAO" text COLLATE "pg_catalog"."default",
  "DS_ELEICAO" text COLLATE "pg_catalog"."default",
  "DT_ELEICAO" text COLLATE "pg_catalog"."default",
  "ST_TURNO" text COLLATE "pg_catalog"."default",
  "TP_PRESTACAO_CONTAS" text COLLATE "pg_catalog"."default",
  "DT_PRESTACAO_CONTAS" text COLLATE "pg_catalog"."default",
  "SQ_PRESTADOR_CONTAS" text COLLATE "pg_catalog"."default",
  "SG_UF" text COLLATE "pg_catalog"."default",
  "DS_TIPO_DOCUMENTO" text COLLATE "pg_catalog"."default",
  "NR_DOCUMENTO" text COLLATE "pg_catalog"."default",
  "CD_FONTE_DESPESA" text COLLATE "pg_catalog"."default",
  "DS_FONTE_DESPESA" text COLLATE "pg_catalog"."default",
  "CD_ORIGEM_DESPESA" text COLLATE "pg_catalog"."default",
  "DS_ORIGEM_DESPESA" text COLLATE "pg_catalog"."default",
  "CD_NATUREZA_DESPESA" text COLLATE "pg_catalog"."default",
  "DS_NATUREZA_DESPESA" text COLLATE "pg_catalog"."default",
  "CD_ESPECIE_RECURSO" text COLLATE "pg_catalog"."default",
  "DS_ESPECIE_RECURSO" text COLLATE "pg_catalog"."default",
  "SQ_DESPESA" text COLLATE "pg_catalog"."default",
  "SQ_PARCELAMENTO_DESPESA" text COLLATE "pg_catalog"."default",
  "DT_PAGTO_DESPESA" text COLLATE "pg_catalog"."default",
  "DS_DESPESA" text COLLATE "pg_catalog"."default",
  "VR_PAGTO_DESPESA" text COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for tse_receita
-- ----------------------------
DROP TABLE IF EXISTS "temp"."tse_receita";
CREATE TABLE "temp"."tse_receita" (
  "DT_GERACAO" text COLLATE "pg_catalog"."default",
  "HH_GERACAO" text COLLATE "pg_catalog"."default",
  "ANO_ELEICAO" text COLLATE "pg_catalog"."default",
  "CD_TIPO_ELEICAO" text COLLATE "pg_catalog"."default",
  "NM_TIPO_ELEICAO" text COLLATE "pg_catalog"."default",
  "CD_ELEICAO" text COLLATE "pg_catalog"."default",
  "DS_ELEICAO" text COLLATE "pg_catalog"."default",
  "DT_ELEICAO" text COLLATE "pg_catalog"."default",
  "ST_TURNO" text COLLATE "pg_catalog"."default",
  "TP_PRESTACAO_CONTAS" text COLLATE "pg_catalog"."default",
  "DT_PRESTACAO_CONTAS" text COLLATE "pg_catalog"."default",
  "SQ_PRESTADOR_CONTAS" text COLLATE "pg_catalog"."default",
  "SG_UF" text COLLATE "pg_catalog"."default",
  "SG_UE" text COLLATE "pg_catalog"."default",
  "NM_UE" text COLLATE "pg_catalog"."default",
  "NR_CNPJ_PRESTADOR_CONTA" text COLLATE "pg_catalog"."default",
  "CD_CARGO" text COLLATE "pg_catalog"."default",
  "DS_CARGO" text COLLATE "pg_catalog"."default",
  "SQ_CANDIDATO" text COLLATE "pg_catalog"."default",
  "NR_CANDIDATO" text COLLATE "pg_catalog"."default",
  "NM_CANDIDATO" text COLLATE "pg_catalog"."default",
  "NR_CPF_CANDIDATO" text COLLATE "pg_catalog"."default",
  "NR_CPF_VICE_CANDIDATO" text COLLATE "pg_catalog"."default",
  "NR_PARTIDO" text COLLATE "pg_catalog"."default",
  "SG_PARTIDO" text COLLATE "pg_catalog"."default",
  "NM_PARTIDO" text COLLATE "pg_catalog"."default",
  "CD_FONTE_RECEITA" text COLLATE "pg_catalog"."default",
  "DS_FONTE_RECEITA" text COLLATE "pg_catalog"."default",
  "CD_ORIGEM_RECEITA" text COLLATE "pg_catalog"."default",
  "DS_ORIGEM_RECEITA" text COLLATE "pg_catalog"."default",
  "CD_NATUREZA_RECEITA" text COLLATE "pg_catalog"."default",
  "DS_NATUREZA_RECEITA" text COLLATE "pg_catalog"."default",
  "CD_ESPECIE_RECEITA" text COLLATE "pg_catalog"."default",
  "DS_ESPECIE_RECEITA" text COLLATE "pg_catalog"."default",
  "CD_CNAE_DOADOR" text COLLATE "pg_catalog"."default",
  "DS_CNAE_DOADOR" text COLLATE "pg_catalog"."default",
  "NR_CPF_CNPJ_DOADOR" text COLLATE "pg_catalog"."default",
  "NM_DOADOR" text COLLATE "pg_catalog"."default",
  "NM_DOADOR_RFB" text COLLATE "pg_catalog"."default",
  "CD_ESFERA_PARTIDARIA_DOADOR" text COLLATE "pg_catalog"."default",
  "DS_ESFERA_PARTIDARIA_DOADOR" text COLLATE "pg_catalog"."default",
  "SG_UF_DOADOR" text COLLATE "pg_catalog"."default",
  "CD_MUNICIPIO_DOADOR" text COLLATE "pg_catalog"."default",
  "NM_MUNICIPIO_DOADOR" text COLLATE "pg_catalog"."default",
  "SQ_CANDIDATO_DOADOR" text COLLATE "pg_catalog"."default",
  "NR_CANDIDATO_DOADOR" text COLLATE "pg_catalog"."default",
  "CD_CARGO_CANDIDATO_DOADOR" text COLLATE "pg_catalog"."default",
  "DS_CARGO_CANDIDATO_DOADOR" text COLLATE "pg_catalog"."default",
  "NR_PARTIDO_DOADOR" text COLLATE "pg_catalog"."default",
  "SG_PARTIDO_DOADOR" text COLLATE "pg_catalog"."default",
  "NM_PARTIDO_DOADOR" text COLLATE "pg_catalog"."default",
  "NR_RECIBO_DOACAO" text COLLATE "pg_catalog"."default",
  "NR_DOCUMENTO_DOACAO" text COLLATE "pg_catalog"."default",
  "SQ_RECEITA" text COLLATE "pg_catalog"."default",
  "DT_RECEITA" text COLLATE "pg_catalog"."default",
  "DS_RECEITA" text COLLATE "pg_catalog"."default",
  "VR_RECEITA" text COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Table structure for tse_receita_doador_originario
-- ----------------------------
DROP TABLE IF EXISTS "temp"."tse_receita_doador_originario";
CREATE TABLE "temp"."tse_receita_doador_originario" (
  "DT_GERACAO" text COLLATE "pg_catalog"."default",
  "HH_GERACAO" text COLLATE "pg_catalog"."default",
  "ANO_ELEICAO" text COLLATE "pg_catalog"."default",
  "CD_TIPO_ELEICAO" text COLLATE "pg_catalog"."default",
  "NM_TIPO_ELEICAO" text COLLATE "pg_catalog"."default",
  "CD_ELEICAO" text COLLATE "pg_catalog"."default",
  "DS_ELEICAO" text COLLATE "pg_catalog"."default",
  "DT_ELEICAO" text COLLATE "pg_catalog"."default",
  "ST_TURNO" text COLLATE "pg_catalog"."default",
  "TP_PRESTACAO_CONTAS" text COLLATE "pg_catalog"."default",
  "DT_PRESTACAO_CONTAS" text COLLATE "pg_catalog"."default",
  "SQ_PRESTADOR_CONTAS" text COLLATE "pg_catalog"."default",
  "SG_UF" text COLLATE "pg_catalog"."default",
  "NR_CPF_CNPJ_DOADOR_ORIGINARIO" text COLLATE "pg_catalog"."default",
  "NM_DOADOR_ORIGINARIO" text COLLATE "pg_catalog"."default",
  "NM_DOADOR_ORIGINARIO_RFB" text COLLATE "pg_catalog"."default",
  "TP_DOADOR_ORIGINARIO" text COLLATE "pg_catalog"."default",
  "CD_CNAE_DOADOR_ORIGINARIO" text COLLATE "pg_catalog"."default",
  "DS_CNAE_DOADOR_ORIGINARIO" text COLLATE "pg_catalog"."default",
  "SQ_RECEITA" text COLLATE "pg_catalog"."default",
  "DT_RECEITA" text COLLATE "pg_catalog"."default",
  "DS_RECEITA" text COLLATE "pg_catalog"."default",
  "VR_RECEITA" text COLLATE "pg_catalog"."default"
)
;

-- ----------------------------
-- Primary Key structure for table arquivo_cheksum
-- ----------------------------
ALTER TABLE "temp"."arquivo_cheksum" ADD CONSTRAINT "arquivo_cheksum_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table cf_despesa_temp
-- ----------------------------
ALTER TABLE "temp"."cf_despesa_temp" ADD CONSTRAINT "cf_despesa_temp_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table cf_funcionario_temp
-- ----------------------------
ALTER TABLE "temp"."cf_funcionario_temp" ADD CONSTRAINT "cf_funcionario_temp_pkey" PRIMARY KEY ("chave");

-- ----------------------------
-- Indexes structure for table cf_secretario_remuneracao_temp
-- ----------------------------
CREATE INDEX "id_cf_secretario" ON "temp"."cf_secretario_remuneracao_temp" USING btree (
  "id_cf_secretario" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
CREATE UNIQUE INDEX "unique" ON "temp"."cf_secretario_remuneracao_temp" USING btree (
  "id_cf_secretario" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST,
  "referencia" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST,
  "descricao" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);

-- ----------------------------
-- Indexes structure for table cl_deputado_de_para
-- ----------------------------
CREATE INDEX "id" ON "temp"."cl_deputado_de_para" USING btree (
  "id" "pg_catalog"."int8_ops" ASC NULLS LAST
);
CREATE UNIQUE INDEX "id_nome_sigla_estado" ON "temp"."cl_deputado_de_para" USING btree (
  "id" "pg_catalog"."int8_ops" ASC NULLS LAST,
  "nome" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST,
  "id_estado" "pg_catalog"."int2_ops" ASC NULLS LAST
);

-- ----------------------------
-- Indexes structure for table cl_despesa
-- ----------------------------
CREATE UNIQUE INDEX "id_cl_deputado_ano_mes_hash" ON "temp"."cl_despesa" USING btree (
  "id_cl_deputado" "pg_catalog"."int8_ops" ASC NULLS LAST,
  "ano_mes" "pg_catalog"."int4_ops" ASC NULLS LAST,
  "hash" "pg_catalog"."bytea_ops" ASC NULLS LAST
);

-- ----------------------------
-- Primary Key structure for table cl_despesa
-- ----------------------------
ALTER TABLE "temp"."cl_despesa" ADD CONSTRAINT "cl_despesa_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table cl_despesa_especificacao
-- ----------------------------
ALTER TABLE "temp"."cl_despesa_especificacao" ADD CONSTRAINT "cl_despesa_especificacao_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table cl_despesa_temp
-- ----------------------------
ALTER TABLE "temp"."cl_despesa_temp" ADD CONSTRAINT "cl_despesa_temp_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table cl_empenho_temp
-- ----------------------------
ALTER TABLE "temp"."cl_empenho_temp" ADD CONSTRAINT "cl_empenho_temp_pkey" PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table fornecedor_cnpj_incorreto
-- ----------------------------
ALTER TABLE "temp"."fornecedor_cnpj_incorreto" ADD CONSTRAINT "fornecedor_cnpj_incorreto_pkey" PRIMARY KEY ("cnpj_incorreto");

-- ----------------------------
-- Indexes structure for table fornecedor_correcao
-- ----------------------------
CREATE UNIQUE INDEX "cnpj_cpf_nome" ON "temp"."fornecedor_correcao" USING btree (
  "cnpj_cpf" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST,
  "nome" COLLATE "pg_catalog"."default" "pg_catalog"."text_ops" ASC NULLS LAST
);
