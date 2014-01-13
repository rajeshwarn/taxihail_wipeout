using System;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Droid.Target;
using apcurium.MK.Booking.Mobile.Client.Controls;

namespace apcurium.MK.Booking.Mobile.Client.Binding
{
    public class EditTextCreditCardNumberBinding : MvxAndroidTargetBinding
    {
        private readonly EditTextLeftImage _control;

        public EditTextCreditCardNumberBinding(EditTextLeftImage control)
        {
            _control = control;

            _control.AfterCreditCardNumberChanged += HandleCreditCardNumberChanged;
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.TwoWay; }
        }

        public override Type TargetType
        {
            get { return typeof (string); }
        }

        private void HandleCreditCardNumberChanged(object sender, EventArgs e)
        {
            FireValueChanged(_control.CreditCardNumber);
        }

        public override void SetValue(object value)
        {
            _control.CreditCardNumber = (string) value;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _control.AfterCreditCardNumberChanged -= HandleCreditCardNumberChanged;
            }
            base.Dispose(isDisposing);
        }
    }
}