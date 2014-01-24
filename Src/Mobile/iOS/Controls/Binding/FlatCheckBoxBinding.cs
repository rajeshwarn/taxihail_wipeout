using System;
using Cirrious.MvvmCross.Binding.Bindings.Target;
using MonoTouch.UIKit;
using System.Reflection;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Binding
{
    public class FlatCheckBoxBinding : MvxTargetBinding
    {
        public static void Register(IMvxTargetBindingFactoryRegistry registry)
        {           
            registry.RegisterFactory(new MvxCustomBindingFactory<FlatCheckBox>("Selected", obj => new FlatCheckBoxBinding(obj)));
        }

        public FlatCheckBoxBinding(FlatCheckBox target) : base(target)
        {
            target.CheckValueChanged += HandleSelectedChanged;
        }

        void HandleSelectedChanged (object sender, EventArgs e)
        {
            var control = Target as FlatCheckBox;
            FireValueChanged(control.Selected);
        }

        public override void SetValue (object value)
        {
            var control = Target as FlatCheckBox;
            control.Selected = (bool)value;
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

