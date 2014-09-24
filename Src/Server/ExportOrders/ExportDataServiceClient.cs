using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class ExportDataServiceClient : BaseServiceClient
    {
        public ExportDataServiceClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
        }

        public Task<object> GetOrders(ExportDataRequest request)
        {
            var req = string.Format("/admin/export/{0}?format=csv", request.Target.ToString().ToLower());
            return Client.PostAsync<object>(req, request);
        }
    }
}