using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Data;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Helpers;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Resources;
using CMTPayment;
using CMTPayment.Pair;
using CMTServices;
using CustomerPortal.Client;
using CustomerPortal.Contract.Response;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Jobs
{
    public class OrderStatusUpdater
    {
        private const string FailedCode = "0";
        
        // maximum probable time between the moment when user changes payment type on his device and it's saving in the database on server, seconds
        private const int TimeBetweenPaymentChangeAndSaveInDb = 15;

        private readonly ICommandBus _commandBus;
        private readonly IServerSettings _serverSettings;
        private readonly IOrderPaymentDao _paymentDao;
        private readonly IOrderDao _orderDao;
        private readonly INotificationService _notificationService;
        private readonly IDirections _directions;
        private readonly IAccountDao _accountDao;
        private readonly IIbsOrderService _ibs;
        private readonly IPromotionDao _promotionDao;
        private readonly IEventSourcedRepository<Promotion> _promoRepository;
        private readonly IPaymentService _paymentService;
        private readonly ICreditCardDao _creditCardDao;
        private readonly IFeeService _feeService;
        private readonly IOrderNotificationsDetailDao _orderNotificationsDetailDao;
        private readonly CmtGeoServiceClient _cmtGeoServiceClient;
        private readonly IDispatcherService _dispatcherService;
        private readonly IVehicleTypeDao _vehicleTypeDao;
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly ITaxiHailNetworkServiceClient _taxiHailNetworkServiceClient;
        private readonly ILogger _logger;
        private readonly Resources.Resources _resources;

        private CmtTripInfoServiceHelper _cmtTripInfoServiceHelper;
        private TaxiHailNetworkHelper _taxiHailNetworkHelper;

        private string _languageCode = string.Empty;
        

        public OrderStatusUpdater(IServerSettings serverSettings, 
            ICommandBus commandBus, 
            IOrderPaymentDao paymentDao, 
            IOrderDao orderDao,
            INotificationService notificationService,
            IDirections directions,
            IAccountDao accountDao,
            IIbsOrderService ibs,
            IPromotionDao promotionDao,
            IEventSourcedRepository<Promotion> promoRepository,
            IPaymentService paymentService,
            ICreditCardDao creditCardDao,
            IFeeService feeService,
            IOrderNotificationsDetailDao orderNotificationsDetailDao,
            CmtGeoServiceClient cmtGeoServiceClient,
            IDispatcherService dispatcherService,
            IVehicleTypeDao vehicleTypeDao,
            IIBSServiceProvider ibsServiceProvider,
            ITaxiHailNetworkServiceClient taxiHailNetworkServiceClient,
            ILogger logger)
        {
            _orderDao = orderDao;
            _notificationService = notificationService;
            _directions = directions;
            _serverSettings = serverSettings;
            _accountDao = accountDao;
            _ibs = ibs;
            _promotionDao = promotionDao;
            _promoRepository = promoRepository;
            _paymentService = paymentService;
            _creditCardDao = creditCardDao;
            _feeService = feeService;
            _logger = logger;
            _commandBus = commandBus;
            _paymentDao = paymentDao;
            _orderNotificationsDetailDao = orderNotificationsDetailDao;
            _cmtGeoServiceClient = cmtGeoServiceClient;
            _dispatcherService = dispatcherService;
            _vehicleTypeDao = vehicleTypeDao;
            _ibsServiceProvider = ibsServiceProvider;
            _taxiHailNetworkServiceClient = taxiHailNetworkServiceClient;

            _resources = new Resources.Resources(serverSettings);
            _taxiHailNetworkHelper = new TaxiHailNetworkHelper(accountDao, _ibsServiceProvider, _serverSettings, taxiHailNetworkServiceClient, _commandBus, _logger);
        }

        public virtual void Update(IBSOrderInformation orderFromIbs, OrderStatusDetail orderStatusDetail)
        {
            var paymentSettings = _serverSettings.GetPaymentSettings(orderStatusDetail.CompanyKey);
            var orderDetail = _orderDao.FindById(orderStatusDetail.OrderId);
            
            UpdateVehiclePositionAndSendNearbyNotificationIfNecessary(orderFromIbs, orderStatusDetail, orderDetail);

            SendUnpairWarningNotificationIfNecessary(orderStatusDetail, paymentSettings);

            if (orderFromIbs.IsUnloaded)
            {
                if (orderStatusDetail.ChargeAmountsTimeOut.HasValue
                    && orderStatusDetail.ChargeAmountsTimeOut.Value < DateTime.UtcNow)
                {
                    orderFromIbs.Status = VehicleStatuses.Common.Done;
                }
            }

            if (orderFromIbs.IsLoaded)
            {
                SendChargeTypeMessageToDriver(orderStatusDetail, paymentSettings, orderDetail);
            }

            if (orderFromIbs.IsWaitingToBeAssigned)
            {
                CheckForOrderTimeOut(orderStatusDetail);
            }

            var trip = CheckForRideLinqCmtPairingErrors(orderStatusDetail, paymentSettings);

            if (!OrderNeedsUpdate(orderFromIbs, orderStatusDetail))
            {
                _logger.LogMessage("Skipping order update (Id: {0})", orderStatusDetail.OrderId);
                return;
            }

            _logger.LogMessage("Running order update (Id: {0})", orderStatusDetail.OrderId);

            var hasDriverBailed = HasDriverBailed(orderStatusDetail, orderFromIbs);

            PopulateFromIbsOrder(orderStatusDetail, orderFromIbs, orderDetail, hasDriverBailed);

            CheckForPairingAndHandleIfNecessary(orderStatusDetail, orderFromIbs, paymentSettings, orderDetail, trip);

            _commandBus.Send(new ChangeOrderStatus
            {
                Status = orderStatusDetail,
                Fare = orderFromIbs.Fare,
                Toll = orderFromIbs.Toll,
                Tip = orderFromIbs.Tip,
                Tax = orderFromIbs.VAT,
                Surcharge = orderFromIbs.Surcharge
            });

            if (hasDriverBailed)
            {
                Task.Run(() =>
                {
                    var ibsOrderResult = DispatchAgainIfDriverBailed(orderStatusDetail.OrderId, orderStatusDetail.Market, orderStatusDetail.DriverInfos.VehicleRegistration);

                    if (ibsOrderResult != null)
                    {
                        var orderBailedHelper = new OrderBailedCreationHelper(_commandBus, _serverSettings, _logger);
                        orderBailedHelper.SendOrderCreationCommands(
                            ibsOrderResult.OrderKey.TaxiHailOrderId,
                            ibsOrderResult.OrderKey.IbsOrderId,
                            ibsOrderResult.DispatcherTimedOut,
                            ibsOrderResult.CompanyKey,
                            orderDetail.ClientLanguageCode);
                    }
                });
            }
        }

        private void SendChargeTypeMessageToDriver(OrderStatusDetail orderStatusDetail, ServerPaymentSettings paymentSettings, OrderDetail orderDetail)
        {
            if (orderStatusDetail.IsPrepaid
                || orderDetail.Settings.ChargeTypeId == ChargeTypes.PaymentInCar.Id)
            {
                return;
            }

            var marketSettings = _taxiHailNetworkServiceClient.GetCompanyMarketSettings(orderDetail.PickupAddress.Latitude, orderDetail.PickupAddress.Longitude);
            
            if (orderStatusDetail.UnpairingTimeOut != null && !marketSettings.DisableOutOfAppPayment && orderStatusDetail.UnpairingTimeOut.Value != DateTime.MaxValue)
            {
                if (DateTime.UtcNow >= orderStatusDetail.UnpairingTimeOut.Value.AddSeconds(TimeBetweenPaymentChangeAndSaveInDb))
                {
                    var orderNotification = _orderNotificationsDetailDao.FindByOrderId(orderStatusDetail.OrderId);

                    if (orderNotification == null || !orderNotification.InfoAboutPaymentWasSentToDriver)
                    {
						_ibs.SendMessageToDriver(_resources.Get("PairingConfirmationToDriver"), orderStatusDetail.VehicleNumber, orderDetail.CompanyKey);

                        _commandBus.Send(new UpdateOrderNotificationDetail
                        {
                            OrderId = orderStatusDetail.OrderId,
                            InfoAboutPaymentWasSentToDriver = true
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Return value is only set if we succesfully got a trip from RideLinq
        /// </summary>
        /// <param name="orderStatusDetail"></param>
        /// <param name="paymentSettings"></param>
        /// <returns></returns>
        private Trip CheckForRideLinqCmtPairingErrors(OrderStatusDetail orderStatusDetail, ServerPaymentSettings paymentSettings)
        {
            var paymentMode = paymentSettings.PaymentMode;
            if (paymentMode != PaymentMethod.RideLinqCmt)
            {
                // Only for CMT RideLinQ
                return null;
            }

            var pairingInfo = _orderDao.FindOrderPairingById(orderStatusDetail.OrderId);
            if (pairingInfo == null)
            {
                // Order not paired
                return null;
            }

            InitializeCmtServiceClient(paymentSettings);

            var tripInfo = _cmtTripInfoServiceHelper.GetTripInfo(pairingInfo.PairingToken);
            if (tripInfo != null
                && (tripInfo.ErrorCode == CmtErrorCodes.UnableToPair
                    || tripInfo.ErrorCode == CmtErrorCodes.TripUnpaired))
            {
                orderStatusDetail.IBSStatusDescription = _resources.Get("OrderStatus_PairingFailed", _languageCode);
                orderStatusDetail.PairingError = string.Format("CMT Pairing Error Code: {0}", tripInfo.ErrorCode);
            }

            return tripInfo;
        }

        public virtual void HandleManualRidelinqFlow(OrderStatusDetail orderStatusDetail)
        {
            if (orderStatusDetail.Status == OrderStatus.WaitingForPayment && orderStatusDetail.LastTripPollingDateInUtc.HasValue)
            {
                var nextPolling = orderStatusDetail.LastTripPollingDateInUtc.Value.AddHours(4);
                if (nextPolling >= DateTime.UtcNow)
                {
                    return;
                }
            }

            _logger.LogMessage("Starting OrderStatusUpdater for order {0} (Paired via Manual RideLinQ code).", orderStatusDetail.OrderId);

            var rideLinqDetails = _orderDao.GetManualRideLinqById(orderStatusDetail.OrderId);
            if (rideLinqDetails == null)
            {
                _logger.LogMessage("No manual RideLinq details found for order {0}", orderStatusDetail.OrderId);
                return;
            }

            if (rideLinqDetails.EndTime.HasValue || rideLinqDetails.PairingError.HasValueTrimmed())
            {
                // Trip ended. Nothing do to has end of trip errors are handled by the event handler.
                return;
            }

            _logger.LogMessage("Initializing CmdClient for order {0} (RideLinq Pairing Token: {1})", orderStatusDetail.OrderId, rideLinqDetails.PairingToken);

            var paymentSettings = _serverSettings.GetPaymentSettings(orderStatusDetail.CompanyKey);
            InitializeCmtServiceClient(paymentSettings);

            var tripInfo = _cmtTripInfoServiceHelper.GetTripInfo(rideLinqDetails.PairingToken);

            if (tripInfo == null)
            {
                var errorMessage = string.Format("No Trip information found for order {0} (pairing token {1})", orderStatusDetail.OrderId, rideLinqDetails.PairingToken);
                _logger.LogMessage(errorMessage);

                // Unpair and mark order as cancelled
                _commandBus.Send(new UnpairOrderForManualRideLinq
                {
                    OrderId = rideLinqDetails.OrderId
                });

                return;
            }

            string pairingError = null;
            if (tripInfo.ErrorCode.HasValue && tripInfo.ErrorCode.Value != CmtErrorCodes.TripNotFound)
            {
                pairingError = tripInfo.ErrorCode.ToString();
            }

            if (tripInfo.EndTime.HasValue || pairingError.HasValueTrimmed())
            {
                _logger.LogMessage("Trip ended for trip id: {0} (order {1})", tripInfo.TripId, orderStatusDetail.OrderId);

                _commandBus.Send(new UpdateTripInfoInOrderForManualRideLinq
                {
                    StartTime = tripInfo.StartTime,
                    EndTime = tripInfo.EndTime,
                    Distance = tripInfo.Distance,
                    Extra = Math.Round(((double)tripInfo.Extra / 100), 2),
                    Fare = Math.Round(((double)tripInfo.Fare / 100), 2),
                    Tax = Math.Round(((double)tripInfo.Tax / 100), 2),
                    Tip = Math.Round(((double)tripInfo.Tip / 100), 2),
                    TollTotal = tripInfo.TollHistory.SelectOrDefault(tollHistory => tollHistory.Sum(toll => Math.Round(((double)toll.TollAmount / 100), 2))),
                    Surcharge = Math.Round(((double)tripInfo.Surcharge / 100), 2),
                    Total = Math.Round(((double)tripInfo.Total / 100), 2),
                    FareAtAlternateRate = Math.Round(((double)tripInfo.FareAtAlternateRate / 100), 2),
                    RateAtTripStart = tripInfo.RateAtTripStart,
                    RateAtTripEnd = tripInfo.RateAtTripEnd,
                    RateChangeTime = tripInfo.RateChangeTime,
                    OrderId = orderStatusDetail.OrderId,
                    PairingToken = tripInfo.PairingToken,
                    TripId = tripInfo.TripId,
                    DriverId = tripInfo.DriverId,
                    AccessFee = Math.Round(((double)tripInfo.AccessFee / 100), 2),
                    LastFour = tripInfo.LastFour,
                    Tolls = tripInfo.TollHistory.SelectOrDefault(tollHistory => tollHistory.ToArray(), new TollDetail[0]),
                    LastLatitudeOfVehicle = tripInfo.Lat,
                    LastLongitudeOfVehicle = tripInfo.Lon,
                    PairingError = pairingError
                });                
                return;
            }

            if (orderStatusDetail.Status == OrderStatus.Created
                && rideLinqDetails.PairingDate.AddHours(2) <= DateTime.Now)
            {
                _logger.LogMessage("Trip has been active for 2 hours, change it's status to waiting for payment to trigger a trip end to the client [tripId: {0} orderId: {1} pairingDate (server local time): {2}]", 
                    tripInfo.TripId, orderStatusDetail.OrderId, rideLinqDetails.PairingDate.ToLongDateString());

                _commandBus.Send(new ChangeOrderStatusForManualRideLinq
                {
                    OrderId = orderStatusDetail.OrderId,
                    Status = OrderStatus.WaitingForPayment,
                    LastTripPollingDateInUtc = DateTime.UtcNow
                });

                return;
            }

            if (orderStatusDetail.Status == OrderStatus.WaitingForPayment)
            {
                var orderShouldBeSetToTimedOut = rideLinqDetails.PairingDate.AddDays(30) <= DateTime.Now;

                if (orderShouldBeSetToTimedOut)
                {
                    _logger.LogMessage("Trip for order {0} has timed out after 30 days of waiting for EndTime", orderStatusDetail.OrderId);
                }
                else
                {
                    _logger.LogMessage("Trip for order {0} is still WaitingForPayment... Will stop polling at {1} (server local time)", orderStatusDetail.OrderId, rideLinqDetails.PairingDate.AddDays(30).ToLongDateString());
                }

                _commandBus.Send(new ChangeOrderStatusForManualRideLinq
                {
                    OrderId = orderStatusDetail.OrderId,
                    Status = orderShouldBeSetToTimedOut 
                        ? OrderStatus.TimedOut 
                        : OrderStatus.WaitingForPayment,
                    LastTripPollingDateInUtc = DateTime.UtcNow
                });
                return;
            }

            _logger.LogMessage("Trip for order {0} is in progress. Nothing to update.", orderStatusDetail.OrderId);
        }

        private bool HasDriverBailed(OrderStatusDetail orderStatusDetail, IBSOrderInformation ibsOrderInfo)
        {
            // Detect if a bail occurred
            return orderStatusDetail.IBSStatusId == VehicleStatuses.Common.Assigned
                && ibsOrderInfo.IsWaitingToBeAssigned;
        }

        private void PopulateFromIbsOrder(OrderStatusDetail orderStatusDetail, IBSOrderInformation ibsOrderInfo, OrderDetail orderDetail, bool hasDriverBailed)
        {
            if (orderStatusDetail.IBSStatusId == VehicleStatuses.Common.Bailed
                && ibsOrderInfo.IsCanceled)
            {
                // Do nothing, yo
                return;
            }

            var ibsStatusId = orderStatusDetail.IBSStatusId;

            orderStatusDetail.IBSStatusId =                     hasDriverBailed ? VehicleStatuses.Common.Bailed : ibsOrderInfo.Status;
            orderStatusDetail.DriverInfos.FirstName =           ibsOrderInfo.FirstName.GetValue(orderStatusDetail.DriverInfos.FirstName);
            orderStatusDetail.DriverInfos.LastName =            ibsOrderInfo.LastName.GetValue(orderStatusDetail.DriverInfos.LastName);
            orderStatusDetail.DriverInfos.MobilePhone =         ibsOrderInfo.MobilePhone.GetValue(orderStatusDetail.DriverInfos.MobilePhone);
            orderStatusDetail.DriverInfos.VehicleColor =        ibsOrderInfo.VehicleColor.GetValue(orderStatusDetail.DriverInfos.VehicleColor);
            orderStatusDetail.DriverInfos.VehicleMake =         ibsOrderInfo.VehicleMake.GetValue(orderStatusDetail.DriverInfos.VehicleMake);
            orderStatusDetail.DriverInfos.VehicleModel =        ibsOrderInfo.VehicleModel.GetValue(orderStatusDetail.DriverInfos.VehicleModel);
            orderStatusDetail.DriverInfos.VehicleRegistration = GetVehicleRegistration(ibsOrderInfo, orderStatusDetail);
            orderStatusDetail.DriverInfos.VehicleType =         ibsOrderInfo.VehicleType.GetValue(orderStatusDetail.DriverInfos.VehicleType);
            orderStatusDetail.DriverInfos.DriverId =            ibsOrderInfo.DriverId.GetValue(orderStatusDetail.DriverInfos.DriverId);
            orderStatusDetail.VehicleNumber =                   ibsOrderInfo.VehicleNumber.GetValue(orderStatusDetail.VehicleNumber);
            orderStatusDetail.TerminalId =                      ibsOrderInfo.TerminalId.GetValue(orderStatusDetail.TerminalId);
            orderStatusDetail.ReferenceNumber =                 ibsOrderInfo.ReferenceNumber.GetValue(orderStatusDetail.ReferenceNumber);
            orderStatusDetail.Eta =                             ibsOrderInfo.Eta ?? orderStatusDetail.Eta;
            orderStatusDetail.RideLinqPairingCode =             ibsOrderInfo.PairingCode.GetValue(orderStatusDetail.RideLinqPairingCode);
            orderStatusDetail.DriverInfos.DriverPhotoUrl =      ibsOrderInfo.DriverPhotoUrl.GetValue(orderStatusDetail.DriverInfos.DriverPhotoUrl);

            UpdateStatusIfNecessary(orderStatusDetail, ibsOrderInfo, hasDriverBailed);

            var wasProcessingOrderOrWaitingForDriver = ibsStatusId == null || ibsStatusId.SoftEqual(VehicleStatuses.Common.Waiting);

            // In the case of Driver ETA Notification mode is Once, this next value will indicate if we should send the notification or not.
            orderStatusDetail.IBSStatusDescription = GetDescription(orderStatusDetail.OrderId, ibsOrderInfo, orderStatusDetail.CompanyName, wasProcessingOrderOrWaitingForDriver && ibsOrderInfo.IsAssigned, hasDriverBailed, orderDetail);
        }

        private IBSOrderResult DispatchAgainIfDriverBailed(Guid orderId, string market, string driverIdWhoBailed)
        {
            var orderDetail = _orderDao.FindById(orderId);

            var dispatcherSettings = _dispatcherService.GetSettings(market, orderDetail.PickupAddress.Latitude, orderDetail.PickupAddress.Longitude);

            if (dispatcherSettings.NumberOfOffersPerCycle <= 0)
            {
                return null;
            }
            
            // Prepare order for re-dispatch
            var ibsAccountIdForOrderToCancel = _accountDao.GetIbsAccountId(orderDetail.AccountId, orderDetail.CompanyKey);
            if (ibsAccountIdForOrderToCancel.HasValue)
            {
                _dispatcherService.CancelIbsOrder(orderDetail.IBSOrderId, orderDetail.CompanyKey, orderDetail.Settings.Phone, ibsAccountIdForOrderToCancel.Value);

                var defaultVehicleType = _vehicleTypeDao.GetAll().FirstOrDefault();

                var chargeTypeKey = ChargeTypes.GetList()
                    .Where(x => x.Id == orderDetail.Settings.ChargeTypeId)
                    .Select(x => x.Display)
                    .FirstOrDefault();

                var driverIdsToExclude = new List<string> {driverIdWhoBailed};

                // Determine best available company now based on available vehicles
                var bestAvailableCompany = _taxiHailNetworkHelper.FindBestAvailableCompany(new CompanyMarketSettingsResponse { Market = market }, orderDetail.PickupAddress.Latitude, orderDetail.PickupAddress.Longitude, false, driverIdsToExclude);
                
                var referenceDataCompanyList = _ibsServiceProvider.StaticData(bestAvailableCompany.CompanyKey).GetCompaniesList();

                var ibsOrderParams = IbsHelper.PrepareForIbsOrder(
                    _serverSettings.ServerData.IBS, defaultVehicleType,
                    orderDetail.Settings.ChargeTypeId, orderDetail.PickupAddress,
                    orderDetail.DropOffAddress, orderDetail.Settings.AccountNumber,
                    orderDetail.Settings.CustomerNumber, referenceDataCompanyList,
                    orderDetail.Market, orderDetail.Settings.ProviderId, orderDetail.CompanyKey);

                _logger.LogMessage("Driver {0} bailed, trying to dispatch again to new found best avail company: {1} (fleetid: {2})", driverIdWhoBailed, bestAvailableCompany.CompanyKey, bestAvailableCompany.FleetId);

                var accountDetail = _accountDao.FindById(orderDetail.AccountId);
                var ibsAccountIdForBestAvailableCompany = _taxiHailNetworkHelper.CreateIbsAccountIfNeeded(accountDetail, bestAvailableCompany.CompanyKey);

                // Re-dispatch order (don't dispatch again to the driver who bailed)
                var ibsOrderResult =_dispatcherService.Dispatch(orderDetail.AccountId, orderDetail.Id,
                    ibsOrderParams,
                    bestAvailableCompany,
                    dispatcherSettings,
                    orderDetail.Settings.AccountNumber,
                    ibsAccountIdForBestAvailableCompany,
                    orderDetail.Settings.Name,
                    orderDetail.Settings.Phone,
                    orderDetail.Settings.Passengers,
                    orderDetail.Settings.VehicleTypeId,
                    IbsHelper.BuildNote(
                        _serverSettings.ServerData.IBS.NoteTemplate,
                        _resources.Get(chargeTypeKey, _serverSettings.ServerData.PriceFormat),
                        orderDetail.UserNote,
                        orderDetail.PickupAddress.BuildingName,
                        orderDetail.Settings.LargeBags,
                        _serverSettings.ServerData.IBS.HideChargeTypeInUserNote),
                    orderDetail.PickupDate,
                    null,
                    null,
                    orderDetail.Market,
                    FareHelper.GetFareFromEstimate(new RideEstimate { Price = orderDetail.EstimatedFare }),
                    orderDetail.TipIncentive,
                    driverIdsToExclude: driverIdsToExclude);

                return ibsOrderResult;
            }

            return null;
        }

        private void UpdateStatusIfNecessary(OrderStatusDetail orderStatusDetail, IBSOrderInformation ibsOrderInfo, bool driverHasBailed)
        {
            if (driverHasBailed 
                || orderStatusDetail.Status == OrderStatus.WaitingForPayment
                || (orderStatusDetail.Status == OrderStatus.TimedOut && ibsOrderInfo.IsWaitingToBeAssigned))
            {
                _logger.LogMessage("Order {0}: Status is: {1} Bail Detected: {2}. Don't update since it's a special case outside of IBS.", orderStatusDetail.OrderId, orderStatusDetail.Status, driverHasBailed);
                return;
            }

            if (orderStatusDetail.Status == OrderStatus.TimedOut && !ibsOrderInfo.IsWaitingToBeAssigned)
            {
                // Ride was assigned while waiting for user input on whether or not to switch company
                orderStatusDetail.Status = OrderStatus.Created;
            }

            if (ibsOrderInfo.IsAssigned && orderStatusDetail.TaxiAssignedDate == null)
            {
                orderStatusDetail.TaxiAssignedDate = DateTime.UtcNow;
            }

            if (ibsOrderInfo.IsCanceled)
            {
                orderStatusDetail.Status = OrderStatus.Canceled;

                try
                {
                    if (ibsOrderInfo.Status == VehicleStatuses.Common.NoShow)
                    {
                        // Charge No Show fees on company Null
                        var feeCharged = _feeService.ChargeNoShowFeeIfNecessary(orderStatusDetail);

                        if (orderStatusDetail.CompanyKey != null)
                        {
                            // Company not-null will never (so far) perceive no show fees, so we need to void its preauth
                            _paymentService.VoidPreAuthorization(orderStatusDetail.CompanyKey, orderStatusDetail.OrderId);
                        }
                        else
                        {
                            if (!feeCharged.HasValue)
                            {
                                // No fees were charged on company null, void the preauthorization to prevent misuse fees
                                _paymentService.VoidPreAuthorization(orderStatusDetail.CompanyKey, orderStatusDetail.OrderId);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    orderStatusDetail.PairingError = ex.Message;
                }

                _logger.LogMessage("Order {1}: Status updated to: {0}", orderStatusDetail.Status, orderStatusDetail.OrderId);
            }
            else if (ibsOrderInfo.IsTimedOut)
            {
                orderStatusDetail.Status = OrderStatus.TimedOut;
                _logger.LogMessage("Order {1}: Status updated to: {0}", orderStatusDetail.Status, orderStatusDetail.OrderId);
            }
            else if (ibsOrderInfo.IsComplete)
            {
                orderStatusDetail.Status = OrderStatus.Completed;
                _logger.LogMessage("Order {1}: Status updated to: {0}", orderStatusDetail.Status, orderStatusDetail.OrderId);
            }
        }

        private PreAuthorizePaymentResponse PreauthorizePaymentIfNecessary(OrderDetail orderDetail, string companyKey, Guid orderId, decimal amount, string cvv = null)
        {
            // Check payment instead of PreAuth setting, because we do not preauth in the cases of future bookings
            var paymentInfo = _paymentDao.FindByOrderId(orderId, companyKey);
            if (paymentInfo != null)
            {
                // Already preauthorized on create order, do nothing
                return new PreAuthorizePaymentResponse { IsSuccessful = true };
            }
            
            if (orderDetail == null)
            {
                return new PreAuthorizePaymentResponse
                {
                    IsSuccessful = false, 
                    Message = "Order not found"
                };
            }

            var account = _accountDao.FindById(orderDetail.AccountId);

            var result = _paymentService.PreAuthorize(orderDetail.CompanyKey, orderId, account, amount, cvv: cvv);
            if (result.IsSuccessful)
            {
                // Wait for OrderPaymentDetail to be created
                var paymentCreated = WaitForPaymentDetail(companyKey, orderId);
                if (!paymentCreated)
                {
                    _paymentService.VoidPreAuthorization(companyKey, orderId);

                    result.IsSuccessful = false;
                    result.Message = "OrderPaymentDetail entry failed to be created in time";
                }
            }
            else if (result.IsDeclined)
            {
                // Deactivate credit card if it was declined
                _commandBus.Send(new ReactToPaymentFailure
                {
                    AccountId = orderDetail.AccountId,
                    OrderId = orderId,
                    IBSOrderId = orderDetail.IBSOrderId,
                    CreditCardId = account.DefaultCreditCard.GetValueOrDefault(),
                    OverdueAmount = amount,
                    TransactionId = result.TransactionId,
                    TransactionDate = result.TransactionDate
                });
            }
            
            return result;
        }

        private bool WaitForPaymentDetail(string companyKey, Guid orderId)
        {
            const int checkInterval = 500; // in ms

            // 5 seconds loop in the worse case
            for (var i = 0; i < 10; i++)
            {
                Thread.Sleep(checkInterval);

                if (_paymentDao.FindByOrderId(orderId, companyKey) != null)
                {
                    return true;
                }
            }

            return false;
        }

        private string GetVehicleRegistration(IBSOrderInformation ibsOrderInfo, OrderStatusDetail orderStatusDetail)
        {
            var vehicleRegistration = orderStatusDetail.DriverInfos.VehicleRegistration;

            if (!vehicleRegistration.HasValue() || vehicleRegistration != ibsOrderInfo.VehicleRegistration)
            {
                var vehicleMapping = _orderDao.GetVehicleMapping(orderStatusDetail.OrderId);
                vehicleRegistration = vehicleMapping != null
                    ? vehicleMapping.DeviceName
                    : ibsOrderInfo.VehicleRegistration;
            }

            return vehicleRegistration;
        }

	private void UpdateVehiclePositionAndSendNearbyNotificationIfNecessary(IBSOrderInformation ibsOrderInfo, OrderStatusDetail orderStatus, OrderDetail orderDetail)
        {
		// We are not supposed to attempt to show the vehicle position when not in the proper state.
	        if (VehicleStatuses.ShowOnMapStatuses.None(status => status.Equals(ibsOrderInfo.Status)))
	        {
				return;
	        }

            // Use IBS vehicle position by default
            var vehicleLatitude = ibsOrderInfo.VehicleLatitude;
            var vehicleLongitude = ibsOrderInfo.VehicleLongitude;

            var isUsingGeo = (!orderStatus.Market.HasValue() && _serverSettings.ServerData.LocalAvailableVehiclesMode == LocalAvailableVehiclesModes.Geo)
                || (orderStatus.Market.HasValue() && _serverSettings.ServerData.ExternalAvailableVehiclesMode == ExternalAvailableVehiclesModes.Geo);

            var vehicleRegistration = GetVehicleRegistration(ibsOrderInfo, orderStatus);

            // Override with Geo position if enabled and if we have a vehicle registration.
            if (isUsingGeo && vehicleRegistration.HasValue())
            {
                var vehicleStatus = _cmtGeoServiceClient.GetEta(orderDetail.PickupAddress.Latitude, orderDetail.PickupAddress.Longitude, vehicleRegistration);

                if (vehicleStatus.Latitude != 0.0f && vehicleStatus.Longitude != 0.0f)
                {
                    vehicleLatitude = vehicleStatus.Latitude;
                    vehicleLongitude = vehicleStatus.Longitude;
                }
            }

            if (orderStatus.VehicleLatitude != vehicleLatitude || orderStatus.VehicleLongitude != vehicleLongitude) 
            {
                _orderDao.UpdateVehiclePosition(orderStatus.OrderId, vehicleLatitude, vehicleLongitude);
                _notificationService.SendTaxiNearbyPush(orderStatus.OrderId, ibsOrderInfo.Status, vehicleLatitude, vehicleLongitude);

                _logger.LogMessage("Vehicle position updated. New position: ({0}, {1}).", ibsOrderInfo.VehicleLatitude, ibsOrderInfo.VehicleLongitude);
            }
        }

        private void SendUnpairWarningNotificationIfNecessary(OrderStatusDetail orderStatus, ServerPaymentSettings paymentSettings)
        {
            if (!paymentSettings.IsUnpairingDisabled && orderStatus.UnpairingTimeOut.HasValue)
            {
                var halfwayUnpairTimeout = orderStatus.UnpairingTimeOut.Value.AddSeconds(-0.5 * paymentSettings.UnpairingTimeOut);

                if (DateTime.UtcNow >= halfwayUnpairTimeout)
                {
                    // Send unpair timeout reminder halfway through
                    _notificationService.SendUnpairingReminderPush(orderStatus.OrderId);
                }
            }
        }

        private void HandlePairingForRideLinqCmt(OrderStatusDetail orderStatusDetail, OrderPairingDetail pairingInfo, IBSOrderInformation ibsOrderInfo, ServerPaymentSettings paymentSettings, Trip trip)
        {
            HandleOrderCompletionWithNoFare(orderStatusDetail,
                () =>
                {
                    if (trip == null)
                    {
                        InitializeCmtServiceClient(paymentSettings);
                        trip = _cmtTripInfoServiceHelper.GetTripInfo(pairingInfo.PairingToken);
                    }
                    return !trip.EndTime.HasValue;
                },
                () => { },
                () =>
                {
                    // in the case of RideLinq CMT, we want to change the values of ibsOrderInfo since trip 
                    // could have more reliable info than ibs (sometimes ibs returns 0 but the trip has completed with full info)
                    if (trip == null)
                    {
                        InitializeCmtServiceClient(paymentSettings);
                        trip = _cmtTripInfoServiceHelper.GetTripInfo(pairingInfo.PairingToken);
                    }

                    // this check is only for consistency but it should not happen here since we already made sure that we have an endtime
                    if (trip != null && !trip.ErrorCode.HasValue && trip.EndTime.HasValue)
                    {
                        var tollHistory = trip.TollHistory != null
                            ? trip.TollHistory.Sum(p => p.TollAmount)
                            : 0;

                        ibsOrderInfo.Fare = Math.Round((trip.Fare + trip.FareAtAlternateRate) / 100d, 2);
                        ibsOrderInfo.Tip = Math.Round(trip.Tip / 100d, 2);
                        ibsOrderInfo.Toll = Math.Round(tollHistory / 100d, 2);
                        ibsOrderInfo.VAT = Math.Round(trip.Tax / 100d, 2);
                        ibsOrderInfo.Surcharge = Math.Round((trip.Surcharge + trip.Extra + trip.AccessFee) / 100d, 2);
                    }
                });
        }

        private bool HandleOrderCompletionWithNoFare(OrderStatusDetail orderStatusDetail, Func<bool> hasNoFareInfo, Action doOnTimeOut, Action doOnCompletionWithFare)
        {
            if (orderStatusDetail.Status == OrderStatus.Completed
                || orderStatusDetail.Status == OrderStatus.WaitingForPayment)
            {
                // status is now Completed or WaitingForPayment, check if we have received fare info
                if (hasNoFareInfo())
                {
                    // no fare info yet

                    if (orderStatusDetail.Status == OrderStatus.Completed)
                    {
                        // no fare received but order is completed, change status to increase polling speed and to trigger a completion on clientside
                        orderStatusDetail.Status = OrderStatus.WaitingForPayment;
                        orderStatusDetail.PairingTimeOut = DateTime.UtcNow.AddMinutes(30);
                        _logger.LogMessage("Order {1}: Status updated to: {0} with timeout in 30 minutes", orderStatusDetail.Status, orderStatusDetail.OrderId);
                        return true;
                    }

                    if (orderStatusDetail.Status == OrderStatus.WaitingForPayment
                        && DateTime.UtcNow > orderStatusDetail.PairingTimeOut)
                    {
                        // 30 minutes passed and still no fare info, just complete the ride
                        orderStatusDetail.Status = OrderStatus.Completed;
                        orderStatusDetail.PairingError = "Timed out period reached while waiting for payment informations from IBS.";
                        _logger.LogMessage("Order {1}: Pairing error: {0}", orderStatusDetail.PairingError, orderStatusDetail.OrderId);

                        doOnTimeOut();

                        return true;
                    }
                }
                else
                {
                    orderStatusDetail.Status = OrderStatus.Completed;
                    doOnCompletionWithFare();
                }
            }

            return false;
        }

        private void HandlePairingForStandardPairing(OrderStatusDetail orderStatusDetail, OrderPairingDetail pairingInfo, IBSOrderInformation ibsOrderInfo, OrderDetail orderDetail)
        {
            var orderPayment = _paymentDao.FindByOrderId(orderStatusDetail.OrderId, orderStatusDetail.CompanyKey);
            if (orderPayment != null && (orderPayment.IsCompleted || orderPayment.IsCancelled))
            {
                // Payment was already processed
                _logger.LogMessage("Payment for order {0} was already processed, nothing else to do.", orderStatusDetail.OrderId);
                return;
            }

            if (ibsOrderInfo.IsMeterOffNotPaid)
            {
                SendPaymentBeingProcessedMessageToDriver(ibsOrderInfo.VehicleNumber, orderStatusDetail.CompanyKey);
            }

            if (HandleOrderCompletionWithNoFare(orderStatusDetail, 
                () => ibsOrderInfo.Fare <= 0, 
                () => _paymentService.VoidPreAuthorization(orderStatusDetail.CompanyKey, orderStatusDetail.OrderId),
                () => { }))
            {
                return;
            }

            if (ibsOrderInfo.IsUnloaded)
            {
                return;
            }

            // We received a fare from IBS
            // Send payment for capture, once it's captured, we will set the status to Completed
            double tipPercentage = pairingInfo.AutoTipPercentage ?? _serverSettings.ServerData.DefaultTipPercentage;
            var tipAmount = FareHelper.CalculateTipAmount(ibsOrderInfo.MeterAmount, tipPercentage);

            var bookingFees = 0m;
            var total = ibsOrderInfo.MeterAmount + tipAmount;

            if (orderStatusDetail.CompanyKey.HasValue())
            {
                // Booking fees will be received by the local company
                var feesCharged = _feeService.ChargeBookingFeesIfNecessary(orderStatusDetail);
                if (feesCharged.HasValue)
                {
                    bookingFees = feesCharged.Value;
                    _logger.LogMessage("Order {0}: Booking fees of {1} charged to local company", orderStatusDetail.OrderId, feesCharged);
                    _logger.LogMessage("Order {0}: Received total amount from IBS of {1}, calculated a tip of {2}% (tip amount: {3}), for a total of {4}",
                            orderStatusDetail.OrderId, ibsOrderInfo.MeterAmount, tipPercentage, tipAmount, total);
                }
            }
            else
            {
                // Already in local company, include booking fees in trip
                bookingFees = orderDetail.BookingFees;
                total += Convert.ToDouble(bookingFees);

                _logger.LogMessage("Order {0}: Received total amount from IBS of {1}, calculated a tip of {2}% (tip amount: {3}), adding booking fees of {4} for a total of {5}",
                        orderStatusDetail.OrderId, ibsOrderInfo.MeterAmount, tipPercentage, tipAmount, bookingFees, total);
            }

            if (!_serverSettings.ServerData.SendDetailedPaymentInfoToDriver)
            {
                // this is the only payment related message sent to the driver when this setting is false
                SendMinimalPaymentProcessedMessageToDriver(ibsOrderInfo.VehicleNumber, ibsOrderInfo.MeterAmount + tipAmount, ibsOrderInfo.MeterAmount, tipAmount, orderStatusDetail.CompanyKey);
            }

            try
            {
                var totalOrderAmount = Convert.ToDecimal(total);
                var amountSaved = 0m;

                var promoUsed = _promotionDao.FindByOrderId(orderStatusDetail.OrderId);
                if (promoUsed != null)
                {
                    var promoDomainObject = _promoRepository.Get(promoUsed.PromoId);
                    amountSaved = promoDomainObject.GetDiscountAmount(Convert.ToDecimal(ibsOrderInfo.MeterAmount), Convert.ToDecimal(tipAmount));
                    totalOrderAmount = totalOrderAmount - amountSaved;
                }

                var tempPaymentInfo = _orderDao.GetTemporaryPaymentInfo(orderStatusDetail.OrderId);

                // Preautorize
                var preAuthResponse = PreauthorizePaymentIfNecessary(orderDetail, orderStatusDetail.CompanyKey, orderStatusDetail.OrderId, totalOrderAmount, tempPaymentInfo != null ? tempPaymentInfo.Cvv : null);
                if (preAuthResponse.IsSuccessful)
                {
                    // Commit
                    var paymentResult = CommitPayment(orderDetail,
                        totalOrderAmount,
                        Convert.ToDecimal(ibsOrderInfo.MeterAmount), 
                        Convert.ToDecimal(tipAmount),
                        Convert.ToDecimal(ibsOrderInfo.Toll),
                        Convert.ToDecimal(ibsOrderInfo.Surcharge),
                        bookingFees,
                        orderStatusDetail.OrderId,
                        promoUsed != null
                            ? promoUsed.PromoId
                            : (Guid?) null,
                        amountSaved,
                        Convert.ToDecimal(ibsOrderInfo.Fare),
                        Convert.ToDecimal(ibsOrderInfo.Extras),
                        Convert.ToDecimal(ibsOrderInfo.VAT),
                        Convert.ToDecimal(ibsOrderInfo.Discount)
                        );
                    if (paymentResult.IsSuccessful)
                    {
                        _logger.LogMessage("Order {0}: Payment Successful (Auth: {1}) [Transaction Id: {2}]", orderStatusDetail.OrderId, paymentResult.AuthorizationCode, paymentResult.TransactionId);
                    }
                    else
                    {
                        if (_serverSettings.ServerData.SendDetailedPaymentInfoToDriver)
                        {
                            _ibs.SendMessageToDriver(_resources.Get("PaymentFailedToDriver"), orderStatusDetail.VehicleNumber, orderStatusDetail.CompanyKey);
                        }

                        // set the payment error message in OrderStatusDetail for reporting purpose
                        orderStatusDetail.PairingError = paymentResult.Message;

                        _logger.LogMessage("Order {0}: Payment FAILED (Message: {1}) [Transaction Id: {2}]", orderStatusDetail.OrderId, paymentResult.Message, paymentResult.TransactionId);
                    }
                }
                else
                {
                    if (_serverSettings.ServerData.SendDetailedPaymentInfoToDriver)
                    {
                        _ibs.SendMessageToDriver(_resources.Get("PaymentFailedToDriver"), orderStatusDetail.VehicleNumber, orderStatusDetail.CompanyKey);
                    }

                    // set the payment error message in OrderStatusDetail for reporting purpose
                    orderStatusDetail.PairingError = preAuthResponse.Message;

                    _logger.LogMessage("Order {0}: Payment FAILED (Message: {1}) [Transaction Id: {2}]", orderStatusDetail.OrderId, preAuthResponse.Message, preAuthResponse.TransactionId);
                }
            }
            catch (Exception ex)
            {
                if (_serverSettings.ServerData.SendDetailedPaymentInfoToDriver)
                {
					_ibs.SendMessageToDriver(_resources.Get("PaymentFailedToDriver"), orderStatusDetail.VehicleNumber, orderStatusDetail.CompanyKey);
                }

                // set the payment error message in OrderStatusDetail for reporting purpose
                orderStatusDetail.PairingError = ex.Message;

                _logger.LogMessage("Order {0}: Payment FAILED (Message: {1}) [Transaction Id: {2}]", orderStatusDetail.OrderId, ex.Message, "UNKNOWN");
            }
            
            // whether there's a success or not, we change the status back to Completed since we can't process the payment again
            orderStatusDetail.Status = OrderStatus.Completed;
        }

        private CommitPreauthorizedPaymentResponse CommitPayment(OrderDetail orderDetail, decimal totalOrderAmount, decimal meterAmount, decimal tipAmount, 
            decimal tollAmount, decimal surchargeAmount, decimal bookingFees, Guid orderId, Guid? promoUsedId = null, decimal amountSaved = 0, decimal fareAmount = 0,
            decimal extrasAmount = 0, decimal vatAmount = 0, decimal discountAmount = 0)
        {
            if (orderDetail == null)
            {
                throw new Exception("Order not found");
            }

            if (orderDetail.IBSOrderId == null)
            {
                throw new Exception("Order has no IBSOrderId");
            }

            var account = _accountDao.FindById(orderDetail.AccountId);

            var paymentDetail = _paymentDao.FindByOrderId(orderId, orderDetail.CompanyKey);
            if (paymentDetail == null)
            {
                throw new Exception("Payment not found");
            }

            var paymentProviderServiceResponse = new CommitPreauthorizedPaymentResponse
            {
                TransactionId = paymentDetail.TransactionId
            };

            try
            {
                var message = string.Empty;
                
                if (paymentDetail.IsCompleted)
                {
                    message = "Order already paid or payment currently processing";
                }
                else
                {
                    if (totalOrderAmount > 0)
                    {
                        // Commit
                        paymentProviderServiceResponse = _paymentService.CommitPayment(
                            orderDetail.CompanyKey,
                            orderId,
                            account,
                            paymentDetail.PreAuthorizedAmount,
                            totalOrderAmount,
                            meterAmount,
                            tipAmount,
                            paymentDetail.TransactionId);

                        message = paymentProviderServiceResponse.Message;
                    }
                    else
                    {
                        // promotion made the ride free to the user
                        // void preauth if it exists
                        _paymentService.VoidPreAuthorization(orderDetail.CompanyKey, orderId);

                        paymentProviderServiceResponse.IsSuccessful = true;
                    }
                }

                //send information to IBS
                try
                {
                    var providerType = _paymentService.ProviderType(orderDetail.CompanyKey, orderDetail.Id);

                    string cardToken;
                    if (providerType == PaymentProvider.PayPal)
                    {
                        cardToken = "PayPal";
                    }
                    else
                    {
                        var creditCard = _creditCardDao.FindById(account.DefaultCreditCard.GetValueOrDefault());
                        cardToken = creditCard.Token;
                    }

                    _ibs.ConfirmExternalPayment(orderDetail.Id,
                        orderDetail.IBSOrderId.Value,
                        totalOrderAmount,
                        Convert.ToDecimal(tipAmount),
                        Convert.ToDecimal(meterAmount),
                        paymentProviderServiceResponse.IsSuccessful ? PaymentType.CreditCard.ToString() : FailedCode,
                        providerType.ToString(),
                        paymentProviderServiceResponse.TransactionId,
                        paymentProviderServiceResponse.AuthorizationCode,
                        cardToken,
                        account.IBSAccountId.GetValueOrDefault(),
                        orderDetail.Settings.Name,
                        orderDetail.Settings.Phone,
                        account.Email,
                        orderDetail.UserAgent.GetOperatingSystem(),
                        orderDetail.UserAgent,
                        orderDetail.CompanyKey,
                        fareAmount,
                        extrasAmount,
                        vatAmount,
                        discountAmount,
                        tollAmount,
                        surchargeAmount);
                }
                catch (Exception e)
                {
                    _logger.LogError(e);
                    message = e.Message;

                    try
                    {
                        if (paymentProviderServiceResponse.IsSuccessful)
                        {
                            _paymentService.VoidTransaction(orderDetail.CompanyKey, orderId, paymentProviderServiceResponse.TransactionId, ref message);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogMessage("Can't cancel transaction");
                        _logger.LogError(ex);
                        message = message + ex.Message;
                        //can't cancel transaction, send a command to log later
                    }
                    finally
                    {
                        paymentProviderServiceResponse.IsSuccessful = false;
                    }
                }

                if (paymentProviderServiceResponse.IsSuccessful)
                {
                    // Payment completed

                    var fareObject = FareHelper.GetFareFromAmountInclTax(Convert.ToDouble(meterAmount), _serverSettings.ServerData.VATIsEnabled ? _serverSettings.ServerData.VATPercentage : 0);

                    _commandBus.Send(new CaptureCreditCardPayment
                    {
                        AccountId = account.Id,
                        PaymentId = paymentDetail.PaymentId,
                        Provider = _paymentService.ProviderType(orderDetail.CompanyKey, orderDetail.Id),
                        TotalAmount = totalOrderAmount,
                        MeterAmount = Convert.ToDecimal(fareObject.AmountExclTax),
                        TipAmount = Convert.ToDecimal(tipAmount),
                        TaxAmount = Convert.ToDecimal(fareObject.TaxAmount),
                        TollAmount = tollAmount,
                        SurchargeAmount = surchargeAmount,
                        AuthorizationCode = paymentProviderServiceResponse.AuthorizationCode,
                        TransactionId = paymentProviderServiceResponse.TransactionId,
                        PromotionUsed = promoUsedId,
                        AmountSavedByPromotion = amountSaved,
                        BookingFees = bookingFees
                    });
                }
                else
                {
                    // Void PreAuth because commit failed
                    _paymentService.VoidPreAuthorization(orderDetail.CompanyKey, orderId);

                    // Payment error
                    _commandBus.Send(new LogCreditCardError
                    {
                        PaymentId = paymentDetail.PaymentId,
                        Reason = message
                    });

                    if (paymentProviderServiceResponse.IsDeclined)
                    {
                        _commandBus.Send(new ReactToPaymentFailure
                        {
                            AccountId = account.Id,
                            OrderId = orderId,
                            IBSOrderId = orderDetail.IBSOrderId,
                            CreditCardId = account.DefaultCreditCard.GetValueOrDefault(),
                            OverdueAmount = totalOrderAmount,
                            TransactionId = paymentProviderServiceResponse.TransactionId,
                            TransactionDate = paymentProviderServiceResponse.TransactionDate
                        });
                    }
                }

                return new CommitPreauthorizedPaymentResponse
                {
                    AuthorizationCode = paymentProviderServiceResponse.AuthorizationCode,
                    TransactionId = paymentProviderServiceResponse.TransactionId,
                    IsSuccessful = paymentProviderServiceResponse.IsSuccessful,
                    Message = paymentProviderServiceResponse.IsSuccessful ? "Success" : message
                };
            }
            catch (Exception e)
            {
                _logger.LogMessage("Error during payment " + e);
                _logger.LogError(e);
                return new CommitPreauthorizedPaymentResponse
                {
                    IsSuccessful = false,
                    TransactionId = paymentProviderServiceResponse.TransactionId,
                    Message = e.Message
                };
            }
        }

        private void CheckForPairingAndHandleIfNecessary(OrderStatusDetail orderStatusDetail, IBSOrderInformation ibsOrderInfo, ServerPaymentSettings paymentSettings, OrderDetail orderDetail, Trip trip)
        {
            if (orderStatusDetail.IsPrepaid)
            {
                _logger.LogMessage("Order {0}: No pairing to process as the order has been paid at the time of booking.", orderStatusDetail.OrderId);
                return;
            }

            var pairingInfo = _orderDao.FindOrderPairingById(orderStatusDetail.OrderId);
            if (pairingInfo == null || pairingInfo.WasUnpaired)
            {
                _logger.LogMessage("Order {0}: No pairing to process as no pairing information was found.", orderStatusDetail.OrderId);
                return;
            }

            var paymentMode = paymentSettings.PaymentMode;
            var isPayPal = _paymentService.IsPayPal(null, orderStatusDetail.OrderId);
            
            if (!isPayPal && paymentMode == PaymentMethod.RideLinqCmt)
            {
                HandlePairingForRideLinqCmt(orderStatusDetail, pairingInfo, ibsOrderInfo, paymentSettings, trip);
                return;
            }

            if (isPayPal
                || paymentMode == PaymentMethod.Cmt
                || paymentMode == PaymentMethod.Braintree
                || paymentMode == PaymentMethod.Moneris)
            {
                HandlePairingForStandardPairing(orderStatusDetail, pairingInfo, ibsOrderInfo, orderDetail);
                return;
            }
            
            throw new NotImplementedException("Cannot have pairing without any payment mode");
        }

        private bool OrderNeedsUpdate(IBSOrderInformation ibsOrderInfo, OrderStatusDetail orderStatusDetail)
        {
            return (ibsOrderInfo.Status.HasValue()                                // ibs status changed
                        && orderStatusDetail.IBSStatusId != ibsOrderInfo.Status) 
                   || (!orderStatusDetail.FareAvailable                           // fare was not available and ibs now has the information
                        && ibsOrderInfo.Fare > 0)
                   || (ibsOrderInfo.PairingCode != orderStatusDetail.RideLinqPairingCode) // status could be wosAssigned and we would get the pairing code later.
                   || orderStatusDetail.Status == OrderStatus.WaitingForPayment   // special case for pairing
                   || (orderStatusDetail.Status == OrderStatus.TimedOut           // special case for network                   
                        && _serverSettings.ServerData.Network.Enabled);           
        }

        private void CheckForOrderTimeOut(OrderStatusDetail orderStatusDetail)
        {
            if (!_serverSettings.ServerData.Network.Enabled
                || orderStatusDetail.Status == OrderStatus.TimedOut
                || orderStatusDetail.IgnoreDispatchCompanySwitch)
            {
                // Nothing to do
                return;
            }

            if (orderStatusDetail.NetworkPairingTimeout.HasValue
                && orderStatusDetail.NetworkPairingTimeout.Value <= DateTime.UtcNow)
            {
                // Order timed out
                _commandBus.Send(new NotifyOrderTimedOut
                {
                    OrderId = orderStatusDetail.OrderId,
                    Market = orderStatusDetail.Market
                });
            }
        }

        private string GetDescription(Guid orderId, IBSOrderInformation ibsOrderInfo, string companyName, bool sendEtaToDriverOnNotifyOnce, bool hasBailed, OrderDetail orderDetail)
        {
           
            _languageCode = orderDetail != null ? orderDetail.ClientLanguageCode : SupportedLanguages.en.ToString();

            if (hasBailed)
            {
                return _resources.Get("OrderStatus_BAILED", _languageCode);
            }

            string description = null;
            if (ibsOrderInfo.IsWaitingToBeAssigned)
            {
                if (companyName.HasValue())
                {
                    description = string.Format(_resources.Get("OrderStatus_wosWAITINGRoaming", _languageCode), companyName);
                    _logger.LogMessage("Setting Waiting in roaming status description: {0}", description);
                }
            }
            else if (ibsOrderInfo.IsAssigned)
            {
                description = string.Format(_resources.Get("OrderStatus_CabDriverNumberAssigned", _languageCode), ibsOrderInfo.VehicleNumber);
                _logger.LogMessage("Setting Assigned status description: {0}", description);

                var sendEtaToDriver = _serverSettings.ServerData.DriverEtaNotificationMode == DriverEtaNotificationModes.Always ||
                                      (_serverSettings.ServerData.DriverEtaNotificationMode == DriverEtaNotificationModes.Once && sendEtaToDriverOnNotifyOnce);

                if (_serverSettings.ServerData.ShowEta && sendEtaToDriver && orderDetail != null)
                {
                    try
                    {
                        SendEtaMessageToDriver(ibsOrderInfo.VehicleLatitude.GetValueOrDefault(), ibsOrderInfo.VehicleLongitude.GetValueOrDefault(), 
                            orderDetail.PickupAddress.Latitude, orderDetail.PickupAddress.Longitude, ibsOrderInfo.VehicleNumber, orderDetail.CompanyKey);
                    }
                    catch(Exception ex)
                    {
                        _logger.LogMessage("Cannot Send Eta to Vehicle Number " + ibsOrderInfo.VehicleNumber);
                        _logger.LogError(ex);
                    }
                }
            }
            else if (ibsOrderInfo.IsCanceled)
            {
                if (ibsOrderInfo.Status == VehicleStatuses.Common.NoShow)
                {
                    description = _resources.Get("OrderStatus_" + ibsOrderInfo.Status, _languageCode);
                    _logger.LogMessage("Setting NoShow status description: {0}", description);
                }
                else
                {
                    description = _resources.Get("OrderStatus_" + ibsOrderInfo.Status, _languageCode);
                    _logger.LogMessage("Setting Canceled status description: {0}", description);
                }
                
            }
            else if (ibsOrderInfo.IsComplete)
            {
                description = _resources.Get("OrderStatus_wosDONE", _languageCode);
                _logger.LogMessage("Setting Complete status description: {0}", description);
            }
            else if (ibsOrderInfo.IsLoaded)
            {
                if (orderDetail != null
                    && (orderDetail.Settings.ChargeTypeId == ChargeTypes.CardOnFile.Id
                        || orderDetail.Settings.ChargeTypeId == ChargeTypes.PayPal.Id))
                {
                    description = _resources.Get("OrderStatus_wosLOADEDAutoPairing", _languageCode);
                }
            }

            return description.HasValue()
                        ? description
                        : _resources.Get("OrderStatus_" + ibsOrderInfo.Status, _languageCode);
        }

        private void SendEtaMessageToDriver(double vehicleLatitude, double vehicleLongitude, double pickupLatitude, double pickupLongitude, string vehicleNumber, string company)
        {
            var eta = _directions.GetEta(vehicleLatitude, vehicleLongitude, pickupLatitude, pickupLongitude);
            if (eta != null && eta.IsValidEta())
            {
                var etaMessage = string.Format(_resources.Get("EtaMessageToDriver"), eta.FormattedDistance, eta.Duration);
				_ibs.SendMessageToDriver(etaMessage, vehicleNumber, company);
                _logger.LogMessage(etaMessage);
            }
        }

		private void SendPaymentBeingProcessedMessageToDriver(string vehicleNumber, string companyKey)
        {
            var paymentBeingProcessedMessage = _resources.Get("PaymentBeingProcessedMessageToDriver");
            _ibs.SendMessageToDriver(paymentBeingProcessedMessage, vehicleNumber, companyKey);
            _logger.LogMessage(paymentBeingProcessedMessage);
        }

        private void SendMinimalPaymentProcessedMessageToDriver(string vehicleNumber, double amount, double meter, double tip, string companyKey)
        {
            _ibs.SendPaymentNotification(amount, meter, tip, null, vehicleNumber, companyKey);
        }

        private void InitializeCmtServiceClient(ServerPaymentSettings paymentSettings)
        {
            var cmtMobileServiceClient = new CmtMobileServiceClient(paymentSettings.CmtPaymentSettings, null, null, null);
            _cmtTripInfoServiceHelper = new CmtTripInfoServiceHelper(cmtMobileServiceClient, _logger);
        }
    }
}