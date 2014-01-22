using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Binding
{
    public static class CustomBindingsLoader
    {
        public static void Load(IMvxTargetBindingFactoryRegistry registry)
        {
			PaymentSelectorBinding.Register (registry);
        }
    }
}

