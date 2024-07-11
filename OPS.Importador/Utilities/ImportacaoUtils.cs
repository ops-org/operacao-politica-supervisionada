using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using OPS.Core.Entity;

namespace OPS.Importador.Utilities
{
    public static class ImportacaoUtils
    {
        public static List<string> ParseCsvRowToList(string separator, string row)
        {
            try
            {
                return row.Substring(1, row.Length - 2).Split(new[] { separator }, StringSplitOptions.None).ToList();
            }
            catch
            {
                return null;
            }
        }

        public static void CreateImageThumbnail(string sourcePath, int width = 120, int height = 160)
        {
            Image thumbnail;
            using (var img = Image.FromFile(sourcePath))
            {
                thumbnail = img.GetThumbnailImage(width, height, () => false, nint.Zero);
            }

            thumbnail.Save(sourcePath.Replace(".jpg", "_" + width + "x" + height + ".jpg"), System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        public static void MapearRedeSocial(DeputadoEstadual deputado, IHtmlCollection<IElement> links)
        {
            //TODO: Criar colunas no banco
            foreach (var link in links)
            {
                var href = (link as IHtmlAnchorElement).Href;
                if (string.IsNullOrEmpty(href) || href.EndsWith(".com/")) continue;

                if (href.Contains("instagram.com/"))
                    deputado.Instagram = href;
                else if (href.Contains("facebook.com/"))
                    deputado.Facebook = href;
                else if (href.Contains("twitter.com/") || href.Contains("x.com/"))
                    deputado.Twitter = href;
                else if (href.Contains("youtube.com/"))
                    deputado.YouTube = href;
                else if (href.Contains("tiktok.com/"))
                    deputado.Tiktok = href;
                else if (href.Contains("@"))
                    deputado.Email = href.Replace("mailto:", "");
                else
                    deputado.Site = href;
            }
        }

        public static IEnumerable<string> ReadPdfFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                PdfReader pdfReader = new PdfReader(fileName);
                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();

                for (int page = 1; page <= pdfReader.NumberOfPages; page++)
                {
                    yield return PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);
                }
                pdfReader.Close();
            }
        }
    }
}
