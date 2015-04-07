using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Entity;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ServiceStack.ServiceModel.Serialization;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
    public class ManualRideLinqDetailsViewModel : BaseViewModel
    {
        private readonly IBookingService _bookingService;
        private OrderManualRideLinqDetail _orderManualRideLinqDetail;

        public ManualRideLinqDetailsViewModel(IBookingService bookingService)
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