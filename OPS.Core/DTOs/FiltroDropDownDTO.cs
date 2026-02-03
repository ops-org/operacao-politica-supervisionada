﻿using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class FiltroDropDownDTO
    {
        /// <summary>
        /// Filtro digitado pelo usuario
        /// </summary>
        [JsonPropertyName("q")]
        public string Q { get; set; }

        /// <summary>
        /// Filtro para carregamento especial (navegação/URL)
        /// </summary>
        [JsonPropertyName("qs")]
        public string Qs { get; set; }

        /// <summary>
        /// Pagina pesquisada
        /// </summary>
        [JsonPropertyName("page")]
        public int? Page { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; } = 30;    }
}