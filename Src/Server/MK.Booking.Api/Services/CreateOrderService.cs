using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Jobs;
using apcurium.MK.Common.IoC;
using Infrastructure.Messaging;
using log4net;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Calculator;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using AutoMapper;
using ServiceStack.Common.Web;
using System.Net;
using apcurium.MK.Common;
using ServiceStack.Text;


namespace apcurium.MK.Booking.Api.Services
{
    public class CreateOrderService : RestServiceBase<CreateOrder>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CreateOrderService));
        
        private ICommandBus _commandBus;
        private IBookingWebServiceClient _bookingWebServiceClient;
        private IConfigurationManager _configManager;
        private IAccountDao _accountDao;
        private ReferenceDataService _referenceDataService;
        private IStaticDataWebServiceClient _staticDataWebServiceClient;
        private IRuleCalculator _ruleCalculator;
        private readonly IUpdateOrderStatusJob _updateOrderStatusJob;

        public CreateOrderService(ICommandBus commandBus,
                                    IBookingWebServiceClient bookingWebServiceClient,
                                    IAccountDao accountDao, 
                                    IConfigurationManager configManager,
                                    ReferenceDataService referenceDataService,
                                    IStaticDataWebServiceClient staticDataWebServiceClient,
                                    IRuleCalculator ruleCalculator,
                                    IUpdateOrderStatusJob updateOrderStatusJob)
        {
            _commandBus = commandBus;
            _bookingWebServiceClient = bookingWebServiceClient;
            _accountDao = accountDao;
            _referenceDataService = referenceDataService;
            _configManager = configManager;
            _staticDataWebServiceClient = staticDataWebServiceClient;
            _ruleCalculator = ruleCalculator;
            _updateOrderStatusJob = updateOrderStatusJob;
        }

        public override object OnPost(CreateOrder request)
        {
            Log.Info( "Create order request : " + request.ToJson());

            
            var rule = _ruleCalculator.GetActiveDisableFor(request.PickupDate.HasValue, request.PickupDate.HasValue ? request.PickupDate.Value : GetCurrentOffsetedTime(), ()=>_staticDataWebServiceClient.GetZoneByCoordinate(request.Settings.ProviderId, request.PickupAddress.Latitude, request.PickupAddress.Longitude));
          
            if (rule!= null)
            {
                var err = new HttpError(  HttpStatusCode.Forbidden, ErrorCode.CreateOrder_RuleDisable.ToString(), rule.Message);                
                throw err;
            }

            if (Params.Get(request.Settings.Name, request.Settings.Phone).Any(p => p.IsNullOrEmpty()))
            {
                throw new HttpError(ErrorCode.CreateOrder_SettingsRequired.ToString() );
            }

            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));
            var referenceData = (ReferenceData)_referenceDataService.OnGet(new ReferenceDataRequest());

            request.PickupDate = request.PickupDate.HasValue ? request.PickupDate.Value : GetCurrentOffsetedTime() ;
            request.Settings.Passengers = request.Settings.Passengers <= 0 ? 1 : request.Settings.Passengers;

            var needATarif = bool.Parse(_configManager.GetSetting("Direction.NeedAValidTarif"));

            if (needATarif && (request.Estimate.Price == null || request.Estimate.Price == 0))
            {
                throw new HttpError(ErrorCode.CreateOrder_NoFareEstimateAvailable.ToString());
            }

            var ibsOrderId = CreateIBSOrder(account, request, referenceData);

            if (!ibsOrderId.HasValue
                || ibsOrderId <= 0)
            {
                string code = !ibsOrderId.HasValue || (ibsOrderId.Value >= -1) ? "" : "_" + Math.Abs(ibsOrderId.Value).ToString();
                return new HttpError(ErrorCode.CreateOrder_CannotCreateInIbs.ToString() + code);
            }

            var command = Mapper.Map<Commands.CreateOrder>(request);
            var emailCommand = Mapper.Map<Commands.SendBookingConfirmationEmail>(request);

            command.IBSOrderId = emailCommand.IBSOrderId = ibsOrderId.Value;
            command.AccountId = account.Id;
            command.UserAgent = base.Request.UserAgent;
            emailCommand.EmailAddress = account.Email;

            // Get Charge Type and Vehicle Type from reference data
            var chargeType = referenceData.PaymentsList.Where(x => x.Id == request.Settings.ChargeTypeId).Select(x => x.Display).FirstOrDefault();
            var vehicleType = referenceData.VehiclesList.Where(x => x.Id == request.Settings.VehicleTypeId).Select(x => x.Display).FirstOrDefault();

            command.Settings.ChargeType = chargeType;
            command.Settings.VehicleType = vehicleType;
            emailCommand.Settings.ChargeType = chargeType;
            emailCommand.Settings.VehicleType = vehicleType;

            _commandBus.Send(command);
            if (bool.Parse(_configManager.GetSetting("Booking.ConfirmationEmail")))
            {
                _commandBus.Send(emailCommand);
            }


            UpdateStatusAsync();
            

            

            return new OrderStatusDetail { OrderId = command.OrderId, Status = OrderStatus.Created, IBSOrderId = ibsOrderId, IBSStatusId = "", IBSStatusDescription = "Processing your order" };
        }

        private void UpdateStatusAsync()
        {
            new TaskFactory().StartNew(() =>
            {
                //We have to wait for the order to be completed.
                Thread.Sleep(750);
                _updateOrderStatusJob.CheckStatus();
            });
        }

        private DateTime GetCurrentOffsetedTime()
        {
            //TODO : need to check ibs setup for shortesst time.

            var ibsServerTimeDifference = _configManager.GetSetting("IBS.TimeDifference").SelectOrDefault(setting => long.Parse(setting), 0);
            var offsetedTime =DateTime.Now.AddMinutes(2);
            if (ibsServerTimeDifference != 0)
            {
                offsetedTime = offsetedTime.Add(new TimeSpan(ibsServerTimeDifference));
            }

            return offsetedTime;
        }

        private int? CreateIBSOrder(AccountDetail account, CreateOrder request, ReferenceData referenceData)
        {
            // Provider is optional
            // But if a provider is specified, it must match with one of the ReferenceData values
            if (request.Settings.ProviderId.HasValue &&
                referenceData.CompaniesList.None(c => c.Id == request.Settings.ProviderId.Value))
            {
                throw new HttpError(ErrorCode.CreateOrder_InvalidProvider.ToString());
            }

            var ibsPickupAddress = Mapper.Map<IBSAddress>(request.PickupAddress);
            var ibsDropOffAddress = IsValid(request.DropOffAddress) ? Mapper.Map<IBSAddress>(request.DropOffAddress) : (IBSAddress)null;

            var note = BuildNote(request.Note, request.PickupAddress.BuildingName, request.Settings.LargeBags);
            var fare = GetFare(request.Estimate);
            var result = _bookingWebServiceClient.CreateOrder(request.Settings.ProviderId, account.IBSAccountId, request.Settings.Name, request.Settings.Phone, request.Settings.Passengers,
                request.Settings.VehicleTypeId, request.Settings.ChargeTypeId, note, request.PickupDate.Value, ibsPickupAddress, ibsDropOffAddress, fare);

            return result;
        }

        private bool IsValid(Address address)
        {
            return ((address != null) && address.FullAddress.HasValue() && address.Longitude != 0 && address.Latitude != 0);
        }

        private string BuildNote(string note, string buildingName, int largeBags)
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

                var pattern = @"
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
            var formattedNote = string.Format("{0}{1}", Environment.NewLine, note);
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

            bool vatEnabled = _configManager.GetSetting("VATIsEnabled", false);
            
            if (!vatEnabled)
            {
                return Fare.FromAmountInclTax((decimal)estimate.Price.Value, 0m);
            }

            double taxPercentage = _configManager.GetSetting("VATPercentage", 0d);
            return Fare.FromAmountInclTax((decimal)estimate.Price.Value, (decimal)taxPercentage);


        }
    }
}
