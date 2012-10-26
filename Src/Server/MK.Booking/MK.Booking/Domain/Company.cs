using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.EventSourcing;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Domain
{
    public class Company : EventSourced
    {
        public Company(Guid id) : base(id)
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
            base.Handles<DefaultFavoriteAddressAdded>(OnEventDoNothing);
            base.Handles<DefaultFavoriteAddressRemoved>(OnEventDoNothing);
            base.Handles<DefaultFavoriteAddressUpdated>(OnEventDoNothing);

            base.Handles<PopularAddressAdded>(OnEventDoNothing);
            base.Handles<PopularAddressRemoved>(OnEventDoNothing);
            base.Handles<PopularAddressUpdated>(OnEventDoNothing);

            base.Handles<CompanyCreated>(OnEventDoNothing);
            base.Handles<RateCreated>(OnEventDoNothing);
            base.Handles<RateDeleted>(OnEventDoNothing);
        }



        public void AddDefaultFavoriteAddress(Guid id, string friendlyName, string apartment, string fullAddress, string ringCode, string buildingName, double latitude, double longitude)
        {
            ValidateFavoriteAddress(friendlyName, fullAddress, latitude, longitude);

            this.Update(new DefaultFavoriteAddressAdded
            {
                AddressId = id,
                FriendlyName = friendlyName,
                Apartment = apartment,
                FullAddress = fullAddress,
                RingCode = ringCode,
                BuildingName = buildingName,
                Latitude = latitude,
                Longitude = longitude,
            });
        }

        public void UpdateDefaultFavoriteAddress(Guid id, string friendlyName, string apartment, string fullAddress, string ringCode, string buildingName, double latitude, double longitude)
        {
            ValidateFavoriteAddress(friendlyName, fullAddress, latitude, longitude);

            this.Update(new DefaultFavoriteAddressUpdated()
            {
                AddressId = id,
                FriendlyName = friendlyName,
                Apartment = apartment,
                FullAddress = fullAddress,
                RingCode = ringCode,
                BuildingName = buildingName,
                Latitude = latitude,
                Longitude = longitude
            });
        }

        public void RemoveDefaultFavoriteAddress(Guid addressId)
        {
            this.Update(new DefaultFavoriteAddressRemoved
            {
                AddressId = addressId
            });
        }

        public void AddPopularAddress(Guid id, string friendlyName, string apartment, string fullAddress, string ringCode, string buildingName, double latitude, double longitude)
        {
            ValidateFavoriteAddress(friendlyName, fullAddress, latitude, longitude);

            this.Update(new PopularAddressAdded
            {
                AddressId = id,
                FriendlyName = friendlyName,
                Apartment = apartment,
                FullAddress = fullAddress,
                RingCode = ringCode,
                BuildingName = buildingName,
                Latitude = latitude,
                Longitude = longitude,
            });
        }

        public void UpdatePopularAddress(Guid id, string friendlyName, string apartment, string fullAddress, string ringCode, string buildingName, double latitude, double longitude)
        {
            ValidateFavoriteAddress(friendlyName, fullAddress, latitude, longitude);

            this.Update(new PopularAddressUpdated()
            {
                AddressId = id,
                FriendlyName = friendlyName,
                Apartment = apartment,
                FullAddress = fullAddress,
                RingCode = ringCode,
                BuildingName = buildingName,
                Latitude = latitude,
                Longitude = longitude
            });
        }

        public void RemovePopularAddress(Guid addressId)
        {
            this.Update(new PopularAddressRemoved
            {
                AddressId = addressId
            });
        }

        public void CreateRate(Guid rateId, string name, decimal flatRate, double distanceMultiplicator, double timeAdustmentFactor, decimal pricePerPassenger, DayOfTheWeek daysOfTheWeek, DateTime startTime, DateTime endTime)
        {
            // Ensure StartTime and EndTime are the same day
            startTime = DateTime.Today.AddHours(startTime.Hour).AddMinutes(startTime.Minute);
            endTime = DateTime.Today.AddHours(endTime.Hour).AddMinutes(endTime.Minute);

            if(endTime <= startTime)
            {
                throw new InvalidOperationException("Start time must be before end time");
            }

            this.Update(new RateCreated
            {
                RateId = rateId,
                Name = name,
                FlatRate = flatRate,
                DistanceMultiplicator = distanceMultiplicator,
                TimeAdjustmentFactor = timeAdustmentFactor,
                PricePerPassenger = pricePerPassenger,
                DaysOfTheWeek = daysOfTheWeek,
                StartTime = startTime,
                EndTime = endTime
            });
        }

        public void DeleteRate(Guid rateId)
        {
            this.Update(new RateDeleted
            {
                RateId = rateId
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

        
        private void OnEventDoNothing<T>(T @event) where T: VersionedEvent
        {
            // Do nothing
        }
        
    }
}
