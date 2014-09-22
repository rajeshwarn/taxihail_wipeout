#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using Infrastructure.EventSourcing;
using MK.Common.Configuration;

#endregion

namespace apcurium.MK.Booking.Domain
{
    public class Company : EventSourced
    {
        private readonly Dictionary<RuleCategory, Guid> _defaultRules = new Dictionary<RuleCategory, Guid>();
        private Guid? _defaultTariffId;


        public Company(Guid id)
            : base(id)
        {
            RegisterHandlers();
            Update(new CompanyCreated
            {
                SourceId = id,
            });
        }

        public Company(Guid id, IEnumerable<IVersionedEvent> history)
            : base(id)
        {
            RegisterHandlers();
            LoadFrom(history);
        }

        protected PaymentMethod PaymentMode { get; set; }

        private void RegisterHandlers()
        {
            Handles<DefaultFavoriteAddressAdded>(NoAction);
            Handles<DefaultFavoriteAddressRemoved>(NoAction);
            Handles<DefaultFavoriteAddressUpdated>(NoAction);

            Handles<PopularAddressAdded>(NoAction);
            Handles<PopularAddressRemoved>(NoAction);
            Handles<PopularAddressUpdated>(NoAction);

            Handles<CompanyCreated>(NoAction);
            Handles<AppSettingsAddedOrUpdated>(NoAction);
            Handles<PaymentModeChanged>(NoAction);
            Handles<PaymentSettingUpdated>(OnPaymentSettingUpdated);

            Handles<TariffCreated>(OnRateCreated);
            Handles<TariffUpdated>(NoAction);
            Handles<TariffDeleted>(NoAction);

            Handles<RuleCreated>(OnRuleCreated);
            Handles<RuleUpdated>(NoAction);
            Handles<RuleDeleted>(NoAction);
            Handles<RuleActivated>(NoAction);
            Handles<RuleDeactivated>(NoAction);

            Handles<RatingTypeAdded>(NoAction);
            Handles<RatingTypeHidded>(NoAction);
            Handles<RatingTypeUpdated>(NoAction);

            Handles<TermsAndConditionsUpdated>(NoAction);
            Handles<TermsAndConditionsRetriggered>(NoAction);

            Handles<AccountChargeAddedUpdated>(NoAction);
            Handles<AccountChargeDeleted>(NoAction);

            Handles<VehicleTypeAddedUpdated>(NoAction);
            Handles<VehicleTypeDeleted>(NoAction);

            Handles<NotificationSettingsAddedOrUpdated>(NoAction);
        }

        private void OnPaymentSettingUpdated(PaymentSettingUpdated obj)
        {
            PaymentMode = obj.ServerPaymentSettings.PaymentMode;
        }

        public void AddDefaultFavoriteAddress(Address address)
        {
            ValidateFavoriteAddress(address.FriendlyName, address.FullAddress, address.Latitude, address.Longitude);

            Update(new DefaultFavoriteAddressAdded
            {
                Address = address
            });
        }

        public void UpdateDefaultFavoriteAddress(Address address)
        {
            ValidateFavoriteAddress(address.FriendlyName, address.FullAddress, address.Latitude, address.Longitude);

            Update(new DefaultFavoriteAddressUpdated
            {
                Address = address
            });
        }

        public void RemoveDefaultFavoriteAddress(Guid addressId)
        {
            Update(new DefaultFavoriteAddressRemoved
            {
                AddressId = addressId
            });
        }

        public void AddPopularAddress(Address address)
        {
            ValidateFavoriteAddress(address.FriendlyName, address.FullAddress, address.Latitude, address.Longitude);

            Update(new PopularAddressAdded
            {
                Address = address
            });
        }

        public void UpdatePopularAddress(Address address)
        {
            ValidateFavoriteAddress(address.FriendlyName, address.FullAddress, address.Latitude, address.Longitude);

            Update(new PopularAddressUpdated
            {
                Address = address
            });
        }

        public void RemovePopularAddress(Guid addressId)
        {
            Update(new PopularAddressRemoved
            {
                AddressId = addressId
            });
        }

