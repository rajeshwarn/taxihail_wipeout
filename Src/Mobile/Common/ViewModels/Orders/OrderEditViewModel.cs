using System;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Entity;
using ServiceStack.Text;
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class OrderEditViewModel: ChildViewModel
	{
		readonly IOrderWorkflowService _orderWorkflowService;

		public OrderEditViewModel(IOrderWorkflowService orderWorkflowService)
		{
			_orderWorkflowService = orderWorkflowService;

			this.Observe(orderWorkflowService.GetAndObserveBookingSettings(), settings => Settings = settings);

			RideSettings = new RideSettingsViewModel();
			RideSettings.Init(Settings.ToJson());
		}

		public RideSettingsViewModel RideSettings { get; set; }

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
	}
}

