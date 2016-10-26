/*
 * Created by SharpDevelop.
 * User: Tex Killer
 * Date: 18/01/2016
 * Time: 06:23
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.IO;
using System.Text;
using System.Xml;
using OPS.Core;
using System.Collections.Generic;
using System.Net;
using RestSharp;
using System.Xml.Serialization;
using System.Threading;

namespace OPS.ImportacaoDados
{
	public static class Camara
	{
		public static void AtualizaInfoDeputados()
		{
			var ws = new CamaraWS.Deputados();
			XmlNode deputados = ws.ObterDeputados();
			XmlNodeList deputado = deputados.SelectNodes("*");
			StringBuilder sqlFields = new StringBuilder();
			StringBuilder sqlValues = new StringBuilder();

			using (var banco = new Banco())
			{
				banco.ExecuteNonQuery("TRUNCATE TABLE cf_deputado_temp");

				foreach (XmlNode fileNode in deputado)
				{
					sqlFields.Clear();
					sqlValues.Clear();

					foreach (XmlNode item in fileNode.SelectNodes("*"))
					{
						if (item.Name != "comissoes")
						{
							sqlFields.Append(string.Format(",{0}", item.Name));

							sqlValues.Append(string.Format(",@{0}", item.Name));
							banco.AddParameter(item.Name, item.InnerText.ToUpper());
						}
					}

					banco.ExecuteNonQuery("INSERT cf_deputado_temp (" + sqlFields.ToString().Substring(1) + ")  values (" + sqlValues.ToString().Substring(1) + ")");
				}


				banco.ExecuteNonQuery(@"
					update cf_deputado d
					left join cf_deputado_temp dt on dt.ideCadastro = d.id_cadastro
					left join partido p on p.sigla = dt.partido
					left join estado e on e.sigla = dt.uf
					set
						d.cod_orcamento = CASE dt.codOrcamento WHEN '' THEN NULL ELSE dt.codOrcamento END,
						d.condicao = dt.condicao,
						d.matricula = dt.matricula,
						d.id_parlamentar = dt.idParlamentar,
						d.nome_civil = dt.nome,
						d.nome_parlamentar = dt.nomeParlamentar,
						d.url_foto = dt.urlFoto,
						d.sexo = LEFT(dt.sexo, 1),
						d.id_estado = e.id,
						d.id_partido = p.id,
						d.gabinete = dt.gabinete,
						d.anexo = dt.anexo,
						d.fone = dt.fone,
						d.email = dt.email
					where dt.nomeParlamentar is not null
				");

				banco.ExecuteNonQuery("TRUNCATE TABLE cf_deputado_temp");
			}
		}

		/// <summary>
		///		<numLegislatura>54</numLegislatura>
		///		<email>dep.fabiofaria @camara.gov.br</email>
		///		<nomeProfissao>Administrador de Empresas</nomeProfissao>
		///		<dataNascimento>01/09/1977</dataNascimento>
		///		<dataFalecimento></dataFalecimento>
		///		<ufRepresentacaoAtual>RN</ufRepresentacaoAtual>
		///		<situacaoNaLegislaturaAtual>Em Exercício</situacaoNaLegislaturaAtual>
		///		<ideCadastro>141428</ideCadastro>
		///		<idParlamentarDeprecated>4460</idParlamentarDeprecated>
		///		<nomeParlamentarAtual>FÁBIO FARIA</nomeParlamentarAtual>
		///		<nomeCivil>FÁBIO SALUSTINO MESQUITA DE FARIA</nomeCivil>
		///		<sexo>M</sexo>
		///		<partidoAtual>
		///			<idPartido>PSD</idPartido>
		///			<sigla>PSD</sigla>
		///			<nome>Partido Social Democrático</nome>
		///		</partidoAtual>
		///		<gabinete>
		///			<numero></numero>
		///			<anexo></anexo>
		///			<telefone></telefone>
		///		</gabinete>
		/// </summary>
		public static void AtualizaInfoDeputadosCompleto()
		{
			var ws = new SitCamaraWS.DeputadosSoapClient();
			StringBuilder sqlFields = new StringBuilder();
			StringBuilder sqlValues = new StringBuilder();

			using (var banco = new Banco())
			{
				banco.ExecuteNonQuery("TRUNCATE TABLE cf_deputado_temp_detalhes");

				using (var dReader = banco.ExecuteReader("SELECT * from cf_deputado where nome_civil is null and id_cadastro is not null and id_cadastro <> 0"))
				{
					using (var banco2 = new Banco())
					{
						while (dReader.Read())
						{

							XmlDocument doc = new XmlDocument();
							var client = new RestClient("http://www.camara.leg.br/SitCamaraWS/Deputados.asmx/");
							var request = new RestRequest("ObterDetalhesDeputado?numLegislatura=&ideCadastro=" + dReader["id_cadastro"].ToString(), Method.GET);

							try
							{
								IRestResponse response = client.Execute(request);
								doc.LoadXml(response.Content);
							}
							catch (Exception)
							{
								Thread.Sleep(5000);

								try
								{
									IRestResponse response = client.Execute(request);
									doc.LoadXml(response.Content);
								}
								catch (Exception)
								{
									continue;
								}
							}

							XmlNode deputados = doc.DocumentElement;
							XmlNodeList deputado = deputados.SelectNodes("*");

							foreach (XmlNode fileNode in deputado)
							{
								sqlFields.Clear();
								sqlValues.Clear();

								foreach (XmlNode item in fileNode.ChildNodes)
								{
									if (item.Name == "comissoes") break;
									if (item.Name != "partidoAtual" && item.Name != "gabinete")
									{
										object value;
										if (item.Name.StartsWith("data"))
										{
											if (!string.IsNullOrEmpty(item.InnerText))
											{
												value = DateTime.Parse(item.InnerText).ToString("yyyy-MM-dd");
											}
											else
											{
												value = DBNull.Value;
											}
										}
										else
										{
											value = item.InnerText;
										}

										sqlFields.Append(string.Format(",{0}", item.Name));
										sqlValues.Append(string.Format(",@{0}", item.Name));
										banco2.AddParameter(item.Name, value);
									}
									else
									{
										foreach (XmlNode item2 in item.ChildNodes)
										{
											if (item2.Name == "idPartido" || item2.Name == "nome") continue;



											sqlFields.Append(string.Format(",{0}", item2.Name));
											sqlValues.Append(string.Format(",@{0}", item2.Name));
											banco2.AddParameter(item2.Name, item2.InnerText);
										}
									}
								}

								try
								{
									banco2.ExecuteNonQuery("INSERT cf_deputado_temp_detalhes (" + sqlFields.ToString().Substring(1) + ")  values (" + sqlValues.ToString().Substring(1) + ")");
								}
								catch (Exception ex)
								{
									continue;
								}
							}
						}

						banco2.ExecuteNonQuery(@"
							update cf_deputado d
							left join (
								select		
									ideCadastro,
									idParlamentarDeprecated,
									nomeCivil,
									nomeParlamentarAtual,
									sexo,
									nomeProfissao,
									dataNascimento,
									ufRepresentacaoAtual,
									max(email) as email,
									max(sigla) as sigla,
									max(numero) as numero,
									max(anexo) as anexo,
									max(telefone) as telefone,
									max(dataFalecimento) as dataFalecimento
								from cf_deputado_temp_detalhes
								group by 1, 2, 3, 4, 5, 6, 7, 8
							) dt on dt.ideCadastro = d.id_cadastro
							left join partido p on p.sigla = dt.sigla
							left join estado e on e.sigla = dt.ufRepresentacaoAtual
							set
								d.id_parlamentar = dt.idParlamentarDeprecated,
								d.nome_civil = dt.nomeCivil,
								d.nome_parlamentar = dt.nomeParlamentarAtual,
								d.sexo = LEFT(dt.sexo, 1),
								d.id_estado = e.id,
								d.id_partido = p.id,
								d.gabinete = dt.numero,
								d.anexo = dt.anexo,
								d.fone = dt.telefone,
								d.email = dt.email,
								d.profissao = dt.nomeProfissao,
								d.nascimento = dt.dataNascimento,
								d.falecimento = dt.dataFalecimento
							where dt.nomeParlamentarAtual is not null;
						");

						banco2.ExecuteNonQuery("TRUNCATE TABLE cf_deputado_temp_detalhes;");
					}
				}
			}
		}

		public static void ImportarDespesas(string atualDir)
		{
			string downloadUrl = "http://www.camara.leg.br/cotas/AnoAtual.zip";
			string fullFileNameZip = atualDir + @"\AnoAtual.zip";
			string fullFileNameXml = atualDir + @"\AnoAtual.xml";

			if (!Directory.Exists(atualDir))
			{
				Directory.CreateDirectory(atualDir);
			}

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(downloadUrl);

			request.UserAgent = "Other";
			request.Method = "HEAD";
			request.ContentType = "application/json;charset=UTF-8";
			request.Timeout = 1000000;

			using (System.Net.WebResponse resp = request.GetResponse())
			{
				long ContentLength = Convert.ToInt64(resp.Headers.Get("Content-Length"));
				//long ContentLengthLocal = new System.IO.FileInfo(fullFileNameZip).Length;

				//if (ContentLength == ContentLengthLocal)
				//{
				//	//Do something useful with ContentLength here 
				//	return;
				//}

				using (WebClient client = new WebClient())
				{
					client.Headers.Add("User-Agent: Other");
					client.DownloadFile(downloadUrl, fullFileNameZip);
				}
			}

			ICSharpCode.SharpZipLib.Zip.ZipFile file = null;

			try
			{
				file = new ICSharpCode.SharpZipLib.Zip.ZipFile(fullFileNameZip);

				if (file.TestArchive(true) == false)
				{
					throw new Exception("<script>alert('Erro no Zip. Faça o upload novamente.')</script>");
				}
			}
			finally
			{
				if (file != null)
					file.Close();
			}

			ICSharpCode.SharpZipLib.Zip.FastZip zip = new ICSharpCode.SharpZipLib.Zip.FastZip();
			zip.ExtractZip(fullFileNameZip, atualDir, null);

			CarregaDadosXml(fullFileNameXml);

			//File.Delete(fullFileNameZip);
			//File.Delete(fullFileNameXml);
		}

		private static void CarregaDadosXml(String fullFileNameXml)
		{
			using (Banco banco = new Banco())
			{
				LimpaDespesaTemporaria(banco);
				StreamReader stream = null;
				int linhaAtual = 0;

				try
				{
					if (fullFileNameXml.EndsWith("AnoAnterior.xml"))
						stream = new StreamReader(fullFileNameXml, Encoding.GetEncoding(850)); //"ISO-8859-1"
					else
						stream = new StreamReader(fullFileNameXml, Encoding.GetEncoding("ISO-8859-1"));

					StringBuilder sqlFields = new StringBuilder();
					StringBuilder sqlValues = new StringBuilder();

					using (XmlReader reader = XmlReader.Create(stream, new XmlReaderSettings() { IgnoreComments = true }))
					{
						reader.ReadToDescendant("DESPESAS");
						reader.ReadToDescendant("DESPESA");

						do
						{
							var strXmlNodeDespeza = reader.ReadOuterXml();
							if (string.IsNullOrEmpty(strXmlNodeDespeza))
							{
								ProcessarDespesasTemp(banco);
								break;
							}

							//DataRow drDespesa = dtDespesa.NewRow();
							XmlDocument doc = new XmlDocument();
							doc.LoadXml(strXmlNodeDespeza);
							XmlNodeList files = doc.DocumentElement.SelectNodes("*");

							sqlFields.Clear();
							sqlValues.Clear();

							foreach (XmlNode fileNode in files)
							{
								if (sqlFields.Length > 0)
								{
									sqlFields.Append(",");
									sqlValues.Append(",");
								}

								sqlFields.Append(fileNode.Name);
								sqlValues.Append("@" + fileNode.Name);

								string value;
								if (fileNode.Name == "datEmissao")
								{
									value = string.IsNullOrEmpty(fileNode.InnerText) ? "" : DateTime.Parse(fileNode.InnerText).ToString("yyyy-MM-dd");
								}
								else
								{
									value = fileNode.InnerText.ToUpper();
								}

								banco.AddParameter(fileNode.Name, value);
							}

							banco.ExecuteNonQuery("INSERT INTO cf_despesa_temp (" + sqlFields.ToString() + ") VALUES (" + sqlValues.ToString() + ")");

							if (++linhaAtual == 10000)
							{
								linhaAtual = 0;

								ProcessarDespesasTemp(banco);
							}
						}
						while (true);

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
		}

		private static void ProcessarDespesasTemp(Banco banco)
		{
			CorrigeDespesas(banco);
			//InsereDeputadoFaltante(banco);
			//InsereTipoDespesaFaltante(banco);
			//InsereTipoEspecificacaoFaltante(banco);
			//InsereMandatoFaltante(banco);
			//InsereLegislaturaFaltante(banco);
			InsereFornecedorFaltante(banco);

			InsereDespesaFinal(banco);
			LimpaDespesaTemporaria(banco);
		}

		private static void CorrigeDespesas(Banco banco)
		{
			banco.ExecuteNonQuery(@"
				UPDATE cf_despesa_temp SET txtNumero = NULL WHERE txtNumero = 'S/N' OR txtNumero = '';
				UPDATE cf_despesa_temp SET ideDocumento = NULL WHERE ideDocumento = '';
				UPDATE cf_despesa_temp SET txtCNPJCPF = NULL WHERE txtCNPJCPF = '';
				UPDATE cf_despesa_temp SET txtFornecedor = 'CORREIOS' WHERE txtCNPJCPF is null and txtFornecedor LIKE 'CORREIOS%';	
			");
		}

		private static void InsereDeputadoFaltante(Banco banco)
		{
			banco.ExecuteNonQuery(@"
				INSERT INTO cf_deputado (id, id_cadastro, nome_parlamentar)
				select distinct nuDeputadoId, ideCadastro, txNomeParlamentar 
				from cf_despesa_temp
				where nuDeputadoId  not in (
					select id from cf_deputado
				);
			");
		}

		private static void InsereTipoDespesaFaltante(Banco banco)
		{
			banco.ExecuteNonQuery(@"
				INSERT INTO cf_despesa_tipo (id, descricao)
				select distinct numSubCota, txtDescricao
				from cf_despesa_temp
				where numSubCota  not in (
					select id from cf_despesa_tipo
				);
			");
		}

		private static void InsereTipoEspecificacaoFaltante(Banco banco)
		{
			banco.ExecuteNonQuery(@"
				INSERT INTO cf_tipo_especificacao (id_cf_despesa_tipo, id_cf_especificacao, descricao)
				select distinct numSubCota, numEspecificacaoSubCota, txtDescricaoEspecificacao
				from cf_despesa_temp dt
				left join cf_tipo_especificacao tp on tp.id_cf_despesa_tipo = dt.numSubCota 
					and tp.id_cf_especificacao = dt.numEspecificacaoSubCota
				where numEspecificacaoSubCota <> 0
				AND tp.descricao = null;
			");
		}

		private static void InsereMandatoFaltante(Banco banco)
		{
			banco.ExecuteNonQuery(@"
				INSERT INTO cf_mandato (id_cf_deputado, id_legislatura, id_carteira_parlamantar, id_estado, id_partido)
				select distinct nuDeputadoId, codLegislatura, nuCarteiraParlamentar, e.id, p.id 
				from ( 
					select distinct 
					nuDeputadoId, nuLegislatura, nuCarteiraParlamentar, codLegislatura, sgUF, sgPartido
					from cf_despesa_temp
				) dt
				left join estado e on e.sigla = dt.sgUF
				left join partido p on p.sigla = dt.sgPartido
				left join cf_mandato m on m.id_cf_deputado = dt.nuDeputadoId 
					AND m.id_legislatura = dt.codLegislatura 
					AND m.id_carteira_parlamantar = nuCarteiraParlamentar
				where dt.codLegislatura <> 0
				AND m.id is null;
			");
		}

		private static void InsereLegislaturaFaltante(Banco banco)
		{
			banco.ExecuteNonQuery(@"
				INSERT INTO cf_legislatura (id, ano)
				select distinct codLegislatura, nuLegislatura
				from cf_despesa_temp dt
				where codLegislatura <> 0
				AND codLegislatura  not in (
					select codLegislatura from cf_mandato
				);
			");
		}

		private static void InsereFornecedorFaltante(Banco banco)
		{
			banco.ExecuteNonQuery(@"
				INSERT INTO fornecedor (nome, cnpj_cpf)
				select MAX(dt.txtFornecedor), dt.txtCNPJCPF
				from cf_despesa_temp dt
				left join fornecedor f on f.cnpj_cpf = dt.txtCNPJCPF
				where dt.txtCNPJCPF is not null
				and f.id is null
				GROUP BY dt.txtCNPJCPF;

				INSERT INTO fornecedor (nome, cnpj_cpf)
				select DISTINCT dt.txtFornecedor, dt.txtCNPJCPF
				from cf_despesa_temp dt
				left join fornecedor f on f.nome = dt.txtFornecedor
				where dt.txtCNPJCPF is null
				and f.id is null;
			");
		}

		private static void InsereDespesaFinal(Banco banco)
		{
			banco.ExecuteNonQuery(@"
				ALTER TABLE cf_despesa DISABLE KEYS;

				INSERT INTO cf_despesa (
					id_documento,
					id_cf_deputado,
					id_cf_mandato,
					id_cf_despesa_tipo,
					id_cf_especificacao,
					id_fornecedor,
					nome_passageiro,
					numero_documento,
					tipo_documento,
					data_emissao,
					valor_documento,
					valor_glosa,
					valor_liquido,
					valor_restituicao,
					mes,
					ano,
					parcela,
					trecho_viagem,
					lote,
					ressarcimento,
					ano_mes
				)
				select 
					dt.ideDocumento,
					dt.nuDeputadoId,
					m.id,
					numSubCota,
					numEspecificacaoSubCota,
					f.id,
					txtPassageiro, -- id_passageiro,
					txtNumero,
					indTipoDocumento,
					datEmissao,
					vlrDocumento,
					vlrGlosa,
					vlrLiquido,
					vlrRestituicao,
					numMes,
					numAno,
					numParcela,
					txtTrecho,
					numLote,
					numRessarcimento,
					concat(numAno, LPAD(numMes, 2, '0'))
				from cf_despesa_temp dt
				LEFT join fornecedor f on f.cnpj_cpf = dt.txtCNPJCPF
					or (f.cnpj_cpf is null and dt.txtCNPJCPF is null and f.nome = dt.txtFornecedor)
				left join cf_mandato m on m.id_cf_deputado = dt.nuDeputadoId
					and m.id_legislatura = dt.codLegislatura 
					and m.id_carteira_parlamantar = nuCarteiraParlamentar;
    
				ALTER TABLE cf_despesa ENABLE KEYS;
			", 3600);
		}

		private static void LimpaDespesaTemporaria(Banco banco)
		{
			banco.ExecuteNonQuery(@"
				truncate table cf_despesa_temp;
			");
		}
	}

	/* 
 Licensed under the Apache License, Version 2.0

 http://www.apache.org/licenses/LICENSE-2.0
 */
	[XmlRoot(ElementName = "partidoAtual")]
	public class PartidoAtual
	{
		[XmlElement(ElementName = "idPartido")]
		public string IdPartido { get; set; }
		[XmlElement(ElementName = "sigla")]
		public string Sigla { get; set; }
		[XmlElement(ElementName = "nome")]
		public string Nome { get; set; }
	}

	[XmlRoot(ElementName = "gabinete")]
	public class Gabinete
	{
		[XmlElement(ElementName = "numero")]
		public string Numero { get; set; }
		[XmlElement(ElementName = "anexo")]
		public string Anexo { get; set; }
		[XmlElement(ElementName = "telefone")]
		public string Telefone { get; set; }
	}

	[XmlRoot(ElementName = "comissao")]
	public class Comissao
	{
		[XmlElement(ElementName = "idOrgaoLegislativoCD")]
		public string IdOrgaoLegislativoCD { get; set; }
		[XmlElement(ElementName = "siglaComissao")]
		public string SiglaComissao { get; set; }
		[XmlElement(ElementName = "nomeComissao")]
		public string NomeComissao { get; set; }
		[XmlElement(ElementName = "condicaoMembro")]
		public string CondicaoMembro { get; set; }
		[XmlElement(ElementName = "dataEntrada")]
		public string DataEntrada { get; set; }
		[XmlElement(ElementName = "dataSaida")]
		public string DataSaida { get; set; }
	}

	[XmlRoot(ElementName = "comissoes")]
	public class Comissoes
	{
		[XmlElement(ElementName = "comissao")]
		public List<Comissao> Comissao { get; set; }
	}

	[XmlRoot(ElementName = "cargoComissoes")]
	public class CargoComissoes
	{
		[XmlElement(ElementName = "idOrgaoLegislativoCD")]
		public string IdOrgaoLegislativoCD { get; set; }
		[XmlElement(ElementName = "siglaComissao")]
		public string SiglaComissao { get; set; }
		[XmlElement(ElementName = "nomeComissao")]
		public string NomeComissao { get; set; }
		[XmlElement(ElementName = "idCargo")]
		public string IdCargo { get; set; }
		[XmlElement(ElementName = "nomeCargo")]
		public string NomeCargo { get; set; }
		[XmlElement(ElementName = "dataEntrada")]
		public string DataEntrada { get; set; }
		[XmlElement(ElementName = "dataSaida")]
		public string DataSaida { get; set; }
	}

	[XmlRoot(ElementName = "cargosComissoes")]
	public class CargosComissoes
	{
		[XmlElement(ElementName = "cargoComissoes")]
		public List<CargoComissoes> CargoComissoes { get; set; }
	}

	[XmlRoot(ElementName = "periodoExercicio")]
	public class PeriodoExercicio
	{
		[XmlElement(ElementName = "siglaUFRepresentacao")]
		public string SiglaUFRepresentacao { get; set; }
		[XmlElement(ElementName = "situacaoExercicio")]
		public string SituacaoExercicio { get; set; }
		[XmlElement(ElementName = "dataInicio")]
		public string DataInicio { get; set; }
		[XmlElement(ElementName = "dataFim")]
		public string DataFim { get; set; }
		[XmlElement(ElementName = "idCausaFimExercicio")]
		public string IdCausaFimExercicio { get; set; }
		[XmlElement(ElementName = "descricaoCausaFimExercicio")]
		public string DescricaoCausaFimExercicio { get; set; }
		[XmlElement(ElementName = "idCadastroParlamentarAnterior")]
		public string IdCadastroParlamentarAnterior { get; set; }
	}

	[XmlRoot(ElementName = "periodosExercicio")]
	public class PeriodosExercicio
	{
		[XmlElement(ElementName = "periodoExercicio")]
		public PeriodoExercicio PeriodoExercicio { get; set; }
	}

	[XmlRoot(ElementName = "itemHistoricoNomeParlamentar")]
	public class ItemHistoricoNomeParlamentar
	{
		[XmlElement(ElementName = "nomeParlamentarAnterior")]
		public string NomeParlamentarAnterior { get; set; }
		[XmlElement(ElementName = "nomeParlamentaPosterior")]
		public string NomeParlamentaPosterior { get; set; }
		[XmlElement(ElementName = "dataInicioVigenciaNomePosterior")]
		public string DataInicioVigenciaNomePosterior { get; set; }
	}

	[XmlRoot(ElementName = "historicoNomeParlamentar")]
	public class HistoricoNomeParlamentar
	{
		[XmlElement(ElementName = "itemHistoricoNomeParlamentar")]
		public ItemHistoricoNomeParlamentar ItemHistoricoNomeParlamentar { get; set; }
	}

	[XmlRoot(ElementName = "itemHistoricoLider")]
	public class ItemHistoricoLider
	{
		[XmlElement(ElementName = "idHistoricoLider")]
		public string IdHistoricoLider { get; set; }
		[XmlElement(ElementName = "idCargoLideranca")]
		public string IdCargoLideranca { get; set; }
		[XmlElement(ElementName = "descricaoCargoLideranca")]
		public string DescricaoCargoLideranca { get; set; }
		[XmlElement(ElementName = "numOrdemCargo")]
		public string NumOrdemCargo { get; set; }
		[XmlElement(ElementName = "dataDesignacao")]
		public string DataDesignacao { get; set; }
		[XmlElement(ElementName = "dataTermino")]
		public string DataTermino { get; set; }
		[XmlElement(ElementName = "codigoUnidadeLideranca")]
		public string CodigoUnidadeLideranca { get; set; }
		[XmlElement(ElementName = "siglaUnidadeLideranca")]
		public string SiglaUnidadeLideranca { get; set; }
		[XmlElement(ElementName = "idBlocoPartido")]
		public string IdBlocoPartido { get; set; }
	}

	[XmlRoot(ElementName = "historicoLider")]
	public class HistoricoLider
	{
		[XmlElement(ElementName = "itemHistoricoLider")]
		public List<ItemHistoricoLider> ItemHistoricoLider { get; set; }
	}

	[XmlRoot(ElementName = "Deputado")]
	public class Deputado
	{
		[XmlElement(ElementName = "numLegislatura")]
		public string NumLegislatura { get; set; }
		[XmlElement(ElementName = "email")]
		public string Email { get; set; }
		[XmlElement(ElementName = "nomeProfissao")]
		public string NomeProfissao { get; set; }
		[XmlElement(ElementName = "dataNascimento")]
		public string DataNascimento { get; set; }
		[XmlElement(ElementName = "dataFalecimento")]
		public string DataFalecimento { get; set; }
		[XmlElement(ElementName = "ufRepresentacaoAtual")]
		public string UfRepresentacaoAtual { get; set; }
		[XmlElement(ElementName = "situacaoNaLegislaturaAtual")]
		public string SituacaoNaLegislaturaAtual { get; set; }
		[XmlElement(ElementName = "ideCadastro")]
		public string IdeCadastro { get; set; }
		[XmlElement(ElementName = "idParlamentarDeprecated")]
		public string IdParlamentarDeprecated { get; set; }
		[XmlElement(ElementName = "nomeParlamentarAtual")]
		public string NomeParlamentarAtual { get; set; }
		[XmlElement(ElementName = "nomeCivil")]
		public string NomeCivil { get; set; }
		[XmlElement(ElementName = "sexo")]
		public string Sexo { get; set; }
		[XmlElement(ElementName = "partidoAtual")]
		public PartidoAtual PartidoAtual { get; set; }
		[XmlElement(ElementName = "gabinete")]
		public Gabinete Gabinete { get; set; }
		[XmlElement(ElementName = "comissoes")]
		public Comissoes Comissoes { get; set; }
		[XmlElement(ElementName = "cargosComissoes")]
		public CargosComissoes CargosComissoes { get; set; }
		[XmlElement(ElementName = "periodosExercicio")]
		public PeriodosExercicio PeriodosExercicio { get; set; }
		[XmlElement(ElementName = "historicoNomeParlamentar")]
		public HistoricoNomeParlamentar HistoricoNomeParlamentar { get; set; }
		[XmlElement(ElementName = "filiacoesPartidarias")]
		public string FiliacoesPartidarias { get; set; }
		[XmlElement(ElementName = "historicoLider")]
		public HistoricoLider HistoricoLider { get; set; }
	}

	[XmlRoot(ElementName = "Deputados")]
	public class Deputados
	{
		[XmlElement(ElementName = "Deputado")]
		public Deputado Deputado { get; set; }
	}

}