using apcurium.MK.Common.Enumeration;
using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.ReadModel
{
    public class AccountNoteDetail
    {
        public AccountNoteDetail()
        {
        }

        [Key]
        public Guid Id { get; set; }

        public Guid AccountId { get; set; }

        public string Note { get; set; }

        public NoteType Type { get; set; }

        public string WriterEmail { get; set; }

        public DateTime CreationDate { get; set; }

    }
}
