using System;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using Infrastructure.Messaging;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query;
using ServiceStack.Common.Web;

namespace apcurium.MK.Booking.Api.Services
{
    public class RegisterAccountService : RestServiceBase<RegisterAccount>
    {
        private ICommandBus _commandBus;
        private IAccountDao _accountDao;
        private readonly IAccountWebServiceClient _accountWebServiceClient;

        public RegisterAccountService(ICommandBus commandBus, IAccountWebServiceClient accountWebServiceClient, IAccountDao accountDao)
        {
            _commandBus = commandBus;
            _accountDao = accountDao;
            _accountWebServiceClient = accountWebServiceClient;

            AutoMapper.Mapper.CreateMap<RegisterAccount, Commands.RegisterAccount>();

        }

        public override object OnPost(RegisterAccount request)
        {
            if (_accountDao.FindByEmail(request.Email) != null)
            {

                throw new HttpError(ErrorCode.CreateAccount_AccountAlreadyExist.ToString()); 

            }

            var command = new Commands.RegisterAccount();            
            AutoMapper.Mapper.Map( request,  command  );                                    
            command.Id = Guid.NewGuid();
            command.IbsAccountId = _accountWebServiceClient.CreateAccount(command.AccountId, 
                                                                            command.Email, 
                                                                            command.FirstName,
                                                                            command.LastName, 
                                                                            command.Phone);                        
            _commandBus.Send(command);

            return new Account { Id = command.AccountId };
            
        }

    }
}
