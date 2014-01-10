using System;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Bindings.Target;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using apcurium.MK.Booking.Mobile.Client.Views.Payments;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Binding
{
    public class TipSliderBinding: MvxTargetBinding
    {
        public static void Register(IMvxTargetBindingFactoryRegistry registry)
        {           
            registry.RegisterFactory(new MvxCustomBindingFactory<TipSliderControl>("Value", obj => new TipSliderBinding(obj)));
        }
        
		public TipSliderBinding(TipSliderControl target)
        {
			target.ValueChanged += HandleSelectedChanged;
        }
        
        void HandleSelectedChanged (object sender, EventArgs e)
        {
			var control = Target as TipSliderControl;
            FireValueChanged(control.Value);
        }
        
        
        public override void SetValue (object value)
        {
			var control = Target as TipSliderControl;
            control.Value = (int)value;
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

