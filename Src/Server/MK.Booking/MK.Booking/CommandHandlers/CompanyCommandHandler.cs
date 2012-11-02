using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.CommandHandlers
{
    public class CompanyCommandHandler : ICommandHandler<CreateCompany>, ICommandHandler<CreateTariff>, ICommandHandler<UpdateTariff>, ICommandHandler<DeleteTariff>, ICommandHandler<AddOrUpdateAppSettings>
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

        public void Handle(AddOrUpdateAppSettings command)
        {
            var company = _repository.Find(command.CompanyId);
            company.AddOrUpdateAppSettings(command.AppSettings);
            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(CreateTariff command)
        {
            var company = _repository.Get(command.CompanyId);

            if(command.Type == TariffType.Default)
            {
                company.CreateDefaultTariff(tariffId: command.TariffId,
                    name: command.Name,
                    flatRate: command.FlatRate,
                    distanceMultiplicator: command.KilometricRate,
                    timeAdustmentFactor: command.MarginOfError,
                    pricePerPassenger: command.PassengerRate);
            } 
            else if (command.Type == TariffType.Recurring)
            {
                company.CreateRecurringTariff(tariffId: command.TariffId,
                    name: command.Name,
                    flatRate: command.FlatRate,
                    distanceMultiplicator: command.KilometricRate,
                    timeAdustmentFactor: command.MarginOfError,
                    pricePerPassenger: command.PassengerRate,
                    daysOfTheWeek: command.DaysOfTheWeek,
                    startTime: command.StartTime,
                    endTime: command.EndTime);
            }
            else if (command.Type == TariffType.Day)
            {
                company.CreateDayTariff(tariffId: command.TariffId,
                    name: command.Name,
                    flatRate: command.FlatRate,
                    distanceMultiplicator: command.KilometricRate,
                    timeAdustmentFactor: command.MarginOfError,
                    pricePerPassenger: command.PassengerRate,
                    startTime: command.StartTime,
                    endTime: command.EndTime);
            }

            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(UpdateTariff command)
        {
            var company = _repository.Get(command.CompanyId);

            company.UpdateTariff(tariffId: command.TariffId,
                    name: command.Name,
                    flatRate: command.FlatRate,
                    distanceMultiplicator: command.KilometricRate,
                    timeAdustmentFactor: command.MarginOfError,
                    pricePerPassenger: command.PassengerRate,
                    daysOfTheWeek: command.DaysOfTheWeek,
                    startTime: command.StartTime,
                    endTime: command.EndTime);

            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(DeleteTariff command)
        {
            var company = _repository.Get(command.CompanyId);

            company.DeleteTariff(command.TariffId);

            _repository.Save(company, command.Id.ToString());
        }
        
    }
}
