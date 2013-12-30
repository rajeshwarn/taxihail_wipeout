using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
    public abstract class BaseCallboxViewModel : BaseViewModel
    {
     
		public IMvxCommand Logout
        {
            get
            {
				return GetCommand(() => this.Services().Message.ShowMessage(this.Services().Resources.GetString("LogoutTitle"), 
					this.Services().Resources.GetString("LogoutMessage"), 
					this.Services().Resources.GetString("Yes"), () =>
					{}, this.Services().Resources.GetString("No"), () => { }));
            }
        }
    }
}