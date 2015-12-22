using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IBlackListEntryService
    {
        IList<BlackListEntry> GetAll();
        void Add(BlackListEntry entry);
        void Delete(Guid id);
        void DeleteAll();
        BlackListEntry FindByPhoneNumber(string phoneNumber);
    }
}
