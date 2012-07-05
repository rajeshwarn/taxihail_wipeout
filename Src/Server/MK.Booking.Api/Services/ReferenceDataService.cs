using ServiceStack.CacheAccess;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.IBS;

namespace apcurium.MK.Booking.Api.Services
{
    public class ReferenceDataService : RestServiceBase<ReferenceDataRequest>
    {
        private readonly IStaticDataWebServiceClient _staticDataWebServiceClient;
        private readonly ICacheClient _cacheClient;
        private const string CacheKey = "IBS.StaticData";

        public ReferenceDataService(IStaticDataWebServiceClient staticDataWebServiceClient, ICacheClient  cacheClient)
        {
            _staticDataWebServiceClient = staticDataWebServiceClient;
            _cacheClient = cacheClient;
        }

        public override object OnGet(ReferenceDataRequest request)
        {
            var result = _cacheClient.Get<ReferenceData>(CacheKey);

            if(result == null)
            {
                result = new ReferenceData
                             {
                                 CompaniesList = _staticDataWebServiceClient.GetCompaniesList(),
                                 PaymentsList = _staticDataWebServiceClient.GetPaymentsList(),
                                 VehiclesList = _staticDataWebServiceClient.GetVehiclesList()
                             };
                _cacheClient.Add(CacheKey, result);
            }
            return result;
        }
    }
}