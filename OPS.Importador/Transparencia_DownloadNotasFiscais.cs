using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using CsvHelper;
using ICSharpCode.SharpZipLib.Zip;
using OPS.Core;

namespace OPS.Importador
{
    public class Transparencia_DownloadNotasFiscais
    {
        private static readonly HttpClient client = new HttpClient();

        // Página:
        // > http://www.portaltransparencia.gov.br/download-de-dados/notas-fiscais
        // Download:
        // > http://www.portaltransparencia.gov.br/download-de-dados/notas-fiscais/202109

        public static void ImportarNotas(string atualDir, int ano, int mes)
        {
            if (!Directory.Exists(atualDir))
                Directory.CreateDirectory(atualDir);
            string nomeArquivoZip = $"NFe_{ano}{mes:00}.zip";
            string caminhoZip = Path.Combine(atualDir, nomeArquivoZip);
            // Durante testes, baixar de novo a cada teste é complicado
            if (!File.Exists(caminhoZip))
            {
                string url = geraUrlDownload(ano, mes);
                var response = client.GetAsync(url).Result;
                // Se não conseguiu fazer o download, retorna um HttpRequestException
                response.EnsureSuccessStatusCode();

                // Salva a stream
                using (var fs = new FileStream(caminhoZip, FileMode.CreateNew))
                {
                    var ResponseTask = response.Content.CopyToAsync(fs);
                    ResponseTask.Wait();
                }
            }

            // Desempacota
            ZipFile file = null;
            try
            {
                file = new ZipFile(caminhoZip);

                if (file.TestArchive(true) == false)
                    throw new BusinessException("Notas: Arquivo Zip corrompido.");

                if (!processaZip(file))
                {
                    throw new BusinessException("Notas: Arquivo incompleto");
                }
            }
            finally
            {
                if (file != null)
                    file.Close();
            }
        }
        private static bool processaZip(ZipFile zip)
        {
            // YYYYMM_NFe_NotaFiscal.csv
            Stream arquivoFiscal = null;
            // YYYYMM_NFe_NotaFiscalEvento.csv
            Stream arquivoEventos = null;
            // YYYYMM_NFe_NotaFiscalItem.csv
            Stream arquivoItens = null;

            foreach (ZipEntry entry in zip)
            {
                // Não tem pastas
                if (!entry.IsFile) continue;
                // separa cada um deles

                // Não usando caracteres que podem mudar, só o final
                if (entry.Name.EndsWith("iscal.csv")) arquivoFiscal = zip.GetInputStream(entry);
                if (entry.Name.EndsWith("vento.csv")) arquivoEventos = zip.GetInputStream(entry);
                if (entry.Name.EndsWith("tem.csv")) arquivoItens = zip.GetInputStream(entry);
            }

            // Deu erado?
            if (arquivoFiscal == null || arquivoEventos == null || arquivoItens == null)
            {
                if (arquivoFiscal != null) arquivoFiscal.Dispose();
                if (arquivoEventos != null) arquivoEventos.Dispose();
                if (arquivoItens != null) arquivoItens.Dispose();
                return false;
            }

            // Eventos podem ser de datas diferentes
            // Um evento do mês 10, pode ser referente à uma NF emitida no mês 09
            // Portando, os eventos não precisam ser processados junto com as notas do mês
            // não vou tentar omitizar a gravação

            var nfs = processaNFs(arquivoFiscal);
            var itens = processaProdutos(arquivoItens);

            // Distribuir produtos por chave
            var dicItens = itens.GroupBy(i => i.Chave)
                                .ToDictionary(o => o.Key, e => e.ToArray());

            // o mais eficiente é distribuir os itens nas notas,
            // porém desconfio que podem haver itens sem nota e nota sem itens,
            // Ex.: A NF 13211002660659000108550010000010821080300097
            //  > não tem o item 2 no arquivo do mês 10, apenas os itens 1 e 3

            // Como nos meses 09, 10 e 11 não tem nenhum ocorrência, vou comentar
            /*
                HashSet<string> chaveNFs = nfs.Select(n => n.Chave).ToHashSet();
                // Notas sem Itens
                var notasSemItens = nfs.Where(nf => !dicItens.ContainsKey(nf.Chave)).ToArray();
                if (notasSemItens.Length > 0) { }
                var itensSemNotas = dicItens.Where(i => !chaveNFs.Contains(i.Key)).ToArray();
                if (itensSemNotas.Length > 0) { }
            */

            // Preencher
            foreach (var n in nfs)
            {
                // Se tiver nota sem itens, ele vai quebrar aqui
                // Ainda não vou tratar, pois se ocorrer é um problema que deve imterromper a importação
                n.ItensNota = dicItens[n.Chave];
            }
            gravarNFeBD(nfs);

            // Após a gravação, pode liberar a memória
            nfs = null;
            itens = null;
            dicItens = null;
            arquivoFiscal.Dispose();
            arquivoItens.Dispose();

            // Processa eventos
            var eventos = processaEventos(arquivoEventos);
            gravaEventosBD(eventos);

            arquivoEventos.Dispose();

            return true;
        }

