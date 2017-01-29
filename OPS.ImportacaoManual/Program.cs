using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using OPS.Core;
using OPS.ImportacaoDados;

namespace OPS.ImportacaoManual
{
    internal class Program
    {
        public static string[] ColunasXmlDespesasCamara =
        {
            "txNomeParlamentar",
            "ideCadastro",
            "nuCarteiraParlamentar",
            "nuLegislatura",
            "sgUF",
            "sgPartido",
            "codLegislatura",
            "numSubCota",
            "txtDescricao",
            "numEspecificacaoSubCota",
            "txtDescricaoEspecificacao",
            "txtFornecedor",
            "txtCNPJCPF",
            "txtNumero",
            "indTipoDocumento",
            "datEmissao",
            "vlrDocumento",
            "vlrGlosa",
            "vlrLiquido",
            "numMes",
            "numAno",
            "numParcela",
            "txtPassageiro",
            "txtTrecho",
            "numLote",
            "numRessarcimento",
            "ideDocumento",
            "vlrRestituicao",
            "nuDeputadoId"
        };

        public static string[] ColunasXmlDeputadosCamara =
        {
            "ideCadastro",
            "numLegislatura",
            "nomeParlamentar",
            "SEXO",
            "Profissao",
            "LegendaPartidoEleito",
            "UFEleito",
            "Condicao",
            "SiTuacaoMandato",
            "Matricula",
            "Gabinete",
            "Anexo",
            "Fone"
        };

