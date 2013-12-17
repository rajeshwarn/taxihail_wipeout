using System;
using System.Linq;
using System.Threading;
using Android.App;
using Android.Content;
using Android.GoogleMaps;
using Android.OS;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Android.Views;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Client.MapUtitilties;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Client.Converters;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Controls;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "Book Status", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class BookingStatusActivity : BaseMapActivity<BookingStatusViewModel>
    {
        private const string _doneStatus = "wosDONE";
        private const string _loadedStatus = "wosLOADED";
        private const int _refreshPeriod = 20 * 1000; //20 sec

        private Timer _timer;
        private bool _isInit = false;
        private bool _isThankYouDialogDisplayed = false;
        

        public OrderStatusDetail OrderStatus { get; private set; }
        public Order Order { get; private set; }

        protected int ViewTitleResourceId
        {
            get { return Resource.String.View_BookingStatus; }
        }

        protected override bool IsRouteDisplayed
        {
            get { return false; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_BookingStatus);
			ViewModel.Load();
        }

        protected override void OnResume()
        {
            base.OnResume();
            InitMap();
        }

        protected void InitMap()
        {
            if (_isInit)
            {
                return;
            }

            _isInit = true;

			/*var map = FindViewById<TouchMap>(Resource.Id.mapStatus);
            map.SetBuiltInZoomControls(false);
            map.Clickable = true;
            map.Traffic = false;
            map.Satellite = false;

			map.AddressSelectionMode = Data.AddressSelectionMode.None;*/

        }
    }
}
