#region


using apcurium.MK.Booking.MapDataProvider.Google;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
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
            container.RegisterInstance<IDirectionDataProvider>(new GoogleApiClient(container.Resolve<IAppSettings>(), container.Resolve<ILogger>(), null));

        }
    }
}