﻿using System;
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
	}
}
