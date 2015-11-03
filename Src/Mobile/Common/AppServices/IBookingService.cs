using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Resources;
using OrderRatings = apcurium.MK.Common.Entity.OrderRatings;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface IBookingService
    {
        Task<DirectionInfo> GetFareEstimate(CreateOrder order);

        string GetFareEstimateDisplay(DirectionInfo direction);

        bool IsStatusCompleted(OrderStatusDetail status);

        bool IsStatusTimedOut(string statusId);

        bool IsStatusDone(string statusId);

        bool IsOrderCancellable(OrderStatusDetail status);

        bool IsCallboxStatusActive(string statusId);

        bool IsCallboxStatusCompleted(string statusId);

        Task<bool> CancelOrder(Guid orderId);

        Task<bool> SendReceipt(Guid orderId);

        bool HasLastOrder { get; }

        bool HasUnratedLastOrder { get; }

        Task<bool> IsPaired(Guid orderId);

        Task<OrderStatusDetail> CreateOrder(CreateOrder info);

        Task<OrderStatusDetail> SwitchOrderToNextDispatchCompany(Guid orderId, string nextDispatchCompanyKey, string nextDispatchCompanyName);

        Task IgnoreDispatchCompanySwitch(Guid orderId);

        Task<OrderValidationResult> ValidateOrder(CreateOrder order);

        Task<OrderStatusDetail> GetOrderStatusAsync(Guid orderId);

        Task<OrderStatusDetail> GetLastOrderStatus();

        Guid GetUnratedLastOrder();

        void SetLastUnratedOrderId(Guid orderId);

        void ClearLastOrder();

        Task RemoveFromHistory(Guid orderId);

        Task<IEnumerable<RatingTypeWrapper>> GetRatingTypes();

        Task<OrderRatings> GetOrderRatingAsync(Guid orderId);

        Task SendRatingReview(OrderRatings orderRatings);

        Task<OrderManualRideLinqDetail> PairWithManualRideLinq(string pairingCode, Address pickupAddress);

        Task UnpairFromManualRideLinq(Guid orderId);

        Task<bool> UpdateAutoTipForManualRideLinq(Guid orderId, int autoTipPercentage);

        Task<ManualRideLinqResponse> GetTripInfoFromManualRideLinq(Guid orderId);

        Task<bool> InitiateCallToDriver(Guid orderId);
    }
}