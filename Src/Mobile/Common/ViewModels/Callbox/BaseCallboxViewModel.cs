using Cirrious.MvvmCross.Interfaces.Commands;

namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
    public abstract class BaseCallboxViewModel : BaseViewModel
    {
     
		public IMvxCommand Logout
        {
            get
            {
                return GetCommand(() => MessageService.ShowMessage(Resources.GetString("LogoutTitle"), Resources.GetString("LogoutMessage"), Resources.GetString("Yes"), () =>
					{}, Resources.GetString("No"), () => { }));
            }
        }
    }
}