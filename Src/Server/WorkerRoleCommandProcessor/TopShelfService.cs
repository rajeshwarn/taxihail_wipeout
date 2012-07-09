using Topshelf;
using System.Data.Entity;
using apcurium.MK.Common.Entity;

namespace WorkerRoleCommandProcessor
{
    public class TopShelfService
    {
        static void Main(string[] args)
        {
            //XmlConfigurator.ConfigureAndWatch(new FileInfo(".\\log4net.config"));
            Database.DefaultConnectionFactory = new ServiceConfigurationSettingConnectionFactory(Database.DefaultConnectionFactory);

            Host h = HostFactory.New(x =>
            {
                x.Service<MKBookingProcessor>(s =>
                {
                    s.SetServiceName("MkBookingProcessor");
                    s.ConstructUsing(name => new MKBookingProcessor());
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