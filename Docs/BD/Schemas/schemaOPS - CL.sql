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
-- Table structure for table `cl_deputado`
--

DROP TABLE IF EXISTS `cl_deputado`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cl_deputado` (
  `id` mediumint unsigned NOT NULL AUTO_INCREMENT,
  `id_partido` tinyint unsigned DEFAULT NULL,
  `id_estado` tinyint unsigned DEFAULT NULL,
  `cpf` varchar(20) DEFAULT NULL,
  `nome_parlamentar` varchar(255) NOT NULL,
  `nome_civil` varchar(255) DEFAULT NULL,
  `nascimento` date DEFAULT NULL,
  `sexo` char(2) DEFAULT NULL,
  `email` varchar(100) DEFAULT NULL,
  `valor_total` decimal(10,2) NOT NULL DEFAULT '0.00',
  PRIMARY KEY (`id`),
  UNIQUE KEY `cl_cpf` (`cpf`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COMMENT='Câmara Legislativa - Deputados Estaduais';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cl_despesa`
--

DROP TABLE IF EXISTS `cl_despesa`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cl_despesa` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `id_cl_deputado` int unsigned NOT NULL,
  `id_cl_despesa_tipo` int unsigned DEFAULT NULL,
  `id_fornecedor` int unsigned NOT NULL,
  `data` date DEFAULT NULL,
  `ano_mes` mediumint unsigned DEFAULT NULL,
  `numero_documento` varchar(50) DEFAULT NULL,
  `valor` decimal(10,2) NOT NULL,
  `favorecido` varchar(100) DEFAULT NULL,
  `observacao` varchar(500) DEFAULT NULL,
  `hash` varchar(40) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cl_despesa_temp`
--

DROP TABLE IF EXISTS `cl_despesa_temp`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cl_despesa_temp` (
  `id` mediumint unsigned NOT NULL AUTO_INCREMENT,
  `nome` varchar(255) DEFAULT NULL,
  `cpf` varchar(20) DEFAULT NULL,
  `empresa` varchar(255) DEFAULT NULL,
  `cnpj_cpf` varchar(20) DEFAULT NULL,
  `data_emissao` date DEFAULT NULL,
  `tipo_verba` varchar(100) DEFAULT NULL,
  `tipo_despesa` varchar(100) DEFAULT NULL,
  `documento` varchar(100) DEFAULT NULL,
  `observacao` varchar(500) DEFAULT NULL,
  `valor` decimal(10,2) NOT NULL,
  `ano` smallint unsigned NOT NULL,
  `hash` varchar(40) DEFAULT NULL,
  `favorecido` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cl_despesa_tipo`
--

DROP TABLE IF EXISTS `cl_despesa_tipo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cl_despesa_tipo` (
  `id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `descricao` varchar(100) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cl_despesa_tipo_sub`
--

DROP TABLE IF EXISTS `cl_despesa_tipo_sub`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cl_despesa_tipo_sub` (
  `id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `id_cl_despesa_tipo` smallint unsigned DEFAULT NULL,
  `descricao` varchar(100) NOT NULL DEFAULT '',
  `verba` varchar(100) DEFAULT NULL,
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
