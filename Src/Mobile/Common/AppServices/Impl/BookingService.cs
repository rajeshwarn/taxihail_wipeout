using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.PhoneCall;
using Direction = apcurium.MK.Common.Entity.DirectionSetting;
using OrderRatings = apcurium.MK.Common.Entity.OrderRatings;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class BookingService : BaseService, IBookingService
    {
		readonly IAccountService _accountService;
		readonly ILocalization _localize;
		readonly IMessageService _messageService;
		readonly IAppSettings _appSettings;
		readonly IMvxPhoneCallTask _phoneCallTask;
		readonly IGeolocService _geolocService;

		public BookingService(IAccountService accountService,
			ILocalization localize,
			IMessageService messageService,
			IAppSettings appSettings,
			IMvxPhoneCallTask phoneCallTask,
			IGeolocService geolocService)
		{
			_geolocService = geolocService;
			_phoneCallTask = phoneCallTask;
			_appSettings = appSettings;
			_messageService = messageService;
			_localize = localize;
			_accountService = accountService;
		}

        public Task<OrderValidationResult> ValidateOrder (CreateOrder order)
        {
			return Mvx.Resolve<OrderServiceClient>().ValidateOrder(order);
        }

        public bool IsPaired(Guid orderId)
        {
			var isPairedResult = UseServiceClientTask<OrderServiceClient, OrderPairingDetail>(service => service.GetOrderPairing(orderId));
			return isPairedResult != null;
        }

		public async Task<OrderStatusDetail> CreateOrder (CreateOrder order)
        {
			order.ClientLanguageCode = _localize.CurrentLanguage;
			var orderDetail = await UseServiceClientAsync<OrderServiceClient, OrderStatusDetail>(service => service.CreateOrder(order));

			if (orderDetail.IBSOrderId.HasValue
				&& orderDetail.IBSOrderId > 0)
			{
                UserCache.Set ("LastOrderId", orderDetail.OrderId.ToString ()); // Need to be cached as a string because of a jit error on device
            }

			Task.Run(() => _accountService.RefreshCache (true));

            return orderDetail;
        }

        private void CallCompany (string name, string number)
        {
			_phoneCallTask.MakePhoneCall (name, number);
        }

        public OrderStatusDetail GetOrderStatus (Guid orderId)
        {
			//TODO: Migrate code to async/await
			var task = GetOrderStatusAsync(orderId);
			task.Wait();
			return task.Result;
        }

		public Task<OrderStatusDetail> GetOrderStatusAsync (Guid orderId)
		{
			return UseServiceClientAsync<OrderServiceClient, OrderStatusDetail>(service => service.GetOrderStatus(orderId));
		}

        public bool HasLastOrder {
            get{ return UserCache.Get<string> ("LastOrderId").HasValue ();}
        }

        public Task<OrderStatusDetail> GetLastOrderStatus ()
        {
			try
            {
                if (!HasLastOrder)
                {
                    throw new InvalidOperationException ();
                }
                var lastOrderId = UserCache.Get<string> ("LastOrderId");  // Need to be cached as a string because of a jit error on device
				return UseServiceClientAsync<OrderServiceClient, OrderStatusDetail>(service => service.GetOrderStatus (new Guid (lastOrderId)));
            }
			catch(Exception e)
			{
				Logger.LogError(e);
				throw;
			}
        }

        public void ClearLastOrder ()
        {
            UserCache.Set ("LastOrderId", (string)null); // Need to be cached as a string because of a jit error on device
        }

        public void RemoveFromHistory (Guid orderId)
        {
			UseServiceClientTask<OrderServiceClient> (service => service.RemoveFromHistory (orderId));
        }

        public bool IsCompleted (Guid orderId)
        {
            var status = GetOrderStatus (orderId);
			return IsStatusCompleted (status.IBSStatusId);
        }

        public bool IsStatusTimedOut(string statusId)
        {
            return statusId != null && statusId.SoftEqual(VehicleStatuses.Common.Timeout);
        }

        public bool IsStatusCompleted (string statusId)
        {
            return statusId.IsNullOrEmpty () ||
                statusId.SoftEqual (VehicleStatuses.Common.Cancelled) ||
                statusId.SoftEqual (VehicleStatuses.Common.Done) ||
                statusId.SoftEqual (VehicleStatuses.Common.NoShow) ||
				statusId.SoftEqual (VehicleStatuses.Common.CancelledDone) || 
				statusId.SoftEqual (VehicleStatuses.Common.MeterOffNotPayed);
        }

        public bool IsCallboxStatusActive(string statusId)
        {
            return statusId.IsNullOrEmpty() ||
                statusId.SoftEqual(VehicleStatuses.Common.Scheduled) ||
                statusId.SoftEqual(VehicleStatuses.Common.Waiting) ||
                statusId.SoftEqual(VehicleStatuses.Common.Assigned) ||
                statusId.SoftEqual(VehicleStatuses.Common.Arrived);
        }

        public bool IsCallboxStatusCompleted(string statusId)
        {
            return statusId.SoftEqual(VehicleStatuses.Common.Arrived) ;
        }

        public bool IsStatusDone (string statusId)
        {
            return statusId.SoftEqual(VehicleStatuses.Common.Done) || statusId.SoftEqual(VehicleStatuses.Common.MeterOffNotPayed);
        }

		public async Task<DirectionInfo> GetFareEstimate(Address pickup, Address destination, int? vehicleTypeId, DateTime? pickupDate)
        {
			var tarifMode = _appSettings.Data.TarifMode;            
            var directionInfo = new DirectionInfo();
            
            if (pickup.HasValidCoordinate() && destination.HasValidCoordinate())
            {
                if (tarifMode != TarifMode.AppTarif)
                {
					directionInfo = await UseServiceClientAsync<IIbsFareClient, DirectionInfo>(service => service.GetDirectionInfoFromIbs(pickup.Latitude, pickup.Longitude, destination.Latitude, destination.Longitude));                                                            
                }

                if (tarifMode == TarifMode.AppTarif || (tarifMode == TarifMode.Both && directionInfo.Price == 0d))
                {
					directionInfo = await _geolocService.GetDirectionInfo(pickup.Latitude, pickup.Longitude, destination.Latitude, destination.Longitude, vehicleTypeId, pickupDate);                    
                }            

				return directionInfo ?? new DirectionInfo();
            }

            return new DirectionInfo();
        }

		public async Task<string> GetFareEstimateDisplay (DirectionInfo direction, string fareFormat, string noFareText, bool includeDistance, string cannotGetFareText)
        {
			var fareEstimate = _localize[noFareText];

			if (direction.Distance.HasValue)
            {
				var willShowFare = direction.Price.HasValue && direction.Price.Value > 0;                                

				if (direction.Price.HasValue && willShowFare)
                {                    
					var maxEstimate = _appSettings.Data.MaxFareEstimate;
					if (fareFormat.HasValue() || (direction.Price.Value > maxEstimate && _localize["EstimatePriceOver100"].HasValue()))
                    {
						fareEstimate = String.Format(_localize[direction.Price.Value > maxEstimate 
                                                                           ? "EstimatePriceOver100"
																		   : fareFormat], 
							direction.FormattedPrice);
                    }
                    else
                    {
						fareEstimate = direction.FormattedPrice;
                    }

					if (includeDistance && direction.Distance.HasValue)
                    {
						var destinationString = " " + String.Format(_localize["EstimateDistance"], direction.FormattedDistance);
                        if (!string.IsNullOrWhiteSpace(destinationString))
                        {
                            fareEstimate += destinationString;
                        }
                    }
                }
                else
                {
					fareEstimate = String.Format(_localize[cannotGetFareText]);
                }
            }

            return fareEstimate;
        }

        public bool CancelOrder (Guid orderId)
        {
			var isCompleted = true;
			try{
				UseServiceClientTask<OrderServiceClient> (service => service.CancelOrder (orderId));
			}catch{
				isCompleted = false;
			}
            return isCompleted;
        }

        public bool SendReceipt (Guid orderId)
        {
			var isCompleted = true;
			try{
				UseServiceClientTask<OrderServiceClient> (service => service.SendReceipt (orderId));			
			}catch{
				isCompleted = false;
			}
            return isCompleted;
        }

        public List<RatingType> GetRatingType ()
        {
            var ratingType =
            UseServiceClientTask<OrderServiceClient, List<RatingType>>(service => service.GetRatingTypes());
            return ratingType;
        }

        public OrderRatings GetOrderRating (Guid orderId)
        {
			// TODO: Migrate code to async version
			var task = GetOrderRatingAsync(orderId);
			task.Wait();
			return task.Result;
        }

		public Task<OrderRatings> GetOrderRatingAsync (Guid orderId)
		{
			return UseServiceClientAsync<OrderServiceClient, OrderRatings> (service => service.GetOrderRatings (orderId));
		}

        public void SendRatingReview (OrderRatings orderRatings)
        {
            var request = new OrderRatingsRequest{ Note = orderRatings.Note, OrderId = orderRatings.OrderId, RatingScores = orderRatings.RatingScores };
			UseServiceClientTask<OrderServiceClient> (service => service.RateOrder (request));
        }

    }
}

