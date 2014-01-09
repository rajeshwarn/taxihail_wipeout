using TinyIoC;
using Cirrious.CrossCore.IoC;

namespace apcurium.MK.Booking.Mobile.Mvx_
{
    public class TinyIoCProvider : IMvxIoCProvider
    {
        #region IMvxIoCProvider implementation
        public bool SupportsService<T>() where T : class
        {
            return TinyIoCContainer.Current.CanResolve<T>();
        }

        public T GetService<T>() where T : class
        {
            return TinyIoCContainer.Current.Resolve<T>();
        }

        public bool TryGetService<T>(out T service) where T : class
        {
            return TinyIoCContainer.Current.TryResolve(out service);
        }

        public void RegisterServiceType<TFrom, TTo>() 
        {
            TinyIoCContainer.Current.Register( typeof(TFrom), typeof(TTo ) );
        }

        public void RegisterServiceInstance<TInterface>(TInterface theObject)
        {
            TinyIoCContainer.Current.Register( typeof(TInterface)  ,theObject);
        }
        #endregion

    }
}

