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
        private int _menuWidth = 400;
        private readonly DecelerateInterpolator _interpolator = new DecelerateInterpolator(0.9f);


        public new HomeViewModel ViewModel
		{
			get
			{
				return (HomeViewModel)DataContext;
			}
		}

        private Bundle _mainBundle;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _mainBundle = bundle;

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

        public OrderOptions _orderOptions;

        void PanelMenuInit()
        {
            var menu = FindViewById(Resource.Id.PanelMenu);
            menu.Visibility = ViewStates.Gone;
            _menuWidth = WindowManager.DefaultDisplay.Width - 100;

            var mainSettingsButton = FindViewById<ImageView>(Resource.Id.btnSettings);

            mainSettingsButton.Click -= PanelMenuToggle;
            mainSettingsButton.Click += PanelMenuToggle;


            var signOutButton = FindViewById<View>(Resource.Id.settingsLogout);
            signOutButton.Click -= PanelMenuSignOutClick;
            signOutButton.Click += PanelMenuSignOutClick;

            var bigButton = FindViewById<View>(Resource.Id.BigButtonTransparent);
            bigButton.Click -= PanelMenuToggle;
            bigButton.Click += PanelMenuToggle;

            ViewModel.Panel.PropertyChanged -= PanelMenuPropertyChanged;
            ViewModel.Panel.PropertyChanged += PanelMenuPropertyChanged;

            var _listContainer = FindViewById<ViewGroup>(Resource.Id.listContainer);

            for (int i = 0; i < _listContainer.ChildCount; i++)
            {
                if (_listContainer.GetChildAt(i).GetType() == typeof(Button))
                {
                    try
                    {
                        Button child = (Button)_listContainer.GetChildAt(i);
                        child.SetTypeface(Typeface.Create("sans-serif-light", TypefaceStyle.Normal), TypefaceStyle.Normal);
                    }
                    catch
                    {
                    
                    }
                }
            }
        }

        private void PanelMenuSignOutClick(object sender, EventArgs e)
        {
            ViewModel.Panel.SignOut.Execute(null);
            Observable.Return(Unit.Default).Delay(TimeSpan.FromSeconds(1)).Subscribe(x => { Finish(); });
        }

        private void PanelMenuToggle(object sender, EventArgs eventArgs)
        {
            ViewModel.Panel.MenuIsOpen = !ViewModel.Panel.MenuIsOpen;
        }

        private void PanelMenuPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MenuIsOpen")
            {
                PanelMenuAnimate();
            }
        }

        private void PanelMenuAnimate()
        {
            var mainLayout = FindViewById(Resource.Id.HomeLayout);
            mainLayout.ClearAnimation();


            var menu = FindViewById(Resource.Id.PanelMenu);

            var animation = new SlideAnimation(mainLayout, ViewModel.Panel.MenuIsOpen ? 0 : (_menuWidth),
                ViewModel.Panel.MenuIsOpen ? (_menuWidth) : 0, _interpolator);
            animation.Duration = 400;
            animation.AnimationStart +=
                (sender, e) =>
            {
                if (ViewModel.Panel.MenuIsOpen)
                    menu.Visibility = ViewStates.Visible;
            };

            animation.AnimationEnd +=
                (sender, e) =>
            {
                if (!ViewModel.Panel.MenuIsOpen)
                {
                    menu.Visibility = ViewStates.Gone;
                }
                else
                {

                }
            };

            mainLayout.StartAnimation(animation);

        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_Home);
            ViewModel.OnViewLoaded();
            _orderOptions = (OrderOptions) FindViewById(Resource.Id.orderOptions);

            // Creating a view controller for MapFragment
            Bundle mapViewSavedInstanceState = _mainBundle != null ? _mainBundle.GetBundle("mapViewSaveState") : null;
            _touchMap = (SupportMapFragment)SupportFragmentManager.FindFragmentById(Resource.Id.mapPickup);
            _touchMap.OnCreate(mapViewSavedInstanceState);
            _mapFragment = new OrderMapFragment(_touchMap);

            // Home View Bindings
            var binding = this.CreateBindingSet<HomeActivity, HomeViewModel>();
            binding.Bind(_mapFragment).For("DataContext").To(vm => vm.Map); // Map Fragment View Bindings
            binding.Bind(_orderOptions).For("DataContext").To(vm => vm.OrderOptions); // Map OrderOptions View Bindings
            binding.Apply();

            PanelMenuInit();
        }

        protected override void OnResume()
        { 
            base.OnResume();
            TinyIoCContainer.Current.Resolve<AbstractLocationService>().Start();

            var mainLayout = FindViewById(Resource.Id.HomeLayout);
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
            // See http://code.google.com/p/gmaps-api-issues/issues/detail?id=6237 Comment #9
            // TODO: Adapt solution to C#/mvvm. Currently help to avoid a crash after tombstone but map state isn't saved

            Bundle mapViewSaveState = new Bundle(outState);
            _touchMap.OnSaveInstanceState(mapViewSaveState);
            outState.PutBundle("mapViewSaveState", mapViewSaveState);
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