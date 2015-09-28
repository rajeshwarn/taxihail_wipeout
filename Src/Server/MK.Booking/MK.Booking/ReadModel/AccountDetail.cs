#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        public int? IBSAccountId { get; set; }

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

        public bool IsPayPalAccountLinked { get; set; }

        /// <summary>
        /// This date is saved in UTC
        /// </summary>
        public DateTime CreationDate { get; set; }

        public bool HasAdminAccess
        {
            get { return (Roles & (int) Security.Roles.Support) == (int) Security.Roles.Support; }
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

        public bool HasValidPaymentInformation
        {
            get
            {
                return IsPayPalAccountLinked || DefaultCreditCard != null;
            }
        }
    }
}