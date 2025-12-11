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

-- Copiando estrutura para tabela ops.sf_cargo
CREATE TABLE IF NOT EXISTS `sf_cargo` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `descricao` varchar(100) NOT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `descricao` (`descricao`)
) ENGINE=InnoDB AUTO_INCREMENT=137 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.sf_categoria
CREATE TABLE IF NOT EXISTS `sf_categoria` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `descricao` varchar(100) NOT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `descricao` (`descricao`)
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.sf_despesa
CREATE TABLE IF NOT EXISTS `sf_despesa` (
  `id` bigint unsigned NOT NULL,
  `id_sf_senador` int unsigned NOT NULL,
  `id_sf_despesa_tipo` int unsigned DEFAULT NULL,
  `id_fornecedor` int unsigned DEFAULT NULL,
  `ano_mes` decimal(6,0) unsigned DEFAULT NULL,
  `ano` smallint unsigned DEFAULT NULL,
  `mes` smallint unsigned DEFAULT NULL,
  `documento` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `data_emissao` date DEFAULT NULL,
  `detalhamento` mediumtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `valor` decimal(10,2) DEFAULT NULL,
  `hash` varbinary(100) DEFAULT NULL,
  PRIMARY KEY (`id_sf_senador`,`id`),
  KEY `id_sf_despesa_tipo` (`id_sf_despesa_tipo`) USING BTREE,
  KEY `id_fornecedor` (`id_fornecedor`) USING BTREE,
  KEY `ano_mes` (`ano_mes`) USING BTREE,
  KEY `ano` (`ano`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.sf_despesa_resumo_mensal
CREATE TABLE IF NOT EXISTS `sf_despesa_resumo_mensal` (
  `ano` int unsigned NOT NULL,
  `mes` int unsigned NOT NULL,
  `valor` decimal(10,2) DEFAULT NULL,
  PRIMARY KEY (`ano`,`mes`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.sf_despesa_tipo
CREATE TABLE IF NOT EXISTS `sf_despesa_tipo` (
  `id` tinyint unsigned NOT NULL,
  `descricao` varchar(255) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `descricao_UNIQUE` (`descricao`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.sf_funcao
CREATE TABLE IF NOT EXISTS `sf_funcao` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `descricao` char(5) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `descricao` (`descricao`)
) ENGINE=InnoDB AUTO_INCREMENT=25 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.sf_legislatura
CREATE TABLE IF NOT EXISTS `sf_legislatura` (
  `id` tinyint unsigned NOT NULL,
  `inicio` date NOT NULL,
  `final` date NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.sf_lotacao
CREATE TABLE IF NOT EXISTS `sf_lotacao` (
  `id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `id_senador` int unsigned DEFAULT NULL,
  `descricao` varchar(100) NOT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `descricao` (`descricao`),
  KEY `id_senador` (`id_senador`)
) ENGINE=InnoDB AUTO_INCREMENT=4255 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.sf_mandato
CREATE TABLE IF NOT EXISTS `sf_mandato` (
  `id` mediumint unsigned NOT NULL,
  `id_sf_senador` mediumint unsigned NOT NULL,
  `id_estado` tinyint unsigned DEFAULT NULL,
  `participacao` char(2) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `exerceu` bit(1) NOT NULL,
  PRIMARY KEY (`id`,`id_sf_senador`) USING BTREE,
  KEY `id_sf_senador` (`id_sf_senador`),
  KEY `FK_sf_mandato_estado` (`id_estado`),
  CONSTRAINT `FK_sf_mandato_estado` FOREIGN KEY (`id_estado`) REFERENCES `estado` (`id`),
  CONSTRAINT `FK_sf_mandato_sf_senador` FOREIGN KEY (`id_sf_senador`) REFERENCES `sf_senador` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.sf_mandato_exercicio
CREATE TABLE IF NOT EXISTS `sf_mandato_exercicio` (
  `id` mediumint unsigned NOT NULL,
  `id_sf_senador` mediumint unsigned NOT NULL,
  `id_sf_mandato` mediumint unsigned NOT NULL,
  `id_sf_motivo_afastamento` char(5) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `inicio` date DEFAULT NULL,
  `final` date DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `id_sf_senador` (`id_sf_senador`),
  KEY `FK_sf_mandato_exercicio_sf_mandato` (`id_sf_mandato`),
  CONSTRAINT `FK_sf_mandato_exercicio_sf_mandato` FOREIGN KEY (`id_sf_mandato`) REFERENCES `sf_mandato` (`id`),
  CONSTRAINT `FK_sf_mandato_exercicio_sf_senador` FOREIGN KEY (`id_sf_senador`) REFERENCES `sf_senador` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.sf_mandato_legislatura
CREATE TABLE IF NOT EXISTS `sf_mandato_legislatura` (
  `id_sf_mandato` mediumint unsigned NOT NULL,
  `id_sf_legislatura` tinyint unsigned NOT NULL,
  UNIQUE KEY `id_sf_mandato_id_sf_legislatura` (`id_sf_mandato`,`id_sf_legislatura`),
  KEY `FK__sf_mandato` (`id_sf_mandato`) USING BTREE,
  KEY `FK__sf_legislatura` (`id_sf_legislatura`) USING BTREE,
  CONSTRAINT `FK_sf_mandato_legislatura_sf_legislatura` FOREIGN KEY (`id_sf_legislatura`) REFERENCES `sf_legislatura` (`id`),
  CONSTRAINT `FK_sf_mandato_legislatura_sf_mandato` FOREIGN KEY (`id_sf_mandato`) REFERENCES `sf_mandato` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.sf_motivo_afastamento
CREATE TABLE IF NOT EXISTS `sf_motivo_afastamento` (
  `id` char(5) NOT NULL,
  `descricao` varchar(100) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.sf_referencia_cargo
CREATE TABLE IF NOT EXISTS `sf_referencia_cargo` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `descricao` varchar(100) NOT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `descricao` (`descricao`)
) ENGINE=InnoDB AUTO_INCREMENT=50 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.sf_remuneracao
CREATE TABLE IF NOT EXISTS `sf_remuneracao` (
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
) ENGINE=InnoDB AUTO_INCREMENT=1108022 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.sf_secretario
CREATE TABLE IF NOT EXISTS `sf_secretario` (
  `id` int unsigned DEFAULT NULL,
  `id_senador` int unsigned DEFAULT NULL,
  `nome` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `id_funcao` tinyint unsigned DEFAULT NULL,
  `id_cargo` tinyint unsigned DEFAULT NULL,
  `id_vinculo` tinyint unsigned DEFAULT NULL,
  `id_categoria` tinyint unsigned DEFAULT NULL,
  `id_referencia_cargo` tinyint unsigned DEFAULT NULL,
  `id_especialidade` tinyint unsigned DEFAULT NULL,
  `id_lotacao` smallint unsigned DEFAULT NULL,
  `admissao` smallint unsigned DEFAULT NULL,
  `situacao` bit(1) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.sf_secretario_completo
CREATE TABLE IF NOT EXISTS `sf_secretario_completo` (
  `id` int unsigned DEFAULT NULL,
  `id_senador` int unsigned DEFAULT NULL,
  `nome` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `funcao` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `nome_funcao` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `vinculo` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `situacao` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `admissao` smallint unsigned DEFAULT NULL,
  `cargo` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `padrao` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `especialidade` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `lotacao` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.sf_senador
CREATE TABLE IF NOT EXISTS `sf_senador` (
  `id` mediumint unsigned NOT NULL DEFAULT '0',
  `codigo` mediumint unsigned DEFAULT NULL,
  `nome` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `nome_completo` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `sexo` char(1) DEFAULT NULL,
  `nascimento` date DEFAULT NULL,
  `naturalidade` varchar(50) DEFAULT NULL,
  `id_estado_naturalidade` int unsigned DEFAULT NULL,
  `profissao` varchar(100) DEFAULT NULL,
  `id_partido` int unsigned NOT NULL,
  `id_estado` int unsigned DEFAULT NULL,
  `email` varchar(100) DEFAULT NULL,
  `site` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ativo` char(1) DEFAULT NULL,
  `nome_importacao` varchar(255) DEFAULT NULL,
  `valor_total_ceaps` decimal(16,2) NOT NULL,
  `valor_total_remuneracao` decimal(16,2) NOT NULL,
  `hash` varbinary(100) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `nome` (`nome`),
  KEY `nome_completo` (`nome_completo`),
  KEY `id_partido` (`id_partido`),
  KEY `id_estado` (`id_estado`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.sf_senador_campeao_gasto
CREATE TABLE IF NOT EXISTS `sf_senador_campeao_gasto` (
  `id_sf_senador` int NOT NULL,
  `nome_parlamentar` varchar(100) DEFAULT NULL,
  `valor_total` decimal(10,2) DEFAULT NULL,
  `sigla_partido` varchar(20) DEFAULT NULL,
  `sigla_estado` varchar(2) DEFAULT NULL,
  PRIMARY KEY (`id_sf_senador`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.sf_senador_historico_academico
CREATE TABLE IF NOT EXISTS `sf_senador_historico_academico` (
  `id_sf_senador` mediumint unsigned NOT NULL,
  `nome_curso` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `grau_instrucao` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `estabelecimento` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `local` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  UNIQUE KEY `unique` (`id_sf_senador`,`nome_curso`,`grau_instrucao`) USING BTREE,
  KEY `id_sf_senador` (`id_sf_senador`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.sf_senador_profissao
CREATE TABLE IF NOT EXISTS `sf_senador_profissao` (
  `id_sf_senador` mediumint unsigned NOT NULL DEFAULT '0',
  `id_profissao` smallint unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`id_sf_senador`,`id_profissao`),
  CONSTRAINT `FK_sf_senador_profissao_sf_senador` FOREIGN KEY (`id_sf_senador`) REFERENCES `sf_senador` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.sf_situacao
CREATE TABLE IF NOT EXISTS `sf_situacao` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `descricao` varchar(50) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `descricao` (`descricao`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.sf_tipo_folha
CREATE TABLE IF NOT EXISTS `sf_tipo_folha` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `descricao` varchar(100) NOT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.sf_vinculo
CREATE TABLE IF NOT EXISTS `sf_vinculo` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `descricao` varchar(100) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `descricao` (`descricao`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