        private static NotaFiscal[] processaNFs(Stream arquivoFiscal)
        {
            using var reader = new StreamReader(arquivoFiscal, Encoding.GetEncoding("ISO-8859-1"));
            using var csv = new CsvReader(reader, System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR"));

            List<NotaFiscal> lst = new List<NotaFiscal>();

            csv.Read(); // Avança para a linha[0] 
            csv.ReadHeader(); // carrega, na linha zero, os headers
            while (csv.Read())
            {
                lst.Add(new NotaFiscal()
                {
                    Chave = csv["CHAVE DE ACESSO"],
                    Modelo = int.Parse(csv["MODELO"].Split('-')[0]),
                    Serie = int.Parse(csv["SÉRIE"]),
                    Numero = int.Parse(csv["NÚMERO"]),
                    Natureza = csv["NATUREZA DA OPERAÇÃO"],
                    DataEmissao = csv.GetField<DateTime>("DATA EMISSÃO"),
                    EventoMaisRecente = csv["EVENTO MAIS RECENTE"],
                    DataEventoMaisRecente = csv.GetField<DateTime>("DATA/HORA EVENTO MAIS RECENTE"),

                    Emitente_Documento = csv["CPF/CNPJ Emitente"],
                    Emitente_Razao = csv["RAZÃO SOCIAL EMITENTE"],
                    Emitente_IE = csv["INSCRIÇÃO ESTADUAL EMITENTE"],
                    Emitente_UF = csv["UF EMITENTE"],
                    Emitente_Municipio = csv["MUNICÍPIO EMITENTE"],

                    Destinatario_Documento = csv["CNPJ DESTINATÁRIO"],
                    Destinatario_Nome = csv["NOME DESTINATÁRIO"],
                    Destinatario_UF = csv["UF DESTINATÁRIO"],
                    Destinatario_IndicadorIE = csv["INDICADOR IE DESTINATÁRIO"],

                    DestinoOperacao = int.Parse(csv["DESTINO DA OPERAÇÃO"].Split('-')[0]),
                    ConsumidorFinal = int.Parse(csv["CONSUMIDOR FINAL"].Split('-')[0]),
                    IndicadorPresenca = int.Parse(csv["PRESENÇA DO COMPRADOR"].Split('-')[0]),
                    ValorOperacao = csv.GetField<decimal>("VALOR NOTA FISCAL"),
                });
            }
            return lst.ToArray();
        }
        private static Item[] processaProdutos(Stream arquivoItem)
        {
            using var reader = new StreamReader(arquivoItem, Encoding.GetEncoding("ISO-8859-1"));
            using var csv = new CsvReader(reader, System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR"));

            List<Item> lst = new List<Item>();

            csv.Read(); // Avança para a linha[0] 
            csv.ReadHeader(); // carrega, na linha zero, os headers
            while (csv.Read())
            {
                lst.Add(new Item()
                {
                    Chave = csv["CHAVE DE ACESSO"],
                    Indice = int.Parse(csv["NÚMERO PRODUTO"]),
                    NomeProduto = csv["DESCRIÇÃO DO PRODUTO/SERVIÇO"],
                    NCM = csv["CÓDIGO NCM/SH"],
                    // Vou ignorar a descrição do NCM
                    CFOP = int.Parse(csv["CFOP"]),
                    Quantidade = csv.GetField<decimal>("QUANTIDADE"),
                    Unidade = csv["UNIDADE"],
                    ValorUnitario = csv.GetField<decimal>("VALOR UNITÁRIO"),
                    ValorTotal = csv.GetField<decimal>("VALOR TOTAL"),
                });
            }
            return lst.ToArray();
        }
        private static Evento[] processaEventos(Stream arquivoItem)
        {
            using var reader = new StreamReader(arquivoItem, Encoding.GetEncoding("ISO-8859-1"));
            using var csv = new CsvReader(reader, System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR"));

            List<Evento> lst = new List<Evento>();

            csv.Read(); // Avança para a linha[0] 
            csv.ReadHeader(); // carrega, na linha zero, os headers
            while (csv.Read())
            {
                string chave = csv["CHAVE DE ACESSO"];
                string descricaoEvento = csv["DESCRIÇÃO EVENTO"];
                string protocolo = "";
                // Protocolo não tem campo específico e parece estar em todos os "DESCRIÇÃO EVENTO"
                if (descricaoEvento.Contains("Protocolo"))
                {
                    protocolo = descricaoEvento.Split(':')[1];
                }
                if (protocolo.Length != 16)
                {
                    Console.WriteLine($"Evento sem Protocolo para {chave}");
                }

                lst.Add(new Evento()
                {
                    Chave = chave,
                    // Vou ignorar os dados base da NF do evento
                    EventoTexto = csv["EVENTO"],
                    EventoData = csv.GetField<DateTime>("DATA/HORA EVENTO"),
                    EventoDescricao = descricaoEvento,
                    EventoMotivo = csv["MOTIVO EVENTO"],
                    Protocolo = protocolo,

                });
            }
            return lst.ToArray();
        }

        private static void gravarNFeBD(NotaFiscal[] nfs)
        {
            // TODO
        }
        private static void gravaEventosBD(Evento[] eventos)
        {
            // TODO
        }


        /* -- Auxiliares -- */
        private static string geraUrlDownload(int ano, int mes)
        {
            return $"http://www.portaltransparencia.gov.br/download-de-dados/notas-fiscais/{ano}{mes:00}";
        }
        /* -- Models -- */
        public class NotaFiscal
        {
            // Campos:
            // "CHAVE DE ACESSO";"MODELO";"SÉRIE";"NÚMERO";"NATUREZA DA OPERAÇÃO";
            // "DATA EMISSÃO";"EVENTO MAIS RECENTE";"DATA/HORA EVENTO MAIS RECENTE";
            // "CPF/CNPJ Emitente";"RAZÃO SOCIAL EMITENTE";"INSCRIÇÃO ESTADUAL EMITENTE";
            // "UF EMITENTE";"MUNICÍPIO EMITENTE";"CNPJ DESTINATÁRIO";"NOME DESTINATÁRIO";
            // "UF DESTINATÁRIO";"INDICADOR IE DESTINATÁRIO";"DESTINO DA OPERAÇÃO";
            // "CONSUMIDOR FINAL";"PRESENÇA DO COMPRADOR";"VALOR NOTA FISCAL"

            // A Chave de acesso é única 
            // O par (Série/Número) é única por CNPJ do Emitente (Emitente_Documento)

            public string Chave { get; set; }
            // 55: NFe, 65: NFCe, 59: SAT (SP)
            public int Modelo { get; set; }
            public int Serie { get; set; }
            public int Numero { get; set; }

            // Natureza é irrelevante, é texto livre e o software emissor pode fazer o que quiser
            // Talvez seja útil com a implementação de um bom (complexo) parser textual
            // Que coloque o texto como um Enum
            // Na forma textual, ocupará bastante banco e não terá utilidade em processos de pesquisa em massa de dados
            public string Natureza { get; set; }
            public DateTime DataEmissao { get; set; }
            // Evento mais recente não é relevante aqui, todos os eventos são catalogados
            public string EventoMaisRecente { get; set; }
            public DateTime DataEventoMaisRecente { get; set; }
            public string Emitente_Documento { get; set; }
            public string Emitente_Razao { get; set; }
            public string Emitente_IE { get; set; }
            public string Emitente_UF { get; set; }
            public string Emitente_Municipio { get; set; }

            public string Destinatario_Documento { get; set; }
            public string Destinatario_Nome { get; set; }
            public string Destinatario_UF { get; set; }
            public string Destinatario_IndicadorIE { get; set; }
            public int DestinoOperacao { get; set; }
            public int ConsumidorFinal { get; set; }
            public int IndicadorPresenca { get; set; }
            public decimal ValorOperacao { get; set; }

            public Item[] ItensNota { get; set; }
        }
        public class Item
        {
            // Campos:
            // "CHAVE DE ACESSO";"MODELO";"SÉRIE";"NÚMERO";"NATUREZA DA OPERAÇÃO";"DATA EMISSÃO";
            // "CPF/CNPJ Emitente";"RAZÃO SOCIAL EMITENTE";"INSCRIÇÃO ESTADUAL EMITENTE";
            // "UF EMITENTE";"MUNICÍPIO EMITENTE";"CNPJ DESTINATÁRIO";"NOME DESTINATÁRIO";"UF DESTINATÁRIO";
            // "INDICADOR IE DESTINATÁRIO";"DESTINO DA OPERAÇÃO";"CONSUMIDOR FINAL";"PRESENÇA DO COMPRADOR";
            // "NÚMERO PRODUTO";"DESCRIÇÃO DO PRODUTO/SERVIÇO";"CÓDIGO NCM/SH";"NCM/SH (TIPO DE PRODUTO)";
            // "CFOP";"QUANTIDADE";"UNIDADE";"VALOR UNITÁRIO";"VALOR TOTAL"

            // O par (Chave/Indice) é único
            // Acredito que o [PK da NF]+[Indice] seja um bom Key

            public string Chave { get; set; }
            // Todos os dados da NFe base são irrelevantes, não vou ler para não gastar memória na leitura
            // Vou ignorar tudo até chegar na parte do produto
            public int Indice { get; set; } // Idx é base 1, não base 0
            public string NomeProduto { get; set; }
            public string NCM { get; set; }
            // Significado do NCM é irrelevante
            // Não vou ler o "NCM/SH (TIPO DE PRODUTO)"
            public int CFOP { get; set; }
            public decimal Quantidade { get; set; } // Não sei se é quantidade tributada ou quantidade comerciada
            public string Unidade { get; set; }
            public decimal ValorUnitario { get; set; }
            // Se o valor for o vProd, ele já inclui descontos e acréscimos
            // > vSeg, vFrete, vDesc, vOutro
            public decimal ValorTotal { get; set; }

        }
        public class Evento
        {
            // "CHAVE DE ACESSO";"MODELO";"SÉRIE";"NÚMERO";"NATUREZA DA OPERAÇÃO";"DATA EMISSÃO";
            // "EVENTO";"DATA/HORA EVENTO";"DESCRIÇÃO EVENTO";"MOTIVO EVENTO"
            public string Chave { get; set; }
            // Não precisa os dados da NF de novo
            /*
                public int Modelo { get; set; }
                public int Serie { get; set; }
                public int Numero { get; set; }
                public string Natureza { get; set; }
                public DateTime DataEmissao { get; set; }
            */
            public string EventoTexto { get; set; }
            public DateTime EventoData { get; set; }
            public string EventoDescricao { get; set; }
            public string EventoMotivo { get; set; }
            // Protocolo é um bom campo-chave porém não tem campo próprio
            public string Protocolo { get; set; }
        }
    }
}
