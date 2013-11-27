using System;
using Topshelf;

namespace MK.DeploymentService
{
    public class TopShelfService
    {
        static void Main(string[] args)
        {

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

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            
        }
    }
}