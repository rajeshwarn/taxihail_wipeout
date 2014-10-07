#region

using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Common.Configuration;
using ServiceStack.ServiceInterface;
using System.Globalization;
using System.Web;

#endregion

namespace apcurium.MK.Booking.Api.Services.Maps
{
    public class DirectionsService : Service
    {
        private readonly IDirections _client;
        private readonly IConfigurationManager _configurationManager;
        private readonly IAppSettings _appSettings;
        public DirectionsService(IDirections client, IAppSettings appSettings, IConfigurationManager configurationManager)
        {
            _client = client;
            _appSettings = appSettings;
            _configurationManager = configurationManager;
        }


        public object Get(DirectionsRequest request)
        {
            var result = _client.GetDirection(request.OriginLat, request.OriginLng, request.DestinationLat,
                request.DestinationLng, CurrencyPriceFormat, request.VehicleTypeId, request.Date);
            return new DirectionInfo
            {
                Distance = result.Distance,
                FormattedDistance = result.FormattedDistance,                               
                Price = result.Price,
                FormattedPrice = result.FormattedPrice
            };
        }
        
        public string CurrencyPriceFormat
        {
            get
            {
                var culture = new CultureInfo(HttpContext.Current.Request.UserLanguages[0]);
                var applicationKey = _configurationManager.GetSetting("TaxiHail.ApplicationKey");
                var resources = new Resources.Resources(applicationKey);
                return resources.Get("CurrencyPriceFormat", culture.TwoLetterISOLanguageName);
            }
        }
    }
}