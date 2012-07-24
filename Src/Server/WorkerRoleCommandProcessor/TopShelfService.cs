using System.Diagnostics;
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
            var container = new UnityContainer();
            new Module().Init(container);
            Host h = HostFactory.New(x =>
            {
                x.Service<MKBookingProcessor>(s =>
                {
                    s.SetServiceName("MkBookingProcessor");
                    s.ConstructUsing(name => new MKBookingProcessor(container));
                    s.WhenStarted(tc =>
                                      {
                                          XmlConfigurator.ConfigureAndWatch(new FileInfo(".\\log4net.xml"));
                                          Trace.TraceInformation("Starting the Service...");
                                          tc.Start();
                                          Trace.TraceInformation("Service Started.");
                                      });
                    s.WhenStopped(tc =>
                                      {
                                          Trace.TraceInformation("Stoping the Service...");
                                          tc.Stop();
                                          Trace.TraceInformation("Service Stopped.");
                                      });
                });

                x.RunAsLocalSystem();
                x.DependsOnMsSql();
                x.SetDescription("Command and Events Processor");
                x.SetDisplayName("MkBookingProcessor");
                x.SetServiceName("MkBookingProcessor");
            });
            
            h.Run();
        }
    }
}