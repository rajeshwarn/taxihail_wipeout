using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Controls;
using Cirrious.MvvmCross.Binding.Interfaces.Bindings.Target.Construction;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Binding.Interfaces;
using Cirrious.MvvmCross.Binding.Android.Target;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class EditTextCreditCardNumberBinding : MvxBaseAndroidTargetBinding
    {
		private EditTextLeftImage _control;

		public EditTextCreditCardNumberBinding(EditTextLeftImage control)
		{
			_control = control;
					
			_control.AfterCreditCardNumberChanged += HandleCreditCardNumberChanged;
		}

		private void HandleCreditCardNumberChanged(object sender, EventArgs e)
		{
			FireValueChanged(_control.CreditCardNumber);
		}
	
		public override void SetValue (object value)
		{
			_control.CreditCardNumber = (string)value;
		}

		public override MvxBindingMode DefaultMode
		{
			get { return MvxBindingMode.TwoWay; }
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				_control.AfterCreditCardNumberChanged -= HandleCreditCardNumberChanged;
			}
			base.Dispose(isDisposing);
		}


		public override Type TargetType
		{
			get { return typeof(string); }
		}
    }
}

