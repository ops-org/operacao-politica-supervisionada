using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OPS.Core.DTOs;
using OPS.Core.Enumerators;
using OPS.Core.Exceptions;
using OPS.Core.Utilities;
using OPS.Infraestrutura;

namespace OPS.Core.Repositories
{
    public class DeputadoEstadualRepository : BaseRepository, IParlamentarRepository
    {
        public DeputadoEstadualRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<ParlamentarDetalheDTO> Consultar(int id)
        {
            var deputado = await _context.DeputadosEstaduais
                .Include(d => d.Partido)
                .Include(d => d.Estado)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deputado == null) return null;

            return new ParlamentarDetalheDTO
            {
                IdParlamentar = deputado.Id,
                IdPartido = deputado.IdPartido,
                SiglaEstado = deputado.Estado?.Sigla,
                NomePartido = deputado.Partido?.Nome,
                IdEstado = deputado.IdEstado,
                SiglaPartido = deputado.Partido?.Sigla,
                NomeEstado = deputado.Estado?.Nome,
                NomeParlamentar = deputado.NomeParlamentar,
                NomeCivil = deputado.NomeCivil,
                Sexo = deputado.Sexo,
                Telefone = deputado.Telefone,
                Email = deputado.Email,
                Profissao = deputado.Profissao,
                Naturalidade = deputado.Naturalidade,
                Site = deputado.Site,
                Perfil = deputado.UrlPerfil,
                Foto = deputado.UrlFoto,
                Nascimento = Utils.NascimentoFormatado(deputado.Nascimento),
                ValorTotal = Utils.FormataValor(deputado.ValorTotalCeap)
            };
        }

        public async Task<List<DeputadoFornecedorDTO>> MaioresFornecedores(int id)
        {
            var maioresFornecedores = await _context.DespesasAssembleias
                .Where(d => d.IdDeputado == id && d.ValorLiquido > 0)
                .GroupBy(d => d.IdFornecedor)
                .Select(g => new
                {
                    IdFornecedor = g.Key,
                    ValorTotal = g.Sum(d => d.ValorLiquido)
                })
                .OrderByDescending(g => g.ValorTotal)
                .Take(10)
                .Join(_context.Fornecedores,
                    g => g.IdFornecedor,
                    f => f.Id,
                    (g, f) => new { g.IdFornecedor, f.CnpjCpf, f.Nome, g.ValorTotal })
                .OrderByDescending(x => x.ValorTotal)
                .ToListAsync();

            return maioresFornecedores.Select(f => new DeputadoFornecedorDTO
            {
                IdFornecedor = f.IdFornecedor,
                CnpjCpf = Utils.FormatCnpjCpf(f.CnpjCpf ?? ""),
                NomeFornecedor = f.Nome ?? "",
                ValorTotal = Utils.FormataValor(f.ValorTotal)
            }).ToList();
        }

        public async Task<List<ParlamentarNotaDTO>> MaioresNotas(int id)
        {
            var maioresNotas = await _context.DespesasAssembleias
                .Where(d => d.IdDeputado == id && d.ValorLiquido > 0)
                .OrderByDescending(d => d.ValorLiquido)
                .Take(10)
                .Join(_context.Fornecedores,
                    d => d.IdFornecedor,
                    f => f.Id,
                    (d, f) => new { d.Id, d.IdFornecedor, f.CnpjCpf, f.Nome, d.ValorLiquido })
                .OrderByDescending(x => x.ValorLiquido)
                .ToListAsync();

            return maioresNotas.Select(n => new ParlamentarNotaDTO
            {
                IdDespesa = n.Id.ToString(),
                IdFornecedor = n.IdFornecedor.ToString(),
                CnpjCpf = Utils.FormatCnpjCpf(n.CnpjCpf ?? ""),
                NomeFornecedor = n.Nome ?? "",
                ValorLiquido = Utils.FormataValor(n.ValorLiquido)
            }).ToList();
        }

