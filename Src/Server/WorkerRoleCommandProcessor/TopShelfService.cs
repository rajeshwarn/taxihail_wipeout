using Topshelf;

namespace WorkerRoleCommandProcessor
{
    public class TopShelfService
    {
        static void Main(string[] args)
        {
            //XmlConfigurator.ConfigureAndWatch(new FileInfo(".\\log4net.config"));

            Host h = HostFactory.New(x =>
            {
                x.Service<MkBookingProcessor>(s =>
                {
                    s.SetServiceName("MkBookingProcessor");
                    s.ConstructUsing(name => new MkBookingProcessor());
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