#region

using System;
using apcurium.MK.Booking.MapDataProvider.Google;
using apcurium.MK.Booking.MapDataProvider.TomTom;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using Microsoft.Practices.Unity;

#endregion

namespace apcurium.MK.Booking.MapDataProvider
{
    public class Module
    {
        public void Init(IUnityContainer container)
        {
            var settings = container.Resolve<IAppSettings>();

            container.RegisterInstance<IGeocoder>(new GoogleApiClient(settings, container.Resolve<ILogger>(), null));
            container.RegisterInstance<IPlaceDataProvider>(new GoogleApiClient(settings, container.Resolve<ILogger>(), null));

            switch (settings.Data.DirectionDataProvider)
            {
                case MapProvider.TomTom:
                    container.RegisterInstance<IDirectionDataProvider>(new TomTomProvider(settings, container.Resolve<ILogger>()));
                    break;
                default:
                    container.RegisterInstance<IDirectionDataProvider>(new GoogleApiClient(settings, container.Resolve<ILogger>(), null));
                    break;
            }
        }
    }
}