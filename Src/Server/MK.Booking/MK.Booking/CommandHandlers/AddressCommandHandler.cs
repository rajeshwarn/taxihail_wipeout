
using Infrastructure.EventSourcing;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;

namespace apcurium.MK.Booking.BackOffice.CommandHandlers
{
    public class AddressCommandHandler : ICommandHandler<AddAddress>, ICommandHandler<RemoveAddress>, ICommandHandler<UpdateAddress>
    {
        private readonly IEventSourcedRepository<Account> _repository;

        public AddressCommandHandler(IEventSourcedRepository<Account> repository)
        {
            _repository = repository;
        }

        public void Handle(AddAddress command)
        {
            var account = _repository.Get(command.AccountId);

            account.AddAddress(id: command.AddressId, 
                friendlyName: command.FriendlyName,
                apartment: command.Apartment,
                fullAddress: command.FullAddress,
                ringCode: command.RingCode,
                latitude: command.Latitude,
                longitude: command.Longitude,
                isHistoric:command.IsHistoric);
            
            _repository.Save(account);
        }

        public void Handle(RemoveAddress command)
        {
            var account = _repository.Get(command.AccountId);

            account.RemoveFavoriteAddress(command.AddressId);

            _repository.Save(account);
        }

        public void Handle(UpdateAddress command)
        {
            var account = _repository.Get(command.AccountId);

            account.UpdateAddress(id: command.AddressId,
                friendlyName: command.FriendlyName,
                apartment: command.Apartment,
                fullAddress: command.FullAddress,
                ringCode: command.RingCode,
                latitude: command.Latitude,
                longitude: command.Longitude,
                isHistoric:command.IsHistoric);

            _repository.Save(account);
        }
    }
}