using System;
using apcurium.MK.Booking.Mobile.AppServices;
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class BookLaterViewModel : ChildViewModel
	{
		readonly IOrderWorkflowService _orderWorkflowService;
		public BookLaterViewModel(IOrderWorkflowService orderWorkflowService)
		{
			_orderWorkflowService = orderWorkflowService;

		}

		public ICommand SetPickupDateAndBook
		{
			get
			{
				return GetCommand<DateTime?>(date => 
				{
					if(date.HasValue)
					{
						_orderWorkflowService.SetPickupDate(date);
						// TODO show confirmation screen
					}
				});
			}
		}
	}
}

