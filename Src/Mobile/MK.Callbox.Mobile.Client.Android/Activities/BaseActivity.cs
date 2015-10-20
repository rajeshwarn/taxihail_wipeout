using Android.App;
using Android.Widget;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.ViewModels;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Callbox.Mobile.Client.Activities
{
    public abstract class BaseActivity : Activity
    {
        protected abstract int ViewTitleResourceId { get; }

        protected override void OnResume()
        {
            base.OnResume();

            var txt = FindViewById<TextView>(Resource.Id.ViewTitle);
            if (txt != null)
            {
                txt.Text = GetString(ViewTitleResourceId);
            }
        }
    }

    public abstract class BaseBindingActivity<TViewModel> : MvxActivity where TViewModel : BaseViewModel, IMvxViewModel
    {
		private bool _firstStart = true;
        protected abstract int ViewTitleResourceId { get; }

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

            var txt = FindViewById<TextView>(Resource.Id.ViewTitle);
            if (txt != null) txt.Text = GetString(ViewTitleResourceId);
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
    }

    public abstract class BaseListActivity : ListActivity
    {
        protected abstract int ViewTitleResourceId { get; }


        protected override void OnResume()
        {
            base.OnResume();

            var txt = FindViewById<TextView>(Resource.Id.ViewTitle);
            txt.Text = GetString(ViewTitleResourceId);
        }
    }
}