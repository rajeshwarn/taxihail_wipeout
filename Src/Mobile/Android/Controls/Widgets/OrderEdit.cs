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
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class OrderEdit : MvxFrameControl
    {
        private IAppSettings Settings;

        private TextView _lblName;
        private TextView _lblPhone;
        private TextView _lblPassengers;
        private EditText _txtName;
        private EditText _txtPhone;
        private EditText _txtPassengers;
        private EditText _txtLargeBags;
        private EditText _txtApartment;
        private EditText _txtEntryCode;
        private EditTextSpinner _txtVehicleType;
        private EditTextSpinner _txtChargeType;

        public OrderEdit(Context context, IAttributeSet attrs) : base(Resource.Layout.SubView_OrderEdit, context, attrs)
        {
            Settings = TinyIoCContainer.Current.Resolve<IAppSettings>();

            this.DelayBind(() => 
                {
                    _lblName = Content.FindViewById<TextView>(Resource.Id.lblName);
                    _lblPhone = Content.FindViewById<TextView>(Resource.Id.lblPhone);
                    _lblPassengers = Content.FindViewById<TextView>(Resource.Id.lblPassengers);
                    _txtName = Content.FindViewById<EditText>(Resource.Id.txtName);
                    _txtPhone = Content.FindViewById<EditText>(Resource.Id.txtPhone);
                    _txtPassengers = Content.FindViewById<EditText>(Resource.Id.txtPassengers);
//                    _txtLargeBags = Content.FindViewById<EditText>(Resource.Id.txtLargeBags);
                    _txtApartment = Content.FindViewById<EditText>(Resource.Id.txtApartment);
                    _txtEntryCode = Content.FindViewById<EditText>(Resource.Id.txtEntryCode);
                    _txtVehicleType = Content.FindViewById<EditTextSpinner>(Resource.Id.txtVehicleType);
                    _txtChargeType = Content.FindViewById<EditTextSpinner>(Resource.Id.txtChargeType);

                    InitializeBinding();
                });
        }

        private OrderEditViewModel ViewModel { get { return (OrderEditViewModel)DataContext; } }

        private void InitializeBinding()
        {
            if (!Settings.Data.ShowPassengerName)
            {
                _lblName.Maybe(x => x.Visibility = ViewStates.Gone);
                _txtName.Maybe(x => x.Visibility = ViewStates.Gone);
            }

            if (!Settings.Data.ShowPassengerPhone)
            {
                _lblPhone.Maybe(x => x.Visibility = ViewStates.Gone);
                _txtPhone.Maybe(x => x.Visibility = ViewStates.Gone);
            }

            if (!Settings.Data.ShowPassengerNumber)
            {
                _lblPassengers.Maybe(x => x.Visibility = ViewStates.Gone);
                _txtPassengers.Maybe(x => x.Visibility = ViewStates.Gone);
            }

            var test = ViewModel.BookingSettings.Name;

            var set = this.CreateBindingSet<OrderEdit, OrderEditViewModel> ();

            set.BindSafe(_txtName)
                .For(v => v.Text)
                .To(vm => vm.BookingSettings.Name);

            set.BindSafe(_txtPhone)
                .For(v => v.Text)
                .To(vm => vm.BookingSettings.Phone);

            set.BindSafe(_txtPassengers)
                .For(v => v.Text)
                .To(vm => vm.BookingSettings.Passengers);

//            set.BindSafe(_txtLargeBags)
//                .For(v => v.Text)
//                .To(vm => vm.BookingSettings.LargeBags);

            set.BindSafe(_txtApartment)
                .For(v => v.Text)
                .To(vm => vm.PickupAddress.Apartment);

            set.BindSafe(_txtEntryCode)
                .For(v => v.Text)
                .To(vm => vm.PickupAddress.RingCode);

            set.BindSafe(_txtVehicleType)
                .For("Text")
                .To(vm => vm.VehicleTypeName);
            set.BindSafe(_txtVehicleType)
                .For("Data")
                .To(vm => vm.Vehicles);
            set.BindSafe(_txtVehicleType)
                .For("SelectedItem")
                .To(vm => vm.VehicleTypeId);

            set.BindSafe(_txtChargeType)
                .For("Text")
                .To(vm => vm.ChargeTypeName);
            set.BindSafe(_txtChargeType)
                .For("Data")
                .To(vm => vm.ChargeTypes);
            set.BindSafe(_txtChargeType)
                .For("SelectedItem")
                .To(vm => vm.ChargeTypeId);

            set.Apply();
        }
    }
}

