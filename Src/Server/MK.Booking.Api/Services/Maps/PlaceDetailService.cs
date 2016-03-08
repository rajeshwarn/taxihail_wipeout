#region

using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.Api.Services.Maps
{
    public class PlaceDetailService : BaseApiService
    {
        private readonly IPlaces _client;

        public PlaceDetailService(IPlaces client)
        {
            _client = client;
        }


        public Task<Address> Get(PlaceDetailRequest request)
        {
            return _client.GetPlaceDetail(request.PlaceName, request.PlaceId);
        }
    }
}