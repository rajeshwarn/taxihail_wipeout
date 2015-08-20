using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers
{
    public class AccountIbsDetailGenerator :
        IEventHandler<AccountLinkedToIbs>,
        IEventHandler<AccountUnlinkedFromIbs>
    {

        private readonly Func<BookingDbContext> _contextFactory;

        public AccountIbsDetailGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(AccountLinkedToIbs @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                if (@event.CompanyKey.HasValue())
                {
                    var ibsAccountLink =
                        context.Query<AccountIbsDetail>()
                                .FirstOrDefault(x => x.AccountId == @event.SourceId 
                                                     && x.CompanyKey == @event.CompanyKey)
                            ?? new AccountIbsDetail
                            {
                                AccountId = @event.SourceId,
                                CompanyKey = @event.CompanyKey
                            };

                    ibsAccountLink.IBSAccountId = @event.IbsAccountId;

                    context.Save(ibsAccountLink);
                }
            }
        }

        public void Handle(AccountUnlinkedFromIbs @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.RemoveWhere<AccountIbsDetail>(x => x.AccountId == @event.SourceId);
                context.SaveChanges();
            }
        }
    }
}