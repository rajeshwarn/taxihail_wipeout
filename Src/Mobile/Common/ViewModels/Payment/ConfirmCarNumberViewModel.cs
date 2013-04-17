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
		public ConfirmCarNumberViewModel (string order, string orderStatus)
		{
			Order = order.FromJson<Order>();
			OrderStatus = orderStatus.FromJson<OrderStatusDetail>();

#if DEBUG
			if(string.IsNullOrWhiteSpace(OrderStatus.VehicleNumber))
			{
				OrderStatus.VehicleNumber = "Test1234";
			}
#endif

		}

		Order Order {get; set;}
		OrderStatusDetail OrderStatus {get; set;}

		public string CarNumber{
			get{
				return OrderStatus.VehicleNumber;
			}
			set{
				OrderStatus.VehicleNumber = value;
			}
		}


		public IMvxCommand ConfirmTaxiNumber 
		{
			get {
				return GetCommand (() =>
				{ 
					RequestNavigate<PaymentViewModel>(
							new 
							{ 
								order = Order.ToJson(),
								orderStatus = OrderStatus.ToJson(),
							}, 
							false, MvxRequestedBy.UserAction);
#if IOS

#else
					RequestClose(this);
#endif
				});
			}
		}
	}
}

