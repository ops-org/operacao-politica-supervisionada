using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
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
    public class SenadorRepository : BaseRepository, IParlamentarRepository
    {
        public SenadorRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<ParlamentarDetalheDTO> Consultar(int id)
        {
            // using (AppDb banco = new AppDb())
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
                        , (SELECT m.participacao from senado.sf_mandato m WHERE m.id_sf_senador = d.id ORDER BY m.id desc LIMIT 1) as participacao
                        , d.naturalidade
                        , e.sigla as silga_estado_naturalidade
					from senado.sf_senador d
					LEFT JOIN partido p on p.id = d.id_partido
					LEFT JOIN estado e on e.id = d.id_estado
					LEFT JOIN estado en on e.id = d.id_estado_naturalidade
					WHERE d.id = @id
				";

                using (var reader = await ExecuteReaderAsync(strSql, new { id }))
                {
                    if (await reader.ReadAsync())
                    {
                        var exercicio = "";
                        switch (reader["ativo"].ToString())
                        {
                            case "S": exercicio = "Em exercício"; break;
                            case "N": exercicio = "Fora de Exercício"; break;
                        }

                        var participacao = reader["participacao"].ToString();
                        switch (participacao)
                        {
                            case "T": participacao = "Titular"; break;
                            case "1": participacao = "1º Suplente"; break;
                            case "2": participacao = "2º Suplente"; break;
                        }

                        return new ParlamentarDetalheDTO
                        {
                            IdParlamentar = Convert.ToInt32(reader["id_sf_senador"]),
                            NomeParlamentar = reader["nome_parlamentar"].ToString(),
                            NomeCivil = reader["nome_civil"].ToString(),
                            Nascimento = Utils.NascimentoFormatado(reader["nascimento"]),
                            Sexo = reader["sexo"].ToString(),
                            IdPartido = reader["id_partido"] as int?,
                            SiglaEstado = reader["sigla_estado"].ToString(),
                            NomePartido = reader["nome_partido"].ToString(),
                            IdEstado = reader["id_estado"] as int?,
                            SiglaPartido = reader["sigla_partido"].ToString(),
                            NomeEstado = reader["nome_estado"].ToString(),
                            Email = reader["email"].ToString(),
                            Condicao = $"{participacao} ({exercicio})",
                            Naturalidade = reader["naturalidade"].ToString() + (!string.IsNullOrEmpty(reader["silga_estado_naturalidade"].ToString()) ? "(" + reader["silga_estado_naturalidade"].ToString() + ")" : ""),
                            ValorTotalCeap = Utils.FormataValor(reader["valor_total_ceaps"]),
                            ValorTotalRemuneracao = Utils.FormataValor(reader["valor_total_remuneracao"]),
                            ValorTotal = Utils.FormataValor(reader["valor_total"]),
                        };
                    }

                    return null;
                }
            }
        }

        public async Task<List<ParlamentarListaDTO>> Lista(FiltroParlamentarDTO request)
        {
            // using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT DISTINCT
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
					from senado.sf_senador d
					LEFT JOIN partido p on p.id = d.id_partido
					LEFT JOIN estado e on e.id = d.id_estado
                    JOIN senado.sf_mandato m ON m.id_sf_senador = d.id
					JOIN senado.sf_mandato_legislatura ml on ml.id_sf_mandato = m.id
                    WHERE m.exerceu = true");

                if (request.Periodo > 0)
                {
                    strSql.AppendLine($" AND ml.id_sf_legislatura = {request.Periodo} ");
                }

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
                    strSql.AppendLine("	AND (d.nome ILIKE '%" + Utils.MySqlEscape(request.NomeParlamentar) + "%' or d.nome_completo ILIKE '%" + Utils.MySqlEscape(request.NomeParlamentar) + "%')");
                }

                strSql.AppendLine(@"
                    ORDER BY nome_parlamentar
                    LIMIT 1000
				");

                TextInfo textInfo = new CultureInfo("pt-BR", false).TextInfo;
                var lstRetorno = new List<ParlamentarListaDTO>();
                using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new ParlamentarListaDTO
                        {
                            IdParlamentar = Convert.ToInt32(reader["id_sf_senador"]),
                            NomeParlamentar = reader["nome_parlamentar"].ToString(),
                            NomeCivil = reader["nome_civil"].ToString(),
                            SiglaPartido = !string.IsNullOrEmpty(reader["sigla_partido"].ToString()) ? reader["sigla_partido"].ToString() : "S.PART.",
                            NomePartido = !string.IsNullOrEmpty(reader["nome_partido"].ToString()) ? reader["nome_partido"].ToString() : "SEM PARTIDO",
                            SiglaEstado = reader["sigla_estado"].ToString(),
                            NomeEstado = reader["nome_estado"].ToString(),
                            Ativo = reader["ativo"].ToString() == "S",
                            ValorTotalCeap = Utils.FormataValor(reader["valor_total_ceaps"]),
                            ValorTotalRemuneracao = Utils.FormataValor(reader["valor_total_remuneracao"]),
                        });
                    }
                }
                return lstRetorno;
            }
        }

        public async Task<List<DeputadoFornecedorDTO>> MaioresFornecedores(int id)
        {
            // using (AppDb banco = new AppDb())
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
						from senado.sf_despesa l
						WHERE l.id_sf_senador = @id
						GROUP BY l.id_fornecedor
						order by valor_total desc
						LIMIT 10
					) l1
					LEFT JOIN fornecedor.fornecedor pj on pj.id = l1.id_fornecedor
					order by l1.valor_total desc
				");

                using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString(), new { id }))
                {
                    List<DeputadoFornecedorDTO> lstRetorno = new List<DeputadoFornecedorDTO>();
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new DeputadoFornecedorDTO
                        {
                            IdFornecedor = Convert.ToInt32(reader["id_fornecedor"]),
                            CnpjCpf = Utils.FormatCnpjCpf(reader["cnpj_cpf"].ToString()),
                            NomeFornecedor = reader["nome_fornecedor"].ToString(),
                            ValorTotal = Utils.FormataValor(reader["valor_total"])
                        });
                    }

                    return lstRetorno;
                }
            }
        }

        public async Task<List<ParlamentarNotaDTO>> MaioresNotas(int id)
        {
            // using (AppDb banco = new AppDb())
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
						from senado.sf_despesa l
						WHERE l.id_sf_senador = @id
						order by l.valor desc
						LIMIT 10
					) l1
					LEFT JOIN fornecedor.fornecedor pj on pj.id = l1.id_fornecedor
					order by l1.valor desc 
				");

                using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString(), new { id }))
                {
                    List<ParlamentarNotaDTO> lstRetorno = new List<ParlamentarNotaDTO>();
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new ParlamentarNotaDTO()
                        {
                            IdDespesa = reader["id_sf_despesa"].ToString(),
                            IdFornecedor = reader["id_fornecedor"].ToString(),
                            CnpjCpf = Utils.FormatCnpjCpf(reader["cnpj_cpf"].ToString()),
                            NomeFornecedor = reader["nome_fornecedor"].ToString(),
                            ValorLiquido = Utils.FormataValor(reader["valor"])
                        });
                    }

                    return lstRetorno;
                }
            }
        }

        public async Task<DocumentoDetalheDTO> Documento(int id)
        {
            // using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();

                strSql.AppendLine(@"
					SELECT
                        l.id as id_despesa
                        , l.data_emissao
                        , l.valor as valor_liquido
                        , l.ano_mes
                        , td.id as id_despesa_tipo
                        , td.descricao as descricao_despesa_tipo
                        , d.id as id_parlamentar
                        , d.nome as nome_parlamentar
                        , e.id as id_estado
                        , e.sigla as sigla_estado
                        , p.sigla as sigla_partido
                        , pj.id AS id_fornecedor
                        , pj.cnpj_cpf
                        , pj.nome AS nome_fornecedor
                    FROM senado.sf_despesa l
                    LEFT JOIN fornecedor.fornecedor pj ON pj.id = l.id_fornecedor
                    LEFT JOIN senado.sf_senador d ON d.id = l.id_sf_senador
                    LEFT JOIN senado.sf_despesa_tipo td ON td.id = l.id_sf_despesa_tipo
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
                            DataEmissao = Utils.FormataData(reader["data_emissao"]),
                            ValorLiquido = Utils.FormataValor(Convert.ToDecimal(reader["valor_liquido"])),
                            Ano = ano,
                            Mes = mes,
                            Competencia = $"{mes:00}/{ano:0000}",
                            IdDespesaTipo = Convert.ToInt32(reader["id_despesa_tipo"]),
                            DescricaoDespesa = reader["descricao_despesa_tipo"].ToString(),
                            IdParlamentar = Convert.ToInt32(reader["id_parlamentar"]),
                            NomeParlamentar = reader["nome_parlamentar"].ToString(),
                            SiglaEstado = reader["sigla_estado"].ToString(),
                            SiglaPartido = reader["sigla_partido"].ToString(),
                            IdFornecedor = Convert.ToInt64(reader["id_fornecedor"]),
                            CnpjCpf = cnpjCpf,
                            NomeFornecedor = reader["nome_fornecedor"].ToString()
                        };

                        // TODO Implementar links das despesas
                        if (idEstado == 0)
                        {
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
                        , l.valor as valor_liquido
                    FROM (
                        select id, id_sf_senador, id_fornecedor, data_emissao 
                        FROM senado.sf_despesa
                        where id = @id
                    ) l1 
                    INNER JOIN senado.sf_despesa l on
                        l1.id_sf_senador = l.id_sf_senador and
                        l1.data_emissao = l.data_emissao and
                        l1.id <> l.id
                    LEFT JOIN fornecedor.fornecedor pj ON pj.id = l.id_fornecedor
                    LEFT JOIN fornecedor.fornecedor_info pji ON pji.id_fornecedor = pj.id
                    order by l.valor desc
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
						, l.valor as valor_liquido
					FROM (
						select id, id_sf_senador, id_fornecedor, id_sf_despesa_tipo, ano_mes 
                        FROM senado.sf_despesa
						where id = @id
					) l1 
					INNER JOIN senado.sf_despesa l on
					    l1.id_sf_senador = l.id_sf_senador and
					    l1.ano_mes = l.ano_mes and
					    l1.id_sf_despesa_tipo = l.id_sf_despesa_tipo and
					    l1.id <> l.id
					LEFT JOIN fornecedor.fornecedor pj ON pj.id = l.id_fornecedor
					LEFT JOIN fornecedor.fornecedor_info pji ON pji.id_fornecedor = pj.id
					order by l.valor desc
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
            var strSql = @"
				SELECT d.ano, d.mes, SUM(d.valor) AS valor_total
				from senado.sf_despesa d
				WHERE d.id_sf_senador = @id
				group by d.ano, d.mes
				order by d.ano, d.mes
			";

            return await GastosPorAno(id, strSql);
        }

        public async Task<GraficoBarraDTO> GastosComPessoalPorAno(int id)
        {
            // using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT d.ano, SUM(d.valor) AS valor_total
					FROM senado.sf_senador_verba_gabinete d
					WHERE d.id_sf_senador = @id
					group by d.ano
					order by d.ano
				");

                var categories = new List<int>();
                var series = new List<decimal>();

                using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString(), new { id }))
                {
                    while (await reader.ReadAsync())
                    {
                        categories.Add(Convert.ToInt32(reader["ano"]));
                        series.Add(Convert.ToDecimal(reader["valor_total"]));
                    }
                }

                return new GraficoBarraDTO
                {
                    Categories = categories,
                    Series = series
                };
            }
        }

        public async Task<List<ParlamentarCustoAnualDTO>> CustoAnual(int id)
        {
            var result = new List<ParlamentarCustoAnualDTO>();

            var indices = await _context.IndicesInflacao
                .OrderBy(i => i.Ano).ThenBy(i => i.Mes)
                .ToListAsync();
            var lastIndice = indices.LastOrDefault()?.Indice ?? 1;

            {
                var strSql = @"
					SELECT d.ano, d.mes, SUM(d.valor) AS valor_total
					from senado.sf_despesa d
					WHERE d.id_sf_senador = @id
					group by d.ano, d.mes
					order by d.ano, d.mes
				";

                using (DbDataReader reader = await ExecuteReaderAsync(strSql, new { id }))
                {
                    while (await reader.ReadAsync())
                    {
                        var ano = Convert.ToInt32(reader["ano"]);
                        var mes = Convert.ToInt16(reader["mes"]);
                        var valor = Convert.ToDecimal(reader["valor_total"]);

                        var dto = result.FirstOrDefault(x => x.Ano == ano);
                        if (dto == null)
                        {
                            dto = new ParlamentarCustoAnualDTO { Ano = ano };
                            result.Add(dto);
                        }
                        dto.CotaParlamentar += valor;

                        var indiceMes = indices.FirstOrDefault(i => i.Ano == (short)ano && i.Mes == (short)mes)?.Indice ?? 0;
                        if (indiceMes > 0)
                            dto.ValorTotalDeflacionado += valor * (lastIndice / indiceMes);
                        else
                            dto.ValorTotalDeflacionado += valor;
                    }
                }

                strSql = @"
					SELECT d.ano, d.mes, SUM(d.valor) AS valor_total
					FROM senado.sf_senador_verba_gabinete d
					WHERE d.id_sf_senador = @id
					group by d.ano, d.mes
					order by d.ano, d.mes
				";

                using (DbDataReader reader = await ExecuteReaderAsync(strSql, new { id }))
                {
                    while (await reader.ReadAsync())
                    {
                        var ano = Convert.ToInt32(reader["ano"]);
                        var mes = Convert.ToInt16(reader["mes"]);
                        var valor = Convert.ToDecimal(reader["valor_total"]);

                        var dto = result.FirstOrDefault(x => x.Ano == ano);

                        if (dto != null)
                        {
                            dto.VerbaGabinete += valor;
                        }
                        else
                        {
                            dto = new ParlamentarCustoAnualDTO
                            {
                                Ano = ano,
                                VerbaGabinete = valor
                            };
                            result.Add(dto);
                        }

                        var indiceMes = indices.FirstOrDefault(i => i.Ano == (short)ano && i.Mes == (short)mes)?.Indice ?? 0;
                        if (indiceMes > 0)
                            dto.ValorTotalDeflacionado += valor * (lastIndice / indiceMes);
                        else
                            dto.ValorTotalDeflacionado += valor;
                    }
                }
            }

            return result.OrderBy(x => x.Ano).ToList();
        }

        public async Task<List<DropDownDTO>> Pesquisa(MultiSelectRequest filtro = null)
        {
            // using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT DISTINCT
						d.id, d.nome, d.nome_completo, p.sigla as sigla_partido, e.sigla as sigla_estado 
					from senado.sf_senador d
                    LEFT JOIN partido p on p.id = d.id_partido
                    LEFT JOIN estado e on e.id = d.id_estado
				");

                if (filtro != null && string.IsNullOrEmpty(filtro.Ids))
                {
                    strSql.AppendLine(@"
                        WHERE (1=1) ");

                    if (!string.IsNullOrEmpty(filtro.Busca))
                    {
                        var busca = Utils.MySqlEscape(filtro.Busca);
                        strSql.AppendLine(@" AND (d.nome ILIKE '%" + busca + "%' or d.nome_completo ILIKE '%" + busca + "%') ");
                    }

                    if ((filtro.Periodo ?? 0) > 0)
                    {
                        strSql.AppendLine($" AND d.id IN(select m.id_sf_senador from senado.sf_mandato m JOIN senado.sf_mandato_legislatura ml on ml.id_sf_mandato = m.id and m.exerceu = true where ml.id_sf_legislatura = {filtro.Periodo.ToString()})");

                        strSql.AppendLine(@"
                            ORDER BY d.nome
                            limit 30
				        ");
                    }
                    else
                    {
                        var legislaturas = new List<int>();
                        if ((filtro.Ano ?? 0) > 0)
                            legislaturas = Utils.ObterNumerosLegislatura(filtro.Ano.Value, filtro.Mes);
                        else
                            legislaturas = Utils.ObterNumerosLegislatura(DateTime.Now.Year, DateTime.Now.Month);

                        var listaLegislaturas = string.Join(",", legislaturas);

                        strSql.AppendLine($" AND d.id IN(select m.id_sf_senador from senado.sf_mandato m JOIN senado.sf_mandato_legislatura ml on ml.id_sf_mandato = m.id and m.exerceu = true where ml.id_sf_legislatura IN ({listaLegislaturas}))");

                        strSql.AppendLine(@"
                            ORDER BY d.nome
                            limit 500
				        ");
                    }


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
                                from senado.sf_despesa
                            ) ");
                    }

                    strSql.AppendLine(@"
                        ORDER BY d.nome
				    ");
                }

                var lstRetorno = new List<DropDownDTO>();
                using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new DropDownDTO
                        {
                            Id = int.Parse(reader["id"].ToString()),
                            Tokens = new[] { reader["nome_completo"].ToString() },
                            Text = reader["nome"].ToString(),
                            HelpText = $"{reader["sigla_partido"]} / {reader["sigla_estado"]}"
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
	from senado.sf_despesa l
	WHERE (1=1)
	{sqlWhere}
	GROUP BY id_sf_senador
) l1
LEFT JOIN senado.sf_senador d on d.id = l1.id_sf_senador
LEFT JOIN partido p on p.id = d.id_partido
LEFT JOIN estado e on e.id = d.id_estado
{sqlSortAndPaging};

SELECT COUNT(DISTINCT id_sf_senador) 
FROM senado.sf_despesa l 
WHERE (1=1) 
{sqlWhere};";

            var lstRetorno = new List<LancamentoParlamentarDTO>();
            using (DbDataReader reader = await ExecuteReaderAsync(sql))
            {
                while (await reader.ReadAsync())
                {
                    lstRetorno.Add(new LancamentoParlamentarDTO
                    {
                        IdParlamentar = Convert.ToInt32(reader["id_sf_senador"]),
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
		, sum(l.valor) as valor_total
	from senado.sf_despesa l
	WHERE (1=1)
    {sqlWhere}
	GROUP BY l.id_fornecedor
) l1
LEFT JOIN fornecedor.fornecedor pj on pj.id = l1.id_fornecedor
{sqlSortAndPaging};

SELECT COUNT(DISTINCT id_fornecedor) 
FROM senado.sf_despesa l 
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
	l1.id_sf_despesa_tipo
	, td.descricao
	, l1.total_notas
	, l1.valor_total
FROM (
	SELECT 
		count(l.id) AS total_notas
		, sum(l.valor) as valor_total
		, l.id_sf_despesa_tipo
	from senado.sf_despesa l
	WHERE (1=1)
    {sqlWhere}
	GROUP BY id_sf_despesa_tipo
) l1
LEFT JOIN senado.sf_despesa_tipo td on td.id = l1.id_sf_despesa_tipo
{sqlSortAndPaging};

SELECT COUNT(DISTINCT id_sf_despesa_tipo) 
FROM senado.sf_despesa l 
WHERE (1=1) 
{sqlWhere};";


            var lstRetorno = new List<LancamentoDespesaDTO>();
            using (DbDataReader reader = await ExecuteReaderAsync(sql))
            {
                while (await reader.ReadAsync())
                {
                    lstRetorno.Add(new LancamentoDespesaDTO
                    {
                        IdDespesaTipo = Convert.ToInt32(reader["id_sf_despesa_tipo"]),
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
		, sum(l.valor) as valor_total
		, l.id_sf_senador
	from senado.sf_despesa l
	WHERE (1=1)
    {sqlWhere}
    GROUP BY id_sf_senador
) l1
INNER JOIN senado.sf_senador d on d.id = l1.id_sf_senador
LEFT JOIN partido p on p.id = d.id_partido
GROUP BY p.id, p.nome
{sqlSortAndPaging};

SELECT COUNT(DISTINCT d.id_partido) 
FROM senado.sf_despesa l 
INNER JOIN senado.sf_senador d on d.id = l.id_sf_senador
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
	COALESCE(e.id, 0) as id_estado
	, e.nome as nome_estado
	, sum(l1.total_notas) as total_notas
	, sum(l1.valor_total) as valor_total
FROM (
	SELECT
		count(l.id) AS total_notas
		, sum(l.valor) as valor_total
		, l.id_sf_senador
	from senado.sf_despesa l
	WHERE (1=1)
    {sqlWhere}
    GROUP BY id_sf_senador
) l1
JOIN senado.sf_senador d on d.id = l1.id_sf_senador
LEFT JOIN estado e on e.id = d.id_estado
GROUP BY e.id, e.nome
{sqlSortAndPaging};

SELECT COUNT(DISTINCT d.id_estado) 
FROM senado.sf_despesa l 
JOIN senado.sf_senador d on d.id = l.id_sf_senador
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
            var sqlOrderBy = $" ORDER BY {request.GetSorting("l.ano_mes DESC, l.data_emissao DESC, l.valor DESC")} ";
            var sqlLimit = $" LIMIT {request.Length} OFFSET {request.Start} ";

            var sql = $@"
SELECT
    l.data_emissao
    , l.valor as valor_total
    , l.id as id_sf_despesa
    , d.id as id_senador
    , pj.cnpj_cpf
    , l.id_fornecedor
    , pj.nome AS nome_fornecedor
    , d.nome as nome_parlamentar
    , e.sigla as sigla_estado
    , p.sigla as sigla_partido
    , t.descricao as despesa_tipo
FROM (
	SELECT data_emissao, valor, id, id_sf_senador, id_sf_despesa_tipo, id_fornecedor, l.ano_mes
	FROM senado.sf_despesa l
	WHERE (1=1)
    {sqlWhere}
    {sqlOrderBy}
    {sqlLimit}
) l
JOIN senado.sf_senador d on d.id = l.id_sf_senador
LEFT JOIN fornecedor.fornecedor pj on pj.id = l.id_fornecedor
LEFT JOIN partido p on p.id = d.id_partido
LEFT JOIN estado e on e.id = d.id_estado
LEFT JOIN senado.sf_despesa_tipo t on t.id = l.id_sf_despesa_tipo
{sqlOrderBy};

SELECT count(*) 
FROM senado.sf_despesa l 
WHERE (1=1) 
{sqlWhere}";

            var lstRetorno = new List<LancamentoDocumentoDTO>();
            using (DbDataReader reader = await ExecuteReaderAsync(sql))
            {
                while (await reader.ReadAsync())
                {
                    lstRetorno.Add(new LancamentoDocumentoDTO
                    {
                        IdDespesa = reader["id_sf_despesa"],
                        DataEmissao = Utils.FormataData(reader["data_emissao"]),
                        IdFornecedor = Convert.ToInt32(reader["id_fornecedor"]),
                        CnpjCpf = Utils.FormatCnpjCpf(reader["cnpj_cpf"].ToString()),
                        NomeFornecedor = reader["nome_fornecedor"].ToString(),
                        IdParlamentar = Convert.ToInt32(reader["id_senador"]),
                        NomeParlamentar = reader["nome_parlamentar"].ToString(),
                        SiglaEstado = reader["sigla_estado"].ToString(),
                        SiglaPartido = reader["sigla_partido"].ToString(),
                        DespesaTipo = reader["despesa_tipo"].ToString(),
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

        private void AdicionaFiltroEstadoSenador(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.ContainsKey("Estado") && !string.IsNullOrEmpty(request.Filters["Estado"].ToString()))
            {
                sqlSelect.AppendLine("	AND l.id_sf_senador IN (SELECT id from senado.sf_senador where id_estado IN(" + request.Filters["Estado"].ToString() + ")) ");
            }
        }

        private void AdicionaFiltroPartidoSenador(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.ContainsKey("Partido") && !string.IsNullOrEmpty(request.Filters["Partido"].ToString()))
            {
                sqlSelect.AppendLine("	AND l.id_sf_senador IN (SELECT id from senado.sf_senador where id_partido IN(" + request.Filters["Partido"].ToString() + ")) ");
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
                sqlSelect.AppendLine("	AND l.id_sf_despesa_tipo IN (" + Utils.MySqlEscapeNumberToIn(request.Filters["Despesa"].ToString()) + ") ");
            }
        }

        private void AdicionaFiltroSenador(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.ContainsKey("IdParlamentar") && !string.IsNullOrEmpty(request.Filters["IdParlamentar"].ToString()))
            {
                sqlSelect.AppendLine("	AND l.id_sf_senador IN (" + Utils.MySqlEscapeNumberToIn(request.Filters["IdParlamentar"].ToString()) + ") ");
            }
        }

        private string GetWhereFilter(DataTablesRequest request)
        {
            StringBuilder sql = new StringBuilder();

            AdicionaFiltroPeriodo(request, sql);
            AdicionaFiltroSenador(request, sql);
            AdicionaFiltroDespesa(request, sql);
            AdicionaFiltroFornecedor(request, sql);
            AdicionaFiltroPartidoSenador(request, sql);
            AdicionaFiltroEstadoSenador(request, sql);

            return sql.ToString();
        }

        private string GetSortAndPaging(DataTablesRequest request, string defaultSorting = "valor_total desc")
        {
            var sql = new StringBuilder();

            sql.AppendFormat(" ORDER BY {0} ", request.GetSorting(defaultSorting));
            sql.AppendFormat(" LIMIT {1} OFFSET {0} ", request.Start, request.Length);

            return sql.ToString();
        }

        public async Task<List<TipoDespesaDTO>> TipoDespesa()
        {
            // using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine("SELECT id, descricao from senado.sf_despesa_tipo ");
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

        public async Task<dynamic> SenadoResumoMensal()
        {
            // using (AppDb banco = new AppDb())
            {
                using (DbDataReader reader = await ExecuteReaderAsync(@"select ano, mes, valor from senado.sf_despesa_resumo_mensal"))
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
            var sql = new StringBuilder();
            sql.AppendLine($@"
					SELECT ano, mes, valor
					FROM senado.sf_despesa_resumo_mensal
                    WHERE ano > {DateTime.UtcNow.AddYears(-10).Year}
                    ORDER BY ano, mes
				");

            var indices = await _context.IndicesInflacao
                .OrderBy(i => i.Ano).ThenBy(i => i.Mes)
                .ToListAsync();

            var ultimoIndice = indices.LastOrDefault()?.Indice ?? 1;

            var gastosMensais = new List<(int Ano, int Mes, decimal Valor)>();

            using (DbDataReader reader = await ExecuteReaderAsync(sql.ToString()))
            {
                while (await reader.ReadAsync())
                {
                    gastosMensais.Add((
                        Convert.ToInt32(reader["ano"]),
                        Convert.ToInt32(reader["mes"]),
                        Convert.ToDecimal(reader["valor"])
                    ));
                }
            }

            var anos = gastosMensais.Select(g => g.Ano).Distinct().OrderBy(a => a).ToList();
            var valores = new List<dynamic>();
            var valoresDeflacionados = new List<dynamic>();

            foreach (var ano in anos)
            {
                var gastosDoAno = gastosMensais.Where(g => g.Ano == ano).ToList();
                decimal totalAnual = gastosDoAno.Sum(g => g.Valor);
                valores.Add(totalAnual);

                decimal totalDeflacionado = 0;
                foreach (var gasto in gastosDoAno)
                {
                    var indiceInflacao = indices.FirstOrDefault(i => i.Ano == gasto.Ano && i.Mes == gasto.Mes);
                    if (indiceInflacao != null)
                    {
                        totalDeflacionado += gasto.Valor * (ultimoIndice / indiceInflacao.Indice);
                    }
                    else
                    {
                        totalDeflacionado += gasto.Valor;
                    }
                }
                valoresDeflacionados.Add(totalDeflacionado);
            }

            return new
            {
                anos,
                valores,
                valores_deflacionados = valoresDeflacionados
            };
        }

        public async Task<dynamic> Remuneracao(DataTablesRequest request)
        {
            if (request == null) throw new BusinessException("Parâmetro request não informado!");
            string strSelectFiels, sqlGroupBy;

            AgrupamentoRemuneracaoSenado eAgrupamento;
            if (request.Filters?.TryGetValue("ag", out object agrupamento) ?? false)
                eAgrupamento = (AgrupamentoRemuneracaoSenado)Convert.ToInt32(agrupamento);
            else
                eAgrupamento = AgrupamentoRemuneracaoSenado.AnoMes;

            switch (eAgrupamento)
            {
                case AgrupamentoRemuneracaoSenado.Lotacao:
                    strSelectFiels = "l.id, l.descricao";
                    sqlGroupBy = "GROUP BY l.id, l.descricao";

                    break;
                case AgrupamentoRemuneracaoSenado.Cargo:
                    strSelectFiels = "cr.id, cr.descricao";
                    sqlGroupBy = "GROUP BY cr.id, cr.descricao";

                    break;
                case AgrupamentoRemuneracaoSenado.Categoria:
                    strSelectFiels = "ct.id, ct.descricao";
                    sqlGroupBy = "GROUP BY ct.id, ct.descricao";

                    break;
                case AgrupamentoRemuneracaoSenado.Vinculo:
                    strSelectFiels = "v.id, v.descricao";
                    sqlGroupBy = "GROUP BY v.id, v.descricao";

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
                    sqlGroupBy = "GROUP BY s.id, s.nome";

                    break;
                default:
                    throw new BusinessException("Parâmetro Agrupamento (ag) não informado!");
            }


            var sqlWhere = new StringBuilder();

            if (request.Filters != null)
            {
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
            }

            if (eAgrupamento == AgrupamentoRemuneracaoSenado.Senador)
            {
                sqlWhere.AppendLine("	AND s.id IS NOT NULL ");
            }

            // using (AppDb banco = new AppDb())
            {
                var sqlSelect = new StringBuilder();
                if (eAgrupamento != AgrupamentoRemuneracaoSenado.AnoMes)
                {
                    sqlSelect.AppendLine($@"
SELECT
	{strSelectFiels},
    COUNT(1) AS quantidade,
    SUM(r.custo_total) AS valor_total
from senado.sf_remuneracao r
JOIN senado.sf_vinculo v ON v.id = r.id_vinculo
JOIN senado.sf_categoria ct ON ct.id = r.id_categoria
LEFT JOIN senado.sf_cargo cr ON cr.id = r.id_cargo 
JOIN senado.sf_lotacao l ON l.id = r.id_lotacao
LEFT JOIN senado.sf_senador s on s.id = l.id_senador
WHERE (1=1)
");
                }
                else
                {

                    sqlSelect.AppendLine(@"
SELECT
    r.id,
	v.descricao as vinculo, 
	ct.descricao as categoria, 
	cr.descricao as cargo, 
	rc.descricao as referencia_cargo, 
	f.descricao as funcao, 
	l.descricao as lotacao, 
	tf.descricao as tipo_folha, 
	r.ano_mes, 
	r.custo_total as valor_total
from senado.sf_remuneracao r
JOIN senado.sf_lotacao l ON l.id = r.id_lotacao
JOIN senado.sf_vinculo v ON v.id = r.id_vinculo
JOIN senado.sf_categoria ct ON ct.id = r.id_categoria
JOIN senado.sf_tipo_folha tf ON tf.id = r.id_tipo_folha
LEFT JOIN senado.sf_cargo cr ON cr.id = r.id_cargo 
LEFT JOIN senado.sf_referencia_cargo rc ON rc.id = r.id_referencia_cargo
LEFT JOIN senado.sf_funcao f ON f.id = r.id_simbolo_funcao
LEFT JOIN senado.sf_senador s on s.id = l.id_senador
WHERE (1=1) 
");
                }

                sqlSelect.Append(sqlWhere);
                sqlSelect.Append(sqlGroupBy);

                var sqlToCount = sqlSelect.ToString();

                sqlSelect.AppendFormat(" ORDER BY {0} ", request.GetSorting(" valor_total desc"));
                sqlSelect.AppendFormat(" LIMIT {1} OFFSET {0}; ", request.Start, request.Length);
                sqlSelect.AppendLine($" SELECT COUNT(*) FROM ({sqlToCount}) AS t; ");

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await ExecuteReaderAsync(sqlSelect.ToString()))
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
                        recordsTotal = Convert.ToInt32(TotalCount),
                        recordsFiltered = Convert.ToInt32(TotalCount),
                        data = lstRetorno
                    };
                }
            }

        }

        public async Task<dynamic> Remuneracao(int id)
        {
            // using (AppDb banco = new AppDb())
            {
                var sqlSelect = new StringBuilder();

                sqlSelect.AppendLine(@"
SELECT
    r.id,
    r.admissao,
    r.ano_mes, 
	v.descricao as vinculo, 
	ct.descricao as categoria, 
	cr.descricao as cargo, 
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
    (r.rem_liquida + r.diarias + r.auxilios + r.vant_indenizatorias) AS total_liquido,
    r.custo_total
from senado.sf_remuneracao r
JOIN senado.sf_lotacao l ON l.id = r.id_lotacao
JOIN senado.sf_vinculo v ON v.id = r.id_vinculo
JOIN senado.sf_categoria ct ON ct.id = r.id_categoria
JOIN senado.sf_tipo_folha tf ON tf.id = r.id_tipo_folha
LEFT JOIN senado.sf_cargo cr ON cr.id = r.id_cargo 
LEFT JOIN senado.sf_referencia_cargo rc ON rc.id = r.id_referencia_cargo
LEFT JOIN senado.sf_funcao f ON f.id = r.id_simbolo_funcao
WHERE r.id = @id
");

                using (DbDataReader reader = await ExecuteReaderAsync(sqlSelect.ToString(), new { id }))
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
                            total_liquido = Utils.FormataValor(reader["total_liquido"]),
                            custo_total = Utils.FormataValor(reader["custo_total"])
                        };
                    }
                }
            }

            return null;
        }

        public async Task<dynamic> Lotacao()
        {
            // using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine("SELECT id, descricao from senado.sf_lotacao ");
                strSql.AppendFormat("ORDER BY descricao ");

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString()))
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
            // using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine("SELECT id, descricao from senado.sf_cargo ");
                strSql.AppendFormat("ORDER BY descricao ");

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString()))
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
            // using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine("SELECT id, descricao from senado.sf_categoria ");
                strSql.AppendFormat("ORDER BY descricao ");

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString()))
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
            // using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine("SELECT id, descricao from senado.sf_vinculo ");
                strSql.AppendFormat("ORDER BY descricao ");

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString()))
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