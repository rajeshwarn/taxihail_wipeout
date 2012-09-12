using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using SlidingPanel;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Activities.Setting;
using apcurium.MK.Booking.Mobile.Client.Activities.History;
using apcurium.MK.Booking.Mobile.Client.Activities.Location;
using apcurium.MK.Booking.Mobile.Client.Activities.Book;
using apcurium.MK.Booking.Mobile.AppServices;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
    [Activity(Label = "@string/ApplicationName", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : TabActivity
    {
        private int _currentCie = -1;

        private Guid? _tripToRebook = null;
        private string _addressFromFavorite;
        private bool _menuIsShown;
        private int _menuWidth = 400;
        private DecelerateInterpolator _interpolator = new DecelerateInterpolator(1.2f);

        public ReclickableTabHost MainTabHost
        {
            get
            {
                return FindViewById<ReclickableTabHost>(Android.Resource.Id.TabHost);
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            AddTab<Bookv2Activity>("bookv2", Resource.String.TabBook, Resource.Drawable.book);
            //AddTab<BookActivity>("book", Resource.String.TabBook, Resource.Drawable.book);
            //AddTab<SearchAddressActivity>("search", Resource.String.TabBook, Resource.Drawable.book);
            AddTab<LocationsActivity>("locations", Resource.String.TabLocations, Resource.Drawable.locations);
            AddTab<HistoryActivity>("history", Resource.String.TabHistory, Resource.Drawable.history);
            AddTab<SettingsActivity>("settings", Resource.String.TabSettings, Resource.Drawable.settings);


            var mainSettingsButton = FindViewById<ImageButton>(Resource.Id.MainSettingsBtn);
            mainSettingsButton.Click += MainSettingsButtonOnClick;
            InitMenu();
            _menuIsShown = false;
            
            MainTabHost.CurrentTab = 0;


            bool statusShown = ShowStatusIfLastOrderActive();
            if (!statusShown)
            {
               // ((BookActivity)LocalActivityManager.CurrentActivity).Reset();
            }


            MainTabHost.OnTabChanged += HandleMainTabHostOnTabChanged;
            FindViewById<TextView>(Resource.Id.TitleTab).Text = Resources.GetString(Resource.String.TabBook);

        }

        private void MainSettingsButtonOnClick(object sender, EventArgs eventArgs)
        {
            View v2 = FindViewById<FrameLayout>(Resource.Id.scrollinglayout);
            v2.ClearAnimation();
            v2.DrawingCacheEnabled = true;

            if (_menuIsShown)
            {
                SlideAnimation a = new SlideAnimation(v2, -(_menuWidth), 0, _interpolator);
                a.Duration = 400;
                v2.StartAnimation(a);
            }
            else
            {
                SlideAnimation a = new SlideAnimation(v2, 0, -(_menuWidth), _interpolator);
                a.Duration = 400;
                v2.StartAnimation(a);
            }

            _menuIsShown = !_menuIsShown;
        }


        void HandleMainTabHostOnTabChanged(int tab)
        {
            if (MainTabHost.CurrentTab == tab && MainTabHost.CurrentTabTag == "book")
            {
                bool statusShown = ShowStatusIfLastOrderActive();                

                if (!statusShown)
                {
                    if (_tripToRebook.HasValue)
                    {
                        ((BookActivity)LocalActivityManager.GetActivity("book")).RebookTrip(_tripToRebook.Value);
                        _tripToRebook = null;
                    }
                    else if(!string.IsNullOrEmpty(_addressFromFavorite))
                    {
                        ((BookActivity)LocalActivityManager.GetActivity("book")).BookFromFavorite(_addressFromFavorite);
                        _addressFromFavorite = null;
                    }
                    else
                    {
                        ((BookActivity)LocalActivityManager.CurrentActivity).Reset();
                    }
                }
            }


            RefreshHeader();
        }

        private bool ShowStatusIfLastOrderActive()
        {
            bool statusShown = false;
            if (AppContext.Current.LastOrder.HasValue)
            {
                var isCompleted = TinyIoCContainer.Current.Resolve<IBookingService>().IsCompleted(AppContext.Current.LastOrder.Value);
                if (!isCompleted)
                {
                    statusShown = true;
                    ((BookActivity)LocalActivityManager.CurrentActivity).StartStatusActivity(AppContext.Current.LastOrder.Value);
                }
                else
                {
                    AppContext.Current.LastOrder = null;
                }
            }
            return statusShown;
        }

        private void RefreshHeader()
        {

            FindViewById<TextView>(Resource.Id.TitleTab).Visibility = MainTabHost.CurrentTab == 0 ? ViewStates.Invisible : ViewStates.Visible;

           /* switch (MainTabHost.CurrentTab)
            {
                case 0: FindViewById<TextView>(Resource.Id.TitleTab).Text = Resources.GetString(Resource.String.TabBook);
                    FindViewById<ImageButton>(Resource.Id.BookItBtn).Visibility = ViewStates.Visible;
                    break;
                case 1: FindViewById<TextView>(Resource.Id.TitleTab).Text = Resources.GetString(Resource.String.TabLocations);
                    FindViewById<ImageButton>(Resource.Id.BookItBtn).Visibility = ViewStates.Invisible;
                    break;
                case 2: FindViewById<TextView>(Resource.Id.TitleTab).Text = Resources.GetString(Resource.String.HistoryViewTitle);
                    FindViewById<ImageButton>(Resource.Id.BookItBtn).Visibility = ViewStates.Invisible;
                    break;
                case 3: FindViewById<TextView>(Resource.Id.TitleTab).Text = Resources.GetString(Resource.String.TabSettings);
                    FindViewById<ImageButton>(Resource.Id.BookItBtn).Visibility = ViewStates.Invisible;
                    break;
                default:
                    break;
            }*/
        }

//        void HandleLogoClick(object sender, EventArgs e)
//        {
//            var intent = new Intent().SetClass(this, typeof(ChooseCompanyActivity));
//            StartActivity(intent);
//        }

        public override void OnBackPressed()
        {
            return;
        }

        private void AddTab<TActivity>(string tag, int titleId, int drawableId)
        {
            var intent = new Intent().SetClass(this, typeof(TActivity));

            var spec = MainTabHost.NewTabSpec(tag).SetIndicator(GetString(titleId), Resources.GetDrawable(drawableId)).SetContent(intent);

            MainTabHost.AddTab(spec);
        }
        protected override void OnResume()
        {
            RefreshHeader();
            RefreshLogo();
            base.OnResume();
        }

        private void RefreshLogo()
        {
            //RunOnUiThread(() =>
            //{
            //    if (_currentCie != AppContext.Current.LoggedUser.DefaultSettings.Company)
            //    {
            //        FindViewById<ImageButton>(Resource.Id.logo).SetImageResource(AppSettings.GetLogo(AppContext.Current.LoggedUser.DefaultSettings.Company));
            //        _currentCie = AppContext.Current.LoggedUser.DefaultSettings.Company;
            //    }
            //});
        }


        public void RebookTrip(Guid rebookTripId)
        {
            _tripToRebook = rebookTripId;
        }

        public void BookFromFavorites(string addressFromFavorite)
        {
            _addressFromFavorite = addressFromFavorite;
        }

        public void HideMainLayout(bool hide)
        {
            var mainLayout = FindViewById<RelativeLayout>(Resource.Id.HeaderImage);
            mainLayout.Visibility = hide ? ViewStates.Gone : ViewStates.Visible;
        }

        private void InitMenu()
        {
            List<string> menuItems = new List<string>(); 
            var g = FindViewById<GridView>(Resource.Id.gridMenu);


            for( int i = 0; i<10; i++ )
            {
                menuItems.Add( "Item " + i.ToString() );
            }
            g.Adapter = new ArrayAdapter( this, Resource.Layout.MenuItem, Resource.Id.textView1, menuItems );
            var x = WindowManager.DefaultDisplay.Width;

        }
    }

}