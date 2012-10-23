using System;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using MK.ConfigurationManager;
using MK.ConfigurationManager.Entities;
using Microsoft.Web.Administration;

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
                    var sourceDirectory = Path.Combine(Path.GetTempPath(), job.Id.ToString());
                    //if(Directory.Exists(directory))
                    //{
                    //    Directory.Delete(directory, true);
                    //}
                    //var args = string.Format(@"clone {1} https://buildapcurium:apcurium5200!@bitbucket.org/apcurium/mk-taxi {0}", directory , revision);

                    //var hgClone = new ProcessStartInfo
                    //{
                    //    FileName = "hg.exe",
                    //    WindowStyle = ProcessWindowStyle.Hidden,
                    //    UseShellExecute = false,
                    //    CreateNoWindow = false,
                    //    Arguments = args
                    //};

                    //using (var exeProcess = Process.Start(hgClone))
                    //{
                    //    exeProcess.WaitForExit();
                    //    if (exeProcess.ExitCode > 0)
                    //    {
                    //        throw new Exception("Error during pull source code step");
                    //    }
                    //}


                    //build server and deploy
                    if (job.DeployServer || job.DeployDB)
                    {
                        var buildPackage = new ProcessStartInfo
                                               {
                                                   FileName = "Powershell.exe",
                                                   WindowStyle = ProcessWindowStyle.Hidden,
                                                   UseShellExecute = false,
                                                   LoadUserProfile = true,
                                                   CreateNoWindow = false,
                                                   Arguments =
                                                       "-Executionpolicy ByPass -File \"" + sourceDirectory +
                                                       "\\Deployment\\Server\\BuildPackage.ps1\""
                                               };

                        //using (var exeProcess = Process.Start(buildPackage))
                        //{
                        //    exeProcess.WaitForExit();
                        //    if (exeProcess.ExitCode > 0)
                        //    {
                        //        throw new Exception("Error during build step");
                        //    }
                        //}

                        var companyName = job.Company.ConfigurationProperties["TaxiHail.ApplicationKey"];
                        var iisManager = new ServerManager();
                        var appPool = iisManager.ApplicationPools.FirstOrDefault(x => x.Name == companyName);
                        if (appPool == null)
                        {
                            //create a new one
                            appPool = iisManager.ApplicationPools.Add(companyName);
                            appPool.ManagedRuntimeVersion = "v4.0";
                            iisManager.CommitChanges();
                        }
                        if (appPool.State == ObjectState.Started) appPool.Stop();

                        if(job.DeployDB)
                        {
                            //TODO add company settings from DB

                            var deployDB = new ProcessStartInfo
                            {
                                FileName = Path.Combine(sourceDirectory, "Deployment\\Server\\Package\\DatabaseInitializer\\") + "DatabaseInitializer.exe",
                                WindowStyle = ProcessWindowStyle.Hidden,
                                UseShellExecute = false,
                                LoadUserProfile = true,
                                CreateNoWindow = false,
                                Arguments = string.Format("{0} {1} {2}", companyName ,job.InitDatabase ? "C" : "U", job.TaxHailEnv.SqlServerInstance),
                            };

                            using (var exeProcess = Process.Start(deployDB))
                            {
                                exeProcess.WaitForExit();
                                if (exeProcess.ExitCode > 0)
                                {
                                    throw new Exception("Error during deploy DB step");
                                }
                            }
                        }

                        if(job.DeployServer)
                        {
                            var subFolder = job.Version + job.Revision+ "\\";
                            var targetWeDirectory = Path.Combine(job.TaxHailEnv.WebSitesFolder, companyName, subFolder);
                            
                            if(Directory.Exists(targetWeDirectory))
                            {
                                Directory.Delete(targetWeDirectory, true);
                            }
                            Directory.CreateDirectory(targetWeDirectory);

                            var sourcePath = Path.Combine(sourceDirectory, @"Deployment\Server\Package\WebSites\");

                            foreach (var dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetWeDirectory));

                            foreach (var newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                                File.Copy(newPath, newPath.Replace(sourcePath, targetWeDirectory));

                            var website = iisManager.Sites["Default Web Site"];
                            var webApp = website.Applications.FirstOrDefault(x => x.Path == "/" + companyName);
                            if(webApp != null)
                            {
                                webApp.VirtualDirectories["/"].PhysicalPath = targetWeDirectory;
                                iisManager.CommitChanges();
                                
                            }else
                            {
                                webApp = website.Applications.Add("/" + companyName, targetWeDirectory);
                                iisManager.CommitChanges();
                            }

                            var configuration = webApp.GetWebConfiguration();
                            var section = configuration.GetSection("connectionStrings").GetCollection().First(x => x.Attributes["name"].Value.ToString() == "MKWeb");
                            var connSting = section.Attributes["connectionString"];
                            connSting.Value = string.Format("Data Source=.;Initial Catalog={0};Integrated Security=True; MultipleActiveResultSets=True", companyName);
                            iisManager.CommitChanges();

                        }

                        appPool.Start();

                    }


                    //job.Status = JobStatus.SUCCESS;
                    //DbContext.SaveChanges();

                }catch(Exception e)
                {
                    //job.Status = JobStatus.ERROR;
                    //job.Details = e.Message;
                    //DbContext.SaveChanges();
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