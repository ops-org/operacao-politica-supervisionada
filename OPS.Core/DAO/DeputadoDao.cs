using MySqlConnector;
using OPS.Core.DTO;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace OPS.Core.DAO
{
    public class DeputadoDao
    {
        public async Task<dynamic> Consultar(int id)
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT 
						d.id as id_cf_deputado
						, d.id_partido
						, p.sigla as sigla_partido
						, p.nome as nome_partido
						, d.id_estado
						, e.sigla as sigla_estado
						, e.nome as nome_estado
						, d.nome_parlamentar
						, d.nome_civil
						, d.condicao
						, d.situacao
						, d.sexo
                        , g.id as id_cf_gabinete
						, g.predio
						, g.sala
						, g.andar
						, g.telefone
						, d.email
						, d.profissao
						, d.nascimento
						, d.falecimento
                        , en.sigla as sigla_estado_nascimento
                        , d.municipio as nome_municipio_nascimento
						, d.valor_total_ceap
						, d.quantidade_secretarios
						, d.custo_secretarios
						, d.custo_total_secretarios
					FROM cf_deputado d
					LEFT JOIN partido p on p.id = d.id_partido
					LEFT JOIN estado e on e.id = d.id_estado -- eleito
					LEFT JOIN estado en on en.id = d.id_estado_nascimento -- naturalidade
                    LEFT JOIN cf_gabinete g on g.id = d.id_cf_gabinete
					WHERE d.id = @id
				");
                banco.AddParameter("@id", id);

                using (DbDataReader reader = await banco.ExecuteReaderAsync(strSql.ToString()))
                {
                    if (await reader.ReadAsync())
                    {
                        return new
                        {
                            id_cf_deputado = reader["id_cf_deputado"],
                            id_partido = reader["id_partido"],
                            sigla_estado = reader["sigla_estado"].ToString(),
                            nome_partido = reader["nome_partido"].ToString(),
                            id_estado = reader["id_estado"],
                            sigla_partido = reader["sigla_partido"].ToString(),
                            nome_estado = reader["nome_estado"].ToString(),
                            nome_parlamentar = reader["nome_parlamentar"].ToString(),
                            nome_civil = reader["nome_civil"].ToString(),
                            condicao = reader["condicao"].ToString(),
                            situacao = reader["situacao"].ToString(),
                            sexo = reader["sexo"].ToString(),
                            id_cf_gabinete = reader["id_cf_gabinete"],
                            predio = reader["predio"].ToString(),
                            sala = reader["sala"].ToString(),
                            andar = reader["andar"].ToString(),
                            telefone = reader["telefone"].ToString(),
                            email = reader["email"].ToString(),
                            profissao = reader["profissao"].ToString(),
                            nascimento = Utils.FormataData(reader["nascimento"]),
                            falecimento = Utils.FormataData(reader["falecimento"]),
                            sigla_estado_nascimento = reader["sigla_estado_nascimento"].ToString(),
                            nome_municipio_nascimento = reader["nome_municipio_nascimento"].ToString(),
                            valor_total_ceap = Utils.FormataValor(reader["valor_total_ceap"]),
                            quantidade_secretarios = reader["quantidade_secretarios"].ToString(),
                            custo_secretarios = Utils.FormataValor(reader["custo_secretarios"]),
                            custo_total_secretarios = Utils.FormataValor(reader["custo_total_secretarios"])
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
						FROM cf_despesa l
						WHERE l.id_cf_deputado = @id
						GROUP BY l.id_fornecedor
						order by 1 desc
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
						 l1.id as id_cf_despesa
						, l1.id_fornecedor
						, pj.cnpj_cpf
						, pj.nome AS nome_fornecedor
						, l1.valor_liquido
					from (
						SELECT
						l.id
						, l.valor_liquido
						, l.id_fornecedor
						FROM cf_despesa l
						WHERE l.id_cf_deputado = @id
						order by l.valor_liquido desc
						LIMIT 10
					) l1
					LEFT JOIN fornecedor pj on pj.id = l1.id_fornecedor
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
                            id_cf_despesa = reader["id_cf_despesa"].ToString(),
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
						l.id as id_cf_despesa
						, l.id_documento
						, l.numero_documento
						, l.tipo_documento
						, l.data_emissao
						, l.valor_documento
						, l.valor_glosa
						, l.valor_liquido
						, l.valor_restituicao
						, l.parcela
						, l.nome_passageiro
						, l.trecho_viagem
						, l.ano
						, l.mes
						, l.ano_mes
						, td.id as id_cf_despesa_tipo
						, td.descricao as descricao_despesa
						, d.id as id_cf_deputado
						, d.id_deputado
						, d.nome_parlamentar
						, e.sigla as sigla_estado
						, p.sigla as sigla_partido
						, pj.id AS id_fornecedor
						, pj.cnpj_cpf
						, pj.nome AS nome_fornecedor
                        , l.link
                        , l.url_documento
					FROM cf_despesa l
					LEFT JOIN fornecedor pj ON pj.id = l.id_fornecedor
					LEFT JOIN cf_deputado d ON d.id = l.id_cf_deputado
					LEFT JOIN cf_despesa_tipo td ON td.id = l.id_cf_despesa_tipo
					LEFT JOIN partido p on p.id = d.id_partido
					LEFT JOIN estado e on e.id = d.id_estado
					WHERE l.id = @id
				 ");
                banco.AddParameter("@id", id);

                using (DbDataReader reader = await banco.ExecuteReaderAsync(strSql.ToString()))
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

                        var result = new
                        {
                            id_cf_despesa = await reader.GetValueOrDefaultAsync<dynamic>(0),
                            id_documento = await reader.GetValueOrDefaultAsync<dynamic>(1),
                            numero_documento = await reader.GetValueOrDefaultAsync<string>(2),
                            tipo_documento = sTipoDocumento,
                            data_emissao = Utils.FormataData(await reader.GetValueOrDefaultAsync<MySqlDateTime?>(4)),
                            valor_documento = Utils.FormataValor(await reader.GetValueOrDefaultAsync<decimal?>(5)),
                            valor_glosa = Utils.FormataValor(await reader.GetValueOrDefaultAsync<decimal?>(6)),
                            valor_liquido = Utils.FormataValor(await reader.GetValueOrDefaultAsync<decimal?>(7)),
                            valor_restituicao = Utils.FormataValor(await reader.GetValueOrDefaultAsync<decimal?>(8)),
                            parcela = await reader.GetValueOrDefaultAsync<dynamic>(9),
                            nome_passageiro = await reader.GetValueOrDefaultAsync<string>(10),
                            trecho_viagem = await reader.GetValueOrDefaultAsync<string>(11),
                            ano = await reader.GetValueOrDefaultAsync<dynamic>(12),
                            mes = await reader.GetValueOrDefaultAsync<dynamic>(13),
                            ano_mes = await reader.GetValueOrDefaultAsync<dynamic>(14),
                            id_cf_despesa_tipo = await reader.GetValueOrDefaultAsync<dynamic>(15),
                            descricao_despesa = await reader.GetValueOrDefaultAsync<string>(16),
                            id_cf_deputado = await reader.GetValueOrDefaultAsync<dynamic>(17),
                            id_deputado = await reader.GetValueOrDefaultAsync<dynamic>(18),
                            nome_parlamentar = await reader.GetValueOrDefaultAsync<string>(19),
                            sigla_estado = await reader.GetValueOrDefaultAsync<string>(20),
                            sigla_partido = await reader.GetValueOrDefaultAsync<string>(21),
                            id_fornecedor = await reader.GetValueOrDefaultAsync<dynamic>(22),
                            cnpj_cpf = await reader.GetValueOrDefaultAsync<string>(23),
                            nome_fornecedor = await reader.GetValueOrDefaultAsync<string>(24),
                            competencia = string.Format("{0:00}/{1:0000}", await reader.GetValueOrDefaultAsync<ushort>(13), await reader.GetValueOrDefaultAsync<ushort>(12)),
                            link = await reader.GetValueOrDefaultAsync<dynamic>(25),
                            url_documento = await reader.GetValueOrDefaultAsync<string>(26)
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
						l.id as id_cf_despesa
						, pj.id as id_fornecedor
						, pj.nome as nome_fornecedor
						, pji.estado as sigla_estado_fornecedor
						, l.valor_liquido
					FROM (
						select id, id_cf_deputado, id_fornecedor, data_emissao from cf_despesa
						where id = @id
					) l1 
					INNER JOIN cf_despesa l on
						l1.id_cf_deputado = l.id_cf_deputado and
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
                        id_cf_despesa = await reader.GetValueOrDefaultAsync<ulong>(0),
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
						l.id as id_cf_despesa
						, pj.id as id_fornecedor
						, pj.nome as nome_fornecedor
						, pji.estado as sigla_estado_fornecedor
						, l.valor_liquido
					FROM (
						select id, id_cf_deputado, id_fornecedor, id_cf_despesa_tipo, ano_mes from cf_despesa
						where id = @id
					) l1 
					INNER JOIN cf_despesa l on
						l1.id_cf_deputado = l.id_cf_deputado and
						l1.ano_mes = l.ano_mes and
						l1.id_cf_despesa_tipo = l.id_cf_despesa_tipo and
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
                        id_cf_despesa = await reader.GetValueOrDefaultAsync<ulong>(0),
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

        public async Task<dynamic> GastosMensaisPorAno(int id)
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT d.ano, d.mes, SUM(d.valor_liquido) AS valor_total
					FROM cf_despesa d
					WHERE d.id_cf_deputado = @id
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

        public async Task<dynamic> Lista(FiltroParlamentarDTO request)
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
                    SELECT 
						d.id as id_cf_deputado
						, d.nome_parlamentar 
						, d.nome_civil
						, d.valor_total_ceap
						, d.id_partido
						, p.sigla as sigla_partido
						, p.nome as nome_partido
						, d.id_estado
						, e.sigla as sigla_estado
						, e.nome as nome_estado
                        -- , s.total_sessoes
                        -- , s.total_presencas
					FROM cf_deputado d
                    INNER JOIN cf_mandato m on m.id_cf_deputado = d.id
					LEFT JOIN partido p on p.id = d.id_partido
					LEFT JOIN estado e on e.id = d.id_estado
                    -- LEFT JOIN (
                    --     SELECT 
                    --         sp.id_cf_deputado
                    --         , s.id_legislatura
                    --         , sum(1) as total_sessoes
					-- 	    , sum(IF(sp.presente = 1, 1, 0)) as total_presencas
					--     FROM cf_sessao_presenca sp
					--     inner join cf_sessao s on s.id = sp.id_cf_sessao
					--     group by sp.id_cf_deputado, s.id_legislatura
                    -- ) s on s.id_cf_deputado = d.id and s.id_legislatura = m.id_legislatura
                    WHERE 1=1");

                switch (request.Periodo)
                {
                    case 9: //PERIODO_MANDATO_56
                        strSql.AppendLine(" AND m.id_legislatura = 56");
                        break;

                    case 8: //PERIODO_MANDATO_55
                        strSql.AppendLine(" AND m.id_legislatura = 55");
                        break;

                    case 7: //PERIODO_MANDATO_54
                        strSql.AppendLine(" AND m.id_legislatura = 54");
                        break;

                    case 6: //PERIODO_MANDATO_53
                        strSql.AppendLine(" AND m.id_legislatura = 53");
                        break;
                }

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
                            id_cf_deputado = reader["id_cf_deputado"],
                            nome_parlamentar = reader["nome_parlamentar"].ToString(),
                            nome_civil = reader["nome_civil"].ToString(),
                            valor_total_ceap = Utils.FormataValor(reader["valor_total_ceap"]),
                            sigla_partido = !string.IsNullOrEmpty(reader["sigla_partido"].ToString()) ? reader["sigla_partido"].ToString() : "S.PART.",
                            nome_partido = !string.IsNullOrEmpty(reader["nome_partido"].ToString()) ? reader["nome_partido"].ToString() : "SEM PARTIDO",
                            sigla_estado = reader["sigla_estado"].ToString(),
                            nome_estado = reader["nome_estado"].ToString(),
                            //total_sessoes = Utils.FormataValor(reader["total_sessoes"],  0),
                            //total_presencas = Utils.FormataValor(reader["total_presencas"], 0),
                            //frequencia = Utils.FormataValor(reader.GetValueOrDefault<decimal?>("total_presencas") * 100 / reader.GetValueOrDefault<decimal?>("total_sessoes"))
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
						d.id as id_cf_deputado
						, d.nome_parlamentar 
						, d.nome_civil
						, d.valor_total_ceap
						, d.id_partido
						, p.sigla as sigla_partido
						, p.nome as nome_partido
						, d.id_estado
						, e.sigla as sigla_estado
						, e.nome as nome_estado
					FROM cf_deputado d
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
                            id_cf_deputado = reader["id_cf_deputado"],
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

        public async Task<dynamic> Pesquisa()
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT 
						id, nome_civil, nome_parlamentar 
					FROM cf_deputado 
					ORDER BY nome_parlamentar
				");

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
            Dictionary<int, string> dcFielsSort;

            EnumAgrupamentoAuditoria eAgrupamento = (EnumAgrupamentoAuditoria)Convert.ToInt32(request.Filters["Agrupamento"].ToString());
            switch (eAgrupamento)
            {
                case EnumAgrupamentoAuditoria.Parlamentar:
                    dcFielsSort = new Dictionary<int, string>(){
                        {1, "d.nome_parlamentar" },
                        {2, "e.sigla" },
                        {3, "p.sigla" },
                        {4, "total_notas" },
                        {5, "valor_total" },
                    };

                    return await LancamentosParlamentar(request, dcFielsSort);
                case EnumAgrupamentoAuditoria.Despesa:
                    dcFielsSort = new Dictionary<int, string>(){
                        {1, "td.descricao" },
                        {2, "total_notas" },
                        {3, "valor_total" },
                    };

                    return await LancamentosDespesa(request, dcFielsSort);
                case EnumAgrupamentoAuditoria.Fornecedor:
                    dcFielsSort = new Dictionary<int, string>(){
                        {1, "pj.cnpj_cpf" },
                        {2, "pj.nome" },
                        {3, "total_notas" },
                        {4, "valor_total" },
                    };

                    return await LancamentosFornecedor(request, dcFielsSort);
                case EnumAgrupamentoAuditoria.Partido:
                    dcFielsSort = new Dictionary<int, string>(){
                        {1, "nome_partido" },
                        {2, "total_notas" },
                        {3, "total_deputados" },
                        {4, "valor_medio_por_deputado" },
                        {5, "valor_total" },
                    };

                    return await LancamentosPartido(request, dcFielsSort);
                case EnumAgrupamentoAuditoria.Estado:
                    dcFielsSort = new Dictionary<int, string>(){
                        {1, "nome_estado" },
                        {2, "total_notas" },
                        {3, "valor_total" },
                    };

                    return await LancamentosEstado(request, dcFielsSort);
                case EnumAgrupamentoAuditoria.Documento:
                    dcFielsSort = new Dictionary<int, string>(){
                        {0, "data_emissao" },
                        {1, "cnpj_cpf" },
                        {2, "nome_fornecedor" },
                        {3, "sigla_estado_fornecedor" },
                        {4, "nome_parlamentar" },
                        {5, "numero_documento" },
                        {6, "trecho_viagem" },
                        {7, "valor_liquido" },
                    };

                    return await LancamentosNotaFiscal(request, dcFielsSort);
                default:
                    break;
            }

            throw new BusinessException("Parâmetro request.Agrupamento não informado!");
        }

        private async Task<dynamic> LancamentosParlamentar(DataTablesRequest request, Dictionary<int, string> dcFielsSort)
        {
            using (AppDb banco = new AppDb())
            {
                var sqlSelect = new StringBuilder();

                sqlSelect.AppendLine(@"
					DROP TABLE IF EXISTS table_in_memory;
					CREATE TEMPORARY TABLE table_in_memory
					SELECT
						count(l.id) AS total_notas
					    , sum(l.valor_liquido) as valor_total
					    , l.id_cf_deputado
					FROM cf_despesa l
					WHERE (1=1)
				");

                AdicionaFiltroPeriodo(request, sqlSelect);

                AdicionaFiltroDeputado(request, sqlSelect);

                AdicionaFiltroDespesa(request, sqlSelect);

                AdicionaFiltroFornecedor(request, sqlSelect);

                AdicionaFiltroPartidoDeputado(request, sqlSelect);

                AdicionaFiltroEstadoDeputado(request, sqlSelect);

                sqlSelect.AppendLine(@"
					GROUP BY id_cf_deputado;
					
					SELECT
						 d.id as id_cf_deputado
						, d.nome_parlamentar
						, e.sigla as sigla_estado
						, p.sigla as sigla_partido
						, l1.total_notas
						, l1.valor_total
						from table_in_memory l1
					LEFT JOIN cf_deputado d on d.id = l1.id_cf_deputado
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
                            id_cf_deputado = reader["id_cf_deputado"],
                            nome_parlamentar = reader["nome_parlamentar"].ToString(),
                            sigla_estado = reader["sigla_estado"].ToString(),
                            sigla_partido = reader["sigla_partido"].ToString(),
                            total_notas = Utils.FormataValor(reader["total_notas"], 0),
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
                        valorTotal = ValorTotal,
                        recordsTotal = Convert.ToInt32(TotalCount),
                        recordsFiltered = Convert.ToInt32(TotalCount),
                        data = lstRetorno
                    };
                }
            }
        }

        private async Task<dynamic> LancamentosFornecedor(DataTablesRequest request, Dictionary<int, string> dcFielsSort)
        {
            using (AppDb banco = new AppDb())
            {
                var sqlSelect = new StringBuilder();

                sqlSelect.AppendLine(@"
					DROP TABLE IF EXISTS table_in_memory;
					CREATE TEMPORARY TABLE table_in_memory
					SELECT
						l.id_fornecedor
						, count(l.id) AS total_notas
						, sum(l.valor_liquido) as valor_total
					FROM cf_despesa l
					WHERE (1=1)
				");

                AdicionaFiltroPeriodo(request, sqlSelect);

                AdicionaFiltroDeputado(request, sqlSelect);

                AdicionaFiltroDespesa(request, sqlSelect);

                AdicionaFiltroFornecedor(request, sqlSelect);

                AdicionaFiltroPartidoDeputado(request, sqlSelect);

                AdicionaFiltroEstadoDeputado(request, sqlSelect);

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
                            cnpj_cpf = Utils.FormatCnpjCpf(reader["cnpj_cpf"].ToString()),
                            nome_fornecedor = reader["nome_fornecedor"],
                            total_notas = Utils.FormataValor(reader["total_notas"], 0),
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
                        valorTotal = ValorTotal,
                        recordsTotal = Convert.ToInt32(TotalCount),
                        recordsFiltered = Convert.ToInt32(TotalCount),
                        data = lstRetorno
                    };
                }
            }
        }

        private async Task<dynamic> LancamentosDespesa(DataTablesRequest request, Dictionary<int, string> dcFielsSort)
        {
            using (AppDb banco = new AppDb())
            {
                var sqlSelect = new StringBuilder();

                sqlSelect.AppendLine(@"
					DROP TABLE IF EXISTS table_in_memory;
					CREATE TEMPORARY TABLE table_in_memory
					SELECT
						count(l.id) AS total_notas
						, sum(l.valor_liquido) as valor_total
						, l.id_cf_despesa_tipo
					FROM cf_despesa l
					WHERE (1=1)
				");

                AdicionaFiltroPeriodo(request, sqlSelect);

                AdicionaFiltroDeputado(request, sqlSelect);

                AdicionaFiltroDespesa(request, sqlSelect);

                AdicionaFiltroFornecedor(request, sqlSelect);

                AdicionaFiltroPartidoDeputado(request, sqlSelect);

                AdicionaFiltroEstadoDeputado(request, sqlSelect);

                sqlSelect.AppendLine(@"
					GROUP BY id_cf_despesa_tipo;
					
					SELECT
						l1.id_cf_despesa_tipo
						, td.descricao
						, l1.total_notas
						, l1.valor_total
					from table_in_memory l1
					LEFT JOIN cf_despesa_tipo td on td.id = l1.id_cf_despesa_tipo
				");

                AdicionaResultadoComum(request, sqlSelect, dcFielsSort);

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(sqlSelect.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_cf_despesa_tipo = reader["id_cf_despesa_tipo"],
                            descricao = reader["descricao"].ToString(),
                            total_notas = Utils.FormataValor(reader["total_notas"], 0),
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
                        valorTotal = ValorTotal,
                        recordsTotal = Convert.ToInt32(TotalCount),
                        recordsFiltered = Convert.ToInt32(TotalCount),
                        data = lstRetorno
                    };
                }
            }
        }

        private async Task<dynamic> LancamentosPartido(DataTablesRequest request, Dictionary<int, string> dcFielsSort)
        {
            using (AppDb banco = new AppDb())
            {
                var sqlSelect = new StringBuilder();

                sqlSelect.AppendLine(@"
					DROP TABLE IF EXISTS table_in_memory;
					CREATE TEMPORARY TABLE table_in_memory
					AS (
						SELECT
						 p.id as id_partido
						, IFnull(p.nome, 'SEM PARTIDO') as nome_partido
						, sum(l1.total_notas) as total_notas
						, count(l1.id_cf_deputado) as total_deputados
                        , sum(l1.valor_total) / count(l1.id_cf_deputado) as valor_medio_por_deputado
                        , sum(l1.valor_total) as valor_total
						from (
							SELECT
							 count(l.id) AS total_notas
							, sum(l.valor_liquido) as valor_total
							, l.id_cf_deputado
							FROM cf_despesa l
							WHERE (1=1)
				");

                AdicionaFiltroPeriodo(request, sqlSelect);

                AdicionaFiltroDeputado(request, sqlSelect);

                AdicionaFiltroDespesa(request, sqlSelect);

                AdicionaFiltroFornecedor(request, sqlSelect);

                AdicionaFiltroPartidoDeputado(request, sqlSelect);

                AdicionaFiltroEstadoDeputado(request, sqlSelect);

                sqlSelect.AppendLine(@"
						GROUP BY id_cf_deputado
					) l1
					INNER JOIN cf_deputado d on d.id = l1.id_cf_deputado
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
                            nome_partido = reader["nome_partido"],
                            total_notas = Utils.FormataValor(reader["total_notas"], 0),
                            total_deputados = Utils.FormataValor(reader["total_deputados"], 0),
                            valor_medio_por_deputado = Utils.FormataValor(reader["valor_medio_por_deputado"]),
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
                        valorTotal = ValorTotal,
                        recordsTotal = Convert.ToInt32(TotalCount),
                        recordsFiltered = Convert.ToInt32(TotalCount),
                        data = lstRetorno
                    };
                }
            }
        }

        private async Task<dynamic> LancamentosEstado(DataTablesRequest request, Dictionary<int, string> dcFielsSort)
        {
            using (AppDb banco = new AppDb())
            {
                var sqlSelect = new StringBuilder();

                sqlSelect.AppendLine(@"
					DROP TABLE IF EXISTS table_in_memory;
					CREATE TEMPORARY TABLE table_in_memory
					AS (
						SELECT
						 e.id AS id_estado
						, IFNULL(e.nome, 'Sem Estado / Lideranças de Partido') as nome_estado
						, sum(l1.total_notas) as total_notas
						, sum(l1.valor_total) as valor_total
						from (

							SELECT
							 count(l.id) AS total_notas
							, sum(l.valor_liquido) as valor_total
							, l.id_cf_deputado
							FROM cf_despesa l
							WHERE (1=1)
				");

                AdicionaFiltroPeriodo(request, sqlSelect);

                AdicionaFiltroDeputado(request, sqlSelect);

                AdicionaFiltroDespesa(request, sqlSelect);

                AdicionaFiltroFornecedor(request, sqlSelect);

                AdicionaFiltroPartidoDeputado(request, sqlSelect);

                AdicionaFiltroEstadoDeputado(request, sqlSelect);

                sqlSelect.AppendLine(@"
						GROUP BY id_cf_deputado
					) l1
					INNER JOIN cf_deputado d on d.id = l1.id_cf_deputado
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
                            total_notas = Utils.FormataValor(reader["total_notas"], 0),
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
                        valorTotal = ValorTotal,
                        recordsTotal = Convert.ToInt32(TotalCount),
                        recordsFiltered = Convert.ToInt32(TotalCount),
                        data = lstRetorno
                    };
                }
            }
        }

        private async Task<dynamic> LancamentosNotaFiscal(DataTablesRequest request, Dictionary<int, string> dcFielsSort)
        {
            var sqlWhere = new StringBuilder();
            AdicionaFiltroPeriodo(request, sqlWhere);
            AdicionaFiltroDeputado(request, sqlWhere);
            AdicionaFiltroDespesa(request, sqlWhere);
            AdicionaFiltroFornecedor(request, sqlWhere);
            AdicionaFiltroPartidoDeputado(request, sqlWhere);
            AdicionaFiltroEstadoDeputado(request, sqlWhere);
            AdicionaFiltroDocumento(request, sqlWhere);

            using (AppDb banco = new AppDb())
            {
                var sqlSelect = new StringBuilder();

                sqlSelect.AppendLine(@"
					SELECT
						l.id as id_cf_despesa
						, l.data_emissao
						, l.id_fornecedor			
						, pj.cnpj_cpf
						, pj.nome AS nome_fornecedor
						, d.id as id_cf_deputado
						, d.nome_parlamentar
						, l.numero_documento
						, l.trecho_viagem
						, l.valor_liquido
						, pji.estado as sigla_estado_fornecedor
					FROM (
						SELECT *
						FROM cf_despesa l
						WHERE (1=1)
				");

                sqlSelect.Append(sqlWhere);

                sqlSelect.AppendFormat(" ORDER BY {0} ", request.GetSorting(dcFielsSort, "l.data_emissao desc, l.valor_liquido desc"));
                sqlSelect.AppendFormat(" LIMIT {0},{1} ", request.Start, request.Length);

                sqlSelect.AppendLine(@" ) l
					INNER JOIN cf_deputado d on d.id = l.id_cf_deputado
					LEFT JOIN fornecedor pj on pj.id = l.id_fornecedor
					LEFT JOIN fornecedor_info pji on pji.id_fornecedor = pj.id;");

                sqlSelect.AppendLine(
                    @"SELECT COUNT(1) AS row_count, SUM(l.valor_liquido) as valor_total
					FROM cf_despesa l
					WHERE (1=1)");

                sqlSelect.Append(sqlWhere);

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(sqlSelect.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_cf_despesa = reader["id_cf_despesa"],
                            data_emissao = Utils.FormataData(reader["data_emissao"]),
                            id_fornecedor = reader["id_fornecedor"],
                            cnpj_cpf = Utils.FormatCnpjCpf(reader["cnpj_cpf"].ToString()),
                            nome_fornecedor = reader["nome_fornecedor"].ToString(),
                            sigla_estado_fornecedor = reader["sigla_estado_fornecedor"].ToString(),
                            id_cf_deputado = reader["id_cf_deputado"],
                            nome_parlamentar = reader["nome_parlamentar"].ToString(),
                            numero_documento = reader["numero_documento"].ToString(),
                            trecho_viagem = reader["trecho_viagem"].ToString(),
                            valor_liquido = Utils.FormataValor(reader["valor_liquido"])
                        });
                    }

                    reader.NextResult();
                    await reader.ReadAsync();
                    string TotalCount = reader["row_count"].ToString();
                    string ValorTotal = Utils.FormataValor(reader["valor_total"]);

                    return new
                    {
                        draw = request.Draw,
                        valorTotal = ValorTotal,
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
                    sqlSelect.AppendLine(" AND l.ano_mes >= " + dataIni.ToString("yyyyMM"));
                    break;

                case "5": //PERIODO_ANO_ANTERIOR
                    dataIni = new DateTime(dataIni.Year, 1, 1).AddYears(-1);
                    dataFim = new DateTime(dataIni.Year, 12, 31);
                    sqlSelect.AppendFormat(" AND l.ano_mes BETWEEN {0} AND {1}", dataIni.ToString("yyyyMM"), dataFim.ToString("yyyyMM"));
                    break;

                case "9": //PERIODO_MANDATO_55
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

                case "0": //Customizado
                    if (request.Filters.ContainsKey("PeriodoCustom") && !string.IsNullOrEmpty(request.Filters["PeriodoCustom"].ToString()))
                    {
                        var periodo = request.Filters["PeriodoCustom"].ToString().Split('-');

                        if (periodo[0].Length == 6 && periodo[1].Length == 6)
                        {
                            sqlSelect.AppendLine(string.Format(" AND l.ano_mes BETWEEN {0} AND {1}", periodo[0], periodo[1]));
                        }
                        else if (periodo[0].Length == 6)
                        {
                            sqlSelect.AppendLine(string.Format(" AND l.ano_mes >= {0}", periodo[0]));
                        }
                        else if (periodo[1].Length == 6)
                        {
                            sqlSelect.AppendLine(string.Format(" AND l.ano_mes <= {0}", periodo[1]));
                        }
                    }
                    break;
            }
        }

        private static void AdicionaFiltroPartidoDeputado(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.ContainsKey("Partido") && !string.IsNullOrEmpty(request.Filters["Partido"].ToString()))
            {
                sqlSelect.AppendLine("	AND l.id_cf_deputado IN (SELECT id FROM cf_deputado where id_partido IN(" + request.Filters["Partido"].ToString() + ")) ");
            }
        }

        private static void AdicionaFiltroEstadoDeputado(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.ContainsKey("Estado") && !string.IsNullOrEmpty(request.Filters["Estado"].ToString()))
            {
                sqlSelect.AppendLine("	AND l.id_cf_deputado IN (SELECT id FROM cf_deputado where id_estado IN(" + Utils.MySqlEscapeNumberToIn(request.Filters["Estado"].ToString()) + ")) ");
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
                sqlSelect.AppendLine("	AND l.id_cf_despesa_tipo IN (" + Utils.MySqlEscapeNumberToIn(request.Filters["Despesa"].ToString()) + ") ");
            }
        }

        private static void AdicionaFiltroDeputado(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.ContainsKey("IdParlamentar") && !string.IsNullOrEmpty(request.Filters["IdParlamentar"].ToString()))
            {
                sqlSelect.AppendLine("	AND l.id_cf_deputado IN (" + Utils.MySqlEscapeNumberToIn(request.Filters["IdParlamentar"].ToString()) + ") ");
            }
        }

        private static void AdicionaFiltroDocumento(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.ContainsKey("Documento") && !string.IsNullOrEmpty(request.Filters["Documento"].ToString()))
            {
                sqlSelect.AppendLine("	AND l.numero_documento like '%" + Utils.MySqlEscape(request.Filters["Documento"].ToString()) + "' ");
            }
        }

        private static void AdicionaResultadoComum(DataTablesRequest request, StringBuilder sqlSelect, Dictionary<int, string> dcFielsSort)
        {
            //sqlSelect.AppendLine("select * from table_in_memory ");
            sqlSelect.AppendFormat(" ORDER BY {0} ", request.GetSorting(dcFielsSort, "valor_total desc"));
            sqlSelect.AppendFormat(" LIMIT {0},{1}; ", request.Start, request.Length);

            sqlSelect.AppendLine(
                @"SELECT count(1), sum(valor_total) as valor_total
				FROM table_in_memory; ");
        }

        public async Task<dynamic> TipoDespesa()
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine("SELECT id, descricao FROM cf_despesa_tipo ");
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

        public async Task<dynamic> Secretarios(DataTablesRequest request)
        {
            var dcFielsSort = new Dictionary<int, string>(){
                {1, "p.nome_parlamentar" },
                {2, "p.quantidade_secretarios" },
                {3, "p.custo_mensal_secretarios" },
                {4, "p.custo_total_secretarios" },
            };

            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT SQL_CALC_FOUND_ROWS
						p.id as id_cf_deputado
						, p.nome_parlamentar
						, p.quantidade_secretarios
						, p.custo_secretarios
						, p.custo_total_secretarios
					from cf_deputado p
					where p.quantidade_secretarios > 0
				");

                //if (!string.IsNullOrEmpty(request.NomeParlamentar))
                //{
                //    strSql.AppendFormat("and p.nome_parlamentar LIKE '%{0}%' ", Utils.MySqlEscape(request.NomeParlamentar));
                //}

                strSql.AppendFormat(" ORDER BY {0} ", request.GetSorting(dcFielsSort, "p.nome_parlamentar"));
                strSql.AppendFormat(" LIMIT {0},{1}; ", request.Start, request.Length);

                strSql.AppendLine("SELECT FOUND_ROWS() as row_count; ");

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(strSql.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_cf_deputado = reader["id_cf_deputado"],
                            nome_parlamentar = reader["nome_parlamentar"].ToString(),
                            quantidade_secretarios = reader["quantidade_secretarios"].ToString(),
                            custo_secretarios = Utils.FormataValor(reader["custo_secretarios"]),
                            custo_total_secretarios = Utils.FormataValor(reader["custo_total_secretarios"])
                        });
                    }

                    reader.NextResult();
                    await reader.ReadAsync();
                    string TotalCount = reader["row_count"].ToString();

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

        public async Task<dynamic> SecretariosAtivosPorDeputado(int id, DataTablesRequest request)
        {
            var dcFielsSort = new Dictionary<int, string>(){
                {1, "s.nome" },
                {2, "s.cargo" },
                {3, "s.periodo" },
                {4, "s.valor_bruto" },
                {5, "s.valor_liquido" },
                {6, "s.valor_outros+s.valor_bruto" },
                {7, "s.referencia" },
            };

            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT SQL_CALC_FOUND_ROWS
						s.nome
						, s.cargo
						, s.periodo
						, s.valor_bruto
						, s.valor_liquido
						, s.valor_outros
						, s.valor_outros + s.valor_bruto as custo_total
                        , s.referencia
						, s.link
					FROM cf_secretario s
					WHERE s.id_cf_deputado = @id
				");
                banco.AddParameter("@id", id);

                if (request.Filters.ContainsKey("ativo"))
                {
                    banco.AddParameter("@ativo", Convert.ToInt32(request.Filters["ativo"].ToString()));
                    strSql.Append("and s.em_exercicio = @ativo");
                }

                strSql.AppendFormat(" ORDER BY {0} ", Utils.MySqlEscape(request.GetSorting(dcFielsSort, "s.nome")));
                strSql.AppendFormat(" LIMIT {0},{1}; ", request.Start, request.Length);

                strSql.AppendLine("SELECT FOUND_ROWS() as row_count; ");

                using (DbDataReader reader = await banco.ExecuteReaderAsync(strSql.ToString()))
                {
                    var lstRetorno = new List<dynamic>();
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            nome = reader["nome"].ToString(),
                            cargo = reader["cargo"].ToString(),
                            periodo = reader["periodo"].ToString(),
                            valor_bruto = Utils.FormataValor(reader["valor_bruto"]),
                            valor_liquido = Utils.FormataValor(reader["valor_liquido"]),
                            valor_outros = Utils.FormataValor(reader["valor_outros"]),
                            custo_total = Utils.FormataValor(reader["custo_total"]),
                            referencia = reader["referencia"].ToString(),
                            link = reader["link"].ToString()
                        });


                    }

                    reader.NextResult();
                    await reader.ReadAsync();
                    string TotalCount = reader["row_count"].ToString();

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


        public async Task<dynamic> SecretariosPorDeputado(int id, DataTablesRequest request)
        {
            var dcFielsSort = new Dictionary<int, string>(){
                {1, "s.nome" },
                {2, "custo_total" }
            };

            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT SQL_CALC_FOUND_ROWS
	                    s.nome
	                    , SUM(r.valor_outros + r.valor_bruto) as custo_total
	                    , s.link
                    from cf_secretario_remuneracao r
                    left join (
	                    select distinct id_cf_deputado, nome, link
	                    from cf_secretario s
                    ) s on s.link = r.id_cf_secretario
                    WHERE s.id_cf_deputado = @id
                    group by s.link, s.nome
				");
                banco.AddParameter("@id", id);

                strSql.AppendFormat(" ORDER BY {0} ", Utils.MySqlEscape(request.GetSorting(dcFielsSort, "s.nome")));
                strSql.AppendFormat(" LIMIT {0},{1}; ", request.Start, request.Length);

                strSql.AppendLine("SELECT FOUND_ROWS() as row_count; ");

                using (DbDataReader reader = await banco.ExecuteReaderAsync(strSql.ToString()))
                {
                    var lstRetorno = new List<dynamic>();
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            nome = reader["nome"].ToString(),
                            custo_total = Utils.FormataValor(reader["custo_total"]),
                            link = reader["link"].ToString()
                        });


                    }

                    reader.NextResult();
                    await reader.ReadAsync();
                    string TotalCount = reader["row_count"].ToString();

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

        public async Task<dynamic> ResumoPresenca(int id)
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT 
						year(s.data) as ano
						, sum(IF(sp.presente = 1, 1, 0)) as presenca
						, sum(IF(sp.presente = 0 and sp.justificativa = '', 1, 0)) as ausencia
						, sum(IF(sp.presente = 0 and sp.justificativa <> '', 1, 0)) as ausencia_justificada
					FROM cf_sessao_presenca sp
					inner join cf_sessao s on s.id = sp.id_cf_sessao
					where sp.id_cf_deputado = @id
					group by sp.id_cf_deputado, year(s.data)
					order by year(s.data)
				");
                banco.AddParameter("@id", id);

                var categories = new List<dynamic>();
                var series = new List<dynamic>();

                var presencas = new List<dynamic>();
                var ausencias = new List<dynamic>();
                var ausencias_justificadas = new List<dynamic>();

                int presenca_total = 0;
                int ausencia_total = 0;
                int ausencia_justificada_total = 0;

                using (DbDataReader reader = await banco.ExecuteReaderAsync(strSql.ToString()))
                {
                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            int presenca = Convert.ToInt32(reader["presenca"]);
                            int ausencia = Convert.ToInt32(reader["ausencia"]);
                            int ausencia_justificada = Convert.ToInt32(reader["ausencia_justificada"]);

                            categories.Add(Convert.ToInt32(reader["ano"]));

                            presencas.Add(presenca);
                            ausencias.Add(ausencia);
                            ausencias_justificadas.Add(ausencia_justificada);

                            presenca_total += presenca;
                            ausencia_total += ausencia;
                            ausencia_justificada_total += ausencia_justificada;
                        }
                    }
                    else
                    {
                        return new
                        {
                            frequencia_anual = new
                            {
                                categories = new List<dynamic>(),
                                series = new List<dynamic>()
                            },
                            frequencia_total_percentual = new List<dynamic>()
                        };
                    }
                }

                series.Add(new
                {
                    name = "Presença",
                    stack = "presenca",
                    data = presencas
                });

                series.Add(new
                {
                    name = "Ausência justificada",
                    stack = "ausencia",
                    data = ausencias_justificadas
                });

                series.Add(new
                {
                    name = "Ausência",
                    stack = "ausencia",
                    data = ausencias
                });

                int total_sessoes = presenca_total + ausencia_total + ausencia_justificada_total;

                var frequencia_total_percentual = new List<dynamic>
                {
                    new {name = "Presença", y = presenca_total * 100 / total_sessoes},
                    new {name = "Ausência Justificada", y = ausencia_justificada_total * 100 / total_sessoes},
                    new {name = "Ausência", y = ausencia_total * 100 / total_sessoes}
                };

                return new
                {
                    frequencia_anual = new
                    {
                        categories,
                        series
                    },
                    frequencia_total_percentual
                };
            }

        }

        public async Task<dynamic> CamaraResumoMensal()
        {
            using (AppDb banco = new AppDb())
            {
                using (DbDataReader reader = await banco.ExecuteReaderAsync(@"select ano, mes, valor from cf_despesa_resumo_mensal"))
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

        public async Task<dynamic> CamaraResumoAnual()
        {
            using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					select ano, sum(valor) as valor
					from cf_despesa_resumo_mensal sf
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

        public async Task<dynamic> Frequencia(DataTablesRequest request)
        {
            var sqlWhere = new StringBuilder();
            var dcFielsSort = new Dictionary<int, string>(){
                {1, "s.numero" },
                {2, "s.inicio" },
                {3, "s.tipo" },
                {4, "sp.presenca" },
                {5, "sp.ausencia" },
                {6, "sp.ausencia_justificada" },
                {7, "sp.presenca+sp.ausencia+sp.ausencia_justificada" },
            };

            using (AppDb banco = new AppDb())
            {
                var sqlSelect = new StringBuilder();

                sqlSelect.AppendLine(@"
					SELECT SQL_CALC_FOUND_ROWS 
						s.id as id_cf_sessao
						, s.inicio
						, s.tipo
						, s.numero
						, IFNULL(sp.presenca, 0) AS presenca
						, IFNULL(sp.ausencia, 0) AS ausencia
						, IFNULL(sp.ausencia_justificada, 0) AS ausencia_justificada
					FROM cf_sessao s
					LEFT JOIN (
						SELECT
							id_cf_sessao
							, sum(IF(sp.presente = 1, 1, 0)) as presenca
							, sum(IF(sp.presente = 0 and sp.justificativa = '', 1, 0)) as ausencia
							, sum(IF(sp.presente = 0 and sp.justificativa <> '', 1, 0)) as ausencia_justificada
						FROM cf_sessao_presenca sp 
						GROUP BY id_cf_sessao
					) sp on sp.id_cf_sessao = s.id
					WHERE (1=1)
				");

                sqlSelect.Append(sqlWhere);

                sqlSelect.AppendFormat(" ORDER BY {0} ", Utils.MySqlEscape(request.GetSorting(dcFielsSort, "s.inicio desc")));
                sqlSelect.AppendFormat(" LIMIT {0},{1}; ", request.Start, request.Length);

                sqlSelect.AppendLine(
                    @"SELECT FOUND_ROWS() as row_count;");

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(sqlSelect.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        string sTipo = "";
                        switch (reader["tipo"].ToString())
                        {
                            case "1": sTipo = "Ordinária"; break;
                            case "2": sTipo = "Extraordinária"; break;
                            case "3": sTipo = "Preparatória"; break;
                        }

                        var presenca = Convert.ToInt32(reader["presenca"]);
                        var ausencia = Convert.ToInt32(reader["ausencia"]);
                        var ausencia_justificada = Convert.ToInt32(reader["ausencia_justificada"]);
                        var total = presenca + ausencia + ausencia_justificada;

                        lstRetorno.Add(new
                        {
                            id_cf_sessao = reader["id_cf_sessao"],
                            inicio = Utils.FormataDataHora(reader["inicio"]),
                            tipo = sTipo,
                            numero = reader["numero"].ToString(),
                            presenca = presenca,
                            presenca_percentual = presenca > 0 ? Utils.FormataValor((decimal)presenca * 100 / total) : "",
                            ausencia = ausencia,
                            ausencia_percentual = ausencia > 0 ? Utils.FormataValor((decimal)ausencia * 100 / total) : "",
                            ausencia_justificada = ausencia_justificada,
                            ausencia_justificada_percentual = ausencia_justificada > 0 ? Utils.FormataValor((decimal)ausencia_justificada * 100 / total) : "",
                            total = total
                        });
                    }

                    reader.NextResult();
                    await reader.ReadAsync();
                    string TotalCount = reader["row_count"].ToString();

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

        public async Task<dynamic> Frequencia(int id, DataTablesRequest request)
        {
            var dcFielsSort = new Dictionary<int, string>(){
                {0, "d.nome_parlamentar" },
                {1, "sp.presente" },
                {2, "sp.justificativa" },
                {3, "sp.presenca_externa" },
            };

            using (AppDb banco = new AppDb())
            {
                var sqlSelect = new StringBuilder();

                sqlSelect.AppendFormat(@"
					SELECT SQL_CALC_FOUND_ROWS
						sp.id_cf_deputado
						, d.nome_parlamentar
						, sp.presente
						, sp.justificativa
						, sp.presenca_externa
					FROM cf_sessao_presenca sp
					INNER JOIN cf_deputado d on d.id = sp.id_cf_deputado
					WHERE sp.id_cf_sessao = {0}
				", id);

                sqlSelect.AppendFormat(" ORDER BY {0} ", Utils.MySqlEscape(request.GetSorting(dcFielsSort, "d.nome_parlamentar asc")));
                sqlSelect.AppendFormat(" LIMIT {0},{1}; ", request.Start, request.Length);

                sqlSelect.AppendLine(
                    @"SELECT FOUND_ROWS() as row_count;");

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await banco.ExecuteReaderAsync(sqlSelect.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        int nPresente = Convert.ToInt32(reader["presente"]);
                        int nPresensaExterna = Convert.ToInt32(reader["presenca_externa"]);
                        string sJustificativa = reader["justificativa"].ToString();

                        string sPresenca;
                        int nPresenca;
                        if (nPresensaExterna == 1)
                        {
                            nPresenca = 1;
                            sPresenca = "Presença externa";
                        }
                        else if (nPresente == 1)
                        {
                            nPresenca = 1;
                            sPresenca = "Presente";
                        }
                        else if (!string.IsNullOrEmpty(sJustificativa))
                        {
                            nPresenca = 2;
                            sPresenca = "Ausencia justificada";
                        }
                        else
                        {
                            nPresenca = 3;
                            sPresenca = "Ausente";
                        }

                        lstRetorno.Add(new
                        {
                            id_cf_deputado = reader["id_cf_deputado"],
                            nome_parlamentar = reader["nome_parlamentar"].ToString(),
                            id_presenca = nPresenca,
                            presenca = sPresenca,
                            justificativa = sJustificativa
                        });
                    }

                    reader.NextResult();
                    await reader.ReadAsync();
                    string TotalCount = reader["row_count"].ToString();

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
    }
}