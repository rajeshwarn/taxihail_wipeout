using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class OrderViewModel
    {
            public Guid Id { get; set; }

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

        public OrderStatus Status { get; set; }
    }
}