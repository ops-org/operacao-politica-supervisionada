using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OPS.Core.DTOs;
using OPS.Core.Enumerators;
using OPS.Core.Exceptions;
using OPS.Core.Utilities;
using AppDbContext = OPS.Infraestrutura.AppDbContext;

namespace OPS.Core.Repositories
{
    public class DeputadoRepository : BaseRepository, IParlamentarRepository
    {
        public DeputadoRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<ParlamentarDetalheDTO> Consultar(int id)
        {
            var deputado = await _context.DeputadosFederais
                .Include(d => d.Partido)
                .Include(d => d.Estado)
                .Include(d => d.EstadoNascimento)
                .Include(d => d.Gabinete)
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
                Condicao = deputado.Condicao,
                Situacao = deputado.Situacao,
                Sexo = deputado.Sexo,
                IdGabinete = deputado.IdGabinete,
                Predio = deputado.Gabinete?.Predio,
                Sala = deputado.Gabinete?.Sala,
                Andar = deputado.Gabinete?.Andar,
                Telefone = deputado.Gabinete?.Telefone,
                Email = deputado.Email,
                Profissao = deputado.Profissao,
                Escolaridade = deputado.Escolaridade,
                Nascimento = Utils.NascimentoFormatado(deputado.Nascimento),
                Falecimento = Utils.FormataData(deputado.Falecimento),
                SiglaEstadoNascimento = deputado.EstadoNascimento?.Sigla,
                NomeMunicipioNascimento = deputado.Municipio,
                ValorTotalCeap = Utils.FormataValor(deputado.ValorTotalCeap),
                SecretariosAtivos = deputado.SecretariosAtivos?.ToString() ?? "",
                ValorMensalSecretarios = Utils.FormataValor(deputado.ValorMensalSecretarios),
                ValorTotalRemuneracao = Utils.FormataValor(deputado.ValorTotalRemuneracao),
                ValorTotalSalario = Utils.FormataValor(deputado.ValorTotalSalario),
                ValorTotalAuxilioMoradia = Utils.FormataValor(deputado.ValorTotalAuxilioMoradia),
                ValorTotal = Utils.FormataValor(deputado.ValorTotalCeap + deputado.ValorTotalRemuneracao + deputado.ValorTotalSalario + deputado.ValorTotalAuxilioMoradia)
            };
        }

