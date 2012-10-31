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
    public class CompanyCommandHandler : ICommandHandler<CreateCompany>, ICommandHandler<CreateRate>, ICommandHandler<UpdateRate>, ICommandHandler<DeleteRate>, ICommandHandler<AddAppSettings>, ICommandHandler<UpdateAppSettings>
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
            var company = _repository.Find(command.CompanyId);
            company.AddAppSettings(command.Key, command.Value);
            _repository.Save(company,command.Id.ToString());
        }

        public void Handle(UpdateAppSettings command)
        {
            var company = _repository.Find(command.CompanyId);
            company.UpdateAppSettings(command.Key, command.Value);
            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(CreateRate command)
        {
            var company = _repository.Get(command.CompanyId);

            if(command.Type == RateType.Default)
            {
                company.CreateDefaultRate(rateId: command.RateId,
                    name: command.Name,
                    flatRate: command.FlatRate,
                    distanceMultiplicator: command.DistanceMultiplicator,
                    timeAdustmentFactor: command.TimeAdjustmentFactor,
                    pricePerPassenger: command.PricePerPassenger);
            } 
            else if (command.Type == RateType.Recurring)
            {
                company.CreateRecurringRate(rateId: command.RateId,
                    name: command.Name,
                    flatRate: command.FlatRate,
                    distanceMultiplicator: command.DistanceMultiplicator,
                    timeAdustmentFactor: command.TimeAdjustmentFactor,
                    pricePerPassenger: command.PricePerPassenger,
                    daysOfTheWeek: command.DaysOfTheWeek,
                    startTime: command.StartTime,
                    endTime: command.EndTime);
            }
            else if (command.Type == RateType.Day)
            {
                company.CreateDayRate(rateId: command.RateId,
                    name: command.Name,
                    flatRate: command.FlatRate,
                    distanceMultiplicator: command.DistanceMultiplicator,
                    timeAdustmentFactor: command.TimeAdjustmentFactor,
                    pricePerPassenger: command.PricePerPassenger,
                    startTime: command.StartTime,
                    endTime: command.EndTime);
            }

            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(UpdateRate command)
        {
            var company = _repository.Get(command.CompanyId);

            company.UpdateRate(rateId: command.RateId,
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

        public void Handle(DeleteRate command)
        {
            var company = _repository.Get(command.CompanyId);

            company.DeleteRate(command.RateId);

            _repository.Save(company, command.Id.ToString());
        }
        
    }
}
