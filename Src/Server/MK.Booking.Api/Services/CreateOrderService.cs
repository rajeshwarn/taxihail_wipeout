#region

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Jobs;
using apcurium.MK.Booking.Calculator;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.IBS.Impl;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using AutoMapper;
using Infrastructure.Messaging;
using log4net;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using ServiceStack.Text;
using CreateOrder = apcurium.MK.Booking.Api.Contract.Requests.CreateOrder;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class CreateOrderService : Service
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (CreateOrderService));

        private readonly IAccountDao _accountDao;
        private readonly IOrderDao _orderDao;
        private readonly IPaymentService _paymentService;
        private readonly ICreditCardDao _creditCardDao;
        private readonly IAccountChargeDao _accountChargeDao;
        private readonly ICommandBus _commandBus;
        private readonly IServerSettings _serverSettings;
        private readonly ReferenceDataService _referenceDataService;
        private readonly IRuleCalculator _ruleCalculator;
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly IUpdateOrderStatusJob _updateOrderStatusJob;
        private readonly Resources.Resources _resources;

        public CreateOrderService(ICommandBus commandBus,
            IAccountDao accountDao,
            IServerSettings serverSettings,
            ReferenceDataService referenceDataService,
            IIBSServiceProvider ibsServiceProvider,
            IRuleCalculator ruleCalculator,
            IUpdateOrderStatusJob updateOrderStatusJob,
            IAccountChargeDao accountChargeDao,
            IOrderDao orderDao,
            IPaymentService paymentService,
            ICreditCardDao creditCardDao)
        {
            _accountChargeDao = accountChargeDao;
            _commandBus = commandBus;
            _accountDao = accountDao;
            _referenceDataService = referenceDataService;
            _serverSettings = serverSettings;
            _ibsServiceProvider = ibsServiceProvider;
            _ruleCalculator = ruleCalculator;
            _updateOrderStatusJob = updateOrderStatusJob;
            _orderDao = orderDao;
            _paymentService = paymentService;
            _creditCardDao = creditCardDao;

            _resources = new Resources.Resources(_serverSettings);
        }

        public object Post(CreateOrder request)
        {
            Log.Info("Create order request : " + request.ToJson());

            if (!request.FromWebApp)
            {
                ValidateAppVersion(request.ClientLanguageCode);
            }

            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));

            account.IBSAccountId = CreateIbsAccountIfNeeded(account);

            // User can still create future order, but we allow only one active Book now order.
            if (!request.PickupDate.HasValue)
            {
                var pendingOrderId = GetPendingOrder();

                // We don't allow order creation if there's already an order scheduled
                if (!_serverSettings.ServerData.AllowSimultaneousAppOrders
                    && pendingOrderId != null
                    && !request.FromWebApp)
                {
                    throw new HttpError(HttpStatusCode.Forbidden, ErrorCode.CreateOrder_PendingOrder.ToString(), pendingOrderId.ToString());
                }
            }

            // Payment mode is card on file
            if (request.Settings.ChargeTypeId.HasValue
                && request.Settings.ChargeTypeId.Value == ChargeTypes.CardOnFile.Id)
            {
                ValidateCreditCard(request.Id, account, request.ClientLanguageCode);
            }

            bool isChargeAccountPaymentWithCardOnFile = false;
            var chargeTypeKey = ChargeTypes.GetList()
                    .Where(x => x.Id == request.Settings.ChargeTypeId)
                    .Select(x => x.Display)
                    .FirstOrDefault();


            string[] prompts = null;
            int?[] promptsLength = null;
            // Payment mode is charge account
            if (request.Settings.ChargeTypeId.HasValue
                && request.Settings.ChargeTypeId.Value == ChargeTypes.Account.Id)
            {
                ValidateChargeAccountAnswers(request.Settings.AccountNumber, request.QuestionsAndAnswers);

                prompts = request.QuestionsAndAnswers.Select(q => q.Answer).ToArray();
                promptsLength = request.QuestionsAndAnswers.Select(q => q.MaxLength).ToArray();

                // Change payment mode to card of file if necessary
                var accountChargeDetail = _accountChargeDao.FindByAccountNumber(request.Settings.AccountNumber);

                if (accountChargeDetail.UseCardOnFileForPayment)
                {
                    if (request.FromWebApp)
                    {
                        // Card on file payment not supported by the web app
                        throw new HttpError(HttpStatusCode.Forbidden, ErrorCode.CreateOrder_RuleDisable.ToString(),
                            _resources.Get("CannotCreateOrderChargeAccountNotSupported", request.ClientLanguageCode));
                    }

                    ValidateCreditCard(request.Id, account, request.ClientLanguageCode);

                    chargeTypeKey = ChargeTypes.CardOnFile.Display;
                    request.Settings.ChargeTypeId = ChargeTypes.CardOnFile.Id;
                    isChargeAccountPaymentWithCardOnFile = true;
                }
            }

            var rule = _ruleCalculator.GetActiveDisableFor(
                request.PickupDate.HasValue,
                request.PickupDate.HasValue
                    ? request.PickupDate.Value
                    : GetCurrentOffsetedTime(),
                () => _ibsServiceProvider.StaticData().GetZoneByCoordinate(
                        request.Settings.ProviderId,
                        request.PickupAddress.Latitude,
                        request.PickupAddress.Longitude),
                () => request.DropOffAddress != null
                    ? _ibsServiceProvider.StaticData().GetZoneByCoordinate(
                            request.Settings.ProviderId,
                            request.DropOffAddress.Latitude,
                            request.DropOffAddress.Longitude)
                    : null);

            if (rule != null)
            {
                var err = new HttpError(HttpStatusCode.Forbidden, ErrorCode.CreateOrder_RuleDisable.ToString(), rule.Message);
                throw err;
            }

            if (Params.Get(request.Settings.Name, request.Settings.Phone).Any(p => p.IsNullOrEmpty()))
            {
                throw new HttpError(ErrorCode.CreateOrder_SettingsRequired.ToString());
            }

            var referenceData = (ReferenceData)_referenceDataService.Get(new ReferenceDataRequest());

            request.PickupDate = request.PickupDate.HasValue
                ? request.PickupDate.Value
                : GetCurrentOffsetedTime();

            request.Settings.Passengers = request.Settings.Passengers <= 0
                ? 1
                : request.Settings.Passengers;

            if (_serverSettings.ServerData.Direction.NeedAValidTarif
                && (!request.Estimate.Price.HasValue || request.Estimate.Price == 0))
            {
                throw new HttpError(ErrorCode.CreateOrder_NoFareEstimateAvailable.ToString());
            }

            var chargeTypeIbs = string.Empty;
            var chargeTypeEmail = string.Empty;
            if (chargeTypeKey != null)
            {
                // this must be localized with the priceformat to be localized in the language of the company
                // because it is sent to the driver
                chargeTypeIbs = _resources.Get(chargeTypeKey, _serverSettings.ServerData.PriceFormat);

                chargeTypeEmail = _resources.Get(chargeTypeKey, request.ClientLanguageCode);
            }

            var ibsOrderId = CreateIbsOrder(account.IBSAccountId.Value, request, referenceData, chargeTypeIbs, prompts, promptsLength);

            if (!ibsOrderId.HasValue
                || ibsOrderId <= 0)
            {
                var code = !ibsOrderId.HasValue || (ibsOrderId.Value >= -1) ? "" : "_" + Math.Abs(ibsOrderId.Value);
                return new HttpError(ErrorCode.CreateOrder_CannotCreateInIbs + code);
            }

            //Temporary solution for Aexid, we call the save extr payment to send the account info.  if not successful, we cancel the order.
            var result = TryToSendAccountInformation(request.Id, ibsOrderId.Value, request, account);
            if (result.HasValue)
            {
                return new HttpError(ErrorCode.CreateOrder_CannotCreateInIbs + "_" + Math.Abs(result.Value));
            }

            var command = Mapper.Map<Commands.CreateOrder>(request);
            var emailCommand = Mapper.Map<SendBookingConfirmationEmail>(request);

            command.IBSOrderId = emailCommand.IBSOrderId = ibsOrderId.Value;
            command.AccountId = account.Id;
            command.UserAgent = base.Request.UserAgent;
            command.ClientVersion = base.Request.Headers.Get("ClientVersion");
            command.IsChargeAccountPaymentWithCardOnFile = isChargeAccountPaymentWithCardOnFile;
            emailCommand.EmailAddress = account.Email;

            // Get Vehicle Type from reference data
            var vehicleType =
                referenceData.VehiclesList.Where(x => x.Id == request.Settings.VehicleTypeId)
                    .Select(x => x.Display)
                    .FirstOrDefault();

            command.Settings.ChargeType = chargeTypeIbs;
            command.Settings.VehicleType = vehicleType;
            emailCommand.Settings.ChargeType = chargeTypeEmail;
            emailCommand.Settings.VehicleType = vehicleType;

            _commandBus.Send(command);
            _commandBus.Send(emailCommand);

            UpdateStatusAsync(command.OrderId);

            return new OrderStatusDetail
            {
                OrderId = command.OrderId,
                Status = OrderStatus.Created,
                IBSOrderId = ibsOrderId,
                IBSStatusId = "",
                IBSStatusDescription = (string)_resources.Get("OrderStatus_wosWAITING", command.ClientLanguageCode),
            };
        }

        public object Post(SwitchOrderToNextDispatchCompanyRequest request)
        {
            Log.Info("Switching order to another IBS : " + request.ToJson());

            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));
            var order = _orderDao.FindById(request.OrderId);
            var orderStatusDetail = _orderDao.FindOrderStatusById(request.OrderId);

            if (orderStatusDetail.Status != OrderStatus.TimedOut)
            {
                // Only switch companies if order is timedout
                return orderStatusDetail;
            }

            var newOrderRequest = new CreateOrder
            {
                PickupDate = GetCurrentOffsetedTime(),
                PickupAddress = order.PickupAddress,
                DropOffAddress = order.DropOffAddress,
                Settings = new BookingSettings
                {
                    LargeBags = order.Settings.LargeBags,
                    Name = order.Settings.Name,
                    NumberOfTaxi = order.Settings.NumberOfTaxi,
                    Passengers = order.Settings.Passengers,
                    Phone = order.Settings.Phone,
                    ProviderId = null,

                    // Payment in app is not supported for now when we use another IBS
                    ChargeType = ChargeTypes.PaymentInCar.Display,
                    ChargeTypeId = ChargeTypes.PaymentInCar.Id,
                    
                    // Reset vehicle type
                    VehicleType = null,
                    VehicleTypeId = null
                },
                Note = order.UserNote,
                Estimate = new CreateOrder.RideEstimate { Price = order.EstimatedFare }
            };

            var newReferenceData = (ReferenceData)_referenceDataService.Get(new ReferenceDataRequest { CompanyKey = request.NextDispatchCompanyKey });

            // This must be localized with the priceformat to be localized in the language of the company
            // because it is sent to the driver
            var chargeTypeIbs = _resources.Get(ChargeTypes.PaymentInCar.Display, _serverSettings.ServerData.PriceFormat);

            var networkErrorMessage = string.Format(_resources.Get("Network_CannotCreateOrder", order.ClientLanguageCode), request.NextDispatchCompanyName);

            int ibsAccountId;
            try
            {
                // Recreate order on next dispatch company IBS
                ibsAccountId = CreateIbsAccountIfNeeded(account, request.NextDispatchCompanyKey);
            }
            catch (Exception ex)
            {
                Log.Error(networkErrorMessage, ex);
                throw new HttpError(HttpStatusCode.InternalServerError, networkErrorMessage);
            }

            var newIbsOrderId = CreateIbsOrder(ibsAccountId, newOrderRequest, newReferenceData, chargeTypeIbs, null, null, request.NextDispatchCompanyKey);
            if (!newIbsOrderId.HasValue || newIbsOrderId <= 0)
            {
                var code = !newIbsOrderId.HasValue || (newIbsOrderId.Value >= -1) ? string.Empty : "_" + Math.Abs(newIbsOrderId.Value);
                Log.Error(string.Format("{0}. IBS error code: {1}", networkErrorMessage, code));
                throw new HttpError(HttpStatusCode.InternalServerError, networkErrorMessage);
            }

            // Cancel order on current company IBS
            CancelIbsOrder(order, account.Id, account.IBSAccountId);

            _commandBus.Send(new SwitchOrderToNextDispatchCompany
            {
                OrderId = request.OrderId,
                IBSOrderId = newIbsOrderId.Value,
                CompanyKey = request.NextDispatchCompanyKey,
                CompanyName = request.NextDispatchCompanyName
            });

            return new OrderStatusDetail
            {
                OrderId = request.OrderId,
                Status = OrderStatus.Created,
                CompanyKey = request.NextDispatchCompanyKey,
                CompanyName = request.NextDispatchCompanyName,
                NextDispatchCompanyKey = null,
                NextDispatchCompanyName = null,
                IBSOrderId = newIbsOrderId,
                IBSStatusId = string.Empty,
                IBSStatusDescription = string.Format(_resources.Get("OrderStatus_wosWAITINGRoaming", order.ClientLanguageCode), request.NextDispatchCompanyName),
            };
        }

        public object Post(IgnoreDispatchCompanySwitchRequest request)
        {
            var order = _orderDao.FindById(request.OrderId);
            if (order == null)
            {
                return new HttpResult(HttpStatusCode.NotFound);
            }

            _commandBus.Send(new IgnoreDispatchCompanySwitch
            {
                OrderId = request.OrderId
            });

            return new HttpResult(HttpStatusCode.OK);
        }

        private int CreateIbsAccountIfNeeded(AccountDetail account, string companyKey = null)
        {
            var ibsAccountId = _accountDao.GetIbsAccountId(account.Id, companyKey);
            if (ibsAccountId.HasValue)
            {
                return ibsAccountId.Value;
            }

            // Account doesn't exist, create it
            ibsAccountId = _ibsServiceProvider.Account(companyKey).CreateAccount(account.Id,
                account.Email,
                string.Empty,
                account.Name,
                account.Settings.Phone);

            _commandBus.Send(new LinkAccountToIbs
            {
                AccountId = account.Id,
                IbsAccountId = ibsAccountId.Value,
                CompanyKey = companyKey
            });

            return ibsAccountId.Value;
        }

        private void ValidateCreditCard(Guid orderId, AccountDetail account, string clientLanguageCode)
        {
            if (!_serverSettings.GetPaymentSettings().IsPreAuthEnabled)
            {
                return;
            }

            // check if the account has a credit card
            if (!account.DefaultCreditCard.HasValue)
            {
                throw new HttpError(ErrorCode.CreateOrder_CardOnFileButNoCreditCard.ToString());
            }

            // try to preauthorize a small amount on the card to verify the validity
            var card = _creditCardDao.FindByAccountId(account.Id).First();

            // there's a minimum amount of $50 (warning indicating that on the admin ui)
            var preAuthAmount = Math.Max(_serverSettings.GetPaymentSettings().PreAuthAmount ?? 0, 50);

            var preAuthResponse = _paymentService.PreAuthorize(orderId, account.Email, card.Token, preAuthAmount);
            
            if (!preAuthResponse.IsSuccessful)
            {
                throw new HttpError(HttpStatusCode.Forbidden, ErrorCode.CreateOrder_RuleDisable.ToString(),
                    _resources.Get("CannotCreateOrder_CreditCardWasDeclined", clientLanguageCode));
            }
        }
        
        private void ValidateAppVersion(string clientLanguage)
        {
            var appVersion = base.Request.Headers.Get("ClientVersion");
            var minimumAppVersion = _serverSettings.ServerData.MinimumRequiredAppVersion;

            if (appVersion.IsNullOrEmpty() || minimumAppVersion.IsNullOrEmpty())
            {
                return;
            }

            var minimumMajorMinorBuild = minimumAppVersion.Split(new[]{ "." }, StringSplitOptions.RemoveEmptyEntries);
            var appMajorMinorBuild = appVersion.Split('.');

            for (var i = 0; i < appMajorMinorBuild.Length; i++)
            {
                var appVersionItem = int.Parse(appMajorMinorBuild[i]);
                var minimumVersionItem = int.Parse(minimumMajorMinorBuild.Length <= i ? "0" : minimumMajorMinorBuild[i]);

                if (appVersionItem < minimumVersionItem)
                {
                    throw new HttpError(HttpStatusCode.Forbidden, ErrorCode.CreateOrder_RuleDisable.ToString(),
                                        _resources.Get("CannotCreateOrderInvalidVersion", clientLanguage));
                }
            }
        }

        private void ValidateChargeAccountAnswers(string accountNumber, AccountChargeQuestion[] userQuestionsDetails)
        {
            var accountChargeDetail = _accountChargeDao.FindByAccountNumber(accountNumber);
            if (accountChargeDetail == null)
            {
                throw new HttpError(HttpStatusCode.Forbidden, ErrorCode.AccountCharge_InvalidAccountNumber.ToString());
            }
            
            var answers = userQuestionsDetails.Select(x => x.Answer);
            // TODO: Handle nulls
            var validation = _ibsServiceProvider.ChargeAccount().ValidateIbsChargeAccount(answers, accountNumber, "0");
            if (!validation.Valid)
            {                
                int firstError = validation.ValidResponse.IndexOf(false);                 
                throw new HttpError(HttpStatusCode.Forbidden, ErrorCode.AccountCharge_InvalidAnswer.ToString(),
                                        accountChargeDetail.Questions[firstError].ErrorMessage);
            }
        }

        private int? TryToSendAccountInformation(Guid orderId, int ibsOrderId, CreateOrder request, AccountDetail account)
        {
            if (ChargeTypes.Account.Id == request.Settings.ChargeTypeId)
            {
                return  _ibsServiceProvider.Booking().SendAccountInformation(orderId, ibsOrderId, "Account", request.Settings.AccountNumber, account.IBSAccountId.Value, request.Settings.Name, request.Settings.Phone, account.Email);                
            }

            return null;
        }
        private void UpdateStatusAsync(Guid orderId)
        {
            new TaskFactory().StartNew(() =>
            {
                //We have to wait for the order to be completed.
                Thread.Sleep(750);

                _updateOrderStatusJob.CheckStatus(orderId);
            });
        }

        private DateTime GetCurrentOffsetedTime()
        {
            //TODO : need to check ibs setup for shortesst time.

            var ibsServerTimeDifference = _serverSettings.ServerData.IBS.TimeDifference;
            var offsetedTime = DateTime.Now.AddMinutes(2);
            if (ibsServerTimeDifference != 0)
            {
                offsetedTime = offsetedTime.Add(new TimeSpan(ibsServerTimeDifference));
            }

            return offsetedTime;
        }

        private int? CreateIbsOrder(int ibsAccountId, CreateOrder request, ReferenceData referenceData, string chargeType, string[] prompts, int?[] promptsLength, string companyKey = null)
        {
            // Provider is optional
            // But if a provider is specified, it must match with one of the ReferenceData values
            if (request.Settings.ProviderId.HasValue &&
                referenceData.CompaniesList.None(c => c.Id == request.Settings.ProviderId.Value))
            {
                throw new HttpError(ErrorCode.CreateOrder_InvalidProvider.ToString());
            }

            var ibsPickupAddress = Mapper.Map<IbsAddress>(request.PickupAddress);
            var ibsDropOffAddress = IsValid(request.DropOffAddress)
                ? Mapper.Map<IbsAddress>(request.DropOffAddress)
                : null;

            var note = BuildNote(chargeType, request.Note, request.PickupAddress.BuildingName, request.Settings.LargeBags);
            var fare = GetFare(request.Estimate);

            Debug.Assert(request.PickupDate != null, "request.PickupDate != null");

            var result = _ibsServiceProvider.Booking(companyKey).CreateOrder(
                request.Settings.ProviderId,
                ibsAccountId,
                request.Settings.Name,
                request.Settings.Phone,
                request.Settings.Passengers,
                request.Settings.VehicleTypeId,
                null, // null since we don't use the ChargeTypes of ibs anymore
                note,
                request.PickupDate.Value,
                ibsPickupAddress,
                ibsDropOffAddress,
                request.Settings.ChargeTypeId == ChargeTypes.Account.Id    // send the account number only if we book using charge account
                    ? request.Settings.AccountNumber 
                    : null,
                null,
                prompts,
                promptsLength,
                fare);

            return result;
        }

        private void CancelIbsOrder(OrderDetail order, Guid accountId, int? ibsAccountId)
        {
            // Cancel order on current company IBS
            if (order.IBSOrderId.HasValue && ibsAccountId.HasValue)
            {
                var currentIbsAccountId = _accountDao.GetIbsAccountId(accountId, order.CompanyKey);
                if (currentIbsAccountId.HasValue)
                {
                    // We need to try many times because sometime the IBS cancel method doesn't return an error but doesn't cancel the ride...
                    // After 5 time, we are giving up. But we assume the order is completed.
                    Task.Factory.StartNew(() =>
                    {
                        Func<bool> cancelOrder = () => _ibsServiceProvider.Booking(order.CompanyKey).CancelOrder(order.IBSOrderId.Value, currentIbsAccountId.Value, order.Settings.Phone);
                        cancelOrder.Retry(new TimeSpan(0, 0, 0, 10), 5);
                    });
                }
            }
        }

        private bool IsValid(Address address)
        {
// ReSharper disable CompareOfFloatsByEqualityOperator
            return ((address != null) && address.FullAddress.HasValue() 
                                      && address.Longitude != 0 
                                      && address.Latitude != 0);
// ReSharper restore CompareOfFloatsByEqualityOperator
        }

        private string BuildNote(string chargeType, string note, string buildingName, int largeBags)
        {
            // Building Name is not handled by IBS
            // Put Building Name in note, if specified

            // Get NoteTemplate from app settings, if it exists
            var noteTemplate = _serverSettings.ServerData.IBS.NoteTemplate;

            if (!string.IsNullOrWhiteSpace(buildingName))
            {
                // Quickfix: If the address comes from our Google Places service
                // the building name will be formatted like this: "Building Name (Place Type)"
                // We need to remove the text in parenthesis

                const string pattern = @"
\(         # Look for an opening parenthesis
[^\)]+     # Take all characters that are not a closing parenthesis
\)$        # Look for a closing parenthesis at the end of the string";

                buildingName = new Regex(pattern, RegexOptions.IgnorePatternWhitespace)
                    .Replace(buildingName, string.Empty).Trim();
                buildingName = "Building name: " + buildingName;
            }

            var largeBagsString = string.Empty;
            if (largeBags > 0)
            {
                largeBagsString = "Large bags: " + largeBags;
            }

            if (!string.IsNullOrWhiteSpace(noteTemplate))
            {
                noteTemplate = string.Format("{0}{1}{2}", chargeType, Environment.NewLine, noteTemplate);

                var transformedTemplate = noteTemplate
                    .Replace("\\r", "\r")
                    .Replace("\\n", "\n")
                    .Replace("\\t", "\t")
                    .Replace("{{userNote}}", note ?? string.Empty)
                    .Replace("{{buildingName}}", buildingName ?? string.Empty)
                    .Replace("{{largeBags}}", largeBagsString)
                    .Trim();

                return transformedTemplate;
            }

            // In versions prior to 1.4, there was no note template
            // So if the IBS.NoteTemplate setting does not exist, use the old way 
            var formattedNote = string.Format("{0}{0}{1}{2}{3}", 
                                                Environment.NewLine, chargeType, 
                                                Environment.NewLine, note);
            if (!string.IsNullOrWhiteSpace(buildingName))
            {
                formattedNote += (Environment.NewLine + buildingName).Trim();
            }
            // "Large bags" appeared in 1.4, no need to concat it here
            return formattedNote;
        }

        private Fare GetFare(CreateOrder.RideEstimate estimate)
        {
            if (estimate == null || !estimate.Price.HasValue)
            {
                return default(Fare);
            }

            return Fare.FromAmountInclTax(estimate.Price.Value, 0);
        }

        private Guid? GetPendingOrder()
        {
            var activeOrders = _orderDao.GetOrdersInProgressByAccountId(new Guid(this.GetSession().UserAuthId));

            var latestActiveOrder = activeOrders.FirstOrDefault(o => o.IBSStatusId != VehicleStatuses.Common.Scheduled);
            if (latestActiveOrder != null)
            {
                return latestActiveOrder.OrderId;
            }

            return null;
        }
    }
}