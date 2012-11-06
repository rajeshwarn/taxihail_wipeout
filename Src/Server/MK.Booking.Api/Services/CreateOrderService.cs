using System;
using System.Linq;
using Infrastructure.Messaging;
using ServiceStack.CacheAccess;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

using AutoMapper;
using ServiceStack.Common.Web;
using OrderStatusDetail = apcurium.MK.Booking.Api.Contract.Resources.OrderStatusDetail;


namespace apcurium.MK.Booking.Api.Services
{
    public class CreateOrderService : RestServiceBase<CreateOrder>
    {
        private ICommandBus _commandBus;
        private IBookingWebServiceClient _bookingWebServiceClient;
        private IStaticDataWebServiceClient _staticDataWebServiceClient;

        private IAccountDao _accountDao;
        private ICacheClient _cacheClient;
        private ReferenceDataService _referenceDataService;
        public CreateOrderService(ICommandBus commandBus,
                                    IBookingWebServiceClient bookingWebServiceClient,
                                    IStaticDataWebServiceClient staticDataWebServiceClient,
                                    IAccountDao accountDao,
                                    ICacheClient cacheClient, ReferenceDataService referenceDataService)
        {
            _commandBus = commandBus;
            _bookingWebServiceClient = bookingWebServiceClient;
            _staticDataWebServiceClient = staticDataWebServiceClient;
            _accountDao = accountDao;
            _cacheClient = cacheClient;
            _referenceDataService = referenceDataService;
        }

        public override object OnPost(CreateOrder request)
        {
            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));

            //TODO : need to check ibs setup for shortesst time.
            request.PickupDate = request.PickupDate.HasValue ? request.PickupDate.Value : DateTime.Now.AddMinutes(2);

            var ibsOrderId = CreateIBSOrder(account, request);

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
            
            var referenceData = (ReferenceData) _referenceDataService.OnGet(new ReferenceDataRequest());
            var chargeType = referenceData.PaymentsList.Where(x => x.Id == request.Settings.ChargeTypeId).Select(x => x.Display).FirstOrDefault();
            var vehicleType = referenceData.VehiclesList.Where(x => x.Id == request.Settings.VehicleTypeId).Select(x => x.Display).FirstOrDefault();

            emailCommand.Settings.ChargeType = chargeType;
            emailCommand.Settings.VehicleType = vehicleType;

            _commandBus.Send(command);
            _commandBus.Send(emailCommand);

            return new OrderStatusDetail { OrderId = command.OrderId, Status = OrderStatus.Created, IBSOrderId = ibsOrderId, IBSStatusId = "", IBSStatusDescription = "Processing your order" };
        }

        private int? CreateIBSOrder(ReadModel.AccountDetail account, CreateOrder request)
        {

            if (!request.Settings.ProviderId.HasValue)
            {
                throw new HttpError(ErrorCode.CreateOrder_NoProvider.ToString());
            }
            else if (_staticDataWebServiceClient.GetCompaniesList().None(c => c.Id == request.Settings.ProviderId.Value))
            {
                throw new HttpError(ErrorCode.CreateOrder_InvalidProvider.ToString());
            }
            else if (_staticDataWebServiceClient.GetVehiclesList(_staticDataWebServiceClient.GetCompaniesList().Single(c => c.Id == request.Settings.ProviderId.Value)).None(v => v.Id == request.Settings.VehicleTypeId))
            {
                throw new HttpError(ErrorCode.CreateOrder_VehiculeType.ToString());
            }

            var ibsPickupAddress = Mapper.Map<IBSAddress>(request.PickupAddress);
            var ibsDropOffAddress = IsValid(request.DropOffAddress) ? Mapper.Map<IBSAddress>(request.DropOffAddress) : (IBSAddress)null;

            // Building Name is not handled by IBS
            // Put Building Name in note, if specified
            var note = request.Note;
            if(!string.IsNullOrWhiteSpace(request.PickupAddress.BuildingName))
            {
                var buildingName = "Building Name: " + request.PickupAddress.BuildingName;
                note = (buildingName + Environment.NewLine + note).Trim();
            }

            var result = _bookingWebServiceClient.CreateOrder(request.Settings.ProviderId, account.IBSAccountId, request.Settings.Name, request.Settings.Phone, request.Settings.Passengers,
                                                    request.Settings.VehicleTypeId, request.Settings.ChargeTypeId, note, request.PickupDate.Value, ibsPickupAddress, ibsDropOffAddress);

            return result;
        }

        private bool IsValid(Address address)
        {
            return ((address != null) && address.FullAddress.HasValue() && address.Longitude != 0 && address.Latitude != 0);
        }

    }
}
