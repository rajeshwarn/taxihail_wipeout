using System.IO;
using Microsoft.Practices.Unity;
using Topshelf;
using log4net.Config;

namespace WorkerRoleCommandProcessor
{
    public class TopShelfService
    {
        static void Main(string[] args)
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo(".\\log4net.xml"));
            var container = new UnityContainer();
            new Module().Init(container);
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