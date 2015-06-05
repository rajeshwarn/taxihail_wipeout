using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.MapDataProvider;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services.Maps
{
    public class PostalCodeService : Service
    {
        private readonly IPostalCodeService _postalCodeService;
        private readonly ILogger _log;

        public PostalCodeService(IPostalCodeService postalCodeService, ILogger log)
        {
            _postalCodeService = postalCodeService;
            _log = log;
        }


        public object Post(GetAddressesFromPostcalCodeRequest request)
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
