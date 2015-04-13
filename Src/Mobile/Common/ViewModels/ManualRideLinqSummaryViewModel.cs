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
		private OrderManualRideLinqDetail _orderManualRideLinqDetail;

		public ManualRideLinqSummaryViewModel()
		{
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
				return this.GetCommand(() =>
				{
					ShowViewModelAndRemoveFromHistory<HomeViewModel>(new HomeViewModelPresentationHint(HomeViewModelState.Initial));
				});
			}
		}
	}
}

