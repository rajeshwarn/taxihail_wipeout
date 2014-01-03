using System;
using Cirrious.MvvmCross.Binding.Bindings.Target;
using Cirrious.MvvmCross.Binding.Interfaces.Bindings.Target.Construction;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Binding.Interfaces;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Binding
{
	public class PaymentSelectorBinding: MvxBaseTargetBinding
	{

		public static void Register(IMvxTargetBindingFactoryRegistry registry)
		{           
			registry.RegisterFactory(new MvxCustomBindingFactory<PaymentSelector>("PayPalSelected", obj => new PaymentSelectorBinding(obj)));
		}

		private readonly PaymentSelector _control;

		public PaymentSelectorBinding(PaymentSelector control)
		{
			_control = control;         
			_control.ValueChanged += HandleSelectedChanged;
		}

		void HandleSelectedChanged (object sender, EventArgs e)
		{
			FireValueChanged(_control.PayPalSelected);
		}

		public override void SetValue (object value)
		{
			_control.PayPalSelected = (bool)value;
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

