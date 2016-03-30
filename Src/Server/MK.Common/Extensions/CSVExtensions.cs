using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Common.Extensions
{
    public static class CsvExtensions
    {
        public static string ToCsv(this List<Dictionary<string, string>> content)
        {
            var csvFlattened = new StringBuilder();
            foreach (var item in content.ElementAt(0))
            {
                csvFlattened.Append(item.Key).Append(",");
            }

            csvFlattened.Append("\n");

            foreach (var line in content)
            {
                foreach (var item in line)
                {
                    csvFlattened.Append(item.Value).Append(",");
                }
                csvFlattened.Append("\n");
            }

            return csvFlattened.ToString();
        }
    }
}
