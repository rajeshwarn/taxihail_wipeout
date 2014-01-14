using System;
using apcurium.MK.Booking.Mobile.Client.Controls;
using Cirrious.MvvmCross.Binding.Bindings.Target;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Binding;

namespace apcurium.MK.Booking.Mobile.Client.Binding
{
    public class TipSliderBinding : MvxTargetBinding
    {
		public TipSliderBinding(TipSlider target)
			:base(target)
        {
			target.PercentChanged += HandleSelectedChanged;
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }

        public override Type TargetType
        {
            get { return typeof (object); }
        }

        public static void Register(IMvxTargetBindingFactoryRegistry registry)
        {
            registry.RegisterFactory(new MvxCustomBindingFactory<TipSlider>("Percent", obj => new TipSliderBinding(obj)));
        }

        private void HandleSelectedChanged(object sender, EventArgs e)
        {
			var control = Target  as TipSlider;
            FireValueChanged(control.Percent);
        }


        public override void SetValue(object value)
        {
			var control = Target  as TipSlider;
            control.Percent = (int) value;
        }
    }
}