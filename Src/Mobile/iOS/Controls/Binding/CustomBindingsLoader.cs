using Cirrious.MvvmCross.Binding.Interfaces.Bindings.Target.Construction;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Binding
{
    public static class CustomBindingsLoader
    {
        public static void Load(IMvxTargetBindingFactoryRegistry registry)
        {
            TipSliderBinding.Register(registry);
			PaymentSelectorBinding.Register (registry);
        }
    }
}

