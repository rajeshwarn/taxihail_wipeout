using System;
using System.Linq;
using Cirrious.MvvmCross.Binding.BindingContext;
using UIKit;
using TinyIoC;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Mobile.Client.Views.Order
{
    public partial class OrderEditView : BaseBindableChildView<OrderEditViewModel>
    {
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

            txtPhone.BackgroundColor = UIColor.FromRGB(242, 242, 242);
            lblPhoneDialCode.BackgroundColor = UIColor.FromRGB(242, 242, 242);
            lblPhoneDialCode.Font = UIFont.FromName(FontName.HelveticaNeueLight, 38/2);
            lblPhoneDialCode.TintColor = UIColor.Black;
            lblPhoneDialCode.TextColor = UIColor.FromRGB(44, 44, 44);
            lblPhoneDialCode.TextAlignment = UITextAlignment.Center;
            lblPhoneDialCode.AdjustsFontSizeToFitWidth = true;

            txtPhone.Maybe(x => x.ShowCloseButtonOnKeyboard());
            txtPassengers.Maybe(x => x.ShowCloseButtonOnKeyboard());

            lblName.Maybe(x => x.Text = Localize.GetValue("PassengerNameLabel"));
            lblPhone.Maybe(x => x.Text = Localize.GetValue("PassengerPhoneLabel"));
            lblPassengers.Maybe(x => x.Text = Localize.GetValue("PassengerNumberLabel"));
            lblApartment.Maybe(x => x.Text = Localize.GetValue("ApartmentLabel"));
            lblEntryCode.Maybe(x => x.Text = Localize.GetValue("EntryCodeLabel"));
            lblChargeType.Maybe(x => x.Text = Localize.GetValue("ChargeTypeLabel"));                     
            lblLargeBags.Maybe(x => x.Text = Localize.GetValue ("LargeBagsLabel"));
           
            txtChargeType.Configure(Localize.GetValue("RideSettingsChargeType"), () => ViewModel.ChargeTypes.ToArray(), () => ViewModel.ChargeTypeId, x => ViewModel.ChargeTypeId = x.Id);
        }

        private void InitializeBinding()
        {
			if (!this.Services().Settings.ShowPassengerName)
            {
                lblName.Maybe(x => x.RemoveFromSuperview());
                txtName.Maybe(x => x.RemoveFromSuperview());
            }

			if (!this.Services().Settings.ShowPassengerPhone)
            {
                lblPhone.Maybe(x => x.RemoveFromSuperview());
                phoneNumberView.Maybe(x => x.RemoveFromSuperview());
            }

			if (!this.Services().Settings.ShowPassengerNumber)
            {
                lblPassengers.Maybe(x => x.RemoveFromSuperview());
                txtPassengers.Maybe(x => x.RemoveFromSuperview());
            }

            if (!this.Services().Settings.ShowPassengerApartment)
            {
                lblApartment.Maybe(x => x.RemoveFromSuperview());
                txtApartment.Maybe(x => x.RemoveFromSuperview());
                lblEntryCode.Maybe(x => x.RemoveFromSuperview());
                txtEntryCode.Maybe(x => x.RemoveFromSuperview());
            }

            if (!this.Services().Settings.ShowRingCodeField)
            {
                lblEntryCode.Maybe(x => x.RemoveFromSuperview());
                txtEntryCode.Maybe(x => x.RemoveFromSuperview());
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

            set.BindSafe(txtChargeType)
                .To(vm => vm.ChargeTypeName);

			set.BindSafe(txtChargeType)
				.For(v => v.Enabled)
				.To(vm => vm.IsChargeTypesEnabled);

            set.Apply();


            lblPhoneDialCode.Configure(this.FindViewController().NavigationController, (DataContext as OrderEditViewModel).PhoneNumber);
            lblPhoneDialCode.NotifyChanges += (object sender, PhoneNumberChangedEventArgs e) =>
                {
                    this.ViewModel.SelectedCountryCode = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryDialCode(e.CountryDialCode));
                };
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

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

