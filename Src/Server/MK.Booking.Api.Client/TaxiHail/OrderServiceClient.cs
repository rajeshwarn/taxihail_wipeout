#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using OrderRatings = apcurium.MK.Common.Entity.OrderRatings;

#if !CLIENT
using apcurium.MK.Booking.Api.Client.Extensions;
#endif

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class OrderServiceClient : BaseServiceClient
    {
        public OrderServiceClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
        }

        public Task<OrderStatusDetail> CreateOrder(CreateOrderRequest orderRequest)
        {
            return Client.PostAsync<OrderStatusDetail>("/account/orders", orderRequest);
        }

        public Task<OrderStatusDetail> SwitchOrderToNextDispatchCompany(SwitchOrderToNextDispatchCompanyRequest request)
        {
            var req = string.Format("/account/orders/{0}/switchDispatchCompany", request.OrderId);
            return Client.PostAsync<OrderStatusDetail>(req, request);
        }

        public Task IgnoreDispatchCompanySwitch(Guid orderId)
        {
            var req = string.Format("/account/orders/{0}/ignoreDispatchCompanySwitch", orderId);
            return Client.PostAsync<string>(req, new IgnoreDispatchCompanySwitchRequest { OrderId = orderId });
        }

        public Task CancelOrder(Guid orderId)
        {
            var req = string.Format("/account/orders/{0}/cancel", orderId);
            return Client.PostAsync<string>(req, new CancelOrder { OrderId = orderId });
        }

        public Task SendReceipt(Guid orderId)
        {
            var req = string.Format("/account/orders/{0}/sendreceipt", orderId);
            return Client.PostAsync<string>(req, new SendReceipt());
        }

        public Task<IList<Order>> GetOrders()
        {
            var req = string.Format("/account/orders");
            var result = Client.GetAsync<IList<Order>>(req);
            return result;
        }

        public Task<Order> GetOrder(Guid orderId)
        {
            var req = string.Format("/account/orders/{0}", orderId);
            var result = Client.GetAsync<Order>(req);
            return result;
        }

        public Task RemoveFromHistory(Guid orderId)
        {
            var req = string.Format("/account/orders/{0}", orderId);
            return Client.DeleteAsync<string>(req);
        }

        public Task<OrderStatusDetail> GetOrderStatus(Guid orderId)
        {
            var req = string.Format("/account/orders/{0}/status", orderId);
            var result = Client.GetAsync<OrderStatusDetail>(req);
            return result;
        }

        public Task<OrderPairingDetail> GetOrderPairing(Guid orderId)
        {
            var req = string.Format("/account/orders/{0}/pairing", orderId);
            var result = Client.GetAsync<OrderPairingDetail>(req);
            return result;
        }

        public Task<OrderStatusDetail[]> GetActiveOrdersStatus()
        {
            var req = string.Format("/account/orders/status/active");
            var result = Client.GetAsync<OrderStatusDetail[]>(req);
            return result;
        }

        public Task<IEnumerable<RatingTypeWrapper>> GetRatingTypes(string currentLanguage)
        {
            var req = string.Format("/ratingtypes/{0}", currentLanguage);
            return Client.GetAsync<IEnumerable<RatingTypeWrapper>>(req);
        }

        public Task RateOrder(OrderRatingsRequest orderRatingRequest)
        {
            return Client.PostAsync<string>("/ratings/", orderRatingRequest);
        }

        public Task<OrderRatings> GetOrderRatings(Guid orderId)
        {
            var req = string.Format("/ratings/{0}", orderId);
            return Client.GetAsync<OrderRatings>(req);
        }

		public Task<OrderValidationResult> ValidateOrder(CreateOrderRequest orderRequest, string testZone = null, bool forError = false)
		{
            if (testZone.HasValue())
            {
                var req = string.Format("/account/orders/validate/{0}/{1}", forError, testZone);
                return Client.PostAsync<OrderValidationResult>(req, orderRequest);
            }
            else
            {
                var req = string.Format("/account/orders/validate/{0}", forError);
                return Client.PostAsync<OrderValidationResult>(req, orderRequest);
            }
        }

        public Task<bool> InitiateCallToDriver(Guid orderId)
        {
            var req = string.Format("/account/orders/{0}/calldriver", orderId);
            return Client.GetAsync<bool>(req);
        }

        public Task<int> GetOrderCountForAppRating()
		{
            return Client.GetAsync<int>("/account/ordercountforapprating");
		}
    }
}