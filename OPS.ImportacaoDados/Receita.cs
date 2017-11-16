using OPS.Core;
using OPS.Core.Models;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;

public static class Receita
{
	private static CookieContainer _cookies;
	private static string urlBaseReceitaFederal = "http://www.receita.fazenda.gov.br/PessoaJuridica/CNPJ/cnpjreva/";
	private static string paginaValidacao = "valida.asp";
	private static string paginaPrincipal = "Cnpjreva_Solicitacao2.asp";
	private static string paginaCaptcha = "captcha/gerarCaptcha.asp";
	private static string[] linguagens = new string[] { /*"eng", "captcha", "captcha2", "captcha4", "captcha5", "captcha6", "receita", "receita2", "receita3", "captcha3", "receita4", "receita5", "receita6", "receita7",*/ "receita8" };
	private static int[] erros = new int[linguagens.Length + 1];
	private static float[] tentativas = new float[linguagens.Length];
	private static int[] contagem = new int[linguagens.Length];
	private static int linguagem = linguagens.Length;
	private static readonly Mutex mutex = new Mutex();
	private static readonly Stopwatch sw = new Stopwatch();
	private static string tempPath = @"C:\GitHub\operacao-politica-supervisionada\OPS\temp\";

	private static Bitmap PegarCaptchaCNPJ()
	{
		if (_cookies == null)
		{
			_cookies = new CookieContainer();
			var htmlResult = string.Empty;

			using (var wc = new CookieAwareWebClient(_cookies))
			{
				wc.Headers[HttpRequestHeader.UserAgent] = "Mozilla/4.0 (compatible; Synapse)";
				wc.Headers[HttpRequestHeader.KeepAlive] = "300";
				htmlResult = wc.DownloadString(urlBaseReceitaFederal + paginaPrincipal);
			}

			if (htmlResult.Length == 0)
			{
				return null;
			}
		}

		var wc2 = new CookieAwareWebClient(_cookies);
		wc2.Headers[HttpRequestHeader.UserAgent] = "Mozilla/4.0 (compatible; Synapse)";
		wc2.Headers[HttpRequestHeader.KeepAlive] = "300";
		byte[] data = wc2.DownloadData(urlBaseReceitaFederal + paginaCaptcha);

		_cookies.Add(new Uri(urlBaseReceitaFederal), new Cookie("flag", "1"));
		using (MemoryStream ms = new MemoryStream(data))
		{
			var bmp = new Bitmap(ms);

			if (File.Exists(tempPath))
				File.Delete(tempPath + @"captcha.png");

			bmp.Save(tempPath + @"captcha.png", System.Drawing.Imaging.ImageFormat.Png);

			return bmp;
		}
	}

	public static void ConsultarCNPJ(string CNPJ, ref int totalAcertos, ref int totalErros)
	{
		bool bResolverCapchaAutomatico = true;
		Random aleatorio = new Random();
		mutex.WaitOne();
		try
		{
			string texto;
			Bitmap captcha, limpo;
			int id = 0, i = 0;
			do
			{
				erros[linguagem]++;
				if (linguagem < linguagens.Length - 1)
				{
					linguagem++;
				}
				else
				{
					linguagem = 0;
				}

				if (bResolverCapchaAutomatico)
				{
					sw.Stop();
					if (sw.Elapsed.TotalSeconds < 4)
					{
						System.Threading.Thread.Sleep(aleatorio.Next(3000, 5000));
					}
				}

				captcha = PegarCaptchaCNPJ();

				if (bResolverCapchaAutomatico)
				{
					System.Threading.Thread.Sleep(aleatorio.Next(5000, 7000));
					sw.Reset();
					sw.Start();
				}

				limpo = Captcha.Limpar(captcha);
				texto = Captcha.OCR(limpo, linguagens[linguagem]);

				try
				{
					Console.WriteLine("Captcha: {0}", texto);

					if (!bResolverCapchaAutomatico)
					{
						Console.WriteLine("Digite o Captcha:");
						string temp = Console.ReadLine();
						if (!string.IsNullOrEmpty(temp))
						{
							texto = temp;
						}
					}

					var oFormatarDados = new FormatarDados();
					Fornecedor fornecedor = oFormatarDados.ObterDados(_cookies, CNPJ, texto, true);

					if(fornecedor.Situacao != "ATIVA"){
						Console.WriteLine("--- Fornecedor não ativo ---");
					}

					totalAcertos++;

					break;
				}
				catch (Exception ex)
				{
					if (ex.Message == "O número do CNPJ não foi localizado na Receita Federal")
					{
						totalAcertos++;
						using (Banco banco = new Banco())
						{
							banco.AddParameter("@cnpj_cpf", CNPJ);
							banco.AddParameter("@controle", 5);
							banco.AddParameter("@mensagem", ex.Message);

							banco.ExecuteNonQuery(@"update fornecedor set controle=@controle, mensagem=@mensagem where cnpj_cpf=@cnpj_cpf;");
						}

						break;
					}
					else
					{
						totalErros++;
						if (i++ == 5)
						{
							break;
						}
					}

					Console.WriteLine(ex.Message);

					tentativas[linguagem] = (contagem[linguagem] * tentativas[linguagem] + erros[linguagem] + 1) / (contagem[linguagem] + 1);
					contagem[linguagem]++;
					erros[linguagem] = -1;

					//limpo.Save(tempPath + "c.limpo.png");
					//limpo.Dispose();

					if (ex.Message.Contains("tempo limite"))
					{
						System.Threading.Thread.Sleep(3600000);
					}
				}


			} while (id == 0);

			/*for (int l = 0; l < Receita.linguagens.Length; l++) {
                System.Web.HttpContext.Current.Response.Write(linguagens[l] + ": " + (tentativas[l] < erros[l] ? (contagem[l] * tentativas[l] + erros[l] + 1) / (contagem[l] + 1) : tentativas[l]) + "<br />");
            }*/
			mutex.ReleaseMutex();
			/*using (StreamWriter sw = File.AppendText(System.Web.HttpContext.Current.Server.MapPath("/log.txt"))) {
                sw.WriteLine(DateTime.Now + ": " + retorno);
            }*/
		}
		catch (Exception e)
		{
			Console.WriteLine("Acertos: " + totalAcertos);
			Console.WriteLine("Erros: " + totalErros);

			erros[linguagem]--;
			mutex.ReleaseMutex();
			if (e is System.NullReferenceException)
			{
				/*using (StreamWriter sw = File.AppendText(System.Web.HttpContext.Current.Server.MapPath("/log.txt"))) {
                    sw.WriteLine(DateTime.Now + ": Imagem invalida");
                }*/
				throw new BusinessException("Imagem invalida");
			}
			else
			{
				if ((e.GetBaseException() is System.IO.IOException) || (e.GetBaseException() is System.Net.Sockets.SocketException))
				{
					//System.Threading.Thread.Sleep(aleatorio.Next(50000, 500000));
					/*using (StreamWriter sw = File.AppendText(System.Web.HttpContext.Current.Server.MapPath("/log.txt"))) {
                        sw.WriteLine(DateTime.Now + ": IP bloqueado");
                    }*/
					throw new BusinessException("IP bloqueado");
				}
				else
				{
					/*using (StreamWriter sw = File.AppendText(System.Web.HttpContext.Current.Server.MapPath("/log.txt"))) {
                        sw.WriteLine(DateTime.Now + ": " + e.ToString());
                    }*/
					throw;
				}
			}
		}
	}
}