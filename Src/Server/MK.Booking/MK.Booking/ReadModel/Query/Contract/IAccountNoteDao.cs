using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IAccountNoteDao
    {
        IList<AccountNoteDetail> GetAll();
        AccountNoteDetail FindById(Guid id);
        IList<AccountNoteDetail> FindByAccountId(Guid accountId);
    }
}
