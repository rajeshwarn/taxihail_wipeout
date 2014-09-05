#region

using apcurium.MK.Booking.Maps.Impl;
using Microsoft.Practices.Unity;

#endregion

namespace apcurium.MK.Booking.Maps
{
    public class Module
    {
        public void Init(IUnityContainer container)
        {
            container.RegisterType<IAddresses, Addresses>();
            container.RegisterType<IPriceCalculator, PriceCalculator>();
            container.RegisterType<IDirections, Directions>();
            container.RegisterType<IGeocoding, Geocoding>();
            container.RegisterType<IPlaces, Places>();
            container.RegisterType<IStaticMap, StaticMap>();
        }
    }
}