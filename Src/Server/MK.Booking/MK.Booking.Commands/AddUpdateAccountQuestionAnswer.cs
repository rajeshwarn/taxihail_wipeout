using System;
using apcurium.MK.Common.Entity;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Commands
{
    public class AddUpdateAccountQuestionAnswer : ICommand
    {
        public AddUpdateAccountQuestionAnswer()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public AccountQuestionAnswer[] Answers { get; set; }
    }
}