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

        public string AccountId { get; set; }

        public string AccountEmail { get; set; }

        public DateTime Date { get; set; }

        public string OldValues { get; set; }

        public string NewValues { get; set; }

        public string Type { get; set; }
    }

    public enum ConfigurationChangeType
    {
        CompanySettings,
        PaymentSetttings,
    }
}
