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
    public class CompanyCommandHandler : ICommandHandler<CreateCompany>, ICommandHandler<CreateRate>
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

        public void Handle(CreateRate command)
        {
            var company = _repository.Get(command.CompanyId);

            company.CreateRate(rateId: command.RateId,
                name: command.Name,
                flatRate: command.FlatRate,
                distanceMultiplicator: command.DistanceMultiplicator,
                timeAdustmentFactor: command.TimeAdjustmentFactor,
                pricePerPassenger: command.PricePerPassenger,
                daysOfTheWeek: command.DaysOfTheWeek,
                startTime: command.StartTime,
                endTime: command.EndTime);

            _repository.Save(company, command.Id.ToString());
        }
    }
}
