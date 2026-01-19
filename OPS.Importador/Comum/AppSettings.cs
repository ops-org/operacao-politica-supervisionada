using System;
using System.Collections.Generic;
using System.Text;

namespace OPS.Importador.Comum
{
    public class AppSettings
    {
        /// <summary>
        /// The root directory path used by the application api.
        /// </summary>
        public string SiteRootFolder { get; set; } = "/var/www/ops.net.br";

        /// <summary>
        /// The file system path to the temporary directory used for intermediate files.
        /// </summary>
        /// <remarks>The specified path should be accessible and writable by the application. If not set,
        /// a default temporary directory may be used depending on the implementation.</remarks>
        public string TempFolder { get; set; } = "C:\\temp\\";

        public string SendGridAPIKey { get; set; }

        public string TelegramApiToken { get; set; }

        public string ReceitaWsApiToken { get; set; }

        public bool ReuseDownloadFile { get; set; } = true;
        
        public bool ForceImport { get; set; } = false;

        public bool StoreBackupFile { get; set; } = true;
    }
}
