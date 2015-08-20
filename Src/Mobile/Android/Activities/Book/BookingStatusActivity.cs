using Android.App;
using Android.Content.PM;
using Android.Gms.Common;
using Google.Android.M4b.Maps;
using Android.OS;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using apcurium.MK.Booking.Mobile.ViewModels;
using Android.Widget;
using System.Windows.Input;
using Android.Views;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
	[Activity(Label = "@string/BookStatusActivityName", Theme = "@style/MainTheme", ScreenOrientation = ScreenOrientation.Portrait)]
    public class BookingStatusActivity : BaseBindingActivity<BookingStatusViewModel>
    {
        private TouchMap _touchMap;

        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                MapsInitializer.Initialize(ApplicationContext);
            }
            catch (GooglePlayServicesNotAvailableException e)
            {
                Logger.LogError(e);
            }

            base.OnCreate(bundle);
            _touchMap.OnCreate(bundle);
            _touchMap.ViewTreeObserver.AddOnGlobalLayoutListener(new LayoutObserverForMap(_touchMap));	        
        }        

		protected override void OnViewModelSet()
		{
			base.OnViewModelSet ();
            SetContentView(Resource.Layout.View_BookingStatus);
            _touchMap = FindViewById<TouchMap>(Resource.Id.mapStatus);
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (!base.OnKeyDown(keyCode, e))
            {
                return false;
            }

            if (keyCode == Keycode.Back)
            {
                if (!ViewModel.CanGoBack)
                {
                    MoveTaskToBack(true);
                    return false;
                }
            }

            return base.OnKeyDown(keyCode, e);
        }

        protected override void OnResume()
        {
            base.OnResume();

            _touchMap.OnResume();
        }

        protected override void OnPause()
        {
            base.OnPause();
            _touchMap.Pause();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _touchMap.OnDestroy();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            _touchMap.OnSaveInstanceState(outState);
        }

        public override void OnLowMemory()
        {
            base.OnLowMemory();

            _touchMap.OnLowMemory();
        }
    }
}