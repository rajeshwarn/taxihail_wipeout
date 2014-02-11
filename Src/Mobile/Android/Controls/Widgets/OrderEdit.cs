using System;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Android.Content;
using Android.Util;
using Android.Views;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Common.Configuration;
using TinyIoC;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using Android.Widget;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class OrderEdit : MvxFrameControl
    {
        private IAppSettings Settings;

        private EditText txtName;
        private EditText txtPhone;
        private EditText txtPassengers;
        private EditText txtLargeBags;
        private EditText txtApartment;
        private EditText txtEntryCode;
        private EditTextSpinner txtVehicleType;
        private EditTextSpinner txtChargeType;

        public OrderEdit(Context context, IAttributeSet attrs) : base(Resource.Layout.SubView_OrderEdit, context, attrs)
        {
            Settings = TinyIoCContainer.Current.Resolve<IAppSettings>();

            this.DelayBind(() => 
                {
                    txtName = Content.FindViewById<EditText>(Resource.Id.txtName);
                    txtPhone = Content.FindViewById<EditText>(Resource.Id.txtPhone);
                    txtPassengers = Content.FindViewById<EditText>(Resource.Id.txtPassengers);
//                    txtLargeBags = Content.FindViewById<EditText>(Resource.Id.txtLargeBags);
                    txtApartment = Content.FindViewById<EditText>(Resource.Id.txtApartment);
                    txtEntryCode = Content.FindViewById<EditText>(Resource.Id.txtEntryCode);
                    txtVehicleType = Content.FindViewById<EditTextSpinner>(Resource.Id.txtVehicleType);
                    txtChargeType = Content.FindViewById<EditTextSpinner>(Resource.Id.txtChargeType);

                    InitializeBinding();
                });
        }

        private void InitializeBinding()
        {
//            if (!Settings.Data.ShowPassengerName)
//            {
//                lblName.Maybe(x => x.RemoveFromSuperview());
//                txtName.Maybe(x => x.RemoveFromSuperview());
//            }
//
//            if (!Settings.Data.ShowPassengerPhone)
//            {
//                lblPhone.Maybe(x => x.RemoveFromSuperview());
//                txtPhone.Maybe(x => x.RemoveFromSuperview());
//            }
//
//            if (!Settings.Data.ShowPassengerNumber)
//            {
//                lblPassengers.Maybe(x => x.RemoveFromSuperview());
//                txtPassengers.Maybe(x => x.RemoveFromSuperview());
//            }

            var set = this.CreateBindingSet<OrderEdit, OrderEditViewModel> ();

            set.BindSafe(txtName)
                .To(vm => vm.BookingSettings.Name);

            set.BindSafe(txtPhone)
                .To(vm => vm.BookingSettings.Phone);

            set.BindSafe(txtPassengers)
                .To(vm => vm.BookingSettings.Passengers);

            set.BindSafe(txtLargeBags)
                .To(vm => vm.BookingSettings.LargeBags);

            set.BindSafe(txtApartment)
                .To(vm => vm.PickupAddress.Apartment);

            set.BindSafe(txtEntryCode)
                .To(vm => vm.PickupAddress.RingCode);

            set.BindSafe(txtVehicleType)
                .For("Text")
                .To(vm => vm.VehicleTypeName);

            set.BindSafe(txtChargeType)
                .For("Text")
                .To(vm => vm.ChargeTypeName);

            set.Apply();
        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();
            var inflater = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);

            var isThriev = Settings.Data.ApplicationName == "Thriev";

            View layout;
            if (isThriev)
            {
//                layout = inflater.Inflate(Resource.Layout.SubView_OrderEdit_Thriev, this, true);
            }
            else
            {
                layout = inflater.Inflate(Resource.Layout.SubView_OrderEdit, this, true);
            }
        }
    }
}

