using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.CrossCore;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
    public abstract class BaseBindingActivity<TViewModel> : MvxActivity
        where TViewModel : PageViewModel, IMvxViewModel
    {
        private bool _firstStart = true;

		public new TViewModel ViewModel
		{
			get
			{
				return (TViewModel)DataContext;
			}
		}

        protected override void OnResume()
        {
            base.OnResume();
        }

		protected override void OnViewModelSet()
		{
			base.OnViewModelSet ();
            ViewModel.OnViewLoaded();
            Mvx.Resolve<IConnectivityService>().HandleToastInNewView();
		}

        protected override void OnStart()
        {
            base.OnStart();
            ViewModel.OnViewStarted(_firstStart);
            _firstStart = false;
        }

        protected override void OnStop()
        {
            base.OnStop();
            ViewModel.OnViewStopped();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ViewModel.OnViewUnloaded();
        }
    }
}