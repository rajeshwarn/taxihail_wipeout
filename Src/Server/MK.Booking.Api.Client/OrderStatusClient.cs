using System;
using System.Globalization;
using ServiceStack.Common;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Client
{
    public class OrderStatusClient: BaseServiceClient
    {
        public OrderStatusClient(string url, AuthInfo credential)
            : base(url, credential)
        {
        }

        public string GetStatus(Guid orderId)
        {
            var result = Client.Get<string>("/orderstatus/" + orderId.ToString());
            return result;   
        }
         
    }
}