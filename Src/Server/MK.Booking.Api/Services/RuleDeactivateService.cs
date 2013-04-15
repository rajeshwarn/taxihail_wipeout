using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query;
using AutoMapper;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace apcurium.MK.Booking.Api.Services
{
    public class RuleDeactivateService : RestServiceBase<RuleDeactivateRequest>
    {
        private readonly ICommandBus _commandBus;

        public RuleDeactivateService(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }



        public override object OnPost(RuleDeactivateRequest request)
        {            
            var command = Mapper.Map<DeactivateRule>(request);
         
            _commandBus.Send(  command );
            
            return new
            {
                Id = command.RuleId
            };
        }


    }
}
