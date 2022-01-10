-- MySQL dump 10.13  Distrib 8.0.26, for Linux (x86_64)
-- ------------------------------------------------------
-- Server version	8.0.26

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `cf_deputado`
--

DROP TABLE IF EXISTS `cf_deputado`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_deputado` (
  `id` mediumint unsigned NOT NULL COMMENT 'ideDeputado',
  `id_deputado` mediumint unsigned DEFAULT NULL COMMENT 'nuDeputadoId',
  `id_partido` tinyint unsigned DEFAULT NULL,
  `id_estado` tinyint unsigned DEFAULT NULL,
  `id_cf_gabinete` smallint unsigned DEFAULT NULL COMMENT 'Usado para importação dos secretarios parlamentares',
  `cpf` varchar(15) DEFAULT NULL,
  `nome_parlamentar` varchar(100) DEFAULT NULL,
  `nome_civil` varchar(100) DEFAULT NULL,
  `nome_importacao_presenca` varchar(100) DEFAULT NULL,
  `sexo` varchar(2) DEFAULT NULL,
  `email` varchar(100) DEFAULT NULL,
  `nascimento` date DEFAULT NULL,
  `falecimento` date DEFAULT NULL,
  `id_estado_nascimento` tinyint unsigned DEFAULT NULL,
  `municipio` varchar(500) DEFAULT NULL,
  `website` varchar(255) DEFAULT NULL,
  `profissao` varchar(255) DEFAULT NULL,
  `escolaridade` varchar(100) DEFAULT NULL,
  `condicao` varchar(50) DEFAULT NULL,
  `situacao` varchar(20) DEFAULT NULL,
  `passaporte_diplomatico` bit(1) NOT NULL DEFAULT b'0',
  `processado` bit(1) NOT NULL DEFAULT b'0',
  `valor_total_ceap` decimal(16,2) unsigned DEFAULT NULL COMMENT 'Valor acumulado gasto com a cota parlamentar em todas as legislaturas',
  `quantidade_secretarios` tinyint unsigned DEFAULT NULL COMMENT 'Quantidade de secretarios',
  `custo_secretarios` decimal(16,2) DEFAULT NULL,
  `custo_total_secretarios` decimal(16,2) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `id_deputado` (`id_deputado`),
  KEY `id_partido` (`id_partido`),
  KEY `id_estado` (`id_estado`),
  KEY `id_cf_gabinete` (`id_cf_gabinete`),
  KEY `id_estado_nascimento` (`id_estado_nascimento`),
  KEY `quantidade_secretarios` (`quantidade_secretarios`),
  KEY `nome_parlamentar` (`nome_parlamentar`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_deputado_auxilio_moradia`
--

DROP TABLE IF EXISTS `cf_deputado_auxilio_moradia`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_deputado_auxilio_moradia` (
  `id_cf_deputado` mediumint unsigned NOT NULL,
  `ano` smallint unsigned DEFAULT NULL,
  `mes` smallint unsigned DEFAULT NULL,
  `valor` decimal(10,2) unsigned DEFAULT NULL,
  UNIQUE KEY `id_cf_deputado` (`id_cf_deputado`,`ano`,`mes`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_deputado_campeao_gasto`
--

DROP TABLE IF EXISTS `cf_deputado_campeao_gasto`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_deputado_campeao_gasto` (
  `id_cf_deputado` mediumint unsigned NOT NULL,
  `nome_parlamentar` varchar(100) DEFAULT NULL,
  `valor_total` decimal(10,2) unsigned DEFAULT NULL,
  `sigla_partido` varchar(20) DEFAULT NULL,
  `sigla_estado` varchar(2) DEFAULT NULL,
  PRIMARY KEY (`id_cf_deputado`),
  KEY `nome_parlamentar` (`nome_parlamentar`),
  CONSTRAINT `FK_cf_deputado_campeao_gasto_cf_deputado` FOREIGN KEY (`id_cf_deputado`) REFERENCES `cf_deputado` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_deputado_gabinete`
--

DROP TABLE IF EXISTS `cf_deputado_gabinete`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_deputado_gabinete` (
  `id` int NOT NULL,
  `nome` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_deputado_gabinete_55`
--

DROP TABLE IF EXISTS `cf_deputado_gabinete_55`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_deputado_gabinete_55` (
  `id` int NOT NULL,
  `nome` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_deputado_imovel_funcional`
--

DROP TABLE IF EXISTS `cf_deputado_imovel_funcional`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_deputado_imovel_funcional` (
  `id_cf_deputado` mediumint NOT NULL,
  `uso_de` date NOT NULL,
  `uso_ate` date DEFAULT NULL,
  `total_dias` smallint DEFAULT NULL,
  UNIQUE KEY `id_cf_deputado` (`id_cf_deputado`,`uso_de`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_deputado_missao_oficial`
--

