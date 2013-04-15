using System;
using Cirrious.MvvmCross.Binding.Bindings.Target;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Interfaces;

namespace apcurium.MK.Booking.Mobile
{

	public class TextViewBinding: MvxBaseTargetBinding
	{
		private TextView _control;
		
		public TextViewBinding(TextView control)
		{
			_control = control;			
			_control.AfterTextChanged += HandleSelectedChanged;
		}
		
		void HandleSelectedChanged (object sender, EventArgs e)
		{
			FireValueChanged(_control.Text);
		}
		
		
		public override void SetValue (object value)
		{
			_control.Text = (string)value;
		}
		
		public override MvxBindingMode DefaultMode
		{
			get { return MvxBindingMode.OneWay; }
		}
		
		public override Type TargetType
		{
			get { return typeof(object); }
		}
	}

}

