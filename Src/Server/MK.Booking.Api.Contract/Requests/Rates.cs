using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.ServiceModel;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [AuthorizationRequired(ApplyTo.All, Permissions.Admin)]
    [RestService("/admin/rates", "GET, POST")]
    [RestService("/admin/rates/{Id}", "PUT, DELETE")]
    public class Rates
    {
        public Guid Id { get; set; }
        public RateType Type { get; set; }
        public string Name { get; set; }
        public decimal FlatRate { get; set; }
        public decimal PricePerPassenger { get; set; }
        public double DistanceMultiplicator { get; set; }
        public double TimeAdjustmentFactor { get; set; }
        public DayOfTheWeek DaysOfTheWeek { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class RatesResponse: IHasResponseStatus
    {
        public ResponseStatus ResponseStatus { get; set; }
    }
    
}
