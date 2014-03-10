using System;
using System.Drawing;
using System.Linq;
using Cirrious.MvvmCross.Binding.BindingContext;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TinyIoC;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Views.Order
{
    public partial class OrderEditView : BaseBindableChildView<OrderEditViewModel>
    {
        private IAppSettings Settings;

        public OrderEditView (IntPtr handle) : base(handle)
        {
        }

        private UIView ContentView 
        {
            get { return Subviews[0].Subviews[0].Subviews[0]; }
        }

        private void Initialize()
        {
            BackgroundColor = UIColor.Clear;

            foreach (FlatTextField textField in ContentView.Subviews.Where(x => x is FlatTextField))
            {
                textField.BackgroundColor = UIColor.FromRGB(242, 242, 242);
                DismissKeyboardOnReturn(textField);
            }

            txtPhone.Maybe(x => x.ShowCloseButtonOnKeyboard());
            txtPassengers.Maybe(x => x.ShowCloseButtonOnKeyboard());

            lblName.Maybe(x => x.Text = Localize.GetValue("PassengerNameLabel"));
            lblPhone.Maybe(x => x.Text = Localize.GetValue("PassengerPhoneLabel"));
            lblPassengers.Maybe(x => x.Text = Localize.GetValue("PassengerNumberLabel"));
            lblApartment.Maybe(x => x.Text = Localize.GetValue("ApartmentLabel"));
            lblEntryCode.Maybe(x => x.Text = Localize.GetValue("EntryCodeLabel"));
            lblVehicleType.Maybe(x => x.Text = Localize.GetValue("ConfirmVehiculeTypeLabel"));
            lblChargeType.Maybe(x => x.Text = Localize.GetValue("ChargeTypeLabel"));                     

            lblLargeBags.Maybe(x => x.Text = Localize.GetValue ("LargeBagsLabel"));

            txtVehicleType.Configure(Localize.GetValue("RideSettingsVehiculeType"), () => ViewModel.Vehicles.ToArray(), () => ViewModel.VehicleTypeId, x => ViewModel.VehicleTypeId = x.Id);
            txtChargeType.Configure(Localize.GetValue("RideSettingsChargeType"), () => ViewModel.ChargeTypes.ToArray(), () => ViewModel.ChargeTypeId, x => ViewModel.ChargeTypeId = x.Id);
        }

        private void InitializeBinding()
        {
            if (!Settings.Data.ShowPassengerName)
            {
                lblName.Maybe(x => x.RemoveFromSuperview());
                txtName.Maybe(x => x.RemoveFromSuperview());
            }

            if (!Settings.Data.ShowPassengerPhone)
            {
                lblPhone.Maybe(x => x.RemoveFromSuperview());
                txtPhone.Maybe(x => x.RemoveFromSuperview());
            }

            if (!Settings.Data.ShowPassengerNumber)
            {
                lblPassengers.Maybe(x => x.RemoveFromSuperview());
                txtPassengers.Maybe(x => x.RemoveFromSuperview());
            }

            var set = this.CreateBindingSet<OrderEditView, OrderEditViewModel> ();

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

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            Settings = TinyIoCContainer.Current.Resolve<IAppSettings>();

            var nib = NibHelper.GetNibForView("OrderEditView");
            var view = (UIView)nib.Instantiate(this, null)[0];
            AddSubview(view);

            Initialize();

            this.DelayBind (() => {
                InitializeBinding();
            });
        }
    }
}

