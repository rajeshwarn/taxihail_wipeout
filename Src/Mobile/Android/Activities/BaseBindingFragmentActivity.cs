using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Droid.Fragging;
using Cirrious.MvvmCross.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
    public abstract class BaseBindingFragmentActivity<TViewModel> : MvxFragmentActivity
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

        public override bool OnKeyDown(Android.Views.Keycode keyCode, Android.Views.KeyEvent e)
        {
            if (e.KeyCode == Android.Views.Keycode.Back)
            {
                if (Intent.Categories != null && Intent.Categories.Contains("Progress"))
                {
                    return false;
                }
            }
            return base.OnKeyDown(keyCode, e);
        }      

        protected override void OnResume()
        {
            base.OnResume();
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

            if (ViewModel != null)
            {
                ViewModel.OnViewStopped();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (ViewModel != null)
            {
                ViewModel.OnViewUnloaded ();
            }
        }
    }
}