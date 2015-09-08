using System.Windows.Input;
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
			    if (_orderManualRideLinqDetail != value)
			    {
                    _orderManualRideLinqDetail = value;

                    RaisePropertyChanged();
                    RaisePropertyChanged(() => FormattedDistance);
                    RaisePropertyChanged(() => FormattedFare);
                    RaisePropertyChanged(() => FormattedTax);
                    RaisePropertyChanged(() => FormattedTip);
                    RaisePropertyChanged(() => FormattedTotal);
			    }
			}
		}

	    public string FormattedDistance
	    {
	        get { return string.Format("{0} {1}", _orderManualRideLinqDetail.Distance ?? 0, Settings.DistanceFormat); }
	    }

	    public string FormattedFare
	    {
	        get { return CultureProvider.FormatCurrency(_orderManualRideLinqDetail.Fare ?? 0); }
	    }

        public string FormattedTax
        {
            get { return CultureProvider.FormatCurrency(_orderManualRideLinqDetail.Tax ?? 0); }
        }

        public string FormattedTip
        {
            get { return CultureProvider.FormatCurrency(_orderManualRideLinqDetail.Tip ?? 0); }
        }

        public string FormattedTotal
        {
            get { return CultureProvider.FormatCurrency(_orderManualRideLinqDetail.Total ?? 0); }
        }

		public ICommand GoToHome
		{
			get
			{
				return this.GetCommand(() =>
				{
					Close(this);
				});
			}
		}
	}
}

