#region

using System.Threading.Tasks;
using apcurium.MK.Common;


#if !CLIENT
using apcurium.MK.Booking.Api.Client.Extensions;
#endif
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class ReferenceDataServiceClient : BaseServiceClient
    {
        public ReferenceDataServiceClient(string url, string sessionId, IPackageInfo packageInfo, IConnectivityService connectivityService)
            : base(url, sessionId, packageInfo, connectivityService)
        {
        }

        public Task<ReferenceData> GetReferenceData()
        {
            return Client.GetAsync<ReferenceData>("/referencedata");
        }
    }
}