        public void AddOrUpdateAppSettings(IDictionary<string, string> appSettings)
        {
            Update(new AppSettingsAddedOrUpdated
            {
                AppSettings = appSettings
            });
        }

        public void AddRatingType(string name, Guid ratingTypeId)
        {
            if (name.IsNullOrEmpty())
                throw new ArgumentException("Rating name cannot be null or empty");

            Update(new RatingTypeAdded
            {
                Name = name,
                RatingTypeId = ratingTypeId
            });
        }

        public void UpdateRatingType(string name, Guid ratingTypeId)
        {
            if (name.IsNullOrEmpty())
                throw new ArgumentException("Rating name cannot be null or empty");

            Update(new RatingTypeUpdated
            {
                Name = name,
                RatingTypeId = ratingTypeId
            });
        }

        public void HideRatingType(Guid ratingTypeId)
        {
            Update(new RatingTypeHidded
            {
                RatingTypeId = ratingTypeId
            });
        }

        public void CreateDefaultTariff(Guid tariffId, string name, decimal flatRate, double distanceMultiplicator, double perMinuteRate,
            double timeAdustmentFactor, double kilometerIncluded, double minimumRate)
        {
            if (_defaultTariffId.HasValue)
            {
                throw new InvalidOperationException("Only one default tariff can be created");
            }

            // TODO: Save default value for "all" in vehicleTypeId

            Update(new TariffCreated
            {
                TariffId = tariffId,
                Type = TariffType.Default,
                Name = name,
                MinimumRate = minimumRate,
                FlatRate = flatRate,
                KilometricRate = distanceMultiplicator,
                KilometerIncluded = kilometerIncluded,
                MarginOfError = timeAdustmentFactor,
                PerMinuteRate = perMinuteRate
            });
        }

        public void CreateDefaultVehiculeTariff(Guid tariffId, string name, decimal flatRate, double distanceMultiplicator, double perMinuteRate,
            double timeAdustmentFactor, double kilometerIncluded, int? vehicleTypeId, double minimumRate)
        {
            Update(new TariffCreated
            {
                TariffId = tariffId,
                Type = TariffType.VehicleDefault,
                Name = name,
                MinimumRate = minimumRate,
                FlatRate = flatRate,
                KilometricRate = distanceMultiplicator,
                KilometerIncluded = kilometerIncluded,
                MarginOfError = timeAdustmentFactor,
                PerMinuteRate = perMinuteRate,
                VehicleTypeId = vehicleTypeId
            });
        }

        public void CreateRecurringTariff(Guid tariffId, string name, decimal flatRate, double distanceMultiplicator, double perMinuteRate,
            double timeAdustmentFactor, double kilometerIncluded, DayOfTheWeek daysOfTheWeek,
            DateTime startTime, DateTime endTime, int? vehicleTypeId, double minimumRate)
        {
            Update(new TariffCreated
            {
                TariffId = tariffId,
                Type = TariffType.Recurring,
                Name = name,
                MinimumRate = minimumRate,
                FlatRate = flatRate,
                KilometricRate = distanceMultiplicator,
                MarginOfError = timeAdustmentFactor,
                KilometerIncluded = kilometerIncluded,
                PerMinuteRate = perMinuteRate,
                DaysOfTheWeek = daysOfTheWeek,
                StartTime = startTime,
                EndTime = endTime,
                VehicleTypeId = vehicleTypeId
            });
        }

        public void CreateDayTariff(Guid tariffId, string name, decimal flatRate, double distanceMultiplicator, double perMinuteRate,
            double timeAdustmentFactor, double kilometerIncluded, DateTime startTime,
            DateTime endTime, int? vehicleTypeId, double minumumRate)
        {
            Update(new TariffCreated
            {
                TariffId = tariffId,
                Type = TariffType.Day,
                Name = name,
                MinimumRate = minumumRate,
                FlatRate = flatRate,
                KilometricRate = distanceMultiplicator,
                PerMinuteRate = perMinuteRate,
                KilometerIncluded = kilometerIncluded,
                MarginOfError = timeAdustmentFactor,
                StartTime = startTime,
                EndTime = endTime,
                VehicleTypeId = vehicleTypeId
            });
        }

