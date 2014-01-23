using System;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Common.Entity;
using ServiceStack.Text;
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
	public class ConfirmCarNumberViewModel : BaseViewModel
	{
		public void Init(string order, string orderStatus)
		{
			Order = order.FromJson<Order>();
			OrderStatus = orderStatus.FromJson<OrderStatusDetail>();
		}

		Order Order { get; set; }
		OrderStatusDetail OrderStatus { get; set; }

		public string CarNumber
		{
			get{
				return OrderStatus.VehicleNumber;
			}
		}

		public ICommand ConfirmCarNumber 
		{
			get {
				return GetCommand (() =>
				{ 
					ShowSubViewModel<PaymentViewModel,object>(
	                    new { 
	                        order = Order.ToJson(),
	                        orderStatus = OrderStatus.ToJson(),
	                    }.ToStringDictionary(), 
                    	_=>{});
					
					Close(this);
				});
			}
		}
	}
}

