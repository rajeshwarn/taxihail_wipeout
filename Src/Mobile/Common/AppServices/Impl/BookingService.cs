using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Api.Contract.Resources;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using Address = apcurium.MK.Common.Entity.Address;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.Mobile.Extensions;
using OrderRatings = apcurium.MK.Common.Entity.OrderRatings;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common;
using Direction = apcurium.MK.Common.Entity.DirectionSetting;
using apcurium.MK.Booking.Api.Client;
using Cirrious.MvvmCross.Plugins.PhoneCall;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class BookingService : BaseService, IBookingService
    {
        public bool IsValid (CreateOrder info)        
        {
            //InvalidBookinInfoWhenDestinationIsRequired

            var destinationIsRequired = TinyIoCContainer.Current.Resolve<IConfigurationManager>().GetSetting<bool>("Client.DestinationIsRequired", false);

            return info.PickupAddress.BookAddress.HasValue () 
                && info.PickupAddress.HasValidCoordinate () && (!destinationIsRequired || (  info.DropOffAddress.BookAddress.HasValue () 
                                                                                           && info.DropOffAddress.HasValidCoordinate () ) ) ;
        }
	
		protected IConfigurationManager Config {
            get { return TinyIoCContainer.Current.Resolve<IConfigurationManager> (); }
        }

        public Task<OrderValidationResult> ValidateOrder (CreateOrder order)
        {
            return TinyIoCContainer.Current.Resolve<OrderServiceClient>().ValidateOrder(order);
        }

        public bool IsPaired(Guid orderId)
        {
            var isPaired = false;
            UseServiceClient<OrderServiceClient>(service =>
                {
                    isPaired = service.GetOrderPairing(orderId) != null;
                });
            return isPaired;
        }

        public OrderStatusDetail CreateOrder (CreateOrder order)
        {
            var orderDetail = UseServiceClientAsync<OrderServiceClient, OrderStatusDetail>(service => service.CreateOrder(order));

			if (orderDetail.IbsOrderId.HasValue && orderDetail.IbsOrderId > 0) {
                Cache.Set ("LastOrderId", orderDetail.OrderId.ToString ()); // Need to be cached as a string because of a jit error on device
            }

            ThreadPool.QueueUserWorkItem (o => TinyIoCContainer.Current.Resolve<IAccountService> ().RefreshCache (true));

            return orderDetail;
        }

		public bool CallIsEnabled { get{ return !Config.GetSetting("Client.HideCallDispatchButton", false); } }

        private void HandleCreateOrderError (Exception ex)
        {
            var title = TinyIoCContainer.Current.Resolve<ILocalization>()["ErrorCreatingOrderTitle"];

            string message = TinyIoCContainer.Current.Resolve<ILocalization>()["ServiceError_ErrorCreatingOrderMessage_NoCall"];

			if (CallIsEnabled)
			{
                message = TinyIoCContainer.Current.Resolve<ILocalization>()["ServiceError_ErrorCreatingOrderMessage"];
			}

            try {
                if (ex is WebServiceException) {
                    if (((WebServiceException)ex).ErrorCode ==ErrorCode.CreateOrder_RuleDisable.ToString ()) {
                        message = ((WebServiceException)ex).ErrorMessage;
                    } else {
                        
						var messageKey = "ServiceError" + ((WebServiceException)ex).ErrorCode;
                        var errorMessage = TinyIoCContainer.Current.Resolve<ILocalization>()[messageKey];
						if(errorMessage != messageKey)
						{
							message = errorMessage;
						}

						if ( !CallIsEnabled )
						{
							messageKey += "_NoCall";
                            errorMessage = TinyIoCContainer.Current.Resolve<ILocalization>()[messageKey];
							if(errorMessage != messageKey)
							{
								message = errorMessage;
							}
						}
                    }
                }
            } catch (Exception exe)
            {
                Logger.LogError(exe);
            }

            var settings = TinyIoCContainer.Current.Resolve<IAppSettings> ();
			if (CallIsEnabled)
			{
				string err = string.Format(message, settings.ApplicationName, Config.GetSetting("DefaultPhoneNumberDisplay"));
				TinyIoCContainer.Current.Resolve<IMessageService>().ShowMessage(title, err, "Call", () => CallCompany(settings.ApplicationName, Config.GetSetting("DefaultPhoneNumber")), "Cancel", delegate
				{			
				});
			}
			else
			{
				TinyIoCContainer.Current.Resolve<IMessageService>().ShowMessage(title, message);
			}
        }

        private void CallCompany (string name, string number)
        {
            TinyIoCContainer.Current.Resolve<IMvxPhoneCallTask> ().MakePhoneCall (name, number);
        }

        public OrderStatusDetail GetOrderStatus (Guid orderId)
        {
            var r = new OrderStatusDetail ();
            UseServiceClientAsync<OrderServiceClient, OrderStatusDetail>(service => service.GetOrderStatus(orderId));
            return r;
        }

        public bool HasLastOrder {
            get{ return Cache.Get<string> ("LastOrderId").HasValue ();}
        }

        public Task<OrderStatusDetail> GetLastOrderStatus ()
        {
            var task = Task.Factory.StartNew (() =>
            {
                var result = new OrderStatusDetail ();

                if (!HasLastOrder) {
                    throw new InvalidOperationException ();
                }
                var lastOrderId = Cache.Get<string> ("LastOrderId");  // Need to be cached as a string because of a jit error on device
                result = UseServiceClientAsync<OrderServiceClient, OrderStatusDetail>(service => service.GetOrderStatus (new Guid (lastOrderId)));
                return result;
            });

            task.ContinueWith (t => Logger.LogError (t.Exception), TaskContinuationOptions.OnlyOnFaulted);

            return task;
        }

        public void ClearLastOrder ()
        {
            Cache.Set ("LastOrderId", (string)null); // Need to be cached as a string because of a jit error on device
        }

        public void RemoveFromHistory (Guid orderId)
        {
            UseServiceClient<OrderServiceClient> (service => service.RemoveFromHistory (orderId));
        }

        public bool IsCompleted (Guid orderId)
        {
            var status = GetOrderStatus (orderId);
			return IsStatusCompleted (status.IbsStatusId);
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
                statusId.SoftEqual (VehicleStatuses.Common.CancelledDone);
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
            return
                statusId.SoftEqual(VehicleStatuses.Common.Arrived) ;
        }

        public bool IsStatusDone (string statusId)
        {
            return statusId.SoftEqual (VehicleStatuses.Common.Done);
        }

        public DirectionInfo GetFareEstimate(Address pickup, Address destination, DateTime? pickupDate)
        {
            var tarifMode = TinyIoCContainer.Current.Resolve<IConfigurationManager>().GetSetting<DirectionSetting.TarifMode>("Direction.TarifMode", DirectionSetting.TarifMode.AppTarif);            
            var directionInfo = new DirectionInfo();
            
            if (pickup.HasValidCoordinate() && destination.HasValidCoordinate())
            {
                if (tarifMode != DirectionSetting.TarifMode.AppTarif)
                {
                    directionInfo = UseServiceClientAsync<IIbsFareClient, DirectionInfo>(service => service.GetDirectionInfoFromIbs(pickup.Latitude, pickup.Longitude, destination.Latitude, destination.Longitude));                                                            
                }

                if (tarifMode == DirectionSetting.TarifMode.AppTarif || (tarifMode == DirectionSetting.TarifMode.Both && directionInfo.Price == 0d))
                {
                    directionInfo = TinyIoCContainer.Current.Resolve<IGeolocService>().GetDirectionInfo(pickup.Latitude, pickup.Longitude, destination.Latitude, destination.Longitude, pickupDate);                    
                }            

                return directionInfo ?? new DirectionInfo();
            }

            return new DirectionInfo();
        }

        public string GetFareEstimateDisplay (CreateOrder order, string formatString, string defaultFare, bool includeDistance, string cannotGetFareText)
        {
            var fareEstimate = TinyIoCContainer.Current.Resolve<ILocalization>()[defaultFare];

            if (order != null && order.PickupAddress.HasValidCoordinate() && order.DropOffAddress.HasValidCoordinate())
            {
                var estimatedFare = GetFareEstimate(order.PickupAddress, order.DropOffAddress, order.PickupDate);

                var willShowFare = estimatedFare.Price.HasValue && estimatedFare.Price.Value > 0;                                

                if (estimatedFare.Price.HasValue && willShowFare)
                {                    
                    var maxEstimate = Config.GetSetting<double>("Client.MaxFareEstimate", 100);
                    if (formatString.HasValue() || (estimatedFare.Price.Value > maxEstimate && TinyIoCContainer.Current.Resolve<ILocalization>()["EstimatePriceOver100"].HasValue()))
                    {
                        fareEstimate = String.Format(TinyIoCContainer.Current.Resolve<ILocalization>()[estimatedFare.Price.Value > maxEstimate 
                                                                           ? "EstimatePriceOver100"
                                                                           : formatString], 
                                                 estimatedFare.FormattedPrice);
                    }
                    else
                    {
                        fareEstimate = estimatedFare.FormattedPrice;
                    }

                    if (includeDistance && estimatedFare.Distance.HasValue)
                    {
                        var destinationString = " " + String.Format(TinyIoCContainer.Current.Resolve<ILocalization>()["EstimateDistance"], estimatedFare.FormattedDistance);
                        if (!string.IsNullOrWhiteSpace(destinationString))
                        {
                            fareEstimate += destinationString;
                        }
                    }
                }
                else
                {
                    fareEstimate = String.Format(TinyIoCContainer.Current.Resolve<ILocalization>()[cannotGetFareText]);
                }
            }

            return fareEstimate;
        }

        public bool CancelOrder (Guid orderId)
        {
            bool isCompleted = false;

            UseServiceClient<OrderServiceClient> (service =>
            {
                service.CancelOrder (orderId);
                isCompleted = true;
            });
            return isCompleted;
        }

        public bool SendReceipt (Guid orderId)
        {
            bool isCompleted = false;

            UseServiceClient<OrderServiceClient> (service =>
            {
                service.SendReceipt (orderId);
                isCompleted = true;
            });
            return isCompleted;
        }

        public List<RatingType> GetRatingType ()
        {
            var ratingType =
            UseServiceClientAsync<OrderServiceClient, List<RatingType>>(service => service.GetRatingTypes());
            return ratingType;
        }

        public OrderRatings GetOrderRating (Guid orderId)
        {
            var orderRate = UseServiceClientAsync<OrderServiceClient, OrderRatings> (service => service.GetOrderRatings (orderId));
            return orderRate;
        }

        public void SendRatingReview (OrderRatings orderRatings)
        {
            var request = new OrderRatingsRequest{ Note = orderRatings.Note, OrderId = orderRatings.OrderId, RatingScores = orderRatings.RatingScores };
            UseServiceClient<OrderServiceClient> (service => service.RateOrder (request));
        }

    }
}

