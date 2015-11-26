using System.Linq;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Projections;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers
{
    public class AccountIbsDetailGenerator :
        IEventHandler<AccountLinkedToIbs>,
        IEventHandler<AccountUnlinkedFromIbs>
    {
        private readonly AccountIbsDetailProjectionSet _accountIbsProjectionSet;

        public AccountIbsDetailGenerator(AccountIbsDetailProjectionSet accountIbsProjectionSet)
        {
            _accountIbsProjectionSet = accountIbsProjectionSet;
        }

        public void Handle(AccountLinkedToIbs @event)
        {
            if (@event.CompanyKey.HasValue())
            {
                _accountIbsProjectionSet.Update(@event.SourceId, list =>
                {
                    var existing = list.FirstOrDefault(x => x.AccountId == @event.SourceId && x.CompanyKey == @event.CompanyKey);

                    if (existing == null)
                    {
                        list.Add(new AccountIbsDetail
                        {
                            AccountId = @event.SourceId,
                            CompanyKey = @event.CompanyKey,
                            IBSAccountId = @event.IbsAccountId
                        });
                    }
                    else
                    {
                        existing.IBSAccountId = @event.IbsAccountId;
                    }
                });
            }
        }

        public void Handle(AccountUnlinkedFromIbs @event)
        {
            _accountIbsProjectionSet.Update(@event.SourceId, list =>
            {
                list.RemoveAll(x => x.AccountId == @event.SourceId);
            });
        }
    }
}