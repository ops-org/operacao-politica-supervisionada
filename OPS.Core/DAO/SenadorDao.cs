using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using OPS.Core;
using OPS.Core.DTO;

namespace OPS.Core.DAO
{
    public class SenadorDao
    {
        public async Task<dynamic> Consultar(int id)
        {
            using (Banco banco = new Banco())
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
					FROM sf_senador d
					LEFT JOIN partido p on p.id = d.id_partido
					LEFT JOIN estado e on e.id = d.id_estado
					WHERE d.id = @id
				";
                banco.AddParameter("@id", id);

                using (DbDataReader reader = await banco.ExecuteReaderAsync(strSql))
                {
                    if (await reader.ReadAsync())
                    {
                        return new
                        {
                            id_sf_senador = reader["id_sf_senador"],
                            nome_parlamentar = reader["nome_parlamentar"].ToString(),
                            nome_civil = reader["nome_civil"].ToString(),
                            nascimento = Utils.FormataData(reader["nascimento"]),
                            sexo = reader["sexo"].ToString(),
                            id_partido = reader["id_partido"],
                            sigla_estado = reader["sigla_estado"].ToString(),
                            nome_partido = reader["nome_partido"].ToString(),
                            id_estado = reader["id_estado"],
                            sigla_partido = reader["sigla_partido"].ToString(),
                            nome_estado = reader["nome_estado"].ToString(),
                            email = reader["email"].ToString(),

                            valor_total_ceaps = Utils.FormataValor(reader["valor_total_ceaps"]),
                        };
                    }

                    return null;
                }
            }
        }

