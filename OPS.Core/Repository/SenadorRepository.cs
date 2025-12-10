using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using OPS.Core.DTO;
using OPS.Core.Enumerator;
using OPS.Core.Utilities;

namespace OPS.Core.Repository
{
    public class SenadorRepository
    {
        public async Task<dynamic> Consultar(int id)
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = @"
					SELECT 
						d.id as id_sf_senador
						, d.nome as nome_parlamentar
						, d.nome_completo as nome_civil
						, d.nascimento
						, d.sexo
						, d.id_partido
						, p.sigla as sigla_partido
						, p.nome as nome_partido
						, d.id_estado
						, e.sigla as sigla_estado
						, e.nome as nome_estado
						, d.email
						, d.valor_total_ceaps
                        , d.valor_total_remuneracao
                        , d.valor_total_remuneracao + d.valor_total_ceaps as valor_total
                        , d.ativo
                        , (SELECT m.participacao from sf_mandato m WHERE m.id_sf_senador = d.id ORDER BY m.id desc LIMIT 1) as participacao
                        , d.naturalidade
                        , e.sigla as silga_estado_naturalidade
					FROM sf_senador d
					LEFT JOIN partido p on p.id = d.id_partido
					LEFT JOIN estado e on e.id = d.id_estado
					LEFT JOIN estado en on e.id = d.id_estado_naturalidade
					WHERE d.id = @id
				";
                banco.AddParameter("@id", id);

