using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Entity;
using Address = apcurium.MK.Common.Entity.Address;
using OrderRatings = apcurium.MK.Common.Entity.OrderRatings;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IBookingService
	{
        DirectionInfo GetFareEstimate(Address pickup, Address destination, DateTime? pickupDate = null);
       
        string GetFareEstimateDisplay(CreateOrder order, string fareFormat, string noFareText, bool includeDistance, string cannotGetFareText);

        bool IsValid(CreateOrder info);
				
		bool IsCompleted(Guid orderId);

        bool IsStatusCompleted(string statusId);

        bool IsStatusTimedOut(string statusId);

        bool IsStatusDone(string statusId);

        bool IsCallboxStatusActive(string statusId);

        bool IsCallboxStatusCompleted(string statusId);
        
        bool CancelOrder(Guid orderId);

        bool SendReceipt(Guid orderId);

        bool HasLastOrder{get;}

	    bool IsPaired(Guid orderId);

        OrderStatusDetail CreateOrder(CreateOrder info);

        Task<OrderValidationResult> ValidateOrder (CreateOrder order);

        OrderStatusDetail GetOrderStatus(Guid orderId);
        
		Task<OrderStatusDetail> GetLastOrderStatus();
        
		void ClearLastOrder();

	    void RemoveFromHistory(Guid orderId);	       
			    
	    List<RatingType> GetRatingType();

	    OrderRatings GetOrderRating(Guid orderId);

	    void SendRatingReview(OrderRatings orderRatings);

    }
}

