using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;
using OrderRatings = apcurium.MK.Common.Entity.OrderRatings;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IBookingService
	{
		Task<DirectionInfo> GetFareEstimate(CreateOrder order);
       
		string GetFareEstimateDisplay(DirectionInfo direction);

        bool IsStatusCompleted(string statusId);

        bool IsStatusTimedOut(string statusId);

        bool IsStatusDone(string statusId);

		bool IsOrderCancellable(string statusId);

        bool IsCallboxStatusActive(string statusId);

        bool IsCallboxStatusCompleted(string statusId);
        
        bool CancelOrder(Guid orderId);

        bool SendReceipt(Guid orderId);

        bool HasLastOrder{get;}

        bool HasUnratedLastOrder { get; }

	    bool IsPaired(Guid orderId);

		Task<OrderStatusDetail> CreateOrder(CreateOrder info);

        Task<OrderValidationResult> ValidateOrder (CreateOrder order);

		Task<OrderStatusDetail> GetOrderStatusAsync(Guid orderId);
        
		Task<OrderStatusDetail> GetLastOrderStatus();

	    Guid GetUnratedLastOrder();

	    void SetLastUnratedOrderId(Guid orderId);
        
		void ClearLastOrder();

	    void RemoveFromHistory(Guid orderId);

        IEnumerable<RatingTypeWrapper> GetRatingTypes();

		[Obsolete("Migrate to async/await")]
		OrderRatings GetOrderRating(Guid orderId);
		Task<OrderRatings> GetOrderRatingAsync(Guid orderId);

	    void SendRatingReview(OrderRatings orderRatings);

    }
}

