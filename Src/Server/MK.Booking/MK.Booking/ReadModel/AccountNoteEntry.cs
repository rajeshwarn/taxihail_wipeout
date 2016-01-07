using apcurium.MK.Common.Enumeration;
using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.ReadModel
{
    public class AccountNoteEntry
    {
        public AccountNoteEntry()
        {
            Id = Guid.NewGuid();
        }

        [Key]
        public Guid Id { get; set; }

        public string AccountId { get; set; }

        public string Note { get; set; }

        public NoteType Type { get; set; }

        public string AccountEmail { get; set; }

        public DateTime CreationDate { get; set; }

    }
}
