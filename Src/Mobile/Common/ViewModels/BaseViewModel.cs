using System;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.ViewModels;
using TinyMessenger;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BaseViewModel : MvxViewModel, IMvxServiceConsumer<ITinyMessengerHub>
    {
        protected BaseViewModel()
        {
            MessengerHub = this.GetService<ITinyMessengerHub>();
        }

        protected ITinyMessengerHub MessengerHub
        {
            get; private set;
        }

        public virtual void Load()
        {}
    }
}

