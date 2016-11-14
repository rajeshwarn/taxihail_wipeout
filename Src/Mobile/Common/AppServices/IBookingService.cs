using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Models;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Resources;
using OrderRatings = apcurium.MK.Common.Entity.OrderRatings;
using Gratuity = apcurium.MK.Common.Entity.Gratuity;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface IBookingService
    {
        Task<DirectionInfo> GetFareEstimate(CreateOrderRequest order);

        string GetFareEstimateDisplay(DirectionInfo direction, ServiceType serviceType);

        bool IsStatusCompleted(OrderStatusDetail status);

        bool IsStatusTimedOut(string statusId);

        bool IsStatusDone(string statusId);

        bool IsOrderCancellable(OrderStatusDetail status);

        bool IsCallboxStatusActive(string statusId);

        bool IsCallboxStatusCompleted(string statusId);

        Task<bool> CancelOrder(Guid orderId);

        Task<bool> SendReceipt(Guid orderId);

        Task<OrderRepresentation> GetActiveOrder();

        bool HasLastOrder { get; }

        bool HasUnratedLastOrder { get; }

        bool NeedToSelectGratuity { get; }

        Task<bool> IsPaired(Guid orderId);

        Task<OrderStatusDetail> CreateOrder(CreateOrderRequest info);

        Task<OrderStatusDetail> SwitchOrderToNextDispatchCompany(Guid orderId, string nextDispatchCompanyKey, string nextDispatchCompanyName);

        Task IgnoreDispatchCompanySwitch(Guid orderId);

        Task<OrderValidationResult> ValidateOrder(CreateOrderRequest order);

        Task<OrderStatusDetail> GetOrderStatusAsync(Guid orderId);

        Task<OrderStatusDetail> GetLastOrderStatus();

        Guid GetUnratedLastOrder();

		void SetServiceTypeForProgressAnimation (ServiceType serviceType);

        void SetLastUnratedOrderId(Guid orderId, bool needToSelectGratuity);

        void ClearLastOrder();

        Task RemoveFromHistory(Guid orderId);

        Task<IEnumerable<RatingTypeWrapper>> GetRatingTypes();

        Task<OrderRatings> GetOrderRatingAsync(Guid orderId);

        Task SendRatingReview(OrderRatings orderRatings);

        Task PayGratuity(Gratuity gratuity);

        Task<OrderManualRideLinqDetail> PairWithManualRideLinq(string pairingCode, Address pickupAddress, ServiceType serviceType, string kountSessionId);

        Task UnpairFromManualRideLinq(Guid orderId);

        Task<bool> UpdateAutoTipForManualRideLinq(Guid orderId, int autoTipPercentage);

        Task<ManualRideLinqResponse> GetTripInfoFromManualRideLinq(Guid orderId);

        Task<bool> InitiateCallToDriver(Guid orderId);

		Task<bool> UpdateDropOff (Guid orderId, Address dropOffAddress);
    }
}