using System;
using Cirrious.MvvmCross.ViewModels;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client
{
	public interface IHaveViewModel
	{
		IMvxViewModel ViewModel { get; }
	}
}

