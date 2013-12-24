#region

using System;

#endregion

namespace apcurium.MK.Booking.IBS
{
    public class IbsAddress
    {
        public Guid Id { get; set; }

        public string FriendlyName { get; set; }

        public string FullAddress { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public string Apartment { get; set; }

        public string RingCode { get; set; }

        public string ZipCode { get; set; }

        public string BuildingName { get; set; }
    }
}