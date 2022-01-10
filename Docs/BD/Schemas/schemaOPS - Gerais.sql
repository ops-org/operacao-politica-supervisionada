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
-- Table structure for table `eleicao_candidato`
--

DROP TABLE IF EXISTS `eleicao_candidato`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `eleicao_candidato` (
  `id` int NOT NULL AUTO_INCREMENT,
  `cpf` varchar(11) DEFAULT NULL,
  `nome` varchar(255) DEFAULT NULL,
  `email` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `cpf_UNIQUE` (`cpf`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `eleicao_candidatura`
--

DROP TABLE IF EXISTS `eleicao_candidatura`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `eleicao_candidatura` (
  `numero` int unsigned NOT NULL,
  `cargo` tinyint unsigned NOT NULL,
  `ano` int unsigned NOT NULL,
  `id_eleicao_candidato` int DEFAULT NULL,
  `id_eleicao_candidato_vice` int DEFAULT NULL,
  `sigla_partido` varchar(50) DEFAULT NULL,
  `sigla_partido_vice` varchar(50) DEFAULT NULL,
  `nome_urna` varchar(255) DEFAULT NULL,
  `nome_urna_vice` varchar(255) DEFAULT NULL,
  `sequencia` varchar(50) DEFAULT NULL,
  `sequencia_vice` varchar(50) DEFAULT NULL,
  `sigla_estado` char(2) NOT NULL DEFAULT '',
  PRIMARY KEY (`numero`,`cargo`,`ano`,`sigla_estado`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `eleicao_cargo`
--

DROP TABLE IF EXISTS `eleicao_cargo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `eleicao_cargo` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `nome` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `nome_UNIQUE` (`nome`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `eleicao_doacao`
--

DROP TABLE IF EXISTS `eleicao_doacao`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `eleicao_doacao` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `id_eleicao_cargo` int unsigned DEFAULT NULL,
  `id_eleicao_candidadto` int unsigned DEFAULT NULL,
  `ano_eleicao` decimal(4,0) unsigned DEFAULT NULL,
  `num_documento` varchar(50) DEFAULT NULL,
  `cnpj_cpf_doador` varchar(14) DEFAULT NULL,
  `raiz_cnpj_cpf_doador` varchar(14) DEFAULT NULL,
  `data_receita` date DEFAULT NULL,
  `valor_receita` decimal(10,2) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `estado`
--

DROP TABLE IF EXISTS `estado`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `estado` (
  `id` tinyint unsigned NOT NULL,
  `sigla` char(2) NOT NULL,
  `nome` varchar(30) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `sigla` (`sigla`),
  UNIQUE KEY `nome` (`nome`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `forcecedor_cnpj_incorreto`
--

DROP TABLE IF EXISTS `forcecedor_cnpj_incorreto`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `forcecedor_cnpj_incorreto` (
  `cnpj_incorreto` char(15) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `cnpj_correto` char(15) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `fornecedor`
--

DROP TABLE IF EXISTS `fornecedor`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `fornecedor` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `cnpj_cpf` char(15) DEFAULT NULL,
  `nome` varchar(255) NOT NULL,
  `doador` tinyint unsigned NOT NULL DEFAULT '0',
  `controle` tinyint(1) DEFAULT NULL,
  `mensagem` varchar(8000) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `idx_cnpj_cpf` (`cnpj_cpf`),
  KEY `nome` (`nome`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `fornecedor_atividade`
--

DROP TABLE IF EXISTS `fornecedor_atividade`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `fornecedor_atividade` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `codigo` varchar(15) NOT NULL,
  `descricao` varchar(255) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `descricao` (`descricao`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `fornecedor_atividade_secundaria`
--

DROP TABLE IF EXISTS `fornecedor_atividade_secundaria`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `fornecedor_atividade_secundaria` (
  `id_fornecedor` int unsigned NOT NULL,
  `id_fornecedor_atividade` int unsigned NOT NULL,
  PRIMARY KEY (`id_fornecedor`,`id_fornecedor_atividade`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `fornecedor_info`
--

DROP TABLE IF EXISTS `fornecedor_info`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `fornecedor_info` (
  `id_fornecedor` int unsigned NOT NULL,
  `cnpj` char(14) NOT NULL,
  `tipo` char(20) DEFAULT NULL,
  `nome` varchar(255) DEFAULT NULL,
  `data_de_abertura` date DEFAULT NULL,
  `nome_fantasia` varchar(255) DEFAULT NULL,
  `id_fornecedor_atividade_principal` int DEFAULT NULL,
  `id_fornecedor_natureza_juridica` int DEFAULT NULL,
  `logradouro` varchar(100) DEFAULT NULL,
  `numero` varchar(100) DEFAULT NULL,
  `complemento` varchar(150) DEFAULT NULL,
  `cep` varchar(20) DEFAULT NULL,
  `bairro` varchar(100) DEFAULT NULL,
  `municipio` varchar(100) DEFAULT NULL,
  `estado` varchar(4) DEFAULT NULL,
  `endereco_eletronico` varchar(100) DEFAULT NULL,
  `telefone` varchar(100) DEFAULT NULL,
  `ente_federativo_responsavel` varchar(100) DEFAULT NULL,
  `situacao_cadastral` varchar(100) DEFAULT NULL,
  `data_da_situacao_cadastral` date DEFAULT NULL,
  `motivo_situacao_cadastral` varchar(100) DEFAULT NULL,
  `situacao_especial` varchar(100) DEFAULT NULL,
  `data_situacao_especial` date DEFAULT NULL,
  `capital_social` decimal(65,2) DEFAULT NULL,
  `obtido_em` date DEFAULT NULL,
  `ip_colaborador` varchar(15) DEFAULT NULL,
  PRIMARY KEY (`id_fornecedor`),
  UNIQUE KEY `idx_cnpj` (`cnpj`),
  KEY `nome` (`nome`),
  KEY `nome_fantasia` (`nome_fantasia`),
  KEY `id_fornecedor_atividade_principal` (`id_fornecedor_atividade_principal`),
  KEY `id_fornecedor_natureza_juridica` (`id_fornecedor_natureza_juridica`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `fornecedor_natureza_juridica`
--

DROP TABLE IF EXISTS `fornecedor_natureza_juridica`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `fornecedor_natureza_juridica` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `codigo` varchar(10) NOT NULL,
  `descricao` varchar(100) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `descricao` (`descricao`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `fornecedor_socio`
--

DROP TABLE IF EXISTS `fornecedor_socio`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `fornecedor_socio` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `id_fornecedor` int unsigned NOT NULL,
  `nome` varchar(255) DEFAULT NULL,
  `pais_origem` varchar(255) DEFAULT NULL,
  `id_fornecedor_socio_qualificacao` int unsigned DEFAULT NULL,
  `nome_representante` varchar(255) DEFAULT NULL,
  `id_fornecedor_socio_representante_qualificacao` int unsigned DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `id_fornecedor` (`id_fornecedor`),
  KEY `nome` (`nome`),
  KEY `id_fornecedor_socio_qualificacao` (`id_fornecedor_socio_qualificacao`),
  KEY `id_fornecedor_socio_representante_qualificacao` (`id_fornecedor_socio_representante_qualificacao`),
  KEY `nome_representante` (`nome_representante`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `fornecedor_socio_qualificacao`
--

DROP TABLE IF EXISTS `fornecedor_socio_qualificacao`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `fornecedor_socio_qualificacao` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `descricao` varchar(100) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `descricao` (`descricao`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `parametros`
--

DROP TABLE IF EXISTS `parametros`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `parametros` (
  `cf_deputado_ultima_atualizacao` datetime DEFAULT NULL,
  `sf_senador_ultima_atualizacao` datetime DEFAULT NULL,
  `cf_deputado_presenca_ultima_atualizacao` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `partido`
--

DROP TABLE IF EXISTS `partido`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `partido` (
  `id` tinyint unsigned NOT NULL,
  `legenda` tinyint unsigned DEFAULT NULL,
  `sigla` varchar(20) NOT NULL,
  `nome` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `sigla` (`sigla`),
  UNIQUE KEY `legenda_UNIQUE` (`legenda`),
  UNIQUE KEY `nome` (`nome`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `partido_historico`
--

DROP TABLE IF EXISTS `partido_historico`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `partido_historico` (
  `id` tinyint NOT NULL AUTO_INCREMENT,
  `legenda` tinyint DEFAULT NULL,
  `sigla` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `nome` varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `sede` char(2) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `fundacao` date DEFAULT NULL COMMENT 'Fundação',
  `registro_solicitacao` date DEFAULT NULL COMMENT 'Solitação de habilitação ou registro',
  `registro_provisorio` date DEFAULT NULL COMMENT 'Registro provisório',
  `registro_definitivo` date DEFAULT NULL COMMENT 'Registro definitivo',
  `extincao` date DEFAULT NULL COMMENT 'Extinção',
  `motivo` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COMMENT='https://www.partidosdobrasil.com/';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `profissao`
--

DROP TABLE IF EXISTS `profissao`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `profissao` (
  `id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `descricao` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2022-01-09
