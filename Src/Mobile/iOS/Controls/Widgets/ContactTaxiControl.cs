﻿using System;
using Foundation;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Style;
using System.Linq;
using apcurium.MK.Booking.Mobile.Client.Localization;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public partial class ContactTaxiControl : BaseBindableChildView<BookingStatusViewModel>
    {
        public ContactTaxiControl (IntPtr handle) : base(handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            var nib = UINib.FromName ("ContactTaxiControl", null);

            AddSubview((UIView)nib.Instantiate (this, null)[0]);

            Initialize();

            this.DelayBind (() =>
            {
                InitializeBinding();
            });
        }

        private void Initialize()
        {
            view.BackgroundColor = Theme.CompanyColor;
            lblMedallionTitle.TextColor = Theme.LabelTextColor;
            lblMedallionTitle.Text = Localize.GetValue("OrderStatus_Medallion");
            lblMedallion.TextColor = Theme.LabelTextColor;

            btnCallDriver.SetImage(UIImage.FromFile("phone.png"), UIControlState.Normal);
            btnMessageDriver.SetImage(UIImage.FromFile("message.png"), UIControlState.Normal);

            FlatButtonStyle.Clear.ApplyTo(btnCallDriver);
            FlatButtonStyle.Clear.ApplyTo(btnMessageDriver);
        }

        private void InitializeBinding()
        {
            var set = this.CreateBindingSet<ContactTaxiControl, BookingStatusViewModel>();

            set.Bind(lblMedallion)
                .For(v => v.Text)
                .To(vm => vm.OrderStatusDetail.VehicleNumber);

            set.Bind(btnCallDriver)
                .For("TouchUpInside")
                .To(vm => vm.CallTaxiCommand);

            set.Bind(btnMessageDriver)
                .For("TouchUpInside")
                .To(vm => vm.SendMessageToDriverCommand);

            set.Bind(btnCallDriver)
                .For("HiddenEx")
                .To(vm => vm.IsCallTaxiVisible)
                .WithConversion("BoolInverter");

            set.Bind(btnMessageDriver)
                .For("HiddenEx")
                .To(vm => vm.IsMessageTaxiVisible)
                .WithConversion("BoolInverter");

            set.Apply();
        }
    } 
}