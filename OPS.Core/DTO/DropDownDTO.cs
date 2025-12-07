namespace OPS.Core.DTO
{
    public class DropDownDTO
    {
        public uint id { get; set; }
        public string text { get; set; }
        public string helpText { get; set; }
        public string image { get; set; }
        public string[] tokens { get; set; }
    }
}