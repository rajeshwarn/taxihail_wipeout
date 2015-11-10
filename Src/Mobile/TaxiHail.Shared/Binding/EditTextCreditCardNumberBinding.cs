using System;
using apcurium.MK.Booking.Mobile.Client.Controls;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Droid.Target;

namespace apcurium.MK.Booking.Mobile.Client.Binding
{
    public class EditTextCreditCardNumberBinding : MvxAndroidTargetBinding
    {
        public EditTextCreditCardNumberBinding(EditTextLeftImage target)
			:base(target)
        {
			target.AfterCreditCardNumberChanged += HandleCreditCardNumberChanged;
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
			var control = Target as EditTextLeftImage;
            FireValueChanged(control.CreditCardNumber);
        }

		protected override void SetValueImpl(object target, object value)
        {
			var control = target as EditTextLeftImage;
            control.CreditCardNumber = (string) value;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
				var control = Target as EditTextLeftImage;
                control.AfterCreditCardNumberChanged -= HandleCreditCardNumberChanged;
            }
            base.Dispose(isDisposing);
        }
    }
}