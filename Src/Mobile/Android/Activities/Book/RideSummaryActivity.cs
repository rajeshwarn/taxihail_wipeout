using System;
using Android.App;
using Android.Content.PM;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
	[Activity(Label = "Book", Theme = "@style/MainTheme", ScreenOrientation = ScreenOrientation.Portrait,
        ClearTaskOnLaunch = true, FinishOnTaskLaunch = true)]
    public class RideSummaryActivity : BaseBindingActivity<RideSummaryViewModel>
    {
		protected override void OnViewModelSet()
		{
			base.OnViewModelSet ();

            SetContentView(Resource.Layout.View_Book_RideSummaryPage);
            var lblSubTitle = FindViewById<TextView>(Resource.Id.lblSubTitle);
			lblSubTitle.Text = String.Format(this.Services().Localize["RideSummarySubTitleText"], this.Services().Settings.ApplicationName);

            ViewModel.PropertyChanged += (sender, e) =>
            {
                if (ViewModel.ReceiptSent)
                {
					var sendReceiptBtn = FindViewById<Button>(Resource.Id.SendReceiptBtn);
                    sendReceiptBtn.SetText(Resource.String.ReceiptSent, TextView.BufferType.Normal);
                    sendReceiptBtn.Enabled = false;
                }
            };
        }
    }
}