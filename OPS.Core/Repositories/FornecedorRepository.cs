using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OPS.Core.Utilities;
using OPS.Infraestrutura;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace OPS.Core.Repositories
{
    public class FornecedorRepository : BaseRepository
    {
        public FornecedorRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<dynamic> Consulta(int id)
        {
            // using (AppDb banco = new AppDb())
            {
                using (var reader = await ExecuteReaderAsync(
                        @"SELECT 
							pj.id as id_fornecedor
							, pj.cnpj_cpf
                            , pj.categoria
							, pji.tipo
							, coalesce(pji.nome, pj.nome) as nome
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
							, pji.origem
							, pji.capital_social
						FROM fornecedor.fornecedor pj
						LEFT JOIN fornecedor.fornecedor_info pji on pji.id_fornecedor = pj.id
						LEFT JOIN fornecedor.fornecedor_atividade a on a.id = pji.id_fornecedor_atividade_principal
						LEFT JOIN fornecedor.fornecedor_natureza_juridica nj on nj.id = pji.id_fornecedor_natureza_juridica
						WHERE pj.id = @id;

                        SELECT fa.codigo, fa.descricao
                        FROM fornecedor.fornecedor_atividade_secundaria fas 
                        INNER JOIN fornecedor.fornecedor_atividade fa on fa.id = fas.id_fornecedor_atividade
                        where id_fornecedor = @id;"
                    , new { id }))
                {
                    if (await reader.ReadAsync())
                    {
                        var fornecedor = new
                        {
                            id_fornecedor = reader["id_fornecedor"].ToString(),
                            cnpj_cpf = Utils.FormatCnpjCpf(reader["cnpj_cpf"].ToString()),
                            data_de_abertura = Utils.FormataData(reader["data_de_abertura"]),
                            categoria = reader["categoria"].ToString(),
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
                            capital_social = Utils.FormataValor(reader["capital_social"]),
                            doador = reader["doador"],
                            obtido_em = Utils.FormataData(reader["obtido_em"]),
                            Origem = reader["origem"].ToString(),
                            atividade_secundaria = new List<string>()
                        };

                        await reader.NextResultAsync();
                        while (await reader.ReadAsync())
                        {
                            fornecedor.atividade_secundaria.Add($"{reader["codigo"]} - {reader["descricao"]}");
                        }

                        return fornecedor;
                    }

                    return null;
                }
            }
        }

        public async Task<dynamic> QuadroSocietario(int id)
        {
            var quadroSocietario = await _context.FornecedorSocios
                .Include(fs => fs.FornecedorSocioQualificacao)
                .Include(fs => fs.FornecedorSocioRepresentanteQualificacao)
                .Include(fs => fs.FornecedorFaixaEtaria)
                .Where(fs => fs.IdFornecedor == id)
                .Select(fs => new
                {
                    fs.Nome,
                    fs.CnpjCpf,
                    fs.PaisOrigem,
                    fs.DataEntradaSociedade,
                    FaixaEtaria = fs.FornecedorFaixaEtaria != null ? fs.FornecedorFaixaEtaria.Nome : null,
                    Qualificacao = fs.FornecedorSocioQualificacao != null ? fs.FornecedorSocioQualificacao.Descricao : null,
                    fs.NomeRepresentante,
                    fs.CpfRepresentante,
                    QualificacaoRepresentante = fs.FornecedorSocioRepresentanteQualificacao != null ? fs.FornecedorSocioRepresentanteQualificacao.Descricao : null
                })
                .ToListAsync();

            var quadroSocietarioParsed = quadroSocietario.Select(fs => new
            {
                fs.Nome,
                CnpjCpf = Utils.FormatCnpjCpf(fs.CnpjCpf),
                fs.PaisOrigem,
                DataEntradaSociedade = Utils.FormataData(fs.DataEntradaSociedade),
                fs.FaixaEtaria,
                fs.Qualificacao,
                fs.NomeRepresentante,
                CpfRepresentante = Utils.FormatCnpjCpf(fs.CpfRepresentante),
                fs.QualificacaoRepresentante
            })
            .ToList();

            return quadroSocietarioParsed;
        }

        public async Task<dynamic> SenadoresMaioresGastos(int id)
        {
            // using (AppDb banco = new AppDb())
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
						from senado.sf_despesa l
						WHERE l.id_fornecedor = @id
						GROUP BY l.id_sf_senador
						ORDER BY valor_total desc
						LIMIT 10
					) l1
					LEFT JOIN senado.sf_senador p ON p.id = l1.id_sf_senador";

                using (var reader = await ExecuteReaderAsync(strSql, new { id }))
                {
                    List<dynamic> lstRetorno = new List<dynamic>();
                    while (await reader.ReadAsync())
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

        public async Task<dynamic> MaioresGastos(int id)
        {
            // using (AppDb banco = new AppDb())
            {
                var strSql = new StringBuilder();
                strSql.Append(@"
SELECT 
    tmp.id, 
    tmp.tipo, 
    tmp.nome_parlamentar, 
    pr.sigla as sigla_partido, 
    e.sigla as sigla_estado, 
    tmp.ultima_emissao,
    tmp.valor_total
FROM (
	SELECT
		l1.id_cf_deputado AS id
		, p.nome_parlamentar
		, p.id_partido
		, p.id_estado
		, 'Deputado Federal' as tipo
        , l1.ultima_emissao
		, l1.valor_total
	FROM (
		select 
            MAX(l.data_emissao) AS ultima_emissao,
			SUM(l.valor_liquido) AS valor_total
			, l.id_cf_deputado
		FROM camara.cf_despesa l
		WHERE l.id_fornecedor = @id
		GROUP BY l.id_cf_deputado
		ORDER BY valor_total desc
		LIMIT 10
	) l1
	JOIN camara.cf_deputado p ON p.id = l1.id_cf_deputado
	

	UNION ALL

    SELECT
		l1.id_cl_deputado AS id
		, p.nome_parlamentar
		, p.id_partido
		, p.id_estado
		, 'Deputado Estadual' as tipo
        , l1.ultima_emissao
		, l1.valor_total
	FROM (
		select 
            MAX(l.data_emissao) AS ultima_emissao,
			SUM(l.valor_liquido) AS valor_total
			, l.id_cl_deputado
		from assembleias.cl_despesa l
		WHERE l.id_fornecedor = @id
		GROUP BY l.id_cl_deputado
		ORDER BY valor_total desc
		LIMIT 10
	) l1
	JOIN  assembleias.cl_deputado p ON p.id = l1.id_cl_deputado

	UNION ALL

	SELECT
		l1.id_sf_senador AS id
		, p.nome as nome_parlamentar
		, p.id_partido
		, p.id_estado
		, 'Senador' as tipo
        , l1.ultima_emissao
		, l1.valor_total
	FROM (
		select 
            MAX(l.data_emissao) AS ultima_emissao,
			SUM(l.valor) AS valor_total
			, l.id_sf_senador
		from senado.sf_despesa l
		WHERE l.id_fornecedor = @id
		GROUP BY l.id_sf_senador
		ORDER BY valor_total desc
		LIMIT 10
	) l1
	JOIN senado.sf_senador p ON p.id = l1.id_sf_senador
) tmp
LEFT JOIN partido pr on pr.id = tmp.id_partido
LEFT JOIN estado e on e.id = tmp.id_estado
ORDER BY valor_total desc
LIMIT 10 ");

                using (var reader = await ExecuteReaderAsync(strSql.ToString(), new { id }))
                {
                    List<dynamic> lstRetorno = new List<dynamic>();
                    while (await reader.ReadAsync())
                    {
                        string link_parlamentar = "", link_despesas = "";
                        var tipo = reader["tipo"].ToString();
                        var id_parlamentar = Convert.ToInt32(reader["id"]);

                        if (tipo == "Deputado Federal")
                        {
                            // Deputado Federal
                            link_parlamentar = $"/#/deputado-federal/{id_parlamentar}";
                            link_despesas = $"/#/deputado-federal?IdParlamentar={id_parlamentar}&Fornecedor={id}&Periodo=0&Agrupamento=6";

                        }
                        else if (tipo == "Deputado Estadual")
                        {
                            // Deputado Estadual
                            link_parlamentar = $"/#/deputado-estadual/{id_parlamentar}";
                            link_despesas = $"/#/deputado-estadual?IdParlamentar={id_parlamentar}&Fornecedor={id}&Periodo=0&Agrupamento=6";

                        }
                        else
                        {
                            // Senador
                            link_parlamentar = $"/#/senador/{id_parlamentar}";
                            link_despesas = $"/#/senador?IdParlamentar={id_parlamentar}&Fornecedor={id}&Periodo=0&Agrupamento=6";
                        }

                        lstRetorno.Add(new
                        {
                            id = id_parlamentar,
                            tipo,
                            nome_parlamentar = reader["nome_parlamentar"].ToString(),
                            sigla_partido = reader["sigla_partido"].ToString(),
                            sigla_estado = reader["sigla_estado"].ToString(),
                            ultima_emissao = Utils.FormataData(reader["ultima_emissao"]),
                            valor_total = Utils.FormataValor(reader["valor_total"]),
                            link_parlamentar,
                            link_despesas
                        });
                    }

                    return lstRetorno;
                }
            }
        }

        public async Task<dynamic> RecebimentosPorAno(int id)
        {
            // using (AppDb banco = new AppDb())
            {
                string strSql = @"
SELECT ano, SUM(valor) AS valor
FROM (
    SELECT l.ano, SUM(l.valor_liquido) AS valor
    FROM camara.cf_despesa l
    WHERE l.id_fornecedor = @id
    group by l.ano

    UNION ALL

    SELECT l.ano_mes/100 as ano , SUM(l.valor_liquido) AS valor
    FROM  assembleias.cl_despesa l
    WHERE l.id_fornecedor = @id
    group by ano

    UNION ALL

    SELECT l.ano, SUM(l.valor) AS valor
    from senado.sf_despesa l
    WHERE l.id_fornecedor = @id
    group by l.ano
) tmp
group by ano
order by ano
				";

                var categories = new List<dynamic>();
                var series = new List<dynamic>();

                using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString(), new { id }))
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
        //			// using (AppDb banco = new AppDb())
        //			{
        //				string strSql = @"
        //SELECT l.mes, l.ano, SUM(l.valor_liquido) AS valor_total
        //FROM camara.cf_despesa l
        //WHERE l.id_fornecedor = @id
        //group by l.ano, l.mes


        //UNION ALL

        //SELECT l.mes, l.ano, SUM(l.valor) AS valor_total
        //from senado.sf_despesa l
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


        public dynamic Consulta(string cnpj, string nome)
        {
            var query = _context.Fornecedores.AsQueryable();

            if (!string.IsNullOrEmpty(cnpj) && cnpj.Replace(".", "", StringComparison.Ordinal).Replace("/", "", StringComparison.Ordinal).Replace("-", "", StringComparison.Ordinal).All(char.IsDigit))
            {
                cnpj = Utils.RemoveCaracteresNaoNumericos(cnpj);

                if (cnpj.Length == 14 || cnpj.Length == 11)
                    query = query.Where(f => f.CnpjCpf == cnpj);
                else
                    query = query.Where(f => f.CnpjCpf.Contains(cnpj));
            }
            else
            {
                if (!string.IsNullOrEmpty(nome))
                    query = query.Where(f => EF.Functions.ILike(f.Nome, $"%{nome}%"));
            }

            var fornecedores = query
                .OrderBy(f => f.Nome)
                .Take(100)
                .Select(f => new
                {
                    id_fornecedor = f.Id.ToString(),
                    cnpj_cpf = Utils.FormatCnpjCpf(f.CnpjCpf),
                    nome = f.Nome
                })
                .ToList();

            return fornecedores;
        }

        public async Task<List<dynamic>> Busca(string value)
        {
            var strSql = new StringBuilder();
            strSql.AppendLine(@"
					SELECT 
						f.id as id_fornecedor
						, f.cnpj_cpf as cnpj
						, coalesce(f.nome, fi.nome) as nome
						, fi.nome_fantasia
                        , fi.estado
                        , (f.valor_total_ceap_camara + f.valor_total_ceap_senado + f.valor_total_ceap_assembleias) as valor_total_ceap
					FROM fornecedor.fornecedor f
                    left join fornecedor.fornecedor_info fi on fi.id_fornecedor = f.id
                    WHERE 1=1");

            if (!string.IsNullOrEmpty(value))
            {
                if (value.Replace(".", "", StringComparison.Ordinal).Replace("/", "", StringComparison.Ordinal).Replace("-", "", StringComparison.Ordinal).All(char.IsDigit))
                {
                    var cnpj = Utils.RemoveCaracteresNaoNumericos(value);
                    strSql.AppendLine("	AND f.cnpj_cpf LIKE '%" + cnpj + "%'");
                }
                else
                {
                    strSql.AppendLine("	AND (unaccent(f.nome) ilike unaccent('%" + Utils.MySqlEscape(value) + "%') " +
                        "or unaccent(fi.nome) ilike unaccent('%" + Utils.MySqlEscape(value) + "%') " +
                        "or unaccent(fi.nome_fantasia) ilike unaccent('%" + Utils.MySqlEscape(value) + "%'))");
                }
            }

            strSql.AppendLine(@"
                    ORDER BY lower(coalesce(f.nome, fi.nome)), f.cnpj_cpf, valor_total_ceap
                    limit 100
				");

            var lstRetorno = new List<dynamic>();
            using (DbDataReader reader = await ExecuteReaderAsync(strSql.ToString()))
            {
                while (await reader.ReadAsync())
                {
                    lstRetorno.Add(new
                    {
                        id_fornecedor = reader["id_fornecedor"],
                        cnpj = Utils.FormatCnpjCpf(reader["cnpj"].ToString()),
                        nome = reader["nome"].ToString(),
                        nome_fantasia = reader["nome_fantasia"].ToString(),
                        estado = reader["estado"].ToString(),
                        valor_total_ceap = Utils.FormataValor(reader["valor_total_ceap"])
                    });
                }
            }
            return lstRetorno;
        }
    }
}
