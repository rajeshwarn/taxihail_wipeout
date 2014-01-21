using Android.App;
using Android.Content.PM;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Controls;

using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
	[Activity(Label = "Book", Theme = "@style/MainTheme", ScreenOrientation = ScreenOrientation.Portrait,
        ClearTaskOnLaunch = true, FinishOnTaskLaunch = true)]
    public class RideSummaryActivity : BaseBindingActivity<RideSummaryViewModel>
    {
        protected override int ViewTitleResourceId
        {
            get { return Resource.String.EmptyString; }
        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_Book_RideSummaryPage);

            ViewModel.PropertyChanged += (sender, e) =>
            {
                if (ViewModel.ReceiptSent)
                {
					var sendReceiptBtn = FindViewById<Button>(Resource.Id.SendReceiptBtn);
                    sendReceiptBtn.SetText(Resource.String.HistoryViewSendReceiptSuccess,
                        TextView.BufferType.Normal);
                    sendReceiptBtn.Enabled = false;
                }
            };
        }
    }
}