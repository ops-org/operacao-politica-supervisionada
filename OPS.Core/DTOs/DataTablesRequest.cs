#region Copyright
/* The MIT License (MIT)

Copyright (c) 2014 Anderson Luiz Mendes Matos (Brazil)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
#endregion Copyright

using System.Collections.Generic;
using System.Linq;
using OPS.Core.Utilities;

namespace OPS.Core.DTOs
{
    /// <summary>
    /// Represents a DataTables request.
    /// </summary>
    public class DataTablesRequest
    {
        public Dictionary<string, object> Filters { get; set; }
        public int Length { get; set; }
        public int Start { get; set; }
        public List<Order> Order { get; set; }

        public string GetSorting(Dictionary<int, string> dcColumns, string defaultSort = "")
        {
            if (Order?.Count > 0 && dcColumns != null)
            {
                var lstSort = new List<string>();
                foreach (var item in Order)
                {
                    if (dcColumns.TryGetValue(item.Column, out string column))
                        lstSort.Add(string.Format("{0} {1}", column, item.Dir));
                }

                if (lstSort.Count > 0)
                    return Utils.MySqlEscape(string.Join(",", lstSort));
            }

            return defaultSort;
        }

        public string GetSorting(string defaultSort = "")
        {
            if (Order?.Count > 0)
            {
                var lstSort = new List<string>();
                foreach (var item in Order)
                {
                    lstSort.Add(string.Format("{0} {1}", item.Column + 1, item.Dir));
                }

                if (lstSort.Count > 0)
                    return Utils.MySqlEscape(string.Join(",", lstSort));
            }

            return defaultSort;
        }
    }


    public class Order
    {
        public int Column { get; set; }
        public string Dir { get; set; }
    }
}
