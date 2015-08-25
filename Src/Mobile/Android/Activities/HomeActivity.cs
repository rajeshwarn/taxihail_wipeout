using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Animations;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets.Addresses;
using apcurium.MK.Booking.Mobile.Client.Models;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Cirrious.MvvmCross.Binding.BindingContext;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
	[Activity(Label = "@string/HomeActivityName", 
        Theme = "@style/MainTheme", 
        ScreenOrientation = ScreenOrientation.Portrait, 
        ClearTaskOnLaunch = true, 
        WindowSoftInputMode = SoftInput.AdjustPan, 
        FinishOnTaskLaunch = true, 
        LaunchMode = LaunchMode.SingleTask
    )]   
    public class HomeActivity : BaseBindingFragmentActivity<HomeViewModel>, IChangePresentation
    {
        private Button _bigButton;     
        private TouchableMap _touchMap;
        private LinearLayout _mapOverlay;
        private OrderReview _orderReview;
        private OrderEdit _orderEdit;
        private OrderOptions _orderOptions;
        private AddressPicker _searchAddress;
	    private AppBarBookingStatus _appBarBookingStatus;
		private OrderStatusView _orderStatus;
		private LinearLayout _btnLocation; 
		private LinearLayout _btnSettings;
        private AppBar _appBar;
        private FrameLayout _frameLayout;
        private int _menuWidth = 400;
        private Bundle _mainBundle;
		private readonly SerialDisposable _subscription = new SerialDisposable();

	    protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _mainBundle = bundle;

			var errorCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(ApplicationContext);

            if (errorCode == ConnectionResult.ServiceMissing || errorCode == ConnectionResult.ServiceVersionUpdateRequired || errorCode == ConnectionResult.ServiceDisabled)
            {
				var dialog = GoogleApiAvailability.Instance.GetErrorDialog(this,errorCode, 0);
                dialog.Show();
                dialog.DismissEvent += (s, e) => Finish();
            }    
        }

		public OrderMapFragment MapFragment { get; set; }

        private void PanelMenuInit()
        {
            var menu = FindViewById(Resource.Id.PanelMenu);
            menu.Visibility = ViewStates.Gone;

	        var defaultDisplaySize = new Point();

			WindowManager.DefaultDisplay.GetSize(defaultDisplaySize);

	        _menuWidth = defaultDisplaySize.X - 100;

            _btnSettings.Click -= PanelMenuToggle;
            _btnSettings.Click += PanelMenuToggle;

            var signOutButton = FindViewById<View>(Resource.Id.settingsLogout);
            signOutButton.Click -= PanelMenuSignOutClick;
            signOutButton.Click += PanelMenuSignOutClick;

			if (ViewModel.Settings.HideMkApcuriumLogos) 
			{
				var logos = FindViewById<LinearLayout>(Resource.Id.imgsLogosLayout);
				logos.Visibility = ViewStates.Invisible;
			}

            _bigButton.Touch += (sender, e) => 
            {
                if(e.Event.Action == MotionEventActions.Up)
                {
                    if(ViewModel.Panel.MenuIsOpen)
                    {
                        ViewModel.Panel.MenuIsOpen = false;
                    }
                }
            };

            ViewModel.Panel.PropertyChanged -= PanelMenuPropertyChanged;
            ViewModel.Panel.PropertyChanged += PanelMenuPropertyChanged;

            var listContainer = FindViewById<ViewGroup>(Resource.Id.listContainer);

            for (var i = 0; i < listContainer.ChildCount; i++)
            {
                if (listContainer.GetChildAt(i).GetType() == typeof(Button))
                {
                    try
                    {
                        var child = (Button) listContainer.GetChildAt(i);
                        child.SetTypeface(Typeface.Create("sans-serif-light", TypefaceStyle.Normal), TypefaceStyle.Normal);
                    }
                    catch(Exception ex)
                    {
                        Logger.LogError(ex);
                    }
                }
            }
        }

        private void PanelMenuSignOutClick(object sender, EventArgs e)
        {
            ViewModel.Panel.SignOut.ExecuteIfPossible();
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

            var animation = new SlideAnimation(mainLayout, 
                ViewModel.Panel.MenuIsOpen ? 0 : (_menuWidth),
                ViewModel.Panel.MenuIsOpen ? (_menuWidth) : 0);

            animation.Duration = GetSlidingAnimationTime();

            animation.AnimationStart += (sender, e) =>
            {
                if (ViewModel.Panel.MenuIsOpen)
                    menu.Visibility = ViewStates.Visible;
            };

            animation.AnimationEnd += (sender, e) =>
            {
                if (!ViewModel.Panel.MenuIsOpen)
                {
                    menu.Visibility = ViewStates.Gone;
                    _bigButton.Visibility = ViewStates.Gone;
                }
                else
                {
                    _bigButton.Visibility = ViewStates.Visible;
                }
            };

            mainLayout.StartAnimation(animation);
        }

        private long GetSlidingAnimationTime()
        {
            // Ugly fix warning: disable closing sliding annimation for Ice Cream Sandwich (API 15)
            // since that, due to a known problem with SupportMapFragment and sliding pannel,
            // it will sometimes render the map black
            if (Build.VERSION.Sdk == "15"
                && !ViewModel.Panel.MenuIsOpen
                && ViewModel.Panel.IsClosePanelFromMenuItem)
            {
                ViewModel.Panel.IsClosePanelFromMenuItem = false;
                return 0;
            }
            return 400;
        }

        protected override void OnViewModelSet()
		{
			base.OnViewModelSet ();
			if (ViewModel != null)
			{
				ViewModel.OnViewLoaded();
                ViewModel.SubscribeLifetimeChangedIfNecessary ();
			}

			var screenSize = new Point();

			WindowManager.DefaultDisplay.GetSize(screenSize);

            SetContentView(Resource.Layout.View_Home);

            _bigButton = (Button) FindViewById(Resource.Id.BigButtonTransparent);
            
            _orderOptions = (OrderOptions) FindViewById(Resource.Id.orderOptions);
            _orderReview = (OrderReview) FindViewById(Resource.Id.orderReview);
            _orderEdit = (OrderEdit) FindViewById(Resource.Id.orderEdit);
            _searchAddress = (AddressPicker) FindViewById(Resource.Id.searchAddressControl);
            _appBar = (AppBar) FindViewById(Resource.Id.appBar);
            _frameLayout = (FrameLayout) FindViewById(Resource.Id.RelInnerLayout);
            _mapOverlay = (LinearLayout) FindViewById(Resource.Id.mapOverlay);
			_btnSettings = FindViewById<LinearLayout>(Resource.Id.btnSettings);
			_btnLocation = FindViewById<LinearLayout>(Resource.Id.btnLocation);
	        _appBarBookingStatus = FindViewById<AppBarBookingStatus>(Resource.Id.appBarBookingStatus);
	        _orderStatus = FindViewById<OrderStatusView>(Resource.Id.orderStatus);

            // attach big invisible button to the OrderOptions to be able to pass it to the address text box and clear focus when clicking outside
            _orderOptions.BigInvisibleButton = _bigButton;

            ((ViewGroup.MarginLayoutParams)_orderOptions.LayoutParameters).TopMargin = 0;
			((ViewGroup.MarginLayoutParams)_orderReview.LayoutParameters).TopMargin = screenSize.Y;

			if (this.Services ().Localize.IsRightToLeft) {
				((ViewGroup.MarginLayoutParams)_orderEdit.LayoutParameters).RightMargin = screenSize.X;
			} else {
				((ViewGroup.MarginLayoutParams)_orderEdit.LayoutParameters).LeftMargin = screenSize.X;
			}

	        ((ViewGroup.MarginLayoutParams) _orderStatus.LayoutParameters).TopMargin = -screenSize.Y;

            // Creating a view controller for MapFragment
            var mapViewSavedInstanceState = _mainBundle != null ? _mainBundle.GetBundle("mapViewSaveState") : null;
            _touchMap = (TouchableMap)FragmentManager.FindFragmentById(Resource.Id.mapPickup);
            _touchMap.OnCreate(mapViewSavedInstanceState);
			MapFragment = new OrderMapFragment(_touchMap, Resources, this.Services().Settings);

	        _orderReview.ScreenSize = screenSize;
	        _orderReview.OrderReviewHiddenHeightProvider = () => _frameLayout.Height - _orderOptions.Height;
	        _orderReview.OrderReviewShownHeightProvider = () => _orderOptions.Height;
			_orderEdit.ScreenSize = screenSize;
	        _orderEdit.ParentFrameLayout = _frameLayout;

            SetupHomeViewBinding();

	        PanelMenuInit();
        }

	    private void SetupHomeViewBinding()
	    {
		    var set = this.CreateBindingSet<HomeActivity, HomeViewModel>();
			
			// Setup DataContext
		    set.Bind(MapFragment).For("DataContext").To(vm => vm.Map); // Map Fragment View Bindings
		    set.Bind(_orderOptions).For("DataContext").To(vm => vm.OrderOptions); // OrderOptions View Bindings
		    set.Bind(_orderEdit).For("DataContext").To(vm => vm.OrderEdit); // OrderEdit View Bindings
		    set.Bind(_orderReview).For("DataContext").To(vm => vm.OrderReview); // OrderReview View Bindings
		    set.Bind(_searchAddress).For("DataContext").To(vm => vm.AddressPicker); // OrderReview View Bindings
		    set.Bind(_appBar).For("DataContext").To(vm => vm.BottomBar); // AppBar View Bindings
		    set.Bind(_appBarBookingStatus).For("DataContext").To(vm => vm.BookingStatus.BottomBar);
		    set.Bind(_orderStatus).For("DataContext").To(vm => vm.BookingStatus);

			// Setup bookingStatusMode
		    set.Bind(MapFragment)
			    .For(v => v.TaxiLocation)
			    .To(vm => vm.BookingStatus.OrderStatusDetail);

		    set.Bind(MapFragment)
			    .For(v => v.Center)
			    .To(vm => vm.BookingStatus.MapCenter);

			//Setup Visibility
			set.Bind(_orderReview)
				.For(v => v.AnimatedVisibility)
				.To(vm => vm.CurrentViewState)
				.WithConversion("HomeViewStateToVisibility", new[] { HomeViewModelState.Review });

			set.Bind(_orderEdit)
				.For(v => v.AnimatedVisibility)
				.To(vm => vm.CurrentViewState)
				.WithConversion("HomeViewStateToVisibility", new[] { HomeViewModelState.Edit });

			set.Bind(_orderOptions)
				.For(v => v.AnimatedVisibility)
				.To(vm => vm.CurrentViewState)
				.WithConversion("HomeViewStateToVisibility", new[]
				{
					HomeViewModelState.Initial,
					HomeViewModelState.AddressSearch,
					HomeViewModelState.AirportSearch,
					HomeViewModelState.BookATaxi, 
					HomeViewModelState.Review, 
					HomeViewModelState.PickDate, 
					HomeViewModelState.TrainStationSearch, 
				});

			set.Bind(_orderStatus)
				.For(v => v.AnimatedVisibility)
				.To(vm => vm.CurrentViewState)
				.WithConversion("HomeViewStateToVisibility", new[] { HomeViewModelState.BookingStatus });

			set.Bind(_searchAddress)
				.For(v => v.Visibility)
				.To(vm => vm.CurrentViewState)
				.WithConversion("HomeViewStateToVisibility", new[]{HomeViewModelState.AddressSearch, HomeViewModelState.AirportSearch, HomeViewModelState.TrainStationSearch });
			
			set.Bind(_appBar)
				.For(v => v.Visibility)
				.To(vm => vm.CurrentViewState)
				.WithConversion("HomeViewStateToVisibility", new[] { HomeViewModelState.Initial, HomeViewModelState.Review, HomeViewModelState.Edit, HomeViewModelState.BookATaxi });

			set.Bind(_appBarBookingStatus)
				.For(v => v.Visibility)
				.To(vm => vm.CurrentViewState)
				.WithConversion("HomeViewStateToVisibility", new[] { HomeViewModelState.BookingStatus });

			var settingsAndLocationVisibleStates = new[]
		    {
			    HomeViewModelState.Initial,
			    HomeViewModelState.PickDate,
			    HomeViewModelState.BookATaxi,
			    HomeViewModelState.Edit,
			    HomeViewModelState.AddressSearch,
			    HomeViewModelState.AirportSearch,
			    HomeViewModelState.TrainStationSearch,
			    HomeViewModelState.Review
		    };

			set.Bind(_btnLocation)
				.For(v => v.Visibility)
				.To(vm => vm.CurrentViewState)
				.WithConversion("HomeViewStateToVisibility", settingsAndLocationVisibleStates);

			set.Bind(_btnSettings)
				.For(v => v.Visibility)
				.To(vm => vm.CurrentViewState)
				.WithConversion("HomeViewStateToVisibility", settingsAndLocationVisibleStates);

			//Setup Map enabled state.
		    set.Bind(_touchMap)
			    .For(v => v.IsMapGestuesEnabled)
			    .To(vm => vm.CurrentViewState)
				.WithConversion("EnumToBool", new[] { HomeViewModelState.Initial, HomeViewModelState.BookingStatus });

			set.Bind(_btnLocation)
				.For(v => v.Enabled)
				.To(vm => vm.CurrentViewState)
				.WithConversion("EnumToBool", HomeViewModelState.Initial.ToString());
		    
			set.Bind(_btnSettings)
				.For(v => v.Enabled)
				.To(vm => vm.CurrentViewState)
				.WithConversion("EnumToBool", HomeViewModelState.Initial.ToString());
			
		    set.Apply();
	    }

	    protected override void OnRestart ()
        {
            // when we come back to Home, resubscribe before calling base.OnRestart();
            if (ViewModel != null)
            {
                ViewModel.SubscribeLifetimeChangedIfNecessary ();

				_subscription.Disposable = ObserveCurrentViewState();
            }
            base.OnRestart ();
        }

        protected override void OnResume()
        { 
            base.OnResume();
            var mainLayout = FindViewById(Resource.Id.HomeLayout);
            mainLayout.Invalidate();
            _touchMap.OnResume();
        }

	    private IDisposable ObserveCurrentViewState()
	    {
		    return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
			    h => ViewModel.PropertyChanged += h,
			    h => ViewModel.PropertyChanged -= h
			    )
			    .Where(args => args.EventArgs.PropertyName.Equals("CurrentViewState"))
			    .Select(_ => ViewModel.CurrentViewState)
			    .DistinctUntilChanged()
			    .Subscribe(ChangeState, Logger.LogError);
	    }

	    protected override void OnPause()
        {
            base.OnPause();	        
            if (ViewModel != null)
            {
	            ViewModel.UnsubscribeLifetimeChangedIfNecessary();
            }
        }

        bool _locateUserOnStart;

        protected override void OnStart()
        {
            base.OnStart();
            if (ViewModel != null)
            {
                ViewModel.Start();

				_subscription.Disposable = ObserveCurrentViewState();

                if (_locateUserOnStart)
                {
                    // this happens ONLY when returning from a ride
                    ViewModel.AutomaticLocateMeAtPickup.ExecuteIfPossible(null);
                    _locateUserOnStart = false;
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (ViewModel != null)
            {
                ViewModel.UnsubscribeLifetimeChangedIfNecessary ();
            }
            MapFragment.Dispose();

			_subscription.Disposable = null;
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            // See http://code.google.com/p/gmaps-api-issues/issues/detail?id=6237 Comment #9
            // TODO: Adapt solution to C#/mvvm. Currently help to avoid a crash after tombstone but map state isn't saved

            var mapViewSaveState = new Bundle(outState);
            _touchMap.OnSaveInstanceState(mapViewSaveState);
            outState.PutBundle("mapViewSaveState", mapViewSaveState);
            base.OnSaveInstanceState(outState);
            //_touchMap.OnSaveInstanceState(outState);
        }

        private void SetSelectedOnBookLater(bool selected)
        {
            var btnBookLater = (ImageView) FindViewById(Resource.Id.btnBookLater);
            var txtBookLater = (TextView) FindViewById(Resource.Id.txtBookLater);
            var btnBookLaterLayout = (LinearLayout) FindViewById(Resource.Id.btnBookLaterLayout);
            btnBookLater.Selected = selected;
            txtBookLater.Selected = selected;
            btnBookLaterLayout.Selected = selected;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            SetSelectedOnBookLater(false);

            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == (int)ActivityEnum.DateTimePicked && resultCode == Result.Ok)
            {             
                var dt = new DateTime(data.GetLongExtra("DateTimeResult", DateTime.Now.Ticks));
                ViewModel.BottomBar.SetPickupDateAndReviewOrder.ExecuteIfPossible(dt);
            }
            else if (requestCode == (int) ActivityEnum.BookATaxi && resultCode == Result.Ok)
            {
                var result = (BookATaxiEnum)data.GetIntExtra("BookATaxiResult", (int)BookATaxiEnum.BookCancelled);
                switch (result)
                {
                    case BookATaxiEnum.BookNow:
                        ViewModel.BottomBar.SetPickupDateAndReviewOrder.ExecuteIfPossible();
                        break;
                    case BookATaxiEnum.BookLater:
                        ViewModel.BottomBar.BookLater.ExecuteIfPossible();
                        break;
                    default:
                        ViewModel.BottomBar.ResetToInitialState.ExecuteIfPossible();
                        break;
                }
            }
            else
            {
                ViewModel.BottomBar.ResetToInitialState.ExecuteIfPossible();
            }
        }

        public override void OnLowMemory()
        {
            base.OnLowMemory();
            _touchMap.OnLowMemory();
        }

		private void ChangeState(HomeViewModelState state)
        {
			if (state == HomeViewModelState.PickDate)
            {
                ((ViewGroup.MarginLayoutParams)_orderOptions.LayoutParameters).TopMargin = 0;

                SetSelectedOnBookLater(true);

                var intent = new Intent(this, typeof(DateTimePickerActivity));
                StartActivityForResult(intent, (int)ActivityEnum.DateTimePicked);
            }
			else if (state == HomeViewModelState.BookATaxi)
            {
                ((ViewGroup.MarginLayoutParams)_orderOptions.LayoutParameters).TopMargin = 0;

                var intent = new Intent(this, typeof (OrderBookingOptionsDialogActivity));
                StartActivityForResult(intent, (int)ActivityEnum.BookATaxi);
            }
			else if (state == HomeViewModelState.AddressSearch)
            {
                _searchAddress.Open(AddressLocationType.Unspeficied);
            }
			else if (state == HomeViewModelState.AirportSearch)
            {
                _searchAddress.Open(AddressLocationType.Airport);
            }
			else if (state == HomeViewModelState.TrainStationSearch)
            {
                _searchAddress.Open(AddressLocationType.Train);
            }
			else if (state == HomeViewModelState.Initial)
            {			
                _searchAddress.Close();

                SetSelectedOnBookLater(false);
            }
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (!base.OnKeyDown(keyCode, e))
            {
                return false;
            }

			if (keyCode == Keycode.Back && ViewModel.CanUseCloseCommand())
            {
	            ViewModel.CloseCommand.ExecuteIfPossible();

	            return false;
            }

            return base.OnKeyDown(keyCode, e);
        }

        public void ChangePresentation(ChangePresentationHint hint)
        {
            MapFragment.ChangePresentation(hint);
        }
    }
}