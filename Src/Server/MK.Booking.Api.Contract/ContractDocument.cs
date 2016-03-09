#region

using System.Collections.Generic;
using System.Linq;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract
{
    public class ContractDocument
    {
        public ContractDocumentReport[] Build()
        {
            var assembly = GetType().Assembly;
            var types = assembly.GetTypes();

            return types
                .SelectMany(type => type
                    .GetCustomAttributes(typeof(RouteAttribute), true)
                    .OfType<RouteAttribute>()
                    .Select(restServiceAttribute => new ContractDocumentReport()
                    {
                        Path = restServiceAttribute.Address,
                        Verbs = restServiceAttribute.Method
                    })
                )
                .OrderBy(r => r.Path)
                .ToArray();
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
                if (Verbs.Contains(','))
                {
                    return Verbs.Split(',').Where(v => !string.IsNullOrEmpty(v)).Select(v => v.Trim());
                }
                return new[] {Verbs.Trim()};
            }
        }
    }
}