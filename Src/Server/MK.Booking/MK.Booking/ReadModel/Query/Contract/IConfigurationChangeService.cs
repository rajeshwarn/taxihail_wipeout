using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IConfigurationChangeService
    {
        IList<ConfigurationChangeEntry> GetAll();
        void Add(ConfigurationChangeEntry entry);
        void Delete(Guid id);
        void DeleteAll();
    }
}
