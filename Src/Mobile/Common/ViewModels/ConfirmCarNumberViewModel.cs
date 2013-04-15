using System;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.Text;
using Cirrious.MvvmCross.Interfaces.ViewModels;

namespace apcurium.MK.Booking.Mobile
{
	public class ConfirmCarNumberViewModel : BaseViewModel
	{
		public ConfirmCarNumberViewModel (string order)
		{
		}

		Order Order {get; set;}

		public IMvxCommand ConfirmTaxiNumber 
		{
			get {
				return GetCommand (() =>
				{ 
					RequestNavigate<BookPaymentViewModel>(new { order = Order.ToJson() }, false, MvxRequestedBy.UserAction);
				});
			}
		}
	}
}

