using System;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Common.Entity;
using ServiceStack.Text;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
	public class ConfirmCarNumberViewModel : PageViewModel
	{
		private readonly IAccountService _accountService;

		public ConfirmCarNumberViewModel(IAccountService accountService)
		{
			_accountService = accountService;
		}

		public void Init(string order, string orderStatus)
		{
			Order = order.FromJson<Order>();
			OrderStatus = orderStatus.FromJson<OrderStatusDetail>();
		}

		public override async void OnViewStarted(bool firstTime)
		{
			base.OnViewStarted(firstTime);

			//refresh from the server to get possible fare and tip values
			Order = await _accountService.GetHistoryOrderAsync(Order.Id);
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
				return this.GetCommand (() =>
				{ 
					ShowViewModelAndRemoveFromHistory<PaymentViewModel>(new { 
						order = Order.ToJson(),
						orderStatus = OrderStatus.ToJson()
					});
				});
			}
		}
	}
}

