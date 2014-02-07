using System;
using apcurium.MK.Booking.Mobile.AppServices.Orders;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class OrderReviewViewModel: ChildViewModel
    {
		readonly OrderWorkflowService _orderWorkflowService;
        
		public OrderReviewViewModel(OrderWorkflowService orderWorkflowService)
		{
			_orderWorkflowService = orderWorkflowService;

			this.Observe(orderWorkflowService.GetAndObserveBookingSettings(), settings => Settings = settings);
			this.Observe(orderWorkflowService.GetAndObservePickupAddress(), address => Address = address);
			this.Observe(orderWorkflowService.GetAndObservePickupDate(), date => Date = date);
		}

		private BookingSettings _settings;
		public BookingSettings Settings
		{
			get { return _settings; }
			set
			{
				if (value != _settings)
				{
					_settings = value;
					RaisePropertyChanged();
				}
			}
		}

		private Address _address;
		public Address Address
		{
			get { return _address; }
			set
			{
				if (value != _address)
				{
					_address = value;
					RaisePropertyChanged();
				}
			}
		}

		private DateTime? _date;
		public DateTime? Date
		{
			get{ return _date; }
			set
			{
				_date = value;
				RaisePropertyChanged();
			}
		}
    }
}

