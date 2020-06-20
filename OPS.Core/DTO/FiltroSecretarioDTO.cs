using System.Collections.Generic;

namespace OPS.Core.DTO
{
    public class FiltroSecretarioDTO
    {
        //public string sorting { get; set; }
        //public int count { get; set; }
        //public int page { get; set; }

        public int draw { get; set; }
        public int start { get; set; }
        public int length { get; set; }
        public Dictionary<string, object> order { get; set; }

        public string NomeParlamentar { get; set; }
        public bool Ativo { get; set; }

        public FiltroSecretarioDTO()
        {
            this.start = 0;
            this.length = 500;
        }
    }
}
