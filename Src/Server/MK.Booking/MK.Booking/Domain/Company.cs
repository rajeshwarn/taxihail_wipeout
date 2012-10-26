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
            base.Handles<DefaultFavoriteAddressAdded>(OnDefaultFavoriteAddressAdded);
            base.Handles<DefaultFavoriteAddressRemoved>(OnDefaultFavoriteAddressRemoved);
            base.Handles<DefaultFavoriteAddressUpdated>(OnDefaultFavoriteAddressUpdated);

            base.Handles<PopularAddressAdded>(OnPopularAddressAdded);
            base.Handles<PopularAddressRemoved>(OnPopularAddressRemoved);
            base.Handles<PopularAddressUpdated>(OnPopularAddressUpdated);

            base.Handles<CompanyCreated>(OnCompanyCreated);
            base.Handles<RateCreated>(OnRateCreated);
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

        public void CreateRate(Guid rateId, decimal flatRate, double distanceMultiplicator, double timeAdustmentFactor, decimal pricePerPassenger, DayOfTheWeek daysOfTheWeek, DateTime startTime, DateTime endTime)
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
                FlatRate = flatRate,
                DistanceMultiplicator = distanceMultiplicator,
                TimeAdjustmentFactor = timeAdustmentFactor,
                PricePerPassenger = pricePerPassenger,
                DaysOfTheWeek = daysOfTheWeek,
                StartTime = startTime,
                EndTime = endTime
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

        private void OnCompanyCreated(CompanyCreated obj)
        {

        }

        private void OnDefaultFavoriteAddressUpdated(DefaultFavoriteAddressUpdated obj)
        {

        }

        private void OnDefaultFavoriteAddressRemoved(DefaultFavoriteAddressRemoved obj)
        {
        }

        private void OnDefaultFavoriteAddressAdded(DefaultFavoriteAddressAdded obj)
        {
        }

        private void OnPopularAddressUpdated(PopularAddressUpdated obj)
        {

        }

        private void OnPopularAddressRemoved(PopularAddressRemoved obj)
        {

        }

        private void OnPopularAddressAdded(PopularAddressAdded obj)
        {

        }

        private void OnRateCreated(RateCreated @event)
        {
        }

        
    }
}
