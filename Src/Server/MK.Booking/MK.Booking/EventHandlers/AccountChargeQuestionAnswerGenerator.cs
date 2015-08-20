using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers
{
    public class AccountChargeQuestionAnswerGenerator : IEventHandler<AccountAnswersAddedUpdated>
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public AccountChargeQuestionAnswerGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(AccountAnswersAddedUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                @event.Answers.ForEach(chargeQuestionAnswer => {
                    var answer = context
                        .Query<AccountChargeQuestionAnswer>()
                        .FirstOrDefault(a => a.AccountId == chargeQuestionAnswer.AccountId
                        && a.AccountChargeQuestionId == chargeQuestionAnswer.AccountChargeQuestionId
                        && a.AccountChargeId == chargeQuestionAnswer.AccountChargeId);

                    if (answer == null)
                    {
                        context.Save(chargeQuestionAnswer);

                    }
                    else
                    {
                        answer.LastAnswer = chargeQuestionAnswer.LastAnswer;
                        context.Save(answer);
                    }
                });
            }
        }
    }
}