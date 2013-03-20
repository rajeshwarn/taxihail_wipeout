using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "Book Details", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class BookEditInformationActivity : BaseBindingActivity<BookEditInformationViewModel>
    {
        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_BookingDetail; }
        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_BookEditInformation);
            FindViewById<LinearLayout>(Resource.Id.passengerNameInfoLayout).Visibility = ViewModel.ShowPassengerName ? ViewStates.Visible : ViewStates.Gone;
            FindViewById<LinearLayout>(Resource.Id.passengerPhoneInfoLayout).Visibility = ViewModel.ShowPassengerPhone ? ViewStates.Visible : ViewStates.Gone;
            FindViewById<LinearLayout>(Resource.Id.passengerNumberInfoLayout).Visibility = ViewModel.ShowPassengerNumber ? ViewStates.Visible : ViewStates.Gone;
            ViewModel.Load();
        }
    }
}
