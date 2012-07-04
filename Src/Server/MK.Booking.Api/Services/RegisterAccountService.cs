using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceInterface;

using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking;
using Infrastructure.Messaging;
using ServiceStack.ServiceInterface.Auth;

namespace apcurium.MK.Booking.Api.Services
{
    public class RegisterAccountService : RestServiceBase<RegisterAccount>
    {
        private ICommandBus _commandBus;
        public RegisterAccountService(ICommandBus commandBus)
        {
            _commandBus = commandBus;

            AutoMapper.Mapper.CreateMap<RegisterAccount, Commands.RegisterAccount>();

        }

        public override object OnPost(RegisterAccount request)
        {
            var command = new Commands.RegisterAccount();
            
            AutoMapper.Mapper.Map( request,  command  );
                        
            command.Id = Guid.NewGuid();
                        
            _commandBus.Send(command);

            return new Account { Id = command.AccountId };            
            
        }

    }
}
