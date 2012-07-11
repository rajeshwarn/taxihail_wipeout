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


namespace apcurium.MK.Booking.Api.Services
{
    public class CreateOrderService : RestServiceBase<CreateOrder>
    {
        private ICommandBus _commandBus;        
        private IBookingWebServiceClient _bookingWebServiceClient;
        private IStaticDataWebServiceClient _staticDataWebServiceClient;
        private IAccountDao _accountDao;

        public CreateOrderService(ICommandBus commandBus, IBookingWebServiceClient bookingWebServiceClient,IStaticDataWebServiceClient staticDataWebServiceClient, IAccountDao accountDao)
        {
            _commandBus = commandBus;
            _bookingWebServiceClient = bookingWebServiceClient;
            _staticDataWebServiceClient = staticDataWebServiceClient;
            _accountDao = accountDao;
            AutoMapper.Mapper.CreateMap<CreateOrder, Commands.CreateOrder>().ForMember(p => p.OrderId, options => options.MapFrom(m => m.Id));

        }

        public override object OnPost(CreateOrder request)
        {
            var account = _accountDao.FindById(request.AccountId );
            var co =   _staticDataWebServiceClient.GetCompaniesList().Single( c=>c.Id == 13 );
            var vl = _staticDataWebServiceClient.GetVehiclesList(co);
            
            
            _bookingWebServiceClient.CreateOrder(13, 161 , "Bob Smith", "514-555-1212", 1, vl.First().Id, "Test", null, new IBS.Address { FullAddress = request.PickupAddress, Latitude = request.PickupLatitude, Longitude = request.PickupLongitude }, null);

            var command = new Commands.CreateOrder();
            
            AutoMapper.Mapper.Map( request,  command  );
                        
            command.Id = Guid.NewGuid();
                        
            _commandBus.Send(command);

            return new Order { Id = command.Id };                       
        }
    }
}
