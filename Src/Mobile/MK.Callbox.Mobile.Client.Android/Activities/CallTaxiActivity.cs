using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Android.Views;
using apcurium.MK.Booking.Mobile.ViewModels.Callbox;

namespace apcurium.MK.Callbox.Mobile.Client.Activities
{
    [Activity(Label = "Call Taxi", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape,LaunchMode = Android.Content.PM.LaunchMode.SingleInstance )]
    public class CallTaxiActivity : MvxBindingActivityView<CallboxCallTaxiViewModel>
    {
        private const int NbClick = 5;
        private static  TimeSpan TimeOut = TimeSpan.FromSeconds(5);
        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_CallTaxi);
            var logoutButton = this.FindViewById<Button>(Resource.Id.LogoutButton);
                                  Observable.FromEventPattern<View.TouchEventArgs>(logoutButton, "Touch")
                      .Where(e => e.EventArgs.Event.Action == MotionEventActions.Down)
                      .Buffer(TimeOut,NbClick)
                      .Where(s => s.Count == 5)
                      .Subscribe(_ => RunOnUiThread(() => ViewModel.Logout.Execute()));

            ViewModel.Load();
        }
    }
}