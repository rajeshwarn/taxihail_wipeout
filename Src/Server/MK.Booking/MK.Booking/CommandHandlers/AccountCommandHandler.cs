using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using Infrastructure.EventSourcing;
using apcurium.MK.Booking.Security;

namespace apcurium.MK.Booking.CommandHandlers
{


    public class AccountCommandHandler : ICommandHandler<RegisterAccount>, 
                                         ICommandHandler<ResetAccountPassword>,
                                         ICommandHandler<UpdateAccount>,
                                         ICommandHandler<UpdateBookingSettings>
    {

        private readonly IEventSourcedRepository<Account> _repository;
        private readonly IPasswordService _passwordService;

        public AccountCommandHandler(IEventSourcedRepository<Account> repository, IPasswordService passwordService)
        {
            _repository = repository;
            _passwordService = passwordService;
        }

        public void Handle(RegisterAccount command)
        {
            var password = _passwordService.EncodePassword(command.Password, command.AccountId.ToString());
            var account = new Account(command.AccountId, command.Name, command.Phone, command.Email, password, command.IbsAccountId);
            _repository.Save(account);
        }

        public void Handle(UpdateAccount command)
        {
            var account = _repository.Find(command.AccountId);
            account.Update(command.Name);
            _repository.Save(account);
            
        }

        public void Handle(ResetAccountPassword command)
        {
            var account = _repository.Find(command.AccountId);
            var newPassword = _passwordService.EncodePassword(command.Password, command.AccountId.ToString());
            account.UpdatePassword(newPassword);
            _repository.Save(account);
        }
        
        public void Handle(UpdateBookingSettings command)
        {
            var account = _repository.Find(command.AccountId);

            var settings = new BookingSettings();
            AutoMapper.Mapper.Map(command, settings);
            account.UpdateBookingSettings(settings);
            _repository.Save(account);
        }

    }
}
