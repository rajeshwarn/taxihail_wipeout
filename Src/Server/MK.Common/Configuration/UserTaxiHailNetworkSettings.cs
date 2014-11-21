using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ServiceStack.Text;

namespace apcurium.MK.Common.Configuration
{
    public class UserTaxiHailNetworkSettings
    {
        [Key]
        public Guid Id { get; set; }

        public bool Enabled { get; set; }

        public string SerializedDisabledFleets { get; private set; }

        [NotMapped]
        public List<string> DisabledFleets
        {
            get { return SerializedDisabledFleets.FromJson<List<string>>(); }
            set { SerializedDisabledFleets = value.ToJson(); }
        } 
    }
}