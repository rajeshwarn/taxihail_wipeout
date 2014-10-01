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
            var settings = container.Resolve<IServerSettings>();

            container.RegisterInstance<IGeocoder>(new GoogleApiClient(settings, container.Resolve<ILogger>(), null));
            container.RegisterInstance<IPlaceDataProvider>(new GoogleApiClient(settings, container.Resolve<ILogger>(), null));

            container.RegisterType<IDirectionDataProvider>(
                new TransientLifetimeManager(),
                new InjectionFactory(c =>
                {
                    switch (settings.Data.DirectionDataProvider)
                    {
                        case MapProvider.TomTom:
                            return new TomTomProvider(settings, container.Resolve<ILogger>());
                        default:
                            return new GoogleApiClient(settings, container.Resolve<ILogger>());
                    }
                }));
        }
    }
}