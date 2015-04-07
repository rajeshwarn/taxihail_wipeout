using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Entity;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class ManualRideLinqStatusViewModel : BaseViewModel
    {
        private readonly IBookingService _bookingService;
        private OrderManualRideLinqDetail _orderManualRideLinqDetail;

        public ManualRideLinqStatusViewModel(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public void Init(string orderManualRideLinqDetail)
        {
            OrderManualRideLinqDetail = JsonSerializer.DeserializeFromString<OrderManualRideLinqDetail>(orderManualRideLinqDetail);
        }

        public OrderManualRideLinqDetail OrderManualRideLinqDetail
        {
            get { return _orderManualRideLinqDetail; }
            set
            {
                _orderManualRideLinqDetail = value; 
                RaisePropertyChanged();
            }
        }

        public ICommand UnpairFromRideLinq
        {
            get
            {
                return this.GetCommand(() => _bookingService.ManualRideLinqUnpair(_orderManualRideLinqDetail.OrderId));
            }
        }
    }
}