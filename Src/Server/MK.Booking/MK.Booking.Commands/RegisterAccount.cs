using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;
using System.ComponentModel.DataAnnotations;
using MoveOn.Common.Extensions;


namespace apcurium.MK.Booking.Commands
{
    public class RegisterAccount : ICommand
    {



        public RegisterAccount()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid AccountId { get; set; }        
        public string FirstName { get; set; }        
        public string LastName { get; set; }        
        public string Email { get; set; }
        public string Password { get; set; }


       
    }
}
