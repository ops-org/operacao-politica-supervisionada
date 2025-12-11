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

-- Copiando estrutura para tabela ops.estado
CREATE TABLE IF NOT EXISTS `estado` (
  `id` tinyint unsigned NOT NULL COMMENT 'Código no IBGE',
  `sigla` char(2) NOT NULL,
  `nome` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `regiao` varchar(30) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `sigla` (`sigla`),
  UNIQUE KEY `nome` (`nome`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='https://ibge.gov.br/explica/codigos-dos-municipios.php';

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.parametros
CREATE TABLE IF NOT EXISTS `parametros` (
  `cf_deputado_ultima_atualizacao` datetime DEFAULT NULL,
  `sf_senador_ultima_atualizacao` datetime DEFAULT NULL,
  `cf_deputado_presenca_ultima_atualizacao` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.partido
CREATE TABLE IF NOT EXISTS `partido` (
  `id` tinyint unsigned NOT NULL,
  `legenda` tinyint unsigned DEFAULT NULL,
  `sigla` varchar(20) NOT NULL,
  `nome` varchar(100) DEFAULT NULL,
  `imagem` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `sigla` (`sigla`),
  UNIQUE KEY `nome` (`nome`),
  KEY `legenda` (`legenda`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.partido_historico
CREATE TABLE IF NOT EXISTS `partido_historico` (
  `id` tinyint NOT NULL AUTO_INCREMENT,
  `legenda` tinyint DEFAULT NULL,
  `sigla` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `nome` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `sede` char(2) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `fundacao` date DEFAULT NULL COMMENT 'Fundação',
  `registro_solicitacao` date DEFAULT NULL COMMENT 'Solitação de habilitação ou registro',
  `registro_provisorio` date DEFAULT NULL COMMENT 'Registro provisório',
  `registro_definitivo` date DEFAULT NULL COMMENT 'Registro definitivo',
  `extincao` date DEFAULT NULL COMMENT 'Extinção',
  `motivo` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=126 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='https://www.partidosdobrasil.com/';

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.pessoa
CREATE TABLE IF NOT EXISTS `pessoa` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `cpf` varchar(15) DEFAULT NULL,
  `nome` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=57345 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.pessoa_new
CREATE TABLE IF NOT EXISTS `pessoa_new` (
  `id` int unsigned NOT NULL AUTO_INCREMENT,
  `cpf` varchar(15) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `cpf_parcial` varchar(6) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `nome` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `data_nascimento` date DEFAULT NULL,
  `id_nacionalidade` tinyint unsigned DEFAULT NULL,
  `id_estado_nascimento` mediumint unsigned DEFAULT NULL,
  `municipio_nascimento` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `id_genero` tinyint unsigned DEFAULT NULL,
  `id_etnia` tinyint unsigned DEFAULT NULL,
  `id_estado_civil` tinyint unsigned DEFAULT NULL,
  `id_grau_instrucao` tinyint unsigned DEFAULT NULL,
  `id_ocupacao` mediumint unsigned DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `cpf` (`cpf`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.profissao
CREATE TABLE IF NOT EXISTS `profissao` (
  `id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `descricao` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=89 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.trecho_viagem
CREATE TABLE IF NOT EXISTS `trecho_viagem` (
  `id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `descricao` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=31538 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
