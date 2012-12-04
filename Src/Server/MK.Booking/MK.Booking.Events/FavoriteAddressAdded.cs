using System;
using Infrastructure.EventSourcing;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Events
{
    public class FavoriteAddressAdded : VersionedEvent
    {
        public Guid AddressId { get; set; }
       

        public Address Address { get; set; }

        /***
         * to be deleted after migration
         * **/
        public string FriendlyName { get; set; }
        public string FullAddress { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string Apartment { get; set; }
        public string RingCode { get; set; }
        public string BuildingName { get; set; }
    }
}
