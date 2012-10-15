using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract
{

    public class ContractDocument
    {

        public ContractDocumentReport[] Build()
        {
            var result = new List<ContractDocumentReport>();
              var assembly = this.GetType().Assembly;
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                var att = type.GetCustomAttributes(typeof(RestServiceAttribute), true).OfType<RestServiceAttribute>();
                foreach (var restServiceAttribute in att)
                {
                    result.Add( new ContractDocumentReport{ Path = restServiceAttribute.Path, Verbs = restServiceAttribute.Verbs });
                }
            }
            return result.OrderBy( r=>r.Path ).ToArray();
        }

    }

    public class ContractDocumentReport
    {
        public string Path { get; set; }
        public string Verbs { get; set; }

        public IEnumerable<string> VerbList
        {
            get
            {
                if ( Verbs.Contains(',') )
                {
                    return Verbs.Split(',').Where(v => !string.IsNullOrEmpty(v)).Select(v => v.Trim());
                }
                else
                {
                    return new string[]{ Verbs.Trim()};
                }

            }
        }
    }
}
