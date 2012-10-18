using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MK.ConfigurationManager.Entities
{
    public class Company
    {
        [Key]
        public Guid Id { get; set; }

        private Dictionary<string, string> Settings { get; set; } 
    }
}