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
-- Table structure for table `sf_cargo`
--

DROP TABLE IF EXISTS `sf_cargo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_cargo` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `descricao` varchar(100) NOT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `descricao` (`descricao`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_categoria`
--

DROP TABLE IF EXISTS `sf_categoria`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_categoria` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `descricao` varchar(100) NOT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `descricao` (`descricao`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_despesa`
--

DROP TABLE IF EXISTS `sf_despesa`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_despesa` (
  `id` bigint unsigned NOT NULL,
  `id_sf_senador` int unsigned NOT NULL,
  `id_sf_despesa_tipo` int unsigned DEFAULT NULL,
  `id_fornecedor` int unsigned DEFAULT NULL,
  `ano_mes` decimal(6,0) unsigned DEFAULT NULL,
  `ano` smallint unsigned DEFAULT NULL,
  `mes` smallint unsigned DEFAULT NULL,
  `documento` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `data_documento` date DEFAULT NULL,
  `detalhamento` text CHARACTER SET utf8 COLLATE utf8_general_ci,
  `valor` decimal(10,2) DEFAULT NULL,
  `hash` char(40) DEFAULT NULL,
  `deletado` date DEFAULT NULL,
  PRIMARY KEY (`id_sf_senador`,`id`),
  KEY `id_sf_despesa_tipo` (`id_sf_despesa_tipo`) USING BTREE,
  KEY `id_fornecedor` (`id_fornecedor`) USING BTREE,
  KEY `ano_mes` (`ano_mes`) USING BTREE,
  KEY `ano` (`ano`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_despesa_old`
--

DROP TABLE IF EXISTS `sf_despesa_old`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_despesa_old` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `codigo` int unsigned NOT NULL DEFAULT '0',
  `id_sf_senador` int unsigned DEFAULT NULL,
  `id_sf_despesa_tipo` int unsigned DEFAULT NULL,
  `id_fornecedor` int unsigned DEFAULT NULL,
  `ano_mes` decimal(6,0) unsigned DEFAULT NULL,
  `ano` smallint unsigned DEFAULT NULL,
  `mes` smallint unsigned DEFAULT NULL,
  `documento` varchar(50) DEFAULT NULL,
  `data_documento` date DEFAULT NULL,
  `detalhamento` text,
  `valor` decimal(10,2) DEFAULT NULL,
  `hash` char(40) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `id_sf_senador` (`id_sf_senador`),
  KEY `id_sf_despesa_tipo` (`id_sf_despesa_tipo`),
  KEY `id_fornecedor` (`id_fornecedor`),
  KEY `ano_mes` (`ano_mes`),
  KEY `hash` (`hash`),
  KEY `ano` (`ano`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_despesa_resumo_mensal`
--

DROP TABLE IF EXISTS `sf_despesa_resumo_mensal`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_despesa_resumo_mensal` (
  `ano` int unsigned NOT NULL,
  `mes` int unsigned NOT NULL,
  `valor` decimal(10,2) DEFAULT NULL,
  PRIMARY KEY (`ano`,`mes`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_despesa_temp`
--

DROP TABLE IF EXISTS `sf_despesa_temp`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_despesa_temp` (
  `ano` decimal(4,0) DEFAULT NULL,
  `mes` decimal(2,0) DEFAULT NULL,
  `senador` varchar(255) DEFAULT NULL,
  `tipo_despesa` varchar(255) DEFAULT NULL,
  `cnpj_cpf` varchar(14) DEFAULT NULL,
  `fornecedor` varchar(255) DEFAULT NULL,
  `documento` varchar(50) DEFAULT NULL,
  `data` datetime DEFAULT NULL,
  `detalhamento` text,
  `valor_reembolsado` decimal(10,2) DEFAULT NULL,
  `cod_documento` bigint unsigned DEFAULT NULL,
  `hash` char(40) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_despesa_tipo`
--

DROP TABLE IF EXISTS `sf_despesa_tipo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_despesa_tipo` (
  `id` tinyint unsigned NOT NULL,
  `descricao` varchar(255) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `descricao_UNIQUE` (`descricao`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_funcao`
--

DROP TABLE IF EXISTS `sf_funcao`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_funcao` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `descricao` char(5) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `descricao` (`descricao`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_legislatura`
--

DROP TABLE IF EXISTS `sf_legislatura`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_legislatura` (
  `id` tinyint unsigned NOT NULL,
  `inicio` date NOT NULL,
  `final` date NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_lotacao`
--

DROP TABLE IF EXISTS `sf_lotacao`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_lotacao` (
  `id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `id_senador` int unsigned DEFAULT NULL,
  `descricao` varchar(100) NOT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `descricao` (`descricao`),
  KEY `id_senador` (`id_senador`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_mandato`
--

DROP TABLE IF EXISTS `sf_mandato`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_mandato` (
  `id` mediumint unsigned NOT NULL,
  `id_sf_senador` mediumint unsigned NOT NULL,
  `id_estado` tinyint unsigned DEFAULT NULL,
  `participacao` char(2) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `exerceu` bit(1) NOT NULL,
  PRIMARY KEY (`id`,`id_sf_senador`) USING BTREE,
  KEY `id_sf_senador` (`id_sf_senador`),
  KEY `FK_sf_mandato_estado` (`id_estado`),
  CONSTRAINT `FK_sf_mandato_estado` FOREIGN KEY (`id_estado`) REFERENCES `estado` (`id`),
  CONSTRAINT `FK_sf_mandato_sf_senador` FOREIGN KEY (`id_sf_senador`) REFERENCES `sf_senador` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_mandato_exercicio`
--

DROP TABLE IF EXISTS `sf_mandato_exercicio`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_mandato_exercicio` (
  `id` mediumint unsigned NOT NULL,
  `id_sf_senador` mediumint unsigned NOT NULL,
  `id_sf_mandato` mediumint unsigned NOT NULL,
  `id_sf_motivo_afastamento` char(5) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `inicio` date DEFAULT NULL,
  `final` date DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `id_sf_senador` (`id_sf_senador`),
  KEY `FK_sf_mandato_exercicio_sf_mandato` (`id_sf_mandato`),
  CONSTRAINT `FK_sf_mandato_exercicio_sf_mandato` FOREIGN KEY (`id_sf_mandato`) REFERENCES `sf_mandato` (`id`),
  CONSTRAINT `FK_sf_mandato_exercicio_sf_senador` FOREIGN KEY (`id_sf_senador`) REFERENCES `sf_senador` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_mandato_legislatura`
--

DROP TABLE IF EXISTS `sf_mandato_legislatura`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_mandato_legislatura` (
  `id_sf_mandato` mediumint unsigned NOT NULL,
  `id_sf_legislatura` tinyint unsigned NOT NULL,
  UNIQUE KEY `id_sf_mandato_id_sf_legislatura` (`id_sf_mandato`,`id_sf_legislatura`),
  KEY `FK__sf_mandato` (`id_sf_mandato`) USING BTREE,
  KEY `FK__sf_legislatura` (`id_sf_legislatura`) USING BTREE,
  CONSTRAINT `FK_sf_mandato_legislatura_sf_legislatura` FOREIGN KEY (`id_sf_legislatura`) REFERENCES `sf_legislatura` (`id`),
  CONSTRAINT `FK_sf_mandato_legislatura_sf_mandato` FOREIGN KEY (`id_sf_mandato`) REFERENCES `sf_mandato` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_motivo_afastamento`
--

DROP TABLE IF EXISTS `sf_motivo_afastamento`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_motivo_afastamento` (
  `id` char(5) NOT NULL,
  `descricao` varchar(100) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_referencia_cargo`
--

DROP TABLE IF EXISTS `sf_referencia_cargo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_referencia_cargo` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `descricao` varchar(100) NOT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `descricao` (`descricao`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_remuneracao`
--

DROP TABLE IF EXISTS `sf_remuneracao`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_remuneracao` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `id_vinculo` tinyint unsigned NOT NULL,
  `id_categoria` tinyint unsigned NOT NULL,
  `id_cargo` tinyint unsigned DEFAULT NULL,
  `id_referencia_cargo` tinyint unsigned DEFAULT NULL,
  `id_simbolo_funcao` tinyint unsigned DEFAULT NULL,
  `id_lotacao` smallint unsigned NOT NULL,
  `id_tipo_folha` tinyint unsigned NOT NULL,
  `ano_mes` mediumint unsigned NOT NULL,
  `admissao` smallint unsigned NOT NULL,
  `remun_basica` decimal(10,2) DEFAULT NULL,
  `vant_pessoais` decimal(10,2) DEFAULT NULL,
  `func_comissionada` decimal(10,2) DEFAULT NULL,
  `grat_natalina` decimal(10,2) DEFAULT NULL,
  `horas_extras` decimal(10,2) DEFAULT NULL,
  `outras_eventuais` decimal(10,2) DEFAULT NULL,
  `abono_permanencia` decimal(10,2) DEFAULT NULL,
  `reversao_teto_const` decimal(10,2) DEFAULT NULL,
  `imposto_renda` decimal(10,2) DEFAULT NULL,
  `previdencia` decimal(10,2) DEFAULT NULL,
  `faltas` decimal(10,2) DEFAULT NULL,
  `rem_liquida` decimal(10,2) DEFAULT NULL,
  `diarias` decimal(10,2) DEFAULT NULL,
  `auxilios` decimal(10,2) DEFAULT NULL,
  `vant_indenizatorias` decimal(10,2) DEFAULT NULL,
  `custo_total` decimal(10,2) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  KEY `sfm_id_vinculo` (`id_vinculo`) USING BTREE,
  KEY `sfm_id_categoria` (`id_categoria`) USING BTREE,
  KEY `sfm_id_cargo` (`id_cargo`) USING BTREE,
  KEY `sfm_id_referencia_cargo` (`id_referencia_cargo`) USING BTREE,
  KEY `sfm_id_simbolo_funcao` (`id_simbolo_funcao`) USING BTREE,
  KEY `sfm_id_lotacao` (`id_lotacao`) USING BTREE,
  KEY `sfm_id_tipo_folha` (`id_tipo_folha`) USING BTREE,
  KEY `sfm_ano_mes` (`ano_mes`) USING BTREE,
  CONSTRAINT `FK_sf_remuneracao_sf_cargo_1` FOREIGN KEY (`id_cargo`) REFERENCES `sf_cargo` (`id`),
  CONSTRAINT `FK_sf_remuneracao_sf_categoria` FOREIGN KEY (`id_categoria`) REFERENCES `sf_categoria` (`id`),
  CONSTRAINT `FK_sf_remuneracao_sf_funcao` FOREIGN KEY (`id_simbolo_funcao`) REFERENCES `sf_funcao` (`id`),
  CONSTRAINT `FK_sf_remuneracao_sf_lotacao` FOREIGN KEY (`id_lotacao`) REFERENCES `sf_lotacao` (`id`),
  CONSTRAINT `FK_sf_remuneracao_sf_referencia_cargo` FOREIGN KEY (`id_referencia_cargo`) REFERENCES `sf_referencia_cargo` (`id`),
  CONSTRAINT `FK_sf_remuneracao_sf_tipo_folha` FOREIGN KEY (`id_tipo_folha`) REFERENCES `sf_tipo_folha` (`id`),
  CONSTRAINT `FK_sf_remuneracao_sf_vinculo` FOREIGN KEY (`id_vinculo`) REFERENCES `sf_vinculo` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_remuneracao_temp`
--

DROP TABLE IF EXISTS `sf_remuneracao_temp`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_remuneracao_temp` (
  `ano_mes` int DEFAULT NULL,
  `vinculo` varchar(255) DEFAULT NULL,
  `categoria` varchar(255) DEFAULT NULL,
  `cargo` varchar(255) DEFAULT NULL,
  `referencia_cargo` varchar(255) DEFAULT NULL,
  `simbolo_funcao` varchar(255) DEFAULT NULL,
  `lotacao_exercicio` varchar(255) DEFAULT NULL,
  `tipo_folha` varchar(255) DEFAULT NULL,
  `admissao` int DEFAULT NULL,
  `remun_basica` decimal(10,2) DEFAULT NULL,
  `vant_pessoais` decimal(10,2) DEFAULT NULL,
  `func_comissionada` decimal(10,2) DEFAULT NULL,
  `grat_natalina` decimal(10,2) DEFAULT NULL,
  `horas_extras` decimal(10,2) DEFAULT NULL,
  `outras_eventuais` decimal(10,2) DEFAULT NULL,
  `abono_permanencia` decimal(10,2) DEFAULT NULL,
  `reversao_teto_const` decimal(10,2) DEFAULT NULL,
  `imposto_renda` decimal(10,2) DEFAULT NULL,
  `previdencia` decimal(10,2) DEFAULT NULL,
  `faltas` decimal(10,2) DEFAULT NULL,
  `rem_liquida` decimal(10,2) DEFAULT NULL,
  `diarias` decimal(10,2) DEFAULT NULL,
  `auxilios` decimal(10,2) DEFAULT NULL,
  `vant_indenizatorias` decimal(10,2) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_secretario`
--

DROP TABLE IF EXISTS `sf_secretario`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_secretario` (
  `id` int unsigned DEFAULT NULL,
  `id_senador` int unsigned DEFAULT NULL,
  `nome` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `id_funcao` tinyint unsigned DEFAULT NULL,
  `id_cargo` tinyint unsigned DEFAULT NULL,
  `id_vinculo` tinyint unsigned DEFAULT NULL,
  `id_categoria` tinyint unsigned DEFAULT NULL,
  `id_referencia_cargo` tinyint unsigned DEFAULT NULL,
  `id_especialidade` tinyint unsigned DEFAULT NULL,
  `id_lotacao` smallint unsigned DEFAULT NULL,
  `admissao` smallint unsigned DEFAULT NULL,
  `situacao` bit(1) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_secretario_bkp`
--

DROP TABLE IF EXISTS `sf_secretario_bkp`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_secretario_bkp` (
  `id` int unsigned DEFAULT NULL,
  `id_senador` int unsigned DEFAULT NULL,
  `nome` varchar(255) DEFAULT NULL,
  `funcao` varchar(10) DEFAULT NULL,
  `nome_funcao` varchar(255) DEFAULT NULL,
  `vinculo` varchar(255) DEFAULT NULL,
  `situacao` varchar(255) DEFAULT NULL,
  `admissao` smallint unsigned DEFAULT NULL,
  `cargo` varchar(100) DEFAULT NULL,
  `padrao` varchar(10) DEFAULT NULL,
  `especialidade` varchar(255) DEFAULT NULL,
  `lotacao` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_secretario_completo`
--

DROP TABLE IF EXISTS `sf_secretario_completo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_secretario_completo` (
  `id` int unsigned DEFAULT NULL,
  `id_senador` int unsigned DEFAULT NULL,
  `nome` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `funcao` varchar(10) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `nome_funcao` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `vinculo` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `situacao` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `admissao` smallint unsigned DEFAULT NULL,
  `cargo` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `padrao` varchar(10) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `especialidade` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `lotacao` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_secretario_completo_bkp`
--

DROP TABLE IF EXISTS `sf_secretario_completo_bkp`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_secretario_completo_bkp` (
  `id` int unsigned DEFAULT NULL,
  `id_senador` int unsigned DEFAULT NULL,
  `nome` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `funcao` varchar(10) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `nome_funcao` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `vinculo` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `situacao` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `admissao` smallint unsigned DEFAULT NULL,
  `cargo` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `padrao` varchar(10) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `especialidade` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `lotacao` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_secretario_completo_bkp2`
--

DROP TABLE IF EXISTS `sf_secretario_completo_bkp2`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_secretario_completo_bkp2` (
  `id` int unsigned DEFAULT NULL,
  `id_senador` int unsigned DEFAULT NULL,
  `nome` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `funcao` varchar(10) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `nome_funcao` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `vinculo` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `situacao` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `admissao` smallint unsigned DEFAULT NULL,
  `cargo` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `padrao` varchar(10) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `especialidade` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `lotacao` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_secretario_temp`
--

DROP TABLE IF EXISTS `sf_secretario_temp`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_secretario_temp` (
  `id` int unsigned DEFAULT NULL,
  `id_senador` int unsigned DEFAULT NULL,
  `nome` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `funcao` varchar(10) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `nome_funcao` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `vinculo` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `situacao` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `admissao` smallint unsigned DEFAULT NULL,
  `cargo` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `padrao` varchar(10) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `especialidade` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `lotacao` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_senador`
--

DROP TABLE IF EXISTS `sf_senador`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_senador` (
  `id` mediumint unsigned NOT NULL DEFAULT '0',
  `codigo` mediumint unsigned DEFAULT NULL,
  `nome` varchar(255) DEFAULT NULL,
  `nome_completo` varchar(255) DEFAULT NULL,
  `sexo` char(1) DEFAULT NULL,
  `nascimento` date DEFAULT NULL,
  `naturalidade` varchar(50) DEFAULT NULL,
  `id_estado_naturalidade` int unsigned DEFAULT NULL,
  `profissao` varchar(100) DEFAULT NULL,
  `id_partido` int unsigned DEFAULT NULL,
  `id_estado` int unsigned DEFAULT NULL,
  `email` varchar(100) DEFAULT NULL,
  `site` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `ativo` char(1) DEFAULT NULL,
  `nome_importacao` varchar(255) DEFAULT NULL,
  `valor_total_ceaps` decimal(16,2) DEFAULT NULL,
  `valor_total_remuneracao` decimal(16,2) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `nome` (`nome`),
  KEY `nome_completo` (`nome_completo`),
  KEY `id_partido` (`id_partido`),
  KEY `id_estado` (`id_estado`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_senador_campeao_gasto`
--

DROP TABLE IF EXISTS `sf_senador_campeao_gasto`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_senador_campeao_gasto` (
  `id_sf_senador` int NOT NULL,
  `nome_parlamentar` varchar(100) DEFAULT NULL,
  `valor_total` decimal(10,2) DEFAULT NULL,
  `sigla_partido` varchar(20) DEFAULT NULL,
  `sigla_estado` varchar(2) DEFAULT NULL,
  PRIMARY KEY (`id_sf_senador`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_senador_historico_academico`
--

DROP TABLE IF EXISTS `sf_senador_historico_academico`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_senador_historico_academico` (
  `id_sf_senador` mediumint unsigned NOT NULL,
  `nome_curso` varchar(255) NOT NULL,
  `grau_instrucao` varchar(50) NOT NULL,
  `estabelecimento` varchar(255) DEFAULT NULL,
  `local` varchar(255) DEFAULT NULL,
  UNIQUE KEY `id_sf_senador_nome_curso_grau_instrucao_estabelecimento_local` (`id_sf_senador`,`nome_curso`,`grau_instrucao`,`estabelecimento`,`local`),
  KEY `id_sf_senador` (`id_sf_senador`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_senador_profissao`
--

DROP TABLE IF EXISTS `sf_senador_profissao`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_senador_profissao` (
  `id_sf_senador` mediumint unsigned NOT NULL DEFAULT '0',
  `id_profissao` smallint unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`id_sf_senador`,`id_profissao`),
  CONSTRAINT `FK_sf_senador_profissao_sf_senador` FOREIGN KEY (`id_sf_senador`) REFERENCES `sf_senador` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_situacao`
--

DROP TABLE IF EXISTS `sf_situacao`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_situacao` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `descricao` varchar(50) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `descricao` (`descricao`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_tipo_folha`
--

DROP TABLE IF EXISTS `sf_tipo_folha`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_tipo_folha` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `descricao` varchar(100) NOT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `sf_vinculo`
--

DROP TABLE IF EXISTS `sf_vinculo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sf_vinculo` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `descricao` varchar(100) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `descricao` (`descricao`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2022-01-09
