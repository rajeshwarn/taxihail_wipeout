using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;

namespace apcurium.MK.Booking.Api.Services
{
    public class CancelOrderService : RestServiceBase<CancelOrder>
    {
        private ICommandBus _commandBus;        
        public CancelOrderService(ICommandBus commandBus)
        {
            _commandBus = commandBus;            
            AutoMapper.Mapper.CreateMap<CancelOrder, Commands.CancelOrder>();

        }

        public override object OnPost(CancelOrder request)
        {
            var command = new Commands.CancelOrder();
            
            AutoMapper.Mapper.Map( request,  command  );
                        
            command.Id = Guid.NewGuid();
                        
            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }
    }
}
