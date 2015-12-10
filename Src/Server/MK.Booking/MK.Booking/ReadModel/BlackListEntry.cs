using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.ReadModel
{
    public class BlackListEntry
    {
        [Key]
        public Guid Id { get; set; }

        public string PhoneNumber { get; set; }
    }
}
