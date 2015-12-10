﻿using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.EventSourcing;
using MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Domain
{
    public class Company : EventSourced
    {
        private readonly Dictionary<RuleCategory, Guid> _defaultRules = new Dictionary<RuleCategory, Guid>();
        private readonly Dictionary<Guid, IList<RatingType>> _ratingTypes = new Dictionary<Guid, IList<RatingType>>(); 
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

        protected PayPalClientSettings PayPalClientSettings { get; set; }

        protected PayPalServerSettings PayPalServerSettings { get; set; }

        protected bool IsChargeAccountEnabled { get; set; }

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
            Handles<PayPalSettingsChanged>(NoAction);
            Handles<ChargeAccountPaymentDisabled>(NoAction);
            Handles<PaymentSettingUpdated>(OnPaymentSettingUpdated);

            Handles<TariffCreated>(OnRateCreated);
            Handles<TariffUpdated>(NoAction);
            Handles<TariffDeleted>(NoAction);

            Handles<RuleCreated>(OnRuleCreated);
            Handles<RuleUpdated>(NoAction);
            Handles<RuleDeleted>(NoAction);
            Handles<RuleActivated>(NoAction);
            Handles<RuleDeactivated>(NoAction);

            Handles<RatingTypeAdded>(OnRatingTypeAdded);
            Handles<RatingTypeHidded>(NoAction);
            Handles<RatingTypeUpdated>(OnRatingTypeUpdated);
            Handles<RatingTypeDeleted>(OnRatingTypeDeleted);

            Handles<TermsAndConditionsUpdated>(NoAction);
            Handles<TermsAndConditionsRetriggered>(NoAction);

            Handles<AccountChargeAddedUpdated>(NoAction);
            Handles<AccountChargeDeleted>(NoAction);
            Handles<AccountChargeImported>(NoAction);

            Handles<VehicleTypeAddedUpdated>(NoAction);
            Handles<VehicleTypeDeleted>(NoAction);

            Handles<NotificationSettingsAddedOrUpdated>(NoAction);

            Handles<PrivacyPolicyUpdated>(NoAction);

            Handles<FeesUpdated>(NoAction);
            
            Handles<ServiceTypeSettingsUpdated>(NoAction);
        }

        private void OnPaymentSettingUpdated(PaymentSettingUpdated obj)
        {
            PaymentMode = obj.ServerPaymentSettings.PaymentMode;
            PayPalClientSettings = obj.ServerPaymentSettings.PayPalClientSettings;
            PayPalServerSettings = obj.ServerPaymentSettings.PayPalServerSettings;

            IsChargeAccountEnabled = obj.ServerPaymentSettings.IsChargeAccountPaymentEnabled;
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

        public void AddRatingType(Guid ratingTypeId, string name, string language)
        {
            if (language.IsNullOrEmpty())
                throw new ArgumentException("Rating language cannot be null or empty");

            Update(new RatingTypeAdded
            {
                Name = name,
                RatingTypeId = ratingTypeId,
                Language = language
            });
        }

        public void UpdateRatingType(Guid ratingTypeId, string name, string language)
        {
            if (language.IsNullOrEmpty())
                throw new ArgumentException("Rating language cannot be null or empty");

            if (_ratingTypes.ContainsKey(ratingTypeId))
            {
                var ratingTypes = _ratingTypes[ratingTypeId];
                var ratingType = ratingTypes.FirstOrDefault(t => t.Language == language);

                // Only send Update event if the name value has changed
                if (ratingType != null && ratingType.Name == name)
                {
                    return;
                } 
            }

            Update(new RatingTypeUpdated
            {
                Name = name,
                RatingTypeId = ratingTypeId,
                Language = language
            });
        }

        public void DeleteRatingType(Guid ratingTypeId)
        {
            Update(new RatingTypeDeleted
            {
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
            double timeAdustmentFactor, double kilometerIncluded, int? vehicleTypeId, ServiceType serviceType, double minimumRate)
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
                VehicleTypeId = vehicleTypeId,
                ServiceType = serviceType
            });
        }

        public void CreateRecurringTariff(Guid tariffId, string name, decimal flatRate, double distanceMultiplicator, double perMinuteRate,
            double timeAdustmentFactor, double kilometerIncluded, DayOfTheWeek daysOfTheWeek,
            DateTime startTime, DateTime endTime, int? vehicleTypeId, ServiceType serviceType, double minimumRate)
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
                VehicleTypeId = vehicleTypeId,
                ServiceType = serviceType
            });
        }

        public void CreateDayTariff(Guid tariffId, string name, decimal flatRate, double distanceMultiplicator, double perMinuteRate,
            double timeAdustmentFactor, double kilometerIncluded, DateTime startTime,
            DateTime endTime, int? vehicleTypeId, ServiceType serviceType, double minimumRate)
        {
            Update(new TariffCreated
            {
                TariffId = tariffId,
                Type = TariffType.Day,
                Name = name,
                MinimumRate = minimumRate,
                FlatRate = flatRate,
                KilometricRate = distanceMultiplicator,
                PerMinuteRate = perMinuteRate,
                KilometerIncluded = kilometerIncluded,
                MarginOfError = timeAdustmentFactor,
                StartTime = startTime,
                EndTime = endTime,
                VehicleTypeId = vehicleTypeId,
                ServiceType = serviceType
            });
        }

        public void UpdateTariff(Guid tariffId, string name, decimal flatRate, double distanceMultiplicator, double perMinuteRate,
            double timeAdustmentFactor, double kilometerIncluded, DayOfTheWeek daysOfTheWeek,
            DateTime startTime, DateTime endTime, int? vehicleTypeId, ServiceType serviceType, double minimumRate)
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
                VehicleTypeId = vehicleTypeId,
                ServiceType = serviceType
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
            DateTime? activeTo, string market, bool disableFutureBookingOnError)
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
                Priority = priority,
                Market = market,
                DisableFutureBookingOnError = disableFutureBookingOnError
            });
        }

        public void UpdateRule(Guid ruleId, string name, string message, string zoneList, bool zoneRequired, bool appliedToCurrentBooking,
            bool appliesToFutureBooking, bool appliesToPickup, bool appliesToDropoff, DayOfTheWeek daysOfTheWeek, DateTime? startTime, DateTime? endTime,
            DateTime? activeFrom, DateTime? activeTo, int priority, bool isActive, string market, bool disableFutureBookingOnError)
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
                Market = market,
                DisableFutureBookingOnError = disableFutureBookingOnError
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

            if (HavePayPalSettingsChanged(command.ServerPaymentSettings))
            {
                Update(new PayPalSettingsChanged());
            }

            if (ChargeAccountPaymentEnabledChanged(command.ServerPaymentSettings) && 
                !command.ServerPaymentSettings.IsChargeAccountPaymentEnabled)
            {
                Update(new ChargeAccountPaymentDisabled());
            }

            Update(new PaymentSettingUpdated
            {
                ServerPaymentSettings = command.ServerPaymentSettings
            });
        }

        private bool ChargeAccountPaymentEnabledChanged(ServerPaymentSettings newPaymentSettings)
        {
            return newPaymentSettings.IsChargeAccountPaymentEnabled != IsChargeAccountEnabled;
        }

        private bool HavePayPalSettingsChanged(ServerPaymentSettings newPaymentSettings)
        {
            if (PayPalClientSettings == null || PayPalServerSettings == null)
            {
                return true;
            }

            var disabledStatusChanged = PayPalClientSettings.IsEnabled != newPaymentSettings.PayPalClientSettings.IsEnabled;

            var webLandingPageChanged = PayPalServerSettings.LandingPageType != newPaymentSettings.PayPalServerSettings.LandingPageType;

            var environmentChanged = PayPalClientSettings.IsSandbox != newPaymentSettings.PayPalClientSettings.IsSandbox;

            var sandboxSettingsChanged = PayPalClientSettings.SandboxCredentials.ClientId != newPaymentSettings.PayPalClientSettings.SandboxCredentials.ClientId
                || PayPalServerSettings.SandboxCredentials.Secret != newPaymentSettings.PayPalServerSettings.SandboxCredentials.Secret;

            var prodSettingsChanged = PayPalClientSettings.Credentials.ClientId != newPaymentSettings.PayPalClientSettings.Credentials.ClientId
                   || PayPalServerSettings.Credentials.Secret != newPaymentSettings.PayPalServerSettings.Credentials.Secret;

            return disabledStatusChanged || webLandingPageChanged || environmentChanged || sandboxSettingsChanged || prodSettingsChanged;
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

        public void AddUpdateAccountCharge(Guid accountChargeId, string number, string name,
            bool useCardOnFileForPayment, AccountChargeQuestion[] questions)
        {
            Update(new AccountChargeAddedUpdated
            {
                Name = name,
                Number = number,
                AccountChargeId = accountChargeId,
                UseCardOnFileForPayment = useCardOnFileForPayment,
                Questions = questions
            });
        }

        public void ImportAccountCharge(AccountCharge[] accounts)
        {
            Update(new AccountChargeImported()
            {
                AccountCharges = accounts
            });
        }

        public void DeleteAccountCharge(Guid accountChargeId)
        {
            Update(new AccountChargeDeleted
            {
                AccountChargeId = accountChargeId
            });
        }

        public void AddUpdateVehicleType(Guid vehicleTypeId, string name, string logoName, int referenceDataVehicleId, ServiceType serviceType, int maxNumberPassengers, int? networkVehicleTypeId, bool isWheelchairAccessible)
        {
            Update(new VehicleTypeAddedUpdated
            {
                Name = name,
                LogoName = logoName,
                VehicleTypeId = vehicleTypeId,
                ReferenceDataVehicleId = referenceDataVehicleId,
                MaxNumberPassengers = maxNumberPassengers,
                ReferenceNetworkVehicleTypeId = networkVehicleTypeId,
                ServiceType = serviceType,
                IsWheelchairAccessible = isWheelchairAccessible
            });
        }

        public void UpdateServiceTypeSettings(ServiceTypeSettings serviceTypeSettings)
        {
            Update(new ServiceTypeSettingsUpdated
            {
                ServiceTypeSettings = serviceTypeSettings
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

        public void UpdatePrivacyPolicy(string policy)
        {
            Update(new PrivacyPolicyUpdated
            {
                Policy = policy
            });
        }

        public void UpdateFees(List<Fees> fees)
        {
            Update(new FeesUpdated
            {
                Fees = fees
            });
        }

        private bool GoingFromCmtToRidelinqOrRidelinqToCmt(PaymentMethod original, PaymentMethod newMethod)
        {
            return ((original == PaymentMethod.Cmt || original == PaymentMethod.RideLinqCmt) &&
                    (newMethod == PaymentMethod.Cmt || newMethod == PaymentMethod.RideLinqCmt));
        }

        private static void ValidateFavoriteAddress(string friendlyName, string fullAddress, double latitude,
            double longitude)
        {
            if (Params.Get(friendlyName, fullAddress).Any(string.IsNullOrEmpty))
            {
                throw new InvalidOperationException("Missing required fields");
            }

            if (double.IsNaN(latitude) || latitude < -90 || latitude > 90)
            {
                throw new ArgumentOutOfRangeException("latitude", "Invalid latitude");
            }

            if (double.IsNaN(longitude) || longitude < -180 || longitude > 180)
            {
                throw new ArgumentOutOfRangeException("longitude", "Invalid longitude");
            }
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

        private void OnRatingTypeAdded(RatingTypeAdded @event)
        {
            if (!_ratingTypes.ContainsKey(@event.RatingTypeId))
            {
                _ratingTypes.Add(@event.RatingTypeId, new List<RatingType>());
            }

            _ratingTypes[@event.RatingTypeId].Add(new RatingType
            {
                Id = @event.RatingTypeId,
                Name = @event.Name,
                Language = @event.Language
            });
        }

        private void OnRatingTypeUpdated(RatingTypeUpdated @event)
        {
            if (_ratingTypes.ContainsKey(@event.RatingTypeId))
            {
                var ratingType = _ratingTypes[@event.RatingTypeId];
                var ratingTypeToUpdate = ratingType.FirstOrDefault(t => t.Language == @event.Language);
                if (ratingTypeToUpdate != null)
                {
                    ratingTypeToUpdate.Name = @event.Name;
                }
            }
        }

        private void OnRatingTypeDeleted(RatingTypeDeleted @event)
        {
            if (_ratingTypes.ContainsKey(@event.RatingTypeId))
            {
                _ratingTypes.Remove(@event.RatingTypeId);
            }
        }
    }
}