        public static void ConverterXmlParaCsvDespesasCamara(string fullFileNameXml)
        {
            StreamReader stream = null;

            try
            {
                if (fullFileNameXml.EndsWith("AnoAnterior.xml"))
                    stream = new StreamReader(fullFileNameXml, Encoding.GetEncoding(850)); //"ISO-8859-1"
                else
                    stream = new StreamReader(fullFileNameXml, Encoding.GetEncoding("ISO-8859-1"));

                using (var reader = XmlReader.Create(stream, new XmlReaderSettings { IgnoreComments = true }))
                {
                    reader.ReadToDescendant("DESPESAS");
                    reader.ReadToDescendant("DESPESA");

                    using (var outputFile = new StreamWriter(fullFileNameXml.Replace(".xml", ".csv")))
                    {
                        do
                        {
                            var lstCsv = new List<string>();
                            var strXmlNodeDespeza = reader.ReadOuterXml();
                            if (string.IsNullOrEmpty(strXmlNodeDespeza))
                                break;

                            var doc = new XmlDocument();
                            doc.LoadXml(strXmlNodeDespeza);
                            var files = doc.DocumentElement.SelectNodes("*");

                            var indexXml = 0;
                            for (var i = 0; i < 29; i++)
                                if (files[indexXml].Name == ColunasXmlDespesasCamara[i])
                                    lstCsv.Add(files[indexXml++].InnerText);
                                else
                                    lstCsv.Add("");

                            outputFile.WriteLine("\"" + string.Join("\";\"", lstCsv) + "\"");
                        } while (true);
                    }

                    reader.Close();
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                stream.Close();
                stream.Dispose();
            }
        }

        public static void ConverterXmlParaCsvDeputadosCamara()
        {
            var fullFileNameXml = @"C:\GitHub\OPS\temp\Deputados.xml";
            StreamReader stream = null;

            try
            {
                //if (fullFileNameXml.EndsWith("AnoAnterior.xml"))
                //	stream = new StreamReader(fullFileNameXml, Encoding.GetEncoding(850)); //"ISO-8859-1"
                //else
                stream = new StreamReader(fullFileNameXml, Encoding.GetEncoding("ISO-8859-1"));

                using (var reader = XmlReader.Create(stream, new XmlReaderSettings { IgnoreComments = true }))
                {
                    reader.ReadToDescendant("orgao");
                    reader.ReadToDescendant("Deputados");
                    reader.ReadToDescendant("Deputado");

                    using (var outputFile = new StreamWriter(fullFileNameXml.Replace(".xml", ".csv")))
                    {
                        do
                        {
                            var lstCsv = new List<string>();
                            var strXmlNodeDeputado = reader.ReadOuterXml();
                            if (string.IsNullOrEmpty(strXmlNodeDeputado))
                                break;

                            var doc = new XmlDocument();
                            doc.LoadXml(strXmlNodeDeputado);
                            var files = doc.DocumentElement.SelectNodes("*");

                            var indexXml = 0;
                            for (var i = 0; i < 13; i++)
                                if (files[indexXml].Name == ColunasXmlDeputadosCamara[i])
                                    lstCsv.Add(files[indexXml++].InnerText);
                                else
                                    lstCsv.Add("");

                            outputFile.WriteLine("\"" + string.Join("\";\"", lstCsv) + "\"");
                        } while (true);
                    }

                    reader.Close();
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                stream.Close();
                stream.Dispose();
            }
        }

        public static void MigrarFornecedores()
        {
            using (var banco = new Banco())
            {
                var dtFornecedoresInfo = banco.GetTable(
                    @"SELECT
					txtCNPJCPF,
					txtBeneficiario,
					NomeFantasia,
					AtividadePrincipal,
					AtividadeSecundaria01,
					AtividadeSecundaria02,
					AtividadeSecundaria03,
					AtividadeSecundaria04,
					AtividadeSecundaria05,
					AtividadeSecundaria06,
					AtividadeSecundaria07,
					AtividadeSecundaria08,
					AtividadeSecundaria09,
					AtividadeSecundaria10,
					AtividadeSecundaria11,
					AtividadeSecundaria12,
					AtividadeSecundaria13,
					AtividadeSecundaria14,
					AtividadeSecundaria15,
					AtividadeSecundaria16,
					AtividadeSecundaria17,
					AtividadeSecundaria18,
					AtividadeSecundaria19,
					AtividadeSecundaria20,
					NaturezaJuridica,
					Logradouro,
					Numero,
					Complemento,
					Cep,
					Bairro,
					Cidade,
					Uf,
					Situacao,
					CAST(DataSituacao as CHAR(50)) as DataSituacao,
					MotivoSituacao,
					SituacaoEspecial,
					CAST(DataSituacaoEspecial as CHAR(50)) as DataSituacaoEspecial,
					CAST(DataAbertura as CHAR(50)) as DataAbertura,
					UsuarioInclusao,
					CAST(DataInclusao as CHAR(50)) as DataInclusao,
					Doador,
					PendenteFoto,
					CAST(DataUltimaNotaFiscal as CHAR(50)) as DataUltimaNotaFiscal,
					Email,
					Telefone,
					EnteFederativoResponsavel,
					CapitalSocial
					FROM auditoria.fornecedores where NomeFantasia is not null;");
                var dtFornecedoresAtividade = banco.GetTable("SELECT * FROM ops.fornecedor_atividade;");
                //DataTable dtFornecedoresAtividadeSec = banco.GetTable("SELECT * FROM ops.fornecedor_atividade_secundaria;");
                var dtFornecedoresNatJu = banco.GetTable("SELECT * FROM ops.fornecedor_natureza_juridica;");
                //DataTable dtFornecedoresQAS = banco.GetTable("SELECT * FROM ops.fornecedorquadrosocietario;");

                var dtFornecedores = banco.GetTable("SELECT * FROM ops.fornecedor;");

                foreach (DataRow item in dtFornecedores.Rows)
                {
                    var drItens = dtFornecedoresInfo.Select("txtCNPJCPF='" + item["cnpj_cpf"] + "'");
                    if (drItens.Length == 0)
                        continue;

                    var dr = drItens[0];

                    var strSql =
                        @"insert into ops.fornecedor_info(
							id_fornecedor,
							cnpj,
							obtido_em,
							nome,
							data_de_abertura,
							nome_fantasia,
							id_fornecedor_atividade_principal,
							id_fornecedor_natureza_juridica,
							logradouro,
							numero,
							complemento,
							cep,
							bairro,
							municipio,
							estado,
							endereco_eletronico,
							telefone,
							ente_federativo_responsavel,
							situacao_cadastral,
							data_da_situacao_cadastral,
							motivo_situacao_cadastral,
							situacao_especial,
							data_situacao_especial,
							capital_social
						) values (
							@id_fornecedor,
							@cnpj,
							@obtido_em,
							@nome,
							@data_de_abertura,
							@nome_fantasia,
							@id_fornecedor_atividade_principal,
							@id_fornecedor_natureza_juridica,
							@logradouro,
							@numero,
							@complemento,
							@cep,
							@bairro,
							@municipio,
							@estado,
							@endereco_eletronico,
							@telefone,
							@ente_federativo_responsavel,
							@situacao_cadastral,
							@data_da_situacao_cadastral,
							@motivo_situacao_cadastral,
							@situacao_especial,
							@data_situacao_especial,
							@capital_social
						)";

                    banco.AddParameter("@id_fornecedor", item["id"]);
                    banco.AddParameter("@cnpj", item["cnpj_cpf"]);
                    banco.AddParameter("@obtido_em", ParseDate(dr["DataInclusao"]));
                    banco.AddParameter("@nome", dr["txtBeneficiario"]);
                    banco.AddParameter("@data_de_abertura", ParseDate(dr["DataAbertura"]));
                    banco.AddParameter("@nome_fantasia", dr["NomeFantasia"]);

                    var drAT = dtFornecedoresAtividade.Select("codigo='" + dr["AtividadePrincipal"].ToString().Split(' ')[0] +
                                                       "'");
                    if (drAT.Length > 0)
                        banco.AddParameter("@id_fornecedor_atividade_principal", drAT[0]["id"]);
                    else
                        banco.AddParameter("@id_fornecedor_atividade_principal", DBNull.Value);

                    var drNJ = dtFornecedoresNatJu.Select("codigo='" + dr["NaturezaJuridica"].ToString().Split(' ')[0] + "'");
                    if (drAT.Length > 0)
                        banco.AddParameter("@id_fornecedor_natureza_juridica", drNJ[0]["id"]);
                    else
                        banco.AddParameter("@id_fornecedor_natureza_juridica", DBNull.Value);

                    banco.AddParameter("@logradouro", dr["Logradouro"]);
                    banco.AddParameter("@numero", dr["Numero"]);
                    banco.AddParameter("@complemento", dr["Complemento"]);
                    banco.AddParameter("@cep", dr["Cep"]);
                    banco.AddParameter("@bairro", dr["Bairro"]);
                    banco.AddParameter("@municipio", dr["Cidade"]);
                    banco.AddParameter("@estado", dr["Uf"]);
                    banco.AddParameter("@endereco_eletronico", dr["Email"]);
                    banco.AddParameter("@telefone", dr["Telefone"]);
                    banco.AddParameter("@ente_federativo_responsavel", dr["EnteFederativoResponsavel"]);
                    banco.AddParameter("@situacao_cadastral", dr["Situacao"]);
                    banco.AddParameter("@data_da_situacao_cadastral", ParseDate(dr["DataSituacao"]));
                    banco.AddParameter("@motivo_situacao_cadastral", dr["MotivoSituacao"]);
                    banco.AddParameter("@situacao_especial", dr["SituacaoEspecial"]);
                    banco.AddParameter("@data_situacao_especial", ParseDate(dr["DataSituacaoEspecial"]));
                    banco.AddParameter("@capital_social", ObterValor(dr["CapitalSocial"]));

                    banco.ExecuteNonQuery(strSql);


                    var strSql2 = @"insert into ops.fornecedor_atividade_secundaria values (@id_fornecedor_info, @id_fornecedor_atividade)";

                    for (var i = 0; i < 20; i++)
                        try
                        {
                            var pos = (i + 1).ToString("00");
                            if (Convert.IsDBNull(dr["AtividadeSecundaria" + pos]) ||
                                string.IsNullOrEmpty(dr["AtividadeSecundaria" + pos].ToString())) continue;

                            var drATS = dtFornecedoresAtividade.Select("codigo='" + dr["AtividadeSecundaria" + pos].ToString().Split(' ')[0] + "'");
                            if (drATS.Length > 0)
                            {
                                banco.AddParameter("@id_fornecedor_info", item["id"]);
                                banco.AddParameter("@id_fornecedor_atividade", drATS[0]["id"]);
                                banco.ExecuteNonQuery(strSql2);
                            }
                        }
                        catch
                        {
                            // ignored
                            //if (!ex.Message.Contains("Duplicate entry")) break;
                        }
                }

                banco.ExecuteNonQuery(@"
					update ops.fornecedor_info set nome_fantasia=null where nome_fantasia = '' or nome_fantasia = '********';
					update ops.fornecedor_info set logradouro=null where logradouro = '';
					update ops.fornecedor_info set numero=null where numero = '';
					update ops.fornecedor_info set complemento=null where complemento = '';
					update ops.fornecedor_info set cep=null where cep = '';
					update ops.fornecedor_info set bairro=null where bairro = '';
					update ops.fornecedor_info set municipio=null where municipio = '';
					update ops.fornecedor_info set estado=null where estado = '';
					update ops.fornecedor_info set endereco_eletronico=null where endereco_eletronico = '';
					update ops.fornecedor_info set telefone=null where telefone = '';
					update ops.fornecedor_info set ente_federativo_responsavel=null where ente_federativo_responsavel = '';
					update ops.fornecedor_info set motivo_situacao_cadastral=null where motivo_situacao_cadastral = '';
					update ops.fornecedor_info set situacao_especial=null where situacao_especial = '' or situacao_especial = '********';
				");
            }
        }

        private static object ObterValor(object d)
        {
            if (Convert.IsDBNull(d) || string.IsNullOrEmpty(d.ToString()))
                return DBNull.Value;
            try
            {
                return Convert.ToDecimal(d.ToString().Split(' ')[1]);
            }
            catch
            {
                return DBNull.Value;
            }
        }

        private static object ParseDate(object d)
        {
            if (Convert.IsDBNull(d) || string.IsNullOrEmpty(d.ToString()) || (d.ToString() == "0000-00-00 00:00:00") ||
                d.ToString().StartsWith("*"))
                return DBNull.Value;
            try
            {
                return Convert.ToDateTime(d);
            }
            catch
            {
                return DBNull.Value;
            }
        }

        public static void AtualizaCampeoesGastos()
        {
            var strSql =
                @"truncate table cf_deputado_campeao_gasto;
				insert into cf_deputado_campeao_gasto
				SELECT l1.id_cf_deputado, d.id_cadastro, d.nome_parlamentar, l1.valor_total, p.sigla, e.sigla
				FROM (
					SELECT 
						l.id_cf_deputado,
						sum(l.valor_liquido) as valor_total
					FROM  cf_despesa l
					where l.ano_mes >= 201502 
					GROUP BY l.id_cf_deputado
					order by valor_total desc 
					limit 4
				) l1 
				INNER JOIN cf_deputado d on d.id = l1.id_cf_deputado 
				LEFT JOIN partido p on p.id = d.id_partido
				LEFT JOIN estado e on e.id = d.id_estado;

				truncate table sf_senador_campeao_gasto;
				insert into sf_senador_campeao_gasto
				SELECT l1.id_sf_senador, d.nome, l1.valor_total, p.sigla, e.sigla
				FROM (
					SELECT 
						l.id_sf_senador,
						sum(l.valor) as valor_total
					FROM  sf_despesa l
					where l.ano_mes >= 201002 
					GROUP BY l.id_sf_senador
					order by valor_total desc 
					limit 4
				) l1 
				INNER JOIN sf_senador d on d.id = l1.id_sf_senador
				LEFT JOIN partido p on p.id = d.id_partido
				LEFT JOIN estado e on e.id = d.id_estado;
				";

            using (var banco = new Banco())
            {
                banco.ExecuteNonQuery(strSql);
            }
        }

        public static void AtualizaFornecedorDoador()
        {
            using (var banco = new Banco())
            {
                var dt = banco.GetTable("select id, cnpj_cpf from fornecedor");

                foreach (DataRow dr in dt.Rows)
                {
                    banco.AddParameter("cnpj", dr["cnpj_cpf"]);
                    var existe = banco.ExecuteScalar("select 1 from eleicao_doacao where raiz_cnpj_cpf_doador=@cnpj;");
                    if (existe != null)
                    {
                        banco.AddParameter("id", dr["id"]);
                        banco.ExecuteNonQuery("update fornecedor set doador=1 where id=@id");
                    }
                }
            }
        }

        public static void Main(string[] args)
        {
            var tempPath = @"C:\GitHub\operacao-politica-supervisionada\OPS\temp";

            Padrao.ConnectionString = ConfigurationManager.ConnectionStrings["AuditoriaContext"].ToString();

            //MigrarFornecedores();

            //ImportacaoDados.Camara.AtualizaInfoDeputados();
            //ImportacaoDados.Camara.AtualizaInfoDeputadosCompleto();

            //ImportacaoDados.Camara.ImportarMandatos();

            //ImportacaoDados.Senado.CarregaSenadores();

            //Teste
            //ConverterXmlParaCsvDeputadosCamara();
            //ConverterXmlParaCsvDespesasCamara(tempPath + @"\AnoAtual.xml");

            //Importação na nova estrutura
            ImportacaoDados.Camara.ImportarDespesas(tempPath);

            //for (int ano = 2008; ano <= 2016; ano++)
            //{
            //	ImportacaoDados.Senado.ImportarDespesas(tempPath, ano);
            //}
            ImportacaoDados.Senado.ImportarDespesas(tempPath, 2017);

            //ImportacaoDados.CamaraAtualizaDeputadoValores();
            //ImportacaoDados.Senado.AtualizaSenadorValores();
            AtualizaCampeoesGastos();
            //AtualizaFornecedorDoador();

            //Camara.ImportaPresencasDeputados();
            //Fornecedor.ConsultarReceitaWS();
        }
    }
}