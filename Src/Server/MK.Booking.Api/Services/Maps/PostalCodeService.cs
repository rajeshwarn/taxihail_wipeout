using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.MapDataProvider;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Api.Services.Maps
{
    public class PostalCodeService : BaseApiService
    {
        private readonly IPostalCodeService _postalCodeService;
        private readonly ILogger _log;

        public PostalCodeService(IPostalCodeService postalCodeService, ILogger log)
        {
            _postalCodeService = postalCodeService;
            _log = log;
        }


        public Address[] Post(GetAddressesFromPostcalCodeRequest request)
        {
            if (!request.PostalCode.HasValue())
            {
                _log.LogMessage("Warning, No postal code received");

                return new Address[0];
            }

            return _postalCodeService.GetAddressFromPostalCode(request.PostalCode);
        }
    }
}
