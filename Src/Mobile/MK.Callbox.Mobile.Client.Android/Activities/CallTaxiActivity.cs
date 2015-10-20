using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Extensions;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Droid.Views;
using apcurium.MK.Booking.Mobile.ViewModels.Callbox;
using Cirrious.CrossCore.Core;
using apcurium.MK.Callbox.Mobile.Client.Extensions;

namespace apcurium.MK.Callbox.Mobile.Client.Activities
{
    [Activity(Label = "Call Taxi", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape,LaunchMode = Android.Content.PM.LaunchMode.SingleInstance )]
    public class CallTaxiActivity : MvxActivity
    {
        private const int NbClick = 5;
        private static readonly TimeSpan TimeOut = TimeSpan.FromSeconds(5);

		private readonly SerialDisposable _subscription = new SerialDisposable();

		public new CallboxCallTaxiViewModel ViewModel
		{
			get
			{
				return (CallboxCallTaxiViewModel)DataContext;
			}
		}

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_CallTaxi);
            var logoutButton = FindViewById<Button>(Resource.Id.LogoutButton);

			_subscription.Disposable = Observable.FromEventPattern<View.TouchEventArgs>(logoutButton, "Touch")
				.Where(e => e.EventArgs.Event.Action == MotionEventActions.Down)
                .Buffer(TimeOut, NbClick)
                .Where(s => s.Count == 5)
				.ObserveOn(SynchronizationContext.Current)
				.SubscribeAndLogErrors(_ => ViewModel.Logout.ExecuteIfPossible());

			ViewModel.OnViewLoaded();
        }

	    protected override void OnDestroy()
	    {
		    base.OnDestroy();

			_subscription.DisposeIfDisposable();
	    }
    }
}