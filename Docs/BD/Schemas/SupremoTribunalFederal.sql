-- --------------------------------------------------------
-- Servidor:                     127.0.0.1
-- Versão do servidor:           8.0.40 - MySQL Community Server - GPL
-- OS do Servidor:               Win64
-- HeidiSQL Versão:              12.9.0.6999
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

-- Copiando estrutura para tabela ops.tse_eleicao
CREATE TABLE IF NOT EXISTS `tse_eleicao` (
  `id` int unsigned DEFAULT NULL,
  `descricao` varchar(50) DEFAULT NULL,
  `data` date DEFAULT NULL,
  `turno` varchar(50) DEFAULT NULL,
  `tipo` varchar(50) DEFAULT NULL,
  `abrangencia` varchar(50) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='CD_ELEICAO, DS_ELEICAO, DT_ELEICAO, NM_TIPO_ELEICAO, NR_TURNO, TP_ABRANGENCIA';

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.tse_eleicao_candidato
CREATE TABLE IF NOT EXISTS `tse_eleicao_candidato` (
  `id` int NOT NULL AUTO_INCREMENT,
  `cpf` varchar(11) DEFAULT NULL,
  `nome` varchar(255) DEFAULT NULL,
  `email` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `cpf_UNIQUE` (`cpf`)
) ENGINE=InnoDB AUTO_INCREMENT=50691 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.tse_eleicao_candidatura
CREATE TABLE IF NOT EXISTS `tse_eleicao_candidatura` (
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.tse_eleicao_cargo
CREATE TABLE IF NOT EXISTS `tse_eleicao_cargo` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `nome` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `nome_UNIQUE` (`nome`)
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.tse_eleicao_doacao
CREATE TABLE IF NOT EXISTS `tse_eleicao_doacao` (
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
) ENGINE=InnoDB AUTO_INCREMENT=426557 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