DROP TABLE IF EXISTS `cf_deputado_missao_oficial`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_deputado_missao_oficial` (
  `id_cf_deputado` mediumint unsigned NOT NULL,
  `periodo` varchar(50) NOT NULL,
  `assunto` varchar(4000) NOT NULL,
  `destino` varchar(255) DEFAULT NULL,
  `passagens` decimal(10,2) DEFAULT NULL,
  `diarias` decimal(10,2) DEFAULT NULL,
  `relatorio` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_deputado_temp`
--

DROP TABLE IF EXISTS `cf_deputado_temp`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_deputado_temp` (
  `ideCadastro` bigint DEFAULT NULL,
  `codOrcamento` varchar(10) DEFAULT NULL,
  `condicao` varchar(50) DEFAULT NULL,
  `matricula` int DEFAULT NULL,
  `idParlamentar` int DEFAULT NULL,
  `nome` varchar(255) DEFAULT NULL,
  `nomeParlamentar` varchar(100) DEFAULT NULL,
  `urlFoto` varchar(255) DEFAULT NULL,
  `sexo` varchar(10) DEFAULT NULL,
  `uf` varchar(2) DEFAULT NULL,
  `partido` varchar(50) DEFAULT NULL,
  `gabinete` varchar(20) DEFAULT NULL,
  `anexo` varchar(50) DEFAULT NULL,
  `fone` varchar(100) DEFAULT NULL,
  `email` varchar(100) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_deputado_temp_detalhes`
--

