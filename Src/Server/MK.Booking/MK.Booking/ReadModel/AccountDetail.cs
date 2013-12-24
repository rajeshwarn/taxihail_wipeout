#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.ReadModel
{
    public class AccountDetail
    {
        public AccountDetail()
        {
            //required by EF
            Settings = new BookingSettings();
        }

        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public byte[] Password { get; set; }
        public string Phone { get; set; }
        public int IBSAccountId { get; set; }
        public string TwitterId { get; set; }
        public string FacebookId { get; set; }
        public bool IsConfirmed { get; set; }
        public bool DisabledByAdmin { get; set; }
        public string Language { get; set; }
        public string ConfirmationToken { get; set; }
        public int Roles { get; set; }

        public BookingSettings Settings { get; set; }

        public Guid? DefaultCreditCard { get; set; }
        public int? DefaultTipPercent { get; set; }

        public DateTime CreationDate { get; set; }

        public bool IsAdmin
        {
            get { return (Roles & (int) Security.Roles.Admin) == (int) Security.Roles.Admin; }
        }

        public IEnumerable<string> RoleNames
        {
            get
            {
                foreach (int role in Enum.GetValues(typeof (Roles)))
                {
                    if ((Roles & role) == role)
                    {
                        yield return Enum.GetName(typeof (Roles), role);
                    }
                }
            }
        }
    }
}