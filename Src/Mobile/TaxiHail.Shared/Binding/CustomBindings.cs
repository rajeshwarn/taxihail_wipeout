using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;

namespace apcurium.MK.Booking.Mobile.Client.Binding
{
    public static class CustomBindingsLoader
    {
		public static void Load(IMvxTargetBindingFactoryRegistry registry)
        {
            TextViewBinding.Register(registry);
            MvxButtonClickBinding.Register(registry);
        }
    }
}