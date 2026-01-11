-- Missing Foreign Keys for OPS Database
-- This file contains all missing foreign key constraints identified in the schema

-- ========================================
-- FORNECEDOR SCHEMA FOREIGN KEYS
-- ========================================

-- FK for fornecedor_atividade_secundaria
ALTER TABLE "fornecedor"."fornecedor_atividade_secundaria" 
ADD CONSTRAINT "fk_fornecedor_atividade_secundaria_fornecedor" 
FOREIGN KEY ("id_fornecedor") REFERENCES "fornecedor"."fornecedor" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "fornecedor"."fornecedor_atividade_secundaria" 
ADD CONSTRAINT "fk_fornecedor_atividade_secundaria_atividade" 
FOREIGN KEY ("id_fornecedor_atividade") REFERENCES "fornecedor"."fornecedor_atividade" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for fornecedor_info
ALTER TABLE "fornecedor"."fornecedor_info" 
ADD CONSTRAINT "fk_fornecedor_info_fornecedor" 
FOREIGN KEY ("id_fornecedor") REFERENCES "fornecedor"."fornecedor" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "fornecedor"."fornecedor_info" 
ADD CONSTRAINT "fk_fornecedor_info_atividade_principal" 
FOREIGN KEY ("id_fornecedor_atividade_principal") REFERENCES "fornecedor"."fornecedor_atividade" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "fornecedor"."fornecedor_info" 
ADD CONSTRAINT "fk_fornecedor_info_natureza_juridica" 
FOREIGN KEY ("id_fornecedor_natureza_juridica") REFERENCES "fornecedor"."fornecedor_natureza_juridica" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for fornecedor_socio
ALTER TABLE "fornecedor"."fornecedor_socio" 
ADD CONSTRAINT "fk_fornecedor_socio_fornecedor" 
FOREIGN KEY ("id_fornecedor") REFERENCES "fornecedor"."fornecedor" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "fornecedor"."fornecedor_socio" 
ADD CONSTRAINT "fk_fornecedor_socio_faixa_etaria" 
FOREIGN KEY ("id_fornecedor_faixa_etaria") REFERENCES "fornecedor"."fornecedor_faixa_etaria" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "fornecedor"."fornecedor_socio" 
ADD CONSTRAINT "fk_fornecedor_socio_qualificacao" 
FOREIGN KEY ("id_fornecedor_socio_qualificacao") REFERENCES "fornecedor"."fornecedor_socio_qualificacao" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "fornecedor"."fornecedor_socio" 
ADD CONSTRAINT "fk_fornecedor_socio_representante_qualificacao" 
FOREIGN KEY ("id_fornecedor_socio_representante_qualificacao") REFERENCES "fornecedor"."fornecedor_socio_qualificacao" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for forcecedor_cnpj_incorreto (note: there's a typo in the table name)
ALTER TABLE "fornecedor"."forcecedor_cnpj_incorreto" 
ADD CONSTRAINT "fk_forcecedor_cnpj_incorreto_fornecedor_incorreto" 
FOREIGN KEY ("id_fornecedor_incorreto") REFERENCES "fornecedor"."fornecedor" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "fornecedor"."forcecedor_cnpj_incorreto" 
ADD CONSTRAINT "fk_forcecedor_cnpj_incorreto_fornecedor_correto" 
FOREIGN KEY ("id_fornecedor_correto") REFERENCES "fornecedor"."fornecedor" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- ========================================
-- CAMARA FEDERAL SCHEMA FOREIGN KEYS
-- ========================================

-- FK for cf_deputado
ALTER TABLE "camara"."cf_deputado" 
ADD CONSTRAINT "fk_cf_deputado_partido" 
FOREIGN KEY ("id_partido") REFERENCES "public"."partido" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "camara"."cf_deputado" 
ADD CONSTRAINT "fk_cf_deputado_estado" 
FOREIGN KEY ("id_estado") REFERENCES "public"."estado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "camara"."cf_deputado" 
ADD CONSTRAINT "fk_cf_deputado_estado_nascimento" 
FOREIGN KEY ("id_estado_nascimento") REFERENCES "public"."estado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "camara"."cf_deputado" 
ADD CONSTRAINT "fk_cf_deputado_gabinete" 
FOREIGN KEY ("id_cf_gabinete") REFERENCES "camara"."cf_gabinete" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for cf_deputado_auxilio_moradia
ALTER TABLE "camara"."cf_deputado_auxilio_moradia" 
ADD CONSTRAINT "fk_cf_deputado_auxilio_moradia_deputado" 
FOREIGN KEY ("id_cf_deputado") REFERENCES "camara"."cf_deputado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for cf_deputado_cota_parlamentar
ALTER TABLE "camara"."cf_deputado_cota_parlamentar" 
ADD CONSTRAINT "fk_cf_deputado_cota_parlamentar_deputado" 
FOREIGN KEY ("id_cf_deputado") REFERENCES "camara"."cf_deputado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for cf_despesa
ALTER TABLE "camara"."cf_despesa" 
ADD CONSTRAINT "fk_cf_despesa_deputado" 
FOREIGN KEY ("id_cf_deputado") REFERENCES "camara"."cf_deputado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "camara"."cf_despesa" 
ADD CONSTRAINT "fk_cf_despesa_legislatura" 
FOREIGN KEY ("id_cf_legislatura") REFERENCES "camara"."cf_legislatura" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "camara"."cf_despesa" 
ADD CONSTRAINT "fk_cf_despesa_mandato" 
FOREIGN KEY ("id_cf_mandato") REFERENCES "camara"."cf_mandato" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "camara"."cf_despesa" 
ADD CONSTRAINT "fk_cf_despesa_tipo" 
FOREIGN KEY ("id_cf_despesa_tipo") REFERENCES "camara"."cf_despesa_tipo" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "camara"."cf_despesa" 
ADD CONSTRAINT "fk_cf_despesa_especificacao" 
FOREIGN KEY ("id_cf_especificacao") REFERENCES "camara"."cf_especificacao_tipo" ("id_cf_especificacao") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "camara"."cf_despesa" 
ADD CONSTRAINT "fk_cf_despesa_fornecedor" 
FOREIGN KEY ("id_fornecedor") REFERENCES "fornecedor"."fornecedor" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "camara"."cf_despesa" 
ADD CONSTRAINT "fk_cf_despesa_passageiro" 
FOREIGN KEY ("id_passageiro") REFERENCES "public"."pessoa" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "camara"."cf_despesa" 
ADD CONSTRAINT "fk_cf_despesa_trecho_viagem" 
FOREIGN KEY ("id_trecho_viagem") REFERENCES "public"."trecho_viagem" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for cf_especificacao_tipo
ALTER TABLE "camara"."cf_especificacao_tipo" 
ADD CONSTRAINT "fk_cf_especificacao_tipo_despesa_tipo" 
FOREIGN KEY ("id_cf_despesa_tipo") REFERENCES "camara"."cf_despesa_tipo" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for cf_funcionario_contratacao
ALTER TABLE "camara"."cf_funcionario_contratacao" 
ADD CONSTRAINT "fk_cf_funcionario_contratacao_deputado" 
FOREIGN KEY ("id_cf_deputado") REFERENCES "camara"."cf_deputado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "camara"."cf_funcionario_contratacao" 
ADD CONSTRAINT "fk_cf_funcionario_contratacao_funcionario" 
FOREIGN KEY ("id_cf_funcionario") REFERENCES "camara"."cf_funcionario" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "camara"."cf_funcionario_contratacao" 
ADD CONSTRAINT "fk_cf_funcionario_contratacao_grupo_funcional" 
FOREIGN KEY ("id_cf_funcionario_grupo_funcional") REFERENCES "camara"."cf_funcionario_grupo_funcional" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "camara"."cf_funcionario_contratacao" 
ADD CONSTRAINT "fk_cf_funcionario_contratacao_cargo" 
FOREIGN KEY ("id_cf_funcionario_cargo") REFERENCES "camara"."cf_funcionario_cargo" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "camara"."cf_funcionario_contratacao" 
ADD CONSTRAINT "fk_cf_funcionario_contratacao_nivel" 
FOREIGN KEY ("id_cf_funcionario_nivel") REFERENCES "camara"."cf_funcionario_nivel" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "camara"."cf_funcionario_contratacao" 
ADD CONSTRAINT "fk_cf_funcionario_contratacao_funcao_comissionada" 
FOREIGN KEY ("id_cf_funcionario_funcao_comissionada") REFERENCES "camara"."cf_funcionario_funcao_comissionada" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "camara"."cf_funcionario_contratacao" 
ADD CONSTRAINT "fk_cf_funcionario_contratacao_area_atuacao" 
FOREIGN KEY ("id_cf_funcionario_area_atuacao") REFERENCES "camara"."cf_funcionario_area_atuacao" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "camara"."cf_funcionario_contratacao" 
ADD CONSTRAINT "fk_cf_funcionario_contratacao_local_trabalho" 
FOREIGN KEY ("id_cf_funcionario_local_trabalho") REFERENCES "camara"."cf_funcionario_local_trabalho" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "camara"."cf_funcionario_contratacao" 
ADD CONSTRAINT "fk_cf_funcionario_contratacao_situacao" 
FOREIGN KEY ("id_cf_funcionario_situacao") REFERENCES "camara"."cf_funcionario_situacao" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for cf_funcionario_remuneracao
ALTER TABLE "camara"."cf_funcionario_remuneracao" 
ADD CONSTRAINT "fk_cf_funcionario_remuneracao_funcionario" 
FOREIGN KEY ("id_cf_funcionario") REFERENCES "camara"."cf_funcionario" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "camara"."cf_funcionario_remuneracao" 
ADD CONSTRAINT "fk_cf_funcionario_remuneracao_contratacao" 
FOREIGN KEY ("id_cf_funcionario_contratacao") REFERENCES "camara"."cf_funcionario_contratacao" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "camara"."cf_funcionario_remuneracao" 
ADD CONSTRAINT "fk_cf_funcionario_remuneracao_deputado" 
FOREIGN KEY ("id_cf_deputado") REFERENCES "camara"."cf_deputado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for cf_mandato
ALTER TABLE "camara"."cf_mandato" 
ADD CONSTRAINT "fk_cf_mandato_deputado" 
FOREIGN KEY ("id_cf_deputado") REFERENCES "camara"."cf_deputado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "camara"."cf_mandato" 
ADD CONSTRAINT "fk_cf_mandato_legislatura" 
FOREIGN KEY ("id_legislatura") REFERENCES "camara"."cf_legislatura" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "camara"."cf_mandato" 
ADD CONSTRAINT "fk_cf_mandato_estado" 
FOREIGN KEY ("id_estado") REFERENCES "public"."estado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "camara"."cf_mandato" 
ADD CONSTRAINT "fk_cf_mandato_partido" 
FOREIGN KEY ("id_partido") REFERENCES "public"."partido" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for cf_secretario
ALTER TABLE "camara"."cf_secretario" 
ADD CONSTRAINT "fk_cf_secretario_deputado" 
FOREIGN KEY ("id_cf_deputado") REFERENCES "camara"."cf_deputado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for cf_secretario_historico
ALTER TABLE "camara"."cf_secretario_historico" 
ADD CONSTRAINT "fk_cf_secretario_historico_deputado" 
FOREIGN KEY ("id_cf_deputado") REFERENCES "camara"."cf_deputado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for cf_senador_verba_gabinete
ALTER TABLE "camara"."cf_senador_verba_gabinete" 
ADD CONSTRAINT "fk_cf_senador_verba_gabinete_senador" 
FOREIGN KEY ("id_sf_senador") REFERENCES "senado"."sf_senador" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for cf_sessao
ALTER TABLE "camara"."cf_sessao" 
ADD CONSTRAINT "fk_cf_sessao_legislatura" 
FOREIGN KEY ("id_legislatura") REFERENCES "camara"."cf_legislatura" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for cf_sessao_presenca
ALTER TABLE "camara"."cf_sessao_presenca" 
ADD CONSTRAINT "fk_cf_sessao_presenca_sessao" 
FOREIGN KEY ("id_cf_sessao") REFERENCES "camara"."cf_sessao" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "camara"."cf_sessao_presenca" 
ADD CONSTRAINT "fk_cf_sessao_presenca_deputado" 
FOREIGN KEY ("id_cf_deputado") REFERENCES "camara"."cf_deputado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for cf_deputado_imovel_funcional
ALTER TABLE "camara"."cf_deputado_imovel_funcional" 
ADD CONSTRAINT "fk_cf_deputado_imovel_funcional_deputado" 
FOREIGN KEY ("id_cf_deputado") REFERENCES "camara"."cf_deputado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for cf_deputado_missao_oficial
ALTER TABLE "camara"."cf_deputado_missao_oficial" 
ADD CONSTRAINT "fk_cf_deputado_missao_oficial_deputado" 
FOREIGN KEY ("id_cf_deputado") REFERENCES "camara"."cf_deputado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for cf_deputado_remuneracao
ALTER TABLE "camara"."cf_deputado_remuneracao" 
ADD CONSTRAINT "fk_cf_deputado_remuneracao_deputado" 
FOREIGN KEY ("id_cf_deputado") REFERENCES "camara"."cf_deputado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for cf_deputado_verba_gabinete
ALTER TABLE "camara"."cf_deputado_verba_gabinete" 
ADD CONSTRAINT "fk_cf_deputado_verba_gabinete_deputado" 
FOREIGN KEY ("id_cf_deputado") REFERENCES "camara"."cf_deputado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- ========================================
-- SENADO FEDERAL SCHEMA FOREIGN KEYS
-- ========================================

-- FK for sf_despesa
ALTER TABLE "senado"."sf_despesa" 
ADD CONSTRAINT "fk_sf_despesa_senador" 
FOREIGN KEY ("id_sf_senador") REFERENCES "senado"."sf_senador" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "senado"."sf_despesa" 
ADD CONSTRAINT "fk_sf_despesa_tipo" 
FOREIGN KEY ("id_sf_despesa_tipo") REFERENCES "senado"."sf_despesa_tipo" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "senado"."sf_despesa" 
ADD CONSTRAINT "fk_sf_despesa_fornecedor" 
FOREIGN KEY ("id_fornecedor") REFERENCES "fornecedor"."fornecedor" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for sf_lotacao
ALTER TABLE "senado"."sf_lotacao" 
ADD CONSTRAINT "fk_sf_lotacao_senador" 
FOREIGN KEY ("id_senador") REFERENCES "senado"."sf_senador" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for sf_mandato_exercicio
ALTER TABLE "senado"."sf_mandato_exercicio" 
ADD CONSTRAINT "fk_sf_mandato_exercicio_senador" 
FOREIGN KEY ("id_sf_senador") REFERENCES "senado"."sf_senador" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "senado"."sf_mandato_exercicio" 
ADD CONSTRAINT "fk_sf_mandato_exercicio_mandato" 
FOREIGN KEY ("id_sf_mandato") REFERENCES "senado"."sf_mandato" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "senado"."sf_mandato_exercicio" 
ADD CONSTRAINT "fk_sf_mandato_exercicio_motivo_afastamento" 
FOREIGN KEY ("id_sf_motivo_afastamento") REFERENCES "senado"."sf_motivo_afastamento" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for sf_mandato_legislatura
ALTER TABLE "senado"."sf_mandato_legislatura" 
ADD CONSTRAINT "fk_sf_mandato_legislatura_mandato" 
FOREIGN KEY ("id_sf_mandato") REFERENCES "senado"."sf_mandato" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "senado"."sf_mandato_legislatura" 
ADD CONSTRAINT "fk_sf_mandato_legislatura_legislatura" 
FOREIGN KEY ("id_sf_legislatura") REFERENCES "senado"."sf_legislatura" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "senado"."sf_mandato_legislatura" 
ADD CONSTRAINT "fk_sf_mandato_legislatura_senador" 
FOREIGN KEY ("id_sf_senador") REFERENCES "senado"."sf_senador" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for sf_remuneracao
ALTER TABLE "senado"."sf_remuneracao" 
ADD CONSTRAINT "fk_sf_remuneracao_vinculo" 
FOREIGN KEY ("id_vinculo") REFERENCES "senado"."sf_vinculo" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "senado"."sf_remuneracao" 
ADD CONSTRAINT "fk_sf_remuneracao_categoria" 
FOREIGN KEY ("id_categoria") REFERENCES "senado"."sf_categoria" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "senado"."sf_remuneracao" 
ADD CONSTRAINT "fk_sf_remuneracao_cargo" 
FOREIGN KEY ("id_cargo") REFERENCES "senado"."sf_cargo" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "senado"."sf_remuneracao" 
ADD CONSTRAINT "fk_sf_remuneracao_referencia_cargo" 
FOREIGN KEY ("id_referencia_cargo") REFERENCES "senado"."sf_referencia_cargo" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "senado"."sf_remuneracao" 
ADD CONSTRAINT "fk_sf_remuneracao_funcao" 
FOREIGN KEY ("id_simbolo_funcao") REFERENCES "senado"."sf_funcao" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "senado"."sf_remuneracao" 
ADD CONSTRAINT "fk_sf_remuneracao_lotacao" 
FOREIGN KEY ("id_lotacao") REFERENCES "senado"."sf_lotacao" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "senado"."sf_remuneracao" 
ADD CONSTRAINT "fk_sf_remuneracao_tipo_folha" 
FOREIGN KEY ("id_tipo_folha") REFERENCES "senado"."sf_tipo_folha" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for sf_secretario
ALTER TABLE "senado"."sf_secretario" 
ADD CONSTRAINT "fk_sf_secretario_senador" 
FOREIGN KEY ("id_senador") REFERENCES "senado"."sf_senador" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "senado"."sf_secretario" 
ADD CONSTRAINT "fk_sf_secretario_funcao" 
FOREIGN KEY ("id_funcao") REFERENCES "senado"."sf_funcao" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "senado"."sf_secretario" 
ADD CONSTRAINT "fk_sf_secretario_cargo" 
FOREIGN KEY ("id_cargo") REFERENCES "senado"."sf_cargo" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "senado"."sf_secretario" 
ADD CONSTRAINT "fk_sf_secretario_vinculo" 
FOREIGN KEY ("id_vinculo") REFERENCES "senado"."sf_vinculo" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "senado"."sf_secretario" 
ADD CONSTRAINT "fk_sf_secretario_categoria" 
FOREIGN KEY ("id_categoria") REFERENCES "senado"."sf_categoria" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "senado"."sf_secretario" 
ADD CONSTRAINT "fk_sf_secretario_referencia_cargo" 
FOREIGN KEY ("id_referencia_cargo") REFERENCES "senado"."sf_referencia_cargo" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "senado"."sf_secretario" 
ADD CONSTRAINT "fk_sf_secretario_especialidade" 
FOREIGN KEY ("id_especialidade") REFERENCES "senado"."sf_cargo" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "senado"."sf_secretario" 
ADD CONSTRAINT "fk_sf_secretario_lotacao" 
FOREIGN KEY ("id_lotacao") REFERENCES "senado"."sf_lotacao" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for sf_senador
ALTER TABLE "senado"."sf_senador" 
ADD CONSTRAINT "fk_sf_senador_partido" 
FOREIGN KEY ("id_partido") REFERENCES "public"."partido" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "senado"."sf_senador" 
ADD CONSTRAINT "fk_sf_senador_estado" 
FOREIGN KEY ("id_estado") REFERENCES "public"."estado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "senado"."sf_senador" 
ADD CONSTRAINT "fk_sf_senador_estado_naturalidade" 
FOREIGN KEY ("id_estado_naturalidade") REFERENCES "public"."estado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for sf_senador_campeao_gasto
ALTER TABLE "senado"."sf_senador_campeao_gasto" 
ADD CONSTRAINT "fk_sf_senador_campeao_gasto_senador" 
FOREIGN KEY ("id_sf_senador") REFERENCES "senado"."sf_senador" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for sf_senador_historico_academico
ALTER TABLE "senado"."sf_senador_historico_academico" 
ADD CONSTRAINT "fk_sf_senador_historico_academico_senador" 
FOREIGN KEY ("id_sf_senador") REFERENCES "senado"."sf_senador" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for sf_senador_partido
ALTER TABLE "senado"."sf_senador_partido" 
ADD CONSTRAINT "fk_sf_senador_partido_senador" 
FOREIGN KEY ("id_sf_senador") REFERENCES "senado"."sf_senador" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "senado"."sf_senador_partido" 
ADD CONSTRAINT "fk_sf_senador_partido_partido" 
FOREIGN KEY ("id_partido") REFERENCES "public"."partido" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for sf_senador_profissao
ALTER TABLE "senado"."sf_senador_profissao" 
ADD CONSTRAINT "fk_sf_senador_profissao_senador" 
FOREIGN KEY ("id_sf_senador") REFERENCES "senado"."sf_senador" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "senado"."sf_senador_profissao" 
ADD CONSTRAINT "fk_sf_senador_profissao_profissao" 
FOREIGN KEY ("id_profissao") REFERENCES "public"."profissao" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- ========================================
-- ASSEMBLEIAS LEGISLATIVAS SCHEMA FOREIGN KEYS
-- ========================================

-- FK for cl_deputado
ALTER TABLE "assembleias"."cl_deputado" 
ADD CONSTRAINT "fk_cl_deputado_partido" 
FOREIGN KEY ("id_partido") REFERENCES "public"."partido" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "assembleias"."cl_deputado" 
ADD CONSTRAINT "fk_cl_deputado_estado" 
FOREIGN KEY ("id_estado") REFERENCES "public"."estado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for cl_despesa
ALTER TABLE "assembleias"."cl_despesa" 
ADD CONSTRAINT "fk_cl_despesa_deputado" 
FOREIGN KEY ("id_cl_deputado") REFERENCES "assembleias"."cl_deputado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "assembleias"."cl_despesa" 
ADD CONSTRAINT "fk_cl_despesa_tipo" 
FOREIGN KEY ("id_cl_despesa_tipo") REFERENCES "assembleias"."cl_despesa_tipo" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "assembleias"."cl_despesa" 
ADD CONSTRAINT "fk_cl_despesa_especificacao" 
FOREIGN KEY ("id_cl_despesa_especificacao") REFERENCES "assembleias"."cl_despesa_especificacao" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "assembleias"."cl_despesa" 
ADD CONSTRAINT "fk_cl_despesa_fornecedor" 
FOREIGN KEY ("id_fornecedor") REFERENCES "fornecedor"."fornecedor" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for cl_despesa_especificacao
ALTER TABLE "assembleias"."cl_despesa_especificacao" 
ADD CONSTRAINT "fk_cl_despesa_especificacao_tipo" 
FOREIGN KEY ("id_cl_despesa_tipo") REFERENCES "assembleias"."cl_despesa_tipo" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- ========================================
-- TSE SCHEMA FOREIGN KEYS
-- ========================================

-- FK for tse_eleicao_candidatura
ALTER TABLE "tse"."tse_eleicao_candidatura" 
ADD CONSTRAINT "fk_tse_eleicao_candidatura_candidato" 
FOREIGN KEY ("id_eleicao_candidato") REFERENCES "tse"."tse_eleicao_candidato" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "tse"."tse_eleicao_candidatura" 
ADD CONSTRAINT "fk_tse_eleicao_candidatura_candidato_vice" 
FOREIGN KEY ("id_eleicao_candidato_vice") REFERENCES "tse"."tse_eleicao_candidato" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "tse"."tse_eleicao_candidatura" 
ADD CONSTRAINT "fk_tse_eleicao_candidatura_cargo" 
FOREIGN KEY ("cargo") REFERENCES "tse"."tse_eleicao_cargo" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- FK for tse_eleicao_doacao
ALTER TABLE "tse"."tse_eleicao_doacao" 
ADD CONSTRAINT "fk_tse_eleicao_doacao_cargo" 
FOREIGN KEY ("id_eleicao_cargo") REFERENCES "tse"."tse_eleicao_cargo" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "tse"."tse_eleicao_doacao" 
ADD CONSTRAINT "fk_tse_eleicao_doacao_candidato" 
FOREIGN KEY ("id_eleicao_candidadto") REFERENCES "tse"."tse_eleicao_candidato" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- ========================================
-- COMUM SCHEMA FOREIGN KEYS
-- ========================================

-- FK for pessoa_new
ALTER TABLE "public"."pessoa_new" 
ADD CONSTRAINT "fk_pessoa_new_nacionalidade" 
FOREIGN KEY ("id_nacionalidade") REFERENCES "public"."estado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "public"."pessoa_new" 
ADD CONSTRAINT "fk_pessoa_new_estado_nascimento" 
FOREIGN KEY ("id_estado_nascimento") REFERENCES "public"."estado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "public"."pessoa_new" 
ADD CONSTRAINT "fk_pessoa_new_genero" 
FOREIGN KEY ("id_genero") REFERENCES "public"."estado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "public"."pessoa_new" 
ADD CONSTRAINT "fk_pessoa_new_etnia" 
FOREIGN KEY ("id_etnia") REFERENCES "public"."estado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "public"."pessoa_new" 
ADD CONSTRAINT "fk_pessoa_new_estado_civil" 
FOREIGN KEY ("id_estado_civil") REFERENCES "public"."estado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "public"."pessoa_new" 
ADD CONSTRAINT "fk_pessoa_new_grau_instrucao" 
FOREIGN KEY ("id_grau_instrucao") REFERENCES "public"."estado" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;

ALTER TABLE "public"."pessoa_new" 
ADD CONSTRAINT "fk_pessoa_new_ocupacao" 
FOREIGN KEY ("id_ocupacao") REFERENCES "public"."profissao" ("id") ON DELETE NO ACTION ON UPDATE NO ACTION;
