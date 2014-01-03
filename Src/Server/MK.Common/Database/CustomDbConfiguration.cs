using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace apcurium.MK.Common.Database
{
    public class CustomDbConfiguration : DbConfiguration
    {
        public CustomDbConfiguration()
        {
            SetDefaultConnectionFactory(new ServiceConfigurationSettingConnectionFactory(new SqlConnectionFactory()));
        }
    }
}