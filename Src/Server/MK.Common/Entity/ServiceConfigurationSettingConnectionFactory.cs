using System.Configuration;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace MoveOn.Common.Entity
{
    public class ServiceConfigurationSettingConnectionFactory : IDbConnectionFactory
    {
        private IDbConnectionFactory parent;

        public ServiceConfigurationSettingConnectionFactory(IDbConnectionFactory parent)
        {
            this.parent = parent;
        }

        public DbConnection CreateConnection(string nameOrConnectionString)
        {
            if (!IsConnectionString(nameOrConnectionString))
            {
                var connectionStringName = "DbContext." + nameOrConnectionString;

                if (RoleEnvironment.IsAvailable)
                {
                    try
                    {
                        var settingValue = RoleEnvironment.GetConfigurationSettingValue(connectionStringName);
                        if (!string.IsNullOrEmpty(settingValue))
                        {
                            nameOrConnectionString = settingValue;
                        }
                    }
                    catch (RoleEnvironmentException)
                    {
                        // setting does not exist, use original value
                    }
                }
                else
                {
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
            }

            return this.parent.CreateConnection(nameOrConnectionString);
        }

        private static bool IsConnectionString(string connectionStringCandidate)
        {
            return (connectionStringCandidate.IndexOf('=') >= 0);
        }
    }
}
