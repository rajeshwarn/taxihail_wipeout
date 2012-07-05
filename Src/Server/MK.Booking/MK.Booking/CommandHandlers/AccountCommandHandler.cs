using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.CommandHandlers
{


    public class AccountCommandHandler : ICommandHandler<RegisterAccount>, ICommandHandler<UpdateAccount>
    {

        private readonly IEventSourcedRepository<Account> _repository;

        public AccountCommandHandler(IEventSourcedRepository<Account> repository)
        {
            _repository = repository;
        }

        public void Handle(RegisterAccount command)
        {
            var account = new Account(command.AccountId, command.FirstName, command.LastName, command.Phone, command.Email, command.Password, command.IbsAccountId);
            _repository.Save(account);
        }

        public void Handle(UpdateAccount command)
        {
            var account = _repository.Find(command.AccountId);
            account.Update(command.FirstName, command.LastName);
            _repository.Save(account);
            
        }
    }
}
