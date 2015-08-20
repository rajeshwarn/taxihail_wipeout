#region

using apcurium.MK.Common;
using Infrastructure.EventSourcing;
using System;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class AccountRegistered : VersionedEvent
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public CountryISOCode Country { get; set; }

        public string Phone { get; set; }

        public byte[] Password { get; set; }

        public int? IbsAcccountId { get; set; }

        public string FacebookId { get; set; }

        public string TwitterId { get; set; }

        public string ConfirmationToken { get; set; }

        public string Language { get; set; }

        public bool IsAdmin { get; set; }

        public bool AccountActivationDisabled { get; set; }

        public string PayBack { get; set; }

        public int NbPassengers { get; set; }
    }
}