using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using AngleSharp;
using DDDN.OdtToHtml;
using Microsoft.Extensions.Logging;
using OPS.Core.Enumerators;
using OPS.Core.Utilities;
using OPS.Importador.Comum.Despesa;
using OPS.Importador.Comum.Utilities;
using RestSharp;

namespace OPS.Importador.Assembleias.Roraima
{
    public class ImportadorDespesasRoraima : ImportadorDespesasRestApiAnual
    {
        private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

        public ImportadorDespesasRoraima(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            config = new ImportadorCotaParlamentarBaseConfig()
            {
                BaseAddress = "https://transparencia.al.rr.leg.br/execucao-orcamentaria-e-financeira/verbas-indenizatorias-de-gabinete/",
                Estado = Estados.Roraima,
                ChaveImportacao = ChaveDespesaTemp.NomeCivil
            };
        }

        public override async Task ImportarDespesas(IBrowsingContext context, int ano)
        {
            if (ano <= 2025)
            {
             await   ImportarDespesas2025Antes(context, ano);
            }

            if (ano >= 2025)
            {
              await  ImportarDespesas2025Depois(context, ano);
            }
        }

        public async Task ImportarDespesas2025Depois(IBrowsingContext context, int ano)
        {
            var url = "https://transparencia.al.rr.leg.br/execucao-orcamentaria-e-financeira/verbas-indenizatorias-a-partir-de-set25/";
            var document = await context.OpenAsyncAutoRetry(url);
            var htmlContent = document.DocumentElement.OuterHtml;

            // var wpdf_obj = {"no_file_found_msg":"Nada foi encontrado com esse nome.","no_file_found_image":"https://transparencia.al.rr.leg.br/wp-content/plugins/wp-display-files/assets/images/no-file-found.png","image_formats":["jpg","jpeg","gif","png"],"wpdf_ajax_url":"https://transparencia.al.rr.leg.br/wp-admin/admin-ajax.php","wpdf_nonce":"c1b90c242d"};
            var wpdfNonce = htmlContent
                .Split(new[] { "\"wpdf_nonce\":\"" }, StringSplitOptions.None)[1]
                .Split('"')[0];


            var client = new RestClient("https://transparencia.al.rr.leg.br/");
            var requestAno = new RestRequest("wp-admin/admin-ajax.php", Method.Post);
            requestAno.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            requestAno.AddHeader("Cookie", document.Cookie);
            requestAno.AddHeader("Host", "transparencia.al.rr.leg.br");
            requestAno.AddHeader("Referer", "https://transparencia.al.rr.leg.br/execucao-orcamentaria-e-financeira/verbas-indenizatorias-a-partir-de-set25/");
            requestAno.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:147.0) Gecko/20100101 Firefox/147.0");
            requestAno.AddHeader("X-Requested-With", "XMLHttpRequest");
            requestAno.AddParameter("application/x-www-form-urlencoded", $"action=wpdf_load_folder_data&folder_name=Ano+{ano}&folder_path=Ano+{ano}%2F&sid=9&wpdf_nonce={wpdfNonce}", ParameterType.RequestBody);
            RestResponse responseAno = await client.ExecuteAsync(requestAno);

            var content = JsonSerializer.Deserialize<DespesasAno2025Mais>(responseAno.Content).Content;
            var documentInner = await context.OpenAsync(req => req.Content(content));

            var linksMeses = documentInner
                .QuerySelectorAll("li")
                .Select(li => new
                {
                    FolderName = li.GetAttribute("data-name"), // 09-Setembro de 2025
                    FolderPath = li.GetAttribute("data-path"), // Ano 2025/09-Setembro de 2025/
                })
                .ToList();

            foreach (var linkMes in linksMeses)
            {
                var requestMes = new RestRequest("wp-admin/admin-ajax.php", Method.Post);
                requestMes.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
                requestMes.AddHeader("Cookie", document.Cookie);
                requestMes.AddHeader("Host", "transparencia.al.rr.leg.br");
                requestMes.AddHeader("Referer", "https://transparencia.al.rr.leg.br/execucao-orcamentaria-e-financeira/verbas-indenizatorias-a-partir-de-set25/");
                requestMes.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:147.0) Gecko/20100101 Firefox/147.0");
                requestMes.AddHeader("X-Requested-With", "XMLHttpRequest");
                requestMes.AddParameter("application/x-www-form-urlencoded",
                    $"action=wpdf_load_folder_data&folder_name={linkMes.FolderName}&folder_path={linkMes.FolderPath}&sid=9&wpdf_nonce={wpdfNonce}",
                    ParameterType.RequestBody);

