using System;
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
        public byte[] Password{ get; set; }
        public string Phone{ get; set; }
        public int IBSAccountid { get; set; }   
        
    }
}
