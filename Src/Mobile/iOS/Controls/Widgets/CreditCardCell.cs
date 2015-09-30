
using System;

using Foundation;
using UIKit;
using Cirrious.MvvmCross.Binding.Touch.Views;
using apcurium.MK.Booking.Mobile.Client.Localization;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Data;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public partial class CreditCardCell : MvxTableViewCell
    {
        public static readonly UINib Nib = UINib.FromName("CreditCardCell", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("CreditCardCell");

        public CreditCardCell(IntPtr handle)
            : base(handle)
        {
        }

        public static CreditCardCell Create()
        {
            return (CreditCardCell)Nib.Instantiate(null, null)[0];
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            txtCardNumber.AccessibilityLabel = Localize.GetValue("CreditCardNumber");
            txtCardNumber.Placeholder = txtCardNumber.AccessibilityLabel;
            txtCardNumber.ForceWhiteBackground = true;
            txtCardNumber.HasRightArrow = true;

            var set = this.CreateBindingSet<CreditCardCell, CreditCardInfos>();

            set.Bind(txtCardNumber)
                .To(vm => vm.CardNumber);

            set.Bind(txtCardNumber)
                .For(v => v.ImageLeftSource)
                .To(vm => vm.CreditCardCompany)
                .WithConversion("CreditCardCompanyImageConverter");

            set.Apply();
        }

    }
}

