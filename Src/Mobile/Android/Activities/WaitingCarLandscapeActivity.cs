using System.Linq;
using Android.App;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Droid.Views;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
    [Activity(Label = "@string/WaitingCarLandscapeActivityName", Theme = "@android:style/Theme.NoTitleBar.Fullscreen", ScreenOrientation = ScreenOrientation.Landscape)]
	public class WaitingCarLandscapeActivity:MvxActivity
    {
		TextView _carNumberTextView;
		string _initialContentDescription;

		public new WaitingCarLandscapeViewModel ViewModel
		{
			get
			{
				return (WaitingCarLandscapeViewModel)DataContext;
			}
		}

		protected override void OnViewModelSet()
		{
			base.OnViewModelSet();

            SetContentView(Resource.Layout.View_WaitingCarLandscape);

			_carNumberTextView = FindViewById<TextView>(Resource.Id.CarNumberTextView);
			_initialContentDescription = _carNumberTextView.ContentDescription;

			ViewModel_PropertyChanged(null, null);
			ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (ViewModel.DeviceOrientation == AppServices.DeviceOrientation.Left)
			{
				RequestedOrientation = ScreenOrientation.Landscape;
			}
			else if (ViewModel.DeviceOrientation == AppServices.DeviceOrientation.Right)
			{
				RequestedOrientation = ScreenOrientation.ReverseLandscape;
			}

			_carNumberTextView.ContentDescription = _initialContentDescription + " " + ViewModel.CarNumber;
		}
    }
}