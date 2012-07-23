using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query;
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
        private GeocodingService _geocodingService;
        private IAccountDao _accountDao;
        private OrderStatusService _statusService;
        public CreateOrderService(ICommandBus commandBus, IBookingWebServiceClient bookingWebServiceClient,IStaticDataWebServiceClient staticDataWebServiceClient, IAccountDao accountDao, GeocodingService geocodingService, OrderStatusService statusService )
        {
            _statusService = statusService;
            _commandBus = commandBus;
            _bookingWebServiceClient = bookingWebServiceClient;
            _staticDataWebServiceClient = staticDataWebServiceClient;
            _accountDao = accountDao;
            _geocodingService = geocodingService;

        }

        public override object OnPost(CreateOrder request)
        {
            var account = _accountDao.FindById(request.AccountId );

            if (!IsValid(request.PickupAddress))
            {
                throw new HttpError(ErrorCode.CreateOrder_InvalidPickupAddress.ToString()); 
            }

            //TODO : need to check ibs setup for shortesst time.
            request.PickupDate = request.PickupDate.HasValue ? request.PickupDate.Value : DateTime.Now.AddMinutes(2);            
            
            var ibsOrderId = CreateIBSOrder(account, request);

            if (!ibsOrderId.HasValue)
            {
                throw new HttpError(ErrorCode.CreateOrder_CannotCreateInIbs.ToString()); 
            }

            var command = new Commands.CreateOrder();
            
            Mapper.Map( request,  command  );
                        
            command.Id = Guid.NewGuid();

            command.IBSOrderId = ibsOrderId.Value;
            
            _commandBus.Send(command);
            
            return new OrderStatusDetail { OrderId = request.Id, Status = OrderStatus.Created, IBSOrderId = ibsOrderId, IBSStatusId = "", IBSStatusDescription = "Processing your order"};  
        } 

        private int? CreateIBSOrder(ReadModel.AccountDetail account, CreateOrder request)
        {                       
            var ibsPickupAddress = Mapper.Map<IBSAddress>(request.PickupAddress);
            var ibsDropOffAddress = IsValid(request.DropOffAddress) ? Mapper.Map<IBSAddress>(request.PickupAddress) : (IBSAddress) null;

            var result = _bookingWebServiceClient.CreateOrder(request.Settings.ProviderId, account.IBSAccountId, request.Settings.Name, request.Settings.Phone, request.Settings.Passengers,
                                                    request.Settings.VehicleTypeId, request.Note, request.PickupDate.Value, ibsPickupAddress, ibsDropOffAddress);

            return result;

            
        }

        private bool IsValid(Address address)
        {
            return ((address != null) && address.FullAddress.HasValue() && address.Longitude != 0 && address.Latitude != 0);            
        }

    }
}
