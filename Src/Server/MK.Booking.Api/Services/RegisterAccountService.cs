using System;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using Infrastructure.Messaging;
using apcurium.MK.Booking.IBS;

namespace apcurium.MK.Booking.Api.Services
{
    public class RegisterAccountService : RestServiceBase<RegisterAccount>
    {
        private ICommandBus _commandBus;
        private readonly IWebServiceClient _webServiceClient;

        public RegisterAccountService(ICommandBus commandBus, IWebServiceClient webServiceClient)
        {
            _commandBus = commandBus;
            _webServiceClient = webServiceClient;

            AutoMapper.Mapper.CreateMap<RegisterAccount, Commands.RegisterAccount>();

        }

        public override object OnPost(RegisterAccount request)
        {
            var command = new Commands.RegisterAccount();
            
            AutoMapper.Mapper.Map( request,  command  );
                        
            command.Id = Guid.NewGuid();
            command.IbsAccountId = _webServiceClient.CreateAccount(command.AccountId, command.Email, command.FirstName,
                                                                   command.LastName, command.Phone);
                        
            _commandBus.Send(command);

            return new Account { Id = command.AccountId };            
            
        }

    }
}
