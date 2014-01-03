#region

using System;
using apcurium.MK.Common.Entity;
using Infrastructure.EventSourcing;

#endregion

namespace DatabaseInitializer.OldEvents
{
    public class FavoriteAddressUpdatedv1 : VersionedEvent
    {
        public Address Address { get; set; }
        public Guid AddressId { get; set; }
        public string FriendlyName { get; set; }
        public string FullAddress { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string Apartment { get; set; }
        public string RingCode { get; set; }
        public string BuildingName { get; set; }
    }

    public class FavoriteAddressAddedv1 : VersionedEvent
    {
        public Address Address { get; set; }
        public Guid AddressId { get; set; }
        public string FriendlyName { get; set; }
        public string FullAddress { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string Apartment { get; set; }
        public string RingCode { get; set; }
        public string BuildingName { get; set; }
    }
}