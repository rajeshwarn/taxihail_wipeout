using System;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Bindings.Target;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Binding.Interfaces;
using Cirrious.MvvmCross.Binding.Interfaces.Bindings.Target.Construction;

namespace apcurium.MK.Booking.Mobile.Client.Binding
{
    public class TextViewBinding : MvxBaseTargetBinding
    {
        private readonly TextView _control;

        public TextViewBinding(TextView control)
        {
            _control = control;
            _control.AfterTextChanged += HandleSelectedChanged;
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }

        public override Type TargetType
        {
            get { return typeof (object); }
        }

        private void HandleSelectedChanged(object sender, EventArgs e)
        {
            FireValueChanged(_control.Text);
        }

        public static void Register(IMvxTargetBindingFactoryRegistry registry)
        {
            registry.RegisterFactory(new MvxCustomBindingFactory<TextView>("Text", cell => new TextViewBinding(cell)));
        }

        public override void SetValue(object value)
        {
            _control.Text = (string) value;
        }
    }
}