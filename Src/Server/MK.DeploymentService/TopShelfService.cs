#region

using System;
using log4net;
using Topshelf;

#endregion

namespace MK.DeploymentService
{
    public class TopShelfService
    {
        private static ILog _logger;

        private static void Main()
        {
            _logger = LogManager.GetLogger("DeploymentJobService");
            _logger.Debug("Service started");

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            HostFactory.Run(x =>
            {
                x.Service<DeploymentJobService>(s =>
                {
                    s.ConstructUsing(name => new DeploymentJobService());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsPrompt();

                x.SetDescription("TaxiHail Deployment Service");
                x.SetDisplayName("TaxiHailDeploymentService");
                x.SetServiceName("TaxiHailDeploymentService");
            });
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Error("UnhandledException", e.ExceptionObject as Exception);
        }
    }
}