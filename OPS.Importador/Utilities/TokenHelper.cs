using System.Security.Cryptography;
using System.Text;

namespace OPS.Importador.Utilities
{
    // You may need: using System; using System.Text; using System.Security.Cryptography;

    public static class TokenHelper
    {
        //        // AES key for gerarTokenAngular
        //private static readonly string AngularKey = "qmNuYatzTeMn2sST";
        //// 16 zero bytes for IV
        //private static readonly byte[] ZeroIV = new byte[16];

        //public static string GerarTokenAngular()
        //{
        //    string data = "angular;" + FormatDateBrazil(DateTime.Now, true);
        //    // Pad to multiple of 16 bytes
        //    while (data.Length % 16 != 0)
        //        data += '\0';

        //    byte[] key = Encoding.UTF8.GetBytes(AngularKey);
        //    byte[] plainBytes = Encoding.UTF8.GetBytes(data);

        //    byte[] cipherBytes = EncryptAesCbcNoPadding(plainBytes, key, ZeroIV);

        //    StringBuilder sb = new StringBuilder();
        //    for (int i = 0; i < cipherBytes.Length; i += 4)
        //    {
        //        int word = BitConverter.ToInt32(cipherBytes, i);
        //        if (word < 0)
        //            word = (int)(4294967295 + word + 1);
        //        string hex = word.ToString("x");
        //        while (hex.Length < 8)
        //            hex = "0" + hex;
        //        sb.Append(hex);
        //    }
        //    return sb.ToString();
        //}

        //public static string FormatDateBrazil(DateTime t, bool includeTime)
        //{
        //    string day = t.Day.ToString("D2");
        //    string month = t.Month.ToString("D2");
        //    string year = t.Year.ToString();
        //    string date = $"{day}/{month}/{year}";
        //    if (includeTime)
        //    {
        //        string hour = t.Hour.ToString("D2");
        //        string min = t.Minute.ToString("D2");
        //        string sec = t.Second.ToString("D2");
        //        date += $" {hour}:{min}:{sec}";
        //    }
        //    return date;
        //}

        //private static byte[] EncryptAesCbcNoPadding(byte[] plainBytes, byte[] key, byte[] iv)
        //{
        //    using (Aes aes = Aes.Create())
        //    {
        //        aes.Mode = CipherMode.CBC;
        //        aes.Padding = PaddingMode.None;
        //        aes.KeySize = 128;
        //        aes.BlockSize = 128;
        //        aes.Key = key;
        //        aes.IV = iv;
        //        using (ICryptoTransform encryptor = aes.CreateEncryptor())
        //        {
        //            return encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        //        }
        //    }
        //}

        public static long diffTime = -873;

        public static string GerarTokenAngular(string publicToken)
        {
            long l = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            l += diffTime;
            return GerarToken(
                publicToken,
                "uahu53Zzw5RSSXiK",
                FormatDateBrazil(GetUTCDate(l), true)
            );
        }

        public static string GerarToken(string e, string t, string n)
        {
            byte[] key = Encoding.UTF8.GetBytes(t);
            byte[] iv = new byte[16]; // 16 zero bytes
            string o = e + ";" + n;
            while (o.Length % 16 != 0)
            {
                o += '\0';
            }
            byte[] plainBytes = Encoding.UTF8.GetBytes(o);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;
                aes.KeySize = 128;

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < cipherBytes.Length; i += 4)
                    {
                        int word = BitConverter.ToInt32(cipherBytes, i);
                        if (word < 0)
                            word = (int)(4294967295 + word + 1);
                        string hex = word.ToString("x8");
                        sb.Append(hex);
                    }
                    return sb.ToString();
                }
            }
        }

        public static string FormatDateBrazil(DateTime t, bool includeTime)
        {
            string r = t.Year.ToString();
            string i = (t.Month).ToString("D2");
            string o = t.Day.ToString("D2");
            string l = t.Hour.ToString("D2");
            string a = t.Minute.ToString("D2");
            string s = t.Second.ToString("D2");
            string u = $"{o}/{i}/{r}";
            if (includeTime)
                u += $" {l}:{a}:{s}";
            return u;
        }

        public static DateTime GetUTCDate(long milliseconds)
        {
            // Assumes milliseconds since Unix epoch
            return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).UtcDateTime;
        }
    }
}
