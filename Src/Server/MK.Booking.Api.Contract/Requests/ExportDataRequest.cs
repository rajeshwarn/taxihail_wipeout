using ServiceStack.ServiceHost;
using System;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/admin/export/{Target}", "POST")]
    public class ExportDataRequest
    {
        public Guid? AccountId { get; set; }
        public DataType Target { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public enum DataType
    {
        Orders,
        Accounts,
        Promotions
    }
}