                RestResponse responseMes = await client.ExecuteAsync(requestMes);

                var contentMes = JsonSerializer.Deserialize<DespesasAno2025Mais>(responseMes.Content).Content;
                var documentArquivos = await context.OpenAsync(req => req.Content(contentMes));

                var links = documentArquivos
                    .QuerySelectorAll("a.download_btn")
                    .Select(a => new
                    {
                        Href = a.GetAttribute("href"),
                        FileName = a.Closest("li")?.GetAttribute("data-name"), // Dep. Angela Aguida Portela Alves - Setembro 2025.odt
                    })
                    .ToList();

                foreach (var linkArquivo in links)
                {

                    if (linkArquivo.FileName.EndsWith(".pdf"))
                    {
                        var odtFileName = linkArquivo.FileName.Replace(".pdf", ".odt");
                        if (links.Any(x => x.FileName == linkArquivo.FileName)) continue;

                        logger.LogError("Versão ODT do arquivo {Arquivo} não foi localizada", linkArquivo.FileName);
                        continue;
                    }

                    var tituloPartes = linkArquivo.FileName.Split(new[] { '-' });
                    var nomeParlamentar = tituloPartes[0].Replace("Dep.", "").Trim();
                    var mes = ResolveMes(tituloPartes[1].Trim());

                    using (logger.BeginScope(new Dictionary<string, object> { ["Mes"] = mes, ["Parlamentar"] = nomeParlamentar, ["Arquivo"] = $"CLRR-{ano}-{mes}-{nomeParlamentar}.odt" }))
                    {
                       await ImportarDespesasArquivo(ano, mes, nomeParlamentar, linkArquivo.Href);
                    }
                }
            }
        }

        // {"status":"1","inner_content":"<li class=\"file_record\" data-name=\"Dep. Angela Aguida Portela Alves - Setembro 2025.odt\" data-file-type=\"odt\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_odt_icon\">Dep. Angela Aguida Portela Alves - Setembro 2025.odt<\/span><\/div><div class=\"file_size\">18.21 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=09712e9e-8535-487a-9165-02a63cc07ef1-1410\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Angela Aguida Portela Alves - Setembro 2025.pdf\" data-file-type=\"pdf\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_pdf_icon\">Dep. Angela Aguida Portela Alves - Setembro 2025.pdf<\/span><\/div><div class=\"file_size\">437.21 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=05d59fe5-996a-45c6-8d81-00d334c06424-1405\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Antonio Eduardo Filho - Setembro 2025.odt\" data-file-type=\"odt\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_odt_icon\">Dep. Antonio Eduardo Filho - Setembro 2025.odt<\/span><\/div><div class=\"file_size\">18.12 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=13a34e5e-43f7-432e-a07a-29cbd02da5c2-1406\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Antonio Eduardo Filho - Setembro 2025.pdf\" data-file-type=\"pdf\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_pdf_icon\">Dep. Antonio Eduardo Filho - Setembro 2025.pdf<\/span><\/div><div class=\"file_size\">438.06 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=7abff827-dbba-418f-95ae-1674cffbf113-1417\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Armando do Carmo Ara\u00fajo - Setembro 2025.odt\" data-file-type=\"odt\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_odt_icon\">Dep. Armando do Carmo Ara\u00fajo - Setembro 2025.odt<\/span><\/div><div class=\"file_size\">18.4 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=ee1724cb-352d-4b20-a690-26c0d4cd9b1b-1419\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Armando do Carmo Ara\u00fajo - Setembro 2025.pdf\" data-file-type=\"pdf\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_pdf_icon\">Dep. Armando do Carmo Ara\u00fajo - Setembro 2025.pdf<\/span><\/div><div class=\"file_size\">437.81 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=752e383e-5d2a-4866-b692-2552562ba69f-1400\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Catarina de Lima Guerra da Silva - Setembro 2025.odt\" data-file-type=\"odt\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_odt_icon\">Dep. Catarina de Lima Guerra da Silva - Setembro 2025.odt<\/span><\/div><div class=\"file_size\">18.32 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=4dfda474-31d0-4522-b47f-1d2a78a97abe-1408\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Catarina de Lima Guerra da Silva - Setembro 2025.pdf\" data-file-type=\"pdf\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_pdf_icon\">Dep. Catarina de Lima Guerra da Silva - Setembro 2025.pdf<\/span><\/div><div class=\"file_size\">437.22 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=b067e204-bc78-419d-8f44-1cedf28d023b-1409\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Eder Barcelos Brandao - Setembro 2025.odt\" data-file-type=\"odt\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_odt_icon\">Dep. Eder Barcelos Brandao - Setembro 2025.odt<\/span><\/div><div class=\"file_size\">18.27 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=114e663c-c6c4-44d8-b1ee-0f846d134725-1402\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Eder Barcelos Brandao - Setembro 2025.pdf\" data-file-type=\"pdf\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_pdf_icon\">Dep. Eder Barcelos Brandao - Setembro 2025.pdf<\/span><\/div><div class=\"file_size\">437.18 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=e2d19b58-7080-4b1e-8a29-29766ec959db-1401\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Francisca Aurelina Medeiros Lima -Setembro 2025.odt\" data-file-type=\"odt\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_odt_icon\">Dep. Francisca Aurelina Medeiros Lima -Setembro 2025.odt<\/span><\/div><div class=\"file_size\">18.18 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=aa8eaea3-59fc-4453-a580-080ebd695fe8-1404\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Francisca Aurelina Medeiros Lima -Setembro 2025.pdf\" data-file-type=\"pdf\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_pdf_icon\">Dep. Francisca Aurelina Medeiros Lima -Setembro 2025.pdf<\/span><\/div><div class=\"file_size\">437.64 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=2144ff25-8bd4-4bd1-abd7-149f838c2ea8-1412\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Francisco Claudio Linhares de S\u00e1 Filho - Setembro 2025.odt\" data-file-type=\"odt\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_odt_icon\">Dep. Francisco Claudio Linhares de S\u00e1 Filho - Setembro 2025.odt<\/span><\/div><div class=\"file_size\">18.72 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=f5c12423-e7c5-4a12-b758-2ac8cd2f6d16-1399\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Francisco Claudio Linhares de S\u00e1 Filho - Setembro 2025.pdf\" data-file-type=\"pdf\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_pdf_icon\">Dep. Francisco Claudio Linhares de S\u00e1 Filho - Setembro 2025.pdf<\/span><\/div><div class=\"file_size\">438.3 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=a2b3684c-c4df-4897-ace1-15bdeaef0959-1411\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Francisco Mozart Holanda Pinheiro - Setembro 2025.odt\" data-file-type=\"odt\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_odt_icon\">Dep. Francisco Mozart Holanda Pinheiro - Setembro 2025.odt<\/span><\/div><div class=\"file_size\">18.33 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=f8fc47ca-5c58-4868-8e2b-02b6dadf0c11-1397\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Francisco Mozart Holanda Pinheiro - Setembro 2025.pdf\" data-file-type=\"pdf\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_pdf_icon\">Dep. Francisco Mozart Holanda Pinheiro - Setembro 2025.pdf<\/span><\/div><div class=\"file_size\">438.06 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=228f1f56-4875-4293-83f0-121d9ff28b64-1403\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Francisco dos Santos Sampaio - Setembro 2025.odt\" data-file-type=\"odt\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_odt_icon\">Dep. Francisco dos Santos Sampaio - Setembro 2025.odt<\/span><\/div><div class=\"file_size\">18.4 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=c2ce4e92-3ed6-461a-96b3-00336531be29-1413\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Francisco dos Santos Sampaio - Setembro 2025.pdf\" data-file-type=\"pdf\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_pdf_icon\">Dep. Francisco dos Santos Sampaio - Setembro 2025.pdf<\/span><\/div><div class=\"file_size\">437.62 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=99eea2c1-0f68-4837-ae34-0e0777ad8528-1416\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Gabriel Figueira Pessoa Picanco - Setembro 2025.odt\" data-file-type=\"odt\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_odt_icon\">Dep. Gabriel Figueira Pessoa Picanco - Setembro 2025.odt<\/span><\/div><div class=\"file_size\">18.36 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=6ee8201a-79d0-4aff-b8e6-1423a1b2de65-1414\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Gabriel Figueira Pessoa Picanco - Setembro 2025.pdf\" data-file-type=\"pdf\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_pdf_icon\">Dep. Gabriel Figueira Pessoa Picanco - Setembro 2025.pdf<\/span><\/div><div class=\"file_size\">437.57 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=b4c2542c-d3a3-4488-a77a-1e6f7022f3e4-1415\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Gerson Chagas - Setembro 2025.odt\" data-file-type=\"odt\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_odt_icon\">Dep. Gerson Chagas - Setembro 2025.odt<\/span><\/div><div class=\"file_size\">18.21 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=a97965f5-dc96-4195-9c3c-128eabc91e30-1396\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Gerson Chagas - Setembro 2025.pdf\" data-file-type=\"pdf\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_pdf_icon\">Dep. Gerson Chagas - Setembro 2025.pdf<\/span><\/div><div class=\"file_size\">437.7 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=ce1baa69-5f0b-4726-b67d-27e2e790f10c-1395\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Idazio Chagas de Lima - Setembro 2025.odt\" data-file-type=\"odt\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_odt_icon\">Dep. Idazio Chagas de Lima - Setembro 2025.odt<\/span><\/div><div class=\"file_size\">18.22 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=eef3561e-68e0-4b34-895f-22593d29a09b-1398\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Idazio Chagas de Lima - Setembro 2025.pdf\" data-file-type=\"pdf\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_pdf_icon\">Dep. Idazio Chagas de Lima - Setembro 2025.pdf<\/span><\/div><div class=\"file_size\">437.5 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=4273e730-48b7-46f3-8afe-26feb44b4845-1418\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Isamar Pessoa Ramalho J\u00fanior - Setembro 2025.odt\" data-file-type=\"odt\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_odt_icon\">Dep. Isamar Pessoa Ramalho J\u00fanior - Setembro 2025.odt<\/span><\/div><div class=\"file_size\">18.21 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=f9882ef6-7199-45d8-bbba-00616005cc19-1407\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Isamar Pessoa Ramalho J\u00fanior - Setembro 2025.pdf\" data-file-type=\"pdf\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_pdf_icon\">Dep. Isamar Pessoa Ramalho J\u00fanior - Setembro 2025.pdf<\/span><\/div><div class=\"file_size\">438.23 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=82e8d20b-de2f-4739-9bf2-1789b45a5aa6-1386\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Joilma Teodora de Ara\u00fajo Silva - Setembro 2025.odt\" data-file-type=\"odt\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_odt_icon\">Dep. Joilma Teodora de Ara\u00fajo Silva - Setembro 2025.odt<\/span><\/div><div class=\"file_size\">18.51 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=580ba948-57a3-4ad1-8d12-154a93c7ca31-1388\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Joilma Teodora de Ara\u00fajo Silva - Setembro 2025.pdf\" data-file-type=\"pdf\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_pdf_icon\">Dep. Joilma Teodora de Ara\u00fajo Silva - Setembro 2025.pdf<\/span><\/div><div class=\"file_size\">437.85 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=e7f571ed-b925-43f9-8e45-07561ba92adb-1379\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Jorge Everton Barreto Guimaraes - Setembro 2025.odt\" data-file-type=\"odt\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_odt_icon\">Dep. Jorge Everton Barreto Guimaraes - Setembro 2025.odt<\/span><\/div><div class=\"file_size\">18.3 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=f406d8e6-e2c9-4be4-9c8e-1ef19fadfeef-1377\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Jorge Everton Barreto Guimaraes - Setembro 2025.pdf\" data-file-type=\"pdf\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_pdf_icon\">Dep. Jorge Everton Barreto Guimaraes - Setembro 2025.pdf<\/span><\/div><div class=\"file_size\">437.67 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=5d0c3bd6-ef49-42e2-aea4-1d67fe60fb4a-1394\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Jos\u00e9 Hamilton Gomes Loureiro Neto - Setembro 2025.odt\" data-file-type=\"odt\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_odt_icon\">Dep. Jos\u00e9 Hamilton Gomes Loureiro Neto - Setembro 2025.odt<\/span><\/div><div class=\"file_size\">18.3 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=a61fad12-3682-470c-b6f1-1a7b9811ce71-1373\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Jos\u00e9 Hamilton Gomes Loureiro Neto - Setembro 2025.pdf\" data-file-type=\"pdf\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_pdf_icon\">Dep. Jos\u00e9 Hamilton Gomes Loureiro Neto - Setembro 2025.pdf<\/span><\/div><div class=\"file_size\">438.09 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=3a3c9c3b-00ff-4420-885f-1a3d8509d772-1392\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Lucas Souza Gon\u00e7alves - Setembro 2025.odt\" data-file-type=\"odt\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_odt_icon\">Dep. Lucas Souza Gon\u00e7alves - Setembro 2025.odt<\/span><\/div><div class=\"file_size\">18.3 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=9231d226-b6a5-43bd-8fa6-1cf52d7e9131-1387\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Lucas Souza Gon\u00e7alves - Setembro 2025.pdf\" data-file-type=\"pdf\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_pdf_icon\">Dep. Lucas Souza Gon\u00e7alves - Setembro 2025.pdf<\/span><\/div><div class=\"file_size\">437.22 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=be952661-f322-496d-9dab-01807b838db8-1391\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Marcelo Mota de Macedo - Setembro 2025.odt\" data-file-type=\"odt\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_odt_icon\">Dep. Marcelo Mota de Macedo - Setembro 2025.odt<\/span><\/div><div class=\"file_size\">18.11 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=f593a50d-d20a-4507-8b71-03e087947e93-1372\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Marcelo Mota de Macedo - Setembro 2025.pdf\" data-file-type=\"pdf\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_pdf_icon\">Dep. Marcelo Mota de Macedo - Setembro 2025.pdf<\/span><\/div><div class=\"file_size\">437.16 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=78850df6-c8c3-46ef-8fc7-1604e99e9715-1374\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Marcos Jorge de Lima - Setembro 2025.odt\" data-file-type=\"odt\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_odt_icon\">Dep. Marcos Jorge de Lima - Setembro 2025.odt<\/span><\/div><div class=\"file_size\">18.38 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=032ddd39-ef32-413a-9bc9-0be5878d347c-1376\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Marcos Jorge de Lima - Setembro 2025.pdf\" data-file-type=\"pdf\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_pdf_icon\">Dep. Marcos Jorge de Lima - Setembro 2025.pdf<\/span><\/div><div class=\"file_size\">437.62 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=db72c1e8-58bf-4029-a03f-06690267b9c4-1375\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Meton Melo Maciel - Setembro 2025.odt\" data-file-type=\"odt\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_odt_icon\">Dep. Meton Melo Maciel - Setembro 2025.odt<\/span><\/div><div class=\"file_size\">18.39 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=d4aee639-683b-4a78-9dea-097d2ec7f9ed-1389\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Meton Melo Maciel - Setembro 2025.pdf\" data-file-type=\"pdf\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_pdf_icon\">Dep. Meton Melo Maciel - Setembro 2025.pdf<\/span><\/div><div class=\"file_size\">437.25 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=8220ef29-702f-4a9b-a95d-240c57d1a438-1393\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. M\u00e1rcio Agra Belota - Setembro 2025.odt\" data-file-type=\"odt\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_odt_icon\">Dep. M\u00e1rcio Agra Belota - Setembro 2025.odt<\/span><\/div><div class=\"file_size\">18.3 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=0d119910-8fc3-46d3-ba59-1ce19c7f870e-1380\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. M\u00e1rcio Agra Belota - Setembro 2025.pdf\" data-file-type=\"pdf\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_pdf_icon\">Dep. M\u00e1rcio Agra Belota - Setembro 2025.pdf<\/span><\/div><div class=\"file_size\">437.15 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=b505d6e0-848c-4aeb-b890-129097f13fc5-1378\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Renato de Souza Silva - Setembro 2025.odt\" data-file-type=\"odt\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_odt_icon\">Dep. Renato de Souza Silva - Setembro 2025.odt<\/span><\/div><div class=\"file_size\">18.14 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=b8efb623-ac8e-435c-8b67-1fb30ccfc456-1384\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Renato de Souza Silva - Setembro 2025.pdf\" data-file-type=\"pdf\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_pdf_icon\">Dep. Renato de Souza Silva - Setembro 2025.pdf<\/span><\/div><div class=\"file_size\">437.19 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=7eb8043f-d1c4-454f-9ddd-22a9eb912517-1383\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. R\u00e1rison Francisco Rodrigues Barbosa - Setembro 2025.odt\" data-file-type=\"odt\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_odt_icon\">Dep. R\u00e1rison Francisco Rodrigues Barbosa - Setembro 2025.odt<\/span><\/div><div class=\"file_size\">18.51 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=8fb6afa3-d992-4023-8b46-16c9bf6b625f-1385\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. R\u00e1rison Francisco Rodrigues Barbosa - Setembro 2025.pdf\" data-file-type=\"pdf\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_pdf_icon\">Dep. R\u00e1rison Francisco Rodrigues Barbosa - Setembro 2025.pdf<\/span><\/div><div class=\"file_size\">437.83 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=c16cba27-ab5f-468c-b87d-2059b8b52ec5-1382\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Tayla Ribeiro Peres Silva - Setembro 2025.odt\" data-file-type=\"odt\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_odt_icon\">Dep. Tayla Ribeiro Peres Silva - Setembro 2025.odt<\/span><\/div><div class=\"file_size\">18.39 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=e40c50d1-6a53-46fe-a106-01781ee457ef-1381\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li><li class=\"file_record\" data-name=\"Dep. Tayla Ribeiro Peres Silva - Setembro 2025.pdf\" data-file-type=\"pdf\">\n\t\t\t\t\t\t<div class=\"df_list_row\"><div><span class=\"df_file_icon df_pdf_icon\">Dep. Tayla Ribeiro Peres Silva - Setembro 2025.pdf<\/span><\/div><div class=\"file_size\">437.94 KB<\/div><div><div class=\"action_list\"><div class=\"action_list_b\"><a href=\"https:\/\/transparencia.al.rr.leg.br\/?wpdf_download=true&#038;wpdf_id=27edaa45-59d3-445b-aa2d-08ec3ee5c13b-1390\" class=\"download_btn\"><\/a><\/div><\/div><\/div><\/div>\n\t\t\t\t\t<\/li>"}
        public class DespesasAno2025Mais
        {
            [JsonPropertyName("status")]
            public string Status { get; set; }

            [JsonPropertyName("inner_content")]
            public string Content { get; set; }
        }


        public async Task ImportarDespesas2025Antes(IBrowsingContext context, int ano)
        {
            var url = "https://transparencia.al.rr.leg.br/execucao-orcamentaria-e-financeira/verbas-indenizatorias-de-gabinete-ate-ago25/";
            var document = await context.OpenAsyncAutoRetry(url);

            var idElementoAno = document
                .QuerySelectorAll(".ui-tabs-nav li a")
                .FirstOrDefault(x => x.TextContent == ano.ToString());

            if (idElementoAno is null)
            {
                logger.LogWarning("Dados para o ano {Ano} ainda não disponiveis!", ano);
                return;
            }

            var meses = document.QuerySelector(idElementoAno.Attributes["href"].Value).QuerySelectorAll(".link-template-default .card-body");
            foreach (var item in meses)
            {
                var tipoAquivo = item.QuerySelector("img.wpdm_icon").Attributes["src"].Value;
                if (!tipoAquivo.EndsWith("doc.svg") && !tipoAquivo.EndsWith("docx.svg")) continue;

                var urlPdf = item.QuerySelector("a.wpdm-download-link").Attributes["data-downloadurl"].Value;
                if (urlPdf.Contains("-2/") && !tipoAquivo.EndsWith("docx.svg")) continue;
                var titulo = item.QuerySelector(".package-title a").TextContent.Replace("Dep.", "").Trim();
                if (titulo == "Renato de Souza Silva Junho - 2024")
                    titulo = "Renato de Souza Silva - Junho 2024";
                else if (titulo == "Renato de Souza Silva Julho - 2024")
                    titulo = "Renato de Souza Silva - Julho 2024";

                var tituloPartes = titulo.Split(new[] { '-', '–' });
                var nomeParlamentar = tituloPartes[0].Trim();

                var mes = ResolveMes(tituloPartes[1].Trim());

                using (logger.BeginScope(new Dictionary<string, object> { ["Mes"] = mes, ["Parlamentar"] = nomeParlamentar, ["Arquivo"] = $"CLRR-{ano}-{mes}-{nomeParlamentar}.odt" }))
                {
                  await  ImportarDespesasArquivo(ano, mes, nomeParlamentar, urlPdf);
                }
            }
        }

        private async Task ImportarDespesasArquivo(int ano, int mes, string nomeCivilParlamentar, string urlPdf)
        {
            var filename = Path.Combine(tempFolder, $"CLRR-{ano}-{mes}-{nomeCivilParlamentar}.odt");
            var filenameBackup = filename.Replace(".odt", "_bkp.odt"); // Arquivo de backup é criado pelo FileManager ao baixar o arquivo

            await fileManager.BaixarArquivo(dbContext, urlPdf, filename, config.Estado);

            // structure will contain all the data returned form the ODT to HTML conversion
            OdtConvertedData convertedData = null;

            try
            {
                // open the ODT document on the file system and call the Convert method to convert the document to HTML
                using (IOdtFile odtFile = new OdtFile(filename))
                    convertedData = new OdtConvert().Convert(odtFile, new OdtConvertSettings());

            }
            catch (Exception ex)
            {
                fileManager.MoverArquivoComErro(filename);

                // Rollback bpk file if exists
                if (File.Exists(filenameBackup))
                {
                    // Arquivo pode estar corrompido ou ser um PDF importado como ODT. Tentar importar o backup.
                    logger.LogWarning(ex, "Importando arquivo de backup para {Mes:00}/{Ano} do Parlamentar {Parlamentar}. Url: {UrlPdf}", mes, ano, nomeCivilParlamentar, urlPdf);
                    File.Move(filenameBackup, filename, true);

                    try
                    {
                        // open the ODT document on the file system and call the Convert method to convert the document to HTML
                        using (IOdtFile odtFile = new OdtFile(filename))
                            convertedData = new OdtConvert().Convert(odtFile, new OdtConvertSettings());
                    }
                    catch (Exception ex1)
                    {
                        logger.LogError(ex1, "Erro ao procesar arquivo de {Mes:00}/{Ano} do Parlamentar {Parlamentar}. Url: {UrlPdf}", mes, ano, nomeCivilParlamentar, urlPdf);
                        return;
                    }
                }
                else
                {
                    logger.LogError(ex, "Erro ao procesar arquivo de {Mes:00}/{Ano} do Parlamentar {Parlamentar}. Url: {UrlPdf}", mes, ano, nomeCivilParlamentar, urlPdf);
                    return;
                }
            }

            var context = httpClientResilient.CreateAngleSharpContext();
            var document = await context.OpenAsync(req => req.Content(convertedData.Html.ForceWindows1252ToUtf8Encoding()));

            var linhas = document.QuerySelectorAll("table tr");

            decimal valorTotalDeputado = 0;
            int despesasIncluidas = 0;
            bool totalValidado = false;

            foreach (var row in linhas)
            {
                var colunas = row.QuerySelectorAll("td");
                if (colunas.Length == 0) continue;

                var coluna1 = colunas[0].TextContent.Replace("\u00A0", " ").Trim();

                if (coluna1.StartsWith("Parlamentar:"))
                {
                    var colunaParlametar = coluna1.Split(":")[1].Replace("\u00A0", " ").Trim();
                    if (!Utils.RemoveAccents(nomeCivilParlamentar).Equals(Utils.RemoveAccents(colunaParlametar), StringComparison.InvariantCultureIgnoreCase))
                    {
                        logger.LogWarning("Parlamentar divergente! Esperado: '{ParlamentarEsperado}'; Recebido: '{ParlamentarRecebido}'", nomeCivilParlamentar, colunaParlametar.ToTitleCase());
                        nomeCivilParlamentar = colunaParlametar.ToTitleCase();

                    }

                    if (!string.IsNullOrEmpty(colunas[1].TextContent)) // CLRR-2023-1-Meton Melo Maciel.odt
                    {
                        var colunaMes = colunas[1].TextContent.Split(":").Last().Replace("\u00A0", " ").Trim();
                        if (ResolveMes(colunaMes) != mes)
                            logger.LogError("Mês Divergente! Esperado: '{MesEsperado}'; Recebido: '{MesRecebido}'", mes, colunaMes);
                    }

                    continue;
                }

                if (coluna1.StartsWith("SOMA PARCIAL"))
                {
                    totalValidado = true;
                    var valorTemp = colunas[1].TextContent;
                    if (string.IsNullOrEmpty(valorTemp))
                        valorTemp = colunas[2].TextContent; // CLRR-2023-1-Meton Melo Maciel.odt

                    var valorTotalArquivo = Convert.ToDecimal(valorTemp.Replace("R$", "").Trim(), cultureInfo);
                    ValidaValorTotal(filename, valorTotalArquivo, valorTotalDeputado, despesasIncluidas);

                    break;
                }

                if (colunas.Length < 3) continue;
                //if (colunas.Length != 3)
                //{
                //    foreach (var coluna in row.QuerySelectorAll("td"))
                //    {
                //        Console.Write(coluna.TextContent.Trim());
                //        Console.Write(" | ");
                //    }

                //    Console.WriteLine();
                //}

                var item = colunas[0].TextContent.Trim();
                var tipoDespesa = colunas[1].TextContent.Trim().Replace("\u00A0", " ");
                var valor = colunas[2].TextContent.Trim();
                if (string.IsNullOrEmpty(valor))
                {
                    if (colunas.Length > 3)
                        valor = colunas[3].TextContent.Trim(); // CLRR-2023-10-Meton Melo Maciel.odt
                    else
                        continue;
                }

                if (item.Length != 3 || string.IsNullOrEmpty(valor) || valor == "VALOR R$" || valor == "c") continue;

                switch (item)
                {
                    case "6.1": // Geral (meios de comunicação, blog, influenciadores digitais e similares)
                        tipoDespesa = "Divulgação de Atividade Parlamentar";
                        break;
                    case "8.1": // Geral
                        tipoDespesa = "Pesquisas Sócio-Ecomômicas";
                        break;
                }

                var nomeCivilParlamentarEncoded = nomeCivilParlamentar.ForceWindows1252ToLatin1Encoding();
                CamaraEstadualDespesaTemp despesaTemp = new CamaraEstadualDespesaTemp()
                {
                    Nome = nomeCivilParlamentarEncoded,
                    NomeCivil = nomeCivilParlamentarEncoded,
                    Ano = (short)ano,
                    Mes = (short)mes,
                    DataEmissao = new DateOnly(ano, mes, 1),
                    TipoDespesa = tipoDespesa,
                    Valor = Convert.ToDecimal(valor.Replace("R$", "").Trim(), cultureInfo),
                    Origem = urlPdf
                };

                //logger.LogWarning($"Inserindo Item {tipoDespesa} com valor: {despesaTemp.Valor}!");

                InserirDespesaTemp(despesaTemp);
                valorTotalDeputado += despesaTemp.Valor;
                despesasIncluidas++;
            }

            if (!totalValidado)
            {
                logger.LogError("Valor Não Validado: {ValorTotalCalculado}; Referencia: {Mes}/{Ano}; Parlamentar: {Parlamentar}; Arquivo: {FileName}",
                                valorTotalDeputado, mes, ano, nomeCivilParlamentar, filename);

                //foreach (var linha in linhas)
                //{
                //    foreach (var coluna in linha.QuerySelectorAll("td"))
                //    {
                //        Console.Write(coluna.TextContent.Trim());
                //        Console.Write(" | ");
                //    }

                //    Console.WriteLine();
                //}
            }
        }

        private int ResolveMes(string mes) => mes.Substring(0, 3).ToUpper() switch
        {
            "JAN" => 1,
            "FEV" => 2,
            "MAR" => 3,
            "ABR" => 4,
            "MAI" => 5,
            "JUN" => 6,
            "JUL" => 7,
            "AGO" => 8,
            "SET" => 9,
            "OUT" => 10,
            "NOV" => 11,
            "DEZ" => 12,
            _ => throw new ArgumentOutOfRangeException(nameof(mes), $"Mês invalido: {mes}"),
        };
    }
}
