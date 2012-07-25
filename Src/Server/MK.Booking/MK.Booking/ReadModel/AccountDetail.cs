using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.ReadModel
{
    public class AccountDetail
    {
        public AccountDetail()
        {
            //required by EF
            Settings = new BookingSettingsDetails();
        }

        [Key]
        public Guid Id { get; set; }      
        public string Name { get; set; }        
        public string Email { get; set; }
        public byte[] Password{ get; set; }
        public string Phone{ get; set; }
        public int IBSAccountId { get; set; }
        public string TwitterId { get; set; }
        public string FacebookId { get; set; }

        
        public BookingSettingsDetails Settings { get; set; }
        
    }
}
