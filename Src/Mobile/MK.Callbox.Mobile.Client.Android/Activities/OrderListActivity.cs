using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Input;
using Android.App;
using Android.Media;
using Android.OS;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Droid.Views;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels.Callbox;
using apcurium.MK.Callbox.Mobile.Client.Extensions;

namespace apcurium.MK.Callbox.Mobile.Client.Activities
{
	[Activity(Label = "Order List", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
	public class OrderListActivity : MvxActivity
	{
		private const int NbClick = 5;
		private TimeSpan TimeOut = TimeSpan.FromSeconds(5);
		private TimeSpan BlinkMs = TimeSpan.FromMilliseconds(500);

		IDisposable _blinkTimer;

		private MediaPlayer _mediaPlayer;

		public new CallboxOrderListViewModel ViewModel
		{
			get
			{
				return (CallboxOrderListViewModel)DataContext;
			}
		}

		protected override void OnViewModelSet()
		{
			SetContentView(Resource.Layout.View_OrderList);


			var logoutButton = FindViewById<Button>(Resource.Id.LogoutButton);
			Observable.FromEventPattern<View.TouchEventArgs>(logoutButton, "Touch")
				.Where(e => e.EventArgs.Event.Action == MotionEventActions.Down)
				.Buffer(TimeOut, NbClick)
				.Where(s => s.Count == 5)
				.ObserveOn(SynchronizationContext.Current)
				.SubscribeAndLogErrors(_ => ViewModel.Logout.ExecuteIfPossible());

			_mediaPlayer = MediaPlayer.Create(this, Resource.Raw.vehicle);

			ViewModel.OrderCompleted += ViewModelOnOrderCompleted; 
			ViewModel.NoMoreTaxiWaiting += ViewModelOnNoMoreTaxiWaiting;

			FindViewById<LinearLayout>(Resource.Id.frameLayout).Touch += (sender, e) => DisposeBlinkScreen();

			FindViewById<RelativeLayout>(Resource.Id.orderListLayout).Touch += (sender, e) => DisposeBlinkScreen();

			ViewModel.OnViewLoaded();
		}

		private void ViewModelOnOrderCompleted(object sender, EventArgs args)
		{
			_mediaPlayer.Start();

			var isRed = false;
			if (_blinkTimer != null)
			{
				_blinkTimer.Dispose();
			}

			_blinkTimer = Observable.Timer(TimeSpan.Zero, BlinkMs)
				.Select(c => new Unit())
				.Subscribe(unit => RunOnUiThread(() =>
					{
						if (isRed)
						{
							this.FindViewById<LinearLayout>(Resource.Id.frameLayout).SetBackgroundDrawable(Resources.GetDrawable(Resource.Drawable.background_empty));
						}
						else
						{
							this.FindViewById<LinearLayout>(Resource.Id.frameLayout).SetBackgroundColor(Android.Graphics.Color.Red);
						}

						isRed = !isRed;
					}));
		}

		private void ViewModelOnNoMoreTaxiWaiting(object sender, EventArgs args)
		{
			DisposeBlinkScreen();
		}

		private void DisposeBlinkScreen()
		{ 
			if (_blinkTimer != null)
			{
				_blinkTimer.Dispose();
			}
			RunOnUiThread( () => this.FindViewById<LinearLayout>(Resource.Id.frameLayout).SetBackgroundDrawable(Resources.GetDrawable(Resource.Drawable.background_empty)));
		}

		protected override void OnDestroy()
		{
			if (_blinkTimer != null)
			{
				_blinkTimer.Dispose();
			}
			base.OnDestroy();
			ViewModel.OrderCompleted -= ViewModelOnOrderCompleted; 
			ViewModel.NoMoreTaxiWaiting -= ViewModelOnNoMoreTaxiWaiting; 
			ViewModel.Unsubscribe();
		}
	}
}