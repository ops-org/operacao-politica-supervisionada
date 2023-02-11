using OPS.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace OPS.Core.DAO
{
    public class FornecedorRepository
    {

        public dynamic Consulta(int id)
        {
            using (AppDb banco = new AppDb())
            {
                banco.AddParameter("id", id);

                using (var reader = banco.ExecuteReader(
                        @"SELECT 
							pj.id as id_fornecedor
							, pj.cnpj_cpf
							, pji.tipo
							, IFNULL(pji.nome, pj.nome) as nome
							, pj.doador
							, pji.data_de_abertura
							, pji.nome_fantasia
							, CONCAT(a.codigo,' - ',a.descricao) as atividade_principal
							, CONCAT(nj.codigo,' - ',nj.descricao) as natureza_juridica
							, pji.logradouro
							, pji.numero
							, pji.complemento
							, pji.cep
							, pji.bairro
							, pji.municipio as cidade
							, pji.estado
							, pji.situacao_cadastral
							, pji.data_da_situacao_cadastral
							, pji.motivo_situacao_cadastral
							, pji.situacao_especial
							, pji.data_situacao_especial
							, pji.endereco_eletronico
							, pji.telefone1
							, pji.telefone2
							, pji.ente_federativo_responsavel
							, pji.obtido_em
							, pji.capital_social
						FROM fornecedor pj
						LEFT JOIN fornecedor_info pji on pji.id_fornecedor = pj.id
						LEFT JOIN fornecedor_atividade a on a.id = pji.id_fornecedor_atividade_principal
						LEFT JOIN fornecedor_natureza_juridica nj on nj.id = pji.id_fornecedor_natureza_juridica
						WHERE pj.id = @id;

                        SELECT fa.codigo, fa.descricao
                        FROM fornecedor_atividade_secundaria fas 
                        INNER JOIN fornecedor_atividade fa on fa.id = fas.id_fornecedor_atividade
                        where id_fornecedor = @id;"
                    ))
                {
                    if (reader.Read())
                    {
                        var fornecedor = new
                        {
                            id_fornecedor = reader["id_fornecedor"].ToString(),
                            cnpj_cpf = Utils.FormatCnpjCpf(reader["cnpj_cpf"].ToString()),
                            data_de_abertura = Utils.FormataData(reader["data_de_abertura"]),
                            tipo = reader["tipo"].ToString(),
                            nome = reader["nome"].ToString(),
                            nome_fantasia = reader["nome_fantasia"].ToString(),
                            atividade_principal = reader["atividade_principal"].ToString(),
                            natureza_juridica = reader["natureza_juridica"].ToString(),
                            logradouro = reader["logradouro"].ToString(),
                            numero = reader["numero"].ToString(),
                            complemento = reader["complemento"].ToString(),
                            cep = reader["cep"].ToString(),
                            bairro = reader["bairro"].ToString(),
                            cidade = reader["cidade"].ToString(),
                            estado = reader["estado"].ToString(),
                            situacao_cadastral = reader["situacao_cadastral"].ToString(),
                            data_da_situacao_cadastral = reader["data_da_situacao_cadastral"].ToString().Split(" ")[0],
                            motivo_situacao_cadastral = reader["motivo_situacao_cadastral"].ToString(),
                            situacao_especial = reader["situacao_especial"].ToString(),
                            data_situacao_especial = reader["data_situacao_especial"].ToString(),
                            endereco_eletronico = reader["endereco_eletronico"].ToString(),
                            telefone = reader["telefone1"].ToString(),
                            telefone2 = reader["telefone2"].ToString(),
                            ente_federativo_responsavel = reader["ente_federativo_responsavel"].ToString(),
                            obtido_em = Utils.FormataDataHora(reader["obtido_em"]),
                            capital_social = Utils.FormataValor(reader["capital_social"]),
                            doador = reader["doador"],
                            atividade_secundaria = new List<string>()
                        };

                        reader.NextResult();
                        while (reader.Read())
                        {
                            fornecedor.atividade_secundaria.Add($"{reader["codigo"]} - {reader["descricao"]}");
                        }

                        return fornecedor;
                    }

                    return null;
                }
            }
        }

        public dynamic QuadroSocietario(int id)
        {
            try
            {
                using (AppDb banco = new AppDb())
                {
                    string strSql =
                        @"SELECT
							fs.nome
							, fsq1.descricao as qualificacao
							, fs.nome_representante as nome_representante_legal
							, fsq2.descricao as qualificacao_representante_legal
						FROM fornecedor_socio fs
						LEFT JOIN fornecedor_socio_qualificacao fsq1 on fsq1.id = fs.id_fornecedor_socio_qualificacao
						LEFT JOIN fornecedor_socio_qualificacao fsq2 on fsq2.id = fs.id_fornecedor_socio_representante_qualificacao
						where fs.id_fornecedor = @id";

                    banco.AddParameter("id", id);

                    using (var reader = banco.ExecuteReader(strSql))
                    {
                        List<dynamic> lstRetorno = new List<dynamic>();
                        while (reader.Read())
                        {
                            lstRetorno.Add(new
                            {
                                nome = reader["nome"].ToString(),
                                qualificacao = reader["qualificacao"].ToString(),
                                nome_representante_legal = reader["nome_representante_legal"].ToString(),
                                qualificacao_representante_legal = reader["qualificacao_representante_legal"].ToString()
                            });
                        }

                        return lstRetorno;
                    }
                }
            }
            catch (Exception)
            { } //TODO: logar erro

            return null;
        }

        public dynamic SenadoresMaioresGastos(int id)
        {
            using (AppDb banco = new AppDb())
            {
                var strSql =
                    @"SELECT
						l1.id_sf_senador
						, p.nome as nome_parlamentar
						, l1.valor_total
					FROM (
						select 
							SUM(l.valor) AS valor_total
							, l.id_sf_senador
						from sf_despesa l
						WHERE l.id_fornecedor = @id
						GROUP BY l.id_sf_senador
						ORDER BY valor_total desc
						LIMIT 10
					) l1
					LEFT JOIN sf_senador p ON p.id = l1.id_sf_senador";

                banco.AddParameter("@id", id);

                using (var reader = banco.ExecuteReader(strSql))
                {
                    List<dynamic> lstRetorno = new List<dynamic>();
                    while (reader.Read())
                    {
                        lstRetorno.Add(new
                        {
                            id_sf_senador = reader["id_sf_senador"].ToString(),
                            nome_parlamentar = reader["nome_parlamentar"].ToString(),
                            valor_total = Utils.FormataValor(reader["valor_total"])
                        });
                    }

                    return lstRetorno;
                }
            }
        }

        public dynamic MaioresGastos(int id)
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.Append(@"
SELECT 
    tmp.id, 
    tmp.tipo, 
    tmp.nome_parlamentar, 
    pr.sigla as sigla_partido, 
    e.sigla as sigla_estado, 
    tmp.valor_total
FROM (
	SELECT
		l1.id_cf_deputado AS id
		, p.nome_parlamentar
		, p.id_partido
		, p.id_estado
		, 'Deputado Federal' as tipo
		, l1.valor_total
	FROM (
		select 
			SUM(l.valor_liquido) AS valor_total
			, l.id_cf_deputado
		from cf_despesa l
		WHERE l.id_fornecedor = @id
		GROUP BY l.id_cf_deputado
		ORDER BY valor_total desc
		LIMIT 10
	) l1
	JOIN cf_deputado p ON p.id = l1.id_cf_deputado
	

	UNION ALL

    SELECT
		l1.id_cl_deputado AS id
		, p.nome_parlamentar
		, p.id_partido
		, p.id_estado
		, 'Deputado Estadual' as tipo
		, l1.valor_total
	FROM (
		select 
			SUM(l.valor_liquido) AS valor_total
			, l.id_cl_deputado
		from cl_despesa l
		WHERE l.id_fornecedor = @id
		GROUP BY l.id_cl_deputado
		ORDER BY valor_total desc
		LIMIT 10
	) l1
	JOIN cl_deputado p ON p.id = l1.id_cl_deputado

	UNION ALL

	SELECT
		l1.id_sf_senador AS id
		, p.nome as nome_parlamentar
		, p.id_partido
		, p.id_estado
		, 'Senador' as tipo
		, l1.valor_total
	FROM (
		select 
			SUM(l.valor) AS valor_total
			, l.id_sf_senador
		from sf_despesa l
		WHERE l.id_fornecedor = @id
		GROUP BY l.id_sf_senador
		ORDER BY valor_total desc
		LIMIT 10
	) l1
	JOIN sf_senador p ON p.id = l1.id_sf_senador
) tmp
LEFT JOIN partido pr on pr.id = tmp.id_partido
LEFT JOIN estado e on e.id = tmp.id_estado
ORDER BY valor_total desc
LIMIT 10 ");

                banco.AddParameter("@id", id);

                using (var reader = banco.ExecuteReader(strSql.ToString()))
                {
                    List<dynamic> lstRetorno = new List<dynamic>();
                    while (reader.Read())
                    {
                        string link_parlamentar = "", link_despesas = "";
                        var tipo = reader["tipo"].ToString();
                        var id_parlamentar = Convert.ToInt32(reader["id"]);

                        if (tipo == "Deputado Federal")
                        {
                            // Deputado Federal
                            link_parlamentar = $"/deputado-federal/{id_parlamentar}";
                            link_despesas = $"/deputado-federal?IdParlamentar={id_parlamentar}&Fornecedor={id}&Periodo=0&Agrupamento=6";

                        }
                        else if (tipo == "Deputado Estadual")
                        {
                            // Deputado Estadual
                            link_parlamentar = $"/deputado-estadual/{id_parlamentar}";
                            link_despesas = $"/deputado-estadual?IdParlamentar={id_parlamentar}&Fornecedor={id}&Periodo=0&Agrupamento=6";

                        }
                        else
                        {
                            // Senador
                            link_parlamentar = $"/senador/{id_parlamentar}";
                            link_despesas = $"/senador?IdParlamentar={id_parlamentar}&Fornecedor={id}&Periodo=0&Agrupamento=6";
                        }

                        lstRetorno.Add(new
                        {
                            id = id_parlamentar,
                            tipo,
                            nome_parlamentar = reader["nome_parlamentar"].ToString(),
                            sigla_partido = reader["sigla_partido"].ToString(),
                            sigla_estado = reader["sigla_estado"].ToString(),
                            valor_total = Utils.FormataValor(reader["valor_total"]),
                            link_parlamentar,
                            link_despesas
                        }); ;
                    }

                    return lstRetorno;
                }
            }
        }

        public async Task<dynamic> RecebimentosPorAno(int id)
        {
            using (AppDb banco = new AppDb())
            {
                string strSql = @"
SELECT ano, SUM(valor) AS valor
FROM (
    SELECT l.ano, SUM(l.valor_liquido) AS valor
    FROM cf_despesa l
    WHERE l.id_fornecedor = @id
    group by l.ano

    UNION ALL

    SELECT CAST(l.ano_mes/100 as signed) as ano , SUM(l.valor_liquido) AS valor
    FROM cl_despesa l
    WHERE l.id_fornecedor = @id
    group by ano

    UNION ALL

    SELECT l.ano, SUM(l.valor) AS valor
    FROM sf_despesa l
    WHERE l.id_fornecedor = @id
    group by l.ano
) tmp
group by ano
order by ano
				";

                banco.AddParameter("@id", id);

                var categories = new List<dynamic>();
                var series = new List<dynamic>();

                using (DbDataReader reader = await banco.ExecuteReaderAsync(strSql.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        categories.Add(Convert.ToInt32(reader["ano"]));
                        series.Add(Convert.ToDecimal(reader["valor"]));
                    }
                }

                return new
                {
                    categories,
                    series
                };
            }
        }

        //		public dynamic RecebimentosMensaisPorAno(int id)
        //		{
        //			using (AppDb banco = new AppDb())
        //			{
        //				string strSql = @"
        //SELECT l.mes, l.ano, SUM(l.valor_liquido) AS valor_total
        //FROM cf_despesa l
        //WHERE l.id_fornecedor = @id
        //group by l.ano, l.mes


        //UNION ALL

        //SELECT l.mes, l.ano, SUM(l.valor) AS valor_total
        //FROM sf_despesa l
        //WHERE l.id_fornecedor = @id
        //group by l.ano, l.mes

        //order by ano, mes
        //				";

        //				banco.AddParameter("@id", id);

        //				using (var reader = banco.ExecuteReader(strSql.ToString()))
        //				{
        //					List<dynamic> lstRetorno = new List<dynamic>();
        //					var lstValoresMensais = new decimal?[12];
        //					string anoControle = string.Empty;
        //					bool existeGastoNoAno = false;

        //					while (reader.Read())
        //					{
        //						if (reader["ano"].ToString() != anoControle)
        //						{
        //							if (existeGastoNoAno)
        //							{
        //								lstRetorno.Add(new
        //								{
        //									name = anoControle.ToString(),
        //									data = lstValoresMensais
        //								});

        //								lstValoresMensais = new decimal?[12];
        //								existeGastoNoAno = false;
        //							}

        //							anoControle = reader["ano"].ToString();
        //						}

        //						if (Convert.ToDecimal(reader["valor_total"]) > 0)
        //						{
        //							lstValoresMensais[Convert.ToInt32(reader["mes"]) - 1] = Convert.ToDecimal(reader["valor_total"]);
        //							existeGastoNoAno = true;
        //						}
        //					}
        //					if (existeGastoNoAno)
        //					{
        //						lstRetorno.Add(new
        //						{
        //							name = anoControle.ToString(),
        //							data = lstValoresMensais
        //						});
        //					}


        //					return lstRetorno;
        //				}
        //			}
        //		}

        public int AtualizaDados(Fornecedor fornecedor)
        {
            int id_fornecedor = 0;

            using (AppDb banco = new AppDb())
            {
                bool fornecedor_existente = false;

                string strSqlLocaliza = @"
					SELECT f.id, fi.id_fornecedor
					FROM fornecedor f
					LEFT JOIN fornecedor_info fi on fi.id_fornecedor = f.id
					where f.cnpj_cpf = @cnpj_cpf
				";
                banco.AddParameter("cnpj_cpf", fornecedor.CnpjCpf);

                using (var dReader = banco.ExecuteReader(strSqlLocaliza))
                {
                    if (dReader.Read())
                    {
                        id_fornecedor = Convert.ToInt32(dReader["id"]);
                        fornecedor_existente = !Convert.IsDBNull(dReader["id_fornecedor"]);
                    }
                    else
                    {
                        throw new BusinessException("Fornecedor inexistente.");
                    }
                }

                object id_fornecedor_atividade_principal;
                object id_fornecedor_natureza_juridica;

                try
                {
                    banco.AddParameter("codigo", fornecedor.AtividadePrincipal.Split(' ')[0]);
                    id_fornecedor_atividade_principal = banco.ExecuteScalar("select id from fornecedor_atividade where codigo=@codigo");
                }
                catch (Exception)
                {
                    id_fornecedor_atividade_principal = DBNull.Value;
                }

                try
                {
                    banco.AddParameter("codigo", fornecedor.NaturezaJuridica.Split(' ')[0]);
                    id_fornecedor_natureza_juridica = banco.ExecuteScalar("select id from fornecedor_natureza_juridica where codigo=@codigo");
                }
                catch (Exception)
                {
                    id_fornecedor_natureza_juridica = DBNull.Value;
                }

                banco.AddParameter("tipo", fornecedor.Tipo);
                banco.AddParameter("nome", fornecedor.RazaoSocial);
                banco.AddParameter("data_de_abertura", Utils.ParseDateTime(fornecedor.DataAbertura));
                banco.AddParameter("nome_fantasia", fornecedor.NomeFantasia);
                banco.AddParameter("id_fornecedor_atividade_principal", id_fornecedor_atividade_principal);
                banco.AddParameter("id_fornecedor_natureza_juridica", id_fornecedor_natureza_juridica);
                banco.AddParameter("logradouro", fornecedor.Logradouro);
                banco.AddParameter("numero", fornecedor.Numero);
                banco.AddParameter("complemento", fornecedor.Complemento);
                banco.AddParameter("cep", fornecedor.Cep);
                banco.AddParameter("bairro", fornecedor.Bairro);
                banco.AddParameter("municipio", fornecedor.Cidade);
                banco.AddParameter("estado", fornecedor.Uf);
                banco.AddParameter("situacao_cadastral", fornecedor.Situacao);
                banco.AddParameter("data_da_situacao_cadastral", Utils.ParseDateTime(fornecedor.DataSituacao));
                banco.AddParameter("motivo_situacao_cadastral", fornecedor.MotivoSituacao);
                banco.AddParameter("situacao_especial", fornecedor.SituacaoEspecial);
                banco.AddParameter("data_situacao_especial", Utils.ParseDateTime(fornecedor.DataSituacaoEspecial));
                banco.AddParameter("endereco_eletronico", fornecedor.Email);
                banco.AddParameter("telefone", fornecedor.Telefone);
                banco.AddParameter("ente_federativo_responsavel", fornecedor.EnteFederativoResponsavel);
                banco.AddParameter("capital_social", ObterValor(fornecedor.CapitalSocial));
                //banco.AddParameter("ip_colaborador", fornecedor.UsuarioInclusao);
                banco.AddParameter("id_fornecedor", id_fornecedor);

                string sql;
                if (!fornecedor_existente)
                {
                    banco.AddParameter("cnpj", fornecedor.CnpjCpf);

                    sql =
                        @"INSERT INTO fornecedor_info (
							tipo,
							nome,
							data_de_abertura,
							nome_fantasia,
							id_fornecedor_atividade_principal,
							id_fornecedor_natureza_juridica,
							logradouro,
							numero,
							complemento,
							cep,
							bairro,
							municipio,
							estado,
							situacao_cadastral,
							data_da_situacao_cadastral,
							motivo_situacao_cadastral,
							situacao_especial,
							data_situacao_especial,
							endereco_eletronico,
							telefone1,
							ente_federativo_responsavel,
							capital_social,
							obtido_em,
							id_fornecedor,
							cnpj
						) VALUES (
							@tipo,
							@nome,
							@data_de_abertura,
							@nome_fantasia,
							@id_fornecedor_atividade_principal,
							@id_fornecedor_natureza_juridica,
							@logradouro,
							@numero,
							@complemento,
							@cep,
							@bairro,
							@municipio,
							@estado,
							@situacao_cadastral,
							@data_da_situacao_cadastral,
							@motivo_situacao_cadastral,
							@situacao_especial,
							@data_situacao_especial,
							@endereco_eletronico,
							@telefone,
							@ente_federativo_responsavel,
							@capital_social,
							NOW(),
							@id_fornecedor,
							@cnpj
						)
					";
                }
                else
                {
                    sql =
                        @"UPDATE fornecedor_info SET
							tipo								= @tipo,
							nome								= @nome,
							data_de_abertura					= @data_de_abertura,
							nome_fantasia						= @nome_fantasia,
							id_fornecedor_atividade_principal	= @id_fornecedor_atividade_principal,
							id_fornecedor_natureza_juridica		= @id_fornecedor_natureza_juridica,
							logradouro							= @logradouro,
							numero								= @numero,
							complemento							= @complemento,
							cep									= @cep,
							bairro								= @bairro,
							municipio							= @municipio,
							estado								= @estado,
							situacao_cadastral					= @situacao_cadastral,
							data_da_situacao_cadastral			= @data_da_situacao_cadastral,
							motivo_situacao_cadastral			= @motivo_situacao_cadastral,
							situacao_especial					= @situacao_especial,
							data_situacao_especial				= @data_situacao_especial,
							endereco_eletronico					= @endereco_eletronico,
							telefone1							= @telefone,
							ente_federativo_responsavel			= @ente_federativo_responsavel,
							capital_social						= @capital_social,
							obtido_em							= NOW()
						WHERE id_fornecedor						= @id_fornecedor
					";
                }

                banco.ExecuteNonQuery(sql.ToString());

                banco.AddParameter("id_fornecedor", id_fornecedor);
                banco.ExecuteScalar("DELETE FROM fornecedor_atividade_secundaria WHERE id_fornecedor = @id_fornecedor");

                if (fornecedor.lstFornecedorQuadroSocietario != null)
                {
                    foreach (var atividade in fornecedor.AtividadeSecundaria)
                    {
                        if (string.IsNullOrEmpty(atividade) || atividade == "********") continue;

                        object id_fornecedor_atividade;

                        try
                        {
                            banco.AddParameter("codigo", atividade.Split(' ')[0]);
                            id_fornecedor_atividade = banco.ExecuteScalar("select id from fornecedor_atividade where codigo=@codigo");
                        }
                        catch (Exception)
                        {
                            continue;
                        }

                        banco.AddParameter("id_fornecedor", id_fornecedor);
                        banco.AddParameter("id_fornecedor_atividade", id_fornecedor_atividade);

                        banco.ExecuteNonQuery(
                            @"INSERT fornecedor_atividade_secundaria (
								id_fornecedor, 
								id_fornecedor_atividade
							) VALUES (
								@id_fornecedor, 
								@id_fornecedor_atividade
							)");
                    }
                }

                banco.AddParameter("id_fornecedor", id_fornecedor);
                banco.ExecuteScalar("DELETE FROM fornecedor_socio WHERE id_fornecedor = @id_fornecedor");

                if (fornecedor.lstFornecedorQuadroSocietario != null)
                {
                    foreach (var qas in fornecedor.lstFornecedorQuadroSocietario)
                    {
                        banco.AddParameter("id_fornecedor", id_fornecedor);
                        banco.AddParameter("nome", qas.Nome);

                        if (!string.IsNullOrEmpty(qas.Qualificacao))
                        {
                            banco.AddParameter("id_fornecedor_socio_qualificacao", Convert.ToInt32(qas.Qualificacao.Split('-')[0]));
                        }
                        else
                        {
                            banco.AddParameter("id_fornecedor_socio_qualificacao", DBNull.Value);
                        }

                        banco.AddParameter("nome_representante", qas.NomeRepresentanteLegal);

                        if (!string.IsNullOrEmpty(qas.QualificacaoRepresentanteLegal))
                        {
                            banco.AddParameter("id_fornecedor_socio_representante_qualificacao", Convert.ToInt32(qas.QualificacaoRepresentanteLegal.Split('-')[0]));
                        }
                        else
                        {
                            banco.AddParameter("id_fornecedor_socio_representante_qualificacao", DBNull.Value);
                        }

                        banco.ExecuteNonQuery(
                            @"INSERT fornecedor_socio (
								id_fornecedor, 
								nome, 
								id_fornecedor_socio_qualificacao, 
								nome_representante, 
								id_fornecedor_socio_representante_qualificacao
							) VALUES (
								@id_fornecedor, 
								@nome, 
								@id_fornecedor_socio_qualificacao, 
								@nome_representante, 
								@id_fornecedor_socio_representante_qualificacao
							)");
                    }
                }

                banco.AddParameter("@id", id_fornecedor);
                banco.AddParameter("@controle", null);
                banco.AddParameter("@mensagem", null);

                banco.ExecuteNonQuery(@"update fornecedor set controle=@controle, mensagem=@mensagem where id=@id;");
            }

            return id_fornecedor;
        }

        private object ObterValor(object d)
        {
            if (string.IsNullOrEmpty(d as string) || Convert.IsDBNull(d))
            {
                return DBNull.Value;
            }
            else
            {
                try
                {
                    return Convert.ToDecimal(d.ToString().Split(' ')[0].Replace("R$", "").Trim());
                }
                catch (Exception)
                {
                    return DBNull.Value;
                }
            }
        }

        public dynamic Consulta(string cnpj, string nome)
        {
            string sSql = @"SELECT 
							id as id_fornecedor
							, cnpj_cpf
							, nome
							, doador
						FROM fornecedor
                        WHERE (1=1)";

            using (AppDb banco = new AppDb())
            {
                if (!string.IsNullOrEmpty(cnpj))
                {
                    cnpj = Utils.RemoveCaracteresNaoNumericos(cnpj);

                    if (cnpj.Length == 14 || cnpj.Length == 11)
                    {
                        banco.AddParameter("cnpj", cnpj);
                        sSql += " AND cnpj_cpf = @cnpj";
                    }
                    else
                    {
                        banco.AddParameter("cnpj", cnpj + "%");
                        sSql += " AND cnpj_cpf like @cnpj";
                    }

                }
                else
                {
                    if (!string.IsNullOrEmpty(nome))
                    {
                        banco.AddParameter("nome", "%" + Utils.MySqlEscape(nome) + "%");
                        sSql += " AND nome like @nome";
                    }
                }

                sSql += " order by nome limit 100;";

                var lstFornecedor = new List<dynamic>();
                using (var reader = banco.ExecuteReader(sSql))
                {
                    while (reader.Read())
                    {
                        lstFornecedor.Add(new
                        {
                            id_fornecedor = reader["id_fornecedor"].ToString(),
                            cnpj_cpf = Utils.FormatCnpjCpf(reader["cnpj_cpf"].ToString()),
                            nome = reader["nome"].ToString(),
                        });
                    }

                    return lstFornecedor;
                }
            }
        }

        public async Task<List<dynamic>> Busca(string value)
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT 
						f.id_fornecedor
						, f.cnpj
						, f.nome
						, f.nome_fantasia
                        , f.estado
					FROM fornecedor_info f
                    WHERE 1=1");

                if (!string.IsNullOrEmpty(value))
                {
                    strSql.AppendLine("	AND (f.nome like '%" + Utils.MySqlEscape(value) + "%' or f.nome_fantasia like '%" + Utils.MySqlEscape(value) + "%')");
                }

                strSql.AppendLine(@"
                    ORDER BY nome, cnpj
                    limit 100
				");

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(strSql.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_fornecedor = reader["id_fornecedor"],
                            cnpj = Utils.FormatCnpjCpf(reader["cnpj"].ToString()),
                            nome = reader["nome"].ToString(),
                            nome_fantasia = reader["nome_fantasia"].ToString(),
                            estado = reader["estado"].ToString()
                        });
                    }
                }
                return lstRetorno;
            }
        }
    }
}