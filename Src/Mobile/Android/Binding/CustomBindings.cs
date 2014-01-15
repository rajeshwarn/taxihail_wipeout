using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;


namespace apcurium.MK.Booking.Mobile.Client.Binding
{
    public static class CustomBindingsLoader
    {
		public static void Load(IMvxTargetBindingFactoryRegistry registry)
        {
            TipSliderBinding.Register(registry);
            TextViewBinding.Register(registry);
        }
    }
}