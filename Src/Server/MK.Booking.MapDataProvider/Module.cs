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
            container.RegisterType<IMapsApiClient, MapsApiClient>();
        }
    }
}