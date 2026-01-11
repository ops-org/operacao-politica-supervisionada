using System.Collections.Generic;

namespace OPS.Core.DTO
{
    public class ResultFilter<T>
    {
        public int Draw { get; set; }
        public IEnumerable<T> Data { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
    }
}
