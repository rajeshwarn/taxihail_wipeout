using System;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class OrderViewModel : BaseViewModel
    {
        public Guid Id { get; set; }
// ReSharper disable once InconsistentNaming
        public int? IBSOrderId { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Note { get; set; }
        public Address PickupAddress { get; set; }
        public Address DropOffAddress { get; set; }
        public double? Fare { get; set; }
        public double? Toll { get; set; }
        public double? Tip { get; set; }
        public string Title { get; set; }
        public OrderRatings OrderRatings { get; set; }
        public bool ShowRightArrow { get; set; }
        public bool ShowPlusSign { get; set; }
        public bool IsFirst { get; set; }
        public bool IsLast { get; set; }

        private OrderStatus _status;
        public OrderStatus Status
        {
            get { return _status; }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}