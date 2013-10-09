using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Api.Contract.Resources;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using Address = apcurium.MK.Common.Entity.Address;
using ServiceStack.ServiceClient.Web;
using Cirrious.MvvmCross.Interfaces.Platform.Tasks;
using apcurium.MK.Booking.Mobile.Extensions;
using OrderRatings = apcurium.MK.Common.Entity.OrderRatings;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common;

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




        protected ILogger Logger {
            get { return TinyIoCContainer.Current.Resolve<ILogger> (); }
        }

        protected ICacheService Cache {
            get { return TinyIoCContainer.Current.Resolve<ICacheService> (); }
        }

        protected IConfigurationManager Config {
            get { return TinyIoCContainer.Current.Resolve<IConfigurationManager> (); }
        }

        public Task<OrderValidationResult> ValidateOrder (CreateOrder order)
        {
            return Task.Run(() =>
            {
                var validationResut = new OrderValidationResult();
                
                UseServiceClient<OrderServiceClient>(service =>
                {
                    validationResut = service.ValidateOrder(order);
                }, ex => Logger.LogError(ex));
                return validationResut;
            });
        }
        public OrderStatusDetail CreateOrder (CreateOrder order)
        {
            var orderDetail = new OrderStatusDetail ();
            
            UseServiceClient<OrderServiceClient> (service =>
            {
                orderDetail = service.CreateOrder (order);
            }, ex => HandleCreateOrderError (ex, order));

            if (orderDetail.IBSOrderId.HasValue && orderDetail.IBSOrderId > 0) {
                Cache.Set ("LastOrderId", orderDetail.OrderId.ToString ()); // Need to be cached as a string because of a jit error on device
            }

            ThreadPool.QueueUserWorkItem (o =>
            {
                TinyIoCContainer.Current.Resolve<IAccountService> ().RefreshCache (true);
            });

            return orderDetail;

        }

        private void HandleCreateOrderError (Exception ex, CreateOrder order)
        {
            var appResource = TinyIoCContainer.Current.Resolve<IAppResource> ();
            var title = appResource.GetString ("ErrorCreatingOrderTitle");


            var message = appResource.GetString ("ServiceError_ErrorCreatingOrderMessage"); //= Resources.GetString(Resource.String.ServiceErrorDefaultMessage);


            try {
                if (ex is WebServiceException) {
                    if (((WebServiceException)ex).ErrorCode ==ErrorCode.CreateOrder_RuleDisable.ToString ()) {
                        message = ((WebServiceException)ex).ErrorMessage;
                    } else {
                        var messageKey = "ServiceError" + ((WebServiceException)ex).ErrorCode;
                        var errorMessage = appResource.GetString (messageKey);
                        if(errorMessage != messageKey)
                        {
                            message = errorMessage;
                        }
                    }
                }
            } catch {

            }


            var settings = TinyIoCContainer.Current.Resolve<IAppSettings> ();
            string err = string.Format (message, settings.ApplicationName, settings.PhoneNumberDisplay (order.Settings.ProviderId.HasValue ? order.Settings.ProviderId.Value : 0));

            TinyIoCContainer.Current.Resolve<IMessageService> ().ShowMessage (title, err, "Call", () => CallCompany (settings.ApplicationName, settings.PhoneNumber (order.Settings.ProviderId.HasValue ? order.Settings.ProviderId.Value : 0)), "Cancel", delegate {
            });
        }

        private void CallCompany (string name, string number)
        {
            TinyIoCContainer.Current.Resolve<IMvxPhoneCallTask> ().MakePhoneCall (name, number);
        }

        public OrderStatusDetail GetOrderStatus (Guid orderId)
        {
            var r = new OrderStatusDetail ();


            UseServiceClient<OrderServiceClient>(service =>
                {
                    r = service.GetOrderStatus(orderId);
                }, ex => Logger.LogError(ex));

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
                UseServiceClient<OrderServiceClient> (service =>
                {
                    result = service.GetOrderStatus (new Guid (lastOrderId));
                }, ex => Logger.LogError (ex));

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
            if (pickup.HasValidCoordinate() && destination.HasValidCoordinate())
            {

                var directionInfo = TinyIoCContainer.Current.Resolve<IGeolocService> ().GetDirectionInfo (pickup.Latitude, pickup.Longitude, destination.Latitude, destination.Longitude, pickupDate);

                return directionInfo ?? new DirectionInfo();
            }

            return new DirectionInfo();
        }

        public string GetFareEstimateDisplay (CreateOrder order, string formatString, string defaultFare, bool includeDistance, string cannotGetFareText)
        {
            var appResource = TinyIoCContainer.Current.Resolve<IAppResource> ();
            var fareEstimate = appResource.GetString (defaultFare);

            if (order != null && order.PickupAddress.HasValidCoordinate() && order.DropOffAddress.HasValidCoordinate())
            {
                var estimatedFare = GetFareEstimate(order.PickupAddress, order.DropOffAddress, order.PickupDate);

                if (estimatedFare.Price.HasValue)
                {
                    var maxEstimate = Config.GetSetting<double>("Client.MaxFareEstimate", 100);
                    if (formatString.HasValue() || (estimatedFare.Price.Value > maxEstimate && appResource.GetString("EstimatePriceOver100").HasValue()))
                    {
                        fareEstimate = String.Format(appResource.GetString(estimatedFare.Price.Value > maxEstimate 
                                                                           ? "EstimatePriceOver100"
                                                                           : formatString), 
                                                 estimatedFare.FormattedPrice);
                    }
                    else
                    {
                        fareEstimate = estimatedFare.FormattedPrice;
                    }

                    if (includeDistance && estimatedFare.Distance.HasValue)
                    {
                        var destinationString = " " + String.Format(appResource.GetString("EstimateDistance"), estimatedFare.FormattedDistance);
                        if (!string.IsNullOrWhiteSpace(destinationString))
                        {
                            fareEstimate += destinationString;
                        }
                    }
                }
                else
                {
                    fareEstimate = String.Format(appResource.GetString(cannotGetFareText));
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
            var ratingType = new List<RatingType> ();
            UseServiceClient<OrderServiceClient> (service =>
            {
                ratingType = service.GetRatingTypes ();
            });
            return ratingType;
        }

        public apcurium.MK.Common.Entity.OrderRatings GetOrderRating (Guid orderId)
        {
            var orderRate = new OrderRatings ();
            UseServiceClient<OrderServiceClient> (service =>
            {
                orderRate = service.GetOrderRatings (orderId);
            });
            return orderRate;
        }

        public void SendRatingReview (Common.Entity.OrderRatings orderRatings)
        {
            var request = new OrderRatingsRequest () { Note = orderRatings.Note, OrderId = orderRatings.OrderId, RatingScores = orderRatings.RatingScores };
            UseServiceClient<OrderServiceClient> (service => service.RateOrder (request));
        }

    }
}

