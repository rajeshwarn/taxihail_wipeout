using apcurium.MK.Booking.Maps;
using apcurium.MK.Common.Entity;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class GoogleService : BaseService, IGoogleService
	{
		public Task<Address> GetPlaceDetail(string name, string placeId)
        {
			return TinyIoCContainer.Current.Resolve<IPlaces>().GetPlaceDetail(name, placeId);
        }
        public Address[] GetNearbyPlaces(double? latitude, double? longitude, string name = null)
		{
			string currentLanguage = TinyIoCContainer.Current.Resolve<ILocalization>().CurrentLanguage;
			var places = TinyIoCContainer.Current.Resolve<IPlaces>().SearchPlaces(name, latitude, longitude, currentLanguage);
			return places;
		}
	}
}

