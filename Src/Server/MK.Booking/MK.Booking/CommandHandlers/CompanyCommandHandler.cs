using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.CommandHandlers
{
    public class CompanyCommandHandler : ICommandHandler<CreateCompany>, ICommandHandler<AddAppSettings>, ICommandHandler<UpdateAppSettings>
    {

        private readonly IEventSourcedRepository<Company> _repository;

        public CompanyCommandHandler(IEventSourcedRepository<Company> repository)
        {
            _repository = repository;
        }

        public void Handle(CreateCompany command)
        {

            var company = new Company(command.CompanyId);
            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(AddAppSettings command)
        {
            var company = _repository.Find(AppConstants.CompanyId);
            company.AddAppSettings(command.Key, command.Value);
            _repository.Save(company,command.Id.ToString());
        }

        public void Handle(UpdateAppSettings command)
        {
            var company = _repository.Find(AppConstants.CompanyId);
            company.UpdateAppSettings(command.Key, command.Value);
            _repository.Save(company, command.Id.ToString());
        }
    }
}