        public async Task<List<DeputadoFornecedorDTO>> MaioresFornecedores(int id)
        {
            var maioresFornecedores = await _context.DespesasCamaraFederal
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
            var maioresNotas = await _context.DespesasCamaraFederal
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
            //using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();

                strSql.AppendLine(@"
					SELECT
						l.id as id_despesa
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
						, td.id as id_despesa_tipo
						, td.descricao as descricao_despesa_tipo
						, d.id as id_parlamentar
						, d.id_deputado
						, d.nome_parlamentar
						, e.sigla as sigla_estado
						, p.sigla as sigla_partido
						, pj.id AS id_fornecedor
						, pj.cnpj_cpf
						, pj.nome AS nome_fornecedor
                        , l.tipo_link
					FROM camara.cf_despesa l
					LEFT JOIN fornecedor.fornecedor pj ON pj.id = l.id_fornecedor
					LEFT JOIN camara.cf_deputado d ON d.id = l.id_cf_deputado
					LEFT JOIN camara.cf_despesa_tipo td ON td.id = l.id_cf_despesa_tipo
					LEFT JOIN partido p on p.id = d.id_partido
					LEFT JOIN estado e on e.id = d.id_estado
	                LEFT JOIN trecho_viagem tv ON tv.id = l.id_trecho_viagem
					LEFT JOIN pessoa ps ON ps.id = l.id_passageiro
					WHERE l.id = @id
				 ");

                await using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString(), new { id }))
                {
                    if (await reader.ReadAsync())
                    {
                        string sTipoDocumento = "";
                        switch (Convert.ToInt32(reader["tipo_documento"]))
                        {
                            case 0: sTipoDocumento = "Nota Fiscal"; break;
                            case 1: sTipoDocumento = "Recibo"; break;
                            case 2: case 3: sTipoDocumento = "Despesa no Exterior"; break;
                        }
                        string cnpjCpf = Utils.FormatCnpjCpf(reader["cnpj_cpf"].ToString());
                        var ano = Convert.ToInt32(reader["ano"]);
                        var mes = Convert.ToInt16(reader["mes"]);
                        var emissao = Convert.ToDateTime(reader["data_emissao"].ToString());

                        var documento = new DocumentoDetalheDTO
                        {
                            IdDespesa = Convert.ToInt64(reader["id_despesa"]),
                            IdDocumento = reader["id_documento"] != DBNull.Value ? Convert.ToInt64(reader["id_documento"]) : (long?)null,
                            NumeroDocumento = reader["numero_documento"].ToString(),
                            TipoDocumento = sTipoDocumento,
                            DataEmissao = Utils.FormataData(emissao),
                            ValorDocumento = Utils.FormataValor(Convert.ToDecimal(reader["valor_documento"])),
                            ValorGlosa = Utils.FormataValor(Convert.ToDecimal(reader["valor_glosa"])),
                            ValorLiquido = Utils.FormataValor(Convert.ToDecimal(reader["valor_liquido"])),
                            ValorRestituicao = Utils.FormataValor(Convert.ToDecimal(reader["valor_restituicao"])),
                            NomePassageiro = reader["nome_passageiro"].ToString(), // TODO Mesclar com Favorecido
                            TrechoViagem = reader["trecho_viagem"].ToString(), // TODO Mesclar com observacao
                            Ano = ano,
                            Mes = mes,
                            Competencia = $"{mes:00}/{ano:0000}",
                            IdDespesaTipo = Convert.ToInt32(reader["id_despesa_tipo"]),
                            DescricaoDespesa = reader["descricao_despesa_tipo"].ToString(),
                            IdParlamentar = Convert.ToInt32(reader["id_parlamentar"]),
                            IdDeputado = reader["id_deputado"] != DBNull.Value ? Convert.ToInt32(reader["id_deputado"]) : (int?)null,
                            NomeParlamentar = reader["nome_parlamentar"].ToString(),
                            SiglaEstado = reader["sigla_estado"].ToString(),
                            SiglaPartido = reader["sigla_partido"].ToString(),
                            IdFornecedor = Convert.ToInt64(reader["id_fornecedor"]),
                            CnpjCpf = cnpjCpf,
                            NomeFornecedor = reader["nome_fornecedor"].ToString()
                        };

                        var urlCamara = "http://www.camara.leg.br/cota-parlamentar/";
                        var tipoLink = Convert.ToInt32(reader["tipo_link"]);

                        if (tipoLink == 2)// NF-e
                            documento.UrlDocumento = $"{urlCamara}nota-fiscal-eletronica?ideDocumentoFiscal={documento.IdDocumento}";
                        else if (tipoLink == 1)// Recibo
                            documento.UrlDocumento = $"{urlCamara}documentos/publ/{documento.IdDeputado}/{emissao.Year}/{documento.IdDocumento}.pdf";

                        documento.UrlDemaisDocumentosMes = $"{urlCamara}sumarizado?nuDeputadoId={documento.IdParlamentar}&dataInicio={documento.Competencia}&dataFim={documento.Competencia}&despesa={documento.IdDespesaTipo}&nomeHospede=&nomePassageiro=&nomeFornecedor=&cnpjFornecedor=&numDocumento=&sguf=";
                        documento.UrlDetalhesDocumento = $"{urlCamara}documento?nuDeputadoId={documento.IdParlamentar}&numMes={documento.Mes}&numAno=${documento.Ano}&despesa={documento.IdDespesaTipo}&cnpjFornecedor={documento.CnpjCpf}&idDocumento={documento.NumeroDocumento}";

                        return documento;
                    }
                }
                return null;
            }
        }

        public async Task<List<DocumentoRelacionadoDTO>> DocumentosDoMesmoDia(int id)
        {
            //using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();

                strSql.AppendLine(@"
					SELECT
						l.id as id_cf_despesa
						, pj.id as id_fornecedor
                        , pj.cnpj_cpf
						, pj.nome as nome_fornecedor
						, pji.estado as sigla_estado_fornecedor
						, l.valor_liquido
					FROM (
						select id, id_cf_deputado, id_fornecedor, data_emissao FROM camara.cf_despesa
						where id = @id
					) l1 
					INNER JOIN camara.cf_despesa l on
					l1.id_cf_deputado = l.id_cf_deputado and
					l1.data_emissao = l.data_emissao and
					l1.id <> l.id
					LEFT JOIN fornecedor.fornecedor pj ON pj.id = l.id_fornecedor
					LEFT JOIN fornecedor.fornecedor_info pji ON pji.id_fornecedor = pj.id
					order by l.valor_liquido desc
					limit 50
				 ");

                var lstRetorno = new List<DocumentoRelacionadoDTO>();

                using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString(), new { id }))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new DocumentoRelacionadoDTO()
                        {
                            IdDespesa = Convert.ToInt32(reader["id_cf_despesa"]),
                            IdFornecedor = Convert.ToInt32(reader["id_fornecedor"]),
                            CnpjCpf = Utils.FormatCnpjCpf(reader["cnpj_cpf"].ToString()),
                            NomeFornecedor = reader["nome_fornecedor"].ToString(),
                            SiglaEstadoFornecedor = reader["sigla_estado_fornecedor"].ToString(),
                            ValorLiquido = Utils.FormataValor(Convert.ToDecimal(reader["valor_liquido"]))
                        });
                    }
                }

                return lstRetorno;
            }
        }

        public async Task<List<DocumentoRelacionadoDTO>> DocumentosDaSubcotaMes(int id)
        {
            //using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();

                strSql.AppendLine(@"
					SELECT
						l.id as id_cf_despesa
						, pj.id as id_fornecedor
                        , pj.cnpj_cpf
						, pj.nome as nome_fornecedor
						, pji.estado as sigla_estado_fornecedor
						, l.valor_liquido
					FROM (
						select id, id_cf_deputado, id_fornecedor, id_cf_despesa_tipo, ano, mes 
                        FROM camara.cf_despesa
						where id = @id
					) l1 
					INNER JOIN camara.cf_despesa l on
					l1.id_cf_deputado = l.id_cf_deputado and
					l1.ano = l.ano and
					l1.mes = l.mes and
					l1.id_cf_despesa_tipo = l.id_cf_despesa_tipo and
					l1.id <> l.id
					LEFT JOIN fornecedor.fornecedor pj ON pj.id = l.id_fornecedor
					LEFT JOIN fornecedor.fornecedor_info pji ON pji.id_fornecedor = pj.id
					order by l.valor_liquido desc
					limit 50
				 ");

                var lstRetorno = new List<DocumentoRelacionadoDTO>();

                using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString(), new { id }))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new DocumentoRelacionadoDTO()
                        {
                            IdDespesa = Convert.ToInt32(reader["id_cf_despesa"]),
                            IdFornecedor = Convert.ToInt32(reader["id_fornecedor"]),
                            CnpjCpf = Utils.FormatCnpjCpf(reader["cnpj_cpf"].ToString()),
                            NomeFornecedor = reader["nome_fornecedor"].ToString(),
                            SiglaEstadoFornecedor = reader["sigla_estado_fornecedor"].ToString(),
                            ValorLiquido = Utils.FormataValor(Convert.ToDecimal(reader["valor_liquido"]))
                        });
                    }
                }

                return lstRetorno;
            }
        }

        public async Task<GraficoBarraDTO> GastosPorAno(int id)
        {
            //using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT d.ano, SUM(d.valor_liquido) AS valor_total
					FROM camara.cf_despesa d
					WHERE d.id_cf_deputado = @id
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

                //using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString()))
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

        public async Task<List<ParlamentarCustoAnualDTO>> CustoAnual(int id)
        {
            var result = new List<ParlamentarCustoAnualDTO>();
            //using (AppDb banco = new AppDb())
            {
                var strSql = @"
					SELECT d.ano, SUM(d.valor_liquido) AS valor_total
					FROM camara.cf_despesa d
					WHERE d.id_cf_deputado = @id
					group by d.ano
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
					SELECT d.ano, SUM(d.valor) AS valor_total
					FROM camara.cf_deputado_verba_gabinete d
					WHERE d.id_cf_deputado = @id
					group by d.ano
				";

                using (DbDataReader reader = await ExecuteReaderAsync(strSql, new { id }))
                {
                    while (await reader.ReadAsync())
                    {
                        var ano = Convert.ToInt32(reader["ano"]);
                        var dto = result.FirstOrDefault(x => x.Ano == ano);

                        if (dto != null)
                        {
                            dto.VerbaGabinete = Convert.ToDecimal(reader["valor_total"]);
                        }
                        else
                        {
                            dto = new ParlamentarCustoAnualDTO
                            {
                                Ano = ano,
                                VerbaGabinete = Convert.ToDecimal(reader["valor_total"])
                            };
                            result.Add(dto);
                        }
                    }
                }

                strSql = @"
					SELECT d.ano, SUM(d.valor) AS valor_total
					FROM camara.cf_deputado_remuneracao d
					WHERE d.id_cf_deputado = @id
					group by d.ano
				";

                using (DbDataReader reader = await ExecuteReaderAsync(strSql, new { id }))
                {
                    while (await reader.ReadAsync())
                    {
                        var ano = Convert.ToInt32(reader["ano"]);
                        var dto = result.FirstOrDefault(x => x.Ano == ano);

                        if (dto != null)
                        {
                            dto.SalarioPatronal = Convert.ToDecimal(reader["valor_total"]);
                        }
                        else
                        {
                            dto = new ParlamentarCustoAnualDTO
                            {
                                Ano = ano,
                                SalarioPatronal = Convert.ToDecimal(reader["valor_total"])
                            };
                            result.Add(dto);
                        }
                    }
                }

                strSql = @"
					SELECT d.ano, SUM(d.valor) AS valor_total
					FROM camara.cf_deputado_auxilio_moradia d
					WHERE d.id_cf_deputado = @id
					group by d.ano
				";

                using (DbDataReader reader = await ExecuteReaderAsync(strSql, new { id }))
                {
                    while (await reader.ReadAsync())
                    {
                        var ano = Convert.ToInt32(reader["ano"]);
                        var dto = result.FirstOrDefault(x => x.Ano == ano);

                        if (dto != null)
                        {
                            dto.AuxilioMoradia = Convert.ToDecimal(reader["valor_total"]);
                        }
                        else
                        {
                            dto = new ParlamentarCustoAnualDTO
                            {
                                Ano = ano,
                                AuxilioMoradia = Convert.ToDecimal(reader["valor_total"])
                            };
                            result.Add(dto);
                        }
                    }
                }
            }
            return result.OrderBy(x => x.Ano).ToList();
        }

        public async Task<dynamic> GastosComPessoalPorAno(int id)
        {
            //using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT d.ano, SUM(d.valor) AS valor_total
					FROM camara.cf_deputado_verba_gabinete d
					WHERE d.id_cf_deputado = @id
					group by d.ano
					order by d.ano
				");

                var categories = new List<dynamic>();
                var series = new List<dynamic>();

                using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString(), new { id }))
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

        public async Task<List<ParlamentarListaDTO>> Lista(FiltroParlamentarDTO request)
        {
            //using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
                    SELECT 
						d.id as id_cf_deputado
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
					FROM camara.cf_deputado d
					LEFT JOIN partido p on p.id = d.id_partido
					LEFT JOIN estado e on e.id = d.id_estado
                    WHERE 1=1");

                if (request.Periodo > 50)
                {
                    strSql.AppendLine($" AND d.id IN(select m.id_cf_deputado FROM camara.cf_mandato m where m.id_legislatura = {request.Periodo.ToString()})");
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
                            IdParlamentar = Convert.ToInt32(reader["id_cf_deputado"]),
                            NomeParlamentar = reader["nome_parlamentar"].ToString(),
                            NomeCivil = reader["nome_civil"].ToString(),
                            ValorTotalCeap = Utils.FormataValor(reader["valor_total_ceap"]),
                            ValorTotalRemuneracao = Utils.FormataValor(reader["valor_total_remuneracao"]),
                            SiglaPartido = !string.IsNullOrEmpty(reader["sigla_partido"].ToString()) ? reader["sigla_partido"].ToString() : "S.PART.",
                            NomePartido = !string.IsNullOrEmpty(reader["nome_partido"].ToString()) ? reader["nome_partido"].ToString() : "<Sem Partido>",
                            SiglaEstado = reader["sigla_estado"].ToString(),
                            NomeEstado = reader["nome_estado"].ToString(),
                            Ativo = reader["situacao"].ToString() == "Exercício",
                        });
                    }
                }
                return lstRetorno;
            }
        }

        public async Task<List<DropDownDTO>> Pesquisa(MultiSelectRequest filtro = null)
        {
            //using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT DISTINCT
						d.id, d.nome_civil, d.nome_parlamentar, p.sigla as sigla_partido, e.sigla as sigla_estado 
					FROM camara.cf_deputado d
                    LEFT JOIN partido p on p.id = d.id_partido
                    LEFT JOIN estado e on e.id = d.id_estado
				");

                if (filtro != null && string.IsNullOrEmpty(filtro.Ids))
                {
                    strSql.AppendLine(@"
                        LEFT JOIN camara.cf_mandato m on m.id_cf_deputado = d.id
                        WHERE d.id IS NOT NULL ");

                    if (!string.IsNullOrEmpty(filtro.Busca))
                    {
                        var busca = Utils.MySqlEscape(filtro.Busca);
                        strSql.AppendLine(@" AND (d.nome_parlamentar ILIKE '%" + busca + "%' or d.nome_civil ILIKE '%" + busca + "%') ");
                    }

                    if (filtro.Periodo > 50)
                    {
                        var legislaturas = filtro.Periodo > 100 ? string.Join(",", Utils.ObterNumerosLegislatura(filtro.Periodo.Value)) : filtro.Periodo.ToString();
                        strSql.AppendLine($" AND (d.id < 100 OR m.id_legislatura IN({legislaturas}))");
                    }

                    strSql.AppendLine(@" ORDER BY d.nome_parlamentar");

                    if ((filtro.Periodo ?? 0) == 0)
                        strSql.AppendLine(@" limit 30");
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
                            Id = int.Parse(reader["id"].ToString()),
                            Tokens = new[] { reader["nome_civil"].ToString() },
                            Text = reader["nome_parlamentar"].ToString(),
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
	d.id as id_cf_deputado
    , d.nome_parlamentar
    , e.sigla as sigla_estado
    , p.sigla as sigla_partido
    , l1.total_notas
    , l1.valor_total
FROM (
    SELECT 
	    count(l.id) AS total_notas
	    , sum(l.valor_liquido) as valor_total
	    , l.id_cf_deputado
    FROM camara.cf_despesa l 
    WHERE (1=1)
    {sqlWhere}
    GROUP BY id_cf_deputado
) l1
JOIN camara.cf_deputado d on d.id = l1.id_cf_deputado
LEFT JOIN partido p on p.id = d.id_partido
LEFT JOIN estado e on e.id = d.id_estado
{sqlSortAndPaging};

SELECT COUNT(DISTINCT id_cf_deputado) 
FROM camara.cf_despesa l 
WHERE (1=1) 
{sqlWhere};";

            var lstRetorno = new List<LancamentoParlamentarDTO>();
            using (DbDataReader reader = await ExecuteReaderAsync(sql))
            {
                while (await reader.ReadAsync())
                {
                    lstRetorno.Add(new LancamentoParlamentarDTO
                    {
                        IdParlamentar = Convert.ToInt32(reader["id_cf_deputado"]),
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
    FROM camara.cf_despesa l 
    WHERE (1=1)
    {sqlWhere}
    GROUP BY l.id_fornecedor
) l1
LEFT JOIN fornecedor.fornecedor pj on pj.id = l1.id_fornecedor
{sqlSortAndPaging};

SELECT COUNT(DISTINCT id_fornecedor) 
FROM camara.cf_despesa l 
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
	l1.id_cf_despesa_tipo
	, td.descricao
	, l1.total_notas
	, l1.valor_total
FROM (
    SELECT
	    count(l.id) AS total_notas
	    , sum(l.valor_liquido) as valor_total
	    , l.id_cf_despesa_tipo
    FROM camara.cf_despesa l 
    WHERE (1=1)
    {sqlWhere}
    GROUP BY id_cf_despesa_tipo
) l1
LEFT JOIN camara.cf_despesa_tipo td on td.id = l1.id_cf_despesa_tipo
{sqlSortAndPaging};

SELECT COUNT(DISTINCT id_cf_despesa_tipo) 
FROM camara.cf_despesa l 
WHERE (1=1) 
{sqlWhere};";

            var lstRetorno = new List<LancamentoDespesaDTO>();
            using (DbDataReader reader = await ExecuteReaderAsync(sql))
            {
                while (await reader.ReadAsync())
                {
                    lstRetorno.Add(new LancamentoDespesaDTO
                    {
                        IdDespesaTipo = Convert.ToInt32(reader["id_cf_despesa_tipo"]),
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
    , coalesce(p.nome, '<Sem Partido>') as nome_partido
    , sum(l1.total_notas) as total_notas
    , sum(l1.valor_total) as valor_total
FROM (
	SELECT
		count(l.id) AS total_notas
	    , sum(l.valor_liquido) as valor_total
	    , l.id_cf_deputado
	FROM camara.cf_despesa l 
	WHERE (1=1)
    {sqlWhere}
    GROUP BY id_cf_deputado
) l1
INNER JOIN camara.cf_deputado d on d.id = l1.id_cf_deputado
LEFT JOIN partido p on p.id = d.id_partido
GROUP BY p.id, p.nome
{sqlSortAndPaging};

SELECT COUNT(DISTINCT d.id_partido) 
FROM camara.cf_despesa l 
INNER JOIN camara.cf_deputado d on d.id = l.id_cf_deputado
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
    coalesce(e.id, 99) AS id_estado
    , coalesce(e.nome, '<Sem Estado / Lideranças de Partido>') as nome_estado
    , sum(l1.total_notas) as total_notas
    , sum(l1.valor_total) as valor_total
from (
    SELECT
        count(l.id) AS total_notas
        , sum(l.valor_liquido) as valor_total
        , l.id_cf_deputado
    FROM camara.cf_despesa l 
    WHERE (1=1)
    {sqlWhere}
    GROUP BY id_cf_deputado
) l1
JOIN camara.cf_deputado d on d.id = l1.id_cf_deputado
LEFT JOIN estado e on e.id = d.id_estado
GROUP BY e.id, e.nome
{sqlSortAndPaging};

SELECT COUNT(DISTINCT d.id_estado) 
FROM camara.cf_despesa l 
INNER JOIN camara.cf_deputado d on d.id = l.id_cf_deputado
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
            var sqlSortAndPaging = GetSortAndPaging(request, "l.ano DESC, l.mes DESC, l.data_emissao DESC, l.valor_liquido DESC");

            var sql = $@"
SELECT
	l.data_emissao
	, pj.nome AS nome_fornecedor
	, d.nome_parlamentar
	, l.valor_total
	, l.id as id_cf_despesa
	, e.sigla as sigla_estado
	, p.sigla as sigla_partido
	, l.id_fornecedor
	, pj.cnpj_cpf
	, d.id as id_cf_deputado
	, t.descricao as despesa_tipo
FROM (
	SELECT data_emissao, id, id_cf_deputado, valor_liquido as valor_total, id_cf_despesa_tipo, id_fornecedor
	FROM camara.cf_despesa l 
	WHERE (1=1)
    {sqlWhere}
    {sqlSortAndPaging}
) l
INNER JOIN camara.cf_deputado d on d.id = l.id_cf_deputado
LEFT JOIN fornecedor.fornecedor pj on pj.id = l.id_fornecedor
LEFT JOIN partido p on p.id = d.id_partido
LEFT JOIN estado e on e.id = d.id_estado
LEFT JOIN camara.cf_despesa_tipo t on t.id = l.id_cf_despesa_tipo;

SELECT COUNT(*) 
FROM camara.cf_despesa l 
WHERE (1=1)
{sqlWhere};";

            var lstRetorno = new List<LancamentoDocumentoDTO>();
            using (DbDataReader reader = await ExecuteReaderAsync(sql))
            {
                while (await reader.ReadAsync())
                {
                    lstRetorno.Add(new LancamentoDocumentoDTO
                    {
                        IdDespesa = reader["id_cf_despesa"],
                        DataEmissao = Utils.FormataData(reader["data_emissao"]),
                        IdFornecedor = Convert.ToInt32(reader["id_fornecedor"]),
                        CnpjCpf = Utils.FormatCnpjCpf(reader["cnpj_cpf"].ToString()),
                        NomeFornecedor = reader["nome_fornecedor"].ToString(),
                        IdParlamentar = Convert.ToInt32(reader["id_cf_deputado"]),
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
            int periodo = Convert.ToInt32(request.Filters["Periodo"].ToString());
            //int legislatura = 57;
            //if (periodo > 50)
            //    legislatura = periodo;

            //sqlSelect = sqlSelect.Replace("##LEG##", legislatura.ToString());

            DateTime dataIni = DateTime.Today;
            DateTime dataFim = DateTime.Today;
            switch (periodo)
            {
                case 1: //PERIODO_MES_ATUAL
                    sqlSelect.AppendLine(" AND l.ano_mes = " + dataIni.ToString("yyyyMM"));
                    break;

                case 2: //PERIODO_MES_ANTERIOR
                    dataIni = dataIni.AddMonths(-1);
                    sqlSelect.AppendLine(" AND l.ano_mes = " + dataIni.ToString("yyyyMM"));
                    break;

                case 3: //PERIODO_MES_ULT_4
                    dataIni = dataIni.AddMonths(-3);
                    sqlSelect.AppendLine(" AND l.ano_mes >= " + dataIni.ToString("yyyyMM"));
                    break;

                case 4: //PERIODO_ANO_ATUAL
                    dataIni = new DateTime(dataIni.Year, 1, 1);
                    sqlSelect.AppendLine(" AND l.ano_mes >= " + dataIni.ToString("yyyyMM"));
                    break;

                case 5: //PERIODO_ANO_ANTERIOR
                    dataIni = new DateTime(dataIni.Year, 1, 1).AddYears(-1);
                    dataFim = new DateTime(dataIni.Year, 12, 31);
                    sqlSelect.AppendFormat(" AND l.ano_mes BETWEEN {0} AND {1}", dataIni.ToString("yyMM"), dataFim.ToString("yyMM"));
                    break;

                case 57: //PERIODO_MANDATO_57
                    sqlSelect.AppendLine(" AND l.ano_mes BETWEEN 202302 AND 202701");
                    break;

                case 56: //PERIODO_MANDATO_56
                    sqlSelect.AppendLine(" AND l.ano_mes BETWEEN 201902 AND 202301");
                    break;

                case 55: //PERIODO_MANDATO_55
                    sqlSelect.AppendLine(" AND l.ano_mes BETWEEN 201502 AND 201901");
                    break;

                case 54: //PERIODO_MANDATO_54
                    sqlSelect.AppendLine(" AND l.ano_mes BETWEEN 201102 AND 201501");
                    break;

                case 53: //PERIODO_MANDATO_53
                    sqlSelect.AppendLine(" AND l.ano_mes BETWEEN 200702 AND 201101");
                    break;

                    //case "0": //Customizado
                    //    if (request.Filters.ContainsKey("PeriodoCustom") && !string.IsNullOrEmpty(request.Filters["PeriodoCustom"].ToString()))
                    //    {
                    //        var periodo = request.Filters["PeriodoCustom"].ToString().Split('-');

                    //        if (periodo[0].Length == 6 && periodo[1].Length == 6)
                    //        {
                    //            sqlSelect.AppendLine(string.Format(" AND l.ano_mes BETWEEN {0} AND {1}", periodo[0], periodo[1]));
                    //        }
                    //        else if (periodo[0].Length == 6)
                    //        {
                    //            sqlSelect.AppendLine(string.Format(" AND l.ano_mes >= {0}", periodo[0]));
                    //        }
                    //        else if (periodo[1].Length == 6)
                    //        {
                    //            sqlSelect.AppendLine(string.Format(" AND l.ano_mes <= {0}", periodo[1]));
                    //        }
                    //    }
                    //    break;
            }
        }

        private void AdicionaFiltroPartidoDeputado(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.ContainsKey("Partido") && !string.IsNullOrEmpty(request.Filters["Partido"].ToString()))
            {
                sqlSelect.AppendLine("	AND l.id_cf_deputado IN (SELECT id FROM camara.cf_deputado where id_partido IN(" + request.Filters["Partido"].ToString() + ")) ");
            }
        }

        private void AdicionaFiltroEstadoDeputado(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.TryGetValue("Estado", out object value) && !string.IsNullOrEmpty(value.ToString()))
            {
                sqlSelect.Append("	AND l.id_cf_deputado IN (SELECT id FROM camara.cf_deputado where id_estado IN(" + Utils.MySqlEscapeNumberToIn(value.ToString()) + ")");
                if (value.ToString().Contains("99", StringComparison.InvariantCultureIgnoreCase))
                    sqlSelect.Append("OR id_estado IS NULL");

                sqlSelect.AppendLine(") ");
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
                        // //using (AppDb banco = new AppDb())
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
                sqlSelect.AppendLine("	AND l.id_cf_despesa_tipo IN (" + Utils.MySqlEscapeNumberToIn(request.Filters["Despesa"].ToString()) + ") ");
            }
        }

        private void AdicionaFiltroDeputado(DataTablesRequest request, StringBuilder sqlSelect)
        {
            if (request.Filters.ContainsKey("IdParlamentar") && !string.IsNullOrEmpty(request.Filters["IdParlamentar"].ToString()))
            {
                sqlSelect.AppendLine("	AND l.id_cf_deputado IN (" + Utils.MySqlEscapeNumberToIn(request.Filters["IdParlamentar"].ToString()) + ") ");
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
            //AdicionaFiltroDocumento(request, sql);

            return sql.ToString();
        }

        private string GetSortAndPaging(DataTablesRequest request, string defaultSorting = "valor_total desc")
        {
            var sql = new StringBuilder();

            sql.AppendFormat(" ORDER BY {0} ", request.GetSorting("valor_total desc"));
            sql.AppendFormat(" LIMIT {1} OFFSET {0}", request.Start, request.Length);

            return sql.ToString();
        }

        public async Task<List<TipoDespesaDTO>> TipoDespesa()
        {
            //using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine("SELECT id, descricao FROM camara.cf_despesa_tipo ");
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

        public async Task<dynamic> FuncionarioPesquisa(MultiSelectRequest filtro = null)
        {
            //using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT DISTINCT
						s.id, s.nome
					FROM camara.cf_funcionario s
				");

                if (filtro != null && string.IsNullOrEmpty(filtro.Ids))
                {
                    strSql.AppendLine(@"
                        WHERE (1=1) ");

                    if (!string.IsNullOrEmpty(filtro.Busca))
                    {
                        var busca = Utils.MySqlEscape(filtro.Busca);
                        strSql.AppendLine(@" AND (s.nome ILIKE '%" + busca + "%') ");
                    }

                    strSql.AppendLine(@"
                        ORDER BY s.nome
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
                        strSql.AppendLine(@" AND s.id IN(" + Ids + ") ");
                    }

                    strSql.AppendLine(@"
                        ORDER BY s.nome
				    ");
                }

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id = reader["id"].ToString(),
                            text = reader["nome"].ToString()
                        });
                    }
                }
                return lstRetorno;
            }
        }

        public async Task<dynamic> Funcionarios(DataTablesRequest request)
        {
            var dcFielsSort = new Dictionary<int, string>(){
                {1, "p.nome_parlamentar" },
                {2, "p.secretarios_ativos" },
                {3, "p.valor_mensal_secretarios" },
                {4, "p.custo_total_secretarios" },
            };

            //using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT
						p.id as id_cf_deputado
						, p.nome_parlamentar
						, p.secretarios_ativos
						, p.valor_mensal_secretarios
					FROM camara.cf_deputado p
					where p.quantidade_secretarios > 0
				");

                //if (!string.IsNullOrEmpty(request.NomeParlamentar))
                //{
                //    strSql.AppendFormat("and p.nome_parlamentar ILIKE '%{0}%' ", Utils.MySqlEscape(request.NomeParlamentar));
                //}

                strSql.AppendFormat(" ORDER BY {0} ", request.GetSorting(dcFielsSort, "p.nome_parlamentar"));
                strSql.AppendFormat(" LIMIT {1} OFFSET {0}; ", request.Start, request.Length);

                strSql.AppendLine("SELECT FOUND_ROWS() as row_count; ");

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_parlamentar = reader["id_cf_deputado"],
                            nome_parlamentar = reader["nome_parlamentar"].ToString(),
                            quantidade_secretarios = reader["quantidade_secretarios"].ToString(),
                            custo_total_secretarios = Utils.FormataValor(reader["valor_mensal_secretarios"]) // TODO: Seria o valor mensal ou total?
                        });
                    }

                    await reader.NextResultAsync();
                    await reader.ReadAsync();
                    string TotalCount = reader["row_count"].ToString();

                    return new
                    {
                        recordsTotal = Convert.ToInt32(TotalCount),
                        recordsFiltered = Convert.ToInt32(TotalCount),
                        data = lstRetorno
                    };
                }
            }
        }

        public async Task<dynamic> FuncionariosAtivosPorDeputado(int id, DataTablesRequest request)
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

            //using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
SELECT DISTINCT
	co.id_cf_funcionario
	, s.nome
	, ca.nome as cargo
	, gf.nome AS grupo_funcional
	, r.valor_bruto
	, r.valor_liquido
	, r.valor_outros
	, r.valor_total
    , r.referencia
	, s.chave
FROM camara.cf_funcionario s
JOIN camara.cf_funcionario_contratacao co ON co.id_cf_funcionario = s.id
left JOIN camara.cf_funcionario_remuneracao r ON r.id_cf_funcionario = s.id -- AND r.id_cf_deputado = co.id_cf_deputado
JOIN camara.cf_funcionario_cargo ca ON ca.id = co.id_cf_funcionario_cargo
JOIN camara.cf_funcionario_grupo_funcional gf ON gf.id = co.id_cf_funcionario_grupo_funcional
WHERE co.id_cf_deputado = @id
AND r.referencia = '2021-07-01'
AND ca.nome = r.cargo
AND co.periodo_ate IS null
				");

                //if (request.Filters.ContainsKey("ativo"))
                //{
                //    banco.AddParameter("@ativo", Convert.ToInt32(request.Filters["ativo"].ToString()));
                //    strSql.Append("and s.em_exercicio = @ativo");
                //}

                strSql.AppendFormat(" ORDER BY {0} ", Utils.MySqlEscape(request.GetSorting(dcFielsSort, "s.nome")));
                strSql.AppendFormat(" LIMIT {1} OFFSET {0}; ", request.Start, request.Length);

                strSql.AppendLine("SELECT FOUND_ROWS() as row_count; ");

                using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString(), new { id }))
                {
                    var lstRetorno = new List<dynamic>();
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id_cf_funcionario = Convert.ToInt32(reader["id_cf_funcionario"]),
                            nome = reader["nome"].ToString(),
                            cargo = reader["cargo"].ToString(),
                            grupo_funcional = reader["grupo_funcional"].ToString(),
                            valor_bruto = Utils.FormataValor(reader["valor_bruto"]),
                            valor_liquido = Utils.FormataValor(reader["valor_liquido"]),
                            valor_outros = Utils.FormataValor(reader["valor_outros"]),
                            custo_total = Utils.FormataValor(reader["valor_total"]),
                            referencia = Convert.ToDateTime(reader["referencia"]).ToString("yyyy-MM"),
                            chave = reader["chave"].ToString()
                        });
                    }

                    await reader.NextResultAsync();
                    await reader.ReadAsync();
                    string TotalCount = reader["row_count"].ToString();

                    return new
                    {
                        recordsTotal = Convert.ToInt32(TotalCount),
                        recordsFiltered = Convert.ToInt32(TotalCount),
                        data = lstRetorno
                    };
                }
            }
        }

        public async Task<dynamic> FuncionariosPorDeputado(int id, DataTablesRequest request)
        {
            var dcFielsSort = new Dictionary<int, string>(){
                {1, "s.nome" },
                {2, "custo_total" }
            };

            //using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
					SELECT
	                    s.nome
	                    , SUM(r.valor_outros + r.valor_bruto) as custo_total
	                    , s.link
                    FROM camara.cf_funcionario_remuneracao r
                    left join (
	                    select distinct id_cf_deputado, nome, link
	                    FROM camara.cf_funcionario s
                    ) s on s.link = r.id_cf_funcionario
                    WHERE s.id_cf_deputado = @id
                    group by s.link, s.nome
				");

                strSql.AppendFormat(" ORDER BY {0} ", Utils.MySqlEscape(request.GetSorting(dcFielsSort, "s.nome")));
                strSql.AppendFormat(" LIMIT {1} OFFSET {0}; ", request.Start, request.Length);

                strSql.AppendLine("SELECT FOUND_ROWS() as row_count; ");

                using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString(), new { id }))
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

                    await reader.NextResultAsync();
                    await reader.ReadAsync();
                    string TotalCount = reader["row_count"].ToString();

                    return new
                    {
                        recordsTotal = Convert.ToInt32(TotalCount),
                        recordsFiltered = Convert.ToInt32(TotalCount),
                        data = lstRetorno
                    };
                }
            }
        }

        public async Task<dynamic> ResumoPresenca(int id)
        {
            //using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine(@"
SELECT 
    EXTRACT(YEAR FROM s.data) as ano
    , sum(CASE WHEN sp.presente THEN 1 ELSE 0 END) as presenca
    , sum(CASE WHEN NOT sp.presente AND sp.justificativa = '' THEN 1 ELSE 0 END) as ausencia
    , sum(CASE WHEN NOT sp.presente AND sp.justificativa <> '' THEN 1 ELSE 0 END) as ausencia_justificada
FROM camara.cf_sessao_presenca sp
INNER JOIN camara.cf_sessao s ON s.id = sp.id_cf_sessao
WHERE sp.id_cf_deputado = @id
GROUP BY sp.id_cf_deputado, EXTRACT(YEAR FROM s.data)
ORDER BY EXTRACT(YEAR FROM s.data)
				");

                var categories = new List<dynamic>();
                var series = new List<dynamic>();

                var presencas = new List<dynamic>();
                var ausencias = new List<dynamic>();
                var ausencias_justificadas = new List<dynamic>();

                int presenca_total = 0;
                int ausencia_total = 0;
                int ausencia_justificada_total = 0;

                using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString(), new { id }))
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
            //using (AppDb banco = new AppDb())
            {
                using (DbDataReader reader = await ExecuteReaderAsync(@"select ano, mes, valor FROM camara.cf_despesa_resumo_mensal"))
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
            //using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine($@"
					select ano, sum(valor) as valor
					FROM camara.cf_despesa_resumo_mensal sf
                    WHERE ano > {DateTime.UtcNow.AddYears(-10).Year}
					GROUP BY ano
                    ORDER BY ano
				");

                var categories = new List<dynamic>();
                var series = new List<dynamic>();

                using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString()))
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

            //using (AppDb banco = new AppDb())
            {
                var sqlSelect = new StringBuilder();

                sqlSelect.AppendLine(@"
					SELECT 
						s.id as id_cf_sessao
                        , s.numero
						, s.inicio
						, s.tipo
						, s.presencas
						, s.ausencias
						, s.ausencias_justificadas
                        , (s.presencas + s.ausencias + s.ausencias_justificadas) as participantes
					FROM camara.cf_sessao s
					WHERE (1=1)
				");

                sqlSelect.Append(sqlWhere);

                sqlSelect.AppendFormat(" ORDER BY {0} ", Utils.MySqlEscape(request.GetSorting("s.inicio desc")));
                sqlSelect.AppendFormat(" LIMIT {1} OFFSET {0}; ", request.Start, request.Length);

                sqlSelect.AppendLine(
                    @"SELECT COUNT(*) FROM camara.cf_sessao s WHERE (1=1) ");

                sqlSelect.Append(sqlWhere);

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await ExecuteReaderAsync(sqlSelect.ToString()))
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

                        var presenca = Convert.ToInt32(reader["presencas"]);
                        var ausencia = Convert.ToInt32(reader["ausencias"]);
                        var ausencia_justificada = Convert.ToInt32(reader["ausencias_justificadas"]);
                        var total = Convert.ToInt32(reader["participantes"]);

                        lstRetorno.Add(new
                        {
                            id_cf_sessao = reader["id_cf_sessao"],
                            inicio = Utils.FormataDataHora(reader["inicio"]),
                            tipo = sTipo,
                            numero = reader["numero"].ToString(),
                            presenca,
                            presenca_percentual = presenca > 0 ? Utils.FormataValor((decimal)presenca * 100 / total) : "",
                            ausencia,
                            ausencia_percentual = ausencia > 0 ? Utils.FormataValor((decimal)ausencia * 100 / total) : "",
                            ausencia_justificada,
                            ausencia_justificada_percentual = ausencia_justificada > 0 ? Utils.FormataValor((decimal)ausencia_justificada * 100 / total) : "",
                            total
                        });
                    }

                    var TotalCount = reader.GetTotalRowsFound();
                    return new
                    {
                        recordsTotal = TotalCount,
                        recordsFiltered = TotalCount,
                        data = lstRetorno
                    };
                }
            }
        }

        public async Task<dynamic> Frequencia(int id, DataTablesRequest request)
        {
            var dcFielsSort = new Dictionary<int, string>(){
                {0, "d.nome_parlamentar" },
                {1, "sp.presente, sp.justificativa" },
                {2, "sp.justificativa" },
                {3, "sp.presenca_externa" },
            };

            //using (AppDb banco = new AppDb())
            {
                var sqlSelect = new StringBuilder();

                sqlSelect.AppendFormat(@"
					SELECT
						sp.id_cf_deputado
						, d.nome_parlamentar
						, sp.presente
						, sp.justificativa
						, sp.presenca_externa
					FROM camara.cf_sessao_presenca sp
					INNER JOIN camara.cf_deputado d on d.id = sp.id_cf_deputado
					WHERE sp.id_cf_sessao = {0}
				", id);

                sqlSelect.AppendFormat(" ORDER BY {0} ", Utils.MySqlEscape(request.GetSorting(dcFielsSort, "d.nome_parlamentar asc")));
                sqlSelect.AppendFormat(" LIMIT {1} OFFSET {0}; ", request.Start, request.Length);

                sqlSelect.AppendLine(
                    @"SELECT FOUND_ROWS() as row_count;");

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await ExecuteReaderAsync(sqlSelect.ToString(), new { id }))
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
                            id_parlamentar = reader["id_cf_deputado"],
                            nome_parlamentar = reader["nome_parlamentar"].ToString(),
                            id_presenca = nPresenca,
                            presenca = sPresenca,
                            justificativa = sJustificativa
                        });
                    }

                    await reader.NextResultAsync();
                    await reader.ReadAsync();
                    string TotalCount = reader["row_count"].ToString();

                    return new
                    {
                        recordsTotal = Convert.ToInt32(TotalCount),
                        recordsFiltered = Convert.ToInt32(TotalCount),
                        data = lstRetorno
                    };
                }
            }
        }

        public async Task<dynamic> GrupoFuncional()
        {
            //using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine("SELECT id, nome FROM camara.cf_funcionario_grupo_funcional ");
                strSql.AppendFormat("ORDER BY nome ");

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id = reader["id"].ToString(),
                            text = reader["nome"].ToString(),
                        });
                    }
                }
                return lstRetorno;
            }
        }

        public async Task<dynamic> Cargo()
        {
            //using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.AppendLine("SELECT id, nome FROM camara.cf_funcionario_cargo ");
                strSql.AppendFormat("ORDER BY nome ");

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString()))
                {
                    while (await reader.ReadAsync())
                    {
                        lstRetorno.Add(new
                        {
                            id = reader["id"].ToString(),
                            text = reader["nome"].ToString(),
                        });
                    }
                }
                return lstRetorno;
            }
        }

        public async Task<dynamic> Remuneracao(DataTablesRequest request)
        {
            if (request == null) throw new BusinessException("Parâmetro request não informado!");
            string strSelectFiels, sqlGroupBy;

            AgrupamentoRemuneracaoCamara eAgrupamento;
            if (request.Filters.TryGetValue("ag", out object agrupamento))
                eAgrupamento = (AgrupamentoRemuneracaoCamara)Convert.ToInt32(agrupamento);
            else
                eAgrupamento = AgrupamentoRemuneracaoCamara.AnoMes;

            switch (eAgrupamento)
            {
                case AgrupamentoRemuneracaoCamara.GrupoFuncional:
                    strSelectFiels = "gf.id, gf.nome";
                    sqlGroupBy = "GROUP BY gf.id, gf.nome";

                    break;
                case AgrupamentoRemuneracaoCamara.Cargo:
                    strSelectFiels = "ca.id, ca.nome";
                    sqlGroupBy = "GROUP BY ca.id, ca.nome";

                    break;
                case AgrupamentoRemuneracaoCamara.Deputado:
                    strSelectFiels = "d.id, d.nome_parlamentar as nome";
                    sqlGroupBy = "GROUP BY d.id, d.nome_parlamentar";

                    break;
                case AgrupamentoRemuneracaoCamara.Funcionario:
                    strSelectFiels = "s.id, s.nome";
                    sqlGroupBy = "GROUP BY s.id, s.nome";

                    break;
                case AgrupamentoRemuneracaoCamara.Ano:
                    strSelectFiels = "EXTRACT(YEAR FROM r.referencia) as id, EXTRACT(YEAR FROM r.referencia) as nome";
                    sqlGroupBy = "GROUP BY EXTRACT(YEAR FROM r.referencia)";

                    break;
                case AgrupamentoRemuneracaoCamara.AnoMes:
                    strSelectFiels = "";
                    sqlGroupBy = "";

                    break;
                default:
                    throw new BusinessException("Parâmetro Agrupamento (ag) não informado!");
            }


            var sqlWhere = new StringBuilder();

            if (request.Filters.ContainsKey("gf") && !string.IsNullOrEmpty(request.Filters["gf"].ToString()))
            {
                sqlWhere.AppendLine("	AND co.id_cf_funcionario_grupo_funcional IN(" + Utils.MySqlEscapeNumberToIn(request.Filters["gf"].ToString()) + ") ");
            }
            if (request.Filters.ContainsKey("cr") && !string.IsNullOrEmpty(request.Filters["cr"].ToString()))
            {
                sqlWhere.AppendLine("	AND co.id_cf_funcionario_cargo IN(" + Utils.MySqlEscapeNumberToIn(request.Filters["cr"].ToString()) + ") ");
            }
            if (request.Filters.ContainsKey("df") && !string.IsNullOrEmpty(request.Filters["df"].ToString()))
            {
                sqlWhere.AppendLine("	AND co.id_cf_deputado IN(" + Utils.MySqlEscapeNumberToIn(request.Filters["df"].ToString()) + ") ");
            }
            if (request.Filters.ContainsKey("sc") && !string.IsNullOrEmpty(request.Filters["sc"].ToString()))
            {
                sqlWhere.AppendLine("	AND co.id_cf_funcionario IN(" + Utils.MySqlEscapeNumberToIn(request.Filters["sc"].ToString()) + ") ");
            }
            if (request.Filters.ContainsKey("ms") && !string.IsNullOrEmpty(request.Filters["ms"].ToString()))
            {
                sqlWhere.AppendLine("	AND r.referencia = '" + Convert.ToInt32(request.Filters["an"].ToString()).ToString() + "-" + Convert.ToInt32(request.Filters["ms"].ToString()).ToString("d2") + "-01' ");
            }
            else if (request.Filters.ContainsKey("an") && !string.IsNullOrEmpty(request.Filters["an"].ToString()))
            {
                sqlWhere.AppendLine("	AND EXTRACT(YEAR FROM r.referencia) BETWEEN " + Convert.ToInt32(request.Filters["an"].ToString()).ToString() + " AND " + Convert.ToInt32(request.Filters["an"].ToString()) + " ");
            }

            if (eAgrupamento == AgrupamentoRemuneracaoCamara.Deputado)
            {
                sqlWhere.AppendLine("	AND co.id_cf_deputado IS NOT NULL ");
            }

            //using (AppDb banco = new AppDb())
            {
                var sqlSelect = new StringBuilder();
                if (eAgrupamento != AgrupamentoRemuneracaoCamara.AnoMes)
                {
                    sqlSelect.AppendLine($@"
SELECT
	{strSelectFiels},
    COUNT(1) AS quantidade,
    SUM(r.valor_total) AS valor_total
FROM camara.cf_funcionario s
LEFT JOIN camara.cf_funcionario_contratacao co ON co.id_cf_funcionario = s.id
JOIN camara.cf_funcionario_remuneracao r ON co.id = r.id_cf_funcionario_contratacao
LEFT JOIN camara.cf_funcionario_cargo ca ON ca.id = co.id_cf_funcionario_cargo
JOIN camara.cf_funcionario_grupo_funcional gf ON gf.id = co.id_cf_funcionario_grupo_funcional
LEFT JOIN camara.cf_deputado d ON d.id = co.id_cf_deputado
WHERE (1=1)
");
                }
                else
                {

                    sqlSelect.AppendLine(@"
SELECT
    d.nome_parlamentar as deputado
    , s.nome as funcionario
	, r.referencia
    , r.valor_bruto
    , r.valor_outros
	, r.valor_total
    , r.id
    , ca.nome as cargo
	, gf.nome AS grupo_funcional
	, tf.nome as tipo_folha
FROM camara.cf_funcionario s
JOIN camara.cf_funcionario_contratacao co ON co.id_cf_funcionario = s.id
JOIN camara.cf_funcionario_remuneracao r ON co.id = r.id_cf_funcionario_contratacao
LEFT JOIN camara.cf_funcionario_cargo ca ON ca.id = co.id_cf_funcionario_cargo
LEFT JOIN camara.cf_funcionario_grupo_funcional gf ON gf.id = co.id_cf_funcionario_grupo_funcional
LEFT JOIN camara.cf_funcionario_tipo_folha tf on tf.id = r.id_cf_funcionario_tipo_folha
LEFT JOIN camara.cf_deputado d ON d.id = co.id_cf_deputado
WHERE (1=1) 
");
                }

                sqlSelect.Append(sqlWhere);
                sqlSelect.Append(sqlGroupBy);

                var sqlToCount = sqlSelect.ToString();

                sqlSelect.AppendFormat(" ORDER BY {0} ", request.GetSorting(" valor_total desc"));
                sqlSelect.AppendFormat(" LIMIT {1} OFFSET {0}; ", request.Start, request.Length);
                sqlSelect.Append($" SELECT COUNT(1) FROM ({sqlToCount}); ");

                var lstRetorno = new List<dynamic>();
                using (DbDataReader reader = await ExecuteReaderAsync(sqlSelect.ToString()))
                {
                    if (eAgrupamento != AgrupamentoRemuneracaoCamara.AnoMes)
                    {
                        while (await reader.ReadAsync())
                        {
                            lstRetorno.Add(new
                            {
                                id = Convert.IsDBNull(reader["id"]) ? (int?)null : Convert.ToInt32(reader["id"]),
                                descricao = reader["nome"].ToString(),
                                quantidade = Utils.FormataValor(reader["quantidade"], 0),
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
                                id = Convert.ToInt32(reader["id"]),
                                funcionario = reader["funcionario"].ToString(),
                                deputado = reader["deputado"].ToString(),
                                grupo_funcional = reader["grupo_funcional"].ToString(),
                                cargo = reader["cargo"].ToString(),
                                tipo_folha = reader["tipo_folha"].ToString(),
                                ano_mes = Convert.ToDateTime(reader["referencia"].ToString()).ToString("yyyy-MM"),
                                valor_bruto = Utils.FormataValor(reader["valor_bruto"]),
                                valor_outros = Utils.FormataValor(reader["valor_outros"]),
                                valor_total = Utils.FormataValor(reader["valor_total"])
                            });
                        }
                    }

                    int TotalCount = reader.GetTotalRowsFound();
                    return new
                    {
                        recordsTotal = TotalCount,
                        recordsFiltered = TotalCount,
                        data = lstRetorno
                    };
                }
            }

        }

        public async Task<dynamic> Remuneracao(int id)
        {
            //using (AppDb banco = new AppDb())
            {
                var sqlSelect = new StringBuilder();

                sqlSelect.AppendLine(@"
SELECT
    d.id as id_cf_deputado
    , s.chave
    , s.nome as secretario
    , d.nome_parlamentar as deputado
    , r.referencia
	, ca.nome as cargo
	, gf.nome as grupo_funcional
	, tf.nome as tipo_folha
    , r.remuneracao_fixa as remun_basica
    , r.vantagens_natureza_pessoal as vant_pessoais
    , r.funcao_ou_cargo_em_comissao as func_comissionada
    , r.gratificacao_natalina as grat_natalina
    , r.ferias
    , r.outras_remuneracoes as outras_eventuais
    , r.abono_permanencia as abono_permanencia
    , r.redutor_constitucional as reversao_teto_const
    , r.contribuicao_previdenciaria as previdencia
    , r.imposto_renda as imposto_renda
    , r.valor_liquido as rem_liquida
    , r.valor_diarias as diarias
    , r.valor_auxilios as auxilios
    , r.valor_vantagens as vant_indenizatorias
    , r.valor_outros
    , r.valor_total
FROM camara.cf_funcionario s
LEFT JOIN camara.cf_funcionario_contratacao co ON co.id_cf_funcionario = s.id
JOIN camara.cf_funcionario_remuneracao r ON co.id = r.id_cf_funcionario_contratacao
LEFT JOIN camara.cf_funcionario_cargo ca ON ca.id = co.id_cf_funcionario_cargo
JOIN camara.cf_funcionario_grupo_funcional gf ON gf.id = co.id_cf_funcionario_grupo_funcional
JOIN camara.cf_funcionario_tipo_folha tf on tf.id = r.id_cf_funcionario_tipo_folha
LEFT JOIN camara.cf_deputado d ON d.id = co.id_cf_deputado
WHERE r.id = @id
");

                using (DbDataReader reader = await ExecuteReaderAsync(sqlSelect.ToString(), new { id }))
                {
                    if (await reader.ReadAsync())
                    {
                        return new
                        {
                            chave = reader["chave"].ToString(),
                            id_cf_deputado = !Convert.IsDBNull(reader["id_cf_deputado"]) ? Convert.ToInt32(reader["id_cf_deputado"]) : (int?)null,
                            deputado = reader["deputado"].ToString(),
                            secretario = reader["secretario"].ToString(),
                            cargo = reader["cargo"].ToString(),
                            grupo_funcional = reader["grupo_funcional"].ToString(),
                            tipo_folha = reader["tipo_folha"].ToString(),
                            ano_mes = Convert.ToDateTime(reader["referencia"].ToString()).ToString("yyyy-MM"),
                            remun_basica = Utils.FormataValor(reader["remun_basica"]),
                            vant_pessoais = Utils.FormataValor(reader["vant_pessoais"]),
                            func_comissionada = Utils.FormataValor(reader["func_comissionada"]),
                            grat_natalina = Utils.FormataValor(reader["grat_natalina"]),
                            ferias = Utils.FormataValor(reader["ferias"]),
                            outras_eventuais = Utils.FormataValor(reader["outras_eventuais"]),
                            abono_permanencia = Utils.FormataValor(reader["abono_permanencia"]),
                            reversao_teto_const = Utils.FormataValor(reader["reversao_teto_const"]),
                            imposto_renda = Utils.FormataValor(reader["imposto_renda"]),
                            previdencia = Utils.FormataValor(reader["previdencia"]),
                            rem_liquida = Utils.FormataValor(reader["rem_liquida"]),
                            diarias = Utils.FormataValor(reader["diarias"]),
                            auxilios = Utils.FormataValor(reader["auxilios"]),
                            vant_indenizatorias = Utils.FormataValor(reader["vant_indenizatorias"]),
                            valor_outros = Utils.FormataValor(reader["valor_outros"]),
                            custo_total = Utils.FormataValor(reader["valor_total"])
                        };
                    }
                }
            }

            return null;
        }
    }
}