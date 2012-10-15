using System;
using System.Linq;
using Infrastructure.Messaging;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

using AutoMapper;
using ServiceStack.Common.Web;


namespace apcurium.MK.Booking.Api.Services
{
    public class CreateOrderService : RestServiceBase<CreateOrder>
    {
        private ICommandBus _commandBus;
        private IBookingWebServiceClient _bookingWebServiceClient;
        private IStaticDataWebServiceClient _staticDataWebServiceClient;

        private IAccountDao _accountDao;

        public CreateOrderService(ICommandBus commandBus,
                                    IBookingWebServiceClient bookingWebServiceClient,
                                    IStaticDataWebServiceClient staticDataWebServiceClient,
                                    IAccountDao accountDao)
        {
            _commandBus = commandBus;
            _bookingWebServiceClient = bookingWebServiceClient;
            _staticDataWebServiceClient = staticDataWebServiceClient;
            _accountDao = accountDao;
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

            var command = new Commands.CreateOrder();

            Mapper.Map(request, command);

            command.IBSOrderId = ibsOrderId.Value;
            command.AccountId = account.Id;

            _commandBus.Send(command);

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
