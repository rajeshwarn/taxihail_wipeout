using System;
using System.Drawing;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Attributes;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;
using TinyIoC;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Booking.Mobile.Client.Controls.Behavior;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class OrderReview: MvxFrameControl
    {    
        private IAppSettings _settings;

        private TextView _lblName;
        private TextView _lblPhone;
        private TextView _lblNbPassengers;
        private TextView _lblDate;
        private TextView _lblVehicule;
        private TextView _lblChargeType;
        private TextView _lblApt;
        private TextView _lblRingCode;
        private TextView _lblLargeBags;
        private EditTextEntry _editNote;
        private LinearLayout _bottomPadding;
        private LinearLayout _container;

        public override bool OnTouchEvent(MotionEvent e)
        {
            return base.OnTouchEvent(e);
        }

        public OrderReview(Context context, IAttributeSet attrs) : base (LayoutHelper.GetLayoutForView(Resource.Layout.SubView_OrderReview, context), context, attrs)
        {
            _settings = TinyIoCContainer.Current.Resolve<IAppSettings>();

            this.DelayBind (() => {

                _container = Content.FindViewById<LinearLayout>(Resource.Id.orderReviewContainer);
                _lblName = Content.FindViewById<TextView>(Resource.Id.lblName);
                _lblPhone = Content.FindViewById<TextView>(Resource.Id.lblPhone);
                _lblNbPassengers = Content.FindViewById<TextView>(Resource.Id.lblNbPassengers);
                _lblLargeBags = Content.FindViewById<TextView>(Resource.Id.lblLargeBags);
                _lblDate = Content.FindViewById<TextView>(Resource.Id.lblDate);
                _lblVehicule = Content.FindViewById<TextView>(Resource.Id.lblVehicule);
                _lblChargeType = Content.FindViewById<TextView>(Resource.Id.lblChargeType);
                _lblApt = Content.FindViewById<TextView>(Resource.Id.lblApt);
                _lblRingCode = Content.FindViewById<TextView>(Resource.Id.lblRingCode);
                _editNote = FindViewById<EditTextEntry>(Resource.Id.txtNotes);

                _editNote.SetClickAnywhereToDismiss(_container);

                // hack for scroll in view when in EditText
                _bottomPadding = Content.FindViewById<LinearLayout>(Resource.Id.HackBottomPadding);
                TextFieldInHomeSubviewsBehavior.ApplyTo(
                    _editNote, 
                    () => _bottomPadding.Visibility = ViewStates.Visible, 
                    () => _bottomPadding.Visibility = ViewStates.Gone);

                InitializeBinding();
            });              
        }

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

            set.BindSafe(_lblNbPassengers)
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

            set.BindSafe(_lblApt)
                .For(v => v.Text)
                .To(vm => vm.Apartment);

            set.BindSafe(_lblRingCode)
                .For(v => v.Text)
                .To(vm => vm.RingCode);

            set.BindSafe(_lblLargeBags)
                .For(v => v.Text)
                .To(vm => vm.Settings.LargeBags);

            set.Bind(_editNote)
                .For(v => v.Text)
                .To(vm => vm.Note);

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

