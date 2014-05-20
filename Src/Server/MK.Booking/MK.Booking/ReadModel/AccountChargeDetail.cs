using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.ReadModel
{
    public class AccountChargeDetail
    {
        public AccountChargeDetail()
        {
            Questions = new List<AccountChargeQuestion>();     
        }

        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Number { get; set; }

        public virtual ICollection<AccountChargeQuestion> Questions { get; set; }

    }
}