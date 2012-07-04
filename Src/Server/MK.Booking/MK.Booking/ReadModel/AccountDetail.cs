using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.ReadModel
{
    public class AccountDetail
    {
        [Key]
        public Guid Id { get; set; }      
        public string FirstName { get; set; }
        public string LastName{ get; set; }
        public string Email { get; set; }
        public string Password{ get; set; }
        public string Phone{ get; set; }

        public int IBSAccountid { get; set; }   
        
    }
}
