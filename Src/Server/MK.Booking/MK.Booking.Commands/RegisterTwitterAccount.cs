#region

using System;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Commands
{
    public class RegisterTwitterAccount : ICommand
    {
        public RegisterTwitterAccount()
        {
            Id = Guid.NewGuid();
        }

        public Guid AccountId { get; set; }
        public string TwitterId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int IbsAccountId { get; set; }
        public string Language { get; set; }
        public Guid Id { get; set; }
    }
}