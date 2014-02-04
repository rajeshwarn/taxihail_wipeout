using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Gms.Maps;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Cirrious.MvvmCross.Droid.Views;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Animations;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.Models;
using Android.Support.V4.App;
using Cirrious.MvvmCross.Droid.Fragging;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
	[Activity(Label = "Home", Theme = "@style/MainTheme",
        ScreenOrientation = ScreenOrientation.Portrait, ClearTaskOnLaunch = true,
        FinishOnTaskLaunch = true)]
    public class HomeActivity : BaseBindingFragmentActivity<HomeViewModel>
    {
        private SupportMapFragment _touchMap;
        private OrderReview _orderReview;
        private OrderOptions _orderOptions;
        private readonly DecelerateInterpolator _interpolator = new DecelerateInterpolator(0.9f);

        public new HomeViewModel ViewModel
		{
			get
			{
				return (HomeViewModel)DataContext;
			}
		}

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var errorCode = GooglePlayServicesUtil.IsGooglePlayServicesAvailable(ApplicationContext);
            if (errorCode == ConnectionResult.ServiceMissing
                || errorCode == ConnectionResult.ServiceVersionUpdateRequired
                || errorCode == ConnectionResult.ServiceDisabled)
            {
                var dialog = GooglePlayServicesUtil.GetErrorDialog(errorCode, this, 0);
                dialog.Show();
                dialog.DismissEvent += (s, e) => Finish();
            }
        }

        public OrderMapFragment _mapFragment; 


        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_Home);
            ViewModel.OnViewLoaded();
            _touchMap = (SupportMapFragment)SupportFragmentManager.FindFragmentById(Resource.Id.mapPickup);
            _orderOptions = (OrderOptions) FindViewById(Resource.Id.orderOptions);
            _orderReview = (OrderReview) FindViewById(Resource.Id.orderReview);

            // Creating a view controller for MapFragment
            _mapFragment = new OrderMapFragment(_touchMap);

            // Home View Bindings
            var binding = this.CreateBindingSet<HomeActivity, HomeViewModel>();

            binding.Bind(_mapFragment).For("DataContext").To(vm => vm.Map); // Map Fragment View Bindings
            binding.Bind(_orderOptions).For("DataContext").To(vm => vm.OrderOptions); // Map OrderOptions View Bindings

            var bookNow = FindViewById<Button>(Resource.Id.btnBookNow);
            binding
                .Bind(bookNow)
                .For("Click")
                .To(vm => vm.BottomBar.BookNow);

            binding.Apply();
        }

        protected override void OnResume()
        { 
            base.OnResume();
            TinyIoCContainer.Current.Resolve<AbstractLocationService>().Start();

            var mainLayout = FindViewById(Resource.Id.HomeMainLayout);
            mainLayout.Invalidate();
            _touchMap.OnResume();
        }

        protected override void OnPause()
        {
            base.OnPause();	        
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (ViewModel != null) ViewModel.Start();
        }

        protected override void OnStop()
        {
            base.OnStop();
            if (ViewModel != null) ViewModel.OnViewStopped();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (ViewModel != null)
            {
                ViewModel.OnViewUnloaded();
            }

            //_touchMap.OnDestroy();
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

        TranslateAnimation _animation;

        public void ShowOrderReview()
        {
            _animation = new TranslateAnimation(0, 0, 0, -500);
            _animation.Duration = 600;
            _animation.Interpolator = new DecelerateInterpolator();
            _animation.FillAfter = true;
            _orderReview.StartAnimation(_animation);
           
        }

        void HandleAnimationEnd (object sender, Animation.AnimationEndEventArgs e)
        {


        }


    }
}