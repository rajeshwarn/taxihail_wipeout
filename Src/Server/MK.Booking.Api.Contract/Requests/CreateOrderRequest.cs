#region

using System;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/account/orders", "POST")]
    public class CreateOrderRequest : BaseDto
    {
        public CreateOrderRequest()
        {
            PickupAddress = new Address();
            DropOffAddress = new Address();
            Settings = new BookingSettings();
            Payment = new PaymentSettings();
            Estimate = new RideEstimate();
            FromWebApp = false;
        }

        public Guid Id { get; set; }

        public DateTime? PickupDate { get; set; }

        public string Note { get; set; }

        public Address PickupAddress { get; set; }

        public Address DropOffAddress { get; set; }

        public BookingSettings Settings { get; set; }

        public PaymentSettings Payment { get; set; }

        public RideEstimate Estimate { get; set; }

        public string ClientLanguageCode { get; set; }

        public double? UserLatitude { get; set; }

        public double? UserLongitude { get; set; }
        
		public AccountChargeQuestion[] QuestionsAndAnswers { get; set; }

        public string PromoCode { get; set; }

        /// <summary>
        /// Optional: Manually specify the company where to dispatch the order.
        /// If both the OrderCompanyKey and OrderFleetId are set, OrderCompanyKey will have precedence over OrderFleetId.
        /// </summary>
        public string OrderCompanyKey { get; set; }

        /// <summary>
        /// Optional: Manually specify the company where to dispatch the order.
        /// If both the OrderCompanyKey and OrderFleetId are set, OrderCompanyKey will have precedence over OrderFleetId.
        /// </summary>
        public int? OrderFleetId { get; set; }

        public bool FromWebApp { get; set; }

        public string Cvv { get; set; }

        public double? TipIncentive { get; set; }
    }
}