using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Controls.Behavior;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Common.Configuration;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;
using TinyIoC;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public class OrderReview : MvxFrameControl
    {    
        private TextView _lblName;
        private TextView _lblDialCode;
        private TextView _lblPhone;
        private TextView _lblNbPassengers;
        private TextView _lblDate;
        private TextView _lblVehicule;
        private TextView _lblChargeType;
        private TextView _lblApt;
        private TextView _lblRingCode;
        private TextView _lblLargeBags;
        private EditTextEntry _editNote;
        private EditTextEntry _editPromoCode;
        private Button _btnPromo;
        private LinearLayout _bottomPadding;
                
        public OrderReview(Context context, IAttributeSet attrs) : base (LayoutHelper.GetLayoutForView(Resource.Layout.SubView_OrderReview, context), context, attrs)
        {
            this.DelayBind (() => 
			{
                _lblName = Content.FindViewById<TextView>(Resource.Id.lblName);
                _lblDialCode = Content.FindViewById<TextView>(Resource.Id.lblDialCode);
                _lblPhone = Content.FindViewById<TextView>(Resource.Id.lblPhone);
                _lblNbPassengers = Content.FindViewById<TextView>(Resource.Id.lblNbPassengers);
                _lblLargeBags = Content.FindViewById<TextView>(Resource.Id.lblLargeBags);
                _lblDate = Content.FindViewById<TextView>(Resource.Id.lblDate);
                _lblVehicule = Content.FindViewById<TextView>(Resource.Id.lblVehicule);
                _lblChargeType = Content.FindViewById<TextView>(Resource.Id.lblChargeType);
                _lblApt = Content.FindViewById<TextView>(Resource.Id.lblApt);
                _lblRingCode = Content.FindViewById<TextView>(Resource.Id.lblRingCode);
                _editNote = FindViewById<EditTextEntry>(Resource.Id.txtNotes);
                _editPromoCode = FindViewById<EditTextEntry>(Resource.Id.txtPromoCode);
                _btnPromo = FindViewById<Button>(Resource.Id.btnPromo);

                _editNote.SetClickAnywhereToDismiss();

                // hack for scroll in view when in EditText
                _bottomPadding = Content.FindViewById<LinearLayout>(Resource.Id.HackBottomPadding);
                TextFieldInHomeSubviewsBehavior.ApplyTo(
                    new List<EditText>() { _editNote, _editPromoCode }, 
                    () => _bottomPadding.Visibility = ViewStates.Visible, 
                    () => _bottomPadding.Visibility = ViewStates.Gone);

					var hintTextColor = Resources.GetColor(Resource.Color.drivernode_hint_color);

					_editNote.SetHintTextColor(hintTextColor);
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

            set.Bind(_lblDialCode)
                .For(v => v.Text)
                .To(vm => vm.Settings.CountryDialCode)
                .WithConversion("DialCodeConverter");

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

            set.Bind(_editPromoCode)
                .For(v => v.Text)
                .To(vm => vm.PromoCode);

            set.Bind(_btnPromo)
                .For("Click")
                .To(vm => vm.NavigateToPromotions);

			if (!this.Services().Settings.ShowPassengerName)
            {
                FindViewById<LinearLayout>(Resource.Id.passengerNameLayout).Visibility = ViewStates.Gone;
            }

			if (!this.Services().Settings.ShowPassengerNumber)
            {
                FindViewById<LinearLayout>(Resource.Id.passengerNumberLayout).Visibility = ViewStates.Gone;
            }

			if (!this.Services().Settings.ShowPassengerPhone)
            {
                FindViewById<LinearLayout>(Resource.Id.passengerPhoneLayout).Visibility = ViewStates.Gone;
            }

            if (!this.Services().Settings.ShowPassengerApartment)
            {
                FindViewById<LinearLayout>(Resource.Id.passengerApartmentLayout).Visibility = ViewStates.Gone;
                FindViewById<LinearLayout>(Resource.Id.ringCodeLayout).Visibility = ViewStates.Gone;
                FindViewById<LinearLayout>(Resource.Id.ApartmentInfosLayout).Visibility = ViewStates.Gone;
            }

            if (!this.Services().Settings.ShowRingCodeField)
            {
                FindViewById<LinearLayout>(Resource.Id.ringCodeLayout).Visibility = ViewStates.Gone;
            }

            if (!this.Services().Settings.ShowPassengerApartment && !this.Services().Settings.ShowRingCodeField)
            {
                FindViewById<LinearLayout>(Resource.Id.ApartmentInfosLayout).Visibility = ViewStates.Gone;
            }

            if (!this.Services().Settings.PromotionEnabled)
            {
                _btnPromo.Visibility = ViewStates.Gone;
            }

            set.Apply();
        }
    }
}

