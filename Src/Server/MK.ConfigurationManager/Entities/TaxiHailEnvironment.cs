using System;
using System.ComponentModel.DataAnnotations;

namespace MK.ConfigurationManager.Entities
{
    public class TaxiHailEnvironment
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string IP { get; set; }

        public string Url { get; set; }

        public string SqlServerInstance { get; set; }

        public string WebSitesFolder { get; set; }
    }
}