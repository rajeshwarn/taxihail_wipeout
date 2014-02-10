using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
    public abstract class BaseCallboxViewModel : BaseViewModel
    {
		public AsyncCommand Logout
        {
            get
            {
                return this.GetCommand(() => this.Services().Message.ShowMessage(this.Services().Localize["LogoutTitle"],
                    this.Services().Localize["LogoutMessage"],
                    this.Services().Localize["Yes"], () =>
                    { }, this.Services().Localize["No"], () => { }));
            }
        }
    }
}