namespace OPS.Core.DTO
{
    public class FiltroSecretarioDTO
    {
        public string sorting { get; set; }
        public int count { get; set; }
        public int page { get; set; }

        public string NomeParlamentar { get; set; }
        public bool Ativo { get; set; }

        public FiltroSecretarioDTO()
        {
            this.count = 100;
            this.page = 1;
        }
    }
}
