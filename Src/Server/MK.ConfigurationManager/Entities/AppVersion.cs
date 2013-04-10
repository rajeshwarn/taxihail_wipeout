using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK.ConfigurationManager.Entities
{
    public class AppVersion
    {
        [Key]
        public Guid Id { get; set; }

        public string Display { get; set; }

        public string Revision { get; set; }

    }
}
 