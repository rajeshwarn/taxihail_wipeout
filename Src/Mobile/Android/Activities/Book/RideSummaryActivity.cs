using System;
using Android.App;
using Android.Content.PM;
using Cirrious.MvvmCross.Binding.Android.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Activities;
using apcurium.MK.Booking.Mobile.Client;
using apcurium.MK.Booking.Mobile.Client.Controls;

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

//            ViewModel.PropertyChanged += (sender, e) => 
//            {
//                if(ViewModel.ReceiptSent)
//                {
//                    var sendReceiptBtn = FindViewById<StyledButton>(Resource.Id.SendReceiptBtn);
//                    sendReceiptBtn.SetText("Receipt sent", Android.Widget.TextView.BufferType.Normal);
//                    sendReceiptBtn.Enabled = false;
//                }
//            };
		}
	}
}

