using System;
using apcurium.MK.Booking.Mobile.AppServices;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class InitializeOrderForAccountPaymentViewModel : PageViewModel
	{
		private readonly IOrderWorkflowService _orderWorkflowService;

		public InitializeOrderForAccountPaymentViewModel(IOrderWorkflowService orderWorkflowService)
		{
			_orderWorkflowService = orderWorkflowService;
		}

		public void Init()
		{
			_orderWorkflowService.GetAccountPaymentQuestions ();
		}
	}
}

