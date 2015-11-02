using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
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
using OrderRatings = apcurium.MK.Common.Entity.OrderRatings;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Common.Resources;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class BookingService : BaseService, IBookingService
    {
        private readonly IAccountService _accountService;
        private readonly ILocalization _localize;
        private readonly IAppSettings _appSettings;
        private readonly IGeolocService _geolocService;
        private readonly IMessageService _messageService;

        public BookingService(IAccountService accountService,
            ILocalization localize,
            IAppSettings appSettings,
            IGeolocService geolocService,
            IMessageService messageService)
        {
            _geolocService = geolocService;
            _messageService = messageService;
            _appSettings = appSettings;
            _localize = localize;
            _accountService = accountService;
        }

        public Task<OrderValidationResult> ValidateOrder(CreateOrder order)
        {
            return Mvx.Resolve<OrderServiceClient>().ValidateOrder(order);
        }

        public async Task<bool> IsPaired(Guid orderId)
        {
            var isPairedResult = await UseServiceClientAsync<OrderServiceClient, OrderPairingDetail>(service => service.GetOrderPairing(orderId));
            return isPairedResult != null;
        }

        public async Task<OrderStatusDetail> CreateOrder(CreateOrder order)
        {
            order.ClientLanguageCode = _localize.CurrentLanguage;

            Logger.LogMessage("Starting: *************************************   UseServiceClient : CreateOrder ID : {0}", order.Id);

            var orderDetail = await UseServiceClientAsync<OrderServiceClient, OrderStatusDetail>(service => service.CreateOrder(order));

            if (!order.PickupDate.HasValue) // Check if this is a scheduled ride
            {
                UserCache.Set("LastOrderId", orderDetail.OrderId.ToString()); // Need to be cached as a string because of a jit error on device
            }

            Task.Run(() => _accountService.RefreshCache(true)).FireAndForget();

            return orderDetail;
        }

        public async Task<OrderStatusDetail> SwitchOrderToNextDispatchCompany(Guid orderId, string nextDispatchCompanyKey, string nextDispatchCompanyName)
        {
            return await UseServiceClientAsync<OrderServiceClient, OrderStatusDetail>(service =>
                service.SwitchOrderToNextDispatchCompany(
                    new SwitchOrderToNextDispatchCompanyRequest
                    {
                        OrderId = orderId,
                        NextDispatchCompanyKey = nextDispatchCompanyKey,
                        NextDispatchCompanyName = nextDispatchCompanyName
                    }));
        }

        public async Task IgnoreDispatchCompanySwitch(Guid orderId)
        {
            await UseServiceClientAsync<OrderServiceClient>(service => service.IgnoreDispatchCompanySwitch(orderId));
        }

        public Task<OrderStatusDetail> GetOrderStatusAsync(Guid orderId)
        {
            return UseServiceClientAsync<OrderServiceClient, OrderStatusDetail>(service => service.GetOrderStatus(orderId));
        }

        public bool HasLastOrder
        {
            get { return UserCache.Get<string>("LastOrderId").HasValue(); }
        }

        public bool HasUnratedLastOrder
        {
            get { return UserCache.Get<string>("LastUnratedOrderId").HasValue(); }
        }

        public Task<OrderStatusDetail> GetLastOrderStatus()
        {
            try
            {
                if (!HasLastOrder)
                {
                    throw new InvalidOperationException();
                }
                var lastOrderId = UserCache.Get<string>("LastOrderId");  // Need to be cached as a string because of a jit error on device
                return UseServiceClientAsync<OrderServiceClient, OrderStatusDetail>(service => service.GetOrderStatus(new Guid(lastOrderId)));
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }
        }

        public Guid GetUnratedLastOrder()
        {
            try
            {
                if (!HasUnratedLastOrder)
                {
                    throw new InvalidOperationException();
                }
                var unratedLastOrderId = UserCache.Get<string>("LastUnratedOrderId");
                return Guid.Parse(unratedLastOrderId);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }
        }

        public void SetLastUnratedOrderId(Guid orderId)
        {
            UserCache.Set("LastUnratedOrderId", orderId.ToString()); // Need to be cached as a string because of a jit error on device
        }

        public void ClearLastOrder()
        {
            UserCache.Set("LastOrderId", (string)null); // Need to be cached as a string because of a jit error on device
        }

        public void ClearLastUnratedOrder()
        {
            UserCache.Set("LastUnratedOrderId", (string)null); // Need to be cached as a string because of a jit error on device
        }

        public Task RemoveFromHistory(Guid orderId)
        {
            return UseServiceClientAsync<OrderServiceClient>(service => service.RemoveFromHistory(orderId));
        }

        public bool IsStatusTimedOut(string statusId)
        {
            return statusId != null && statusId.SoftEqual(VehicleStatuses.Common.Timeout);
        }

        public bool IsStatusCompleted(OrderStatusDetail status)
        {
            if (status.IsManualRideLinq)
            {
                return status.Status == OrderStatus.Completed ||
                    status.Status == OrderStatus.Canceled;
            }

            return status.IBSStatusId.IsNullOrEmpty() ||
                status.IBSStatusId.SoftEqual(VehicleStatuses.Common.Cancelled) ||
                status.IBSStatusId.SoftEqual(VehicleStatuses.Common.Done) ||
                status.IBSStatusId.SoftEqual(VehicleStatuses.Common.NoShow) ||
                status.IBSStatusId.SoftEqual(VehicleStatuses.Common.CancelledDone) ||
                status.IBSStatusId.SoftEqual(VehicleStatuses.Common.MeterOffNotPayed) ||
                (status.IBSStatusId.SoftEqual(VehicleStatuses.Unknown.None)
                    && status.Status == OrderStatus.Canceled);
        }

        public bool IsOrderCancellable(OrderStatusDetail status)
        {
            return status.IBSStatusId == VehicleStatuses.Common.Assigned
                || status.IBSStatusId == VehicleStatuses.Common.Waiting
                || status.IBSStatusId == VehicleStatuses.Common.Arrived
                || status.IBSStatusId == VehicleStatuses.Common.Scheduled
                || string.IsNullOrEmpty(status.IBSStatusId) && status.IBSOrderId.HasValue;
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
            return statusId.SoftEqual(VehicleStatuses.Common.Arrived);
        }

        public bool IsStatusDone(string statusId)
        {
            return statusId.SoftEqual(VehicleStatuses.Common.Done) ||
                statusId.SoftEqual(VehicleStatuses.Common.MeterOffNotPayed);
        }

        public async Task<DirectionInfo> GetFareEstimate(CreateOrder order)
        {
            var tarifMode = _appSettings.Data.Direction.TarifMode;

            var validationResult = await UseServiceClientAsync<OrderServiceClient, OrderValidationResult>(service => service.ValidateOrder(order, null, true));

            if (order.PickupAddress.HasValidCoordinate()
                && order.DropOffAddress.HasValidCoordinate())
            {
                DirectionInfo directionInfo = null;
                if (tarifMode != TarifMode.AppTarif)
                {
                    int? duration;

                    duration =
                        (await
                            _geolocService.GetDirectionInfo(order.PickupAddress.Latitude, order.PickupAddress.Longitude,
                                order.DropOffAddress.Latitude, order.DropOffAddress.Longitude, order.Settings.VehicleTypeId,
                                order.PickupDate)).TripDurationInSeconds;

                    directionInfo =
                        await
                            UseServiceClientAsync<IIbsFareClient, DirectionInfo>(
                                service =>
                                    service.GetDirectionInfoFromIbs(order.PickupAddress.Latitude, order.PickupAddress.Longitude,
                                        order.DropOffAddress.Latitude, order.DropOffAddress.Longitude,
                                        order.PickupAddress.ZipCode, order.DropOffAddress.ZipCode,
								        order.Settings.AccountNumber, duration, order.Settings.VehicleTypeId, order.Settings.ServiceType));
                }

                if (tarifMode == TarifMode.AppTarif || (tarifMode == TarifMode.Both && directionInfo != null && directionInfo.Price == 0d))
                {
                    directionInfo = await _geolocService.GetDirectionInfo(order.PickupAddress.Latitude, order.PickupAddress.Longitude, order.DropOffAddress.Latitude, order.DropOffAddress.Longitude, order.Settings.VehicleTypeId, order.PickupDate);
                }

                directionInfo = directionInfo ?? new DirectionInfo();
                directionInfo.ValidationResult = validationResult;

                return directionInfo;
            }

            return new DirectionInfo() { ValidationResult = validationResult };
        }

        public string GetFareEstimateDisplay(DirectionInfo direction)
        {
            var fareEstimate = _localize[_appSettings.Data.DestinationIsRequired
                ? "NoFareTextIfDestinationIsRequired"
                : "NoFareText"];

            if (direction.ValidationResult != null
                && direction.ValidationResult.HasError)
            {
                fareEstimate = direction.ValidationResult.Message;

            }
            else if (direction.Distance.HasValue)
            {
                var willShowFare = direction.Price.HasValue && direction.Price.Value > 0;
                if (willShowFare)
                {
                    var isOverMaxFare = direction.Price.Value > _appSettings.Data.MaxFareEstimate;

                    var formattedCurrency = CultureProvider.FormatCurrency(direction.Price.Value);

                    fareEstimate = String.Format(
                        CultureProvider.CultureInfo,
                        _localize[isOverMaxFare
                            ? "EstimatePriceOver100"
                            : "EstimatePriceFormat"],
                        formattedCurrency,
                        direction.FormattedDistance);
                }
                else
                {
                    fareEstimate = _localize["EstimatedFareNotAvailable"];
                }
            }

            return fareEstimate;
        }

        public async Task<bool> CancelOrder(Guid orderId)
        {
            var isCompleted = true;
            try
            {
                await UseServiceClientAsync<OrderServiceClient>(service => service.CancelOrder(orderId));
            }
            catch
            {
                isCompleted = false;
            }
            return isCompleted;
        }

        public async Task<bool> SendReceipt(Guid orderId)
        {
            var isCompleted = true;
            try
            {
                await UseServiceClientAsync<OrderServiceClient>(service => service.SendReceipt(orderId));
            }
            catch
            {
                isCompleted = false;
            }
            return isCompleted;
        }

        public Task<IEnumerable<RatingTypeWrapper>> GetRatingTypes()
        {
            return UseServiceClientAsync<OrderServiceClient, IEnumerable<RatingTypeWrapper>>(service => service.GetRatingTypes(_localize.CurrentLanguage));
        }

        public Task<OrderRatings> GetOrderRatingAsync(Guid orderId)
        {
            return UseServiceClientAsync<OrderServiceClient, OrderRatings>(service => service.GetOrderRatings(orderId));
        }

        public Task SendRatingReview(OrderRatings orderRatings)
        {
            ClearLastUnratedOrder();
            var request = new OrderRatingsRequest { Note = orderRatings.Note, OrderId = orderRatings.OrderId, RatingScores = orderRatings.RatingScores };
            return UseServiceClientAsync<OrderServiceClient>(service => service.RateOrder(request));
        }

        public async Task<OrderManualRideLinqDetail> PairWithManualRideLinq(string pairingCode, Address pickupAddress)
        {
            var request = new ManualRideLinqPairingRequest
            {
                PairingCode = pairingCode,
                PickupAddress = pickupAddress,
                ClientLanguageCode = _localize.CurrentLanguage,
            };
            try
            {

                var response = await UseServiceClientAsync<ManualPairingForRideLinqServiceClient, ManualRideLinqResponse>(
                    service => RunWithRetryAsync(() => service.Pair(request), TimeSpan.FromSeconds(10), IsExceptionStatusCodeBadRequest, 30));

                if (response.IsSuccessful)
                {
                    UserCache.Set("LastOrderId", response.Data.OrderId.ToString());

                    return response.Data;
                }

                throw new Exception(response.ErrorCode);
            }
            catch (AggregateException ex)
            {
                var badRequestException = ex.InnerExceptions
                    .Select(exception => exception as WebServiceException)
                    .Where(exception => exception != null)
                    .LastOrDefault(webException => webException.StatusCode == (int)HttpStatusCode.BadRequest);

                if (badRequestException != null)
                {
                    throw badRequestException;
                }

                _messageService.ShowMessage(_localize["ManualPairing_TimeOut_Title"], _localize["ManualPairing_TimeOut_Message"]).FireAndForget();

                throw new Exception();
            }
        }

        private bool IsExceptionStatusCodeBadRequest(Exception ex)
        {
            var webServiceException = ex as WebServiceException;

            if (webServiceException == null)
            {
                return false;
            }

            return webServiceException.StatusCode == (int)HttpStatusCode.BadRequest;

        }

        public Task UnpairFromManualRideLinq(Guid orderId)
        {
            return UseServiceClientAsync<ManualPairingForRideLinqServiceClient>(service => service.Unpair(orderId));
        }

        public async Task<bool> UpdateAutoTipForManualRideLinq(Guid orderId, int autoTipPercentage)
        {
            var response = await UseServiceClientAsync<ManualPairingForRideLinqServiceClient, ManualRideLinqResponse>(service =>
                service.UpdateAutoTip(orderId, autoTipPercentage));

            return response.IsSuccessful;
        }

        public Task<ManualRideLinqResponse> GetTripInfoFromManualRideLinq(Guid orderId)
        {
            return UseServiceClientAsync<ManualPairingForRideLinqServiceClient, ManualRideLinqResponse>(service => service.GetUpdatedTrip(orderId));
        }

        public Task<bool> InitiateCallToDriver(Guid orderId)
        {
            try
            {
                return Mvx.Resolve<OrderServiceClient>().InitiateCallToDriver(orderId);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }
    }
}