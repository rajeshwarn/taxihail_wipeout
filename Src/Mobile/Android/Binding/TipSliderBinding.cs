using System;
using apcurium.MK.Booking.Mobile.Client.Controls;
using Cirrious.MvvmCross.Binding.Bindings.Target;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Binding;

namespace apcurium.MK.Booking.Mobile.Client.Binding
{
    public class TipSliderBinding : MvxTargetBinding
    {
        private readonly TipSlider _control;

        public TipSliderBinding(TipSlider control)
        {
            _control = control;
            _control.PercentChanged += HandleSelectedChanged;
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
            FireValueChanged(_control.Percent);
        }


        public override void SetValue(object value)
        {
            _control.Percent = (int) value;
        }
    }
}