﻿#region

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Jobs;
using apcurium.MK.Booking.Api.Services.Payment;
using apcurium.MK.Booking.Calculator;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
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
        private readonly IAccountWebServiceClient _accountWebServiceClient;
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
            ICreditCardDao creditCardDao,
            IAccountWebServiceClient accountWebServiceClient)
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
            _accountWebServiceClient = accountWebServiceClient;

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

            CreateIbsAccountIfNeeded(account);

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

            if (request.Settings.ChargeTypeId.HasValue
                && request.Settings.ChargeTypeId.Value == ChargeTypes.CardOnFile.Id)
            {
                ValidateCreditCard(request.Id, account, request.ClientLanguageCode);
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
            
            var referenceData = (ReferenceData) _referenceDataService.Get(new ReferenceDataRequest());

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

            if (request.Settings.ChargeTypeId.HasValue
                && request.Settings.ChargeTypeId.Value == ChargeTypes.Account.Id)
            {
                ValidateChargeAccountAnswers(request.Settings.AccountNumber, request.QuestionsAndAnswers);
            }

            var chargeTypeKey = ChargeTypes.GetList()
                    .Where(x => x.Id == request.Settings.ChargeTypeId)
                    .Select(x => x.Display)
                    .FirstOrDefault();

            var chargeTypeIbs = string.Empty;
            var chargeTypeEmail = string.Empty;
            if (chargeTypeKey != null)
            {
                // this must be localized with the priceformat to be localized in the language of the company
                // because it is sent to the driver
                chargeTypeIbs = _resources.Get(chargeTypeKey, _serverSettings.ServerData.PriceFormat);

                chargeTypeEmail = _resources.Get(chargeTypeKey, request.ClientLanguageCode);
            }

            var ibsOrderId = CreateIbsOrder(account, request, referenceData, chargeTypeIbs);

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
                IBSOrderId =  ibsOrderId,
                IBSStatusId = "",
                IBSStatusDescription = (string)_resources.Get("OrderStatus_wosWAITING", command.ClientLanguageCode),
            };
        }

        private void CreateIbsAccountIfNeeded(AccountDetail account)
        {
            if (!account.IBSAccountId.HasValue)
            {
                var ibsAccountId = _accountWebServiceClient.CreateAccount(account.Id,
                    account.Email,
                    "",
                    account.Name,
                    account.Settings.Phone);
                account.IBSAccountId = ibsAccountId;

                _commandBus.Send(new LinkAccountToIbs
                {
                    AccountId = account.Id,
                    IbsAccountId = ibsAccountId,
                    CompanyKey = null //for home ibs
                });
            }
        }

        private void ValidateCreditCard(Guid orderId, AccountDetail account, string clientLanguageCode)
        {
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

            for (int i = 0; i < accountChargeDetail.Questions.Count; i++)
            {
                var questionDetails = accountChargeDetail.Questions[i];
                var userQuestionDetails = userQuestionsDetails[i];

                if (!questionDetails.IsRequired)
                {
                    // Facultative question, do nothing
                    continue;
                }

                var userAnswer = userQuestionDetails.Answer;
                var validAnswers = questionDetails.Answer.Split(',').Select(a => a.Trim());

                if (!validAnswers.Any(p => String.Equals(userAnswer, p, questionDetails.IsCaseSensitive
                                                                        ? StringComparison.InvariantCulture
                                                                        : StringComparison.InvariantCultureIgnoreCase)))
                {
                    // User answer is not valid
                    throw new HttpError(HttpStatusCode.Forbidden, ErrorCode.AccountCharge_InvalidAnswer.ToString(),
                                        questionDetails.ErrorMessage);
                }
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

        private int? CreateIbsOrder(AccountDetail account, CreateOrder request, ReferenceData referenceData, string chargeType)
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

            var result = _ibsServiceProvider.Booking().CreateOrder(
                request.Settings.ProviderId,
                account.IBSAccountId.Value,
                request.Settings.Name,
                request.Settings.Phone,
                request.Settings.Passengers,
                request.Settings.VehicleTypeId,
                null,
                note,
                request.PickupDate.Value,
                ibsPickupAddress,
                ibsDropOffAddress,
                fare);

            return result;
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