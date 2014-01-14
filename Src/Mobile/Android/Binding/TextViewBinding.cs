using System;
using Android.Widget;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Bindings.Target;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Binding.Droid.Target;

namespace apcurium.MK.Booking.Mobile.Client.Binding
{
	public class TextViewBinding : MvxAndroidTargetBinding
    {
		public TextViewBinding(TextView target)
			:base(target)
        {
			target.AfterTextChanged += HandleSelectedChanged;
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
			var control = (TextView)Target;
            FireValueChanged(control.Text);
        }

        public static void Register(IMvxTargetBindingFactoryRegistry registry)
        {
            registry.RegisterFactory(new MvxCustomBindingFactory<TextView>("Text", cell => new TextViewBinding(cell)));
        }

		protected override void SetValueImpl(object target, object value)
        {
			var control = (TextView)target;
            control.Text = (string) value;
        }
    }
}