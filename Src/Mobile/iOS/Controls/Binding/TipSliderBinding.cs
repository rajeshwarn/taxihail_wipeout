using System;
using Cirrious.MvvmCross.Binding.Interfaces.Bindings.Target.Construction;
using Cirrious.MvvmCross.Binding.Bindings.Target;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using apcurium.MK.Booking.Mobile.Client.Views.Payments;
using Cirrious.MvvmCross.Binding.Interfaces;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Binding
{
    public class TipSliderBinding: MvxBaseTargetBinding
    {
        
        public static void Register(IMvxTargetBindingFactoryRegistry registry)
        {           
            registry.RegisterFactory(new MvxCustomBindingFactory<TipSliderControl>("Value", obj => new TipSliderBinding(obj)));
        }
        
        
        
        
        private readonly TipSliderControl _control;
        
        public TipSliderBinding(TipSliderControl control)
        {
            _control = control;         
            _control.ValueChanged += HandleSelectedChanged;
        }
        
        void HandleSelectedChanged (object sender, EventArgs e)
        {
            FireValueChanged(_control.Value);
        }
        
        
        public override void SetValue (object value)
        {
            _control.Value = (int)value;
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

