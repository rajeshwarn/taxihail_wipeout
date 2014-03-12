using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public abstract class PageViewModel : BaseViewModel 
    {
        public virtual void OnViewLoaded()
        {
        }

        public virtual void OnViewStarted(bool firstTime)
        {
        }

        public virtual void OnViewStopped()
        {
        }

        public virtual void OnViewUnloaded()
        {
            Subscriptions.Clear();
        }

		protected virtual void Dispose(bool disposing)
		{
			if (disposing) 
			{
				Subscriptions.Dispose();
			}
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