        public async Task<dynamic> MaioresFornecedores(int id)
        {
            using (Banco banco = new Banco())
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
                            cnpj_cpf = reader["cnpj_cpf"].ToString(),
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
            using (Banco banco = new Banco())
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
                            cnpj_cpf = reader["cnpj_cpf"].ToString(),
                            nome_fornecedor = reader["nome_fornecedor"].ToString(),
                            valor = Utils.FormataValor(reader["valor"])
                        });
                    }

                    return lstRetorno;
                }
            }
        }

        public async Task<dynamic> GastosMensaisPorAno(int id)
        {
            using (Banco banco = new Banco())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT d.ano, d.mes, SUM(d.valor) AS valor_total
					FROM sf_despesa d
					WHERE d.id_sf_senador = @id
					group by d.ano, d.mes
					order by d.ano, d.mes
				");
                banco.AddParameter("@id", id);

                using (DbDataReader reader = await banco.ExecuteReaderAsync(strSql.ToString()))
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

                        if (Convert.ToDecimal(reader["valor_total"]) > 0)
                        {
                            lstValoresMensais[Convert.ToInt32(reader["mes"]) - 1] = Convert.ToDecimal(reader["valor_total"]);
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
                    // Ex: [{"$id":"1","name":"2015","data":[null,18404.57,25607.82,29331.99,36839.82,24001.68,40811.97,33641.20,57391.30,60477.07,90448.58,13285.14]}]
                }
            }
        }

        public async Task<dynamic> Busca(string value)
        {
            using (Banco banco = new Banco())
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

        public async Task<dynamic> Pesquisa()
        {
            using (Banco banco = new Banco())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT 
						id, nome, nome_completo
					FROM sf_senador
					WHERE id IN (
						/* Somente senadores com despesas */
						select distinct id_sf_senador 
						from sf_despesa
					)
					ORDER BY nome
				");

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
            Dictionary<int, string> dcFielsSort = null;

            EnumAgrupamentoAuditoria eAgrupamento = (EnumAgrupamentoAuditoria)Convert.ToInt32(request.Filters["Agrupamento"].ToString());
            switch (eAgrupamento)
            {
                case EnumAgrupamentoAuditoria.Parlamentar:
                    dcFielsSort = new Dictionary<int, string>(){
                        {1, "p.nome_parlamentar" },
                        {2, "p.quantidade_secretarios" },
                        {3, "p.custo_secretarios" },
                    };

                    return await LancamentosParlamentar(request, dcFielsSort);
                case EnumAgrupamentoAuditoria.Despesa:
                    dcFielsSort = new Dictionary<int, string>(){
                        {1, "p.nome_parlamentar" },
                        {2, "p.quantidade_secretarios" },
                        {3, "p.custo_secretarios" },
                    };

                    return await LancamentosDespesa(request, dcFielsSort);
                case EnumAgrupamentoAuditoria.Fornecedor:
                    dcFielsSort = new Dictionary<int, string>(){
                        {1, "p.nome_parlamentar" },
                        {2, "p.quantidade_secretarios" },
                        {3, "p.custo_secretarios" },
                    };

                    return await LancamentosFornecedor(request, dcFielsSort);
                case EnumAgrupamentoAuditoria.Partido:
                    dcFielsSort = new Dictionary<int, string>(){
                        {1, "p.nome_parlamentar" },
                        {2, "p.quantidade_secretarios" },
                        {3, "p.custo_secretarios" },
                    };

                    return await LancamentosPartido(request, dcFielsSort);
                case EnumAgrupamentoAuditoria.Uf:
                    dcFielsSort = new Dictionary<int, string>(){
                        {1, "p.nome_parlamentar" },
                        {2, "p.quantidade_secretarios" },
                        {3, "p.custo_secretarios" },
                    };

                    return await LancamentosEstado(request, dcFielsSort);
                case EnumAgrupamentoAuditoria.Documento:
                    dcFielsSort = new Dictionary<int, string>(){
                        {1, "p.nome_parlamentar" },
                        {2, "p.quantidade_secretarios" },
                        {3, "p.custo_secretarios" },
                    };

                    return await LancamentosNotaFiscal(request, dcFielsSort);
                default:
                    break;
            }

            throw new BusinessException("Parâmetro request.Agrupamento não informado!");
        }

        private async Task<dynamic> LancamentosParlamentar(DataTablesRequest request, Dictionary<int, string> dcFielsSort)
        {
            using (Banco banco = new Banco())
            {
                var sqlSelect = new StringBuilder();

                sqlSelect.AppendLine(@"
					DROP TABLE IF EXISTS table_in_memory;
					CREATE TEMPORARY TABLE table_in_memory
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
					GROUP BY id_sf_senador;
					
					SELECT
						 d.id as id_sf_senador
						, d.nome as nome_parlamentar
						, e.sigla as sigla_estado
						, p.sigla as sigla_partido
						, l1.total_notas
						, l1.valor_total
						from table_in_memory l1
					LEFT JOIN sf_senador d on d.id = l1.id_sf_senador
					LEFT JOIN partido p on p.id = d.id_partido
					LEFT JOIN estado e on e.id = d.id_estado
				");

                AdicionaResultadoComum(request, sqlSelect, dcFielsSort);

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
                            total_notas = reader["total_notas"],
                            valor_total = Utils.FormataValor(reader["valor_total"])
                        });
                    }

                    reader.NextResult();
                    await reader.ReadAsync();
                    string TotalCount = reader[0].ToString();
                    string ValorTotal = Utils.FormataValor(reader[1]);

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

        private async Task<dynamic> LancamentosFornecedor(DataTablesRequest request, Dictionary<int, string> dcFielsSort)
        {
            using (Banco banco = new Banco())
            {
                var sqlSelect = new StringBuilder();

                sqlSelect.AppendLine(@"
					DROP TABLE IF EXISTS table_in_memory;
					CREATE TEMPORARY TABLE table_in_memory
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
					GROUP BY l.id_fornecedor;

					select
						l1.id_fornecedor
						, pj.cnpj_cpf
						, pj.nome AS nome_fornecedor
						, l1.total_notas
						, l1.valor_total
					from table_in_memory l1
					LEFT JOIN fornecedor pj on pj.id = l1.id_fornecedor
				");

                AdicionaResultadoComum(request, sqlSelect, dcFielsSort);

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(sqlSelect.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            //SgUf = reader[SgUfOrdinal],
                            //DataUltimaNotaFiscal = Utils.FormataData(reader[DataUltimaNotaFiscalOrdinal]),
                            //Doador = reader[DoadorOrdinal],
                            id_fornecedor = reader["id_fornecedor"],
                            cnpj_cpf = reader["cnpj_cpf"],
                            nome_fornecedor = reader["nome_fornecedor"],
                            total_notas = reader["total_notas"],
                            valor_total = Utils.FormataValor(reader["valor_total"])
                        });
                    }

                    reader.NextResult();
                    await reader.ReadAsync();
                    string TotalCount = reader[0].ToString();
                    string ValorTotal = Utils.FormataValor(reader[1]);

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

        private async Task<dynamic> LancamentosDespesa(DataTablesRequest request, Dictionary<int, string> dcFielsSort)
        {
            using (Banco banco = new Banco())
            {
                var sqlSelect = new StringBuilder();

                sqlSelect.AppendLine(@"
					DROP TABLE IF EXISTS table_in_memory;
					CREATE TEMPORARY TABLE table_in_memory
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
					GROUP BY id_sf_despesa_tipo;
					
					SELECT
						l1.id_sf_despesa_tipo
						, td.descricao
						, l1.total_notas
						, l1.valor_total
					from table_in_memory l1
					LEFT JOIN sf_despesa_tipo td on td.id = l1.id_sf_despesa_tipo
				");

                AdicionaResultadoComum(request, sqlSelect, dcFielsSort);

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(sqlSelect.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_sf_despesa_tipo = reader["id_sf_despesa_tipo"],
                            descricao = reader["descricao"],
                            total_notas = reader["total_notas"],
                            valor_total = Utils.FormataValor(reader["valor_total"])
                        });
                    }

                    reader.NextResult();
                    await reader.ReadAsync();
                    string TotalCount = reader[0].ToString();
                    string ValorTotal = Utils.FormataValor(reader[1]);

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

        private async Task<dynamic> LancamentosPartido(DataTablesRequest request, Dictionary<int, string> dcFielsSort)
        {
            using (Banco banco = new Banco())
            {
                var sqlSelect = new StringBuilder();

                sqlSelect.AppendLine(@"
					DROP TABLE IF EXISTS table_in_memory;
					CREATE TEMPORARY TABLE table_in_memory
					AS (
						SELECT
						 d.id_partido
						, p.nome as nome_partido
						, sum(l1.total_notas) as total_notas
                        , count(l1.id_sf_senador) as total_senadores
                        , sum(l1.valor_total) / count(l1.id_sf_senador) as valor_medio_por_senador
						, sum(l1.valor_total) as valor_total
						from (
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
				);"); //end table_in_memory

                sqlSelect.AppendLine("select * from table_in_memory ");
                AdicionaResultadoComum(request, sqlSelect, dcFielsSort);

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(sqlSelect.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_partido = reader["id_partido"],
                            nome_partido = reader["nome_partido"].ToString(),
                            total_notas = reader["total_notas"],
                            total_senadores = reader["total_senadores"],
                            valor_medio_por_senador = Utils.FormataValor(reader["valor_medio_por_senador"]),
                            valor_total = Utils.FormataValor(reader["valor_total"])
                        });
                    }

                    reader.NextResult();
                    await reader.ReadAsync();
                    string TotalCount = reader[0].ToString();
                    string ValorTotal = Utils.FormataValor(reader[1]);

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

        private async Task<dynamic> LancamentosEstado(DataTablesRequest request, Dictionary<int, string> dcFielsSort)
        {
            using (Banco banco = new Banco())
            {
                var sqlSelect = new StringBuilder();

                sqlSelect.AppendLine(@"
					DROP TABLE IF EXISTS table_in_memory;
					CREATE TEMPORARY TABLE table_in_memory
					AS (
						SELECT
						 d.id_estado
						, e.nome as nome_estado
						, count(l1.total_notas) as total_notas
						, sum(l1.valor_total) as valor_total
						from (

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
					LEFT JOIN estado e on e.id = d.id_estado
					GROUP BY e.id, e.nome
				); "); //end table_in_memory

                sqlSelect.AppendLine(@"select * from table_in_memory ");
                AdicionaResultadoComum(request, sqlSelect, dcFielsSort);

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(sqlSelect.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_estado = reader["id_estado"],
                            nome_estado = reader["nome_estado"],
                            total_notas = reader["total_notas"],
                            valor_total = Utils.FormataValor(reader["valor_total"])
                        });
                    }

                    reader.NextResult();
                    await reader.ReadAsync();
                    string TotalCount = reader[0].ToString();
                    string ValorTotal = Utils.FormataValor(reader[1]);

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

        private async Task<dynamic> LancamentosNotaFiscal(DataTablesRequest request, Dictionary<int, string> dcFielsSort)
        {

            //sqlSelect.AppendLine(" p.IdeCadastro as IdCadastro");
            //sqlSelect.AppendLine(", p.nuDeputadoId as IdDeputado");
            //sqlSelect.AppendLine(", l.id as Id");
            //sqlSelect.AppendLine(", l.ideDocumento as IdDocumento");
            //sqlSelect.AppendLine(", l.txtNumero as NotaFiscal");
            //sqlSelect.AppendLine(", l.txtCNPJCPF AS Codigo");
            //sqlSelect.AppendLine(", l.numano as NumAno");
            //sqlSelect.AppendLine(", l.txtNumero as Numero");
            //sqlSelect.AppendLine(", l.datEmissao as DataEmissao");
            //sqlSelect.AppendLine(", SUBSTRING(IFNULL(f.txtbeneficiario, l.txtbeneficiario), 1, 50) AS NomeBeneficiario");
            //sqlSelect.AppendLine(", l.txNomeParlamentar as nome_parlamentar");
            //sqlSelect.AppendLine(", SUM(l.vlrLiquido) AS vlrTotal ");

            using (Banco banco = new Banco())
            {
                var sqlSelect = new StringBuilder();

                //sqlSelect.AppendLine("DROP TABLE IF EXISTS table_in_memory; ");
                //sqlSelect.AppendLine("CREATE TEMPORARY TABLE table_in_memory ");
                //sqlSelect.AppendLine("AS ( ");
                sqlSelect.AppendLine(@"
					SELECT SQL_CALC_FOUND_ROWS
						 l.id as id_sf_despesa
						, l.data_documento
						, l.id_fornecedor
						, pj.cnpj_cpf
						, pj.nome AS nome_fornecedor
						, d.id as id_senador
						, d.nome as nome_parlamentar
						, l.valor as valor_total
					FROM sf_despesa l
					LEFT JOIN sf_senador d on d.id = l.id_sf_senador
					LEFT JOIN fornecedor pj on pj.id = l.id_fornecedor
					WHERE (1=1)
				");

                AdicionaFiltroPeriodo(request, sqlSelect);

                AdicionaFiltroSenador(request, sqlSelect);

                AdicionaFiltroDespesa(request, sqlSelect);

                AdicionaFiltroFornecedor(request, sqlSelect);

                AdicionaFiltroPartidoSenador(request, sqlSelect);

                AdicionaFiltroEstadoSenador(request, sqlSelect);

                AdicionaResultadoComum(request, sqlSelect, dcFielsSort, "l.data_documento desc, l.valor desc");

                sqlSelect.AppendFormat("SELECT FOUND_ROWS();");

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(sqlSelect.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_sf_despesa = reader["id_sf_despesa"],
                            data_documento = Utils.FormataData(reader["data_documento"]),
                            id_fornecedor = reader["id_fornecedor"],
                            cnpj_cpf = reader["cnpj_cpf"],
                            nome_fornecedor = reader["nome_fornecedor"].ToString(),
                            id_senador = reader["id_senador"],
                            nome_parlamentar = reader["nome_parlamentar"].ToString(),
                            valor_total = Utils.FormataValor(reader["valor_total"])
                        });
                    }

                    reader.NextResult();
                    await reader.ReadAsync();
                    string TotalCount = reader[0].ToString();
                    string ValorTotal = Utils.FormataValor(reader[1]);

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

        private static void AdicionaFiltroPeriodo(DataTablesRequest request, StringBuilder sqlSelect)
        {
            DateTime dataIni = DateTime.Today;
            DateTime dataFim = DateTime.Today;
            switch (request.Filters["Periodo"].ToString())
            {
                case "1": //PERIODO_MES_ATUAL
                    sqlSelect.AppendLine(" AND l.ano_mes = " + dataIni.ToString("yyyyMM"));
                    break;

                case "2": //PERIODO_MES_ANTERIOR
                    dataIni = dataIni.AddMonths(-1);
                    sqlSelect.AppendLine(" AND l.ano_mes = " + dataIni.ToString("yyyyMM"));
                    break;

                case "3": //PERIODO_MES_ULT_4
                    dataIni = dataIni.AddMonths(-3);
                    sqlSelect.AppendLine(" AND l.ano_mes >= " + dataIni.ToString("yyyyMM"));
                    break;

                case "4": //PERIODO_ANO_ATUAL
                    dataIni = new DateTime(dataIni.Year, 1, 1);
                    sqlSelect.AppendLine(string.Format(" AND l.ano_mes >= {0}01", dataIni.Year));
                    break;

                case "5": //PERIODO_ANO_ANTERIOR
                    sqlSelect.AppendLine(string.Format(" AND l.ano_mes BETWEEN {0}01 AND {0}12", dataIni.Year - 1));
                    break;

                case "9": //PERIODO_MANDATO_56
                    sqlSelect.AppendLine(" AND l.ano_mes BETWEEN 201902 AND 202301");
                    break;

                case "8": //PERIODO_MANDATO_55
                    sqlSelect.AppendLine(" AND l.ano_mes BETWEEN 201502 AND 201901");
                    break;

                case "7": //PERIODO_MANDATO_54
                    sqlSelect.AppendLine(" AND l.ano_mes BETWEEN 201102 AND 201501");
                    break;

                case "6": //PERIODO_MANDATO_53
                    sqlSelect.AppendLine(" AND l.ano_mes BETWEEN 200702 AND 201101");
                    break;
            }
        }

        private static void AdicionaFiltroEstadoSenador(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (!string.IsNullOrEmpty(request.Filters["Estado"].ToString()))
            {
                sqlSelect.AppendLine("	AND l.id_sf_senador IN (SELECT id FROM sf_senador where id_partido IN(" + request.Filters["Estado"].ToString() + ")) ");
            }
        }

        private static void AdicionaFiltroPartidoSenador(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (!string.IsNullOrEmpty(request.Filters["Partido"].ToString()))
            {
                sqlSelect.AppendLine("	AND l.id_sf_senador IN (SELECT id FROM sf_senador where id_estado IN(" + request.Filters["Partido"].ToString() + ")) ");
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
                        using (Banco banco = new Banco())
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
            if (!string.IsNullOrEmpty(request.Filters["Despesa"].ToString()))
            {
                sqlSelect.AppendLine("	AND l.id_sf_despesa_tipo IN (" + Utils.MySqlEscapeNumberToIn(request.Filters["Despesa"].ToString()) + ") ");
            }
        }

        private static void AdicionaFiltroSenador(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (!string.IsNullOrEmpty(request.Filters["IdParlamentar"].ToString()))
            {
                sqlSelect.AppendLine("	AND l.id_sf_senador IN (" + Utils.MySqlEscapeNumberToIn(request.Filters["IdParlamentar"].ToString()) + ") ");
            }
        }

        private static void AdicionaResultadoComum(DataTablesRequest request, StringBuilder sqlSelect, Dictionary<int, string> dcFielsSort, string defaultSort = "valor_total desc")
        {
            //sqlSelect.AppendLine("select * from table_in_memory ");
            sqlSelect.AppendFormat(" ORDER BY {0} ", request.GetSorting(dcFielsSort, defaultSort));
            sqlSelect.AppendFormat(" LIMIT {0},{1}; ", request.Start, request.Length);

            sqlSelect.AppendLine(
                @"SELECT count(1), sum(valor_total) as valor_total
				FROM table_in_memory; ");
        }

        public async Task<dynamic> TipoDespesa()
        {
            using (Banco banco = new Banco())
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
            using (Banco banco = new Banco())
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
            using (Banco banco = new Banco())
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
    }
}