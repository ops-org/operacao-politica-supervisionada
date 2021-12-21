using System;
using System.IO;
using System.Net.Http;
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

            string url = geraUrlDownload(ano, mes);

            var response = client.GetAsync(url).Result;
            // Se não conseguiu fazer o download, retorna um HttpRequestException
            response.EnsureSuccessStatusCode();

            // Salva a stream
            string caminhoZip = Path.Combine(atualDir, nomeArquivoZip);
            using (var fs = new FileStream(caminhoZip, FileMode.CreateNew))
            {
                var ResponseTask = response.Content.CopyToAsync(fs);
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
                if (arquivoFiscal != null)  arquivoFiscal.Dispose();
                if (arquivoEventos != null) arquivoEventos.Dispose();
                if (arquivoItens != null)   arquivoItens.Dispose();
                return false;
            }

            


            arquivoFiscal.Dispose();
            arquivoEventos.Dispose();
            arquivoItens.Dispose();

            return true;
        }



        /* -- Auxiliares -- */
        private static string geraUrlDownload(int ano, int mes)
        {
            return $"http://www.portaltransparencia.gov.br/download-de-dados/notas-fiscais/{ano}{mes:00}";
        }


        public class NotaFiscal
        {
            // "CHAVE DE ACESSO";"MODELO";"SÉRIE";"NÚMERO";"NATUREZA DA OPERAÇÃO";
            // "DATA EMISSÃO";"EVENTO MAIS RECENTE";"DATA/HORA EVENTO MAIS RECENTE";
            // "CPF/CNPJ Emitente";"RAZÃO SOCIAL EMITENTE";"INSCRIÇÃO ESTADUAL EMITENTE";
            // "UF EMITENTE";"MUNICÍPIO EMITENTE";"CNPJ DESTINATÁRIO";"NOME DESTINATÁRIO";
            // "UF DESTINATÁRIO";"INDICADOR IE DESTINATÁRIO";"DESTINO DA OPERAÇÃO";
            // "CONSUMIDOR FINAL";"PRESENÇA DO COMPRADOR";"VALOR NOTA FISCAL"
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
            public DateTime DataEventoMaisRecente { get; set;}
            public string Emitente_Documento{ get; set; }
            public string Emitente_Razao { get; set; }
            public string Emitente_IE { get; set; }
            public string Emitente_UF { get; set; }
            public string Emitente_Municipio { get; set; }

            public string Destinatario_Documento { get; set; }
            public string Destinatario_Nome { get; set; }
            public string Destinatario_UF { get; set; }
            public string Destinatario_IndicadorIE{ get; set; }
            public int DestinoOperacao { get; set; }
            public int ConsumidorFinal { get; set; }
            public int IndicadorPresenca { get; set; }
            public decimal ValorOperacao { get; set; }
        }
        public class Item
        {
        }
        public class Evento
        {
            // "CHAVE DE ACESSO";"MODELO";"SÉRIE";"NÚMERO";"NATUREZA DA OPERAÇÃO";"DATA EMISSÃO";"EVENTO";"DATA/HORA EVENTO";"DESCRIÇÃO EVENTO";"MOTIVO EVENTO"
            public string Chave { get; set; }
            // 55: NFe, 65: NFCe, 59: SAT (SP)
            public int Modelo { get; set; }
            public int Serie { get; set; }
            public int Numero { get; set; }
            public string Natureza { get; set; }
            public DateTime DataEmissao { get; set; }

            public string EventoTexto { get; set; }
            public DateTime EventoData { get; set; }
            public string EventoDescricao { get; set; }
            public string EventoMotivo { get; set; }
        }
    }
}
