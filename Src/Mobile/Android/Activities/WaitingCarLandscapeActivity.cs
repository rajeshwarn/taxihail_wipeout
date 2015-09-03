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

            ViewModel_PropertyChanged(null, null);

            SetContentView(Resource.Layout.View_WaitingCarLandscape);
			ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (ViewModel.DeviceOrientation == AppServices.DeviceOrientation.Left)
                RequestedOrientation = ScreenOrientation.Landscape;
			else if (ViewModel.DeviceOrientation == AppServices.DeviceOrientation.Right)
                RequestedOrientation = ScreenOrientation.ReverseLandscape;
		}
    }
}