DROP TABLE IF EXISTS `cf_deputado_temp_detalhes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_deputado_temp_detalhes` (
  `numLegislatura` int DEFAULT NULL,
  `ideCadastro` bigint DEFAULT NULL,
  `idParlamentarDeprecated` int DEFAULT NULL,
  `nomeCivil` varchar(255) DEFAULT NULL,
  `nomeParlamentarAtual` varchar(100) DEFAULT NULL,
  `sexo` varchar(10) DEFAULT NULL,
  `ufRepresentacaoAtual` varchar(2) DEFAULT NULL,
  `sigla` varchar(50) DEFAULT NULL,
  `numero` varchar(20) DEFAULT NULL,
  `anexo` varchar(50) DEFAULT NULL,
  `telefone` varchar(100) DEFAULT NULL,
  `email` varchar(100) DEFAULT NULL,
  `nomeProfissao` varchar(255) DEFAULT NULL,
  `dataNascimento` date DEFAULT NULL,
  `dataFalecimento` date DEFAULT NULL,
  `situacaoNaLegislaturaAtual` varchar(30) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_despesa`
--

DROP TABLE IF EXISTS `cf_despesa`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_despesa` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `id_documento` bigint unsigned DEFAULT NULL,
  `id_cf_deputado` mediumint unsigned NOT NULL,
  `id_cf_mandato` smallint unsigned DEFAULT NULL,
  `id_cf_despesa_tipo` smallint unsigned NOT NULL,
  `id_cf_especificacao` tinyint unsigned DEFAULT NULL,
  `id_fornecedor` mediumint unsigned DEFAULT NULL,
  `nome_passageiro` varchar(100) DEFAULT NULL,
  `numero_documento` varchar(100) DEFAULT NULL,
  `tipo_documento` int unsigned NOT NULL,
  `data_emissao` date DEFAULT NULL,
  `valor_documento` decimal(10,2) DEFAULT NULL,
  `valor_glosa` decimal(10,2) NOT NULL,
  `valor_liquido` decimal(10,2) NOT NULL,
  `valor_restituicao` decimal(10,2) DEFAULT NULL,
  `mes` smallint unsigned NOT NULL,
  `ano` smallint unsigned NOT NULL,
  `parcela` int unsigned DEFAULT NULL,
  `trecho_viagem` varchar(100) DEFAULT NULL,
  `lote` mediumint unsigned DEFAULT NULL,
  `ressarcimento` smallint unsigned DEFAULT NULL,
  `ano_mes` mediumint unsigned NOT NULL,
  `importacao` date DEFAULT NULL,
  `hash` char(40) NOT NULL,
  `link` tinyint unsigned DEFAULT '0',
  `url_documento` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`,`ano`),
  UNIQUE KEY `ano_hash` (`ano`,`hash`),
  KEY `idx_ano_mes` (`ano_mes`),
  KEY `idx_id_fornecedor` (`id_fornecedor`),
  KEY `idx_id_cf_deputado` (`id_cf_deputado`),
  KEY `id_cf_mandato` (`id_cf_mandato`),
  KEY `id_cf_despesa_tipo` (`id_cf_despesa_tipo`),
  KEY `id_cf_especificacao` (`id_cf_especificacao`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
/*!50100 PARTITION BY RANGE (`ano`)
(PARTITION a2008 VALUES LESS THAN (2008) ENGINE = InnoDB,
 PARTITION a2009 VALUES LESS THAN (2009) ENGINE = InnoDB,
 PARTITION a2010 VALUES LESS THAN (2010) ENGINE = InnoDB,
 PARTITION a2011 VALUES LESS THAN (2011) ENGINE = InnoDB,
 PARTITION a2012 VALUES LESS THAN (2012) ENGINE = InnoDB,
 PARTITION a2013 VALUES LESS THAN (2013) ENGINE = InnoDB,
 PARTITION a2014 VALUES LESS THAN (2014) ENGINE = InnoDB,
 PARTITION a2015 VALUES LESS THAN (2015) ENGINE = InnoDB,
 PARTITION a2016 VALUES LESS THAN (2016) ENGINE = InnoDB,
 PARTITION a2017 VALUES LESS THAN (2017) ENGINE = InnoDB,
 PARTITION a2018 VALUES LESS THAN (2018) ENGINE = InnoDB,
 PARTITION a2019 VALUES LESS THAN (2019) ENGINE = InnoDB,
 PARTITION a2020 VALUES LESS THAN MAXVALUE ENGINE = InnoDB) */;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_despesa_resumo_mensal`
--

DROP TABLE IF EXISTS `cf_despesa_resumo_mensal`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_despesa_resumo_mensal` (
  `ano` int unsigned NOT NULL,
  `mes` int unsigned NOT NULL,
  `valor` decimal(10,2) unsigned DEFAULT NULL,
  PRIMARY KEY (`ano`,`mes`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_despesa_temp`
--

DROP TABLE IF EXISTS `cf_despesa_temp`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_despesa_temp` (
  `idDeputado` bigint DEFAULT NULL,
  `nomeParlamentar` varchar(100) DEFAULT NULL,
  `numeroCarteiraParlamentar` int DEFAULT NULL,
  `legislatura` int DEFAULT NULL,
  `siglaUF` varchar(2) DEFAULT NULL,
  `siglaPartido` varchar(10) DEFAULT NULL,
  `codigoLegislatura` int DEFAULT NULL,
  `numeroSubCota` int DEFAULT NULL,
  `descricao` varchar(100) DEFAULT NULL,
  `numeroEspecificacaoSubCota` int DEFAULT NULL,
  `descricaoEspecificacao` varchar(100) DEFAULT NULL,
  `fornecedor` varchar(255) DEFAULT NULL,
  `cnpjCPF` varchar(14) DEFAULT NULL,
  `numero` varchar(50) DEFAULT NULL,
  `tipoDocumento` varchar(10) DEFAULT NULL,
  `dataEmissao` date DEFAULT NULL,
  `valorDocumento` decimal(10,2) DEFAULT NULL,
  `valorGlosa` decimal(10,2) DEFAULT NULL,
  `valorLiquido` decimal(10,2) DEFAULT NULL,
  `mes` decimal(2,0) DEFAULT NULL,
  `ano` decimal(4,0) DEFAULT NULL,
  `parcela` decimal(3,0) DEFAULT NULL,
  `passageiro` varchar(100) DEFAULT NULL,
  `trecho` varchar(100) DEFAULT NULL,
  `lote` int DEFAULT NULL,
  `ressarcimento` int DEFAULT NULL,
  `idDocumento` varchar(20) DEFAULT NULL,
  `restituicao` decimal(10,2) DEFAULT NULL,
  `numeroDeputadoID` int DEFAULT NULL,
  `hash` char(40) NOT NULL,
  `cpf` varchar(15) DEFAULT NULL,
  `urlDocumento` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_despesa_tipo`
--

DROP TABLE IF EXISTS `cf_despesa_tipo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_despesa_tipo` (
  `id` smallint unsigned NOT NULL,
  `descricao` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `descricao` (`descricao`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_especificacao_tipo`
--

DROP TABLE IF EXISTS `cf_especificacao_tipo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_especificacao_tipo` (
  `id_cf_despesa_tipo` smallint unsigned NOT NULL,
  `id_cf_especificacao` tinyint unsigned NOT NULL,
  `descricao` varchar(100) NOT NULL,
  PRIMARY KEY (`id_cf_despesa_tipo`,`id_cf_especificacao`),
  KEY `descricao` (`descricao`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_funcionario`
--

DROP TABLE IF EXISTS `cf_funcionario`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_funcionario` (
  `id` mediumint unsigned NOT NULL AUTO_INCREMENT,
  `chave` varchar(45) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `nome` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `processado` tinyint NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `chave` (`chave`),
  KEY `idx_nome` (`nome`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_funcionario_area_atuacao`
--

DROP TABLE IF EXISTS `cf_funcionario_area_atuacao`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_funcionario_area_atuacao` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `nome` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `nome` (`nome`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_funcionario_cargo`
--

DROP TABLE IF EXISTS `cf_funcionario_cargo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_funcionario_cargo` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `nome` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `nome` (`nome`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_funcionario_contratacao`
--

DROP TABLE IF EXISTS `cf_funcionario_contratacao`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_funcionario_contratacao` (
  `id` mediumint unsigned NOT NULL AUTO_INCREMENT,
  `id_cf_deputado` mediumint unsigned DEFAULT NULL,
  `id_cf_funcionario` mediumint unsigned NOT NULL,
  `id_cf_funcionario_grupo_funcional` tinyint unsigned DEFAULT NULL,
  `id_cf_funcionario_cargo` tinyint unsigned DEFAULT NULL,
  `id_cf_funcionario_nivel` tinyint unsigned DEFAULT NULL,
  `id_cf_funcionario_funcao_comissionada` smallint unsigned DEFAULT NULL,
  `id_cf_funcionario_area_atuacao` tinyint unsigned DEFAULT NULL,
  `id_cf_funcionario_local_trabalho` tinyint unsigned DEFAULT NULL,
  `id_cf_funcionario_situacao` tinyint unsigned DEFAULT NULL,
  `periodo_de` date DEFAULT NULL,
  `periodo_ate` date DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `cf_secretario_contratacao_unique` (`id_cf_funcionario`,`id_cf_deputado`,`periodo_de`) USING BTREE,
  KEY `FK_cf_funcionario_contratacao_cf_deputado` (`id_cf_deputado`),
  KEY `FK_cf_funcionario_contratacao_cf_funcionario_grupo_funcional` (`id_cf_funcionario_grupo_funcional`),
  KEY `FK_cf_funcionario_contratacao_cf_funcionario_cargo` (`id_cf_funcionario_cargo`),
  KEY `FK_cf_funcionario_contratacao_cf_funcionario_nivel` (`id_cf_funcionario_nivel`),
  KEY `FK_cf_funcionario_contratacao_cf_funcionario_area_atuacao` (`id_cf_funcionario_area_atuacao`),
  KEY `FK_cf_funcionario_contratacao_cf_funcionario_local_trabalho` (`id_cf_funcionario_local_trabalho`),
  KEY `FK_cf_funcionario_contratacao_cf_funcionario_situacao` (`id_cf_funcionario_situacao`),
  KEY `FK_cf_funcionario_contratacao_cf_funcionario_funcao_comissionada` (`id_cf_funcionario_funcao_comissionada`),
  CONSTRAINT `FK_cf_funcionario_contratacao_cf_deputado` FOREIGN KEY (`id_cf_deputado`) REFERENCES `cf_deputado` (`id`),
  CONSTRAINT `FK_cf_funcionario_contratacao_cf_funcionario` FOREIGN KEY (`id_cf_funcionario`) REFERENCES `cf_funcionario` (`id`),
  CONSTRAINT `FK_cf_funcionario_contratacao_cf_funcionario_area_atuacao` FOREIGN KEY (`id_cf_funcionario_area_atuacao`) REFERENCES `cf_funcionario_area_atuacao` (`id`),
  CONSTRAINT `FK_cf_funcionario_contratacao_cf_funcionario_cargo` FOREIGN KEY (`id_cf_funcionario_cargo`) REFERENCES `cf_funcionario_cargo` (`id`),
  CONSTRAINT `FK_cf_funcionario_contratacao_cf_funcionario_funcao_comissionada` FOREIGN KEY (`id_cf_funcionario_funcao_comissionada`) REFERENCES `cf_funcionario_funcao_comissionada` (`id`),
  CONSTRAINT `FK_cf_funcionario_contratacao_cf_funcionario_grupo_funcional` FOREIGN KEY (`id_cf_funcionario_grupo_funcional`) REFERENCES `cf_funcionario_grupo_funcional` (`id`),
  CONSTRAINT `FK_cf_funcionario_contratacao_cf_funcionario_local_trabalho` FOREIGN KEY (`id_cf_funcionario_local_trabalho`) REFERENCES `cf_funcionario_local_trabalho` (`id`),
  CONSTRAINT `FK_cf_funcionario_contratacao_cf_funcionario_nivel` FOREIGN KEY (`id_cf_funcionario_nivel`) REFERENCES `cf_funcionario_nivel` (`id`),
  CONSTRAINT `FK_cf_funcionario_contratacao_cf_funcionario_situacao` FOREIGN KEY (`id_cf_funcionario_situacao`) REFERENCES `cf_funcionario_situacao` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_funcionario_funcao_comissionada`
--

DROP TABLE IF EXISTS `cf_funcionario_funcao_comissionada`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_funcionario_funcao_comissionada` (
  `id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `nome` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `nome` (`nome`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_funcionario_grupo_funcional`
--

DROP TABLE IF EXISTS `cf_funcionario_grupo_funcional`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_funcionario_grupo_funcional` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `nome` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `nome` (`nome`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_funcionario_local_trabalho`
--

DROP TABLE IF EXISTS `cf_funcionario_local_trabalho`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_funcionario_local_trabalho` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `nome` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `nome` (`nome`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_funcionario_nivel`
--

DROP TABLE IF EXISTS `cf_funcionario_nivel`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_funcionario_nivel` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `nome` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `nome` (`nome`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_funcionario_remuneracao`
--

DROP TABLE IF EXISTS `cf_funcionario_remuneracao`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_funcionario_remuneracao` (
  `id` mediumint unsigned NOT NULL AUTO_INCREMENT,
  `id_cf_funcionario` mediumint unsigned NOT NULL,
  `id_cf_funcionario_contratacao` mediumint unsigned DEFAULT NULL,
  `id_cf_deputado` mediumint unsigned DEFAULT NULL,
  `referencia` date NOT NULL,
  `tipo` tinyint unsigned DEFAULT NULL,
  `remuneracao_fixa` decimal(10,2) DEFAULT NULL,
  `vantagens_natureza_pessoal` decimal(10,2) DEFAULT NULL,
  `funcao_ou_cargo_em_comissao` decimal(10,2) DEFAULT NULL,
  `gratificacao_natalina` decimal(10,2) DEFAULT NULL,
  `ferias` decimal(10,2) DEFAULT NULL,
  `outras_remuneracoes` decimal(10,2) DEFAULT NULL,
  `abono_permanencia` decimal(10,2) DEFAULT NULL,
  `valor_bruto` decimal(10,2) DEFAULT NULL,
  `redutor_constitucional` decimal(10,2) DEFAULT NULL,
  `contribuicao_previdenciaria` decimal(10,2) DEFAULT NULL,
  `imposto_renda` decimal(10,2) DEFAULT NULL,
  `valor_liquido` decimal(10,2) DEFAULT NULL,
  `valor_diarias` decimal(10,2) DEFAULT NULL,
  `valor_auxilios` decimal(10,2) DEFAULT NULL,
  `valor_vantagens` decimal(10,2) DEFAULT NULL,
  `valor_outros` decimal(10,2) DEFAULT NULL,
  `valor_total` decimal(10,2) DEFAULT NULL COMMENT 'valor_bruto + valor_outros',
  `nivel` varchar(5) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_cf_secretario` (`id_cf_funcionario`,`referencia`,`tipo`) USING BTREE,
  KEY `FK_cf_funcionario_remuneracao_cf_funcionario_contratacao` (`id_cf_funcionario_contratacao`),
  KEY `FK_cf_funcionario_remuneracao_cf_deputado` (`id_cf_deputado`),
  CONSTRAINT `FK_cf_funcionario_remuneracao_cf_deputado` FOREIGN KEY (`id_cf_deputado`) REFERENCES `cf_deputado` (`id`),
  CONSTRAINT `FK_cf_funcionario_remuneracao_cf_funcionario` FOREIGN KEY (`id_cf_funcionario`) REFERENCES `cf_funcionario` (`id`),
  CONSTRAINT `FK_cf_funcionario_remuneracao_cf_funcionario_contratacao` FOREIGN KEY (`id_cf_funcionario_contratacao`) REFERENCES `cf_funcionario_contratacao` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_funcionario_situacao`
--

DROP TABLE IF EXISTS `cf_funcionario_situacao`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_funcionario_situacao` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `nome` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `nome` (`nome`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_funcionario_temp`
--

DROP TABLE IF EXISTS `cf_funcionario_temp`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_funcionario_temp` (
  `chave` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT '''''',
  `nome` varchar(255) DEFAULT NULL,
  `categoria_funcional` varchar(255) DEFAULT NULL COMMENT 'Categoria funcional',
  `cargo` varchar(255) DEFAULT NULL COMMENT 'Cargo',
  `nivel` varchar(255) DEFAULT NULL COMMENT 'Nível',
  `funcao_comissionada` varchar(255) DEFAULT NULL COMMENT 'Função comissionada',
  `area_atuacao` varchar(255) DEFAULT NULL COMMENT 'Área de atuação',
  `local_trabalho` varchar(255) DEFAULT NULL COMMENT 'Local de trabalho',
  `situacao` varchar(255) DEFAULT NULL COMMENT 'Situação',
  `data_designacao_funcao` date DEFAULT NULL COMMENT 'Data da designação da função',
  PRIMARY KEY (`chave`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_funcionario_tipo_folha`
--

DROP TABLE IF EXISTS `cf_funcionario_tipo_folha`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_funcionario_tipo_folha` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `nome` varchar(50) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_gabinete`
--

DROP TABLE IF EXISTS `cf_gabinete`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_gabinete` (
  `id` smallint unsigned NOT NULL,
  `nome` varchar(50) DEFAULT NULL,
  `predio` varchar(50) DEFAULT NULL,
  `andar` tinyint unsigned DEFAULT NULL,
  `sala` varchar(50) DEFAULT NULL,
  `telefone` varchar(20) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `nome` (`nome`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_legislatura`
--

DROP TABLE IF EXISTS `cf_legislatura`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_legislatura` (
  `id` tinyint unsigned NOT NULL,
  `ano` smallint unsigned DEFAULT NULL,
  `inicio` mediumint unsigned DEFAULT NULL,
  `final` mediumint unsigned DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `ano` (`ano`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_mandato`
--

DROP TABLE IF EXISTS `cf_mandato`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_mandato` (
  `id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `id_cf_deputado` mediumint unsigned NOT NULL,
  `id_legislatura` tinyint unsigned DEFAULT NULL,
  `id_carteira_parlamantar` mediumint unsigned DEFAULT NULL,
  `id_estado` tinyint unsigned DEFAULT NULL,
  `id_partido` tinyint unsigned DEFAULT NULL,
  `condicao` varchar(10) DEFAULT NULL,
  `valor_total_ceap` decimal(26,2) unsigned DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_cf_deputado_id_legislatura` (`id_cf_deputado`,`id_legislatura`),
  KEY `id_cf_deputado` (`id_cf_deputado`),
  KEY `id_legislatura` (`id_legislatura`),
  KEY `id_estado` (`id_estado`),
  KEY `id_carteira_parlamantar` (`id_carteira_parlamantar`),
  KEY `id_partido` (`id_partido`),
  CONSTRAINT `FK_cf_mandato_cf_legislatura` FOREIGN KEY (`id_legislatura`) REFERENCES `cf_legislatura` (`id`),
  CONSTRAINT `FK_cf_mandato_estado` FOREIGN KEY (`id_estado`) REFERENCES `estado` (`id`),
  CONSTRAINT `FK_cf_mandato_partido` FOREIGN KEY (`id_partido`) REFERENCES `partido` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_mandato_temp`
--

DROP TABLE IF EXISTS `cf_mandato_temp`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_mandato_temp` (
  `ideCadastro` int DEFAULT NULL,
  `numLegislatura` int DEFAULT NULL,
  `nomeParlamentar` varchar(45) DEFAULT NULL,
  `Sexo` varchar(45) CHARACTER SET latin1 COLLATE latin1_swedish_ci DEFAULT NULL,
  `Profissao` varchar(255) DEFAULT NULL,
  `LegendaPartidoEleito` char(10) DEFAULT NULL,
  `UFEleito` char(2) DEFAULT NULL,
  `Condicao` varchar(45) DEFAULT NULL,
  `SituacaoMandato` varchar(45) CHARACTER SET latin1 COLLATE latin1_swedish_ci DEFAULT NULL,
  `Matricula` int DEFAULT NULL,
  `Gabinete` varchar(45) DEFAULT NULL,
  `Anexo` varchar(45) DEFAULT NULL,
  `Fone` varchar(45) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_remuneracao_temp`
--

DROP TABLE IF EXISTS `cf_remuneracao_temp`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_remuneracao_temp` (
  `id` int DEFAULT NULL,
  `ano_mes` int DEFAULT NULL,
  `cargo` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT 'Cargo Individualizado do Servidor',
  `grupo_funcional` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT 'Grupo Funcional',
  `tipo_folha` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT 'Folha de Pagamento',
  `admissao` int DEFAULT NULL COMMENT 'Ano Ingresso',
  `remun_basica` decimal(10,2) DEFAULT NULL COMMENT 'Remuneração Fixa',
  `vant_pessoais` decimal(10,2) DEFAULT NULL COMMENT 'Vantagens de Natureza Pessoal',
  `func_comissionada` decimal(10,2) DEFAULT NULL COMMENT 'Função ou Cargo em Comissão',
  `grat_natalina` decimal(10,2) DEFAULT NULL COMMENT 'Gratificação Natalina',
  `ferias` decimal(10,2) DEFAULT NULL COMMENT 'Férias (1/3 Constitucional)',
  `outras_eventuais` decimal(10,2) DEFAULT NULL COMMENT 'Outras Remunerações Eventuais/Provisórias(*)',
  `abono_permanencia` decimal(10,2) DEFAULT NULL COMMENT 'Abono de Permanência',
  `reversao_teto_const` decimal(10,2) DEFAULT NULL COMMENT 'Redutor Constitucional',
  `previdencia` decimal(10,2) DEFAULT NULL COMMENT 'Constribuição Previdenciária',
  `imposto_renda` decimal(10,2) DEFAULT NULL COMMENT 'Imposto de Renda',
  `rem_liquida` decimal(10,2) DEFAULT NULL COMMENT 'Remuneração Após Descontos Obrigatórios',
  `diarias` decimal(10,2) DEFAULT NULL COMMENT 'Diárias',
  `auxilios` decimal(10,2) DEFAULT NULL COMMENT 'Auxílios',
  `vant_indenizatorias` decimal(10,2) DEFAULT NULL COMMENT 'Vantagens Indenizatórias'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_secretario`
--

DROP TABLE IF EXISTS `cf_secretario`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_secretario` (
  `id` mediumint unsigned NOT NULL AUTO_INCREMENT,
  `id_cf_deputado` mediumint unsigned NOT NULL DEFAULT '0',
  `nome` varchar(100) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `periodo` varchar(100) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `cargo` varchar(45) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `valor_bruto` decimal(10,2) DEFAULT NULL,
  `valor_liquido` decimal(10,2) DEFAULT NULL,
  `valor_outros` decimal(10,2) DEFAULT NULL,
  `link` varchar(255) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `referencia` varchar(255) COLLATE utf8mb4_general_ci DEFAULT NULL,
  `em_exercicio` bit(1) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  KEY `idx_id_cf_deputado` (`id_cf_deputado`) USING BTREE,
  KEY `idx_nome` (`nome`) USING BTREE,
  KEY `idx_link` (`link`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_secretario_55`
--

DROP TABLE IF EXISTS `cf_secretario_55`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_secretario_55` (
  `id` mediumint unsigned NOT NULL,
  `id_cf_gabinete` smallint unsigned DEFAULT NULL,
  `id_cf_deputado` int unsigned DEFAULT NULL,
  `nome` varchar(200) NOT NULL,
  `orgao` varchar(200) NOT NULL,
  `data` varchar(200) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `id_cf_gabinete` (`id_cf_gabinete`),
  KEY `id_cf_deputado` (`id_cf_deputado`),
  KEY `nome` (`nome`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_secretario_56`
--

DROP TABLE IF EXISTS `cf_secretario_56`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_secretario_56` (
  `id` mediumint unsigned NOT NULL,
  `id_cf_gabinete` smallint unsigned DEFAULT NULL,
  `id_cf_deputado` int unsigned DEFAULT NULL,
  `nome` varchar(200) NOT NULL,
  `orgao` varchar(200) NOT NULL,
  `data` varchar(200) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `id_cf_gabinete` (`id_cf_gabinete`),
  KEY `id_cf_deputado` (`id_cf_deputado`),
  KEY `nome` (`nome`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_secretario_BKP`
--

DROP TABLE IF EXISTS `cf_secretario_BKP`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_secretario_BKP` (
  `id_cf_deputado` mediumint unsigned NOT NULL,
  `nome` varchar(100) DEFAULT NULL,
  `cargo` varchar(45) DEFAULT NULL,
  `periodo` varchar(255) DEFAULT NULL,
  `valor_bruto` decimal(10,2) DEFAULT NULL,
  `valor_liquido` decimal(10,2) DEFAULT NULL,
  `valor_outros` decimal(10,2) DEFAULT NULL,
  `link` varchar(255) DEFAULT NULL,
  `referencia` varchar(255) DEFAULT NULL,
  `em_exercicio` bit(1) DEFAULT NULL,
  KEY `idx_id_cf_deputado` (`id_cf_deputado`),
  KEY `idx_nome` (`nome`),
  KEY `idx_link` (`link`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_secretario_contratacao_old`
--

DROP TABLE IF EXISTS `cf_secretario_contratacao_old`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_secretario_contratacao_old` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `id_cf_deputado` int unsigned NOT NULL DEFAULT '0',
  `nome` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `cargo` varchar(45) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `funcao` varchar(45) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `link` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `periodo` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `periodo_de` date DEFAULT NULL,
  `periodo_ate` date DEFAULT NULL,
  `em_exercicio` bit(1) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `id_cf_deputado` (`id_cf_deputado`,`funcao`,`link`,`periodo_de`),
  KEY `idx_id_cf_deputado` (`id_cf_deputado`) USING BTREE,
  KEY `idx_nome` (`nome`) USING BTREE,
  KEY `idx_link` (`link`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_secretario_historico`
--

DROP TABLE IF EXISTS `cf_secretario_historico`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_secretario_historico` (
  `id_cf_deputado` mediumint unsigned NOT NULL,
  `nome` varchar(100) DEFAULT NULL,
  `cargo` varchar(45) DEFAULT NULL,
  `periodo` varchar(255) DEFAULT NULL,
  `valor_bruto` decimal(10,2) DEFAULT NULL,
  `valor_liquido` decimal(10,2) DEFAULT NULL,
  `valor_outros` decimal(10,2) DEFAULT NULL,
  `link` varchar(255) DEFAULT NULL,
  `referencia` varchar(255) DEFAULT NULL,
  `ano_mes` int unsigned DEFAULT NULL,
  KEY `idx_id_cf_deputado` (`id_cf_deputado`),
  KEY `idx_nome` (`nome`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_secretario_remuneracao_temp`
--

DROP TABLE IF EXISTS `cf_secretario_remuneracao_temp`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_secretario_remuneracao_temp` (
  `id_cf_secretario` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `referencia` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `descricao` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `remuneracao_fixa` decimal(10,2) DEFAULT NULL,
  `vantagens_natureza_pessoal` decimal(10,2) DEFAULT NULL,
  `funcao_ou_cargo_em_comissao` decimal(10,2) DEFAULT NULL,
  `gratificacao_natalina` decimal(10,2) DEFAULT NULL,
  `ferias` decimal(10,2) DEFAULT NULL,
  `outras_remuneracoes` decimal(10,2) DEFAULT NULL,
  `valor_bruto` decimal(10,2) DEFAULT NULL,
  `abono_permanencia` decimal(10,2) DEFAULT NULL,
  `redutor_constitucional` decimal(10,2) DEFAULT NULL,
  `contribuicao_previdenciaria` decimal(10,2) DEFAULT NULL,
  `imposto_renda` decimal(10,2) DEFAULT NULL,
  `valor_liquido` decimal(10,2) DEFAULT NULL,
  `valor_diarias` decimal(10,2) DEFAULT NULL,
  `valor_auxilios` decimal(10,2) DEFAULT NULL,
  `valor_vantagens` decimal(10,2) DEFAULT NULL,
  `valor_outros` decimal(10,2) DEFAULT NULL,
  UNIQUE KEY `unique` (`id_cf_secretario`,`referencia`,`descricao`),
  KEY `id_cf_secretario` (`id_cf_secretario`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_secretario_remuneracao_temp_1`
--

DROP TABLE IF EXISTS `cf_secretario_remuneracao_temp_1`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_secretario_remuneracao_temp_1` (
  `id_cf_secretario` varchar(255) NOT NULL,
  `referencia` varchar(255) DEFAULT NULL,
  `descricao` varchar(255) DEFAULT NULL,
  `remuneracao_fixa` decimal(10,2) DEFAULT NULL,
  `vantagens_natureza_pessoal` decimal(10,2) DEFAULT NULL,
  `funcao_ou_cargo_em_comissao` decimal(10,2) DEFAULT NULL,
  `gratificacao_natalina` decimal(10,2) DEFAULT NULL,
  `ferias` decimal(10,2) DEFAULT NULL,
  `outras_remuneracoes` decimal(10,2) DEFAULT NULL,
  `valor_bruto` decimal(10,2) DEFAULT NULL,
  `abono_permanencia` decimal(10,2) DEFAULT NULL,
  `redutor_constitucional` decimal(10,2) DEFAULT NULL,
  `contribuicao_previdenciaria` decimal(10,2) DEFAULT NULL,
  `imposto_renda` decimal(10,2) DEFAULT NULL,
  `valor_liquido` decimal(10,2) DEFAULT NULL,
  `valor_diarias` decimal(10,2) DEFAULT NULL,
  `valor_auxilios` decimal(10,2) DEFAULT NULL,
  `valor_vantagens` decimal(10,2) DEFAULT NULL,
  `valor_outros` decimal(10,2) DEFAULT NULL,
  KEY `id_cf_secretario` (`id_cf_secretario`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_secretario_temp_2020`
--

DROP TABLE IF EXISTS `cf_secretario_temp_2020`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_secretario_temp_2020` (
  `id_cf_deputado` mediumint unsigned NOT NULL,
  `nome` varchar(100) DEFAULT NULL,
  `cargo` varchar(45) DEFAULT NULL,
  `periodo` varchar(255) DEFAULT NULL,
  `valor_bruto` decimal(10,2) DEFAULT NULL,
  `valor_liquido` decimal(10,2) DEFAULT NULL,
  `valor_outros` decimal(10,2) DEFAULT NULL,
  `link` varchar(255) DEFAULT NULL,
  `referencia` varchar(255) DEFAULT NULL,
  `em_exercicio` bit(1) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_secretario_temp_2021`
--

DROP TABLE IF EXISTS `cf_secretario_temp_2021`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_secretario_temp_2021` (
  `id_cf_deputado` mediumint unsigned NOT NULL,
  `nome` varchar(100) DEFAULT NULL,
  `cargo` varchar(45) DEFAULT NULL,
  `periodo` varchar(255) DEFAULT NULL,
  `valor_bruto` decimal(10,2) DEFAULT NULL,
  `valor_liquido` decimal(10,2) DEFAULT NULL,
  `valor_outros` decimal(10,2) DEFAULT NULL,
  `link` varchar(255) DEFAULT NULL,
  `referencia` varchar(255) DEFAULT NULL,
  `em_exercicio` bit(1) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_sessao`
--

DROP TABLE IF EXISTS `cf_sessao`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_sessao` (
  `id` mediumint unsigned NOT NULL AUTO_INCREMENT,
  `id_legislatura` tinyint unsigned NOT NULL,
  `data` date NOT NULL,
  `inicio` datetime NOT NULL,
  `tipo` tinyint unsigned NOT NULL,
  `numero` varchar(45) DEFAULT NULL,
  `checksum` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `id_legislatura` (`id_legislatura`),
  KEY `data` (`data`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cf_sessao_presenca`
--

DROP TABLE IF EXISTS `cf_sessao_presenca`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cf_sessao_presenca` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `id_cf_sessao` mediumint unsigned NOT NULL,
  `id_cf_deputado` mediumint unsigned NOT NULL,
  `presente` char(1) NOT NULL,
  `justificativa` varchar(100) DEFAULT NULL,
  `presenca_externa` char(1) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `id_cf_sessao` (`id_cf_sessao`),
  KEY `id_cf_deputado` (`id_cf_deputado`),
  CONSTRAINT `FK_cf_sessao_presenca_cf_sessao` FOREIGN KEY (`id_cf_sessao`) REFERENCES `cf_sessao` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;
/*!40101 SET character_set_client = @saved_cs_client */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2022-01-09
