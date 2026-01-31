using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.RioDeJaneiro.Entities
{
    public class BudgetDetails
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        //[JsonPropertyName("congressman_legislature_id")]
        //public int CongressmanLegislatureId { get; set; }

        //[JsonPropertyName("budget_id")]
        //public int BudgetId { get; set; }

        //[JsonPropertyName("percentage")]
        //public string Percentage { get; set; }

        //[JsonPropertyName("value")]
        //public string Value { get; set; }

        //[JsonPropertyName("analysed_by_id")]
        //public int AnalysedById { get; set; }

        //[JsonPropertyName("analysed_at")]
        //public string AnalysedAt { get; set; }

        //[JsonPropertyName("published_by_id")]
        //public int PublishedById { get; set; }

        //[JsonPropertyName("published_at")]
        //public string PublishedAt { get; set; }

        //[JsonPropertyName("created_by_id")]
        //public int CreatedById { get; set; }

        //[JsonPropertyName("updated_by_id")]
        //public int UpdatedById { get; set; }

        //[JsonPropertyName("transport_from_previous_entry_id")]
        //public int? TransportFromPreviousEntryId { get; set; }

        //[JsonPropertyName("transport_to_next_entry_id")]
        //public int TransportToNextEntryId { get; set; }

        //[JsonPropertyName("created_at")]
        //public DateTime CreatedAt { get; set; }

        //[JsonPropertyName("updated_at")]
        //public DateTime UpdatedAt { get; set; }

        //[JsonPropertyName("closed_by_id")]
        //public int ClosedById { get; set; }

        //[JsonPropertyName("closed_at")]
        //public string ClosedAt { get; set; }

        //[JsonPropertyName("missing_analysis")]
        //public bool MissingAnalysis { get; set; }

        //[JsonPropertyName("missing_verification")]
        //public bool MissingVerification { get; set; }

        //[JsonPropertyName("has_deposit")]
        //public bool HasDeposit { get; set; }

        [JsonPropertyName("entries_count")]
        public int EntriesCount { get; set; }

        //[JsonPropertyName("sum_credit")]
        //public string SumCredit { get; set; }

        //[JsonPropertyName("sum_debit")]
        //public string SumDebit { get; set; }

        //[JsonPropertyName("has_comments_pendent")]
        //public bool HasCommentsPendent { get; set; }

        //[JsonPropertyName("has_comments_pendent_others_months")]
        //public bool HasCommentsPendentOthersMonths { get; set; }

        //[JsonPropertyName("has_comments_pendent_unanalysed")]
        //public bool HasCommentsPendentUnanalysed { get; set; }

        //[JsonPropertyName("sum_i")]
        //public string SumI { get; set; }

        //[JsonPropertyName("sum_ii")]
        //public object SumIi { get; set; }

        //[JsonPropertyName("sum_iii")]
        //public object SumIii { get; set; }

        //[JsonPropertyName("sum_iv")]
        //public string SumIv { get; set; }

        //[JsonPropertyName("sum_v")]
        //public object SumV { get; set; }

        //[JsonPropertyName("sum_vi_a")]
        //public string SumViA { get; set; }

        //[JsonPropertyName("sum_vi_b")]
        //public string SumViB { get; set; }

        //[JsonPropertyName("sum_vii")]
        //public string SumVii { get; set; }

        //[JsonPropertyName("sum_viii")]
        //public string SumViii { get; set; }

        //[JsonPropertyName("sum_ix")]
        //public object SumIx { get; set; }

        //[JsonPropertyName("sum_x")]
        //public string SumX { get; set; }

        //[JsonPropertyName("sum_xi")]
        //public object SumXi { get; set; }

        //[JsonPropertyName("has_refund")]
        //public bool HasRefund { get; set; }

        //[JsonPropertyName("pendency_comments_date")]
        //public List<object> PendencyCommentsDate { get; set; }

        //[JsonPropertyName("budget")]
        //public Budget Budget { get; set; }

        //[JsonPropertyName("congressman_legislature")]
        //public CongressmanLegislature CongressmanLegislature { get; set; }

        //[JsonPropertyName("entries")]
        //public List<Entry> Entries { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("month")]
        public string Month { get; set; }

        //[JsonPropertyName("state_value_formatted")]
        //public string StateValueFormatted { get; set; }

        //[JsonPropertyName("value_formatted")]
        //public string ValueFormatted { get; set; }

        //[JsonPropertyName("sum_debit_formatted")]
        //public string SumDebitFormatted { get; set; }

        //[JsonPropertyName("sum_credit_formatted")]
        //public string SumCreditFormatted { get; set; }

        //[JsonPropertyName("balance")]
        //public int Balance { get; set; }

        //[JsonPropertyName("balance_formatted")]
        //public string BalanceFormatted { get; set; }

        //[JsonPropertyName("percentage_formatted")]
        //public string PercentageFormatted { get; set; }

        //[JsonPropertyName("pendencies")]
        //public List<object> Pendencies { get; set; }
    }
}
