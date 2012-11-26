using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using MK.ConfigurationManager;
using MK.ConfigurationManager.Entities;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Web.Administration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using log4net;

namespace MK.DeploymentService
{
    public class DeploymentJobService
    {
        private readonly Timer timer;
        private readonly ReaderWriterLockSlim @lock = new ReaderWriterLockSlim();
        private readonly ILog logger;
        
        public DeploymentJobService()
        {
            timer = new Timer(TimerOnElapsed, null, Timeout.Infinite, Timeout.Infinite);
            logger = LogManager.GetLogger("DeploymentJobService");
        }

        public void Start()
        {
            Database.SetInitializer<ConfigurationManagerDbContext>(null);
            timer.Change(0, 2000);
        }

        private void TimerOnElapsed(object state)
        {
            try
            {
                if (@lock.TryEnterWriteLock(0))
                {
                    CheckAndRunJobWithBuild();
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

        private void CheckAndRunJobWithBuild()
        {
            var DbContext = new ConfigurationManagerDbContext(System.Configuration.ConfigurationManager.ConnectionStrings["MKConfig"].ConnectionString);
            var job = DbContext.Set<DeploymentJob>()
                .Include(x => x.Company)
                .Include(x => x.IBSServer)
                .Include(x => x.TaxHailEnv)
                .FirstOrDefault(x => x.Status == JobStatus.REQUESTED && (x.DeployServer || x.DeployDB));

            if(job != null)
            {
                try
                {
                    logger.DebugFormat("New Job for {0}", job.Company.Name);
                    job.Status = JobStatus.INPROGRESS;
                    DbContext.SaveChanges();

                    var sourceDirectory = Path.Combine(Path.GetTempPath(), "TaxiHailSource");
                    if (Properties.Settings.Default.Mode == "Build")
                    {
                        FetchSourceAndBuild(job, sourceDirectory);
                    }

                    //build server and deploy
                    if (job.DeployServer || job.DeployDB)
                    {
                        var packagesDirectory = Path.Combine(sourceDirectory, "Deployment\\Server\\Package\\");
                        if (Properties.Settings.Default.Mode != "Build")
                        {
                            packagesDirectory = Properties.Settings.Default.DeployFolder;
                        }
                        DeployTaxiHail(job, packagesDirectory);
                        logger.DebugFormat("Job Done");
                    }

                    job.Details = string.Empty;
                    job.Status = JobStatus.SUCCESS;
                    DbContext.SaveChanges();

                }catch(Exception e)
                {
                    logger.Error(e.Message, e);
                    job.Status = JobStatus.ERROR;
                    job.Details = e.Message;
                    DbContext.SaveChanges();
                }
            }
        }

        private void FetchSourceAndBuild(DeploymentJob job, string sourceDirectory)
        {
            //pull source from bitbucket if not done yet
            var revision = string.IsNullOrEmpty(job.Revision) ? string.Empty : "-r " + job.Revision;

            if (!Directory.Exists(sourceDirectory))
            {
                logger.DebugFormat("Clone Source Code");
                Directory.CreateDirectory(sourceDirectory);
                var args = string.Format(@"clone {1} https://buildapcurium:apcurium5200!@bitbucket.org/apcurium/mk-taxi {0}",
                                         sourceDirectory, revision);

                var hgClone = new ProcessStartInfo
                                  {
                                      FileName = "hg.exe",
                                      WindowStyle = ProcessWindowStyle.Hidden,
                                      UseShellExecute = false,
                                      CreateNoWindow = false,
                                      Arguments = args
                                  };

                using (var exeProcess = Process.Start(hgClone))
                {
                    exeProcess.WaitForExit();
                    if (exeProcess.ExitCode > 0)
                    {
                        throw new Exception("Error during clone source code step");
                    }
                }
            }
            else
            {
                logger.DebugFormat("Revert, Purge and Update Source Code");
                //already clone just do a revert and update the source
                RevertAndPull(sourceDirectory);
            }


            if (!string.IsNullOrEmpty(job.Revision))
            {
                logger.DebugFormat("Update to revision {0}", job.Revision);
                var hgUpdate = new ProcessStartInfo
                                   {
                                       FileName = "hg.exe",
                                       WindowStyle = ProcessWindowStyle.Hidden,
                                       UseShellExecute = false,
                                       CreateNoWindow = false,
                                       Arguments =
                                           string.Format("update --repository {0} -r {1}", sourceDirectory, job.Revision)
                                   };

                using (var exeProcess = Process.Start(hgUpdate))
                {
                    exeProcess.WaitForExit();
                    if (exeProcess.ExitCode > 0)
                    {
                        throw new Exception("Error during revert source code step");
                    }
                }
            }

            logger.DebugFormat("Build Databse Initializer");
            var slnFilePath = Path.Combine(sourceDirectory, @"Src\Server\") + "MKBooking.sln";
            var pc = new ProjectCollection();
            var globalProperty = new Dictionary<string, string> {{"Configuration", "Release"}};
            var buildRequestData = new BuildRequestData(slnFilePath, globalProperty, null, new[] { "Build" }, null);
            var buildResult = BuildManager.DefaultBuildManager.Build(new BuildParameters(pc), buildRequestData);

            if(buildResult.Exception != null)
            {
                throw new Exception(buildResult.Exception.Message, buildResult.Exception);
            }

            var targetDir = Path.Combine(sourceDirectory, @"Deployment\Server\Package\DatabaseInitializer");
            var sourcePath = Path.Combine(sourceDirectory, @"Src\Server\DatabaseInitializer\bin\Release");
            CopyFiles(sourcePath, targetDir);

            logger.DebugFormat("Build Web Site");
            slnFilePath = Path.Combine(sourceDirectory, @"Src\Server\apcurium.MK.Web\") + "apcurium.MK.Web.csproj";
            buildRequestData = new BuildRequestData(slnFilePath, globalProperty, null, new[] { "Package" }, null);
            var buildResultWeb = BuildManager.DefaultBuildManager.Build(new BuildParameters(pc), buildRequestData);

            if (buildResultWeb.Exception != null)
            {
                throw new Exception(buildResultWeb.Exception.Message, buildResult.Exception);
            }

            targetDir = Path.Combine(sourceDirectory, @"Deployment\Server\Package\WebSites");
            sourcePath = Path.Combine(sourceDirectory, @"Src\Server\apcurium.MK.Web\obj\Release\Package\PackageTmp");
            CopyFiles(sourcePath, targetDir);
        }

        private void CopyFiles(string source, string target)
        {
            if (Directory.Exists(target))
            {
                Directory.Delete(target, true);
            }
            Directory.CreateDirectory(target);

            foreach (var dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(source, target));

            foreach (var newPath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(source, target));
        }

        private void DeployTaxiHail(DeploymentJob job, string packagesDirectory)
        {
            logger.DebugFormat("Deploying");
            var companyName = job.Company.ConfigurationProperties["TaxiHail.ServerCompanyName"];
            var iisManager = new ServerManager();
            var appPool = iisManager.ApplicationPools.FirstOrDefault(x => x.Name == companyName);
            if (appPool == null)
            {
                //create a new one
                appPool = iisManager.ApplicationPools.Add(companyName);
                appPool.ManagedRuntimeVersion = "v4.0";
                iisManager.CommitChanges();
                Thread.Sleep(2000);
            }
            if (appPool.State == ObjectState.Started) appPool.Stop();

            if (job.DeployDB)
            {
                DeployDataBase(job, packagesDirectory, companyName);
            }

            if (job.DeployServer)
            {
                DeployServer(job, companyName, packagesDirectory, iisManager);
            }
            appPool.Start();
        }

        private void DeployDataBase(DeploymentJob job, string packagesDirectory, string companyName)
        {
            logger.DebugFormat("Deploying DB");
            var jsonSettings = new JObject();
            foreach (var setting in job.Company.ConfigurationProperties)
            {
                jsonSettings.Add(setting.Key, JToken.FromObject(setting.Value));
            }

            jsonSettings.Add("IBS.WebServicesUrl", JToken.FromObject(job.IBSServer.Url));
            jsonSettings.Add("IBS.WebServicesUserName", JToken.FromObject(job.IBSServer.Username));
            jsonSettings.Add("IBS.WebServicesPassword", JToken.FromObject(job.IBSServer.Password));

            var fileSettings = Path.Combine(packagesDirectory, "DatabaseInitializer\\Settings\\") +
                               companyName + ".json";
            var stringBuilder = new StringBuilder();
            jsonSettings.WriteTo(new JsonTextWriter(new StringWriter(stringBuilder)));
            File.WriteAllText(fileSettings, stringBuilder.ToString());

            var deployDB = new ProcessStartInfo
                               {
                                   FileName = Path.Combine(packagesDirectory, "DatabaseInitializer\\") +"DatabaseInitializer.exe",
                                   WindowStyle = ProcessWindowStyle.Hidden,
                                   UseShellExecute = false,
                                   LoadUserProfile = true,
                                   CreateNoWindow = false,
                                   Arguments =
                                       string.Format("{0} {1} {2}", companyName, job.InitDatabase ? "C" : "U",
                                                     job.TaxHailEnv.SqlServerInstance),
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

        private void DeployServer(DeploymentJob job, string companyName, string packagesDirectory, ServerManager iisManager)
        {
            logger.DebugFormat("Deploying IIS");
            var subFolder = job.Company.ConfigurationProperties["TaxiHail.Version"] + job.Revision + "." + DateTime.Now.Ticks +"\\";
            var targetWeDirectory = Path.Combine(job.TaxHailEnv.WebSitesFolder, companyName, subFolder);
            var sourcePath = Path.Combine(packagesDirectory, @"WebSites\");

            CopyFiles(sourcePath, targetWeDirectory);

            var website = iisManager.Sites["Default Web Site"];
            var webApp = website.Applications.FirstOrDefault(x => x.Path == "/" + companyName);
            if (webApp != null)
            {
                webApp.VirtualDirectories["/"].PhysicalPath = targetWeDirectory;
                iisManager.CommitChanges();
            }
            else
            {
                webApp = website.Applications.Add("/" + companyName, targetWeDirectory);
                webApp.ApplicationPoolName = companyName;
                iisManager.CommitChanges();
            }

            var configuration = webApp.GetWebConfiguration();
            var section =
                configuration.GetSection("connectionStrings").GetCollection().First(
                    x => x.Attributes["name"].Value.ToString() == "MKWeb");
            var connSting = section.Attributes["connectionString"];
            connSting.Value =
                string.Format("Data Source=.;Initial Catalog={0};Integrated Security=True; MultipleActiveResultSets=True",
                              companyName);

            //log4net comn
            var document = XDocument.Load(targetWeDirectory + "log4net.xml");

            var atttribute = (from XElement e in document.Descendants("appender")
                              where e.Attribute("name").Value == "Courriel"
                              select e).FirstOrDefault();

            if (atttribute != null)
            {
                var attribute = atttribute.Descendants("subject").Attributes("value").FirstOrDefault();
                if (attribute != null)
                    attribute.SetValue(string.Format("[{0}] - TaxiHail Error", companyName));
            }

            document.Save(targetWeDirectory + "log4net.xml");


            iisManager.CommitChanges();
        }

        private void RevertAndPull(string repository)
        {
            var hgRevert = new ProcessStartInfo
                               {
                                   FileName = "hg.exe",
                                   WindowStyle = ProcessWindowStyle.Hidden,
                                   UseShellExecute = false,
                                   CreateNoWindow = false,
                                   Arguments = string.Format("update --repository {0} -C -r default", repository)
                               };

            using (var exeProcess = Process.Start(hgRevert))
            {
                exeProcess.WaitForExit();
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during revert source code step");
                }
            }

            var hgPurge = new ProcessStartInfo
                              {
                                  FileName = "hg.exe",
                                  WindowStyle = ProcessWindowStyle.Hidden,
                                  UseShellExecute = false,
                                  CreateNoWindow = false,
                                  Arguments = string.Format("purge --all --repository {0}", repository)
                              };

            using (var exeProcess = Process.Start(hgPurge))
            {
                exeProcess.WaitForExit();
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during purge source code step");
                }
            }

            var hgPull = new ProcessStartInfo
                             {
                                 FileName = "hg.exe",
                                 WindowStyle = ProcessWindowStyle.Hidden,
                                 UseShellExecute = false,
                                 CreateNoWindow = false,
                                 Arguments = string.Format("pull https://buildapcurium:apcurium5200!@bitbucket.org/apcurium/mk-taxi --repository {0}", repository)
                             };

            using (var exeProcess = Process.Start(hgPull))
            {
                exeProcess.WaitForExit();
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during pull source code step");
                }
            }

            var hgUpdate = new ProcessStartInfo
            {
                FileName = "hg.exe",
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                CreateNoWindow = false,
                Arguments = string.Format("update --repository {0}", repository)
            };

            using (var exeProcess = Process.Start(hgUpdate))
            {
                exeProcess.WaitForExit();
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during revert source code step");
                }
            }
        }

        public void Stop()
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}