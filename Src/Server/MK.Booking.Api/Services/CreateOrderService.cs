using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Services
{
    public class CreateOrderService : RestServiceBase<CreateOrder>
    {
        private ICommandBus _commandBus;

        public CreateOrderService(ICommandBus commandBus)
        {
            _commandBus = commandBus;

            AutoMapper.Mapper.CreateMap<CreateOrder, Commands.CreateOrder>();

        }

        public override object OnPost(CreateOrder request)
        {
            var command = new Commands.CreateOrder();
            
            AutoMapper.Mapper.Map( request,  command  );
                        
            command.Id = Guid.NewGuid();
                        
            _commandBus.Send(command);

            return new Order { Id = command.Id };            
            
        }
    }
}