        public async Task<DocumentoDetalheDTO> Documento(int id)
        {
            // using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();

                strSql.AppendLine(@"
					SELECT
				        l.id as id_despesa
				        , l.numero_documento
				        , l.data_emissao
				        , l.valor_liquido
				        , l.ano_mes
				        , td.id as id_despesa_tipo
				        , td.descricao as descricao_despesa_tipo
                        , te.descricao as descricao_despesa_especificacao
				        , d.id as id_parlamentar
				        , d.nome_parlamentar
				        , e.id as id_estado
				        , e.sigla as sigla_estado
				        , p.sigla as sigla_partido
				        , pj.id AS id_fornecedor
				        , pj.cnpj_cpf
				        , pj.nome AS nome_fornecedor
                        , l.favorecido        
                        , l.observacao
                    FROM assembleias.cl_despesa l
                    LEFT JOIN fornecedor.fornecedor pj ON pj.id = l.id_fornecedor
                    LEFT JOIN assembleias.cl_deputado d ON d.id = l.id_cl_deputado
                    LEFT JOIN assembleias.cl_despesa_tipo td ON td.id = l.id_cl_despesa_tipo
                    LEFT JOIN assembleias.cl_despesa_especificacao te ON te.id = l.id_cl_despesa_especificacao
                    LEFT JOIN partido p on p.id = d.id_partido
                    LEFT JOIN estado e on e.id = d.id_estado
					WHERE l.id = @id
				 ");

                await using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString(), new { id }))
                {
                    if (await reader.ReadAsync())
                    {
                        string cnpjCpf = Utils.FormatCnpjCpf(reader["cnpj_cpf"].ToString());
                        var anoMes = reader["ano_mes"].ToString();
                        var ano = Convert.ToInt16(anoMes.Substring(0, 4));
                        var mes = Convert.ToInt16(anoMes.Substring(4));
                        var idEstado = Convert.ToInt16(reader["id_estado"]);

                        var documento = new DocumentoDetalheDTO
                        {
                            IdDespesa = Convert.ToInt64(reader["id_despesa"]),
                            NumeroDocumento = reader["numero_documento"].ToString(),
                            DataEmissao = Utils.FormataData(reader["data_emissao"]),
                            ValorLiquido = Utils.FormataValor(Convert.ToDecimal(reader["valor_liquido"])),
                            Ano = ano,
                            Mes = mes,
                            Competencia = $"{mes:00}/{ano:0000}",
                            IdDespesaTipo = Convert.ToInt32(reader["id_despesa_tipo"]),
                            DescricaoDespesa = reader["descricao_despesa_tipo"].ToString(),
                            DescricaoDespesaEspecificacao = reader["descricao_despesa_especificacao"].ToString(),
                            IdParlamentar = Convert.ToInt32(reader["id_parlamentar"]),
                            NomeParlamentar = reader["nome_parlamentar"].ToString(),
                            SiglaEstado = reader["sigla_estado"].ToString(),
                            SiglaPartido = reader["sigla_partido"].ToString(),
                            IdFornecedor = Convert.ToInt64(reader["id_fornecedor"]),
                            CnpjCpf = cnpjCpf,
                            NomeFornecedor = reader["nome_fornecedor"].ToString(),
                            Favorecido = reader["favorecido"].ToString(),
                            Observacao = reader["observacao"].ToString()
                        };

                        // TODO Implementar links das despesas
                        if (documento.Observacao.StartsWith("http", StringComparison.Ordinal))
                        {
                            documento.UrlDocumento = documento.Observacao;
                            documento.Observacao = null;
                        }

                        return documento;
                    }
                }
                return null;
            }
        }

        public async Task<List<DocumentoRelacionadoDTO>> DocumentosDoMesmoDia(int id)
        {
            // using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();

                strSql.AppendLine(@"
					SELECT
						l.id as id_despesa
						, pj.id as id_fornecedor
						, pj.cnpj_cpf
						, pj.nome as nome_fornecedor
						, pji.estado as sigla_estado_fornecedor
						, l.valor_liquido
					FROM (
						select id, id_cl_deputado, id_fornecedor, data_emissao FROM assembleias.cl_despesa
						where id = @id
					) l1 
					INNER JOIN assembleias.cl_despesa l on
						l1.id_cl_deputado = l.id_cl_deputado and
						l1.data_emissao = l.data_emissao and
						l1.id <> l.id
					LEFT JOIN fornecedor.fornecedor pj ON pj.id = l.id_fornecedor
					LEFT JOIN fornecedor.fornecedor_info pji ON pji.id_fornecedor = pj.id
					order by l.valor_liquido desc
					limit 50
				 ");

                var lstRetorno = new List<DocumentoRelacionadoDTO>();

                DbDataReader reader = await ExecuteReaderAsync(strSql.ToString(), new { id });
                while (await reader.ReadAsync())
                {
                    lstRetorno.Add(new DocumentoRelacionadoDTO
                    {
                        IdDespesa = Convert.ToInt32(reader["id_despesa"]),
                        IdFornecedor = Convert.ToInt32(reader["id_fornecedor"]),
                        CnpjCpf = Utils.FormatCnpjCpf(reader["cnpj_cpf"].ToString()),
                        NomeFornecedor = reader["nome_fornecedor"].ToString(),
                        SiglaEstadoFornecedor = reader["sigla_estado_fornecedor"].ToString(),
                        ValorLiquido = Utils.FormataValor(reader["valor_liquido"])
                    });
                }

                return lstRetorno;
            }
        }

        public async Task<List<DocumentoRelacionadoDTO>> DocumentosDaSubcotaMes(int id)
        {
            // using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();

                strSql.AppendLine(@"
					SELECT
						l.id as id_despesa
						, pj.id as id_fornecedor
                        , pj.cnpj_cpf
						, pj.nome as nome_fornecedor
						, pji.estado as sigla_estado_fornecedor
						, l.valor_liquido
					FROM (
						select id, id_cl_deputado, id_fornecedor, id_cl_despesa_tipo, ano_mes 
                        FROM assembleias.cl_despesa
						where id = @id
					) l1 
					INNER JOIN assembleias.cl_despesa l on
					    l1.id_cl_deputado = l.id_cl_deputado and
					    l1.ano_mes = l.ano_mes and
					    l1.id_cl_despesa_tipo = l.id_cl_despesa_tipo and
					    l1.id <> l.id
					LEFT JOIN fornecedor.fornecedor pj ON pj.id = l.id_fornecedor
					LEFT JOIN fornecedor.fornecedor_info pji ON pji.id_fornecedor = pj.id
					order by l.valor_liquido desc
					limit 50
				 ");

                var lstRetorno = new List<DocumentoRelacionadoDTO>();

                DbDataReader reader = await ExecuteReaderAsync(strSql.ToString(), new { id });
                while (await reader.ReadAsync())
                {
                    lstRetorno.Add(new DocumentoRelacionadoDTO
                    {
                        IdDespesa = Convert.ToInt32(reader["id_despesa"]),
                        IdFornecedor = Convert.ToInt32(reader["id_fornecedor"]),
                        CnpjCpf = Utils.FormatCnpjCpf(reader["cnpj_cpf"].ToString()),
                        NomeFornecedor = reader["nome_fornecedor"].ToString(),
                        SiglaEstadoFornecedor = reader["sigla_estado_fornecedor"].ToString(),
                        ValorLiquido = Utils.FormataValor(reader["valor_liquido"])
                    });
                }

                return lstRetorno;
            }
        }

        public async Task<GraficoBarraDTO> GastosPorAno(int id)
        {
            // Note: Using DespesasAssembleias which has valor field instead of valor_liquido
            // and ano field needs to be derived from data_emissao or available in the entity
            var gastosPorAno = await _context.DespesasAssembleias
                .Where(d => d.IdDeputado == id && d.ValorLiquido > 0 && d.DataEmissao.HasValue)
                .GroupBy(d => d.DataEmissao.Value.Year)
                .Select(g => new
                {
                    Ano = g.Key,
                    ValorTotal = g.Sum(d => d.ValorLiquido)
                })
                .OrderBy(g => g.Ano)
                .ToListAsync();

            return new GraficoBarraDTO
            {
                Categories = gastosPorAno.Select(g => g.Ano).ToList(),
                Series = gastosPorAno.Select(g => g.ValorTotal).ToList()
            };
        }

        public async Task<List<ParlamentarCustoAnualDTO>> CustoAnual(int id)
        {
            var result = new List<ParlamentarCustoAnualDTO>();
            //using (AppDb banco = new AppDb())
            {
                var strSql = @"
					SELECT d.ano_mes/100 as ano , SUM(d.valor_liquido) AS valor_total
                    FROM assembleias.cl_despesa d
                    join assembleias.cl_despesa_tipo t on t.id = d.id_cl_despesa_tipo
                    WHERE d.id_cl_deputado = @id
                    and t.id NOT IN(10, 11)
                    group by d.ano_mes/100
				";

                using (DbDataReader reader = await ExecuteReaderAsync(strSql, new { id }))
                {
                    while (await reader.ReadAsync())
                    {
                        var dto = new ParlamentarCustoAnualDTO
                        {
                            Ano = Convert.ToInt32(reader["ano"]),
                            CotaParlamentar = Convert.ToDecimal(reader["valor_total"])
                        };
                        result.Add(dto);
                    }
                }

                strSql = @"
					SELECT d.ano_mes/100 as ano , SUM(d.valor_liquido) AS valor_total
                    FROM assembleias.cl_despesa d
                    join assembleias.cl_despesa_tipo t on t.id = d.id_cl_despesa_tipo
                    WHERE d.id_cl_deputado = @id
                    and t.id = 11
                    group by d.ano_mes/100
				";

                using (DbDataReader reader = await ExecuteReaderAsync(strSql, new { id }))
                {
                    while (await reader.ReadAsync())
                    {
                        var ano = Convert.ToInt32(reader["ano"]);
                        var dto = result.FirstOrDefault(x => x.Ano == ano);

                        if (dto != null)
                        {
                            dto.AuxilioSaude = Convert.ToDecimal(reader["valor_total"]);
                        }
                        else
                        {
                            dto = new ParlamentarCustoAnualDTO
                            {
                                Ano = ano,
                                AuxilioSaude = Convert.ToDecimal(reader["valor_total"])
                            };
                            result.Add(dto);
                        }
                    }
                }

                strSql = @"
					SELECT d.ano_mes/100 as ano , SUM(d.valor_liquido) AS valor_total
                    FROM assembleias.cl_despesa d
                    join assembleias.cl_despesa_tipo t on t.id = d.id_cl_despesa_tipo
                    WHERE d.id_cl_deputado = @id
                    and t.id = 10
                    group by d.ano_mes/100
				";

                using (DbDataReader reader = await ExecuteReaderAsync(strSql, new { id }))
                {
                    while (await reader.ReadAsync())
                    {
                        var ano = Convert.ToInt32(reader["ano"]);
                        var dto = result.FirstOrDefault(x => x.Ano == ano);

                        if (dto != null)
                        {
                            dto.Diarias = Convert.ToDecimal(reader["valor_total"]);
                        }
                        else
                        {
                            dto = new ParlamentarCustoAnualDTO
                            {
                                Ano = ano,
                                Diarias = Convert.ToDecimal(reader["valor_total"])
                            };
                            result.Add(dto);
                        }
                    }
                }
            }

            return result.OrderBy(x => x.Ano).ToList();
        }

        public async Task<dynamic> ResumoMensal()
        {
            var resumoMensal = await _context.DespesaResumosMensais
                .OrderBy(r => r.Ano)
                .ThenBy(r => r.Mes)
                .ToListAsync();

            var lstRetorno = new List<dynamic>();
            var lstValoresMensais = new decimal?[12];
            string anoControle = string.Empty;
            bool existeGastoNoAno = false;

            foreach (var item in resumoMensal)
            {
                if (item.Ano.ToString() != anoControle)
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

                    anoControle = item.Ano.ToString();
                }

                if (item.Valor > 0)
                {
                    lstValoresMensais[item.Mes - 1] = item.Valor.Value;
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

        public async Task<dynamic> ResumoAnual()
        {
            var resumoAnual = await _context.DespesaResumosMensais
                .GroupBy(r => r.Ano)
                .Select(g => new
                {
                    Ano = g.Key,
                    ValorTotal = g.Sum(r => r.Valor)
                })
                .OrderBy(g => g.Ano)
                .ToListAsync();

            return new
            {
                categories = resumoAnual.Select(r => r.Ano),
                series = resumoAnual.Select(r => r.ValorTotal)
            };
        }

        public async Task<List<ParlamentarListaDTO>> Lista(FiltroParlamentarDTO request)
        {
            // using (AppDb banco = new AppDb())
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
                        -- , d.situacao
					FROM assembleias.cl_deputado d
					LEFT JOIN partido p on p.id = d.id_partido
					LEFT JOIN estado e on e.id = d.id_estado
                    WHERE 1=1");

                //if (request.Periodo > 50)
                //{
                //    strSql.AppendLine($" AND d.id IN(select m.id_cl_deputado FROM assembleias.cl_mandato m where m.id_legislatura = {request.Periodo.ToString()})");
                //}

                if (!string.IsNullOrEmpty(request.Partido))
                {
                    strSql.AppendLine("	AND d.id_partido IN(" + Utils.MySqlEscapeNumberToIn(request.Partido) + ") ");
                }

                if (!string.IsNullOrEmpty(request.Estado))
                {
                    strSql.AppendLine("	AND d.id_estado IN(" + Utils.MySqlEscapeNumberToIn(request.Estado) + ") ");
                }

                if (!string.IsNullOrEmpty(request.NomeParlamentar))
                {
                    strSql.AppendLine("	AND (d.nome_parlamentar ILIKE '%" + Utils.MySqlEscape(request.NomeParlamentar) + "%' or d.nome_civil ILIKE '%" + Utils.MySqlEscape(request.NomeParlamentar) + "%')");
                }

                strSql.AppendLine(@"
                    ORDER BY nome_parlamentar
                    LIMIT 1000
				");

                var lstRetorno = new List<ParlamentarListaDTO>();
                using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new ParlamentarListaDTO
                        {
                            IdParlamentar = Convert.ToInt32(reader["id_cl_deputado"]),
                            NomeParlamentar = reader["nome_parlamentar"].ToString(),
                            NomeCivil = reader["nome_civil"].ToString(),
                            ValorTotalCeap = Utils.FormataValor(reader["valor_total_ceap"]),
                            ValorTotalRemuneracao = Utils.FormataValor(reader["valor_total_remuneracao"]),
                            SiglaPartido = !string.IsNullOrEmpty(reader["sigla_partido"].ToString()) ? reader["sigla_partido"].ToString() : "S.PART.",
                            NomePartido = !string.IsNullOrEmpty(reader["nome_partido"].ToString()) ? reader["nome_partido"].ToString() : "<Sem Partido>",
                            SiglaEstado = reader["sigla_estado"].ToString(),
                            NomeEstado = reader["nome_estado"].ToString(),
                            //situacao = reader["situacao"].ToString(),
                            //ativo = reader["situacao"].ToString() == "Exercício",
                        });
                    }
                }
                return lstRetorno;
            }
        }

        public async Task<List<DropDownDTO>> Pesquisa(MultiSelectRequest filtro = null)
        {
            // using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT DISTINCT
						d.id, d.nome_civil, d.nome_parlamentar 
					FROM assembleias.cl_deputado d
				");

                if (filtro != null && string.IsNullOrEmpty(filtro.Ids))
                {
                    strSql.AppendLine(@"
                        WHERE (1=1)");

                    if (!string.IsNullOrEmpty(filtro.Busca))
                    {
                        var busca = Utils.MySqlEscape(filtro.Busca);
                        strSql.AppendLine(@" AND (d.nome_parlamentar ILIKE '%" + busca + "%' or d.nome_civil ILIKE '%" + busca + "%') ");
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

                var lstRetorno = new List<DropDownDTO>();
                using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new DropDownDTO
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Tokens = new[] { reader["nome_civil"].ToString(), reader["nome_parlamentar"].ToString() },
                            Text = reader["nome_parlamentar"].ToString()
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

        private async Task<DataTablesResponseDTO<LancamentoParlamentarDTO>> LancamentosParlamentar(DataTablesRequest request)
        {
            var sqlWhere = GetWhereFilter(request);
            var sqlSortAndPaging = GetSortAndPaging(request);

            var sql = $@"
SELECT 
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
    FROM assembleias.cl_despesa l
    WHERE (1=1)
    {sqlWhere}
    GROUP BY id_cl_deputado
) l1
JOIN assembleias.cl_deputado d on d.id = l1.id_cl_deputado
LEFT JOIN partido p on p.id = d.id_partido
LEFT JOIN estado e on e.id = d.id_estado
{sqlSortAndPaging};

SELECT COUNT(DISTINCT id_cl_deputado) 
FROM assembleias.cl_despesa l
WHERE (1=1) 
{sqlWhere};";

            var lstRetorno = new List<LancamentoParlamentarDTO>();
            using (DbDataReader reader = await ExecuteReaderAsync(sql))
            {
                while (await reader.ReadAsync())
                {
                    lstRetorno.Add(new LancamentoParlamentarDTO
                    {
                        IdParlamentar = Convert.ToInt32(reader["id_cl_deputado"]),
                        NomeParlamentar = reader["nome_parlamentar"].ToString(),
                        SiglaEstado = reader["sigla_estado"].ToString(),
                        SiglaPartido = reader["sigla_partido"].ToString(),
                        TotalNotas = Convert.ToInt32(reader["total_notas"]),
                        ValorTotal = Utils.FormataValor(reader["valor_total"])
                    });
                }

                var TotalCount = reader.GetTotalRowsFound();
                return new DataTablesResponseDTO<LancamentoParlamentarDTO>
                {
                    recordsTotal = TotalCount,
                    recordsFiltered = TotalCount,
                    data = lstRetorno
                };
            }
        }

        private async Task<DataTablesResponseDTO<LancamentoFornecedorDTO>> LancamentosFornecedor(DataTablesRequest request)
        {
            var sqlWhere = GetWhereFilter(request);
            var sqlSortAndPaging = GetSortAndPaging(request);

            var sql = $@"
SELECT 
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
	FROM assembleias.cl_despesa l
	WHERE (1=1)
    {sqlWhere}
	GROUP BY l.id_fornecedor
) l1
LEFT JOIN fornecedor.fornecedor pj on pj.id = l1.id_fornecedor
{sqlSortAndPaging};

SELECT COUNT(DISTINCT id_fornecedor) 
FROM assembleias.cl_despesa l
WHERE (1=1) 
{sqlWhere};";

            var lstRetorno = new List<LancamentoFornecedorDTO>();
            using (DbDataReader reader = await ExecuteReaderAsync(sql))
            {
                while (await reader.ReadAsync())
                {
                    lstRetorno.Add(new LancamentoFornecedorDTO
                    {
                        IdFornecedor = Convert.ToInt32(reader["id_fornecedor"]),
                        CnpjCpf = Utils.FormatCnpjCpf(reader["cnpj_cpf"].ToString()),
                        NomeFornecedor = reader["nome_fornecedor"].ToString(),
                        TotalNotas = Utils.FormataValor(reader["total_notas"], 0),
                        ValorTotal = Utils.FormataValor(reader["valor_total"])
                    });
                }

                var TotalCount = reader.GetTotalRowsFound();
                return new DataTablesResponseDTO<LancamentoFornecedorDTO>
                {
                    recordsTotal = TotalCount,
                    recordsFiltered = TotalCount,
                    data = lstRetorno
                };
            }
        }

        private async Task<DataTablesResponseDTO<LancamentoDespesaDTO>> LancamentosDespesa(DataTablesRequest request)
        {
            var sqlWhere = GetWhereFilter(request);
            var sqlSortAndPaging = GetSortAndPaging(request);

            var sql = $@"
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
    FROM assembleias.cl_despesa l
    WHERE (1=1)
    {sqlWhere}
    GROUP BY id_cl_despesa_tipo
) l1
LEFT JOIN assembleias.cl_despesa_tipo td on td.id = l1.id_cl_despesa_tipo
{sqlSortAndPaging};

SELECT COUNT(DISTINCT id_cl_despesa_tipo) 
FROM assembleias.cl_despesa l
WHERE (1=1) 
{sqlWhere};";

            var lstRetorno = new List<LancamentoDespesaDTO>();
            using (DbDataReader reader = await ExecuteReaderAsync(sql))
            {
                while (await reader.ReadAsync())
                {
                    lstRetorno.Add(new LancamentoDespesaDTO
                    {
                        IdDespesaTipo = Convert.ToInt32(reader["id_cl_despesa_tipo"]),
                        Descricao = reader["descricao"].ToString(),
                        TotalNotas = Utils.FormataValor(reader["total_notas"], 0),
                        ValorTotal = Utils.FormataValor(reader["valor_total"])
                    });
                }

                var TotalCount = reader.GetTotalRowsFound();
                return new DataTablesResponseDTO<LancamentoDespesaDTO>
                {
                    recordsTotal = TotalCount,
                    recordsFiltered = TotalCount,
                    data = lstRetorno
                };
            }
        }

        private async Task<DataTablesResponseDTO<LancamentoPartidoDTO>> LancamentosPartido(DataTablesRequest request)
        {
            var sqlWhere = GetWhereFilter(request);
            var sqlSortAndPaging = GetSortAndPaging(request);

            var sql = $@"
					
SELECT 
    p.id as id_partido
    , p.nome as nome_partido
    , sum(l1.total_notas) as total_notas
    , sum(l1.valor_total) as valor_total
FROM (
    SELECT
        count(l.id) AS total_notas
        , sum(l.valor_liquido) as valor_total
        , l.id_cl_deputado
    FROM assembleias.cl_despesa l
    WHERE (1=1)
    {sqlWhere}
    GROUP BY id_cl_deputado
) l1
INNER JOIN assembleias.cl_deputado d on d.id = l1.id_cl_deputado
LEFT JOIN partido p on p.id = d.id_partido
GROUP BY p.id, p.nome
{sqlSortAndPaging};

SELECT COUNT(DISTINCT id_partido) 
FROM assembleias.cl_despesa l
INNER JOIN assembleias.cl_deputado d on d.id = l.id_cl_deputado
WHERE (1=1) 
{sqlWhere};";

            var lstRetorno = new List<LancamentoPartidoDTO>();
            using (DbDataReader reader = await ExecuteReaderAsync(sql))
            {
                while (await reader.ReadAsync())
                {
                    lstRetorno.Add(new LancamentoPartidoDTO
                    {
                        IdPartido = Convert.ToInt32(reader["id_partido"]),
                        NomePartido = reader["nome_partido"].ToString(),
                        TotalNotas = Utils.FormataValor(reader["total_notas"], 0),
                        ValorTotal = Utils.FormataValor(reader["valor_total"])
                    });
                }

                var TotalCount = reader.GetTotalRowsFound();
                return new DataTablesResponseDTO<LancamentoPartidoDTO>
                {
                    recordsTotal = TotalCount,
                    recordsFiltered = TotalCount,
                    data = lstRetorno
                };
            }
        }

        private async Task<DataTablesResponseDTO<LancamentoEstadoDTO>> LancamentosEstado(DataTablesRequest request)
        {
            var sqlWhere = GetWhereFilter(request);
            var sqlSortAndPaging = GetSortAndPaging(request);

            var sql = $@"
SELECT 
    e.id AS id_estado
    , e.nome as nome_estado
    , sum(l.total_notas) as total_notas
    , sum(l.valor_total) as valor_total
from (
    SELECT
        count(l.id) AS total_notas
        , sum(l.valor_liquido) as valor_total
        , l.id_cl_deputado
    FROM assembleias.cl_despesa l
    WHERE (1=1)
    {sqlWhere}
    GROUP BY id_cl_deputado
) l
JOIN assembleias.cl_deputado d on d.id = l.id_cl_deputado
LEFT JOIN estado e on e.id = d.id_estado
GROUP BY e.id, e.nome
{sqlSortAndPaging};

SELECT COUNT(DISTINCT id_estado) 
FROM assembleias.cl_despesa l
JOIN assembleias.cl_deputado d on d.id = l.id_cl_deputado
WHERE (1=1) 
{sqlWhere};";

            var lstRetorno = new List<LancamentoEstadoDTO>();
            using (DbDataReader reader = await ExecuteReaderAsync(sql))
            {
                while (await reader.ReadAsync())
                {
                    lstRetorno.Add(new LancamentoEstadoDTO
                    {
                        IdEstado = Convert.ToInt32(reader["id_estado"]),
                        NomeEstado = reader["nome_estado"].ToString(),
                        TotalNotas = Convert.ToInt32(reader["total_notas"]),
                        ValorTotal = Utils.FormataValor(reader["valor_total"])
                    });
                }

                var TotalCount = reader.GetTotalRowsFound();
                return new DataTablesResponseDTO<LancamentoEstadoDTO>
                {
                    recordsTotal = TotalCount,
                    recordsFiltered = TotalCount,
                    data = lstRetorno
                };
            }
        }

        private async Task<DataTablesResponseDTO<LancamentoDocumentoDTO>> LancamentosNotaFiscal(DataTablesRequest request)
        {
            var sqlWhere = GetWhereFilter(request);
            var sqlSortAndPaging = GetSortAndPaging(request, "l.ano_mes DESC, l.data_emissao DESC, l.valor_liquido DESC");

            var sql = $@"
SELECT 
	l.data_emissao
	, pj.nome AS nome_fornecedor
	, d.nome_parlamentar
	, l.valor_total
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
	SELECT data_emissao, id, id_cl_deputado, valor_liquido as valor_total, id_cl_despesa_tipo, id_cl_despesa_especificacao, id_fornecedor, favorecido
	FROM assembleias.cl_despesa l
	WHERE (1=1)
    {sqlWhere}
    {sqlSortAndPaging}
) l
INNER JOIN assembleias.cl_deputado d on d.id = l.id_cl_deputado
LEFT JOIN fornecedor.fornecedor pj on pj.id = l.id_fornecedor
LEFT JOIN partido p on p.id = d.id_partido
LEFT JOIN estado e on e.id = d.id_estado
LEFT JOIN assembleias.cl_despesa_tipo t on t.id = l.id_cl_despesa_tipo
LEFT JOIN assembleias.cl_despesa_especificacao de on de.id = l.id_cl_despesa_especificacao;

SELECT COUNT(*) 
FROM assembleias.cl_despesa l
WHERE (1=1) 
{sqlWhere};";

            var lstRetorno = new List<LancamentoDocumentoDTO>();
            using (DbDataReader reader = await ExecuteReaderAsync(sql))
            {
                while (await reader.ReadAsync())
                {
                    lstRetorno.Add(new LancamentoDocumentoDTO()
                    {
                        IdDespesa = reader["id_cl_despesa"],
                        DataEmissao = Utils.FormataData(reader["data_emissao"]),
                        IdFornecedor = Convert.ToInt32(reader["id_fornecedor"]),
                        CnpjCpf = Utils.FormatCnpjCpf(reader["cnpj_cpf"].ToString()),
                        NomeFornecedor = reader["nome_fornecedor"].ToString(),
                        IdParlamentar = Convert.ToInt32(reader["id_cl_deputado"]),
                        NomeParlamentar = reader["nome_parlamentar"].ToString(),
                        SiglaEstado = reader["sigla_estado"].ToString(),
                        SiglaPartido = reader["sigla_partido"].ToString(),
                        DespesaTipo = reader["despesa_tipo"].ToString(),
                        DespesaEspecificacao = reader["despesa_especificacao"].ToString(),
                        Favorecido = reader["favorecido"].ToString(),
                        ValorTotal = Utils.FormataValor(reader["valor_total"])
                    });
                }

                var TotalCount = reader.GetTotalRowsFound();
                return new DataTablesResponseDTO<LancamentoDocumentoDTO>
                {
                    recordsTotal = TotalCount,
                    recordsFiltered = TotalCount,
                    data = lstRetorno
                };
            }
        }

        private void AdicionaFiltroPeriodo(DataTablesRequest request, StringBuilder sqlSelect)
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

        private void AdicionaFiltroPartidoDeputado(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.ContainsKey("Partido") && !string.IsNullOrEmpty(request.Filters["Partido"].ToString()))
            {
                sqlSelect.AppendLine("	AND l.id_cl_deputado IN (SELECT id FROM assembleias.cl_deputado where id_partido IN(" + request.Filters["Partido"].ToString() + ")) ");
            }
        }

        private void AdicionaFiltroEstadoDeputado(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.ContainsKey("Estado") && !string.IsNullOrEmpty(request.Filters["Estado"].ToString()))
            {
                sqlSelect.AppendLine("	AND l.id_cl_deputado IN (SELECT id FROM assembleias.cl_deputado where id_estado IN(" + Utils.MySqlEscapeNumberToIn(request.Filters["Estado"].ToString()) + ")) ");
            }
        }

        private void AdicionaFiltroFornecedor(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.ContainsKey("Fornecedor") && !string.IsNullOrEmpty(request.Filters["Fornecedor"].ToString()))
            {
                var Fornecedor = string.Join("", System.Text.RegularExpressions.Regex.Split(request.Filters["Fornecedor"].ToString(), @"[^\d]"));

                if (!string.IsNullOrEmpty(Fornecedor))
                {
                    if (Fornecedor.Length == 14 || Fornecedor.Length == 11)
                    {
                        // using (AppDb banco = new AppDb())
                        {
                            var cnpjCpf = Utils.RemoveCaracteresNaoNumericos(Fornecedor);
                            var id_fornecedor = _context.Fornecedores.Where(x => x.CnpjCpf == cnpjCpf).Select(x => x.Id).FirstOrDefault();

                            if (!Convert.IsDBNull(id_fornecedor))
                            {
                                sqlSelect.AppendLine("	AND l.id_fornecedor =" + id_fornecedor + " ");
                            }
                        }
                    }
                    else if (Fornecedor.Length == 8) //CNPJ raiz
                    {
                        sqlSelect.AppendLine("	AND l.id_fornecedor IN (select id from fornecedor where cnpj_cpf ILIKE '" + Utils.RemoveCaracteresNaoNumericos(Fornecedor) + "%')");
                    }
                    else
                    {
                        sqlSelect.AppendLine("	AND l.id_fornecedor =" + Utils.RemoveCaracteresNaoNumericos(Fornecedor) + " ");
                    }
                }
            }
        }

        private void AdicionaFiltroDespesa(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.ContainsKey("Despesa") && !string.IsNullOrEmpty(request.Filters["Despesa"].ToString()))
            {
                sqlSelect.AppendLine("	AND l.id_cl_despesa_tipo IN (" + Utils.MySqlEscapeNumberToIn(request.Filters["Despesa"].ToString()) + ") ");
            }
        }

        private void AdicionaFiltroDeputado(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.ContainsKey("IdParlamentar") && !string.IsNullOrEmpty(request.Filters["IdParlamentar"].ToString()))
            {
                sqlSelect.AppendLine("	AND l.id_cl_deputado IN (" + Utils.MySqlEscapeNumberToIn(request.Filters["IdParlamentar"].ToString()) + ") ");
            }
        }

        private void AdicionaFiltroDocumento(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.ContainsKey("Documento") && !string.IsNullOrEmpty(request.Filters["Documento"].ToString()))
            {
                sqlSelect.AppendLine("	AND l.numero_documento ILIKE '%" + Utils.MySqlEscape(request.Filters["Documento"].ToString()) + "' ");
            }
        }

        private string GetWhereFilter(DataTablesRequest request)
        {
            StringBuilder sql = new StringBuilder();

            AdicionaFiltroPeriodo(request, sql);
            AdicionaFiltroDeputado(request, sql);
            AdicionaFiltroDespesa(request, sql);
            AdicionaFiltroFornecedor(request, sql);
            AdicionaFiltroPartidoDeputado(request, sql);
            AdicionaFiltroEstadoDeputado(request, sql);

            return sql.ToString();
        }

        private string GetSortAndPaging(DataTablesRequest request, string defaultSorting = "valor_total desc")
        {
            var sql = new StringBuilder();

            sql.AppendFormat(" ORDER BY {0} ", request.GetSorting("valor_total desc"));
            sql.AppendFormat(" LIMIT {1} OFFSET {0} ", request.Start, request.Length);

            return sql.ToString();
        }

        public async Task<List<TipoDespesaDTO>> TipoDespesa()
        {
            // using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine("SELECT id, descricao FROM assembleias.cl_despesa_tipo ");
                strSql.AppendFormat("ORDER BY descricao ");

                var lstRetorno = new List<TipoDespesaDTO>();
                using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new TipoDespesaDTO
                        {
                            Id = reader["id"].ToString(),
                            Text = reader["descricao"].ToString(),
                        });
                    }
                }
                return lstRetorno;
            }
        }
    }
}