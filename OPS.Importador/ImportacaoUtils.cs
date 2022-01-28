using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace OPS.Importador
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
				thumbnail = img.GetThumbnailImage(width, height, () => false, IntPtr.Zero);
			}

			thumbnail.Save(sourcePath.Replace(".jpg", "_" + width + "x" + height + ".jpg"), System.Drawing.Imaging.ImageFormat.Jpeg);
		}

		//public static List<int> ReadPdfFile(string fileName, String searthText)
		//{
		//    List<int> pages = new List<int>();
		//    if (File.Exists(fileName))
		//    {
		//        PdfReader pdfReader = new PdfReader(fileName);
		//        for (int page = 1; page <= pdfReader.NumberOfPages; page++)
		//        {
		//            ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();

		//            string currentPageText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);
		//            if (currentPageText.Contains(searthText))
		//            {
		//                pages.Add(page);
		//            }
		//        }
		//        pdfReader.Close();
		//    }
		//    return pages;
		//}
	}
}
