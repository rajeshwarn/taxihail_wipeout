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
using apcurium.MK.Booking.Mobile.Client.Controls;
using System.Reactive.Linq;
using System.Reactive;
using SlidingPanel;
using Android.Views.Animations;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
	[Activity(Label = "CmtBookActivity", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, ClearTaskOnLaunch = true, FinishOnTaskLaunch = true  )]
    public class CmtHomeActivity : MvxBindingMapActivityView<CmtHomeViewModel>
    {
		private int _menuWidth = 400;
		private DecelerateInterpolator _interpolator = new DecelerateInterpolator(0.9f);

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_HomeCmt);

			var mainSettingsButton = FindViewById<HeaderedLayout>(Resource.Id.MainLayout).FindViewById<ImageButton>(Resource.Id.ViewNavBarRightButton);
			mainSettingsButton.Click -= MainSettingsButtonOnClick;
			mainSettingsButton.Click += MainSettingsButtonOnClick;

			var headerLayoutMenu = FindViewById<HeaderedLayout>(Resource.Id.HeaderLayoutMenu);
			var titleText = headerLayoutMenu.FindViewById<TextView>(Resource.Id.ViewTitle);
			titleText.Text = GetString(Resource.String.View_BookSettingMenu);

			var menu = FindViewById(Resource.Id.BookSettingsMenu);
			menu.Visibility = ViewStates.Gone;
			_menuWidth = WindowManager.DefaultDisplay.Width - 100;

			var signOutButton = FindViewById<View>(Resource.Id.settingsLogout);
			signOutButton.Click -= HandleSignOutButtonClick;
			signOutButton.Click += HandleSignOutButtonClick;
			
			ViewModel.Panel.PropertyChanged -= HandlePropertyChanged;
			ViewModel.Panel.PropertyChanged += HandlePropertyChanged;

			ViewModel.Load();

        }
		private void MainSettingsButtonOnClick(object sender, EventArgs eventArgs)
		{
			ViewModel.Panel.MenuIsOpen = !ViewModel.Panel.MenuIsOpen;
		}

		void HandleSignOutButtonClick (object sender, EventArgs e)
		{
			ViewModel.Panel.SignOut.Execute();
			// Finish the activity, because clearTop does not seem to be enough in this case
			// Finish is delayed 1sec in order to prevent the application from being terminated
			Observable.Return(Unit.Default).Delay (TimeSpan.FromSeconds(1)).Subscribe(x=>{
				Finish();
			});
		}
		
		void HandlePropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "MenuIsOpen") {
				AnimateMenu();
			}
		}

		private void AnimateMenu()
		{
			var mainLayout = FindViewById(Resource.Id.MainLayout);
			mainLayout.ClearAnimation();
			mainLayout.DrawingCacheEnabled = true;
			
			var menu = FindViewById(Resource.Id.BookSettingsMenu);
			
			var animation = new SlideAnimation(mainLayout, ViewModel.Panel.MenuIsOpen ? 0: -(_menuWidth), ViewModel.Panel.MenuIsOpen ? -(_menuWidth): 0, _interpolator);
			animation.Duration = 400;
			animation.AnimationStart +=	 (sender, e) => {
				if(ViewModel.Panel.MenuIsOpen) menu.Visibility = ViewStates.Visible;
			};
			animation.AnimationEnd +=	 (sender, e) => {
				if(!ViewModel.Panel.MenuIsOpen) menu.Visibility = ViewStates.Gone;
			};
			
			mainLayout.StartAnimation(animation);
		}

        protected override bool IsRouteDisplayed
        {
            get { return true; }
        }
    }
}