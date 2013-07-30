using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using Android.App;
using Android.Media;
using Android.OS;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Android.Views;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels.Callbox;
using System.Linq;

namespace apcurium.MK.Callbox.Mobile.Client.Activities
{
    [Activity(Label = "Order List", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class OrderListActivity : MvxBindingActivityView<CallboxOrderListViewModel>
    {
        private const int NbClick = 5;
        private TimeSpan TimeBlinkScreenLongSec = TimeSpan.FromSeconds(30);
        private TimeSpan TimeOut = TimeSpan.FromSeconds(5);
        private TimeSpan BlinkMs = TimeSpan.FromMilliseconds(500);

        private MediaPlayer _mediaPlayer;

        public OrderListActivity()
        {
            
        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_OrderList);
             

            var logoutButton = this.FindViewById<Button>(Resource.Id.LogoutButton);
            Observable.FromEventPattern<View.TouchEventArgs>(logoutButton, "Touch")
                .Where(e => e.EventArgs.Event.Action == MotionEventActions.Down)
                .Buffer(TimeOut, NbClick)
                .Where(s => s.Count == 5)
                .Subscribe(_ => RunOnUiThread(() => ViewModel.Logout.Execute(null)));
             
            _mediaPlayer = MediaPlayer.Create(this, Resource.Raw.vehicle);

            ViewModel.OrderCompleted += ViewModelOnOrderCompleted; 

            FindViewById<LinearLayout>(Resource.Id.frameLayout).Touch += (sender, e) => DisposeBlinkScreen();

            FindViewById<RelativeLayout>(Resource.Id.orderListLayout).Touch += (sender, e) => DisposeBlinkScreen();

            ViewModel.Load();
        }

        private void ViewModelOnOrderCompleted(object sender, EventArgs args)
        {
                _mediaPlayer.Start();
                var isRed = false;
                Observable.Timer(TimeSpan.Zero, BlinkMs).Select(c => new Unit())
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

                Observable.Timer(TimeBlinkScreenLongSec).Select(c => new Unit())
                .Subscribe(unit => DisposeBlinkScreen());
        }


        private void DisposeBlinkScreen()
        {         
            RunOnUiThread( () => this.FindViewById<LinearLayout>(Resource.Id.frameLayout).SetBackgroundDrawable(Resources.GetDrawable(Resource.Drawable.background_empty)));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ViewModel.OrderCompleted -= ViewModelOnOrderCompleted; 
			ViewModel.UnsubscribeToken();
        }
    }
}