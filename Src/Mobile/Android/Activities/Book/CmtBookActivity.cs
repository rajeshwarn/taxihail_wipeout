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
using Cirrious.MvvmCross.Binding.Android.Views;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "CmtBookActivity")]
    public class CmtBookActivity : MvxBindingMapActivityView<CmtBookViewModel>
    {
        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_BookCmt);
        }

        protected override bool IsRouteDisplayed
        {
            get { return true; }
        }
    }
}