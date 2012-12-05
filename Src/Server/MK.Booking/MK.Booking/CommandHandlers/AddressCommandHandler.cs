
using Infrastructure.EventSourcing;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.BackOffice.CommandHandlers
{
    public class AddressCommandHandler : ICommandHandler<AddFavoriteAddress>, ICommandHandler<RemoveFavoriteAddress>, ICommandHandler<UpdateFavoriteAddress>, ICommandHandler<RemoveAddressFromHistory>, ICommandHandler<AddDefaultFavoriteAddress>, ICommandHandler<RemoveDefaultFavoriteAddress>, ICommandHandler<UpdateDefaultFavoriteAddress>
        , ICommandHandler<AddPopularAddress>, ICommandHandler<RemovePopularAddress>, ICommandHandler<UpdatePopularAddress>
    {
        private readonly IEventSourcedRepository<Account> _repository;
        private readonly IEventSourcedRepository<Company> _companyRepository;

        public AddressCommandHandler(IEventSourcedRepository<Account> repository, IEventSourcedRepository<Company> company)
        {
            _repository = repository;
            _companyRepository = company;
        }

        public void Handle(AddFavoriteAddress command)
        {
            var account = _repository.Get(command.AccountId);

            account.AddFavoriteAddress(command.Address);
            
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

            account.UpdateFavoriteAddress(command.Address);

            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(RemoveAddressFromHistory command)
        {
            var account = _repository.Get(command.AccountId);
            account.RemoveAddressFromHistory(command.AddressId);
            _repository.Save(account, command.Id.ToString());
        }

        public void Handle(AddDefaultFavoriteAddress command)
        {
            var company = _companyRepository.Get(AppConstants.CompanyId);
            company.AddDefaultFavoriteAddress(command.Address);
            _companyRepository.Save(company, command.Id.ToString());

        }

        public void Handle(RemoveDefaultFavoriteAddress command)
        {
            var company = _companyRepository.Get(AppConstants.CompanyId);

            company.RemoveDefaultFavoriteAddress(command.AddressId);

            _companyRepository.Save(company, command.Id.ToString());
        }

        public void Handle(UpdateDefaultFavoriteAddress command)
        {
            var company = _companyRepository.Get(AppConstants.CompanyId);

            company.UpdateDefaultFavoriteAddress(command.Address);

            _companyRepository.Save(company, command.Id.ToString());
        }

        public void Handle(AddPopularAddress command)
        {
            var company = _companyRepository.Get(AppConstants.CompanyId);
            company.AddPopularAddress(id: command.AddressId,
                friendlyName: command.FriendlyName,
                apartment: command.Apartment,
                fullAddress: command.FullAddress,
                ringCode: command.RingCode,
                buildingName: command.BuildingName,
                latitude: command.Latitude,
                longitude: command.Longitude);
            _companyRepository.Save(company, command.Id.ToString());
        }

        public void Handle(RemovePopularAddress command)
        {
            var company = _companyRepository.Get(AppConstants.CompanyId);

            company.RemovePopularAddress(command.AddressId);

            _companyRepository.Save(company, command.Id.ToString());
        }

        public void Handle(UpdatePopularAddress command)
        {
            var company = _companyRepository.Get(AppConstants.CompanyId);

            company.UpdatePopularAddress(id: command.AddressId,
                friendlyName: command.FriendlyName,
                apartment: command.Apartment,
                fullAddress: command.FullAddress,
                ringCode: command.RingCode,
                buildingName: command.BuildingName,
                latitude: command.Latitude,
                longitude: command.Longitude);

            _companyRepository.Save(company, command.Id.ToString());
        }
    }
}