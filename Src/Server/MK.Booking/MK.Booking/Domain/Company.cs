﻿using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.EventSourcing;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Domain
{
    public class Company : EventSourced
    {
        private Guid? _defaultTariffId;

        private Dictionary<RuleCategory, Guid> _defaultRules = new Dictionary<RuleCategory, Guid>();


        public Company(Guid id)
            : base(id)
        {
            RegisterHandlers();
            this.Update(new CompanyCreated
            {
                SourceId = id,
            });
        }

        public Company(Guid id, IEnumerable<IVersionedEvent> history)
            : base(id)
        {
            RegisterHandlers();
            this.LoadFrom(history);
        }

        private void RegisterHandlers()
        {
            Handles<DefaultFavoriteAddressAdded>(OnEventDoNothing);
            Handles<DefaultFavoriteAddressRemoved>(OnEventDoNothing);
            Handles<DefaultFavoriteAddressUpdated>(OnEventDoNothing);

            Handles<PopularAddressAdded>(OnEventDoNothing);
            Handles<PopularAddressRemoved>(OnEventDoNothing);
            Handles<PopularAddressUpdated>(OnEventDoNothing);

            Handles<CompanyCreated>(OnEventDoNothing);
            Handles<AppSettingsAddedOrUpdated>(OnEventDoNothing);
            Handles<PaymentSettingUpdated>(OnEventDoNothing);

            Handles<TariffCreated>(OnRateCreated);
            Handles<TariffUpdated>(OnEventDoNothing);
            Handles<TariffDeleted>(OnEventDoNothing);

            Handles<RuleCreated>(OnRuleCreated);
            Handles<RuleUpdated>(OnEventDoNothing);
            Handles<RuleDeleted>(OnEventDoNothing);
            Handles<RuleActivated>(OnEventDoNothing);
            Handles<RuleDeactivated>(OnEventDoNothing);


            Handles<RatingTypeAdded>(OnEventDoNothing);
            Handles<RatingTypeHidded>(OnEventDoNothing);
            Handles<RatingTypeUpdated>(OnEventDoNothing);
        }

        public void AddDefaultFavoriteAddress(Address address)
        {
            ValidateFavoriteAddress(address.FriendlyName, address.FullAddress, address.Latitude, address.Longitude);

            this.Update(new DefaultFavoriteAddressAdded
            {
                Address = address
            });
        }

        public void UpdateDefaultFavoriteAddress(Address address)
        {
            ValidateFavoriteAddress(address.FriendlyName, address.FullAddress, address.Latitude, address.Longitude);

            this.Update(new DefaultFavoriteAddressUpdated()
            {
                Address = address
            });
        }

        public void RemoveDefaultFavoriteAddress(Guid addressId)
        {
            this.Update(new DefaultFavoriteAddressRemoved
            {
                AddressId = addressId
            });
        }

        public void AddPopularAddress(Address address)
        {
            ValidateFavoriteAddress(address.FriendlyName, address.FullAddress, address.Latitude, address.Longitude);

            this.Update(new PopularAddressAdded
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

            Update(new RatingTypeAdded()
                       {
                           Name = name,
                           RatingTypeId = ratingTypeId
                       });
        }

        public void UpdateRatingType(string name, Guid ratingTypeId)
        {
            if (name.IsNullOrEmpty())
                throw new ArgumentException("Rating name cannot be null or empty");

            Update(new RatingTypeUpdated()
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

        public void CreateDefaultTariff(Guid tariffId, string name, decimal flatRate, double distanceMultiplicator, double timeAdustmentFactor, decimal pricePerPassenger)
        {
            if (_defaultTariffId.HasValue)
            {
                throw new InvalidOperationException("Only one default tariff can be created");
            }

            this.Update(new TariffCreated
            {
                TariffId = tariffId,
                Type = TariffType.Default,
                Name = name,
                FlatRate = flatRate,
                KilometricRate = distanceMultiplicator,
                MarginOfError = timeAdustmentFactor,
                PassengerRate = pricePerPassenger,
            });

        }




        public void CreateRecurringTariff(Guid tariffId, string name, decimal flatRate, double distanceMultiplicator, double timeAdustmentFactor, decimal pricePerPassenger, DayOfTheWeek daysOfTheWeek, DateTime startTime, DateTime endTime)
        {
            this.Update(new TariffCreated
            {
                TariffId = tariffId,
                Type = TariffType.Recurring,
                Name = name,
                FlatRate = flatRate,
                KilometricRate = distanceMultiplicator,
                MarginOfError = timeAdustmentFactor,
                PassengerRate = pricePerPassenger,
                DaysOfTheWeek = daysOfTheWeek,
                StartTime = startTime,
                EndTime = endTime
            });
        }

        public void CreateDayTariff(Guid tariffId, string name, decimal flatRate, double distanceMultiplicator, double timeAdustmentFactor, decimal pricePerPassenger, DateTime startTime, DateTime endTime)
        {
            this.Update(new TariffCreated
            {
                TariffId = tariffId,
                Type = TariffType.Day,
                Name = name,
                FlatRate = flatRate,
                KilometricRate = distanceMultiplicator,
                MarginOfError = timeAdustmentFactor,
                PassengerRate = pricePerPassenger,
                StartTime = startTime,
                EndTime = endTime
            });
        }

        public void UpdateTariff(Guid tariffId, string name, decimal flatRate, double distanceMultiplicator, double timeAdustmentFactor, decimal pricePerPassenger, DayOfTheWeek daysOfTheWeek, DateTime startTime, DateTime endTime)
        {
            this.Update(new TariffUpdated
            {
                TariffId = tariffId,
                Name = name,
                FlatRate = flatRate,
                KilometricRate = distanceMultiplicator,
                MarginOfError = timeAdustmentFactor,
                PassengerRate = pricePerPassenger,
                DaysOfTheWeek = daysOfTheWeek,
                StartTime = startTime,
                EndTime = endTime
            });
        }

        public void DeleteTariff(Guid tariffId)
        {
            if (tariffId == this._defaultTariffId)
            {
                throw new InvalidOperationException("Cannot delete default tariff");
            }
            this.Update(new TariffDeleted
            {
                TariffId = tariffId
            });

        }

        
        public void CreateRule(Guid ruleId, string name, string message, string zoneList,  RuleType type, RuleCategory category, bool appliedToCurrentBooking, bool appliesToFutureBooking, int priority, bool isActive, DayOfTheWeek daysOfTheWeek, DateTime? startTime, DateTime? endTime, DateTime? activeFrom, DateTime? activeTo)
        {
            /*if ((type == RuleType.Default) && _defaultRules.ContainsKey(category))
            {
                throw new InvalidOperationException(string.Format("Only one default rule of type {0} can be created", category.ToString()));
            }*/

            if ((type == RuleType.Default) && message.IsNullOrEmpty() )
            {
                throw new InvalidOperationException(string.Format("Missing message for default rule", category.ToString()));
            }
            else if ((type == RuleType.Recurring ) && (Params.Get( message, name ).Any( s => s.IsNullOrEmpty() ) || (daysOfTheWeek == DayOfTheWeek.None) || (!startTime.HasValue) || (!endTime.HasValue) )  )
            {
                throw new InvalidOperationException("Missing message for recurrring rule");
            }
            else if ((type == RuleType.Date) && (Params.Get(message, name).Any(s => s.IsNullOrEmpty()) || (!activeFrom.HasValue) || (!activeFrom.HasValue)))
            {
                throw new InvalidOperationException("Missing message for date rule");
            }

            
            this.Update(new RuleCreated
            {
                RuleId = ruleId,
                Type = type,
                Name = name,
                Message = message,
                ZoneList = zoneList,
                Category = category,
                AppliesToCurrentBooking = appliedToCurrentBooking,
                AppliesToFutureBooking = appliesToFutureBooking,
                IsActive = isActive,
                DaysOfTheWeek = daysOfTheWeek,
                StartTime = startTime,
                EndTime = endTime,
                ActiveFrom = activeFrom,
                ActiveTo = activeTo,
                Priority = /*type == RuleType.Default ? 0 :*/ priority,
            });

        }

        public void UpdateRule(Guid ruleId, string name, string message, string zoneList, bool appliedToCurrentBooking, bool appliesToFutureBooking, DayOfTheWeek daysOfTheWeek, DateTime? startTime, DateTime? endTime, DateTime? activeFrom, DateTime? activeTo, int priority, bool isActive)
        {

            this.Update(new RuleUpdated
            {
                RuleId = ruleId,
                Name = name,
                Message = message,
                ZoneList = zoneList,
                AppliesToCurrentBooking = appliedToCurrentBooking,
                AppliesToFutureBooking = appliesToFutureBooking,
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
           /* if (_defaultRules.ContainsValue( ruleId ) )
            {
                throw new InvalidOperationException("Cannot delete default tariff");
            }*/
            this.Update(new RuleDeleted
            {
                RuleId = ruleId
            });
        }

        public void UpdatePaymentSettings(UpdatePaymentSettings command)
        {
            this.Update(new PaymentSettingUpdated()
            {
                ServerPaymentSettings = command.ServerPaymentSettings
            });
        }

        public void ActivateRule(Guid ruleId)
        {            
            this.Update(new RuleActivated
            {
                RuleId = ruleId
            });

        }

        public void DeactivateRule(Guid ruleId)
        {            
            this.Update(new RuleDeactivated
            {
                RuleId = ruleId
            });

        }


        private static void ValidateFavoriteAddress(string friendlyName, string fullAddress, double latitude, double longitude)
        {
            if (Params.Get(friendlyName, fullAddress).Any(string.IsNullOrEmpty))
            {
                throw new InvalidOperationException("Missing required fields");
            }

            if (latitude < -90 || latitude > 90)
            {
                throw new ArgumentOutOfRangeException("latitude", "Invalid latitude");
            }

            if (longitude < -180 || latitude > 180)
            {
                throw new ArgumentOutOfRangeException("longitude", "Invalid longitude");
            }
        }

        private void OnRateCreated(TariffCreated @event)
        {
            if (@event.Type == TariffType.Default)
            {
                this._defaultTariffId = @event.TariffId;
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


        private void OnEventDoNothing<T>(T @event) where T : VersionedEvent
        {
            // Do nothing
        }

    }
}
