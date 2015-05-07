using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public abstract class PageViewModel : BaseViewModel 
    {
        public virtual void OnViewLoaded()
        {
        }

        public virtual void OnViewUnloaded()
        {
            Subscriptions.Clear();
        }

		public ICommand CloseCommand
		{
			get
			{
				return this.GetCommand(() => Close(this));
			}
		}
    }
}

