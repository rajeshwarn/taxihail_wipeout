using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
    public abstract class BaseCallboxViewModel : BaseViewModel, IMvxServiceConsumer<IBookingService>,
        IMvxServiceConsumer<IAccountService>,
        IMvxServiceConsumer<IAppSettings>,
        IMvxServiceConsumer<ICacheService>
    {
     
		public IMvxCommand Logout
        {
            get
            {
                return this.GetCommand(() => this.MessageService.ShowMessage(this.Resources.GetString("LogoutTitle"), this.Resources.GetString("LogoutMessage"), this.Resources.GetString("Yes"), () =>
					{}, this.Resources.GetString("No"), () => { }));
            }
        }
    }
}