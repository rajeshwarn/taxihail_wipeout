using System.Configuration;
using System.Data.Common;
using System.Data.Entity.Infrastructure;

namespace apcurium.MK.Common.Entity
{
    public class ServiceConfigurationSettingConnectionFactory : IDbConnectionFactory
    {
        private readonly IDbConnectionFactory parent;

        public ServiceConfigurationSettingConnectionFactory(IDbConnectionFactory parent)
        {
            this.parent = parent;
        }

        public DbConnection CreateConnection(string nameOrConnectionString)
        {
            if (!IsConnectionString(nameOrConnectionString))
            {
                var connectionStringName = "DbContext." + nameOrConnectionString;
                try
                {
                    var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName];
                    if (connectionString != null)
                    {
                        nameOrConnectionString = connectionString.ConnectionString;
                    }
                }
                catch (ConfigurationErrorsException e)
                {
                }
                
            }

            return this.parent.CreateConnection(nameOrConnectionString);
        }

        private static bool IsConnectionString(string connectionStringCandidate)
        {
            return (connectionStringCandidate.IndexOf('=') >= 0);
        }
    }
}