                using (var reader = await banco.ExecuteReaderAsync(strSql))
                {
                    if (await reader.ReadAsync())
                    {
                        var exercicio = "";
                        switch (reader["ativo"].ToString())
                        {
                            case "S": exercicio = "Em exercício"; break;
                            case "N": exercicio = "Fora de Exercício"; break;
                        }

                        var participacao = "";
                        switch (reader["participacao"].ToString())
                        {
                            case "T": participacao = "Titular"; break;
                            case "1": participacao = "1º Suplente"; break;
                            case "2": participacao = "2º Suplente"; break;
                        }

                        return new
                        {
                            id_sf_senador = reader["id_sf_senador"],
                            nome_parlamentar = reader["nome_parlamentar"].ToString(),
                            nome_civil = reader["nome_civil"].ToString(),
                            nascimento = Utils.NascimentoFormatado(reader["nascimento"]),
                            sexo = reader["sexo"].ToString(),
                            id_partido = reader["id_partido"],
                            sigla_estado = reader["sigla_estado"].ToString(),
                            nome_partido = reader["nome_partido"].ToString(),
                            id_estado = reader["id_estado"],
                            sigla_partido = reader["sigla_partido"].ToString(),
                            nome_estado = reader["nome_estado"].ToString(),
                            email = reader["email"].ToString(),
                            condicao = $"{participacao} ({exercicio})",
                            naturalidade = reader["naturalidade"].ToString() + (!string.IsNullOrEmpty(reader["silga_estado_naturalidade"].ToString()) ? "(" + reader["silga_estado_naturalidade"].ToString() + ")" : ""),
                            valor_total_ceaps = Utils.FormataValor(reader["valor_total_ceaps"]),
                            valor_total_remuneracao = Utils.FormataValor(reader["valor_total_remuneracao"]),
                            valor_total = Utils.FormataValor(reader["valor_total"]),
                        };
                    }

                    return null;
                }
            }
        }

        public async Task<dynamic> Lista(FiltroParlamentarDTO request)
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT 
						d.id as id_sf_senador
						, d.nome as nome_parlamentar 
						, d.nome_completo as nome_civil
						, p.sigla as sigla_partido
						, p.nome as nome_partido
						, e.sigla as sigla_estado
						, e.nome as nome_estado
                        , d.valor_total_ceaps
                        , d.valor_total_remuneracao
                        , d.ativo
					FROM sf_senador d
					LEFT JOIN partido p on p.id = d.id_partido
					LEFT JOIN estado e on e.id = d.id_estado
                    JOIN sf_mandato m ON m.id_sf_senador = d.id
					JOIN sf_mandato_legislatura ml on ml.id_sf_mandato = m.id
                    WHERE m.exerceu = 1
                    AND ml.id_sf_legislatura = " + request.Periodo.ToString());

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
                    -- LIMIT 1000
				");

                TextInfo textInfo = new CultureInfo("pt-BR", false).TextInfo;
                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(strSql.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_sf_senador = reader["id_sf_senador"],
                            nome_parlamentar = reader["nome_parlamentar"].ToString(),
                            nome_civil = reader["nome_civil"].ToString(),
                            sigla_partido = !string.IsNullOrEmpty(reader["sigla_partido"].ToString()) ? reader["sigla_partido"].ToString() : "S.PART.",
                            nome_partido = !string.IsNullOrEmpty(reader["nome_partido"].ToString()) ? reader["nome_partido"].ToString() : "SEM PARTIDO",
                            sigla_estado = reader["sigla_estado"].ToString(),
                            nome_estado = reader["nome_estado"].ToString(),
                            ativo = reader["ativo"].ToString() == "S",
                            situacao = reader["ativo"].ToString() == "S" ? "Em exercício" : "Fora de Exercício",

                            valor_total_ceaps = Utils.FormataValor(reader["valor_total_ceaps"]),
                            valor_total_remuneracao = Utils.FormataValor(reader["valor_total_remuneracao"]),
                        }); ;
                    }
                }
                return lstRetorno;
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
					from (
						SELECT
							l.id_fornecedor
							, SUM(l.valor) as valor_total
						FROM sf_despesa l
						WHERE l.id_sf_senador = @id
						GROUP BY l.id_fornecedor
						order by valor_total desc
						LIMIT 10
					) l1
					LEFT JOIN fornecedor pj on pj.id = l1.id_fornecedor
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
						 l1.id as id_sf_despesa
						, l1.id_fornecedor
						, pj.cnpj_cpf
						, pj.nome AS nome_fornecedor
						, l1.valor
					from (
						SELECT
						l.id
						, l.valor
						, l.id_fornecedor
						FROM sf_despesa l
						WHERE l.id_sf_senador = @id
						order by l.valor desc
						LIMIT 10
					) l1
					LEFT JOIN fornecedor pj on pj.id = l1.id_fornecedor
					order by l1.valor desc 
				");

                banco.AddParameter("@id", id);

                using (DbDataReader reader = await banco.ExecuteReaderAsync(strSql.ToString()))
                {
                    List<dynamic> lstRetorno = new List<dynamic>();
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_sf_despesa = reader["id_sf_despesa"].ToString(),
                            id_fornecedor = reader["id_fornecedor"].ToString(),
                            cnpj_cpf = Utils.FormatCnpjCpf(reader["cnpj_cpf"].ToString()),
                            nome_fornecedor = reader["nome_fornecedor"].ToString(),
                            valor = Utils.FormataValor(reader["valor"])
                        });
                    }

                    return lstRetorno;
                }
            }
        }

        public async Task<dynamic> GastosPorAno(int id)
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT d.ano, SUM(d.valor) AS valor_total
					FROM sf_despesa d
					WHERE d.id_sf_senador = @id
					group by d.ano
					order by d.ano
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
                //    var lstValoresMensais = new decimal?[12];
                //    string anoControle = string.Empty;
                //    bool existeGastoNoAno = false;
                //    var categories = new List<dynamic>();
                //    var series = new List<dynamic>();

                //    while (await reader.ReadAsync())
                //    {
                //        if (reader["ano"].ToString() != anoControle)
                //        {
                //            if (existeGastoNoAno)
                //            {
                //                categories.Add(anoControle);
                //                series.Add(new
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
                //        categories.Add(anoControle);
                //        series.Add(new
                //        {
                //            name = anoControle.ToString(),
                //            data = lstValoresMensais
                //        });
                //    }

                //    return new
                //    {
                //        categories,
                //        series
                //    };
                //}
            }
        }

        public async Task<dynamic> GastosComPessoalPorAno(int id)
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT d.ano, SUM(d.valor) AS valor_total
					FROM cf_senador_verba_gabinete d
					WHERE d.id_sf_senador = @id
					group by d.ano
					order by d.ano
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
            }
        }

        public async Task<dynamic> Busca(string value)
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT 
						s.id as id_sf_senador
						, s.nome
						, s.nome_completo
						, s.valor_total_ceaps
						, s.id_partido
						, p.sigla as sigla_partido
						, p.nome as nome_partido
						, s.id_estado
						, e.sigla as sigla_estado
						, e.nome as nome_estado
					FROM sf_senador s
					LEFT JOIN partido p on p.id = s.id_partido
					LEFT JOIN estado e on e.id = s.id_estado
                    WHERE valor_total_ceaps > 0");

                if (!string.IsNullOrEmpty(value))
                {
                    strSql.AppendLine("	AND (s.nome like '%" + Utils.MySqlEscape(value) + "%' or s.nome_completo like '%" + Utils.MySqlEscape(value) + "%')");
                }

                strSql.AppendLine(@"
                    ORDER BY nome
                    limit 100
				");

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(strSql.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_sf_senador = reader["id_sf_senador"],
                            nome = reader["nome"].ToString(),
                            nome_completo = reader["nome_completo"].ToString(),
                            valor_total_ceaps = Utils.FormataValor(reader["valor_total_ceaps"]),
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
						d.id, d.nome, d.nome_completo
					FROM sf_senador d
				");

                if (filtro != null && string.IsNullOrEmpty(filtro.Ids))
                {
                    strSql.AppendLine(@"
                        WHERE (1=1) ");

                    if (!string.IsNullOrEmpty(filtro.Busca))
                    {
                        var busca = Utils.MySqlEscape(filtro.Busca);
                        strSql.AppendLine(@" AND (d.nome like '%" + busca + "%' or d.nome_completo like '%" + busca + "%') ");
                    }

                    if (filtro.Periodo > 0)
                    {
                        strSql.AppendLine($" AND d.id IN(select m.id_sf_senador from sf_mandato m JOIN sf_mandato_legislatura ml on ml.id_sf_mandato = m.id and m.exerceu = 1 where ml.id_sf_legislatura = {filtro.Periodo.ToString()})");
                    }

                    strSql.AppendLine(@"
                        ORDER BY d.nome
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
                    else
                    {
                        strSql.AppendLine(@"
                            AND d.id IN(
                                /* Somente senadores com despesas */
                                select distinct id_sf_senador
                                from sf_despesa
                            ) ");
                    }

                    strSql.AppendLine(@"
                        ORDER BY d.nome
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
                            tokens = new[] { reader["nome"].ToString(), reader["nome_completo"].ToString() },
                            text = reader["nome"].ToString()
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
						 d.id as id_sf_senador
						, d.nome as nome_parlamentar
						, e.sigla as sigla_estado
						, p.sigla as sigla_partido
						, l1.total_notas
						, l1.valor_total
					FROM (
					    SELECT 
						    count(l.id) AS total_notas
					    , sum(l.valor) as valor_total
					    , l.id_sf_senador
					    FROM sf_despesa l
					    WHERE (1=1)
				");

                AdicionaFiltroPeriodo(request, sqlSelect);
                AdicionaFiltroSenador(request, sqlSelect);
                AdicionaFiltroDespesa(request, sqlSelect);
                AdicionaFiltroFornecedor(request, sqlSelect);
                AdicionaFiltroPartidoSenador(request, sqlSelect);
                AdicionaFiltroEstadoSenador(request, sqlSelect);

                sqlSelect.AppendLine(@"
					    GROUP BY id_sf_senador
                    ) l1
					LEFT JOIN sf_senador d on d.id = l1.id_sf_senador
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
                            id_sf_senador = reader["id_sf_senador"],
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
						    , sum(l.valor) as valor_total
					    FROM sf_despesa l
					    WHERE (1=1)
				");

                AdicionaFiltroPeriodo(request, sqlSelect);
                AdicionaFiltroSenador(request, sqlSelect);
                AdicionaFiltroDespesa(request, sqlSelect);
                AdicionaFiltroFornecedor(request, sqlSelect);
                AdicionaFiltroPartidoSenador(request, sqlSelect);
                AdicionaFiltroEstadoSenador(request, sqlSelect);

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
                            id_fornecedor = reader["id_fornecedor"],
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
                    SELECT SQL_CALC_FOUND_ROWS
						l1.id_sf_despesa_tipo
						, td.descricao
						, l1.total_notas
						, l1.valor_total
					FROM (
					    SELECT 
						    count(l.id) AS total_notas
						    , sum(l.valor) as valor_total
						    , l.id_sf_despesa_tipo
					    FROM sf_despesa l
					    WHERE (1=1)
				");

                AdicionaFiltroPeriodo(request, sqlSelect);
                AdicionaFiltroSenador(request, sqlSelect);
                AdicionaFiltroDespesa(request, sqlSelect);
                AdicionaFiltroFornecedor(request, sqlSelect);
                AdicionaFiltroPartidoSenador(request, sqlSelect);
                AdicionaFiltroEstadoSenador(request, sqlSelect);

                sqlSelect.AppendLine(@"
					    GROUP BY id_sf_despesa_tipo
                    ) l1
					LEFT JOIN sf_despesa_tipo td on td.id = l1.id_sf_despesa_tipo
				");

                AdicionaResultadoComum(request, sqlSelect);

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(sqlSelect.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_sf_despesa_tipo = reader["id_sf_despesa_tipo"],
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
					    , p.nome as nome_partido
					    , sum(l1.total_notas) as total_notas
                        , count(l1.id_sf_senador) as total_senadores
                        , sum(l1.valor_total) / count(l1.id_sf_senador) as valor_medio_por_senador
					    , sum(l1.valor_total) as valor_total
					FROM (
						SELECT
							count(l.id) AS total_notas
						    , sum(l.valor) as valor_total
						    , l.id_sf_senador
						FROM sf_despesa l
						WHERE (1=1)
				");

                AdicionaFiltroPeriodo(request, sqlSelect);
                AdicionaFiltroSenador(request, sqlSelect);
                AdicionaFiltroDespesa(request, sqlSelect);
                AdicionaFiltroFornecedor(request, sqlSelect);
                AdicionaFiltroPartidoSenador(request, sqlSelect);
                AdicionaFiltroEstadoSenador(request, sqlSelect);

                sqlSelect.AppendLine(@"
						GROUP BY id_sf_senador
					) l1
					INNER JOIN sf_senador d on d.id = l1.id_sf_senador
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
                            nome_partido = reader["nome_partido"].ToString(),
                            total_notas = Utils.FormataValor(reader["total_notas"], 0),
                            total_senadores = Utils.FormataValor(reader["total_senadores"], 0),
                            valor_medio_por_senador = Utils.FormataValor(reader["valor_medio_por_senador"]),
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
						e.id as id_estado
					    , e.nome as nome_estado
					    , count(l1.total_notas) as total_notas
					    , sum(l1.valor_total) as valor_total
					FROM (
						SELECT
							count(l.id) AS total_notas
						    , sum(l.valor) as valor_total
						    , l.id_sf_senador
						FROM sf_despesa l
						WHERE (1=1)
				");

                AdicionaFiltroPeriodo(request, sqlSelect);
                AdicionaFiltroSenador(request, sqlSelect);
                AdicionaFiltroDespesa(request, sqlSelect);
                AdicionaFiltroFornecedor(request, sqlSelect);
                AdicionaFiltroPartidoSenador(request, sqlSelect);
                AdicionaFiltroEstadoSenador(request, sqlSelect);

                sqlSelect.AppendLine(@"
						GROUP BY id_sf_senador
					) l1
					JOIN sf_senador d on d.id = l1.id_sf_senador
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
                            nome_estado = reader["nome_estado"].ToString(),
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
            var sqlWhere = new StringBuilder();
            AdicionaFiltroPeriodo(request, sqlWhere);
            AdicionaFiltroSenador(request, sqlWhere);
            AdicionaFiltroDespesa(request, sqlWhere);
            AdicionaFiltroFornecedor(request, sqlWhere);
            AdicionaFiltroPartidoSenador(request, sqlWhere);
            AdicionaFiltroEstadoSenador(request, sqlWhere);

            using (AppDb banco = new AppDb())
            {
                var sqlSelect = new StringBuilder();

                sqlSelect.AppendLine(@"
					SELECT
						l.data_emissao
						, pj.nome AS nome_fornecedor
						, d.nome as nome_parlamentar
						, l.valor as valor_total
                        , d.id as id_senador
                        , l.id_fornecedor
                        , l.id as id_sf_despesa
                        , pj.cnpj_cpf
                        , e.sigla as sigla_estado
						, p.sigla as sigla_partido
					FROM sf_despesa l
					JOIN sf_senador d on d.id = l.id_sf_senador
					LEFT JOIN fornecedor pj on pj.id = l.id_fornecedor
                    LEFT JOIN partido p on p.id = d.id_partido
					LEFT JOIN estado e on e.id = d.id_estado
					WHERE (1=1)
				");

                sqlSelect.Append(sqlWhere);

                sqlSelect.AppendFormat(" ORDER BY {0} ", request.GetSorting("l.ano_mes DESC, l.data_emissao DESC, l.valor DESC"));
                sqlSelect.AppendFormat(" LIMIT {0},{1}; ", request.Start, request.Length);

                sqlSelect.AppendLine(
                    @"SELECT count(1) FROM sf_despesa l WHERE (1=1) ");

                sqlSelect.Append(sqlWhere);

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(sqlSelect.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_sf_despesa = reader["id_sf_despesa"],
                            data_emissao = Utils.FormataData(reader["data_emissao"]),
                            id_fornecedor = reader["id_fornecedor"],
                            cnpj_cpf = Utils.FormatCnpjCpf(reader["cnpj_cpf"].ToString()),
                            nome_fornecedor = reader["nome_fornecedor"].ToString(),
                            id_sf_senador = reader["id_senador"],
                            nome_parlamentar = reader["nome_parlamentar"].ToString(),
                            sigla_estado = reader["sigla_estado"].ToString(),
                            sigla_partido = reader["sigla_partido"].ToString(),
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

        private static void AdicionaFiltroPeriodo(DataTablesRequest request, StringBuilder sqlSelect)
        {
            //DateTime dataIni = DateTime.Today;
            //DateTime dataFim = DateTime.Today;
            switch (request.Filters["Periodo"].ToString())
            {
                //case "1": //PERIODO_MES_ATUAL
                //    sqlSelect.AppendLine(" AND l.ano_mes = " + dataIni.ToString("yyyyMM"));
                //    break;

                //case "2": //PERIODO_MES_ANTERIOR
                //    dataIni = dataIni.AddMonths(-1);
                //    sqlSelect.AppendLine(" AND l.ano_mes = " + dataIni.ToString("yyyyMM"));
                //    break;

                //case "3": //PERIODO_MES_ULT_4
                //    dataIni = dataIni.AddMonths(-3);
                //    sqlSelect.AppendLine(" AND l.ano_mes >= " + dataIni.ToString("yyyyMM"));
                //    break;

                //case "4": //PERIODO_ANO_ATUAL
                //    dataIni = new DateTime(dataIni.Year, 1, 1);
                //    sqlSelect.AppendLine(string.Format(" AND l.ano_mes >= {0}01", dataIni.Year));
                //    break;

                //case "5": //PERIODO_ANO_ANTERIOR
                //    sqlSelect.AppendLine(string.Format(" AND l.ano_mes BETWEEN {0}01 AND {0}12", dataIni.Year - 1));
                //    break;
                case "57": //PERIODO_MANDATO_56
                    sqlSelect.AppendLine(" AND l.ano_mes BETWEEN 202302 AND 202701");
                    break;

                case "56": //PERIODO_MANDATO_56
                    sqlSelect.AppendLine(" AND l.ano_mes BETWEEN 201902 AND 202301");
                    break;

                case "55": //PERIODO_MANDATO_55
                    sqlSelect.AppendLine(" AND l.ano_mes BETWEEN 201502 AND 201901");
                    break;

                case "54": //PERIODO_MANDATO_54
                    sqlSelect.AppendLine(" AND l.ano_mes BETWEEN 201102 AND 201501");
                    break;

                case "53": //PERIODO_MANDATO_53
                    sqlSelect.AppendLine(" AND l.ano_mes BETWEEN 200702 AND 201101");
                    break;
            }
        }

        private static void AdicionaFiltroEstadoSenador(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.ContainsKey("Estado") && !string.IsNullOrEmpty(request.Filters["Estado"].ToString()))
            {
                sqlSelect.AppendLine("	AND l.id_sf_senador IN (SELECT id FROM sf_senador where id_estado IN(" + request.Filters["Estado"].ToString() + ")) ");
            }
        }

        private static void AdicionaFiltroPartidoSenador(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.ContainsKey("Partido") && !string.IsNullOrEmpty(request.Filters["Partido"].ToString()))
            {
                sqlSelect.AppendLine("	AND l.id_sf_senador IN (SELECT id FROM sf_senador where id_partido IN(" + request.Filters["Partido"].ToString() + ")) ");
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
                            var id_fornecedor =
                                banco.ExecuteScalar("select id from fornecedor where cnpj_cpf = '" + Utils.RemoveCaracteresNaoNumericos(Fornecedor) + "'");

                            if (!Convert.IsDBNull(id_fornecedor))
                            {
                                sqlSelect.AppendLine("	AND l.id_fornecedor =" + id_fornecedor + " ");
                            }
                        }
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
                sqlSelect.AppendLine("	AND l.id_sf_despesa_tipo IN (" + Utils.MySqlEscapeNumberToIn(request.Filters["Despesa"].ToString()) + ") ");
            }
        }

        private static void AdicionaFiltroSenador(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.ContainsKey("IdParlamentar") && !string.IsNullOrEmpty(request.Filters["IdParlamentar"].ToString()))
            {
                sqlSelect.AppendLine("	AND l.id_sf_senador IN (" + Utils.MySqlEscapeNumberToIn(request.Filters["IdParlamentar"].ToString()) + ") ");
            }
        }

        private static void AdicionaResultadoComum(DataTablesRequest request, StringBuilder sqlSelect, string defaultSort = "valor_total desc")
        {
            sqlSelect.AppendFormat(" ORDER BY {0} ", request.GetSorting(defaultSort));
            sqlSelect.AppendFormat(" LIMIT {0},{1}; ", request.Start, request.Length);

            sqlSelect.AppendLine("SELECT FOUND_ROWS();");
        }

        public async Task<dynamic> TipoDespesa()
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine("SELECT id, descricao FROM sf_despesa_tipo ");
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

        public async Task<dynamic> SenadoResumoMensal()
        {
            using (AppDb banco = new AppDb())
            {
                using (DbDataReader reader = await banco.ExecuteReaderAsync(@"select ano, mes, valor from sf_despesa_resumo_mensal"))
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

        public async Task<dynamic> SenadoResumoAnual()
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					select ano, sum(valor) as valor
					from sf_despesa_resumo_mensal
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

        public async Task<dynamic> Remuneracao(DataTablesRequest request)
        {
            if (request == null) throw new BusinessException("Parâmetro request não informado!");
            string strSelectFiels, sqlGroupBy;

            AgrupamentoRemuneracaoSenado eAgrupamento = (AgrupamentoRemuneracaoSenado)Convert.ToInt32(request.Filters["ag"].ToString());
            switch (eAgrupamento)
            {
                case AgrupamentoRemuneracaoSenado.Lotacao:
                    strSelectFiels = "l.id, l.descricao";
                    sqlGroupBy = "GROUP BY r.id_lotacao";

                    break;
                case AgrupamentoRemuneracaoSenado.Cargo:
                    strSelectFiels = "cr.id, cr.descricao";
                    sqlGroupBy = "GROUP BY r.id_cargo";

                    break;
                case AgrupamentoRemuneracaoSenado.Categoria:
                    strSelectFiels = "ct.id, ct.descricao";
                    sqlGroupBy = "GROUP BY r.id_categoria";

                    break;
                case AgrupamentoRemuneracaoSenado.Vinculo:
                    strSelectFiels = "v.id, v.descricao";
                    sqlGroupBy = "GROUP BY r.id_vinculo";

                    break;
                case AgrupamentoRemuneracaoSenado.Ano:
                    strSelectFiels = "CAST(r.ano_mes/100 AS UNSIGNED) as id, CAST(r.ano_mes/100 AS UNSIGNED) as descricao";
                    sqlGroupBy = "GROUP BY CAST(r.ano_mes/100 AS UNSIGNED)";

                    break;
                case AgrupamentoRemuneracaoSenado.AnoMes:
                    strSelectFiels = "";
                    sqlGroupBy = "";

                    break;
                case AgrupamentoRemuneracaoSenado.Senador:
                    strSelectFiels = "s.id, s.nome as descricao";
                    sqlGroupBy = "GROUP BY s.id";

                    break;
                default:
                    throw new BusinessException("Parâmetro Agrupamento (ag) não informado!");
            }


            var sqlWhere = new StringBuilder();

            if (request.Filters.ContainsKey("lt") && !string.IsNullOrEmpty(request.Filters["lt"].ToString()))
            {
                sqlWhere.AppendLine("	AND r.id_lotacao IN(" + Utils.MySqlEscapeNumberToIn(request.Filters["lt"].ToString()) + ") ");
            }
            if (request.Filters.ContainsKey("cr") && !string.IsNullOrEmpty(request.Filters["cr"].ToString()))
            {
                sqlWhere.AppendLine("	AND r.id_cargo IN(" + Utils.MySqlEscapeNumberToIn(request.Filters["cr"].ToString()) + ") ");
            }
            if (request.Filters.ContainsKey("ct") && !string.IsNullOrEmpty(request.Filters["ct"].ToString()))
            {
                sqlWhere.AppendLine("	AND r.id_categoria IN(" + Utils.MySqlEscapeNumberToIn(request.Filters["ct"].ToString()) + ") ");
            }
            if (request.Filters.ContainsKey("vn") && !string.IsNullOrEmpty(request.Filters["vn"].ToString()))
            {
                sqlWhere.AppendLine("	AND r.id_vinculo IN(" + Utils.MySqlEscapeNumberToIn(request.Filters["vn"].ToString()) + ") ");
            }
            if (request.Filters.ContainsKey("sn") && !string.IsNullOrEmpty(request.Filters["sn"].ToString()))
            {
                sqlWhere.AppendLine("	AND s.id IN(" + Utils.MySqlEscapeNumberToIn(request.Filters["sn"].ToString()) + ") ");
            }

            if (request.Filters.ContainsKey("ms") && !string.IsNullOrEmpty(request.Filters["ms"].ToString()))
            {
                sqlWhere.AppendLine("	AND r.ano_mes = " + Convert.ToInt32(request.Filters["an"].ToString()).ToString() + Convert.ToInt32(request.Filters["ms"].ToString()).ToString("d2") + " ");
            }
            else //if (request.Filters.ContainsKey("an") && !string.IsNullOrEmpty(request.Filters["an"].ToString()))
            {
                sqlWhere.AppendLine("	AND r.ano_mes BETWEEN " + Convert.ToInt32(request.Filters["an"].ToString()).ToString() + "01 AND " + request.Filters["an"].ToString() + "12 ");
            }

            if (eAgrupamento == AgrupamentoRemuneracaoSenado.Senador)
            {
                sqlWhere.AppendLine("	AND s.id IS NOT NULL ");
            }

            using (AppDb banco = new AppDb())
            {
                var sqlSelect = new StringBuilder();
                if (eAgrupamento != AgrupamentoRemuneracaoSenado.AnoMes)
                {
                    sqlSelect.AppendLine($@"
SELECT SQL_CALC_FOUND_ROWS
	{strSelectFiels},
    COUNT(1) AS quantidade,
    SUM(r.custo_total) AS valor_total
FROM sf_remuneracao r
JOIN sf_vinculo v ON v.id = r.id_vinculo
JOIN sf_categoria ct ON ct.id = r.id_categoria
LEFT JOIN sf_cargo cr ON cr.id = r.id_cargo 
JOIN sf_lotacao l ON l.id = r.id_lotacao
LEFT JOIN sf_senador s on s.id = l.id_senador
WHERE (1=1)
");
                }
                else
                {

                    sqlSelect.AppendLine(@"
SELECT SQL_CALC_FOUND_ROWS
    r.id,
	v.descricao as vinculo, 
	ct.descricao as categoria, 
	ct.descricao as cargo, 
	rc.descricao as referencia_cargo, 
	f.descricao as funcao, 
	l.descricao as lotacao, 
	tf.descricao as tipo_folha, 
	r.ano_mes, 
	r.custo_total as valor_total
FROM sf_remuneracao r
JOIN sf_lotacao l ON l.id = r.id_lotacao
JOIN sf_vinculo v ON v.id = r.id_vinculo
JOIN sf_categoria ct ON ct.id = r.id_categoria
JOIN sf_tipo_folha tf ON tf.id = r.id_tipo_folha
LEFT JOIN sf_cargo cr ON cr.id = r.id_cargo 
LEFT JOIN sf_referencia_cargo rc ON rc.id = r.id_referencia_cargo
LEFT JOIN sf_funcao f ON f.id = r.id_simbolo_funcao
LEFT JOIN sf_senador s on s.id = l.id_senador
WHERE (1=1) 
");
                }

                sqlSelect.Append(sqlWhere);
                sqlSelect.Append(sqlGroupBy);

                sqlSelect.AppendFormat(" ORDER BY {0} ", request.GetSorting(" valor_total desc"));
                sqlSelect.AppendFormat(" LIMIT {0},{1}; ", request.Start, request.Length);
                sqlSelect.AppendLine("SELECT FOUND_ROWS();");

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(sqlSelect.ToString()))
                {
                    if (eAgrupamento != AgrupamentoRemuneracaoSenado.AnoMes)
                    {
                        while (await reader.ReadAsync())
                        {
                            lstRetorno.Add(new
                            {
                                id = reader["id"],
                                descricao = reader["descricao"].ToString(),
                                quantidade = reader["quantidade"],
                                valor_total = Utils.FormataValor(reader["valor_total"])
                            });
                        }
                    }
                    else
                    {
                        while (await reader.ReadAsync())
                        {
                            lstRetorno.Add(new
                            {
                                id = reader["id"],
                                vinculo = reader["vinculo"].ToString(),
                                categoria = reader["categoria"].ToString(),
                                cargo = reader["cargo"].ToString(),
                                referencia_cargo = reader["referencia_cargo"].ToString(),
                                simbolo_funcao = reader["funcao"].ToString(),
                                lotacao = reader["lotacao"].ToString(),
                                tipo_folha = reader["tipo_folha"].ToString(),
                                ano_mes = Convert.ToInt32(reader["ano_mes"].ToString()).ToString("0000-00"),
                                valor_total = Utils.FormataValor(reader["valor_total"])
                            });
                        }
                    }

                    var TotalCount = reader.GetTotalRowsFound();
                    return new
                    {
                        draw = request.Draw,
                        recordsTotal = Convert.ToInt32(TotalCount),
                        recordsFiltered = Convert.ToInt32(TotalCount),
                        data = lstRetorno
                    };
                }
            }

        }

        public async Task<dynamic> Remuneracao(int id)
        {
            using (AppDb banco = new AppDb())
            {
                var sqlSelect = new StringBuilder();

                sqlSelect.AppendLine(@"
SELECT
    r.id,
    r.admissao,
    r.ano_mes, 
	v.descricao as vinculo, 
	ct.descricao as categoria, 
	ct.descricao as cargo, 
	rc.descricao as referencia_cargo, 
	f.descricao as funcao, 
	l.descricao as lotacao, 
	tf.descricao as tipo_folha, 
    r.remun_basica,
    r.vant_pessoais,
    r.func_comissionada,
    r.grat_natalina,
    r.horas_extras,
    r.outras_eventuais,
    r.abono_permanencia,
    r.reversao_teto_const,
    r.imposto_renda,
    r.previdencia,
    r.faltas,
    r.rem_liquida,
    r.diarias,
    r.auxilios,
    r.vant_indenizatorias,
    r.custo_total
FROM sf_remuneracao r
JOIN sf_lotacao l ON l.id = r.id_lotacao
JOIN sf_vinculo v ON v.id = r.id_vinculo
JOIN sf_categoria ct ON ct.id = r.id_categoria
JOIN sf_tipo_folha tf ON tf.id = r.id_tipo_folha
LEFT JOIN sf_cargo cr ON cr.id = r.id_cargo 
LEFT JOIN sf_referencia_cargo rc ON rc.id = r.id_referencia_cargo
LEFT JOIN sf_funcao f ON f.id = r.id_simbolo_funcao
WHERE r.id = @id
");

                banco.AddParameter("@id", id);
                using (DbDataReader reader = await banco.ExecuteReaderAsync(sqlSelect.ToString()))
                {
                    if (await reader.ReadAsync())
                    {
                        return new
                        {
                            id = reader["id"],
                            vinculo = reader["vinculo"].ToString(),
                            categoria = reader["categoria"].ToString(),
                            cargo = reader["cargo"].ToString(),
                            referencia_cargo = reader["referencia_cargo"].ToString(),
                            simbolo_funcao = reader["funcao"].ToString(),
                            lotacao = reader["lotacao"].ToString(),
                            tipo_folha = reader["tipo_folha"].ToString(),
                            ano_mes = Convert.ToInt32(reader["ano_mes"].ToString()).ToString("0000-00"),
                            admissao = Convert.ToInt32(reader["admissao"].ToString()),
                            remun_basica = Utils.FormataValor(reader["remun_basica"]),
                            vant_pessoais = Utils.FormataValor(reader["vant_pessoais"]),
                            func_comissionada = Utils.FormataValor(reader["func_comissionada"]),
                            grat_natalina = Utils.FormataValor(reader["grat_natalina"]),
                            horas_extras = Utils.FormataValor(reader["horas_extras"]),
                            outras_eventuais = Utils.FormataValor(reader["outras_eventuais"]),
                            abono_permanencia = Utils.FormataValor(reader["abono_permanencia"]),
                            reversao_teto_const = Utils.FormataValor(reader["reversao_teto_const"]),
                            imposto_renda = Utils.FormataValor(reader["imposto_renda"]),
                            previdencia = Utils.FormataValor(reader["previdencia"]),
                            faltas = Utils.FormataValor(reader["faltas"]),
                            rem_liquida = Utils.FormataValor(reader["rem_liquida"]),
                            diarias = Utils.FormataValor(reader["diarias"]),
                            auxilios = Utils.FormataValor(reader["auxilios"]),
                            vant_indenizatorias = Utils.FormataValor(reader["vant_indenizatorias"]),
                            custo_total = Utils.FormataValor(reader["custo_total"])
                        };
                    }
                }
            }

            return null;
        }

        public async Task<dynamic> Lotacao()
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine("SELECT id, descricao FROM sf_lotacao ");
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

        public async Task<dynamic> Cargo()
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine("SELECT id, descricao FROM sf_cargo ");
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

        public async Task<dynamic> Categoria()
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine("SELECT id, descricao FROM sf_categoria ");
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

        public async Task<dynamic> Vinculo()
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine("SELECT id, descricao FROM sf_vinculo ");
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