using System;
using System.ComponentModel.DataAnnotations;

namespace MK.ConfigurationManager.Entities
{
    public class IBSServer
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }
    }
}