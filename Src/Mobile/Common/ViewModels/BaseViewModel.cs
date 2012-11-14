using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.ViewModels;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BaseViewModel : MvxViewModel,
        IMvxServiceConsumer<ITinyMessengerHub>,
        IMvxServiceConsumer<IAppResource>,
        IMvxServiceConsumer<IMessageService>

    {
        protected BaseViewModel()
        {
            MessengerHub = this.GetService<ITinyMessengerHub>();
            Resources = this.GetService<IAppResource>();
            MessageService = this.GetService<IMessageService>();
        }

        protected IMessageService MessageService
        {
            get; set;
        }

        protected IAppResource Resources
        {
            get; private set;
        }

        protected ITinyMessengerHub MessengerHub
        {
            get; private set;
        }

        public virtual void Load()
        {}

        

    }
}

