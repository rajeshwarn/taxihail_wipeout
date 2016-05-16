using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class UpdateCreditCardValidationDate : ICommand
    {
        public UpdateCreditCardValidationDate()
        {
            Id = Guid.NewGuid();
        }

        public Guid CreditCardId { get; set; }
        public Guid AccountId { get; set; }
        public Guid Id { get; set; }
        public DateTime? LastTokenValidateDateTime { get; set; }
        //public DateTime? LastTokenValidateDateTime { get; set; }

        //public int Year { get; set; }

        //public string DateString { get; set; }

        //public DateTime LastTokenValidateDateTimeNotNull { get; set; }

    }
}
