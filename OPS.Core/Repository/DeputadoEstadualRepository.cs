using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using OPS.Core.DTO;
using OPS.Core.Enumerator;
using OPS.Core.Utilities;

namespace OPS.Core.Repository
{
    public class DeputadoEstadualRepository
    {
        public async Task<dynamic> Consultar(int id)
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT 
						d.id as id_cl_deputado
						, d.id_partido
						, p.sigla as sigla_partido
						, p.nome as nome_partido
						, d.id_estado
						, e.sigla as sigla_estado
						, e.nome as nome_estado
						, d.nome_parlamentar
						, d.nome_civil
						, d.sexo
						, d.telefone
						, d.email
						, d.nascimento
						, d.naturalidade
                        , d.profissao
                        , d.site
                        , d.perfil
						, d.valor_total_ceap
					FROM cl_deputado d
					LEFT JOIN partido p on p.id = d.id_partido
					LEFT JOIN estado e on e.id = d.id_estado -- eleito
					WHERE d.id = @id
				");
                banco.AddParameter("@id", id);

                using (DbDataReader reader = await banco.ExecuteReaderAsync(strSql.ToString()))
                {
                    if (await reader.ReadAsync())
                    {
                        return new
                        {
                            id_cl_deputado = reader["id_cl_deputado"],
                            id_partido = reader["id_partido"],
                            sigla_estado = reader["sigla_estado"].ToString(),
                            nome_partido = reader["nome_partido"].ToString(),
                            id_estado = reader["id_estado"],
                            sigla_partido = reader["sigla_partido"].ToString(),
                            nome_estado = reader["nome_estado"].ToString(),
                            nome_parlamentar = reader["nome_parlamentar"].ToString(),
                            nome_civil = reader["nome_civil"].ToString(),
                            sexo = reader["sexo"].ToString(),
                            telefone = reader["telefone"].ToString(),
                            email = reader["email"].ToString(),
                            profissao = reader["profissao"].ToString(),
                            naturalidade = reader["naturalidade"].ToString(),
                            site = reader["site"].ToString(),
                            perfil = reader["perfil"].ToString(),
                            nascimento = Utils.NascimentoFormatado(reader["nascimento"]),
                            valor_total_ceap = Utils.FormataValor(reader["valor_total_ceap"])
                        };
                    }

                    return null;
                }
            }
        }

        public async Task<dynamic> MaioresFornecedores(int id)
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();

                strSql.AppendLine(@"
					SELECT
						 pj.id as id_fornecedor
						, pj.cnpj_cpf
						, pj.nome AS nome_fornecedor
						, l1.valor_total
					from(
						SELECT
						sum(l.valor_liquido) as valor_total
						, l.id_fornecedor
						FROM cl_despesa l
						WHERE l.id_cl_deputado = @id
                        AND l.id_fornecedor IS NOT NULL
						GROUP BY l.id_fornecedor
						order by 1 desc
						LIMIT 10
					) l1
					JOIN fornecedor pj on pj.id = l1.id_fornecedor
					order by l1.valor_total desc
				");

                banco.AddParameter("@id", id);

                using (DbDataReader reader = await banco.ExecuteReaderAsync(strSql.ToString()))
                {
                    List<dynamic> lstRetorno = new List<dynamic>();
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_fornecedor = reader["id_fornecedor"].ToString(),
                            cnpj_cpf = Utils.FormatCnpjCpf(reader["cnpj_cpf"].ToString()),
                            nome_fornecedor = reader["nome_fornecedor"].ToString(),
                            valor_total = Utils.FormataValor(reader["valor_total"])
                        });
                    }

                    return lstRetorno;
                }
            }
        }

        public async Task<dynamic> MaioresNotas(int id)
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();

                strSql.AppendLine(@"
					SELECT
						 l1.id as id_cl_despesa
						, l1.id_fornecedor
						, pj.cnpj_cpf
						, pj.nome AS nome_fornecedor
						, l1.valor_liquido
					from (
						SELECT
						    l.id
						    , l.valor_liquido
						    , l.id_fornecedor
						FROM cl_despesa l
						WHERE l.id_cl_deputado = @id
                        AND l.id_fornecedor IS NOT NULL
						order by l.valor_liquido desc
						LIMIT 10
					) l1
					JOIN fornecedor pj on pj.id = l1.id_fornecedor
					order by l1.valor_liquido desc 
				");

                banco.AddParameter("@id", id);

                using (DbDataReader reader = await banco.ExecuteReaderAsync(strSql.ToString()))
                {
                    List<dynamic> lstRetorno = new List<dynamic>();
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_cl_despesa = reader["id_cl_despesa"].ToString(),
                            id_fornecedor = reader["id_fornecedor"].ToString(),
                            cnpj_cpf = Utils.FormatCnpjCpf(reader["cnpj_cpf"].ToString()),
                            nome_fornecedor = reader["nome_fornecedor"].ToString(),
                            valor_liquido = Utils.FormataValor(reader["valor_liquido"])
                        });
                    }

                    return lstRetorno;
                }
            }
        }

        public async Task<dynamic> Documento(int id)
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();

                strSql.AppendLine(@"
					SELECT
						l.id as id_cl_despesa
						, l.id_documento
						, l.numero_documento
						, l.tipo_documento
						, l.data_emissao
						, l.valor_documento
						, l.valor_glosa
						, l.valor_liquido
						, l.valor_restituicao
						, ps.nome as nome_passageiro
						, tv.descricao as trecho_viagem
						, l.ano
						, l.mes
						, td.id as id_cl_despesa_tipo
						, td.descricao as descricao_despesa
						, d.id as id_cl_deputado
						, d.id_deputado
						, d.nome_parlamentar
						, e.sigla as sigla_estado
						, p.sigla as sigla_partido
						, pj.id AS id_fornecedor
						, pj.cnpj_cpf
						, pj.nome AS nome_fornecedor
                        , l.tipo_link
					FROM cl_despesa l
					LEFT JOIN fornecedor pj ON pj.id = l.id_fornecedor
					LEFT JOIN cl_deputado d ON d.id = l.id_cl_deputado
					LEFT JOIN cl_despesa_tipo td ON td.id = l.id_cl_despesa_tipo
					LEFT JOIN partido p on p.id = d.id_partido
					LEFT JOIN estado e on e.id = d.id_estado
	                LEFT JOIN trecho_viagem tv ON tv.id = l.id_trecho_viagem
					LEFT JOIN pessoa ps ON ps.id = l.id_passageiro
					WHERE l.id = @id
				 ");
                banco.AddParameter("@id", id);

                await using (DbDataReader reader = await banco.ExecuteReaderAsync(strSql.ToString()))
                {
                    if (await reader.ReadAsync())
                    {
                        string sTipoDocumento = "";
                        switch (await reader.GetValueOrDefaultAsync<uint>(3))
                        {
                            case 0: sTipoDocumento = "Nota Fiscal"; break;
                            case 1: sTipoDocumento = "Recibo"; break;
                            case 2: case 3: sTipoDocumento = "Despesa no Exterior"; break;
                        }
                        string cnpjCpf = Utils.FormatCnpjCpf(await reader.GetValueOrDefaultAsync<string>(21));
                        var ano = await reader.GetValueOrDefaultAsync<uint>(11);
                        var mes = await reader.GetValueOrDefaultAsync<ushort>(12);

                        var result = new
                        {
                            id_cl_despesa = await reader.GetValueOrDefaultAsync<dynamic>(0),
                            id_documento = await reader.GetValueOrDefaultAsync<dynamic>(1),
                            numero_documento = await reader.GetValueOrDefaultAsync<string>(2),
                            tipo_documento = sTipoDocumento,
                            data_emissao = Utils.FormataData(await reader.GetValueOrDefaultAsync<MySqlDateTime?>(4)),
                            valor_documento = Utils.FormataValor(await reader.GetValueOrDefaultAsync<decimal?>(5)),
                            valor_glosa = Utils.FormataValor(await reader.GetValueOrDefaultAsync<decimal?>(6)),
                            valor_liquido = Utils.FormataValor(await reader.GetValueOrDefaultAsync<decimal?>(7)),
                            valor_restituicao = Utils.FormataValor(await reader.GetValueOrDefaultAsync<decimal?>(8)),
                            nome_passageiro = await reader.GetValueOrDefaultAsync<string>(9),
                            trecho_viagem = await reader.GetValueOrDefaultAsync<string>(10),
                            ano,
                            mes,
                            competencia = $"{mes:00}/{ano:0000}",
                            id_cl_despesa_tipo = await reader.GetValueOrDefaultAsync<dynamic>(13),
                            descricao_despesa = await reader.GetValueOrDefaultAsync<string>(14),
                            id_cl_deputado = await reader.GetValueOrDefaultAsync<dynamic>(15),
                            id_deputado = await reader.GetValueOrDefaultAsync<dynamic>(16),
                            nome_parlamentar = await reader.GetValueOrDefaultAsync<string>(17),
                            sigla_estado = await reader.GetValueOrDefaultAsync<string>(18),
                            sigla_partido = await reader.GetValueOrDefaultAsync<string>(19),
                            id_fornecedor = await reader.GetValueOrDefaultAsync<dynamic>(20),
                            cnpj_cpf = cnpjCpf,
                            nome_fornecedor = await reader.GetValueOrDefaultAsync<string>(22),
                            link = await reader.GetValueOrDefaultAsync<dynamic>(23)
                        };

                        return result;
                    }
                }
                return null;
            }
        }

        public async Task<dynamic> DocumentosDoMesmoDia(int id)
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();

                strSql.AppendLine(@"
					SELECT
						l.id as id_cl_despesa
						, pj.id as id_fornecedor
						, pj.nome as nome_fornecedor
						, pji.estado as sigla_estado_fornecedor
						, l.valor_liquido
					FROM (
						select id, id_cl_deputado, id_fornecedor, data_emissao from cl_despesa
						where id = @id
					) l1 
					INNER JOIN cl_despesa l on
						l1.id_cl_deputado = l.id_cl_deputado and
						l1.data_emissao = l.data_emissao and
						l1.id <> l.id
					LEFT JOIN fornecedor pj ON pj.id = l.id_fornecedor
					LEFT JOIN fornecedor_info pji ON pji.id_fornecedor = pj.id
					order by l.valor_liquido desc
					limit 50
				 ");
                banco.AddParameter("@id", id);

                var lstRetorno = new List<dynamic>();

                DbDataReader reader = await banco.ExecuteReaderAsync(strSql.ToString());
                while (await reader.ReadAsync())
                {
                    lstRetorno.Add(new
                    {
                        id_cl_despesa = await reader.GetValueOrDefaultAsync<ulong>(0),
                        id_fornecedor = await reader.GetValueOrDefaultAsync<uint>(1),
                        nome_fornecedor = await reader.GetValueOrDefaultAsync<string>(2),
                        sigla_estado_fornecedor = await reader.GetValueOrDefaultAsync<string>(3),
                        valor_liquido = Utils.FormataValor(await reader.GetValueOrDefaultAsync<decimal>(4))
                    });
                }
                reader.Close();
                return lstRetorno;
            }
        }

        public async Task<dynamic> DocumentosDaSubcotaMes(int id)
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();

                strSql.AppendLine(@"
					SELECT
						l.id as id_cl_despesa
						, pj.id as id_fornecedor
						, pj.nome as nome_fornecedor
						, pji.estado as sigla_estado_fornecedor
						, l.valor_liquido
					FROM (
						select id, id_cl_deputado, id_fornecedor, id_cl_despesa_tipo, ano, mes 
                        from cl_despesa
						where id = @id
					) l1 
					INNER JOIN cl_despesa l on
					l1.id_cl_deputado = l.id_cl_deputado and
					l1.ano = l.ano and
					l1.mes = l.mes and
					l1.id_cl_despesa_tipo = l.id_cl_despesa_tipo and
					l1.id <> l.id
					LEFT JOIN fornecedor pj ON pj.id = l.id_fornecedor
					LEFT JOIN fornecedor_info pji ON pji.id_fornecedor = pj.id
					order by l.valor_liquido desc
					limit 50
				 ");
                banco.AddParameter("@id", id);

                var lstRetorno = new List<dynamic>();

                DbDataReader reader = await banco.ExecuteReaderAsync(strSql.ToString());
                while (await reader.ReadAsync())
                {
                    lstRetorno.Add(new
                    {
                        id_cl_despesa = await reader.GetValueOrDefaultAsync<ulong>(0),
                        id_fornecedor = await reader.GetValueOrDefaultAsync<uint>(1),
                        nome_fornecedor = await reader.GetValueOrDefaultAsync<string>(2),
                        sigla_estado_fornecedor = await reader.GetValueOrDefaultAsync<string>(3),
                        valor_liquido = Utils.FormataValor(await reader.GetValueOrDefaultAsync<decimal>(4))
                    });
                }
                reader.Close();
                return lstRetorno;
            }
        }

        public async Task<dynamic> GastosPorAno(int id)
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT cast(d.ano_mes/100 as signed) as ano, SUM(d.valor_liquido) AS valor_total
					FROM cl_despesa d
					WHERE d.id_cl_deputado = @id
					group by ano
					order by ano
				");
                banco.AddParameter("@id", id);

                var categories = new List<dynamic>();
                var series = new List<dynamic>();

                using (DbDataReader reader = await banco.ExecuteReaderAsync(strSql.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        categories.Add(Convert.ToInt32(reader["ano"]));
                        series.Add(Convert.ToDecimal(reader["valor_total"]));
                    }
                }

                return new
                {
                    categories,
                    series
                };

                //using (DbDataReader reader = await banco.ExecuteReaderAsync(strSql.ToString()))
                //{
                //    List<dynamic> lstRetorno = new List<dynamic>();
                //    var lstValoresMensais = new decimal?[12];
                //    string anoControle = string.Empty;
                //    bool existeGastoNoAno = false;

                //    while (await reader.ReadAsync())
                //    {
                //        if (reader["ano"].ToString() != anoControle)
                //        {
                //            if (existeGastoNoAno)
                //            {
                //                lstRetorno.Add(new
                //                {
                //                    name = anoControle.ToString(),
                //                    data = lstValoresMensais
                //                });

                //                lstValoresMensais = new decimal?[12];
                //                existeGastoNoAno = false;
                //            }

                //            anoControle = reader["ano"].ToString();
                //        }

                //        if (Convert.ToDecimal(reader["valor_total"]) > 0)
                //        {
                //            lstValoresMensais[Convert.ToInt32(reader["mes"]) - 1] = Convert.ToDecimal(reader["valor_total"]);
                //            existeGastoNoAno = true;
                //        }
                //    }

                //    if (existeGastoNoAno)
                //    {
                //        lstRetorno.Add(new
                //        {
                //            name = anoControle.ToString(),
                //            data = lstValoresMensais
                //        });
                //    }

                //    return lstRetorno;
                //    // Ex: [{"$id":"1","name":"2015","data":[null,18404.57,25607.82,29331.99,36839.82,24001.68,40811.97,33641.20,57391.30,60477.07,90448.58,13285.14]}]
                //}
            }
        }

        public async Task<dynamic> ResumoMensal()
        {
            using (AppDb banco = new AppDb())
            {
                using (DbDataReader reader = await banco.ExecuteReaderAsync(@"select ano, mes, valor from cl_despesa_resumo_mensal"))
                {
                    List<dynamic> lstRetorno = new List<dynamic>();
                    var lstValoresMensais = new decimal?[12];
                    string anoControle = string.Empty;
                    bool existeGastoNoAno = false;

                    while (await reader.ReadAsync())
                    {
                        if (reader["ano"].ToString() != anoControle)
                        {
                            if (existeGastoNoAno)
                            {
                                lstRetorno.Add(new
                                {
                                    name = anoControle.ToString(),
                                    data = lstValoresMensais
                                });

                                lstValoresMensais = new decimal?[12];
                                existeGastoNoAno = false;
                            }

                            anoControle = reader["ano"].ToString();
                        }

                        if (Convert.ToDecimal(reader["valor"]) > 0)
                        {
                            lstValoresMensais[Convert.ToInt32(reader["mes"]) - 1] = Convert.ToDecimal(reader["valor"]);
                            existeGastoNoAno = true;
                        }
                    }

                    if (existeGastoNoAno)
                    {
                        lstRetorno.Add(new
                        {
                            name = anoControle.ToString(),
                            data = lstValoresMensais
                        });
                    }

                    return lstRetorno;
                }
            }
        }

        public async Task<dynamic> ResumoAnual()
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					select ano, sum(valor) as valor
					from cl_despesa_resumo_mensal
					group by ano
				");

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

        public async Task<dynamic> Lista(FiltroParlamentarDTO request)
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
                    SELECT 
						d.id as id_cl_deputado
						, d.nome_parlamentar 
						, d.nome_civil
						, d.valor_total_ceap
                        , d.valor_total_remuneracao
						, d.id_partido
						, p.sigla as sigla_partido
						, p.nome as nome_partido
						, d.id_estado
						, e.sigla as sigla_estado
						, e.nome as nome_estado
                        , d.situacao
					FROM cl_deputado d
					LEFT JOIN partido p on p.id = d.id_partido
					LEFT JOIN estado e on e.id = d.id_estado
                    WHERE 1=1");

                //if (request.Periodo > 50)
                //{
                //    strSql.AppendLine($" AND d.id IN(select m.id_cl_deputado from cl_mandato m where m.id_legislatura = {request.Periodo.ToString()})");
                //}

                if (!string.IsNullOrEmpty(request.Partido))
                {
                    strSql.AppendLine("	AND d.id_partido IN(" + Utils.MySqlEscapeNumberToIn(request.Partido) + ") ");
                }

                if (!string.IsNullOrEmpty(request.Estado))
                {
                    strSql.AppendLine("	AND d.id_estado IN(" + Utils.MySqlEscapeNumberToIn(request.Estado) + ") ");
                }

                strSql.AppendLine(@"
                    ORDER BY nome_parlamentar
                    LIMIT 1000
				");

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(strSql.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_cl_deputado = reader["id_cl_deputado"],
                            nome_parlamentar = reader["nome_parlamentar"].ToString(),
                            nome_civil = reader["nome_civil"].ToString(),
                            valor_total_ceap = Utils.FormataValor(reader["valor_total_ceap"]),
                            valor_total_remuneracao = Utils.FormataValor(reader["valor_total_remuneracao"]),
                            sigla_partido = !string.IsNullOrEmpty(reader["sigla_partido"].ToString()) ? reader["sigla_partido"].ToString() : "S.PART.",
                            nome_partido = !string.IsNullOrEmpty(reader["nome_partido"].ToString()) ? reader["nome_partido"].ToString() : "SEM PARTIDO",
                            sigla_estado = reader["sigla_estado"].ToString(),
                            nome_estado = reader["nome_estado"].ToString(),
                            situacao = reader["situacao"].ToString(),
                            ativo = reader["situacao"].ToString() == "Exercício",
                        });
                    }
                }
                return lstRetorno;
            }
        }

        public async Task<dynamic> Busca(string value)
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT 
						d.id as id_cl_deputado
						, d.nome_parlamentar 
						, d.nome_civil
						, d.valor_total_ceap
						, d.id_partido
						, p.sigla as sigla_partido
						, p.nome as nome_partido
						, d.id_estado
						, e.sigla as sigla_estado
						, e.nome as nome_estado
					FROM cl_deputado d
					LEFT JOIN partido p on p.id = d.id_partido
					LEFT JOIN estado e on e.id = d.id_estado
                    WHERE id_deputado IS NOT NULL");

                if (!string.IsNullOrEmpty(value))
                {
                    strSql.AppendLine("	AND (d.nome_parlamentar like '%" + Utils.MySqlEscape(value) + "%' or d.nome_civil like '%" + Utils.MySqlEscape(value) + "%')");
                }

                strSql.AppendLine(@"
                    ORDER BY nome_parlamentar
                    limit 100
				");

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(strSql.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_cl_deputado = reader["id_cl_deputado"],
                            nome_parlamentar = reader["nome_parlamentar"].ToString(),
                            nome_civil = reader["nome_civil"].ToString(),
                            valor_total_ceap = Utils.FormataValor(reader["valor_total_ceap"]),
                            id_partido = reader["id_partido"],
                            sigla_partido = !string.IsNullOrEmpty(reader["sigla_partido"].ToString()) ? reader["sigla_partido"].ToString() : "S.PART.",
                            nome_partido = !string.IsNullOrEmpty(reader["nome_partido"].ToString()) ? reader["nome_partido"].ToString() : "SEM PARTIDO",
                            id_estado = reader["id_estado"],
                            sigla_estado = reader["sigla_estado"].ToString(),
                            nome_estado = reader["nome_estado"].ToString()
                        });
                    }
                }
                return lstRetorno;
            }
        }

        public async Task<dynamic> Pesquisa(MultiSelectRequest filtro = null)
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT DISTINCT
						d.id, d.nome_civil, d.nome_parlamentar 
					FROM cl_deputado d
				");

                if (filtro != null && string.IsNullOrEmpty(filtro.Ids))
                {
                    strSql.AppendLine(@"
                        WHERE (1=1)");

                    if (!string.IsNullOrEmpty(filtro.Busca))
                    {
                        var busca = Utils.MySqlEscape(filtro.Busca);
                        strSql.AppendLine(@" AND (d.nome_parlamentar like '%" + busca + "%' or d.nome_civil like '%" + busca + "%') ");
                    }

                    //if (filtro.Periodo > 50)
                    //{
                    //    strSql.AppendLine($" AND (d.id < 100 OR m.id_legislatura = {filtro.Periodo.ToString()})");
                    //}

                    strSql.AppendLine(@"
                        ORDER BY d.nome_parlamentar
                        limit 30
				    ");
                }
                else
                {
                    strSql.AppendLine(@"
                        WHERE (1=1) ");

                    if (filtro != null && !string.IsNullOrEmpty(filtro.Ids))
                    {
                        var Ids = Utils.MySqlEscapeNumberToIn(filtro.Ids);
                        strSql.AppendLine(@" AND d.id IN(" + Ids + ") ");
                    }

                    strSql.AppendLine(@"
                        ORDER BY d.nome_parlamentar
				    ");
                }

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(strSql.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id = reader["id"].ToString(),
                            tokens = new[] { reader["nome_civil"].ToString(), reader["nome_parlamentar"].ToString() },
                            text = reader["nome_parlamentar"].ToString()
                        });
                    }
                }
                return lstRetorno;
            }
        }

        public async Task<dynamic> Lancamentos(DataTablesRequest request)
        {
            if (request == null) throw new BusinessException("Parâmetro request não informado!");

            EnumAgrupamentoAuditoria eAgrupamento = (EnumAgrupamentoAuditoria)Convert.ToInt32(request.Filters["Agrupamento"].ToString());
            switch (eAgrupamento)
            {
                case EnumAgrupamentoAuditoria.Parlamentar:
                    return await LancamentosParlamentar(request);
                case EnumAgrupamentoAuditoria.Despesa:
                    return await LancamentosDespesa(request);
                case EnumAgrupamentoAuditoria.Fornecedor:
                    return await LancamentosFornecedor(request);
                case EnumAgrupamentoAuditoria.Partido:
                    return await LancamentosPartido(request);
                case EnumAgrupamentoAuditoria.Estado:
                    return await LancamentosEstado(request);
                case EnumAgrupamentoAuditoria.Documento:
                    return await LancamentosNotaFiscal(request);
                default:
                    break;
            }

            throw new BusinessException("Parâmetro request.Agrupamento não informado!");
        }

        private async Task<dynamic> LancamentosParlamentar(DataTablesRequest request)
        {
            using (AppDb banco = new AppDb())
            {
                var sqlSelect = new StringBuilder();

                sqlSelect.AppendLine(@"
                    SELECT SQL_CALC_FOUND_ROWS
						 d.id as id_cl_deputado
						, d.nome_parlamentar
						, e.sigla as sigla_estado
						, p.sigla as sigla_partido
						, l1.total_notas
						, l1.valor_total
                    FROM (
					    SELECT 
						    count(l.id) AS total_notas
					        , sum(l.valor_liquido) as valor_total
					        , l.id_cl_deputado
					    FROM cl_despesa l
					    WHERE (1=1)
				");

                AdicionaFiltroPeriodo(request, sqlSelect);
                AdicionaFiltroDeputado(request, sqlSelect);
                AdicionaFiltroDespesa(request, sqlSelect);
                AdicionaFiltroFornecedor(request, sqlSelect);
                AdicionaFiltroPartidoDeputado(request, sqlSelect);
                AdicionaFiltroEstadoDeputado(request, sqlSelect);

                sqlSelect.AppendLine(@"
					GROUP BY id_cl_deputado
                    ) l1
					JOIN cl_deputado d on d.id = l1.id_cl_deputado
					LEFT JOIN partido p on p.id = d.id_partido
					LEFT JOIN estado e on e.id = d.id_estado
                ");

                AdicionaResultadoComum(request, sqlSelect);

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(sqlSelect.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_cl_deputado = reader["id_cl_deputado"],
                            nome_parlamentar = reader["nome_parlamentar"].ToString(),
                            sigla_estado = reader["sigla_estado"].ToString(),
                            sigla_partido = reader["sigla_partido"].ToString(),
                            total_notas = Utils.FormataValor(reader["total_notas"], 0),
                            valor_total = Utils.FormataValor(reader["valor_total"])
                        });
                    }

                    var TotalCount = reader.GetTotalRowsFound();
                    return new
                    {
                        draw = request.Draw,
                        recordsTotal = TotalCount,
                        recordsFiltered = TotalCount,
                        data = lstRetorno
                    };
                }
            }
        }

        private async Task<dynamic> LancamentosFornecedor(DataTablesRequest request)
        {
            using (AppDb banco = new AppDb())
            {
                var sqlSelect = new StringBuilder();

                sqlSelect.AppendLine(@"
                    SELECT SQL_CALC_FOUND_ROWS
						l1.id_fornecedor
						, pj.nome AS nome_fornecedor
						, l1.total_notas
						, l1.valor_total
                        , pj.cnpj_cpf
                    FROM (
					    SELECT
						    l.id_fornecedor
						    , count(l.id) AS total_notas
						    , sum(l.valor_liquido) as valor_total
					    FROM cl_despesa l
					    WHERE (1=1)
				");

                AdicionaFiltroPeriodo(request, sqlSelect);
                AdicionaFiltroDeputado(request, sqlSelect);
                AdicionaFiltroDespesa(request, sqlSelect);
                AdicionaFiltroFornecedor(request, sqlSelect);
                AdicionaFiltroPartidoDeputado(request, sqlSelect);
                AdicionaFiltroEstadoDeputado(request, sqlSelect);

                sqlSelect.AppendLine(@"
					    GROUP BY l.id_fornecedor
                    ) l1
					LEFT JOIN fornecedor pj on pj.id = l1.id_fornecedor
				");

                AdicionaResultadoComum(request, sqlSelect);

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(sqlSelect.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_fornecedor = reader["id_fornecedor"].ToString(),
                            cnpj_cpf = Utils.FormatCnpjCpf(reader["cnpj_cpf"].ToString()),
                            nome_fornecedor = reader["nome_fornecedor"].ToString(),
                            total_notas = Utils.FormataValor(reader["total_notas"], 0),
                            valor_total = Utils.FormataValor(reader["valor_total"])
                        });
                    }

                    var TotalCount = reader.GetTotalRowsFound();
                    return new
                    {
                        draw = request.Draw,
                        recordsTotal = TotalCount,
                        recordsFiltered = TotalCount,
                        data = lstRetorno
                    };
                }
            }
        }

        private async Task<dynamic> LancamentosDespesa(DataTablesRequest request)
        {
            using (AppDb banco = new AppDb())
            {
                var sqlSelect = new StringBuilder();

                sqlSelect.AppendLine(@"
					SELECT
						l1.id_cl_despesa_tipo
						, td.descricao
						, l1.total_notas
						, l1.valor_total
					from (
					SELECT
						count(l.id) AS total_notas
						, sum(l.valor_liquido) as valor_total
						, l.id_cl_despesa_tipo
					FROM cl_despesa l
					WHERE (1=1)
				");

                AdicionaFiltroPeriodo(request, sqlSelect);
                AdicionaFiltroDeputado(request, sqlSelect);
                AdicionaFiltroDespesa(request, sqlSelect);
                AdicionaFiltroFornecedor(request, sqlSelect);
                AdicionaFiltroPartidoDeputado(request, sqlSelect);
                AdicionaFiltroEstadoDeputado(request, sqlSelect);

                sqlSelect.AppendLine(@"
					GROUP BY id_cl_despesa_tipo
					
					) l1
					LEFT JOIN cl_despesa_tipo td on td.id = l1.id_cl_despesa_tipo
				");

                AdicionaResultadoComum(request, sqlSelect);

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(sqlSelect.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_cl_despesa_tipo = reader["id_cl_despesa_tipo"],
                            descricao = reader["descricao"].ToString(),
                            total_notas = Utils.FormataValor(reader["total_notas"], 0),
                            valor_total = Utils.FormataValor(reader["valor_total"])
                        });
                    }

                    var TotalCount = reader.GetTotalRowsFound();
                    return new
                    {
                        draw = request.Draw,
                        recordsTotal = TotalCount,
                        recordsFiltered = TotalCount,
                        data = lstRetorno
                    };
                }
            }
        }

        private async Task<dynamic> LancamentosPartido(DataTablesRequest request)
        {
            using (AppDb banco = new AppDb())
            {
                var sqlSelect = new StringBuilder();

                sqlSelect.AppendLine(@"
					
						SELECT SQL_CALC_FOUND_ROWS
						 p.id as id_partido
						, IFnull(p.nome, 'SEM PARTIDO') as nome_partido
						, sum(l1.total_notas) as total_notas
						, count(l1.id_cl_deputado) as total_deputados
                        , sum(l1.valor_total) / count(l1.id_cl_deputado) as valor_medio_por_deputado
                        , sum(l1.valor_total) as valor_total
						FROM (
							SELECT
							 count(l.id) AS total_notas
							, sum(l.valor_liquido) as valor_total
							, l.id_cl_deputado
							FROM cl_despesa l
							WHERE (1=1)
				");

                AdicionaFiltroPeriodo(request, sqlSelect);
                AdicionaFiltroDeputado(request, sqlSelect);
                AdicionaFiltroDespesa(request, sqlSelect);
                AdicionaFiltroFornecedor(request, sqlSelect);
                AdicionaFiltroPartidoDeputado(request, sqlSelect);
                AdicionaFiltroEstadoDeputado(request, sqlSelect);

                sqlSelect.AppendLine(@"
						GROUP BY id_cl_deputado
					) l1
					INNER JOIN cl_deputado d on d.id = l1.id_cl_deputado
					LEFT JOIN partido p on p.id = d.id_partido
					GROUP BY p.id, p.nome
				");

                AdicionaResultadoComum(request, sqlSelect);

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(sqlSelect.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_partido = reader["id_partido"],
                            nome_partido = reader["nome_partido"],
                            total_notas = Utils.FormataValor(reader["total_notas"], 0),
                            total_deputados = Utils.FormataValor(reader["total_deputados"], 0),
                            valor_medio_por_deputado = Utils.FormataValor(reader["valor_medio_por_deputado"]),
                            valor_total = Utils.FormataValor(reader["valor_total"])
                        });
                    }

                    var TotalCount = reader.GetTotalRowsFound();
                    return new
                    {
                        draw = request.Draw,
                        recordsTotal = TotalCount,
                        recordsFiltered = TotalCount,
                        data = lstRetorno
                    };
                }
            }
        }

        private async Task<dynamic> LancamentosEstado(DataTablesRequest request)
        {
            using (AppDb banco = new AppDb())
            {
                var sqlSelect = new StringBuilder();

                sqlSelect.AppendLine(@"
						SELECT SQL_CALC_FOUND_ROWS
						 e.id AS id_estado
						, IFNULL(e.nome, 'Sem Estado / Lideranças de Partido') as nome_estado
						, sum(l1.total_notas) as total_notas
						, sum(l1.valor_total) as valor_total
						from (
							SELECT
							 count(l.id) AS total_notas
							, sum(l.valor_liquido) as valor_total
							, l.id_cl_deputado
							FROM cl_despesa l
							WHERE (1=1)
				");

                AdicionaFiltroPeriodo(request, sqlSelect);
                AdicionaFiltroDeputado(request, sqlSelect);
                AdicionaFiltroDespesa(request, sqlSelect);
                AdicionaFiltroFornecedor(request, sqlSelect);
                AdicionaFiltroPartidoDeputado(request, sqlSelect);
                AdicionaFiltroEstadoDeputado(request, sqlSelect);

                sqlSelect.AppendLine(@"
						GROUP BY id_cl_deputado
					) l1
					JOIN cl_deputado d on d.id = l1.id_cl_deputado
					LEFT JOIN estado e on e.id = d.id_estado
					GROUP BY e.id, e.nome
                ");

                AdicionaResultadoComum(request, sqlSelect);

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(sqlSelect.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_estado = reader["id_estado"],
                            nome_estado = reader["nome_estado"],
                            total_notas = Utils.FormataValor(reader["total_notas"], 0),
                            valor_total = Utils.FormataValor(reader["valor_total"])
                        });
                    }

                    var TotalCount = reader.GetTotalRowsFound();
                    return new
                    {
                        draw = request.Draw,
                        recordsTotal = TotalCount,
                        recordsFiltered = TotalCount,
                        data = lstRetorno
                    };
                }
            }
        }

        private async Task<dynamic> LancamentosNotaFiscal(DataTablesRequest request)
        {
            using (AppDb banco = new AppDb())
            {
                var sqlWhere = new StringBuilder();

                AdicionaFiltroPeriodo(request, sqlWhere);
                AdicionaFiltroDeputado(request, sqlWhere);
                AdicionaFiltroDespesa(request, sqlWhere);
                AdicionaFiltroFornecedor(request, sqlWhere);
                AdicionaFiltroPartidoDeputado(request, sqlWhere);
                AdicionaFiltroEstadoDeputado(request, sqlWhere);
                AdicionaFiltroDocumento(request, sqlWhere);

                var sqlSelect = new StringBuilder();

                sqlSelect.AppendLine(@"
					SELECT SQL_CALC_FOUND_ROWS
						l.data_emissao
						, pj.nome AS nome_fornecedor
						, d.nome_parlamentar
						, l.valor_liquido
                        , l.id as id_cl_despesa
                        , e.sigla as sigla_estado
                        , p.sigla as sigla_partido
                        , t.descricao as despesa_tipo
                        , de.descricao as despesa_especificacao
                        , l.favorecido
                        , l.id_fornecedor
                        , pj.cnpj_cpf
                        , d.id as id_cl_deputado
					FROM (
						SELECT data_emissao, id, id_cl_deputado, valor_liquido, id_cl_despesa_tipo, id_cl_despesa_especificacao, id_fornecedor, favorecido
						FROM cl_despesa l
						WHERE (1=1)
				");

                sqlSelect.AppendLine(sqlWhere.ToString());

                sqlSelect.AppendFormat(" ORDER BY {0} ", request.GetSorting("l.ano_mes DESC, l.data_emissao DESC, l.valor_liquido DESC"));
                sqlSelect.AppendFormat(" LIMIT {0},{1} ", request.Start, request.Length);

                sqlSelect.AppendLine(@" ) l
					INNER JOIN cl_deputado d on d.id = l.id_cl_deputado
					LEFT JOIN fornecedor pj on pj.id = l.id_fornecedor
	                LEFT JOIN partido p on p.id = d.id_partido
					LEFT JOIN estado e on e.id = d.id_estado
                    LEFT JOIN cl_despesa_tipo t on t.id = l.id_cl_despesa_tipo
                    LEFT JOIN cl_despesa_especificacao de on de.id = l.id_cl_despesa_especificacao;

                    SELECT COUNT(1) FROM cl_despesa l WHERE (1=1) ");

                sqlSelect.AppendLine(sqlWhere.ToString());

                AdicionaFiltroPeriodo(request, sqlSelect);

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(sqlSelect.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_cl_despesa = reader["id_cl_despesa"],
                            data_emissao = Utils.FormataData(reader["data_emissao"]),
                            id_fornecedor = reader["id_fornecedor"].ToString(),
                            cnpj_cpf = Utils.FormatCnpjCpf(reader["cnpj_cpf"].ToString()),
                            nome_fornecedor = reader["nome_fornecedor"].ToString(),
                            id_cl_deputado = reader["id_cl_deputado"],
                            nome_parlamentar = reader["nome_parlamentar"].ToString(),
                            sigla_estado = reader["sigla_estado"].ToString(),
                            sigla_partido = reader["sigla_partido"].ToString(),
                            despesa_tipo = reader["despesa_tipo"].ToString(),
                            despesa_especificacao = reader["despesa_especificacao"].ToString(),
                            favorecido = reader["favorecido"].ToString(),
                            valor_liquido = Utils.FormataValor(reader["valor_liquido"])
                        });
                    }

                    var TotalCount = reader.GetTotalRowsFound();
                    return new
                    {
                        draw = request.Draw,
                        recordsTotal = TotalCount,
                        recordsFiltered = TotalCount,
                        data = lstRetorno
                    };
                }
            }
        }

        private static void AdicionaFiltroPeriodo(DataTablesRequest request, StringBuilder sqlSelect)
        {
            int periodo = Convert.ToInt32(request.Filters["Periodo"].ToString());

            switch (periodo)
            {
                case 57:
                    sqlSelect.AppendLine(" AND l.ano_mes BETWEEN 202302 AND 202701");
                    break;

                case 56:
                    sqlSelect.AppendLine(" AND l.ano_mes BETWEEN 201902 AND 202301");
                    break;

                case 55:
                    sqlSelect.AppendLine(" AND l.ano_mes BETWEEN 201502 AND 201901");
                    break;

                case 54:
                    sqlSelect.AppendLine(" AND l.ano_mes BETWEEN 201102 AND 201501");
                    break;

                case 53:
                    sqlSelect.AppendLine(" AND l.ano_mes BETWEEN 200702 AND 201101");
                    break;
            }
        }

        private static void AdicionaFiltroPartidoDeputado(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.ContainsKey("Partido") && !string.IsNullOrEmpty(request.Filters["Partido"].ToString()))
            {
                sqlSelect.AppendLine("	AND l.id_cl_deputado IN (SELECT id FROM cl_deputado where id_partido IN(" + request.Filters["Partido"].ToString() + ")) ");
            }
        }

        private static void AdicionaFiltroEstadoDeputado(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.ContainsKey("Estado") && !string.IsNullOrEmpty(request.Filters["Estado"].ToString()))
            {
                sqlSelect.AppendLine("	AND l.id_cl_deputado IN (SELECT id FROM cl_deputado where id_estado IN(" + Utils.MySqlEscapeNumberToIn(request.Filters["Estado"].ToString()) + ")) ");
            }
        }

        private static void AdicionaFiltroFornecedor(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.ContainsKey("Fornecedor") && !string.IsNullOrEmpty(request.Filters["Fornecedor"].ToString()))
            {
                var Fornecedor = string.Join("", System.Text.RegularExpressions.Regex.Split(request.Filters["Fornecedor"].ToString(), @"[^\d]"));

                if (!string.IsNullOrEmpty(Fornecedor))
                {
                    if (Fornecedor.Length == 14 || Fornecedor.Length == 11)
                    {
                        using (AppDb banco = new AppDb())
                        {
                            var id_fornecedor = banco.ExecuteScalar("select id from fornecedor where cnpj_cpf = '" + Utils.RemoveCaracteresNaoNumericos(Fornecedor) + "'");

                            if (!Convert.IsDBNull(id_fornecedor))
                            {
                                sqlSelect.AppendLine("	AND l.id_fornecedor =" + id_fornecedor + " ");
                            }
                        }
                    }
                    else if (Fornecedor.Length == 8) //CNPJ raiz
                    {
                        sqlSelect.AppendLine("	AND l.id_fornecedor IN (select id from fornecedor where cnpj_cpf like '" + Utils.RemoveCaracteresNaoNumericos(Fornecedor) + "%')");
                    }
                    else
                    {
                        sqlSelect.AppendLine("	AND l.id_fornecedor =" + Utils.RemoveCaracteresNaoNumericos(Fornecedor) + " ");
                    }
                }
            }
        }

        private static void AdicionaFiltroDespesa(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.ContainsKey("Despesa") && !string.IsNullOrEmpty(request.Filters["Despesa"].ToString()))
            {
                sqlSelect.AppendLine("	AND l.id_cl_despesa_tipo IN (" + Utils.MySqlEscapeNumberToIn(request.Filters["Despesa"].ToString()) + ") ");
            }
        }

        private static void AdicionaFiltroDeputado(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.ContainsKey("IdParlamentar") && !string.IsNullOrEmpty(request.Filters["IdParlamentar"].ToString()))
            {
                sqlSelect.AppendLine("	AND l.id_cl_deputado IN (" + Utils.MySqlEscapeNumberToIn(request.Filters["IdParlamentar"].ToString()) + ") ");
            }
        }

        private static void AdicionaFiltroDocumento(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.ContainsKey("Documento") && !string.IsNullOrEmpty(request.Filters["Documento"].ToString()))
            {
                sqlSelect.AppendLine("	AND l.numero_documento like '%" + Utils.MySqlEscape(request.Filters["Documento"].ToString()) + "' ");
            }
        }

        private static void AdicionaResultadoComum(DataTablesRequest request, StringBuilder sqlSelect)
        {
            sqlSelect.AppendFormat(" ORDER BY {0} ", request.GetSorting("valor_total desc"));
            sqlSelect.AppendFormat(" LIMIT {0},{1}; ", request.Start, request.Length);

            sqlSelect.AppendLine("SELECT FOUND_ROWS();");
        }

        public async Task<dynamic> TipoDespesa()
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine("SELECT id, descricao FROM cl_despesa_tipo ");
                strSql.AppendFormat("ORDER BY descricao ");

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(strSql.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id = reader["id"].ToString(),
                            text = reader["descricao"].ToString(),
                        });
                    }
                }
                return lstRetorno;
            }
        }
    }
}