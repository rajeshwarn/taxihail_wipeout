#region

using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.CommandHandlers
{
    public class CompanyCommandHandler :
        ICommandHandler<CreateCompany>,
        ICommandHandler<CreateTariff>,
        ICommandHandler<UpdateTariff>,
        ICommandHandler<DeleteTariff>,
        ICommandHandler<AddOrUpdateAppSettings>,
        ICommandHandler<CreateRule>,
        ICommandHandler<UpdateRule>,
        ICommandHandler<DeleteRule>,
        ICommandHandler<ActivateRule>,
        ICommandHandler<DeactivateRule>,
        ICommandHandler<AddRatingType>,
        ICommandHandler<UpdateRatingType>,
        ICommandHandler<UpdatePaymentSettings>,
        ICommandHandler<HideRatingType>,
        ICommandHandler<AddPopularAddress>,
        ICommandHandler<RemovePopularAddress>,
        ICommandHandler<UpdatePopularAddress>,
        ICommandHandler<AddDefaultFavoriteAddress>,
        ICommandHandler<RemoveDefaultFavoriteAddress>,
        ICommandHandler<UpdateDefaultFavoriteAddress>,
        ICommandHandler<UpdateTermsAndConditions>,
        ICommandHandler<RetriggerTermsAndConditions>,
        ICommandHandler<AddUpdateAccountCharge>,
        ICommandHandler<DeleteAccountCharge>,
        ICommandHandler<AddUpdateVehicleType>,
        ICommandHandler<DeleteVehicleType>
    {
        private readonly IEventSourcedRepository<Company> _repository;

        public CompanyCommandHandler(IEventSourcedRepository<Company> repository)
        {
            _repository = repository;
        }

        public void Handle(ActivateRule command)
        {
            var company = _repository.Get(command.CompanyId);
            company.ActivateRule(command.RuleId);
            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(AddOrUpdateAppSettings command)
        {
            var company = _repository.Find(command.CompanyId);
            company.AddOrUpdateAppSettings(command.AppSettings);
            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(AddRatingType command)
        {
            var company = _repository.Get(command.CompanyId);
            company.AddRatingType(command.Name, command.RatingTypeId);
            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(CreateCompany command)
        {
            var company = new Company(command.CompanyId);
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

        public void Handle(CreateTariff command)
        {
            var company = _repository.Get(command.CompanyId);

            if (command.Type == TariffType.Default)
            {
                company.CreateDefaultTariff(command.TariffId, command.Name, command.FlatRate, command.KilometricRate,
                    command.MarginOfError,
                    kilometerIncluded: command.KilometerIncluded,
                    pricePerPassenger: command.PassengerRate);
            }
            else if (command.Type == TariffType.Recurring)
            {
                company.CreateRecurringTariff(command.TariffId, command.Name, command.FlatRate, command.KilometricRate,
                    command.MarginOfError, command.PassengerRate,
                    daysOfTheWeek: command.DaysOfTheWeek,
                    kilometerIncluded: command.KilometerIncluded,
                    startTime: command.StartTime,
                    endTime: command.EndTime);
            }
            else if (command.Type == TariffType.Day)
            {
                company.CreateDayTariff(command.TariffId, command.Name, command.FlatRate, command.KilometricRate,
                    command.MarginOfError,
                    kilometerIncluded: command.KilometerIncluded,
                    pricePerPassenger: command.PassengerRate,
                    startTime: command.StartTime,
                    endTime: command.EndTime);
            }

            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(DeactivateRule command)
        {
            var company = _repository.Get(command.CompanyId);
            company.DeactivateRule(command.RuleId);
            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(DeleteRule command)
        {
            var company = _repository.Get(command.CompanyId);
            company.DeleteRule(command.RuleId);
            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(DeleteTariff command)
        {
            var company = _repository.Get(command.CompanyId);
            company.DeleteTariff(command.TariffId);
            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(HideRatingType command)
        {
            var company = _repository.Get(command.CompanyId);
            company.HideRatingType(command.RatingTypeId);
            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(UpdatePaymentSettings command)
        {
            var company = _repository.Get(command.CompanyId);
            company.UpdatePaymentSettings(command);
            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(UpdateRatingType command)
        {
            var company = _repository.Get(command.CompanyId);
            company.UpdateRatingType(command.Name, command.RatingTypeId);
            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(UpdateRule command)
        {
            var company = _repository.Get(command.CompanyId);

            company.UpdateRule(command.RuleId,
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
                command.IsActive);

            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(UpdateTariff command)
        {
            var company = _repository.Get(command.CompanyId);

            company.UpdateTariff(command.TariffId, command.Name, command.FlatRate, command.KilometricRate,
                command.MarginOfError, command.PassengerRate, command.KilometerIncluded, command.DaysOfTheWeek,
                command.StartTime, command.EndTime);

            _repository.Save(company, command.Id.ToString());
        }

        #region Popular Addresses

        public void Handle(AddPopularAddress command)
        {
            var company = _repository.Get(AppConstants.CompanyId);
            company.AddPopularAddress(command.Address);
            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(RemovePopularAddress command)
        {
            var company = _repository.Get(AppConstants.CompanyId);
            company.RemovePopularAddress(command.AddressId);
            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(UpdatePopularAddress command)
        {
            var company = _repository.Get(AppConstants.CompanyId);
            company.UpdatePopularAddress(command.Address);
            _repository.Save(company, command.Id.ToString());
        }

        #endregion

        #region Default Favorite Addresses

        public void Handle(AddDefaultFavoriteAddress command)
        {
            var company = _repository.Get(AppConstants.CompanyId);
            company.AddDefaultFavoriteAddress(command.Address);
            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(RemoveDefaultFavoriteAddress command)
        {
            var company = _repository.Get(AppConstants.CompanyId);
            company.RemoveDefaultFavoriteAddress(command.AddressId);
            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(UpdateDefaultFavoriteAddress command)
        {
            var company = _repository.Get(AppConstants.CompanyId);
            company.UpdateDefaultFavoriteAddress(command.Address);
            _repository.Save(company, command.Id.ToString());
        }

        #endregion

        public void Handle(UpdateTermsAndConditions command)
        {
            var company = _repository.Get(command.CompanyId);

            company.UpdateTermsAndConditions(command.TermsAndConditions);

            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(RetriggerTermsAndConditions command)
        {
            var company = _repository.Get(command.CompanyId);

            company.RetriggerTermsAndConditions();

            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(AddUpdateAccountCharge command)
        {
            var company = _repository.Get(command.CompanyId);

            company.AddUpdateAccountCharge(command.AccountChargeId, command.Number, command.Name, command.Questions);

            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(DeleteAccountCharge command)
        {
            var company = _repository.Get(command.CompanyId);

            company.DeleteAccountCharge(command.AccountChargeId);

            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(AddUpdateVehicleType command)
        {
            var company = _repository.Get(command.CompanyId);

            company.AddUpdateVehicleType(command.VehicleTypeId, command.Name, command.LogoName, command.ReferenceDataVehicleId);

            _repository.Save(company, command.Id.ToString());
        }

        public void Handle(DeleteVehicleType command)
        {
            var company = _repository.Get(command.CompanyId);

            company.DeleteVehicleType(command.VehicleTypeId);

            _repository.Save(company, command.Id.ToString());
        }
    }
}