using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OPS.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace OPS.Importador
{
    /// <summary>
    /// Assembleia Legislativa do Estado de Santa Catarina
    /// https://transparencia.alesc.sc.gov.br/
    /// </summary>
    public class CamaraSantaCatarina : ImportadorCotaParlamentarBase
    {
        public CamaraSantaCatarina(ILogger<CamaraSantaCatarina> logger, IConfiguration configuration) : base("Câmara Legislativa do Santa Catarina", logger, configuration)
        {
        }

        public override Dictionary<string, string> DefinirOrigemDestino(int ano)
        {
            Dictionary<string, string> arquivos = new();

            // https://transparencia.alesc.sc.gov.br/gabinetes_dados_abertos.php
            // Arquivos disponiveis anualmente a partir de 2011
            var _urlOrigem = $"http://transparencia.alesc.sc.gov.br/gabinetes_csv.php?ano={ano}";
            var _caminhoArquivo = $"{tempPath}/CLSC-{ano}.csv";

            arquivos.Add(_urlOrigem, _caminhoArquivo);
            return arquivos;
        }

        protected override void ProcessarDespesas(string caminhoArquivo, int ano)
        {
            int indice = 0;
            int Verba = indice++;
            int Descricao = indice++;
            int Conta = indice++;
            int Favorecido = indice++;
            int Trecho = indice++;
            int Vencimento = indice++;
            int Valor = indice++;

            using (var banco = new AppDb())
            {
                var dc = new Dictionary<string, UInt32>();
                using (var dReader = banco.ExecuteReader(
                    $"select d.id, d.hash from cl_despesa d join cl_deputado p on d.id_cl_deputado = p.id where p.id_estado = 42 and d.ano_mes between {ano}01 and {ano}12"))
                    while (dReader.Read())
                    {
                        var hex = Convert.ToHexString((byte[])dReader["hash"]);
                        if (!dc.ContainsKey(hex))
                            dc.Add(hex, (UInt32)dReader["id"]);
                    }

                using (var reader = new StreamReader(caminhoArquivo, Encoding.GetEncoding("ISO-8859-1")))
                {
                    short count = 0;

                    using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR")))
                    {

                        while (csv.Read())
                        {
                            count++;

                            if (count == 1)
                            {

                                if (
                                    (csv[Verba] != "Verba") ||
                                    (csv[Descricao] != "Descrição") ||
                                    (csv[Conta] != "Conta") ||
                                    (csv[Favorecido] != "Favorecido") ||
                                    (csv[Trecho] != "Trecho") ||
                                    (csv[Vencimento] != "Vencimento") ||
                                    (csv[Valor] != "Valor")
                                )
                                {
                                    throw new Exception("Mudança de integração detectada para o Câmara Legislativa do Distrito Federal");
                                }

                                // Pular linha de titulo
                                continue;
                            }

                            if (string.IsNullOrEmpty(csv[Verba])) continue; //Linha vazia


                            banco.AddParameter("verba", csv[Verba]);
                            banco.AddParameter("descricao", csv[Descricao]);
                            banco.AddParameter("conta", csv[Conta]);
                            banco.AddParameter("favorecido", csv[Favorecido]);
                            banco.AddParameter("trecho", csv[Trecho]);
                            banco.AddParameter("vencimento", DateTime.Parse(csv[Vencimento]));

                            string valorTmp = csv[Valor];
                            // Valor 1.500.00 é na verdade 1.500,00
                            Regex myRegex = new Regex(@"\.(\d\d$)", RegexOptions.Singleline);
                            if (myRegex.IsMatch(valorTmp))
                            {
                                valorTmp = myRegex.Replace(valorTmp, @",$1");
                            }

                            banco.AddParameter("valor", !string.IsNullOrEmpty(valorTmp) ? (object)Convert.ToDouble(valorTmp) : 0);
                            banco.AddParameter("Ano", ano);

                            byte[] hash = banco.ParametersHash();
                            var key = Convert.ToHexString(hash);
                            if (dc.Remove(key))
                            {
                                banco.ClearParameters();
                                continue;
                            }

                            banco.AddParameter("hash", hash);

                            banco.ExecuteNonQuery(
                                @"INSERT INTO tmp.cl_despesa_temp (
								    tipo_verba, tipo_despesa, nome, favorecido, observacao, data_emissao, valor, ano, hash
							    ) VALUES (
								    @verba, @descricao, @conta, @favorecido, @trecho, @vencimento, @valor, @ano, @hash
							    )");

                        }
                    }
                }

                foreach (var id in dc.Values)
                {
                    banco.AddParameter("id", id);
                    banco.ExecuteNonQuery("delete from cf_despesa where id=@id");
                }

                ProcessarDespesasTemp(banco);

                if (ano == DateTime.Now.Year)
                {
                    AtualizaParlamentarValores(banco);
                }
            }
        }

        public override void AtualizaParlamentarValores(AppDb banco)
        {
            banco.ExecuteNonQuery(@"
        	        UPDATE cl_deputado dp SET
                        valor_total = IFNULL((
                            SELECT SUM(ds.valor) FROM cl_despesa ds WHERE ds.id_cl_deputado = dp.id
                        ), 0);");
        }

        private void ProcessarDespesasTemp(AppDb banco)
        {
            InsereTipoDespesaFaltante(banco);
            InsereDeputadoFaltante(banco);
            //InsereFornecedorFaltante(banco);
            InsereDespesaFinal(banco);
            LimpaDespesaTemporaria(banco);
        }

        private void InsereTipoDespesaFaltante(AppDb banco)
        {
            banco.ExecuteNonQuery(@"
        	        INSERT INTO cl_despesa_tipo (descricao)
        	        select distinct tipo_despesa
        	        from tmp.cl_despesa_temp
        	        where tipo_despesa is not null
                    and tipo_despesa not in (
        		        select descricao from cl_despesa_tipo
        	        );
                ");

            if (banco.RowsAffected > 0)
            {
                logger.LogInformation("{Itens} tipos de despesa incluidas!", banco.RowsAffected);
            }
        }

        private void InsereDeputadoFaltante(AppDb banco)
        {
            banco.ExecuteNonQuery(@"
        	        INSERT INTO cl_deputado (nome_parlamentar, id_estado)
        	        select distinct Nome, 42
        	        from tmp.cl_despesa_temp
        	        where Nome not in (
        		        select Nome from cl_deputado where id_estado = 42
        	        );
                ");

            if (banco.RowsAffected > 0)
            {
                logger.LogInformation("{Itens} parlamentares incluidas!", banco.RowsAffected);
            }
        }

        //private  string InsereFornecedorFaltante(Banco banco)
        // {
        //     banco.ExecuteNonQuery(@"
        // INSERT INTO fornecedor (nome, cnpj_cpf)
        // select MAX(dt.empresa), dt.cnpj_cpf
        // from tmp.cl_despesa_temp dt
        // left join fornecedor f on f.cnpj_cpf = dt.cnpj_cpf
        // where dt.cnpj_cpf is not null
        // and f.id is null
        //             -- and LENGTH(dt.cnpj_cpf) <= 14
        // GROUP BY dt.cnpj_cpf;
        //");

        //     if (banco.Rows > 0)
        //     {
        //         return "<p>" + banco.Rows + "+ Fornecedor</p>";
        //     }

        //     return string.Empty;
        // }

        private void InsereDespesaFinal(AppDb banco)
        {
            banco.ExecuteNonQuery(@"
                -- UPDATE tmp.cl_despesa_temp SET observacao = NULL WHERE observacao = 'não consta documento';

				INSERT INTO cl_despesa (
					id_cl_deputado,
                    id_cl_despesa_tipo,
	                id_fornecedor,
	                data,
	                ano_mes,
	                numero_documento,
	                valor,
                    favorecido,
                    observacao,
                    hash
				)
                SELECT 
	                p.id AS id_cl_deputado,
                    dts.id_cl_despesa_tipo,
                    IFNULL(f.id, 82624) AS id_fornecedor,
                    d.data_emissao,
                    concat(year(d.data_emissao), LPAD(month(d.data_emissao), 2, '0')) AS ano_mes,
                    d.documento AS numero_documento,
                    d.valor AS valor,
                    d.favorecido,
                    d.observacao AS observacao,
                    d.hash
                FROM tmp.cl_despesa_temp d
                inner join cl_deputado p on p.nome_parlamentar like d.nome and id_estado = 42
                left join cl_despesa_tipo_sub dts on dts.descricao = d.tipo_despesa
                LEFT join fornecedor f on f.cnpj_cpf = d.cnpj_cpf
                ORDER BY d.id;
			", 3600);

            if (banco.RowsAffected > 0)
            {
                logger.LogInformation("{Itens} despesas incluidas!", banco.RowsAffected);
            }
        }

        private void LimpaDespesaTemporaria(AppDb banco)
        {
            banco.ExecuteNonQuery(@"
				truncate table tmp.cl_despesa_temp;
			");
        }

    }
}
