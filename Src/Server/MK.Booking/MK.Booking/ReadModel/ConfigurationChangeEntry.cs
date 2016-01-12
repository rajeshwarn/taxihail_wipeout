using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.ReadModel
{
    public class ConfigurationChangeEntry
    {
        public ConfigurationChangeEntry()
        {
            Id = Guid.NewGuid();
        }

        [Key]
        public Guid Id { get; set; }

        public Guid AccountId { get; set; }

        public string AccountEmail { get; set; }

        public DateTime Date { get; set; }

        public string OldValues { get; set; }

        public string NewValues { get; set; }

        public ConfigurationChangeType Type { get; set; }
    }

    public enum ConfigurationChangeType
    {
        CompanySettings,
        PaymentSetttings,
    }
}
