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
using apcurium.MK.Booking.Mobile.Client.Messages;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using Android.Graphics.Drawables;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "Home", 
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
        private LinearLayout _mapContainer;
        private OrderReview _orderReview;
        private OrderEdit _orderEdit;
        private OrderOptions _orderOptions;
        private SearchAddress _searchAddress;
        private ImageView _btnLocation; 
        private ImageView _btnSettings;
        private AppBar _appBar;
        private FrameLayout _frameLayout;
        private HomeViewModelState _presentationState = HomeViewModelState.Initial;

        private int _menuWidth = 400;

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

        void PanelMenuInit()
        {
            var menu = FindViewById(Resource.Id.PanelMenu);
            menu.Visibility = ViewStates.Gone;
            _menuWidth = WindowManager.DefaultDisplay.Width - 100;

            _btnSettings.Click -= PanelMenuToggle;
            _btnSettings.Click += PanelMenuToggle;

            var signOutButton = FindViewById<View>(Resource.Id.settingsLogout);
            signOutButton.Click -= PanelMenuSignOutClick;
            signOutButton.Click += PanelMenuSignOutClick;

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

            var animation = new SlideAnimation(mainLayout, 
                ViewModel.Panel.MenuIsOpen ? 0 : (_menuWidth),
                ViewModel.Panel.MenuIsOpen ? (_menuWidth) : 0);
            animation.Duration = 400;
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

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_Home);
            ViewModel.OnViewLoaded();
            _bigButton = (Button) FindViewById(Resource.Id.BigButtonTransparent);
            _orderOptions = (OrderOptions) FindViewById(Resource.Id.orderOptions);
            _orderReview = (OrderReview) FindViewById(Resource.Id.orderReview);
            _orderEdit = (OrderEdit) FindViewById(Resource.Id.orderEdit);
            _searchAddress = (SearchAddress) FindViewById(Resource.Id.searchAddressControl);
            _appBar = (AppBar) FindViewById(Resource.Id.appBar);
            _frameLayout = (FrameLayout) FindViewById(Resource.Id.RelInnerLayout);
            _mapOverlay = (LinearLayout) FindViewById(Resource.Id.mapOverlay);
            _mapContainer = (LinearLayout) FindViewById(Resource.Id.mapContainer);
            _btnSettings = FindViewById<ImageView>(Resource.Id.btnSettings);
            _btnLocation = FindViewById<ImageView>(Resource.Id.btnLocation);

            // attach big invisible button to the OrderOptions to be able to pass it to the address text box and clear focus when clicking outside
            _orderOptions.BigInvisibleButton = _bigButton;

            ((LinearLayout.MarginLayoutParams)_orderOptions.LayoutParameters).TopMargin = 0;
            ((LinearLayout.MarginLayoutParams)_orderReview.LayoutParameters).TopMargin = WindowManager.DefaultDisplay.Height;
            ((LinearLayout.MarginLayoutParams)_orderEdit.LayoutParameters).LeftMargin = WindowManager.DefaultDisplay.Width;
            _orderReview.Visibility = ViewStates.Gone;
            _orderEdit.Visibility = ViewStates.Gone;
            _searchAddress.Visibility = ViewStates.Gone;

            // Creating a view controller for MapFragment
            Bundle mapViewSavedInstanceState = _mainBundle != null ? _mainBundle.GetBundle("mapViewSaveState") : null;
            _touchMap = (TouchableMap)SupportFragmentManager.FindFragmentById(Resource.Id.mapPickup);
            _touchMap.OnCreate(mapViewSavedInstanceState);
            _mapFragment = new OrderMapFragment(_touchMap, Resources);

            // Home View Bindings
            var binding = this.CreateBindingSet<HomeActivity, HomeViewModel>();

            binding.Bind(_mapFragment).For("DataContext").To(vm => vm.Map); // Map Fragment View Bindings
            binding.Bind(_orderOptions).For("DataContext").To(vm => vm.OrderOptions); // OrderOptions View Bindings
            binding.Bind(_orderEdit).For("DataContext").To(vm => vm.OrderEdit); // OrderEdit View Bindings
            binding.Bind(_orderReview).For("DataContext").To(vm => vm.OrderReview); // OrderReview View Bindings
            binding.Bind(_searchAddress).For("DataContext").To(vm => vm.AddressPicker); // OrderReview View Bindings
            binding.Bind(_appBar).For("DataContext").To(vm => vm.BottomBar); // AppBar View Bindings

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
            _mapFragment.Dispose();
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
            //_touchMap.OnSaveInstanceState(outState);
        }

        private void SetSelectedOnBookLater(bool selected)
        {
            var _btnBookLater = (ImageView) FindViewById(Resource.Id.btnBookLater);
            var _txtBookLater = (TextView) FindViewById(Resource.Id.txtBookLater);
            var _btnBookLaterLayout = (LinearLayout) FindViewById(Resource.Id.btnBookLaterLayout);
            _btnBookLater.Selected = selected;
            _txtBookLater.Selected = selected;
            _btnBookLaterLayout.Selected = selected;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            SetSelectedOnBookLater(false);

            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == (int)ActivityEnum.DateTimePicked && resultCode == Result.Ok)
            {             
                DateTime dt = new DateTime(data.GetLongExtra("DateTimeResult", DateTime.Now.Ticks));
                ViewModel.BottomBar.SetPickupDateAndReviewOrder.Execute(dt);
            }
            else
            {
                ViewModel.BottomBar.CancelBookLater.Execute();
            }
        }

        public override void OnLowMemory()
        {
            base.OnLowMemory();
            _touchMap.OnLowMemory();
        }

        private void SetMapEnabled(bool state)
        {

            _mapContainer = (LinearLayout) FindViewById(Resource.Id.mapContainer);
            _touchMap.Map.UiSettings.SetAllGesturesEnabled(state);
            _btnLocation.Enabled = state;
            _btnSettings.Enabled = state;

            if (!state)
            {                
//                var _size = new Size(((View)_mapContainer).Width, ((View)_mapContainer).Height);
//                if (blurryImage == null)
//                {
//                    blurryImage = new BitmapDrawable(DrawHelper.Blur(DrawHelper.LoadBitmapFromView(_mapContainer, _size)));
//                    _mapOverlay.SetBackgroundDrawable(blurryImage);
//                }
                    
                _mapOverlay.Visibility = ViewStates.Visible;
                ViewGroup parent = (ViewGroup)_mapOverlay.Parent;               
                parent.RemoveView(_mapOverlay);
                parent.AddView(_mapOverlay, 1);

            }
            else
            {
                //_mapOverlay.SetBackgroundDrawable(null);
                _mapOverlay.Visibility = ViewStates.Gone;
            }
        }

        BitmapDrawable blurryImage = null;

        public void ChangeState(HomeViewModelPresentationHint hint)
        {
            _presentationState = hint.State;
            SetMapEnabled(true);

            if (_presentationState == HomeViewModelState.PickDate)
            {
                SetMapEnabled(false);
                // Order Options: Visible
                // Order Review: Hidden
                // Order Edit: Hidden
                // Date Picker: Visible

                ((LinearLayout.MarginLayoutParams)_orderOptions.LayoutParameters).TopMargin = 0;

                SetSelectedOnBookLater(true);

                var intent = new Intent(this, typeof(DateTimePickerActivity));
                StartActivityForResult(intent, (int)ActivityEnum.DateTimePicked);
            }
            else if (_presentationState == HomeViewModelState.Review)
            {
                SetMapEnabled(false);
                // Order Options: Visible
                // Order Review: Visible
                // Order Edit: Hidden
                // Date Picker: Hidden

                var animation = AnimationHelper.GetForYTranslation(_orderReview, _orderOptions.Height);
                animation.AnimationStart += (sender, e) =>
                {
                    var desiredHeight = _frameLayout.Height - _orderOptions.Height;
                    if (((LinearLayout.MarginLayoutParams)_orderReview.LayoutParameters).Height != desiredHeight)
                    {
                        ((LinearLayout.MarginLayoutParams)_orderReview.LayoutParameters).Height = desiredHeight;
                    }
                };

                var animation2 = AnimationHelper.GetForXTranslation(_orderEdit, WindowManager.DefaultDisplay.Width);
                animation2.AnimationStart += (sender, e) =>
                {
                    if (((LinearLayout.MarginLayoutParams)_orderEdit.LayoutParameters).Width != _frameLayout.Width)
                    {
                        ((LinearLayout.MarginLayoutParams)_orderEdit.LayoutParameters).Width = _frameLayout.Width;
                    }
                };

                var animation3 = AnimationHelper.GetForYTranslation(_orderOptions, 0);

                _orderReview.StartAnimation(animation);
                _orderEdit.StartAnimation(animation2);
                _orderOptions.StartAnimation(animation3);
            }
            else if (_presentationState == HomeViewModelState.Edit)
            {
                SetMapEnabled(false);

                // Order Options: Hidden
                // Order Review: Hidden
                // Order Edit: Visible
                // Date Picker: Hidden

                var animation = AnimationHelper.GetForYTranslation(_orderReview, WindowManager.DefaultDisplay.Height);
                var animation2 = AnimationHelper.GetForXTranslation(_orderEdit, 0);
                var animation3 = AnimationHelper.GetForYTranslation(_orderOptions, -_orderOptions.Height);

                _orderReview.StartAnimation(animation);
                _orderEdit.StartAnimation(animation2);
                _orderOptions.StartAnimation(animation3);
            }
            else if (_presentationState == HomeViewModelState.AddressSearch)
            {
                SetMapEnabled(false);
                _searchAddress.Visibility = ViewStates.Visible;

            } 
            else if(_presentationState == HomeViewModelState.Initial)
            {
                SetMapEnabled(true);
                // Order Options: Visible
                // Order Review: Hidden
                // Order Edit: Hidden
                // Date Picker: Hidden

                var animation = AnimationHelper.GetForYTranslation(_orderReview, WindowManager.DefaultDisplay.Height);
                var animation2 = AnimationHelper.GetForXTranslation(_orderEdit, WindowManager.DefaultDisplay.Width);
                var animation3 = AnimationHelper.GetForYTranslation(_orderOptions, 0);

                _orderReview.StartAnimation(animation);
                _orderEdit.StartAnimation(animation2);
                _orderOptions.StartAnimation(animation3);

                SetSelectedOnBookLater(false);
            }

            _appBar.ChangePresentation(hint);
            _orderOptions.ChangePresentation(hint);
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back)
            {
                switch (_presentationState)
                {
                    case HomeViewModelState.Review:
                        ChangeState(new HomeViewModelPresentationHint(HomeViewModelState.Initial));
                        return true;
                    case HomeViewModelState.Edit:
                        ChangeState(new HomeViewModelPresentationHint(HomeViewModelState.Review));
                        return true;
                    case HomeViewModelState.PickDate:
                        ChangeState(new HomeViewModelPresentationHint(HomeViewModelState.Initial));
                        return true;
                    case HomeViewModelState.AddressSearch:
                        ChangeState(new HomeViewModelPresentationHint(HomeViewModelState.Initial));
                        return true;
                    default:
                        break;
                }
            }

            return base.OnKeyDown(keyCode, e);
        }


        void IChangePresentation.ChangePresentation(ChangePresentationHint hint)
        {
            if (hint is HomeViewModelPresentationHint)
            {
                ChangeState((HomeViewModelPresentationHint)hint);
            }

            ((IChangePresentation)_mapFragment).ChangePresentation(hint);

        }
    }
}