using System;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers
{
    public class AccountChargeDetailGenerator :
        IEventHandler<AccountChargeAddedUpdated>,
        IEventHandler<AccountChargeDeleted>,
        IEventHandler<AccountChargeImported>,
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public AccountChargeDetailGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(AccountChargeAddedUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var accountChargeDetail = context.Find<AccountChargeDetail>(@event.AccountChargeId);
                if (accountChargeDetail == null)
                {
                    accountChargeDetail = new AccountChargeDetail {Id = @event.AccountChargeId};
                }
                accountChargeDetail.Number = @event.Number;
                accountChargeDetail.Name = @event.Name;
                accountChargeDetail.Questions.Clear();
                context.Save(accountChargeDetail);

                accountChargeDetail.Questions.AddRange(@event.Questions);
                foreach (var question in accountChargeDetail.Questions)
                {
                    question.AccountId = accountChargeDetail.Id;
                }
                context.Save(accountChargeDetail);
            }
        }

        public void Handle(AccountChargeImported @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                // TODO: Should we check for same Guid in db context?

                foreach (var account in @event.AccountCharges)
                {
                    var accountChargeDetail = new AccountChargeDetail { Id = account.AccountChargeId };
                
                    accountChargeDetail.Number = account.Number;
                    accountChargeDetail.Name = account.Name;
                    accountChargeDetail.Questions.Clear();
                    context.Save(accountChargeDetail);

                    accountChargeDetail.Questions.AddRange(account.Questions);
                    foreach (var question in accountChargeDetail.Questions)
                    {
                        question.AccountId = accountChargeDetail.Id;
                    }

                    context.Save(accountChargeDetail);
                }
            }
        }

        public void Handle(AccountChargeDeleted @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var existing = context.Find<AccountChargeDetail>(@event.AccountChargeId);
                if (existing != null)
                {
                    context.Set<AccountChargeDetail>().Remove(existing);
                    context.SaveChanges();
                }
            }
        }
    }
}