using System;
using Cirrious.MvvmCross.Binding.Interfaces.Bindings.Target.Construction;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Binding
{
    public class CustomBindingsLoader
    {
        public static void Load(IMvxTargetBindingFactoryRegistry registry)
        {
            //registry.RegisterFactory(new MvxSimplePropertyInfoTargetBindingFactory(typeof(TextViewBinding), typeof(TextView), "Text"));
            TipSliderBinding.Register(registry);
			PaymentSelectorBinding.Register (registry);
        }
    }
}

