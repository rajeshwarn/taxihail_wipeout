using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Dictionary<string, string> OldValues { get; set; }

        public Dictionary<string, string> NewValues { get; set; }

        public ConfigurationChangeType Type { get; set; }
    }

    public enum ConfigurationChangeType
    {
        CompanySettings,
        PaymentSetttings,
    }
}
