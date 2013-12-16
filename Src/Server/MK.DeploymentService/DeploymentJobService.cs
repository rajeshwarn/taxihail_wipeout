using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using CustomerPortal.Web.Entities;
using DeploymentServiceTools;
using Microsoft.Build.Exceptions;
using Microsoft.Build.Framework;

using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Web.Administration;
using MK.DeploymentService.Service;
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
            _timer.Change(0, 20000);
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

        private DeploymentJob _job;
        //private ConfigurationManagerDbContext dbContext;
        private void CheckAndRunJobWithBuild()
        {
            var job = new DeploymentJobServiceClient().GetNext();

            if (job == null) return;

            _job = job;
            try
            {

                Log("Starting", JobStatus.Inprogress);

                var sourceDirectory = Path.Combine(Path.GetTempPath(), "TaxiHailSource");

                Log("Source Folder = " + sourceDirectory);

                var taxiRepo = new TaxiRepository("hg.exe", sourceDirectory);
                if (_job.Server.Role == EnvironmentRole.BuildServer )
                {
                    taxiRepo.FetchSource(_job.Revision.Commit, str => Log(str));
                    Build(sourceDirectory);
                }

                //build server and deploy
                if (_job.ServerSide || _job.Database)
                {
                    var packagesDirectory = Path.Combine(sourceDirectory, "Deployment\\Server\\Package\\");
                    if (_job.Server.Role != EnvironmentRole.BuildServer)
                    {
                        packagesDirectory = Properties.Settings.Default.DeployFolder;
                    }
                    var packagesLocation = CleanAndUnZip(packagesDirectory);
                    DeployTaxiHail(packagesLocation);
                }

                Log("Job Complete", JobStatus.Success);


            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
                Log(e.Message, JobStatus.Error);
            }
        }

        private string CleanAndUnZip(string packagesDirectory)
        {
            Log("Get the binaries and unzip it");

            
            var packageFile = Path.Combine(Properties.Settings.Default.DropFolder, GetZipFileName(_job));
            if (!File.Exists(  packageFile ) )
            {
                Log("Getting the binaries from server");
                var client = new PackagesServiceClient();
                client.GetPackage(GetZipFileName(_job), packageFile);
                Log("Done getting the binaries from server");
            }
            var unzipDirectory = Path.Combine(packagesDirectory, MakeValidFileName(_job.Revision.Tag));

            if (Directory.Exists(unzipDirectory))
            {
                Directory.Delete(unzipDirectory, true);
            }
            Directory.CreateDirectory(unzipDirectory);

            Log("packageFile : " + packageFile);
            Log("unzipDirectory : " + unzipDirectory);

            if (!Directory.Exists(unzipDirectory))
            {
                Directory.CreateDirectory(unzipDirectory);
            }
            try
            {
                ZipFile.ExtractToDirectory(packageFile, unzipDirectory);
              
            }
            catch (Exception)
            {
                Log("Cannot unzip file, make sure 7zip is installed");
                throw;
            }

            return unzipDirectory;
        }

        private void Log( string details, JobStatus? status = null)
        {

            _logger.Debug(details);

            if (_job != null)
            {
                new DeploymentJobServiceClient().UpdateStatus(_job.Id, details, status);
            }
        }


        private void Build(string sourceDirectory)
        {
            Log("Build Databse Initializer");
            var slnFilePath = Path.Combine(sourceDirectory, @"Src\Server\") + "MKBooking.sln";
            var pc = new ProjectCollection();
            var globalProperty = new Dictionary<string, string> { { "Configuration", "Release" } };
            var buildRequestData = new BuildRequestData(slnFilePath, globalProperty, null, new[] { "Build" }, null);

            var bParam = new BuildParameters(pc);


            Log("Setting logger");

            bParam.Loggers = new ArraySegment<ILogger>(new ILogger[] { new BuildLogger(txt => Log(txt)) });

            var buildResult = BuildManager.DefaultBuildManager.Build(bParam, buildRequestData);



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
            var fileName = GetZipFileName(_job);

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            
            var packageDir = Path.Combine(sourceDirectory, @"Deployment\Server\Package\");

            ZipFile.CreateFromDirectory(packageDir, fileName);
            
            Log("Uploading package to server...");
            new PackagesServiceClient().UploadPackage(fileName);
            Log("Done uploading package to server...");

            
            Log("Finished");
        }

        private void CopyFiles(string source, string target)
        {
            Log("CopyFiles " + source + " => " + target);
            if (Directory.Exists(target))
            {
                Directory.Delete(target, true);
            }
            Directory.CreateDirectory(target);

            foreach (var dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(source, target));

            foreach (var newPath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(source, target), true);
        }

        private void DeployTaxiHail(string packagesDirectory)
        {
            Log(String.Format("Deploying"));
            var companyName = _job.Company.CompanyKey;
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

            if (_job.Database)
            {
                Log("Deploying Database");
                DeployDataBase(packagesDirectory, companyName);
            }


            Log("Deploying Server");
            DeployServer(_job.Company.Id, companyName, packagesDirectory, iisManager);

            appPool.Start();
        }

        private void DeployDataBase(string packagesDirectory, string companyName)
        {
            Log("Deploying DB");
            var jsonSettings = new JObject();
            foreach (var setting in _job.Company.CompanySettings)
            {
                if ((setting.Key != "IBS.WebServicesUrl") &&
                    (setting.Key != "IBS.WebServicesUserName") &&
                    (setting.Key != "IBS.WebServicesPassword"))
                {
                    jsonSettings.Add(setting.Key, JToken.FromObject(setting.Value ?? ""));
                }
            }


            jsonSettings.Add("IBS.WebServicesUrl", JToken.FromObject(_job.Company.IBS.ServiceUrl ?? ""));
            jsonSettings.Add("IBS.WebServicesUserName", JToken.FromObject(_job.Company.IBS.Username ?? ""));
            jsonSettings.Add("IBS.WebServicesPassword", JToken.FromObject(_job.Company.IBS.Password ?? ""));

            var fileSettings = Path.Combine(packagesDirectory, "DatabaseInitializer\\Settings\\") + companyName + ".json";
            var stringBuilder = new StringBuilder();
            jsonSettings.WriteTo(new JsonTextWriter(new StringWriter(stringBuilder)));
            File.WriteAllText(fileSettings, stringBuilder.ToString());

            var deployDB = ProcessEx.GetProcess(Path.Combine(packagesDirectory, "DatabaseInitializer\\") + "DatabaseInitializer.exe",
                                                   string.Format("{0} {1}", companyName, _job.Server.SqlServerInstance), null, true);

            

            using (var exeProcess = Process.Start(deployDB))
            {
                exeProcess.OutputDataReceived += exeProcess_OutputDataReceived;
                var output = ProcessEx.GetOutput(exeProcess);
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during deploy DB step" + output);
                }

                Log("Deploying Database finished");
            }
        }

        void exeProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
            Log(e.Data);
        }

        private void DeployServer(string companyId, string companyName, string packagesDirectory, ServerManager iisManager)
        {
            Log("Deploying IIS");

            var revision = _job.Revision.Commit;

            var subFolder = _job.Revision.Tag + "." + DateTime.Now.Ticks + "\\";
            var targetWeDirectory = Path.Combine(_job.Server.WebSitesFolder, companyName, subFolder);
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
                configuration.GetSection("connectionStrings")
                    .GetCollection()
                    .First(x => x.Attributes["name"].Value.ToString() == "MKWeb");
            var connSting = section.Attributes["connectionString"];
            connSting.Value =
                string.Format(
                    "Data Source=.;Initial Catalog={0};Integrated Security=True; MultipleActiveResultSets=True",
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

            DeployTheme(companyId, companyName, targetWeDirectory);

            iisManager.CommitChanges();

            Log("Deploying IIS Finished");
        }

        private void DeployTheme(string companyId, string companyName, string targetWeDirectory)
        {
            try
            {
                var themeFolder = Path.Combine(targetWeDirectory, @"themes\" + companyName);
                Log("Checking if default theme exist : " + themeFolder);
                if (!Directory.Exists(themeFolder))
                {
                    Log("Copying default theme");
                    var defaultThemeFolder = Path.Combine(targetWeDirectory, @"themes\TaxiHail");
                    DirectoryCopy(defaultThemeFolder, themeFolder, true);

                    Log("Getting web theme from cutsomer portal");
                    var service = new CompanyServiceClient();
                    using (var zip = new ZipArchive(service.GetCompanyFiles(companyId, "webtheme")))
                    {
                        foreach (var  entry in zip.Entries)
                        {
                            if (entry.FullName.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
                            {
                                entry.ExtractToFile(Path.Combine(themeFolder + "\\less", entry.Name));
                            }
                            else
                            {
                                entry.ExtractToFile(Path.Combine(themeFolder + "\\img", entry.Name));
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Log("Warning, cannot copy theme : " + ex.Message );
            }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private static string GetZipFileName(DeploymentJob job)
        {
            return string.Format("TaxiHail_[Bitbucket]{0}.zip", MakeValidFileName(job.Revision.Tag));
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