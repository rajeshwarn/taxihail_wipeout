#region

using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class PlaceDetailServiceClient : BaseServiceClient
    {
        public PlaceDetailServiceClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
        }


        public Task<Address> GetPlaceDetail(string reference, string placeName)
        {
            var result = Client.GetAsync(new PlaceDetailRequest
            {
                ReferenceId = reference,
                PlaceName = placeName
            });
            return result;
        }
    }
}