using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OPS.Core.Models;

namespace OPS.Core.Repository
{
    public class PresidenciaRepository
    {
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
                case EnumAgrupamentoAuditoria.Documento:
                    return await LancamentosNotaFiscal(request);
                case EnumAgrupamentoAuditoria.Ano:
                    return await LancamentosAno(request);
                default:
                    break;
            }

            throw new BusinessException("Parâmetro request.Agrupamento não informado!");
        }

        private async Task<dynamic> LancamentosAno(DataTablesRequest request)
        {
            using (AppDb banco = new AppDb())
            {
                var sqlSelect = new StringBuilder();

                sqlSelect.AppendLine(@"
                    SELECT SQL_CALC_FOUND_ROWS
						 null as id_ano
						, l1.ano
						, l1.total_notas
						, l1.valor_total
                    FROM (
					    SELECT 
						    count(l.id) AS total_notas
					        , sum(l.valor_liquido) as valor_total
					        , l.ano
					    FROM pr_despesa l
					    WHERE (1=1)
				");

                AdicionaFiltroAno(request, sqlSelect);
                AdicionaFiltroServidor(request, sqlSelect);
                AdicionaFiltroDespesa(request, sqlSelect);
                AdicionaFiltroFornecedor(request, sqlSelect);

                sqlSelect.AppendLine(@"
					GROUP BY ano
                    ) l1
                ");

                AdicionaResultadoComum(request, sqlSelect);

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(sqlSelect.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_ano = reader["ano"],
                            ano = reader["ano"],
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

        private async Task<dynamic> LancamentosParlamentar(DataTablesRequest request)
        {
            using (AppDb banco = new AppDb())
            {
                var sqlSelect = new StringBuilder();

                sqlSelect.AppendLine(@"
                    SELECT SQL_CALC_FOUND_ROWS
						 d.id as id_pessoa_servidor
						, ifnull(d.nome, d.cpf) as nome_servidor
						, l1.total_notas
						, l1.valor_total
                    FROM (
					    SELECT 
						    count(l.id) AS total_notas
					        , sum(l.valor_liquido) as valor_total
					        , l.id_pessoa_servidor
					    FROM pr_despesa l
					    WHERE (1=1)
				");

                AdicionaFiltroAno(request, sqlSelect);
                AdicionaFiltroServidor(request, sqlSelect);
                AdicionaFiltroDespesa(request, sqlSelect);
                AdicionaFiltroFornecedor(request, sqlSelect);

                sqlSelect.AppendLine(@"
					GROUP BY id_pessoa_servidor
                    ) l1
					JOIN pessoa d on d.id = l1.id_pessoa_servidor
                ");

                AdicionaResultadoComum(request, sqlSelect);

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(sqlSelect.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_pessoa_servidor = reader["id_pessoa_servidor"],
                            nome_servidor = reader["nome_servidor"].ToString(),
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
						l1.id_pr_despesa_tipo
						, td.descricao
						, l1.total_notas
						, l1.valor_total
					from (
					SELECT
						count(l.id) AS total_notas
						, sum(l.valor_liquido) as valor_total
						, l.id_pr_despesa_tipo
					FROM pr_despesa l
					WHERE (1=1)
				");

                AdicionaFiltroAno(request, sqlSelect);
                AdicionaFiltroServidor(request, sqlSelect);
                AdicionaFiltroDespesa(request, sqlSelect);
                AdicionaFiltroFornecedor(request, sqlSelect);

                sqlSelect.AppendLine(@"
					GROUP BY id_pr_despesa_tipo
					) l1
					LEFT JOIN pr_despesa_tipo td on td.id = l1.id_pr_despesa_tipo
				");

                AdicionaResultadoComum(request, sqlSelect);

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(sqlSelect.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_pr_despesa_tipo = reader["id_pr_despesa_tipo"],
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
					    FROM pr_despesa l
					    WHERE (1=1)
				");

                AdicionaFiltroAno(request, sqlSelect);
                AdicionaFiltroServidor(request, sqlSelect);
                AdicionaFiltroDespesa(request, sqlSelect);
                AdicionaFiltroFornecedor(request, sqlSelect);



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
                            nome_fornecedor = reader["nome_fornecedor"],
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

                AdicionaFiltroAno(request, sqlWhere);
                AdicionaFiltroServidor(request, sqlWhere);
                AdicionaFiltroDespesa(request, sqlWhere);
                AdicionaFiltroFornecedor(request, sqlWhere);
                AdicionaFiltroDocumento(request, sqlWhere);

                var sqlSelect = new StringBuilder();

                sqlSelect.AppendLine(@"
					SELECT SQL_CALC_FOUND_ROWS
						l.data_pgto
						, pj.nome AS nome_fornecedor
						, ifnull(d.nome, d.cpf) as nome_servidor
						, l.valor_liquido
                        , l.id as id_pr_despesa
                        , l.id_fornecedor
                        , pj.cnpj_cpf
                        , d.id as id_pessoa_servidor
					FROM (
						SELECT data_pgto, id, id_pessoa_servidor, valor_liquido, id_pr_despesa_tipo, id_fornecedor
						FROM pr_despesa l
						WHERE (1=1)
				");

                sqlSelect.AppendLine(sqlWhere.ToString());

                sqlSelect.AppendFormat(" ORDER BY {0} ", request.GetSorting("l.ano DESC, l.data_pgto DESC, l.valor_liquido DESC"));
                sqlSelect.AppendFormat(" LIMIT {0},{1} ", request.Start, request.Length);

                sqlSelect.AppendLine(@" ) l
					INNER JOIN pessoa d on d.id = l.id_pessoa_servidor
					LEFT JOIN fornecedor pj on pj.id = l.id_fornecedor;

                    SELECT COUNT(1) FROM pr_despesa l WHERE (1=1) ");

                sqlSelect.AppendLine(sqlWhere.ToString());

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(sqlSelect.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_pr_despesa = reader["id_pr_despesa"],
                            data_pgto = Utils.FormataData(reader["data_pgto"]),
                            id_fornecedor = reader["id_fornecedor"],
                            cnpj_cpf = Utils.FormatCnpjCpf(reader["cnpj_cpf"].ToString()),
                            nome_fornecedor = reader["nome_fornecedor"].ToString(),
                            id_pessoa_servidor = reader["id_pessoa_servidor"],
                            nome_servidor = reader["nome_servidor"].ToString(),
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

        private static void AdicionaFiltroAno(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.ContainsKey("Ano"))
            {
                string anos = request.Filters["Ano"].ToString();
                sqlSelect.AppendLine("	AND l.ano IN(" + Utils.MySqlEscapeNumberToIn(anos) + ") ");
            }
        }

        private static void AdicionaFiltroFornecedor(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.ContainsKey("Fornecedor") && !string.IsNullOrEmpty(request.Filters["Fornecedor"].ToString()))
            {
                var Fornecedor = String.Join("", System.Text.RegularExpressions.Regex.Split(request.Filters["Fornecedor"].ToString(), @"[^\d]"));

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
                sqlSelect.AppendLine("	AND l.id_pr_despesa_tipo IN (" + Utils.MySqlEscapeNumberToIn(request.Filters["Despesa"].ToString()) + ") ");
            }
        }

        private static void AdicionaFiltroServidor(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.ContainsKey("Servidor") && !string.IsNullOrEmpty(request.Filters["Servidor"].ToString()))
            {
                sqlSelect.AppendLine("	AND l.id_pessoa_servidor IN (" + Utils.MySqlEscapeNumberToIn(request.Filters["Servidor"].ToString()) + ") ");
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


        public Task<dynamic> Servidor(MultiSelectRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<dynamic> TipoDespesa()
        {
            throw new NotImplementedException();
        }
    }
}
