using Infrastructure.EventSourcing;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.CommandHandlers
{
    public class CompanyCommandHandler : ICommandHandler<CreateCompany>, ICommandHandler<CreateTariff>, ICommandHandler<UpdateTariff>, ICommandHandler<DeleteTariff>, ICommandHandler<AddOrUpdateAppSettings>,
        ICommandHandler<CreateRule>, ICommandHandler<UpdateRule>, ICommandHandler<DeleteRule>, ICommandHandler<ActivateRule>, ICommandHandler<DeactivateRule>,
        ICommandHandler<AddRatingType>,
        ICommandHandler<UpdateRatingType>, ICommandHandler<HideRatingType>
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

            if (command.Type == TariffType.Default)
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


        public void Handle(CreateRule command)
        {
            var company = _repository.Get(command.CompanyId);

            
            company.CreateRule(command.RuleId,
                    command.Name,
                    command.Message,
                    command.ZoneList,
                    command.Type,
                    command.Category,
                    command.AppliesToCurrentBooking,
                    command.AppliesToFutureBooking,
                    command.Priority, 
                    command.IsActive, 
                    command.DaysOfTheWeek,
                    command.StartTime,
                    command.EndTime,
                    command.ActiveFrom,
                    command.ActiveTo 
                    );            
            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(UpdateRule command)
        {
            var company = _repository.Get(command.CompanyId);

            company.UpdateRule( command.RuleId, 
                                command.Name, 
                                command.Message, 
                                command.ZoneList,
                                command.AppliesToCurrentBooking, 
                                command.AppliesToFutureBooking, 
                                command.DaysOfTheWeek, 
                                command.StartTime, 
                                command.EndTime, 
                                command.ActiveFrom, 
                                command.ActiveTo, 
                                command.Priority, 
                                command.IsActive );

            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(DeleteRule command)
        {
            var company = _repository.Get(command.CompanyId);

            company.DeleteRule(command.RuleId);

            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(ActivateRule command)
        {
            var company = _repository.Get(command.CompanyId);

            company.ActivateRule(command.RuleId);

            _repository.Save(company, command.Id.ToString());
        }
        public void Handle(DeactivateRule command)
        {
            var company = _repository.Get(command.CompanyId);

            company.DeactivateRule(command.RuleId);

            _repository.Save(company, command.Id.ToString());
        }



        public void Handle(AddRatingType command)
        {
            var company = _repository.Get(command.CompanyId);

            company.AddRatingType(command.Name, command.RatingTypeId);

            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(UpdateRatingType command)
        {
            var company = _repository.Get(command.CompanyId);

            company.UpdateRatingType(command.Name, command.RatingTypeId);

            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(HideRatingType command)
        {
            var company = _repository.Get(command.CompanyId);

            company.HideRatingType(command.RatingTypeId);

            _repository.Save(company, command.Id.ToString());
        }
    }
}
