using System;
using Cirrious.MvvmCross.Binding.Interfaces.Bindings.Target.Construction;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Android.Widget;

namespace apcurium.MK.Booking.Mobile
{
	public class CustomBindingsLoader
	{
		public static void Load(IMvxTargetBindingFactoryRegistry registry)
		{
			//registry.RegisterFactory(new MvxSimplePropertyInfoTargetBindingFactory(typeof(TextViewBinding), typeof(TextView), "Text"));
			registry.RegisterFactory(new MvxCustomBindingFactory<TextView>("Text", cell => new TextViewBinding(cell)));
		}
	}
}