        public void UpdateTariff(Guid tariffId, string name, decimal flatRate, double distanceMultiplicator, double perMinuteRate,
            double timeAdustmentFactor, double kilometerIncluded, DayOfTheWeek daysOfTheWeek,
            DateTime startTime, DateTime endTime, int? vehicleTypeId, double minimumRate)
        {
            Update(new TariffUpdated
            {
                TariffId = tariffId,
                Name = name,
                MinimumRate = minimumRate,
                FlatRate = flatRate,
                KilometricRate = distanceMultiplicator,
                PerMinuteRate = perMinuteRate,
                MarginOfError = timeAdustmentFactor,
                KilometerIncluded = kilometerIncluded,
                DaysOfTheWeek = daysOfTheWeek,
                StartTime = startTime,
                EndTime = endTime,
                VehicleTypeId = vehicleTypeId
            });
        }

        public void DeleteTariff(Guid tariffId)
        {
            if (tariffId == _defaultTariffId)
            {
                throw new InvalidOperationException("Cannot delete default tariff");
            }
            Update(new TariffDeleted
            {
                TariffId = tariffId
            });
        }

        public void CreateRule(Guid ruleId, string name, string message, string zoneList,bool zoneRequired, RuleType type,
            RuleCategory category, bool appliedToCurrentBooking, bool appliesToFutureBooking,bool appliesToPickup, bool appliesToDropoff, int priority,
            bool isActive, DayOfTheWeek daysOfTheWeek, DateTime? startTime, DateTime? endTime, DateTime? activeFrom,
            DateTime? activeTo)
        {
            if ((type == RuleType.Default) && message.IsNullOrEmpty())
            {
                throw new InvalidOperationException(string.Format("Missing message for default rule - category {0}",
                    category));
            }
            if ((type == RuleType.Recurring) &&
                (Params.Get(message, name).Any(s => s.IsNullOrEmpty()) || (daysOfTheWeek == DayOfTheWeek.None) ||
                 (!startTime.HasValue) || (!endTime.HasValue)))
            {
                throw new InvalidOperationException("Missing message for recurrring rule");
            }
            if ((type == RuleType.Date) &&
                (Params.Get(message, name).Any(s => s.IsNullOrEmpty()) || (!activeFrom.HasValue) ||
                 (!activeFrom.HasValue)))
            {
                throw new InvalidOperationException("Missing message for date rule");
            }


            Update(new RuleCreated
            {
                RuleId = ruleId,
                Type = type,
                Name = name,
                Message = message,
                ZoneList = zoneList,
                ZoneRequired = zoneRequired,
                Category = category,
                AppliesToCurrentBooking = appliedToCurrentBooking,
                AppliesToFutureBooking = appliesToFutureBooking,
                AppliesToPickup = appliesToPickup,
                AppliesToDropoff = appliesToDropoff,
                IsActive = isActive,
                DaysOfTheWeek = daysOfTheWeek,
                StartTime = startTime,
                EndTime = endTime,
                ActiveFrom = activeFrom,
                ActiveTo = activeTo,
                Priority = /*type == RuleType.Default ? 0 :*/ priority,
            });
        }

        public void UpdateRule(Guid ruleId, string name, string message, string zoneList, bool zoneRequired, bool appliedToCurrentBooking,
            bool appliesToFutureBooking, bool appliesToPickup, bool appliesToDropoff, DayOfTheWeek daysOfTheWeek, DateTime? startTime, DateTime? endTime,
            DateTime? activeFrom, DateTime? activeTo, int priority, bool isActive)
        {
            Update(new RuleUpdated
            {
                RuleId = ruleId,
                Name = name,
                Message = message,
                ZoneList = zoneList,
                ZoneRequired = zoneRequired,
                AppliesToCurrentBooking = appliedToCurrentBooking,
                AppliesToFutureBooking = appliesToFutureBooking,
                AppliesToPickup = appliesToPickup,
                AppliesToDropoff = appliesToDropoff,
                IsActive = isActive,
                DaysOfTheWeek = daysOfTheWeek,
                StartTime = startTime,
                EndTime = endTime,
                ActiveFrom = activeFrom,
                ActiveTo = activeTo,
                Priority = priority,
            });
        }

