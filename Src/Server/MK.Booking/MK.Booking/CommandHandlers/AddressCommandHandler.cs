
using Infrastructure.EventSourcing;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;

namespace apcurium.MK.Booking.BackOffice.CommandHandlers
{
    public class AddressCommandHandler : ICommandHandler<AddFavoriteAddress>, ICommandHandler<RemoveFavoriteAddress>, ICommandHandler<UpdateFavoriteAddress>, ICommandHandler<RemoveAddressFromHistory>
    {
        private readonly IEventSourcedRepository<Account> _repository;

        public AddressCommandHandler(IEventSourcedRepository<Account> repository)
        {
            _repository = repository;
        }

        public void Handle(AddFavoriteAddress command)
        {
            var account = _repository.Get(command.AccountId);

            account.AddFavoriteAddress(id: command.AddressId, 
                friendlyName: command.FriendlyName,
                apartment: command.Apartment,
                fullAddress: command.FullAddress,
                ringCode: command.RingCode,
                latitude: command.Latitude,
                longitude: command.Longitude);
            
            _repository.Save(account ,command.Id.ToString());
        }

        public void Handle(RemoveFavoriteAddress command)
        {
            var account = _repository.Get(command.AccountId);

            account.RemoveFavoriteAddress(command.AddressId);

            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(UpdateFavoriteAddress command)
        {
            var account = _repository.Get(command.AccountId);

            account.UpdateFavoriteAddress(id: command.AddressId,
                friendlyName: command.FriendlyName,
                apartment: command.Apartment,
                fullAddress: command.FullAddress,
                ringCode: command.RingCode,
                latitude: command.Latitude,
                longitude: command.Longitude);

            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(RemoveAddressFromHistory command)
        {
            var account = _repository.Get(command.AccountId);
            account.RemoveAddressFromHistory(command.AddressId);
            _repository.Save(account, command.Id.ToString());
        }
    }
}