using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.Text;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile
{
    public class ConfirmCarNumberViewModel : BaseViewModel
	{
		public ConfirmCarNumberViewModel (string order, string orderStatus)
		{
			Order = order.FromJson<Order>();
			OrderStatus = orderStatus.FromJson<OrderStatusDetail>();
		}

		Order Order { get; set; }
		OrderStatusDetail OrderStatus { get; set; }

		public string CarNumber{
			get{
				return OrderStatus.VehicleNumber;
			}
		}
             
		public IMvxCommand ConfirmTaxiNumber 
		{
			get {
				return GetCommand (() =>
				{ 
                    RequestSubNavigate<PaymentViewModel,object>(
                    new { 
                        order = Order.ToJson(),
                        orderStatus = OrderStatus.ToJson(),
                    }.ToStringDictionary(), 
                    _=>{
                    });
					
					RequestClose(this);
				});
			}
		}
	}
}

