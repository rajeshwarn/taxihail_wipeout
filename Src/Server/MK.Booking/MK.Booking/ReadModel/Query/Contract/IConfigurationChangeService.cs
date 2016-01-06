using System.Collections.Generic;

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IConfigurationChangeService
    {
        IList<ConfigurationChangeEntry> GetAll();
        void Add(Dictionary<string, string> oldValues, Dictionary<string, string> newValues, ConfigurationChangeType type, string accountId, string email);
    }
}
