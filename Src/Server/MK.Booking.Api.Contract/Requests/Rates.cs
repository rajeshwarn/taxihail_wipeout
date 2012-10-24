using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface.ServiceModel;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RestService("/rates", "GET, POST")]
    [RestService("/rates/{Id}", "PUT, DELETE")]
    public class Rates
    {
        public Guid Id { get; set; }
        public decimal FlatRate { get; set; }
        public decimal PricePerPassenger { get; set; }
        public double DistanceMultiplicator { get; set; }
        public double TimeAdjustmentFactor { get; set; }
        public DayOfTheWeek DaysOfTheWeek { get; set; }
    }

    public class RatesResponse: IHasResponseStatus
    {
        public ResponseStatus ResponseStatus { get; set; }
    }
    
}
