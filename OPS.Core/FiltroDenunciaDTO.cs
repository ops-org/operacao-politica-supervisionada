namespace OPS.Core
{
    public class FiltroDenunciaDTO
    {
        public string sorting { get; set; }
        public int count { get; set; }
        public int page { get; set; }

        public bool MansagensNaoLidas { get; set; }
        public bool AguardandoRevisao { get; set; }
        public bool PendenteInformacao { get; set; }
        public bool Duvidoso { get; set; }
        public bool Dossie { get; set; }
        public bool Repetido { get; set; }
        public bool NaoProcede { get; set; }

        public FiltroDenunciaDTO()
        {
            this.count = 100;
            this.page = 1;
        }
    }
}
