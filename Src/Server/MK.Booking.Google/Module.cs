#region

using apcurium.MK.Booking.Google.Impl;
using Microsoft.Practices.Unity;

#endregion

namespace apcurium.MK.Booking.Google
{
    public class Module
    {
        public void Init(IUnityContainer container)
        {
            container.RegisterType<IMapsApiClient, MapsApiClient>();
        }
    }
}