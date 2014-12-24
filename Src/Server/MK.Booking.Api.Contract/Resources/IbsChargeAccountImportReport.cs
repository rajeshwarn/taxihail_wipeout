using System.Collections.Generic;
namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class IbsChargeAccountImportReport
    {
        public List<KeyValuePair<string, string>> ReportLines = new List<KeyValuePair<string, string>>();
    }
}
