using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Microsoft.Practices.ServiceLocation;
using TaxiMobile.Activities.Book;
using TaxiMobile.Activities.History;
using TaxiMobile.Activities.Location;
using TaxiMobile.Activities.Setting;
using TaxiMobile.Controls;
using TaxiMobileApp;

namespace TaxiMobile.Activities
{
    [Activity(Label = "@string/ApplicationName", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : TabActivity
    {
        private int _currentCie = -1;

        private int? _tripToRebook = null;

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

            AddTab<BookActivity>("book", Resource.String.TabBook, Resource.Drawable.book);
            AddTab<LocationsActivity>("locations", Resource.String.TabLocations, Resource.Drawable.locations);
            AddTab<HistoryActivity>("history", Resource.String.TabHistory, Resource.Drawable.history);
            AddTab<SettingsActivity>("settings", Resource.String.TabSettings, Resource.Drawable.settings);

            MainTabHost.CurrentTab = 0;
            ((BookActivity)LocalActivityManager.CurrentActivity).Reset();
            MainTabHost.OnTabChanged += HandleMainTabHostOnTabChanged;


            if (AppContext.Current.LoggedUser != null)
            {
                FindViewById<ImageButton>(Resource.Id.logo).SetImageResource(AppSettings.GetLogo(AppContext.Current.LoggedUser.DefaultSettings.Company));
            }
            else
            {
                FindViewById<ImageButton>(Resource.Id.logo).SetImageResource(AppSettings.GetLogo(int.MaxValue));
            }
            FindViewById<ImageButton>(Resource.Id.logo).Click += HandleLogoClick;
            FindViewById<TextView>(Resource.Id.TitleTab).Text = Resources.GetString(Resource.String.TabBook);

        }

        void HandleMainTabHostOnTabChanged(int tab)
        {
            if (MainTabHost.CurrentTab == tab && MainTabHost.CurrentTabTag == "book")
            {
                bool statusShown = false;
                if (AppContext.Current.LastOrder.HasValue)
                {
                    var isCompleted = ServiceLocator.Current.GetInstance<IBookingService>().IsCompleted(AppContext.Current.LoggedUser, AppContext.Current.LastOrder.Value);
                    if (!isCompleted)
                    {
                        statusShown = true;
                        ((BookActivity)LocalActivityManager.CurrentActivity).StartStatusActivity(
                            AppContext.Current.LastOrder.Value);
                    }
                    else
                    {
                        AppContext.Current.LastOrder = null;
                    }
                }

                if (!statusShown)
                {
                    if (!_tripToRebook.HasValue)
                    {
                        ((BookActivity)LocalActivityManager.CurrentActivity).Reset();
                    }
                    else
                    {
                        ((BookActivity)LocalActivityManager.GetActivity("book")).RebookTrip(_tripToRebook.Value);
                        _tripToRebook = null;
                    }
                }
            }


            RefreshHeader();
        }

        private void RefreshHeader()
        {

            FindViewById<TextView>(Resource.Id.TitleTab).Visibility = MainTabHost.CurrentTab == 0 ? ViewStates.Invisible : ViewStates.Visible;

            switch (MainTabHost.CurrentTab)
            {
                case 0: FindViewById<TextView>(Resource.Id.TitleTab).Text = Resources.GetString(Resource.String.TabBook);
                    FindViewById<Button>(Resource.Id.BookItBtn).Visibility = ViewStates.Visible;
                    break;
                case 1: FindViewById<TextView>(Resource.Id.TitleTab).Text = Resources.GetString(Resource.String.TabLocations);
                    FindViewById<Button>(Resource.Id.BookItBtn).Visibility = ViewStates.Invisible;
                    break;
                case 2: FindViewById<TextView>(Resource.Id.TitleTab).Text = Resources.GetString(Resource.String.HistoryViewTitle);
                    FindViewById<Button>(Resource.Id.BookItBtn).Visibility = ViewStates.Invisible;
                    break;
                case 3: FindViewById<TextView>(Resource.Id.TitleTab).Text = Resources.GetString(Resource.String.TabSettings);
                    FindViewById<Button>(Resource.Id.BookItBtn).Visibility = ViewStates.Invisible;
                    break;
                default:
                    break;
            }
        }
        
        void HandleLogoClick(object sender, EventArgs e)
        {
            var intent = new Intent().SetClass(this, typeof(ChooseCompanyActivity));
            StartActivity(intent);
        }

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
            RunOnUiThread(() =>
            {
                if (_currentCie != AppContext.Current.LoggedUser.DefaultSettings.Company)
                {
                    FindViewById<ImageButton>(Resource.Id.logo).SetImageResource(AppSettings.GetLogo(AppContext.Current.LoggedUser.DefaultSettings.Company));
                    _currentCie = AppContext.Current.LoggedUser.DefaultSettings.Company;
                }
            });
        }


        public void RebookTrip(int rebookTripId)
        {

            _tripToRebook = rebookTripId;
        }
    }
}