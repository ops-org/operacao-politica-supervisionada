using AForge.Imaging.Filters;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using Tesseract;

public static class Captcha
{
    private static Random aleatorio = new Random();
    
    public static Bitmap Limpar(Image img)
    {
        Bitmap imagem = new Bitmap(img);
        Bitmap newBitmap = new Bitmap(imagem.Width, imagem.Height);
        Graphics g = Graphics.FromImage(newBitmap);
        g.FillRectangle(Brushes.White,0,0,imagem.Width,imagem.Height);
        ColorMatrix colorMatrix = new ColorMatrix(
        new float[][] 
        {
             new float[] {.3f, .3f, .3f, 0, 0},
             new float[] {.59f, .59f, .59f, 0, 0},
             new float[] {.11f, .11f, .11f, 0, 0},
             new float[] {0, 0, 0, 1, 0},
             new float[] {0, 0, 0, 0, 1}
        });
        ImageAttributes attributes = new ImageAttributes();
        attributes.SetColorMatrix(colorMatrix);
        g.DrawImage(imagem, new Rectangle(0, (int)(imagem.Height * 0.1), imagem.Width, (int)(imagem.Height * 0.8)), 0, imagem.Height * 0.1f, imagem.Width, imagem.Height * 0.8f, GraphicsUnit.Pixel, attributes);
        g.Dispose();
        imagem = newBitmap;
        imagem = imagem.Clone(new Rectangle(0, 0, img.Width, img.Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        Erosion erosion = new Erosion();
        Dilatation dilatation = new Dilatation();
        Invert inverter = new Invert();
        ColorFiltering cor = new ColorFiltering();
        cor.Blue = new AForge.IntRange(200, 255);
        cor.Red = new AForge.IntRange(200, 255);
        cor.Green = new AForge.IntRange(200, 255);
        Opening open = new Opening();
        BlobsFiltering bc = new BlobsFiltering();
        GaussianSharpen gs = new GaussianSharpen();
        ContrastCorrection cc = new ContrastCorrection();
        bc.MinHeight = 3;
        FiltersSequence seq = new FiltersSequence(gs, inverter, open, inverter, bc, inverter, open, cc, cor, bc, inverter);
        return (Bitmap)seq.Apply(imagem);
    }
    
    private static string LetraAleatoria() {
        int num = aleatorio.Next(0, 36);
        if (num > 25) {
            return "" + (num - 25);
        } else {
            return "" + (char)((int)'A' + num);
        }
    }
    
    private static string Resolver(string sobreposto) {
        if (sobreposto == "") {
            return LetraAleatoria();
        } else {
            if (sobreposto.Length == 1) {
                return sobreposto;
            } else {
                if (sobreposto == "II") {
                    return "H";
                } else {
                    if (sobreposto == "CJ") {
                        return "G";
                    } else {
                        return "" + sobreposto[aleatorio.Next(0, sobreposto.Length)];
                    }
                }
            }
        }
    }

    public static string OCR(Bitmap imagem, string linguagem)
    {
        string texto = "";
        using (TesseractEngine engine = new TesseractEngine(@"C:\GitHub\operacao-politica-supervisionada\OPS\temp\", linguagem, EngineMode.Default)) {
            engine.SetVariable("tessedit_char_whitelist", "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            engine.SetVariable("tessedit_unrej_any_wd", true);
            engine.SetVariable("applybox_learn_chars_and_char_frags_mode", true);
            engine.SetVariable("save_blob_choices", true);
            
            string sobreposto = "";
            int    ultimo = 12;
            using (Page page = engine.Process(imagem, PageSegMode.SingleLine)) {
                using (ResultIterator ri = page.GetIterator()) {
                    do
                    {
                        string word = ri.GetText(PageIteratorLevel.Symbol);
                        Tesseract.Rect bb;
                        if (ri.TryGetBoundingBox(PageIteratorLevel.Symbol, out bb)) {
                            if ((bb.Width > 13) && (bb.Height > 15) && (word.Trim() != "")) {
                                while (bb.X1 > ultimo + 14) {
                                    texto += Resolver(sobreposto);
                                    sobreposto = "";
                                    ultimo += 28;
                                }
                                //System.Web.HttpContext.Current.Response.Write(word + ": " + bb.X1 + "<br />\n");
                                if ((word != "Q") || (bb.Height <= 30)) {
                                    sobreposto += word;
                                } else {
                                    sobreposto += "O";
                                }
                            }
                        }
                    } while((ri.Next(PageIteratorLevel.Symbol)));
                    if (texto.Length < 6) {
                        texto += Resolver(sobreposto);
                        while (texto.Length < 6) {
                            texto += LetraAleatoria();
                        }
                    }
                }
            }
        }
        return texto;
    }

    public static string Box(Bitmap imagem, string linguagem, string correto, int pagina)
    {
        string texto = "";
        using (TesseractEngine engine = new TesseractEngine(@"C:\GitHub\operacao-politica-supervisionada\OPS\temp\", linguagem)) {
            engine.SetVariable("tessedit_char_whitelist", "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            engine.SetVariable("tessedit_unrej_any_wd", true);
            engine.SetVariable("applybox_learn_chars_and_char_frags_mode", true);
            engine.SetVariable("save_blob_choices", true);
            
            string sobreposto = "";
            int    ultimo = 12;
            using (Page page = engine.Process(imagem, PageSegMode.SingleLine)) {
                Tesseract.Rect bb;
                int x1 = 14, y1 = 0, x2 = 0, y2 = 0;
                int pos = 0;
                int miny1 = 50;
                using (ResultIterator ri = page.GetIterator()) {
                    do
                    {
                        string word = ri.GetText(PageIteratorLevel.Symbol);
                        if (ri.TryGetBoundingBox(PageIteratorLevel.Symbol, out bb)) {
                            if ((bb.Width > 13) && (bb.Height > 15) && (word.Trim() != "")) {
                                while (bb.X1 > ultimo + 14) {
                                    x2 = Math.Max(x1 + 15, x2);
                                    texto += correto[pos] + " " + x1 + " " + Math.Min(10, y1) + " " + x2 + " " + Math.Max(40, y2) + " " + pagina + "\n";
                                    pos++;
                                    sobreposto = "";
                                    ultimo += 28;
                                    x1 = Math.Max(x1 + 28, x2);
                                }
                                miny1 = Math.Min(miny1, bb.Y1);
                                if (sobreposto != "") {
                                    x1 = Math.Min(x1, bb.X1);
                                    y1 = Math.Min(y1, bb.Y1);
                                    x2 = Math.Max(x2, bb.X2);
                                    y2 = Math.Max(y2, bb.Y2);
                                } else {
                                    x1 = Math.Max(x2 - 5, bb.X1);
                                    y1 = bb.Y1;
                                    x2 = bb.X2;
                                    y2 = bb.Y2;
                                }
                                //System.Web.HttpContext.Current.Response.Write(word + ": " + bb.X1 + "<br />\n");
                                if ((word != "Q") || (bb.Height <= 30)) {
                                    sobreposto += word;
                                } else {
                                    sobreposto += "O";
                                }
                            }
                        }
                    } while((ri.Next(PageIteratorLevel.Symbol)));
                    int limite = imagem.Width - 6;
                    if (pos < 6) {
                        texto += correto[pos] + " " + x1 + " " + Math.Min(10, y1) + " " + x2 + " " + Math.Max(40, y2) + " " + pagina + "\n";
                        while (pos < 5) {
                            pos++;
                            x1 = x2;
                            x2 = x1 + (limite - x1) / (6 - pos);
                            texto += correto[pos] + " " + x1 + " " + Math.Min(10, y1) + " " + x2 + " " + Math.Max(40, y2) + " " + pagina + "\n";
                        }
                    }
                }
                if (miny1 > 40) {
                    texto = "";
                }
            }
        }
        return texto;
    }
}
