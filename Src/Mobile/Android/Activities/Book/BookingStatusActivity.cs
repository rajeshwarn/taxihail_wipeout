using System;
using System.Linq;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Android.Views;
using TinyIoC;
using TinyMessenger;
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
using Android.Gms.Maps;
using Android.Gms.Common;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "Book Status", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class BookingStatusActivity : BaseBindingActivity<BookingStatusViewModel>
    {
        private const string _doneStatus = "wosDONE";
        private const string _loadedStatus = "wosLOADED";
        private const int _refreshPeriod = 20 * 1000; //20 sec
		private TouchMap _touchMap;

        public OrderStatusDetail OrderStatus { get; private set; }
        public Order Order { get; private set; }

		protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_BookingStatus; }
        }
		       

        protected override void OnCreate(Bundle bundle)
        {
			try
			{
				MapsInitializer.Initialize(this.ApplicationContext);
			}
			catch (GooglePlayServicesNotAvailableException e)
			{
				Logger.LogError(e);
			}

			base.OnCreate(bundle);
			_touchMap.OnCreate(bundle);
			_touchMap.ViewTreeObserver.AddOnGlobalLayoutListener(new LayoutObserverForMap(_touchMap));			
        }        

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_BookingStatus);
			_touchMap = FindViewById<TouchMap>(Resource.Id.mapStatus);

			ViewModel.Load();
        }

        protected override void OnResume()
        {
            base.OnResume();

			_touchMap.OnResume();
        }

		protected override void OnPause()
		{
			base.OnPause();

			_touchMap.Pause();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			_touchMap.OnDestroy();
		}

		protected override void OnSaveInstanceState(Bundle outState)
		{
			base.OnSaveInstanceState(outState);

			_touchMap.OnSaveInstanceState(outState);
		}

		public override void OnLowMemory()
		{
			base.OnLowMemory();

			_touchMap.OnLowMemory();
		}

    }
}
