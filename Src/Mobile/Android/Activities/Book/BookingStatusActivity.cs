using Android.App;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Gms.Maps;
using Android.OS;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;

using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "Book Status", Theme = "@style/MainTheme", ScreenOrientation = ScreenOrientation.Portrait)]
    public class BookingStatusActivity : BaseBindingActivity<BookingStatusViewModel>
    {
        //private TouchMap _touchMap;
        private OrderMapView _touchMap;

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
	        //_touchMap.ViewTreeObserver.AddOnGlobalLayoutListener(new LayoutObserverForMap(_touchMap));			
        }        

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_BookingStatus);
            //_touchMap = FindViewById<OrderMapView>(Resource.Id.mapStatus);

            ViewModel.OnViewLoaded();
        }

        protected override void OnResume()
        {
            base.OnResume();

            _touchMap.OnResume();
        }

        protected override void OnPause()
        {
            base.OnPause();

	    //_touchMap.Pause();
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