        public void DeleteRule(Guid ruleId)
        {
            Update(new RuleDeleted
            {
                RuleId = ruleId
            });
        }

        public void UpdatePaymentSettings(UpdatePaymentSettings command)
        {
            if (PaymentMode != command.ServerPaymentSettings.PaymentMode &&
                !GoingFromCmtToRidelinqOrRidelinqToCmt(PaymentMode, command.ServerPaymentSettings.PaymentMode))
            {
                Update(new PaymentModeChanged());
            }

            Update(new PaymentSettingUpdated
            {
                ServerPaymentSettings = command.ServerPaymentSettings
            });
        }

        private bool GoingFromCmtToRidelinqOrRidelinqToCmt(PaymentMethod original, PaymentMethod newMethod)
        {
            return ((original == PaymentMethod.Cmt || original == PaymentMethod.RideLinqCmt) &&
                    (newMethod == PaymentMethod.Cmt || newMethod == PaymentMethod.RideLinqCmt));
        }

        public void ActivateRule(Guid ruleId)
        {
            Update(new RuleActivated
            {
                RuleId = ruleId
            });
        }

        public void DeactivateRule(Guid ruleId)
        {
            Update(new RuleDeactivated
            {
                RuleId = ruleId
            });
        }

        private static void ValidateFavoriteAddress(string friendlyName, string fullAddress, double latitude,
            double longitude)
        {
            if (Params.Get(friendlyName, fullAddress).Any(string.IsNullOrEmpty))
            {
                throw new InvalidOperationException("Missing required fields");
            }

            if (latitude < -90 || latitude > 90)
            {
// ReSharper disable LocalizableElement
                throw new ArgumentOutOfRangeException("latitude", "Invalid latitude");
            }

            if (longitude < -180 || latitude > 180)
            {
                throw new ArgumentOutOfRangeException("longitude", "Invalid longitude");
            }
// ReSharper restore LocalizableElement
        }

        private void OnRateCreated(TariffCreated @event)
        {
            if (@event.Type == TariffType.Default)
            {
                _defaultTariffId = @event.TariffId;
            }
        }

        private void OnRuleCreated(RuleCreated @event)
        {
            if (@event.Type == RuleType.Default)
            {
                if (!_defaultRules.ContainsKey(@event.Category))
                {
                    _defaultRules.Add(@event.Category, @event.RuleId);
                }
                else
                {
                    _defaultRules[@event.Category] = @event.RuleId;
                }
            }
        }

        public void UpdateTermsAndConditions(string termsAndConditions)
        {
            Update(new TermsAndConditionsUpdated
            {
                TermsAndConditions = termsAndConditions
            });
        }

        public void RetriggerTermsAndConditions()
        {
            Update(new TermsAndConditionsRetriggered());
        }

        public void AddUpdateAccountCharge(Guid accountChargeId, string number, string name, AccountChargeQuestion[] questions)
        {
            Update(new AccountChargeAddedUpdated
            {
                Name = name,
                Number = number,
                AccountChargeId = accountChargeId,
                Questions = questions
            });
        }

        public void DeleteAccountCharge(Guid accountChargeId)
        {
            Update(new AccountChargeDeleted
            {
                AccountChargeId = accountChargeId
            });
        }

        public void AddUpdateVehicleType(Guid vehicleTypeId, string name, string logoName, int referenceDataVehicleId)
        {
            Update(new VehicleTypeAddedUpdated
            {
                Name = name,
                LogoName = logoName,
                VehicleTypeId = vehicleTypeId,
                ReferenceDataVehicleId = referenceDataVehicleId
            });
        }

        public void DeleteVehicleType(Guid vehicleTypeId)
        {
            Update(new VehicleTypeDeleted
            {
                VehicleTypeId = vehicleTypeId
            });
        }

        public void AddOrUpdateNotificationSettings(NotificationSettings notificationSettings)
        {
            notificationSettings.Id = Id;

            Update(new NotificationSettingsAddedOrUpdated
            {
                NotificationSettings = notificationSettings
            });
        }
    }
}