#region

using System.Configuration;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using apcurium.MK.Common.Diagnostic;

#endregion

namespace apcurium.MK.Common.Database
{
    public class ServiceConfigurationSettingConnectionFactory : IDbConnectionFactory
    {
        private readonly IDbConnectionFactory _parent;
        private readonly Logger _logger;

        public ServiceConfigurationSettingConnectionFactory(IDbConnectionFactory parent)
        {
            _parent = parent;
            _logger = new Logger();
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
                    _logger.LogError(e);
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