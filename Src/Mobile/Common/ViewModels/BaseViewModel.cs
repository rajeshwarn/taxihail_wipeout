using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.ViewModels;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BaseViewModel : MvxViewModel,
        IMvxServiceConsumer<ITinyMessengerHub>,
        IMvxServiceConsumer<IAppResource>,
        IMvxServiceConsumer<IMessageService>,
        IMvxServiceConsumer<ILogger>

    {
        protected BaseViewModel()
        {
            MessengerHub = this.GetService<ITinyMessengerHub>();
            Resources = this.GetService<IAppResource>();
            MessageService = this.GetService<IMessageService>();
			Logger = this.GetService<ILogger>();

			Initialize();
        }

		protected ILogger Logger
		{
			get; private set;
		}

        protected IMessageService MessageService
        {
            get; private set;
        }

        protected IAppResource Resources
        {
            get; private set;
        }

        protected ITinyMessengerHub MessengerHub
        {
            get; private set;
        }

		protected virtual void Initialize ()
		{

		}
    }
}

