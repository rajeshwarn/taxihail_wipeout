using apcurium.MK.Booking.Mobile.Infrastructure;

namespace CMTPayment
{
    public partial class BaseServiceClient
    {
        private const string DefaultUserAgent = "TaxiHail";

        private readonly string _sessionId;
        private readonly string _url;
        private readonly IPackageInfo _packageInfo;
        

        public BaseServiceClient(string url, string sessionId, IPackageInfo packageInfo)
        {
            _url = url;
            _sessionId = sessionId;
            _packageInfo = packageInfo;
        }



        
    }
}