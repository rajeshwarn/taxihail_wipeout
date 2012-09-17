using Microsoft.Practices.Unity;
using apcurium.MK.Booking.Google.Impl;

namespace apcurium.MK.Booking.Google
{
    public class Module
    {
        public void Init(IUnityContainer container)
        {
            container.RegisterType<IMapsApiClient,MapsApiClient>();
        }
    }
}
