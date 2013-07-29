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
        private static TimeSpan TimeBlinkScreenLongSec = TimeSpan.FromSeconds(30);
        private static TimeSpan TimeOut = TimeSpan.FromSeconds(5);
        private static TimeSpan BlinkMs = TimeSpan.FromMilliseconds(500);
        private MediaPlayer mp;
        protected readonly CompositeDisposable Subscriptions = new CompositeDisposable();
        private LinearLayout rootLayout;

        public OrderListActivity()
        {
            
        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_OrderList);
             rootLayout = this.FindViewById<LinearLayout>(Resource.Id.frameLayout);
            var orderListLayout = this.FindViewById<RelativeLayout>(Resource.Id.orderListLayout);
            var logoutButton = this.FindViewById<Button>(Resource.Id.LogoutButton);
            Observable.FromEventPattern<View.TouchEventArgs>(logoutButton, "Touch")
                .Where(e => e.EventArgs.Event.Action == MotionEventActions.Down)
                .Buffer(TimeOut, NbClick)
                .Where(s => s.Count == 5)
                .Subscribe(_ => RunOnUiThread(() => ViewModel.Logout.Execute(null)));
             mp = MediaPlayer.Create(this, Resource.Raw.vehicle);
            ViewModel.OrderCompleted += ViewModelOnOrderCompleted; 
            Observable.FromEventPattern<View.TouchEventArgs>(rootLayout, "Touch")
                     .Subscribe(e => DisposeBlinkScreen());
            
            Observable.FromEventPattern<View.TouchEventArgs>(orderListLayout, "Touch")
                     .Subscribe(e => DisposeBlinkScreen());

            ViewModel.Load();
        }

        private void ViewModelOnOrderCompleted(object sender, EventArgs args)
        {
            if (Subscriptions.Count == 0)
                                                {
                                                    
                                                    mp.Start();
                                                    var isRed = false;
                                                    Observable.Timer(TimeSpan.Zero, BlinkMs).Select(c => new Unit())
                                                    .Subscribe(unit => RunOnUiThread(() =>
                                                                                         {

                                                                                             if (isRed)
                                                                                             {
                                                                                                 rootLayout.SetBackgroundDrawable(Resources.GetDrawable(Resource.Drawable.background_empty));
                                                                                             }
                                                                                             else
                                                                                             {
                                                                                                 rootLayout
                                                                                                     .SetBackgroundColor
                                                                                                     (Android.Graphics
                                                                                                             .Color
                                                                                                             .Red);
                                                                                             }
                                                                                             isRed = !isRed;
                                                                                         }))
                                                    .DisposeWith(Subscriptions);

                                                    Observable.Timer(TimeBlinkScreenLongSec).Select(c => new Unit())
                                                    .Subscribe(unit => RunOnUiThread(DisposeBlinkScreen))
                                                    .DisposeWith(Subscriptions);
                                                }
         
        }


        private void DisposeBlinkScreen()
        {
            Subscriptions.DisposeAll();
            RunOnUiThread( () =>rootLayout.SetBackgroundDrawable(Resources.GetDrawable(Resource.Drawable.background_empty)));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ViewModel.OrderCompleted -= ViewModelOnOrderCompleted; 
			ViewModel.UnsubscribeToken();
        }
    }
}