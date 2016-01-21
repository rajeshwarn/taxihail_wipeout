using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common;

namespace CMTPayment
{
    public partial class BaseServiceClient
    {
        private const string DefaultUserAgent = "TaxiHail";

        private readonly string _sessionId;
        private readonly string _url;
        private readonly IPackageInfo _packageInfo;
        private readonly IConnectivityService _connectivityService;

        public BaseServiceClient(string url, string sessionId, IPackageInfo packageInfo, IConnectivityService connectivityService)
        {
            _url = url;
            _sessionId = sessionId;
            _packageInfo = packageInfo;
            _connectivityService = connectivityService;
        }
    }
}