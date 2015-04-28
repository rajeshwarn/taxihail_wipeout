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

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "@string/HomeView_BookTaxi", Theme = "@android:style/Theme.Dialog")]
    public class BookATaxiDialogActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.View_BookATaxiDialog);
        }
    }
}