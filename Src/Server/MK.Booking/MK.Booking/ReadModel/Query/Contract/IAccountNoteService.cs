using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IAccountNoteService
    {
        IList<AccountNoteEntry> GetAll();
        AccountNoteEntry FindById(Guid id);
        IList<AccountNoteEntry> FindByAccountId(string accountId);
        void Add(AccountNoteEntry accountNoteEntry);
    }
}
