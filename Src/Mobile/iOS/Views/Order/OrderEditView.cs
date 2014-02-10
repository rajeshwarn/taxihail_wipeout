using System;
using System.Drawing;
using Cirrious.MvvmCross.Binding.BindingContext;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TinyIoC;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using System.Linq;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions;

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

            foreach (var textField in ContentView.Subviews.Where(x => x is FlatTextField))
            {
                textField.BackgroundColor = UIColor.FromRGB(242, 242, 242);
            }

            DismissKeyboardOnReturn(txtApartment, txtEntryCode);

            lblName.Maybe(x => x.Text = Localize.GetValue("PassengerNameLabel"));
            lblPhone.Maybe(x => x.Text = Localize.GetValue("PassengerPhoneLabel"));
            lblPassengers.Maybe(x => x.Text = Localize.GetValue("PassengerNumberLabel"));
            lblApartment.Maybe(x => x.Text = Localize.GetValue("ApartmentLabel"));
            lblEntryCode.Maybe(x => x.Text = Localize.GetValue("EntryCodeLabel"));
            lblVehicleType.Maybe(x => x.Text = Localize.GetValue("ConfirmVehiculeTypeLabel"));
            lblChargeType.Maybe(x => x.Text = Localize.GetValue("ChargeTypeLabel"));                     

//            lblLargeBags.Maybe(x => x.Text = Localize.GetValue ("LargeBagsLabel"));

            // TODO values should only be reflected in the workflow service when the user saves
            // and should not affect account settings
            txtVehicleType.Configure(Localize.GetValue("RideSettingsVehiculeType"), () => ViewModel.RideSettings.Vehicles, () => ViewModel.Settings.VehicleTypeId, x => {});//ViewModel.SetVehiculeType.Execute(x.Id));
            txtChargeType.Configure(Localize.GetValue("RideSettingsChargeType"), () => ViewModel.RideSettings.Payments, () => ViewModel.Settings.ChargeTypeId, x => {});//ViewModel.SetChargeType.Execute(x.Id));
        }

        private void InitializeBinding()
        {
            if (!Settings.Data.ShowPassengerName)
            {
                lblName.Maybe(x => x.RemoveFromSuperview());
                lblName.Maybe(x => x.RemoveFromSuperview());
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
                .To(vm => vm.Settings.Name);

            set.BindSafe(txtPhone)
                .To(vm => vm.Settings.Phone);

            set.BindSafe(txtPassengers)
                .To(vm => vm.Settings.Passengers);

//            set.BindSafe(txtLargeBags)
//                .To(vm => vm.Settings.LargeBags);

//            set.BindSafe(txtApartment)
//                .To(vm => vm.PickupAddress.Apartment);
//
//            set.BindSafe(txtEntryCode)
//                .To(vm => vm.PickupAddress.RingCode);

            set.BindSafe(txtVehicleType)
                .For("Text")
                .To(vm => vm.Settings.VehicleType);

            set.BindSafe(txtChargeType)
                .For("Text")
                .To(vm => vm.Settings.ChargeType);

            set.Apply();
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            Settings = TinyIoCContainer.Current.Resolve<IAppSettings>();
            var isThriev = Settings.Data.ApplicationName == "Thriev";

            UINib nib;
            if (isThriev)
            {
                nib = UINib.FromName ("OrderEditView_Thriev", null);
            } 
            else 
            {
                nib = UINib.FromName ("OrderEditView", null);
            }

            var view = (UIView)nib.Instantiate(this, null)[0];
            AddSubview(view);

            Initialize();

            this.DelayBind (() => {
                InitializeBinding();
            });
        }
    }
}

