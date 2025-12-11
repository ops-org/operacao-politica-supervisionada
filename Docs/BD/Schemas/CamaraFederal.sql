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

-- Copiando estrutura para tabela ops.cf_deputado
CREATE TABLE IF NOT EXISTS `cf_deputado` (
  `id` mediumint unsigned NOT NULL COMMENT 'ideDeputado',
  `id_deputado` mediumint unsigned DEFAULT NULL COMMENT 'nuDeputadoId',
  `id_partido` tinyint unsigned NOT NULL,
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
  `processado` tinyint NOT NULL DEFAULT (0),
  `valor_total_ceap` decimal(16,2) unsigned NOT NULL DEFAULT '0.00' COMMENT 'Valor acumulado gasto com a cota parlamentar em todas as legislaturas',
  `secretarios_ativos` tinyint unsigned DEFAULT NULL COMMENT 'Quantidade de secretarios',
  `valor_mensal_secretarios` decimal(16,2) NOT NULL DEFAULT '0.00',
  `valor_total_remuneracao` decimal(16,2) NOT NULL DEFAULT '0.00' COMMENT 'Renomear para valor_total_gabinete',
  `valor_total_salario` decimal(16,2) NOT NULL DEFAULT '0.00' COMMENT 'Renomear para valor_total_remuneracao',
  `valor_total_auxilio_moradia` decimal(16,2) NOT NULL DEFAULT '0.00',
  PRIMARY KEY (`id`),
  KEY `id_deputado` (`id_deputado`),
  KEY `id_partido` (`id_partido`),
  KEY `id_estado` (`id_estado`),
  KEY `id_cf_gabinete` (`id_cf_gabinete`),
  KEY `id_estado_nascimento` (`id_estado_nascimento`),
  KEY `nome_parlamentar` (`nome_parlamentar`),
  KEY `quantidade_secretarios` (`secretarios_ativos`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_deputado_auxilio_moradia
CREATE TABLE IF NOT EXISTS `cf_deputado_auxilio_moradia` (
  `id_cf_deputado` mediumint unsigned NOT NULL,
  `ano` smallint unsigned DEFAULT NULL,
  `mes` smallint unsigned DEFAULT NULL,
  `valor` decimal(10,2) unsigned DEFAULT NULL,
  UNIQUE KEY `id_cf_deputado` (`id_cf_deputado`,`ano`,`mes`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_deputado_campeao_gasto
CREATE TABLE IF NOT EXISTS `cf_deputado_campeao_gasto` (
  `id_cf_deputado` mediumint unsigned NOT NULL,
  `nome_parlamentar` varchar(100) DEFAULT NULL,
  `valor_total` decimal(10,2) unsigned DEFAULT NULL,
  `sigla_partido` varchar(20) DEFAULT NULL,
  `sigla_estado` varchar(2) DEFAULT NULL,
  PRIMARY KEY (`id_cf_deputado`),
  KEY `nome_parlamentar` (`nome_parlamentar`),
  CONSTRAINT `FK_cf_deputado_campeao_gasto_cf_deputado` FOREIGN KEY (`id_cf_deputado`) REFERENCES `cf_deputado` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_deputado_cota_parlamentar
CREATE TABLE IF NOT EXISTS `cf_deputado_cota_parlamentar` (
  `id_cf_deputado` int NOT NULL,
  `ano` smallint NOT NULL,
  `mes` smallint NOT NULL,
  `valor` decimal(10,2) NOT NULL,
  `percentual` decimal(10,2) DEFAULT NULL,
  UNIQUE KEY `id_cl_deputado_ano_mes` (`id_cf_deputado`,`ano`,`mes`) USING BTREE,
  KEY `id_cl_deputado` (`id_cf_deputado`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_deputado_gabinete
CREATE TABLE IF NOT EXISTS `cf_deputado_gabinete` (
  `id` int NOT NULL,
  `nome` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_deputado_imovel_funcional
CREATE TABLE IF NOT EXISTS `cf_deputado_imovel_funcional` (
  `id_cf_deputado` mediumint NOT NULL,
  `uso_de` date NOT NULL,
  `uso_ate` date DEFAULT NULL,
  `total_dias` smallint DEFAULT NULL,
  UNIQUE KEY `id_cf_deputado` (`id_cf_deputado`,`uso_de`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_deputado_missao_oficial
CREATE TABLE IF NOT EXISTS `cf_deputado_missao_oficial` (
  `id_cf_deputado` mediumint unsigned NOT NULL,
  `periodo` varchar(50) NOT NULL,
  `assunto` varchar(4000) NOT NULL,
  `destino` varchar(255) DEFAULT NULL,
  `passagens` decimal(10,2) DEFAULT NULL,
  `diarias` decimal(10,2) DEFAULT NULL,
  `relatorio` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_deputado_remuneracao
CREATE TABLE IF NOT EXISTS `cf_deputado_remuneracao` (
  `id_cf_deputado` int unsigned NOT NULL,
  `ano` smallint NOT NULL,
  `mes` smallint NOT NULL,
  `valor` decimal(10,2) NOT NULL,
  UNIQUE KEY `id_cl_deputado_ano_mes` (`id_cf_deputado`,`ano`,`mes`) USING BTREE,
  KEY `id_cl_deputado` (`id_cf_deputado`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_deputado_verba_gabinete
CREATE TABLE IF NOT EXISTS `cf_deputado_verba_gabinete` (
  `id_cf_deputado` int NOT NULL,
  `ano` smallint NOT NULL,
  `mes` smallint NOT NULL,
  `valor` decimal(10,2) NOT NULL,
  `percentual` decimal(10,2) DEFAULT NULL,
  UNIQUE KEY `id_cl_deputado_ano_mes` (`id_cf_deputado`,`ano`,`mes`) USING BTREE,
  KEY `id_cl_deputado` (`id_cf_deputado`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_despesa
CREATE TABLE IF NOT EXISTS `cf_despesa` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `ano` smallint unsigned NOT NULL,
  `mes` tinyint unsigned NOT NULL,
  `id_cf_legislatura` tinyint unsigned NOT NULL,
  `id_cf_deputado` mediumint unsigned NOT NULL,
  `id_cf_mandato` smallint unsigned DEFAULT NULL,
  `id_cf_despesa_tipo` smallint unsigned NOT NULL,
  `id_cf_especificacao` tinyint unsigned DEFAULT NULL,
  `id_fornecedor` mediumint unsigned NOT NULL,
  `id_documento` bigint unsigned DEFAULT NULL,
  `id_passageiro` mediumint unsigned DEFAULT NULL,
  `id_trecho_viagem` mediumint unsigned DEFAULT NULL,
  `data_emissao` date DEFAULT NULL,
  `valor_documento` decimal(10,2) DEFAULT NULL,
  `valor_glosa` decimal(10,2) NOT NULL,
  `valor_liquido` decimal(10,2) NOT NULL,
  `valor_restituicao` decimal(10,2) DEFAULT NULL,
  `tipo_documento` tinyint unsigned NOT NULL DEFAULT '0' COMMENT '0: Nota Fiscal; 1: Recibo; 2: Despesa no Exterior',
  `tipo_link` tinyint unsigned NOT NULL DEFAULT '0' COMMENT '0: Sem Arquivo; 1: Recibo; 2: Nota Fiscal Eletronica',
  `numero_documento` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `hash` varbinary(100) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  KEY `idx_id_fornecedor` (`id_fornecedor`) USING BTREE,
  KEY `idx_id_cf_deputado` (`id_cf_deputado`) USING BTREE,
  KEY `id_cf_mandato` (`id_cf_mandato`) USING BTREE,
  KEY `id_cf_despesa_tipo` (`id_cf_despesa_tipo`) USING BTREE,
  KEY `id_cf_especificacao` (`id_cf_especificacao`) USING BTREE,
  KEY `id_legislatura` (`id_cf_legislatura`),
  KEY `ano` (`ano`),
  KEY `mes` (`mes`)
) ENGINE=InnoDB AUTO_INCREMENT=11201357 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_despesa_53
CREATE TABLE IF NOT EXISTS `cf_despesa_53` (
  `id` bigint unsigned NOT NULL,
  `id_cf_deputado` mediumint unsigned NOT NULL,
  `id_cf_despesa_tipo` smallint unsigned NOT NULL,
  `id_fornecedor` mediumint unsigned NOT NULL,
  `ano_mes` mediumint unsigned NOT NULL,
  `data_emissao` date DEFAULT NULL,
  `valor_liquido` decimal(10,2) NOT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  KEY `idx_id_fornecedor` (`id_fornecedor`) USING BTREE,
  KEY `idx_id_cf_deputado` (`id_cf_deputado`) USING BTREE,
  KEY `id_cf_despesa_tipo` (`id_cf_despesa_tipo`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_despesa_54
CREATE TABLE IF NOT EXISTS `cf_despesa_54` (
  `id` bigint unsigned NOT NULL,
  `id_cf_deputado` mediumint unsigned NOT NULL,
  `id_cf_despesa_tipo` smallint unsigned NOT NULL,
  `id_fornecedor` mediumint unsigned NOT NULL,
  `ano_mes` mediumint unsigned NOT NULL,
  `data_emissao` date DEFAULT NULL,
  `valor_liquido` decimal(10,2) NOT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  KEY `idx_id_fornecedor` (`id_fornecedor`) USING BTREE,
  KEY `idx_id_cf_deputado` (`id_cf_deputado`) USING BTREE,
  KEY `id_cf_despesa_tipo` (`id_cf_despesa_tipo`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_despesa_55
CREATE TABLE IF NOT EXISTS `cf_despesa_55` (
  `id` bigint unsigned NOT NULL,
  `id_cf_deputado` mediumint unsigned NOT NULL,
  `id_cf_despesa_tipo` smallint unsigned NOT NULL,
  `id_fornecedor` mediumint unsigned NOT NULL,
  `ano_mes` mediumint unsigned NOT NULL,
  `data_emissao` date DEFAULT NULL,
  `valor_liquido` decimal(10,2) NOT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  KEY `idx_id_fornecedor` (`id_fornecedor`) USING BTREE,
  KEY `idx_id_cf_deputado` (`id_cf_deputado`) USING BTREE,
  KEY `id_cf_despesa_tipo` (`id_cf_despesa_tipo`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_despesa_56
CREATE TABLE IF NOT EXISTS `cf_despesa_56` (
  `id` bigint unsigned NOT NULL,
  `id_cf_deputado` mediumint unsigned NOT NULL,
  `id_cf_despesa_tipo` smallint unsigned NOT NULL,
  `id_fornecedor` mediumint unsigned NOT NULL,
  `ano_mes` mediumint unsigned NOT NULL,
  `data_emissao` date DEFAULT NULL,
  `valor_liquido` decimal(10,2) NOT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  KEY `idx_id_fornecedor` (`id_fornecedor`) USING BTREE,
  KEY `idx_id_cf_deputado` (`id_cf_deputado`) USING BTREE,
  KEY `id_cf_despesa_tipo` (`id_cf_despesa_tipo`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_despesa_57
CREATE TABLE IF NOT EXISTS `cf_despesa_57` (
  `id` bigint unsigned NOT NULL,
  `id_cf_deputado` mediumint unsigned NOT NULL,
  `id_cf_despesa_tipo` smallint unsigned NOT NULL,
  `id_fornecedor` mediumint unsigned NOT NULL,
  `ano_mes` mediumint unsigned NOT NULL,
  `data_emissao` date DEFAULT NULL,
  `valor_liquido` decimal(10,2) NOT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  KEY `idx_id_fornecedor` (`id_fornecedor`) USING BTREE,
  KEY `idx_id_cf_deputado` (`id_cf_deputado`) USING BTREE,
  KEY `id_cf_despesa_tipo` (`id_cf_despesa_tipo`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_despesa_resumo_mensal
CREATE TABLE IF NOT EXISTS `cf_despesa_resumo_mensal` (
  `ano` int unsigned NOT NULL,
  `mes` int unsigned NOT NULL,
  `valor` decimal(10,2) unsigned DEFAULT NULL,
  PRIMARY KEY (`ano`,`mes`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_despesa_tipo
CREATE TABLE IF NOT EXISTS `cf_despesa_tipo` (
  `id` smallint unsigned NOT NULL,
  `descricao` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `descricao` (`descricao`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_especificacao_tipo
CREATE TABLE IF NOT EXISTS `cf_especificacao_tipo` (
  `id_cf_despesa_tipo` smallint unsigned NOT NULL,
  `id_cf_especificacao` tinyint unsigned NOT NULL,
  `descricao` varchar(100) NOT NULL,
  PRIMARY KEY (`id_cf_despesa_tipo`,`id_cf_especificacao`),
  KEY `descricao` (`descricao`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_funcionario
CREATE TABLE IF NOT EXISTS `cf_funcionario` (
  `id` mediumint unsigned NOT NULL AUTO_INCREMENT,
  `chave` varchar(45) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_as_cs NOT NULL,
  `nome` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `processado` tinyint NOT NULL DEFAULT '0',
  `controle` char(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `chave` (`chave`) USING BTREE,
  KEY `idx_nome` (`nome`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=89281 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_funcionario_area_atuacao
CREATE TABLE IF NOT EXISTS `cf_funcionario_area_atuacao` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `nome` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `nome` (`nome`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=32 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_funcionario_cargo
CREATE TABLE IF NOT EXISTS `cf_funcionario_cargo` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `nome` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `nome` (`nome`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=33 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_funcionario_contratacao
CREATE TABLE IF NOT EXISTS `cf_funcionario_contratacao` (
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
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `cf_secretario_contratacao_unique` (`id_cf_funcionario`,`id_cf_deputado`,`periodo_de`) USING BTREE,
  KEY `FK_cf_funcionario_contratacao_cf_deputado` (`id_cf_deputado`) USING BTREE,
  KEY `FK_cf_funcionario_contratacao_cf_funcionario_grupo_funcional` (`id_cf_funcionario_grupo_funcional`) USING BTREE,
  KEY `FK_cf_funcionario_contratacao_cf_funcionario_cargo` (`id_cf_funcionario_cargo`) USING BTREE,
  KEY `FK_cf_funcionario_contratacao_cf_funcionario_nivel` (`id_cf_funcionario_nivel`) USING BTREE,
  KEY `FK_cf_funcionario_contratacao_cf_funcionario_area_atuacao` (`id_cf_funcionario_area_atuacao`) USING BTREE,
  KEY `FK_cf_funcionario_contratacao_cf_funcionario_local_trabalho` (`id_cf_funcionario_local_trabalho`) USING BTREE,
  KEY `FK_cf_funcionario_contratacao_cf_funcionario_situacao` (`id_cf_funcionario_situacao`) USING BTREE,
  KEY `FK_cf_funcionario_contratacao_cf_funcionario_funcao_comissionada` (`id_cf_funcionario_funcao_comissionada`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=142802 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_funcionario_funcao_comissionada
CREATE TABLE IF NOT EXISTS `cf_funcionario_funcao_comissionada` (
  `id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `nome` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `nome` (`nome`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=512 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_funcionario_grupo_funcional
CREATE TABLE IF NOT EXISTS `cf_funcionario_grupo_funcional` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `nome` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `nome` (`nome`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_funcionario_local_trabalho
CREATE TABLE IF NOT EXISTS `cf_funcionario_local_trabalho` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `nome` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `nome` (`nome`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=23 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_funcionario_nivel
CREATE TABLE IF NOT EXISTS `cf_funcionario_nivel` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `nome` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `nome` (`nome`)
) ENGINE=InnoDB AUTO_INCREMENT=78 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_funcionario_remuneracao
CREATE TABLE IF NOT EXISTS `cf_funcionario_remuneracao` (
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
  `contratacao` date DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `id_cf_secretario` (`id_cf_funcionario`,`referencia`,`tipo`) USING BTREE,
  KEY `FK_cf_funcionario_remuneracao_cf_funcionario_contratacao` (`id_cf_funcionario_contratacao`) USING BTREE,
  KEY `FK_cf_funcionario_remuneracao_cf_deputado` (`id_cf_deputado`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=1079473 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_funcionario_situacao
CREATE TABLE IF NOT EXISTS `cf_funcionario_situacao` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `nome` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE KEY `nome` (`nome`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_funcionario_tipo_folha
CREATE TABLE IF NOT EXISTS `cf_funcionario_tipo_folha` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `nome` varchar(50) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_gabinete
CREATE TABLE IF NOT EXISTS `cf_gabinete` (
  `id` smallint unsigned NOT NULL,
  `nome` varchar(50) DEFAULT NULL,
  `predio` varchar(50) DEFAULT NULL,
  `andar` tinyint unsigned DEFAULT NULL,
  `sala` varchar(50) DEFAULT NULL,
  `telefone` varchar(20) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `nome` (`nome`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_legislatura
CREATE TABLE IF NOT EXISTS `cf_legislatura` (
  `id` tinyint unsigned NOT NULL,
  `ano` smallint unsigned DEFAULT NULL,
  `inicio` mediumint unsigned DEFAULT NULL,
  `final` mediumint unsigned DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `ano` (`ano`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_mandato
CREATE TABLE IF NOT EXISTS `cf_mandato` (
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
) ENGINE=InnoDB AUTO_INCREMENT=10915 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_secretario
CREATE TABLE IF NOT EXISTS `cf_secretario` (
  `id` mediumint unsigned NOT NULL AUTO_INCREMENT,
  `id_cf_deputado` mediumint unsigned NOT NULL DEFAULT '0',
  `nome` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `periodo` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `cargo` varchar(45) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `valor_bruto` decimal(10,2) DEFAULT NULL,
  `valor_liquido` decimal(10,2) DEFAULT NULL,
  `valor_outros` decimal(10,2) DEFAULT NULL,
  `link` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `referencia` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `em_exercicio` bit(1) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  KEY `idx_id_cf_deputado` (`id_cf_deputado`) USING BTREE,
  KEY `idx_nome` (`nome`) USING BTREE,
  KEY `idx_link` (`link`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=12816 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_secretario_historico
CREATE TABLE IF NOT EXISTS `cf_secretario_historico` (
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_senador_verba_gabinete
CREATE TABLE IF NOT EXISTS `cf_senador_verba_gabinete` (
  `id_sf_senador` int NOT NULL,
  `ano` smallint NOT NULL,
  `mes` smallint NOT NULL,
  `valor` decimal(10,2) NOT NULL,
  `percentual` decimal(10,2) DEFAULT NULL,
  UNIQUE KEY `id_sf_senador_ano_mes` (`id_sf_senador`,`ano`,`mes`) USING BTREE,
  KEY `id_sf_senador` (`id_sf_senador`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Exportação de dados foi desmarcado.

-- Copiando estrutura para tabela ops.cf_sessao
CREATE TABLE IF NOT EXISTS `cf_sessao` (
  `id` mediumint unsigned NOT NULL AUTO_INCREMENT,
  `id_legislatura` tinyint unsigned NOT NULL,
  `data` date NOT NULL,
  `inicio` datetime NOT NULL,
  `tipo` tinyint unsigned NOT NULL,
  `numero` varchar(45) DEFAULT NULL,
  `presencas` smallint unsigned NOT NULL DEFAULT '0',
  `ausencias` smallint unsigned NOT NULL DEFAULT '0',
  `ausencias_justificadas` smallint unsigned NOT NULL DEFAULT '0',
  `checksum` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `id_legislatura` (`id_legislatura`),
  KEY `data` (`data`)
) ENGINE=InnoDB AUTO_INCREMENT=4772 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci ROW_FORMAT=DYNAMIC;

-- Exportação de dados foi desmarcado.

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

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
