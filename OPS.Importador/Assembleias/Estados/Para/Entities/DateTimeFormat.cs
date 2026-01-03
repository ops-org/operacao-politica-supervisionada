using System.Text.Json.Serialization;



namespace OPS.Importador.Assembleias.Estados.Para.Entities
{
    public class DateTimeFormat
    {
        [JsonPropertyName("GroupInterval")]
        public string GroupInterval { get; set; }

        [JsonPropertyName("ExactDateFormat")]
        public string ExactDateFormat { get; set; }

        [JsonPropertyName("YearFormat")]
        public string YearFormat { get; set; }

        [JsonPropertyName("QuarterFormat")]
        public string QuarterFormat { get; set; }

        [JsonPropertyName("MonthFormat")]
        public string MonthFormat { get; set; }

        [JsonPropertyName("DayOfWeekFormat")]
        public string DayOfWeekFormat { get; set; }

        [JsonPropertyName("DateFormat")]
        public string DateFormat { get; set; }

        [JsonPropertyName("DateHourFormat")]
        public string DateHourFormat { get; set; }

        [JsonPropertyName("DateHourMinuteFormat")]
        public string DateHourMinuteFormat { get; set; }

        [JsonPropertyName("DateTimeFormat")]
        public string DtFormat { get; set; }

        [JsonPropertyName("HourFormat")]
        public string HourFormat { get; set; }

        [JsonPropertyName("FirstDayOfWeek")]
        public int FirstDayOfWeek { get; set; }

        [JsonPropertyName("CalendarWeekRule")]
        public string CalendarWeekRule { get; set; }
    }
}

