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

        public CreateOrderService(ICommandBus commandBus, IBookingWebServiceClient bookingWebServiceClient,IStaticDataWebServiceClient staticDataWebServiceClient, IAccountDao accountDao, GeocodingService geocodingService)
        {
            _commandBus = commandBus;
            _bookingWebServiceClient = bookingWebServiceClient;
            _staticDataWebServiceClient = staticDataWebServiceClient;
            _accountDao = accountDao;
            _geocodingService = geocodingService;

            Mapper.CreateMap<CreateOrder, Commands.CreateOrder>().ForMember(p => p.OrderId, options => options.MapFrom(m => m.Id));                
            Mapper.CreateMap<Address, Commands.CreateOrder.Address>();
            Mapper.CreateMap<BookingSettings, Commands.CreateOrder.BookingSettings>();

            Mapper.CreateMap<Address, IBSAddress>();

        }

        public override object OnPost(CreateOrder request)
        {
            var account = _accountDao.FindById(request.AccountId );

            if (!IsValid(request.PickupAddress))
            {
                throw new HttpError(ErrorCode.CreateOrder_InvalidPickupAddress.ToString()); 
            }

            //CreateIbsOrder(account, request);
            
            var command = new Commands.CreateOrder();
            
            Mapper.Map( request,  command  );
                        
            command.Id = Guid.NewGuid();
                        
            _commandBus.Send(command);

            return new Order { Id = command.Id };                       
        }

        private void CreateIbsOrder(ReadModel.AccountDetail account, CreateOrder request)
        {                       
            var ibsPickupAddress = Mapper.Map<IBSAddress>(request.PickupAddress);
            var ibsDropOffAddress = IsValid(request.DropOffAddress) ? Mapper.Map<IBSAddress>(request.PickupAddress) : (IBSAddress) null;

            _bookingWebServiceClient.CreateOrder(request.Settings.ProviderId, account.IBSAccountId, request.Settings.Name, request.Settings.Phone, request.Settings.Passengers,
                                                    request.Settings.VehicleTypeId, request.Note, request.PickupDate, ibsPickupAddress, ibsDropOffAddress);
        }

        private bool IsValid(Address address)
        {
            return ((address != null) && address.FullAddress.HasValue() && address.Longitude != 0 && address.Latitude != 0);            
        }

    }
}
