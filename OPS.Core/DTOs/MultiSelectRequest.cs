namespace OPS.Core.DTOs
{
    /// <summary>
    /// Represents a MultiSelect request.
    /// </summary>
    public class MultiSelectRequest
    {
        public string Ids { get; set; }

        public string Busca { get; set; }

        public int? Periodo { get; set; }

        public int? Ano { get; set; }

        public int? Mes { get; set; }
    }

}
