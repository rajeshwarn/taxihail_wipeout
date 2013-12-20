using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class OrderServiceClient : BaseServiceClient
    {
        public OrderServiceClient(string url, string sessionId, string userAgent)
            : base(url, sessionId,userAgent)
        {
        }

        public OrderStatusDetail CreateOrder(CreateOrder order)
        {
            var req = string.Format("/account/orders");
            var result = Client.Post<OrderStatusDetail>(req, order);
            return result;
        }

        public void CancelOrder(Guid orderId)
        {
            var req = string.Format("/account/orders/{0}/cancel", orderId);
            Client.Post<string>(req, new CancelOrder { OrderId = orderId  });            
        }

        public void SendReceipt(Guid orderId)
        {
            var req = string.Format("/account/orders/{0}/sendreceipt", orderId);
            Client.Post<string>(req, new SendReceipt());
        }

        public IList<Order> GetOrders()
        {            
            var req = string.Format("/account/orders");
            var result = Client.Get<IList<Order>>(req).ToArray();
            return result;
        }

        public Order GetOrder(Guid orderId)
        {
            var req = string.Format("/account/orders/{0}", orderId);
            var result = Client.Get<Order>(req);
            return result;
        }

        public void RemoveFromHistory(Guid orderId)
        {
            var req = string.Format("/account/orders/{0}", orderId);
            Client.Delete<string>(req);
        }

        public OrderStatusDetail GetOrderStatus(Guid orderId)
        {
            var req = string.Format("/account/orders/{0}/status", orderId);
            var result = Client.Get<OrderStatusDetail>(req);
            return result;
        }

        public OrderPairingDetail GetOrderPairing(Guid orderId)
        {
            var req = string.Format("/account/orders/{0}/pairing", orderId);
            var result = Client.Get<OrderPairingDetail>(req);
            return result;
        }

        public OrderStatusDetail[] GetActiveOrdersStatus()
        {
            var req = string.Format("/account/orders/status/active");
            var result = Client.Get<OrderStatusDetail[]>(req);
            return result;
        }

        public List<RatingType> GetRatingTypes()
        {
            return Client.Get<List<RatingType>>("/ratingtypes");
        }

        public void RateOrder(OrderRatingsRequest orderRatingRequest)
        {
            Client.Post<string>("/ratings/", orderRatingRequest);
        }

        public Common.Entity.OrderRatings GetOrderRatings(Guid orderId)
        {
            var req = string.Format("/ratings/{0}", orderId);
            return Client.Get<Common.Entity.OrderRatings>(req);
        }
        
        public OrderValidationResult ValidateOrder(CreateOrder order, string testZone = null)
        {
            if (testZone.HasValue())
            {
                var req = string.Format("/account/orders/validate/" + testZone);
                return Client.Post<OrderValidationResult>(req, order);
            }
            else
            {
                var req = string.Format("/account/orders/validate");
                return Client.Post<OrderValidationResult>(req, order);
            }
        }
    }
}
