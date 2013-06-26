using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Entity;
using Address = apcurium.MK.Common.Entity.Address;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IBookingService
	{
	    
       
        string GetFareEstimateDisplay(CreateOrder order, string fareFormat, string noFareText, bool includeDistance, string cannotGetFareText);

        bool IsValid(CreateOrder info);
				
		bool IsCompleted(Guid orderId);

        bool IsStatusCompleted(string statusId);

        bool IsStatusDone(string statusId);

        bool IsCallboxStatusActive(string statusId);

        bool IsCallboxStatusCompleted(string statusId);
        
        bool CancelOrder(Guid orderId);

        bool SendReceipt(Guid orderId);

        bool HasLastOrder{get;}

        OrderStatusDetail CreateOrder(CreateOrder info);

        OrderValidationResult ValidateOrder (CreateOrder order);

        OrderStatusDetail GetOrderStatus(Guid orderId);
        
		Task<OrderStatusDetail> GetLastOrderStatus();
        
		void ClearLastOrder();

	    void RemoveFromHistory(Guid orderId);	       
			    
	    List<RatingType> GetRatingType();

	    apcurium.MK.Common.Entity.OrderRatings GetOrderRating(Guid orderId);

	    void SendRatingReview(Common.Entity.OrderRatings orderRatings);

        void FinalizePayment (Guid orderId, double amount, string carNumber, long transactionId, int ibsOrderNumber);


    }
}

