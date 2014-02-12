using apcurium.MK.Booking.Mobile.Extensions;
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
    public abstract class BaseCallboxViewModel : BaseViewModel
    {
        public ICommand Logout
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