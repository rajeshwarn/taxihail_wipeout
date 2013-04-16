using System;
using Cirrious.MvvmCross.Binding.Bindings.Target;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Interfaces;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client;
using Cirrious.MvvmCross.Binding.Interfaces.Bindings.Target.Construction;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;

namespace apcurium.MK.Booking.Mobile
{

	public class TipSliderBinding: MvxBaseTargetBinding
	{

		public static void Register(IMvxTargetBindingFactoryRegistry registry)
		{			
			registry.RegisterFactory(new MvxCustomBindingFactory<TipSlider>("Percent", obj => new TipSliderBinding(obj)));
		}




		private TipSlider _control;

		public TipSliderBinding(TipSlider control)
		{
			_control = control;			
			_control.PercentChanged += HandleSelectedChanged;
		}
		
		void HandleSelectedChanged (object sender, EventArgs e)
		{
			FireValueChanged(_control.Percent);
		}
		
		
		public override void SetValue (object value)
		{
			_control.Percent = (int)value;
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

