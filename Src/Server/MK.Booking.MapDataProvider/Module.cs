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
            container.RegisterInstance<IGeocoder>(new GoogleApiClient(container.Resolve<IAppSettings>(), container.Resolve<ILogger>(), null));
            container.RegisterInstance<IPlaceDataProvider>(new GoogleApiClient(container.Resolve<IAppSettings>(), container.Resolve<ILogger>(), null));
            container.RegisterInstance<IDirectionDataProvider>(new GoogleApiClient(container.Resolve<IAppSettings>(), container.Resolve<ILogger>(), null));

        }
    }
}