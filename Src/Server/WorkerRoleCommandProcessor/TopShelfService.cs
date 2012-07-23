using Microsoft.Practices.Unity;
using Topshelf;
using System.Data.Entity;
using apcurium.MK.Common.Entity;

namespace WorkerRoleCommandProcessor
{
    public class TopShelfService
    {
#if STAGING
        const string databaseName = "MkWebStaging";
#else
        const string DatabaseName = "MkWeb";
#endif
        static void Main(string[] args)
        {
            //XmlConfigurator.ConfigureAndWatch(new FileInfo(".\\log4net.config"));
            Database.DefaultConnectionFactory = new ServiceConfigurationSettingConnectionFactory(Database.DefaultConnectionFactory);
            var container = new UnityContainer();
            new Module().Init(container, DatabaseName);
            Host h = HostFactory.New(x =>
            {
                x.Service<MKBookingProcessor>(s =>
                {
                    s.SetServiceName("MkBookingProcessor");
                    s.ConstructUsing(name => new MKBookingProcessor(container));
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.RunAsLocalSystem();

                x.SetDescription("Command and Events Processor");
                x.SetDisplayName("MkBookingProcessor");
                x.SetServiceName("MkBookingProcessor");
            });

            h.Run();
        }
    }
}