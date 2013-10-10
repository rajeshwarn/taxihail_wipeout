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
using DeploymentServiceTools;
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

        private DeploymentJob job;
        private ConfigurationManagerDbContext dbContext;
        private void CheckAndRunJobWithBuild()
        {
            dbContext = new ConfigurationManagerDbContext(System.Configuration.ConfigurationManager.ConnectionStrings["MKConfig"].ConnectionString);
            job = dbContext.Set<DeploymentJob>()
                .Include(x => x.Company)
                .Include(x => x.IBSServer)
                .Include(x => x.TaxHailEnv)
                .Include(x => x.Version)
                .FirstOrDefault(x => x.Status == JobStatus.REQUESTED && !x.iOS_AdHoc && !x.iOS_AppStore && !x.Android);

            if (job == null) return;

            try
            {
                Log("Starting",JobStatus.INPROGRESS);

                var sourceDirectory = Path.Combine(Path.GetTempPath(), "TaxiHailSource");

                Log("Source Folder = " + sourceDirectory);
                
                var taxiRepo = new TaxiRepository("hg.exe", sourceDirectory);
                if (Properties.Settings.Default.Mode == "Build")
                {
                    taxiRepo.FetchSource(job.GetRevisionNumber(),str=>Log(str));
                    Build(sourceDirectory);
                }

                //build server and deploy
                if (job.DeployServer || job.DeployDB)
                {
                    var packagesDirectory = Path.Combine(sourceDirectory, "Deployment\\Server\\Package\\");
                    if (Properties.Settings.Default.Mode != "Build")
                    {
                        packagesDirectory = Properties.Settings.Default.DeployFolder;
                    }
                    var packagesLocation = CleanAndUnZip(packagesDirectory);
                    DeployTaxiHail(packagesLocation);
                }

                Log("Job Complete", JobStatus.SUCCESS);
                

            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
                Log(e.Message, JobStatus.ERROR);
            }
        }

        private string CleanAndUnZip(string packagesDirectory)
        {
            Log("Get the binaries and unzip it");
            var packageFile = Path.Combine(Properties.Settings.Default.DropFolder, GetZipFileName(job));
            var unzipDirectory = Path.Combine(packagesDirectory, MakeValidFileName(job.GetVersionNumber()));

            if (Directory.Exists(unzipDirectory))
            {
                Directory.Delete(unzipDirectory, true);
            }
            Directory.CreateDirectory(unzipDirectory);

            var zipProcess = ProcessEx.GetProcess(@"C:\Program Files\7-Zip\7z", string.Format("x {0} *", packageFile), unzipDirectory);
            using (var exeProcess = Process.Start(zipProcess))
            {
                var output = ProcessEx.GetOutput(exeProcess);
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during unziping Process" + output);
                }
            }
            return unzipDirectory;
        }

        private void Log(string details, JobStatus? status=null)
        {

            _logger.Debug(details);
            if (status.HasValue)
            {
                job.Status = status.Value;
            }
            job.Details += details + "\n";

            dbContext.SaveChanges();
        }

        private void Build(string sourceDirectory)
        {
            Log("Build Databse Initializer");
            var slnFilePath = Path.Combine(sourceDirectory, @"Src\Server\") + "MKBooking.sln";
            var pc = new ProjectCollection();
            var globalProperty = new Dictionary<string, string> { { "Configuration", "Release" } };
            var buildRequestData = new BuildRequestData(slnFilePath, globalProperty, null, new[] { "Build" }, null);
            var buildResult = BuildManager.DefaultBuildManager.Build(new BuildParameters(pc), buildRequestData);

            Log("Build Finished");

            if (buildResult.Exception != null)
            {
                throw new Exception(buildResult.Exception.Message, buildResult.Exception);
            }

            var targetDir = Path.Combine(sourceDirectory, @"Deployment\Server\Package\DatabaseInitializer");
            var sourcePath = Path.Combine(sourceDirectory, @"Src\Server\DatabaseInitializer\bin\Release");
            CopyFiles(sourcePath, targetDir);

            Log(String.Format("Build Web Site"));
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

            Log("Zip Web directory and move it to the drop folder");
            //compress data
            var fileName = GetZipFileName(job);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            var packageDir = Path.Combine(sourceDirectory, @"Deployment\Server\Package\");
            var zipProcess = ProcessEx.GetProcess(@"C:\Program Files\7-Zip\7z", string.Format("a -tzip {0} *", fileName), packageDir);
            using (var exeProcess = Process.Start(zipProcess))
            {
                var output = ProcessEx.GetOutput(exeProcess);
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during Zip Process" + output);
                }
            }

            File.Copy(Path.Combine(packageDir, fileName), Path.Combine(Properties.Settings.Default.DropFolder, fileName));
            Log("Finished");
        }

        private void CopyFiles(string source, string target)
        {
            Log("CopyFiles "+source+" => "+target);
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

        private void DeployTaxiHail(string packagesDirectory)
        {
            Log(String.Format("Deploying"));
            var companyName = job.Company.ConfigurationProperties["TaxiHail.ServerCompanyName"];
            var iisManager = new ServerManager();
            var appPool = iisManager.ApplicationPools.FirstOrDefault(x => x.Name == companyName);
            if (appPool == null)
            {
                Log("Creating a new app pool");
                //create a new one
                appPool = iisManager.ApplicationPools.Add(companyName);
                appPool.ManagedRuntimeVersion = "v4.0";
                iisManager.CommitChanges();
                Thread.Sleep(2000);
            }
            if (appPool.State == ObjectState.Started) appPool.Stop();

            if (job.DeployDB)
            {
                Log("Deploying Database");
                DeployDataBase(packagesDirectory, companyName);
            }

            if (job.DeployServer)
            {
                Log("Deploying Server");
                DeployServer(companyName, packagesDirectory, iisManager);
            }
            appPool.Start();
        }

        private void DeployDataBase( string packagesDirectory, string companyName)
        {
            Log("Deploying DB");
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

            var deployDB = ProcessEx.GetProcess(Path.Combine(packagesDirectory, "DatabaseInitializer\\") + "DatabaseInitializer.exe",
                                                   string.Format("{0} {1} {2}", companyName, job.InitDatabase ? "C" : "U",
                                                   job.TaxHailEnv.SqlServerInstance), null, true);

            using (var exeProcess = Process.Start(deployDB))
            {
                var output = ProcessEx.GetOutput(exeProcess);
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during deploy DB step" + output);
                }

                Log("Deploying Database finished");
            }
        }

        private void DeployServer(string companyName, string packagesDirectory, ServerManager iisManager)
        {
            Log("Deploying IIS");

            var revision = job.GetRevisionNumber();

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

            Log("Deploying IIS Finished");
        }

        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private static string GetZipFileName(DeploymentJob job)
        {
            return string.Format("TaxiHail_{0}.zip", MakeValidFileName(job.GetVersionNumber()));
        }

        private static string MakeValidFileName(string name)
        {
            name = name.Replace(" ", string.Empty);
            var invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidReStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            var safeName = System.Text.RegularExpressions.Regex.Replace(name, invalidReStr, "_");
            return safeName;
        }
    }
}