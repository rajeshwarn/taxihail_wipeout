using System;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class ManualRideLinqSummaryViewModel : PageViewModel
	{
		private readonly IBookingService _bookingService;
		private OrderManualRideLinqDetail _orderManualRideLinqDetail;

		public ManualRideLinqSummaryViewModel(IBookingService bookingService)
		{
			_bookingService = bookingService;
		}

		public void Init(string orderManualRideLinqDetail)
		{
			OrderManualRideLinqDetail = JsonSerializer.DeserializeFromString<OrderManualRideLinqDetail>(orderManualRideLinqDetail);
		}

		public OrderManualRideLinqDetail OrderManualRideLinqDetail
		{
			get
			{
				return _orderManualRideLinqDetail;
			}
			set
			{
				_orderManualRideLinqDetail = value;

				RaisePropertyChanged();
			}
		}

		public ICommand GoToHome
		{
			get
			{
				return this.GetCommand(async () =>
				{
					ShowViewModelAndRemoveFromHistory<HomeViewModel>(new HomeViewModelPresentationHint(HomeViewModelState.Initial));
				});
			}
		}
	}
}

