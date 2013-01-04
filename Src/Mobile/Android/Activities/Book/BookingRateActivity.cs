using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Android.Views;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Adapters;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Client.Models;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "BookingRateActivity", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class BookingRateActivity : BaseBindingActivity<BookRatingViewModel>
    {

        private ListView _listView;
        
        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_RatePage; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _listView = FindViewById<ListView>(Resource.Id.RatingListView);
            _listView.Divider = null;
            _listView.DividerHeight = 0;
            _listView.SetPadding(10, 0, 10, 0);
        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_BookingRating);
        }
    }
}