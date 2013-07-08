using System;
using Android.App;
using Android.Content.PM;
using Cirrious.MvvmCross.Binding.Android.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Activities;
using apcurium.MK.Booking.Mobile.Client;

namespace Activities.Book
{
	[Activity(Label = "Book", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = ScreenOrientation.Portrait, ClearTaskOnLaunch = true, FinishOnTaskLaunch = true  )]
	public class RideSummaryActivity : BaseBindingActivity<RideSummaryViewModel>
	{
		protected override int ViewTitleResourceId {
			get {
				return Resource.String.View_BookingStatus_ThankYouTitle; 
			}
		}

		public RideSummaryActivity ()
		{
		}


		protected override void OnViewModelSet ()
		{
			
			SetContentView(Resource.Layout.View_Book_RideSummaryPage);
		}



	}
}

