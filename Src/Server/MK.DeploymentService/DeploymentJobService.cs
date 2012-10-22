using System;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using MK.ConfigurationManager;
using MK.ConfigurationManager.Entities;

namespace MK.DeploymentService
{
    public class DeploymentJobService
    {
        private readonly Timer timer;
        private readonly ReaderWriterLockSlim @lock = new ReaderWriterLockSlim();
        ConfigurationManagerDbContext DbContext { get; set; }

        public DeploymentJobService()
        {
            timer = new Timer(TimerOnElapsed, null, Timeout.Infinite, Timeout.Infinite);
        }

        private void TimerOnElapsed(object state)
        {
            try
            {
                if (@lock.TryEnterWriteLock(0))
                {
                    CheckAndRunJob();
                }

            }
            catch (LockRecursionException)
            {
                Debug.WriteLine("LockRecursionException");
            }
            finally
            {
                if (@lock.IsWriteLockHeld) @lock.ExitWriteLock();
            }
        }

        private void CheckAndRunJob()
        {
            var job = DbContext.Set<DeploymentJob>()
                .Include(x => x.Company)
                .Include(x => x.IBSServer)
                .Include(x => x.TaxHailEnv)
                .FirstOrDefault(x => x.Status == JobStatus.REQUESTED);
            if(job != null)
            {
                try
                {
                    job.Status = JobStatus.INPROGRESS;
                    DbContext.SaveChanges();

                    //pull source from bitbucket
                    var revision = string.IsNullOrEmpty(job.Revision) ? string.Empty : "-r " + job.Revision;
                    var directory = Path.Combine(Path.GetTempPath(), job.Id.ToString());
                    var args = string.Format(@"clone {1} https://buildapcurium:apcurium5200!@bitbucket.org/apcurium/mk-taxi {0}", directory , revision);

                    var hgClone = new ProcessStartInfo
                    {
                        FileName = "hg.exe",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        CreateNoWindow = false,
                        Arguments = args
                    };

                    //using (var exeProcess = Process.Start(hgClone))
                    //{
                    //    exeProcess.WaitForExit();
                        //if (exeProcess.ExitCode > 0)
                        //{
                        //    throw new Exception("Error during pull source code step");
                        //}
                    //}


                    //build server?
                    if(job.Server)
                    {
                        var buildPackage = new ProcessStartInfo
                        {
                            FileName = "Powershell.exe",
                            WindowStyle = ProcessWindowStyle.Hidden,
                            UseShellExecute = false,
                            LoadUserProfile = true,
                            CreateNoWindow = false,
                            Arguments = string.Format("-executionpolicy ByPass -File \"" + directory + "\\Deployment\\Server\\BuildPackage.ps1\" \"-env {0}\"", job.Company.ConfigurationProperties["TaxiHail.ApplicationKey"]) 
                        };

                        using (var exeProcess = Process.Start(buildPackage))
                        {
                            exeProcess.WaitForExit();
                            if(exeProcess.ExitCode > 0)
                            {
                                throw  new Exception("Error during build step");
                            }
                        }
                    }



                    //job.Status = JobStatus.SUCCESS;
                    DbContext.SaveChanges();

                }catch(Exception e)
                {
                    job.Status = JobStatus.ERROR;
                    job.Details = e.Message;
                    DbContext.SaveChanges();
                }
            }
        }

        public void Start()
        {
            Database.SetInitializer<ConfigurationManagerDbContext>(null);
            DbContext = new ConfigurationManagerDbContext(System.Configuration.ConfigurationManager.ConnectionStrings["MKConfig"].ConnectionString);
            timer.Change(0, 2000);
        }

        

        public void Stop()
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}