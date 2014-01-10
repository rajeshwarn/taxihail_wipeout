using System;
using Cirrious.MvvmCross.Binding.Bindings.Target;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Binding;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Binding
{
	public class PaymentSelectorBinding: MvxTargetBinding
	{
		public static void Register(IMvxTargetBindingFactoryRegistry registry)
		{           
			registry.RegisterFactory(new MvxCustomBindingFactory<PaymentSelector>("PayPalSelected", obj => new PaymentSelectorBinding(obj)));
		}

		public PaymentSelectorBinding(PaymentSelector target)
			:base(target)
		{
			target.ValueChanged += HandleSelectedChanged;
		}

		void HandleSelectedChanged (object sender, EventArgs e)
		{
			var control = Target as PaymentSelector;
			FireValueChanged(control.PayPalSelected);
		}

		public override void SetValue (object value)
		{
			var control = Target as PaymentSelector;
			control.PayPalSelected = (bool)value;
		}

		public override MvxBindingMode DefaultMode
		{
			get { return MvxBindingMode.TwoWay; }
		}

		public override Type TargetType
		{
			get { return typeof(object); }
		}
	}
}

