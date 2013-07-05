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
        private readonly Timer _timer;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly ILog _logger;

        public DeploymentJobService()
        {
            _timer = new Timer(TimerOnElapsed, null, Timeout.Infinite, Timeout.Infinite);
            _logger = LogManager.GetLogger("DeploymentJobService");
        }

        public void Start()
        {
            Database.SetInitializer<ConfigurationManagerDbContext>(null);
            _timer.Change(0, 2000);
        }

        private void TimerOnElapsed(object state)
        {
            try
            {
                if (_lock.TryEnterWriteLock(0))
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
                if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
            }
        }

        private void CheckAndRunJobWithBuild()
        {
            var dbContext = new ConfigurationManagerDbContext(System.Configuration.ConfigurationManager.ConnectionStrings["MKConfig"].ConnectionString);
            var job = dbContext.Set<DeploymentJob>()
                .Include(x => x.Company)
                .Include(x => x.IBSServer)
                .Include(x => x.TaxHailEnv)
                .FirstOrDefault(x => x.Status == JobStatus.REQUESTED && (x.DeployServer || x.DeployDB));

            if (job != null)
            {
                try
                {
                    _logger.DebugFormat("New Job for {0}", job.Company.Name);
                    job.Status = JobStatus.INPROGRESS;
                    dbContext.SaveChanges();

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
                        _logger.DebugFormat("Job Done");
                    }

                    job.Details = string.Empty;
                    job.Status = JobStatus.SUCCESS;
                    dbContext.SaveChanges();

                }
                catch (Exception e)
                {
                    _logger.Error(e.Message, e);
                    job.Status = JobStatus.ERROR;
                    job.Details = e.Message;
                    dbContext.SaveChanges();
                }
            }
        }

        private string GetRevisionNumber(DeploymentJob job)
        {
            if (job.Version != null)
            {
                return job.Version.Revision;
            }

            return string.IsNullOrEmpty(job.Revision) ? string.Empty : job.Revision;
        }

        private void FetchSourceAndBuild(DeploymentJob job, string sourceDirectory)
        {
            //pull source from bitbucket if not done yet
            var revisionNumber = GetRevisionNumber(job);
            var revision = string.IsNullOrEmpty(revisionNumber) ? string.Empty : "-r " + revisionNumber;

            if (!Directory.Exists(sourceDirectory))
            {
                _logger.DebugFormat("Clone Source Code");
                Directory.CreateDirectory(sourceDirectory);

                var hgClone = GetProcess("hg.exe", string.Format(@"clone {1} https://buildapcurium:apcurium5200!@bitbucket.org/apcurium/mk-taxi {0}", sourceDirectory, revision));
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
                _logger.DebugFormat("Revert, Purge and Update Source Code");
                //already clone just do a revert and update the source
                RevertAndPull(sourceDirectory);
            }

            if (!string.IsNullOrEmpty(revision))
            {
                _logger.DebugFormat("Update to revision {0}", revision);
                var hgUpdate = GetProcess("hg.exe", string.Format("update --repository {0} -r {1}", sourceDirectory, revision));
                using (var exeProcess = Process.Start(hgUpdate))
                {
                    exeProcess.WaitForExit();
                    if (exeProcess.ExitCode > 0)
                    {
                        throw new Exception("Error during revert source code step");
                    }
                }
            }

            _logger.DebugFormat("Build Databse Initializer");
            var slnFilePath = Path.Combine(sourceDirectory, @"Src\Server\") + "MKBooking.sln";
            var pc = new ProjectCollection();
            var globalProperty = new Dictionary<string, string> { { "Configuration", "Release" } };
            var buildRequestData = new BuildRequestData(slnFilePath, globalProperty, null, new[] { "Build" }, null);
            var buildResult = BuildManager.DefaultBuildManager.Build(new BuildParameters(pc), buildRequestData);

            if (buildResult.Exception != null)
            {
                throw new Exception(buildResult.Exception.Message, buildResult.Exception);
            }

            var targetDir = Path.Combine(sourceDirectory, @"Deployment\Server\Package\DatabaseInitializer");
            var sourcePath = Path.Combine(sourceDirectory, @"Src\Server\DatabaseInitializer\bin\Release");
            CopyFiles(sourcePath, targetDir);

            _logger.DebugFormat("Build Web Site");
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
            _logger.DebugFormat("Deploying");
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
            _logger.DebugFormat("Deploying DB");
            var jsonSettings = new JObject();
            foreach (var setting in job.Company.ConfigurationProperties)
            {
                jsonSettings.Add(setting.Key, JToken.FromObject(setting.Value));
            }

            jsonSettings.Add("IBS.WebServicesUrl", JToken.FromObject(job.IBSServer.Url));
            jsonSettings.Add("IBS.WebServicesUserName", JToken.FromObject(job.IBSServer.Username));
            jsonSettings.Add("IBS.WebServicesPassword", JToken.FromObject(job.IBSServer.Password));

            var fileSettings = Path.Combine(packagesDirectory, "DatabaseInitializer\\Settings\\") + companyName + ".json";
            var stringBuilder = new StringBuilder();
            jsonSettings.WriteTo(new JsonTextWriter(new StringWriter(stringBuilder)));
            File.WriteAllText(fileSettings, stringBuilder.ToString());

            var deployDB = GetProcess(Path.Combine(packagesDirectory, "DatabaseInitializer\\") + "DatabaseInitializer.exe",
                                                   string.Format("{0} {1} {2}", companyName, job.InitDatabase ? "C" : "U",
                                                   job.TaxHailEnv.SqlServerInstance), true);

            using (var exeProcess = Process.Start(deployDB))
            {
                var output = GetOutput(exeProcess);
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during deploy DB step" + output);
                }
            }
        }

        private void DeployServer(DeploymentJob job, string companyName, string packagesDirectory, ServerManager iisManager)
        {
            _logger.DebugFormat("Deploying IIS");

            var revision = GetRevisionNumber(job);

            var subFolder = job.Company.ConfigurationProperties["TaxiHail.Version"] + revision + "." + DateTime.Now.Ticks + "\\";
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
            var section = configuration.GetSection("connectionStrings").GetCollection().First(x => x.Attributes["name"].Value.ToString() == "MKWeb");
            var connSting = section.Attributes["connectionString"];
            connSting.Value = string.Format("Data Source=.;Initial Catalog={0};Integrated Security=True; MultipleActiveResultSets=True", companyName);

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
            var hgRevert = GetProcess("hg.exe", string.Format("update --repository {0} -C -r default", repository));
            using (var exeProcess = Process.Start(hgRevert))
            {
                var output = GetOutput(exeProcess);
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during revert source code step" + output);
                }
            }

            var hgPurge = GetProcess("hg.exe", string.Format("purge --all --repository {0}", repository));
            using (var exeProcess = Process.Start(hgPurge))
            {
                var output = GetOutput(exeProcess);
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during purge source code step" + output);
                }
            }

            var hgPull = GetProcess("hg.exe", string.Format("pull https://buildapcurium:apcurium5200!@bitbucket.org/apcurium/mk-taxi --repository {0}", repository));
            using (var exeProcess = Process.Start(hgPull))
            {
                var output = GetOutput(exeProcess);
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during pull source code step" + output);
                }
            }

            var hgUpdate = GetProcess("hg.exe", string.Format("update --repository {0}", repository));
            using (var exeProcess = Process.Start(hgUpdate))
            {
                var output = GetOutput(exeProcess);
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during update source code step" + output);
                }
            }
        }

        private ProcessStartInfo GetProcess(string filename, string args, bool loadUserProfile = false)
        {
            _logger.DebugFormat("Starting process {0} with args {1}", filename, args);
            return new ProcessStartInfo
            {
                FileName = filename,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                CreateNoWindow = false,
                LoadUserProfile = loadUserProfile,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = args
            };
        }

        private string GetOutput(Process exeProcess)
        {
            var output = "\n---------------------------------------------\n";

            exeProcess.OutputDataReceived += (s, e) =>
            {
                output += e.Data + "\n";
            };
            exeProcess.ErrorDataReceived += (s, e) =>
            {
                output += e.Data + "\n";
            };

            exeProcess.BeginOutputReadLine();
            exeProcess.BeginErrorReadLine();
            exeProcess.WaitForExit();

            return output += "\n---------------------------------------------\n";
        }

        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}