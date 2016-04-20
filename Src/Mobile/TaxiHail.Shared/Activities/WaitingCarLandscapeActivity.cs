using System.ComponentModel;
using apcurium.MK.Booking.Mobile.Enumeration;
using Android.App;
using Android.Content.PM;
using Android.Widget;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
    [Activity(Label = "@string/WaitingCarLandscapeActivityName", Theme = "@android:style/Theme.NoTitleBar.Fullscreen", ScreenOrientation = ScreenOrientation.Landscape)]
	public class WaitingCarLandscapeActivity : BaseBindingActivity<WaitingCarLandscapeViewModel>
    {
		private TextView _carNumberTextView;
		private string _initialContentDescription;

		protected override void OnViewModelSet()
		{
			base.OnViewModelSet();

            SetContentView(Resource.Layout.View_WaitingCarLandscape);

			_carNumberTextView = FindViewById<TextView>(Resource.Id.CarNumberTextView);
			_initialContentDescription = _carNumberTextView.ContentDescription;

            _carNumberTextView.ContentDescription = _initialContentDescription + " " + ViewModel.CarNumber;
		    UpdateDeviceOrientation();
		}

        protected override void OnStart()
        {
            base.OnStart();

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        protected override void OnStop()
        {
            base.OnStop();

            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
		    if (e.PropertyName == nameof(ViewModel.DeviceOrientation))
		    {
		        UpdateDeviceOrientation();
		    }
		    else if (e.PropertyName == nameof(ViewModel.CarNumber))
		    {
		        _carNumberTextView.ContentDescription = _initialContentDescription + " " + ViewModel.CarNumber;
		    }
		}

        private void UpdateDeviceOrientation()
        {
            if (ViewModel.DeviceOrientation == DeviceOrientations.Left)
            {
                RequestedOrientation = ScreenOrientation.Landscape;
            }
            else if (ViewModel.DeviceOrientation == DeviceOrientations.Right)
            {
                RequestedOrientation = ScreenOrientation.ReverseLandscape;
            }
        }
    }
}