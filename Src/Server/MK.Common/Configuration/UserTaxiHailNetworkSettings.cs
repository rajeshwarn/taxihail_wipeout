using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Common.Configuration
{
    public class UserTaxiHailNetworkSettings
    {
        [Key]
        public Guid Id { get; set; }

        public bool IsEnabled { get; set; }

        public string SerializedDisabledFleets { get; set; }
    }
}
