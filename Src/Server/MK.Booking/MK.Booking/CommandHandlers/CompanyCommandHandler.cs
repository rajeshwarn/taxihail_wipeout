using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;

namespace apcurium.MK.Booking.CommandHandlers
{
    public class CompanyCommandHandler : ICommandHandler<CreateCompany>
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
    }
}
