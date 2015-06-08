using System;
using apcurium.MK.Common.Entity;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class AccountAnswersAddedUpdated : VersionedEvent
    {
        public AccountQuestionAnswer[] Answers { get; set; }
    }
}