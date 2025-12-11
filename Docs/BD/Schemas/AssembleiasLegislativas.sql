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

-- Copiando estrutura para tabela ops.cf_sessao_presenca
CREATE TABLE IF NOT EXISTS `cf_sessao_presenca` (
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
) ENGINE=InnoDB AUTO_INCREMENT=2443476 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci ROW_FORMAT=DYNAMIC;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cl_deputado
CREATE TABLE IF NOT EXISTS `cl_deputado` (
  `id` mediumint unsigned NOT NULL AUTO_INCREMENT,
  `matricula` mediumint unsigned DEFAULT NULL,
  `gabinete` mediumint unsigned DEFAULT NULL,
  `id_partido` tinyint unsigned NOT NULL,
  `id_estado` tinyint unsigned NOT NULL,
  `cpf` varchar(11) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `cpf_parcial` varchar(6) DEFAULT NULL,
  `nome_parlamentar` varchar(255) NOT NULL,
  `nome_civil` varchar(255) DEFAULT NULL,
  `nome_importacao` varchar(255) DEFAULT NULL,
  `nascimento` date DEFAULT NULL,
  `sexo` char(2) DEFAULT NULL,
  `email` varchar(100) DEFAULT NULL,
  `naturalidade` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `escolaridade` varchar(100) DEFAULT NULL,
  `profissao` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `telefone` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `site` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `perfil` varchar(100) DEFAULT NULL,
  `foto` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `valor_total_ceap` decimal(12,2) NOT NULL DEFAULT '0.00',
  PRIMARY KEY (`id`),
  UNIQUE KEY `cl_cpf` (`cpf`),
  UNIQUE KEY `cpf_parcial` (`cpf_parcial`),
  UNIQUE KEY `id_estado_nome_civil` (`id_estado`,`nome_civil`),
  UNIQUE KEY `email` (`email`)
) ENGINE=InnoDB AUTO_INCREMENT=8497 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Câmara Legislativa - Deputados Estaduais';

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cl_deputado_campeao_gasto
CREATE TABLE IF NOT EXISTS `cl_deputado_campeao_gasto` (
  `id_cl_deputado` mediumint unsigned NOT NULL,
  `nome_parlamentar` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `valor_total` decimal(10,2) unsigned DEFAULT NULL,
  `sigla_partido` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `sigla_estado` varchar(2) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`id_cl_deputado`) USING BTREE,
  KEY `nome_parlamentar` (`nome_parlamentar`) USING BTREE,
  CONSTRAINT `FK_cl_deputado_campeao_gasto_cf_deputado` FOREIGN KEY (`id_cl_deputado`) REFERENCES `cl_deputado` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cl_despesa
CREATE TABLE IF NOT EXISTS `cl_despesa` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `id_cl_deputado` int unsigned NOT NULL,
  `id_cl_despesa_tipo` int unsigned DEFAULT NULL,
  `id_cl_despesa_especificacao` int unsigned DEFAULT NULL,
  `id_fornecedor` int unsigned DEFAULT NULL,
  `data_emissao` date DEFAULT NULL,
  `ano_mes` mediumint unsigned DEFAULT NULL,
  `numero_documento` varchar(50) DEFAULT NULL,
  `valor_liquido` decimal(10,2) NOT NULL,
  `favorecido` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `observacao` varchar(8000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `hash` varbinary(100) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_cl_deputado_ano_mes_hash` (`id_cl_deputado`,`ano_mes`,`hash`)
) ENGINE=InnoDB AUTO_INCREMENT=990896 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cl_despesa_especificacao
CREATE TABLE IF NOT EXISTS `cl_despesa_especificacao` (
  `id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `id_cl_despesa_tipo` smallint unsigned DEFAULT NULL,
  `descricao` varchar(400) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT '',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=807 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cl_despesa_resumo_mensal
CREATE TABLE IF NOT EXISTS `cl_despesa_resumo_mensal` (
  `ano` int unsigned NOT NULL,
  `mes` int unsigned NOT NULL,
  `valor` decimal(10,2) unsigned DEFAULT NULL,
  PRIMARY KEY (`ano`,`mes`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cl_despesa_tipo
CREATE TABLE IF NOT EXISTS `cl_despesa_tipo` (
  `id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `descricao` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
