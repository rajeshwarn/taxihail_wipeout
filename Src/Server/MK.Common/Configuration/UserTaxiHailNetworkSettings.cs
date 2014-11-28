using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Common.Configuration
{
    public class UserTaxiHailNetworkSettings
    {
        [Key]
        public Guid Id { get; set; }

        public bool IsEnabled { get; set; }

        public string SerializedDisabledFleets { get; set; }

        [NotMapped]
        public List<string> DisbledFleets
        {
            get { return SerializedDisabledFleets.UnFlatten(",").ToList(); }
            set { SerializedDisabledFleets = value.Flatten(","); }
        }
    }
}
