using System;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.Drawing;
using Cirrious.MvvmCross.Binding.Attributes;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Common.Configuration;
using TinyIoC;
using apcurium.MK.Booking.Mobile.PresentationHints;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class OrderReview: MvxFrameControl
    {    
        private IAppSettings _settings;

        public OrderReview(Context context, IAttributeSet attrs) : base (Resource.Layout.SubView_OrderReview, context, attrs)
        {
            _settings = TinyIoCContainer.Current.Resolve<IAppSettings>();

            this.DelayBind (() => {

                _lblName = (TextView)FindViewById<TextView>(Resource.Id.lblName);
                _lblPhone = (TextView)FindViewById<TextView>(Resource.Id.lblPhone);
                _lblNbPassengers = (TextView)FindViewById<TextView>(Resource.Id.lblNbPassengers);
                _lblDate = (TextView)FindViewById<TextView>(Resource.Id.lblDate);
                _lblVehicule = (TextView)FindViewById<TextView>(Resource.Id.lblVehicule);
                _lblChargeType = (TextView)FindViewById<TextView>(Resource.Id.lblChargeType);
                _lblApt = (TextView)FindViewById<TextView>(Resource.Id.lblApt);
                _lblRingCode = (TextView)FindViewById<TextView>(Resource.Id.lblRingCode);

                InitializeBinding();
            });              
        }

        private TextView _lblName;
        private TextView _lblPhone;
        private TextView _lblNbPassengers;
        private TextView _lblDate;
        private TextView _lblVehicule;
        private TextView _lblChargeType;
        private TextView _lblApt;
        private TextView _lblRingCode;

        private OrderReviewViewModel ViewModel { get { return (OrderReviewViewModel)DataContext; } }

        private void InitializeBinding()
        {
            var set = this.CreateBindingSet<OrderReview, OrderReviewViewModel>();

            set.Bind(_lblName)
                .For(v => v.Text)
                .To(vm => vm.Settings.Name);

            set.Bind(_lblPhone)
                .For(v => v.Text)
                .To(vm => vm.Settings.Phone);

            set.Bind(_lblNbPassengers)
                .For(v => v.Text)
                .To(vm => vm.Settings.Passengers);

            set.Bind(_lblDate)
                .For(v => v.Text)
                .To(vm => vm.Date);

            set.Bind(_lblVehicule)
                .For(v => v.Text)
                .To(vm => vm.VehiculeType);

            set.Bind(_lblChargeType)
                .For(v => v.Text)
                .To(vm => vm.ChargeType);

            set.Bind(_lblApt)
                .For(v => v.Text)
                .To(vm => vm.Apartment);

            set.Bind(_lblRingCode)
                .For(v => v.Text)
                .To(vm => vm.RingCode);

            if (!_settings.Data.ShowPassengerName)
            {
                FindViewById<LinearLayout>(Resource.Id.passengerNameLayout).Visibility = ViewStates.Gone;
            }

            if (!_settings.Data.ShowPassengerNumber)
            {
                FindViewById<LinearLayout>(Resource.Id.passengerNumberLayout).Visibility = ViewStates.Gone;
            }

            if (!_settings.Data.ShowPassengerPhone)
            {
                FindViewById<LinearLayout>(Resource.Id.passengerPhoneLayout).Visibility = ViewStates.Gone;
            }

            set.Apply();
        }

    }
}

