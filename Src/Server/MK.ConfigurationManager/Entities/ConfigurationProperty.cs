using System;
using System.ComponentModel.DataAnnotations;

namespace MK.ConfigurationManager.Entities
{
    public class ConfigurationProperty
    {
        [Key]
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public Guid PrincipalId { get; set; }
    }
}