#region


using apcurium.MK.Booking.MapDataProvider.Google;
using Microsoft.Practices.Unity;

#endregion

namespace apcurium.MK.Booking.MapDataProvider
{
    public class Module
    {
        public void Init(IUnityContainer container)
        {
            container.RegisterType<IGeocoder, GoogleApiClient>();
            container.RegisterType<IPlaceDataProvider, GoogleApiClient>();
            container.RegisterType<IDirectionDataProvider, GoogleApiClient>();

        }
    }
}