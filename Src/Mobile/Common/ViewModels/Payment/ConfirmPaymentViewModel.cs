using System;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Common.Entity;
using ServiceStack.Text;


namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
	public class ConfirmPaymentViewModel : BaseViewModel
	{
		public void Init(string order, string orderStatus)
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

		private PaymentDetailsViewModel _paymentPreferences;
		public PaymentDetailsViewModel PaymentPreferences
		{
			get
			{
				if (_paymentPreferences == null)
				{
					var account = this.Services().Account.CurrentAccount;
					var paymentInformation = new PaymentInformation
					{
						CreditCardId = account.DefaultCreditCard,
						TipPercent = account.DefaultTipPercent,
					};

					_paymentPreferences = new PaymentDetailsViewModel();
					_paymentPreferences.Init(paymentInformation);
				}
				return _paymentPreferences;
			}
		}

		public AsyncCommand ConfirmPayment 
		{
			get {
				return GetCommand (() =>
				{ 
						ShowSubViewModel<PaymentViewModel,object>(
                    new { 
                        order = Order.ToJson(),
                        orderStatus = OrderStatus.ToJson(),
                    }.ToStringDictionary(), 
                    _=>{
                    });
					
					Close(this);
				});
			}
		}	

		public AsyncCommand ChangePayment 
		{
			get {
				return GetCommand (() =>
					{ 
						Close(this);
					});
			}
		}	
	}
}

