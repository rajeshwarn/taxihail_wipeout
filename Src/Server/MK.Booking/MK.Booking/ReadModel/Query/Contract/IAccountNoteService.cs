using System;
using System.Collections.Generic;

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IAccountNoteService
    {
        IList<AccountNoteEntry> GetAll();
        AccountNoteEntry FindById(Guid id);
        IList<AccountNoteEntry> FindByAccountId(Guid accountId);
        void Add(AccountNoteEntry accountNoteEntry);
    }
}
