#region

using System.Configuration;
using System.Data.Common;
using System.Data.Entity.Infrastructure;

#endregion

namespace apcurium.MK.Common.Database
{
    public class ServiceConfigurationSettingConnectionFactory : IDbConnectionFactory
    {
        private readonly IDbConnectionFactory _parent;

        public ServiceConfigurationSettingConnectionFactory(IDbConnectionFactory parent)
        {
            this._parent = parent;
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

            return _parent.CreateConnection(nameOrConnectionString);
        }

        private static bool IsConnectionString(string connectionStringCandidate)
        {
            return (connectionStringCandidate.IndexOf('=') >= 0);
        }
    }
}