#region

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
        private readonly IAccountChargeDao _accountChargeDao;
        private readonly IBookingWebServiceClient _bookingWebServiceClient;
        private readonly ICommandBus _commandBus;
        private readonly IConfigurationManager _configManager;
        private readonly IAppSettings _appSettings;
        private readonly ReferenceDataService _referenceDataService;
        private readonly IRuleCalculator _ruleCalculator;
        private readonly IStaticDataWebServiceClient _staticDataWebServiceClient;
        private readonly IUpdateOrderStatusJob _updateOrderStatusJob;
        private readonly Resources.Resources _resources;

        public CreateOrderService(ICommandBus commandBus,
            IBookingWebServiceClient bookingWebServiceClient,
            IAccountDao accountDao,
            IConfigurationManager configManager,
            IAppSettings appSettings,
            ReferenceDataService referenceDataService,
            IStaticDataWebServiceClient staticDataWebServiceClient,
            IRuleCalculator ruleCalculator,
            IUpdateOrderStatusJob updateOrderStatusJob,
            IAccountChargeDao accountChargeDao,
            IOrderDao orderDao)
        {
            _accountChargeDao = accountChargeDao;
            _commandBus = commandBus;
            _bookingWebServiceClient = bookingWebServiceClient;
            _accountDao = accountDao;
            _referenceDataService = referenceDataService;
            _configManager = configManager;
            _appSettings = appSettings;
            _staticDataWebServiceClient = staticDataWebServiceClient;
            _ruleCalculator = ruleCalculator;
            _updateOrderStatusJob = updateOrderStatusJob;
            _orderDao = orderDao;

            var applicationKey = _configManager.GetSetting("TaxiHail.ApplicationKey");
            _resources = new Resources.Resources(applicationKey);
        }

        public object Post(CreateOrder request)
        {
            Log.Info("Create order request : " + request.ToJson());

            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));
            // User can still create future order, but we allow only one active Book now order.
            if (!request.PickupDate.HasValue)
            {
                Guid? pendingOrderId = GetPendingOrder();

                // We don't allow order creation if there's already on order being scheduled
                if (pendingOrderId != null)
                {
                    throw new HttpError(HttpStatusCode.Forbidden, ErrorCode.CreateOrder_PendingOrder.ToString(), pendingOrderId.ToString());
                }
            }

            //check if the account has a credit card
            if (request.Settings.ChargeTypeId.HasValue
                && request.Settings.ChargeTypeId.Value == ChargeTypes.CardOnFile.Id
                && !account.DefaultCreditCard.HasValue)
            {
                throw new HttpError(ErrorCode.CreateOrder_CardOnFileButNoCreditCard.ToString());
            }

            var rule = _ruleCalculator.GetActiveDisableFor(request.PickupDate.HasValue,
                request.PickupDate.HasValue ? request.PickupDate.Value : GetCurrentOffsetedTime(),
                () =>
                    _staticDataWebServiceClient.GetZoneByCoordinate(request.Settings.ProviderId,
                        request.PickupAddress.Latitude, request.PickupAddress.Longitude));

            if (rule != null)
            {
                var err = new HttpError(HttpStatusCode.Forbidden, ErrorCode.CreateOrder_RuleDisable.ToString(),
                    rule.Message);
                throw err;
            }

            if (Params.Get(request.Settings.Name, request.Settings.Phone).Any(p => p.IsNullOrEmpty()))
            {
                throw new HttpError(ErrorCode.CreateOrder_SettingsRequired.ToString());
            }

            
            var referenceData = (ReferenceData) _referenceDataService.Get(new ReferenceDataRequest());

            request.PickupDate = request.PickupDate.HasValue ? request.PickupDate.Value : GetCurrentOffsetedTime();
            request.Settings.Passengers = request.Settings.Passengers <= 0 ? 1 : request.Settings.Passengers;

            var needATarif = bool.Parse(_configManager.GetSetting("Direction.NeedAValidTarif"));

            if (needATarif && (!request.Estimate.Price.HasValue || request.Estimate.Price == 0))
            {
                throw new HttpError(ErrorCode.CreateOrder_NoFareEstimateAvailable.ToString());
            }

            if (request.Settings.ChargeTypeId.HasValue
                && request.Settings.ChargeTypeId.Value == ChargeTypes.Account.Id)
            {
                ValidateChargeAccountAnswers(request.Settings.AccountNumber, request.QuestionsAndAnswers);
            }

            var chargeType = ChargeTypes.GetList()
                    .Where(x => x.Id == request.Settings.ChargeTypeId)
                    .Select(x => x.Display)
                    .FirstOrDefault();

            if (chargeType != null)
            {
                chargeType = _resources.Get(chargeType, _appSettings.Data.PriceFormat);
            }

            var ibsOrderId = CreateIbsOrder(account, request, referenceData, chargeType);

            if (!ibsOrderId.HasValue
                || ibsOrderId <= 0)
            {
                var code = !ibsOrderId.HasValue || (ibsOrderId.Value >= -1) ? "" : "_" + Math.Abs(ibsOrderId.Value);
                return new HttpError(ErrorCode.CreateOrder_CannotCreateInIbs + code);
            }

            //Temporary solution for Aexid, we call the save extr payment to send the account info.  if not successful, we cancel the order.
            var result = TryToSendAccountInformation(  request.Id,   ibsOrderId.Value , request, account);
            if ( result.HasValue  )
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

            command.Settings.ChargeType = chargeType;
            command.Settings.VehicleType = vehicleType;
            emailCommand.Settings.ChargeType = chargeType;
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
            var accountChargeTypeId = _configManager.GetSetting<int>("AccountChargeTypeId", -1);
            if (accountChargeTypeId == -1)
            {
                accountChargeTypeId = _configManager.GetSetting<int>("Client.AccountChargeTypeId", -1);
            }

            if (accountChargeTypeId == request.Settings.ChargeTypeId)
            {
                return  _bookingWebServiceClient.SendAccountInformation(orderId, ibsOrderId, "Account", request.Settings.AccountNumber, account.IBSAccountId, request.Settings.Name, request.Settings.Phone, account.Email);                
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

            var ibsServerTimeDifference =
                _configManager.GetSetting("IBS.TimeDifference").SelectOrDefault(setting => long.Parse(setting), 0);
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
            var result = _bookingWebServiceClient.CreateOrder(request.Settings.ProviderId, account.IBSAccountId,
                request.Settings.Name, request.Settings.Phone, request.Settings.Passengers,
                request.Settings.VehicleTypeId, null, note, request.PickupDate.Value,
                ibsPickupAddress, ibsDropOffAddress, fare);

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
            var noteTemplate = _configManager.GetSetting("IBS.NoteTemplate");

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

            var vatEnabled = _configManager.GetSetting("VATIsEnabled", false);

            if (!vatEnabled)
            {
                return Fare.FromAmountInclTax((decimal) estimate.Price.Value, 0m);
            }

            var taxPercentage = _configManager.GetSetting("VATPercentage", 0d);
            return Fare.FromAmountInclTax((decimal) estimate.Price.Value, (decimal) taxPercentage);
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