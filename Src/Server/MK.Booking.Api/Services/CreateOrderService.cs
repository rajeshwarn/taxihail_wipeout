using System;
using System.Diagnostics;
using System.Linq;
using Infrastructure.Messaging;
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


namespace apcurium.MK.Booking.Api.Services
{
    public class CreateOrderService : RestServiceBase<CreateOrder>
    {
        private ICommandBus _commandBus;
        private IBookingWebServiceClient _bookingWebServiceClient;
        private IConfigurationManager _configManager;
        private IAccountDao _accountDao;
        private ReferenceDataService _referenceDataService;
        private IStaticDataWebServiceClient _staticDataWebServiceClient;
        private IRuleCalculator _ruleCalculator;

        public CreateOrderService(ICommandBus commandBus,
                                    IBookingWebServiceClient bookingWebServiceClient,
                                    IAccountDao accountDao, 
                                    IConfigurationManager configManager,
                                    ReferenceDataService referenceDataService,
             IStaticDataWebServiceClient staticDataWebServiceClient,
            IRuleCalculator ruleCalculator)
        {
            _commandBus = commandBus;
            _bookingWebServiceClient = bookingWebServiceClient;
            _accountDao = accountDao;
            _referenceDataService = referenceDataService;
            _configManager = configManager;
            _staticDataWebServiceClient = staticDataWebServiceClient;
            _ruleCalculator = ruleCalculator;
        }

        public override object OnPost(CreateOrder request)
        {
            Trace.WriteLine("Create order request : " + request);
            var zone = _staticDataWebServiceClient.GetZoneByCoordinate(request.Settings.ProviderId, request.PickupAddress.Latitude, request.PickupAddress.Longitude);
            var rule = _ruleCalculator.GetActiveDisableFor(request.PickupDate.HasValue, request.PickupDate.HasValue ? request.PickupDate.Value : GetCurrentOffsetedTime(), zone);
          
            if (rule!= null)
            {
                var err = new HttpError(  HttpStatusCode.Forbidden, ErrorCode.CreateOrder_RuleDisable.ToString(), rule.Message);                
                throw err;
            }

            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));

            //TODO: Fix this when IBS will accept more than 10 digits phone numbers
            //Send 10 digits maximum to IBS
            request.Settings.Phone = new string(request.Settings.Phone.Where(Char.IsDigit).Reverse().Take(10).Reverse().ToArray());

            var referenceData = (ReferenceData)_referenceDataService.OnGet(new ReferenceDataRequest());

            request.PickupDate = request.PickupDate.HasValue ? request.PickupDate.Value : GetCurrentOffsetedTime() ;

            request.Settings.Passengers = request.Settings.Passengers <= 0 ? 1 : request.Settings.Passengers; 

            var ibsOrderId = CreateIBSOrder(account, request, referenceData);

            if (!ibsOrderId.HasValue
                || ibsOrderId <= 0)
            {
                string code = !ibsOrderId.HasValue || (ibsOrderId.Value >= -1) ? "" : "_" + Math.Abs(ibsOrderId.Value).ToString();
                throw new HttpError(ErrorCode.CreateOrder_CannotCreateInIbs.ToString() + code);
            }

            var command = Mapper.Map<Commands.CreateOrder>(request);
            var emailCommand = Mapper.Map<Commands.SendBookingConfirmationEmail>(request);

            command.IBSOrderId = emailCommand.IBSOrderId = ibsOrderId.Value;
            command.AccountId = account.Id;
            emailCommand.EmailAddress = account.Email;

            // Get Charge Type and Vehicle Type from reference data
            var chargeType = referenceData.PaymentsList.Where(x => x.Id == request.Settings.ChargeTypeId).Select(x => x.Display).FirstOrDefault();
            var vehicleType = referenceData.VehiclesList.Where(x => x.Id == request.Settings.VehicleTypeId).Select(x => x.Display).FirstOrDefault();

            emailCommand.Settings.ChargeType = chargeType;
            emailCommand.Settings.VehicleType = vehicleType;

            _commandBus.Send(command);
            if (bool.Parse(_configManager.GetSetting("Booking.ConfirmationEmail")))
            {
                _commandBus.Send(emailCommand);
            }

            return new OrderStatusDetail { OrderId = command.OrderId, Status = OrderStatus.Created, IBSOrderId = ibsOrderId, IBSStatusId = "", IBSStatusDescription = "Processing your order" };
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

            var note = BuildNote(request.Note, request.PickupAddress.BuildingName);

            var result = _bookingWebServiceClient.CreateOrder(request.Settings.ProviderId, account.IBSAccountId, request.Settings.Name, request.Settings.Phone, request.Settings.Passengers,
                                                    request.Settings.VehicleTypeId, request.Settings.ChargeTypeId, note, request.PickupDate.Value, ibsPickupAddress, ibsDropOffAddress);

            return result;
        }

        private bool IsValid(Address address)
        {
            return ((address != null) && address.FullAddress.HasValue() && address.Longitude != 0 && address.Latitude != 0);
        }

        private string BuildNote(string note, string buildingName)
        {
            var noteTemplate = _configManager.GetSetting("IBS.NoteTemplate");

            if (!string.IsNullOrWhiteSpace(noteTemplate))
            {
                return noteTemplate
                    .Replace("\\r", "\r")
                    .Replace("\\n", "\n")
                    .Replace("\\t", "\t")
                    .Replace("{{userNote}}", note ?? string.Empty)
                    .Replace("{{buildingName}}", buildingName ?? string.Empty)
                    .Trim();
            }

            // Building Name is not handled by IBS
            // Put Building Name in note, if specified
            var formattedNote = string.Format("{0}{1}", Environment.NewLine, note);
            if (!string.IsNullOrWhiteSpace(buildingName))
            {
                formattedNote += (Environment.NewLine + buildingName).Trim();
            }
            return formattedNote;
        }
    }
}
