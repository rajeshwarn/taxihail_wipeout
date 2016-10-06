using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using DeploymentServiceTools;
using log4net;
using System.IO;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using MK.DeploymentService.Service;
using CustomerPortal.Web.Entities;
using System.Reactive.Disposables;

namespace MK.DeploymentService.Mobile
{
	public class DeploymentJobService
	{
        private readonly Timer _timer;
		private readonly ILog _logger;
        private readonly MonoBuilder _builder;
        private readonly CustomerPortalRepository _customerPortalRepository;

        private DeploymentJob _job;
        private bool _isWorking = false;
        private bool _isNewFolder = false;

		public DeploymentJobService ()
		{
			_timer = new Timer (TimerOnElapsed, null, Timeout.Infinite, Timeout.Infinite);
			_logger = LogManager.GetLogger ("DeploymentJobService");
			_builder = new MonoBuilder (str => UpdateJob (str));
			_customerPortalRepository = new CustomerPortalRepository();
		}

		public void Start ()
		{
			_timer.Change (0, 10000);
		}

        public void Stop()
        {
            _timer.Change (Timeout.Infinite, Timeout.Infinite);
        }

		private void TimerOnElapsed (object state)
		{
			if (!_isWorking)
            {
				_isWorking = true;
				CheckAndRunJobWithBuild ();
				_isWorking = false;
			}
		}
        	
		private void CheckAndRunJobWithBuild ()
		{
			try 
            {
				var job = new DeploymentJobServiceClient ().GetNext ();

				if (job == null)
                {
                    return;
                }
			    
				_job = job;

				try 
                {
					_logger.Debug ("Begin work on " + job.Company.CompanyName);

					UpdateJob ("Starting at " + DateTime.Now, JobStatus.Inprogress);

                    var sourceDirectoryConfig = ConfigurationManager.AppSettings["CheckoutDir"];

					var sourceDirectory = string.IsNullOrEmpty(sourceDirectoryConfig)
						? Path.Combine (Path.GetTempPath (), "TaxiHailSourceNewService")
						: sourceDirectoryConfig;

                    if (!Directory.Exists(sourceDirectory))
                    {
                        Directory.CreateDirectory(sourceDirectory);
                        UpdateJob("Directory " + sourceDirectory + " did not exist, creating...");
                        _isNewFolder = true;
                    }

                    var releaseiOSAdHocDir = Path.Combine (sourceDirectory, "Src", "Mobile", "iOS", "bin", "iPhone", "AdHoc");
                    if (Directory.Exists (releaseiOSAdHocDir)) { Directory.Delete (releaseiOSAdHocDir, true); }
                    var releaseiOSAdHocObjDir = Path.Combine (sourceDirectory, "Src", "Mobile", "iOS", "obj", "iPhone", "AdHoc");
                    if (Directory.Exists (releaseiOSAdHocObjDir)) { Directory.Delete (releaseiOSAdHocObjDir, true); }
						
					var releaseiOSAppStoreDir = Path.Combine (sourceDirectory, "Src", "Mobile", "iOS", "bin", "iPhone", "AppStore");
                    if (Directory.Exists (releaseiOSAppStoreDir)) { Directory.Delete (releaseiOSAppStoreDir, true); }
                    var releaseiOSAppStoreObjDir = Path.Combine (sourceDirectory, "Src", "Mobile", "iOS", "obj", "iPhone", "AppStore");
                    if (Directory.Exists (releaseiOSAppStoreObjDir)) { Directory.Delete (releaseiOSAppStoreObjDir, true); }	

					var releaseAndroidDir = Path.Combine (sourceDirectory, "Src", "Mobile", "Android", "bin", "Release");
                    if (Directory.Exists (releaseAndroidDir)) { Directory.Delete (releaseAndroidDir, true); }
						
                    var releaseBlackBerryApkDir = Path.Combine (sourceDirectory, "Src", "Mobile", "TaxiHail.BlackBerry", "bin", "Release");
                    if (Directory.Exists (releaseBlackBerryApkDir)) { Directory.Delete (releaseBlackBerryApkDir, true); }
                        
                    var releaseBlackBerryBarDir = Path.Combine (sourceDirectory, "Src", "BBTools", "Outputs");
                    if (Directory.Exists (releaseBlackBerryBarDir)) { Directory.Delete (releaseBlackBerryBarDir, true); }
                        
					var releaseCallboxAndroidDir = Path.Combine (sourceDirectory, "Src", "Mobile", "MK.Callbox.Mobile.Client.Android", "bin", "Release");
                    if (Directory.Exists (releaseCallboxAndroidDir)) { Directory.Delete (releaseCallboxAndroidDir, true); }
												
					DownloadAndInstallProfileIfNecessary();

                    var isGitHub = bool.Parse(ConfigurationManager.AppSettings["IsGitHubSourceControl"]);
                    var taxiRepo = new TaxiRepository (sourceDirectory,  isGitHub);

					UpdateJob ("FetchSource");
					taxiRepo.FetchSource (_job.Revision.Commit, str => UpdateJob (str));

					UpdateJob ("Customize");
					Customize (sourceDirectory, _job);

					UpdateJob ("Build");
					BuildMobile (sourceDirectory, releaseiOSAdHocDir, releaseiOSAppStoreDir);

					UpdateJob ("Deploy");
                    var deploymentInfo = Deploy (_job.Company, releaseiOSAdHocDir, releaseiOSAppStoreDir, releaseAndroidDir, releaseCallboxAndroidDir, releaseBlackBerryApkDir, releaseBlackBerryBarDir);

					CreateNewVersionInCustomerPortal(deploymentInfo);
                    UpdateJob (string.Format("Done (ended at {0})", DateTime.Now), JobStatus.Success);

					_logger.Debug ("Deployment finished without error");
				}
                catch (Exception e) 
                {
					_logger.Error (e.Message);
                    UpdateJob (string.Format("{0} (ended at {1})", e.Message, DateTime.Now), JobStatus.Error);
				}
			} 
            catch (Exception e) 
            {
				_logger.Error (e.Message);
			}
		}

		private async void DownloadAndInstallProfileIfNecessary()
		{
			if (_job.IosAdhoc || _job.IosAppStore)
			{
				var appId = _job.Company.CompanySettings.Any(s => s.Key == "Package") 
    	            ? _job.Company.CompanySettings.First(s => s.Key == "Package").Value
    	            : null;
                
				if (string.IsNullOrWhiteSpace (_job.Company.AppleAppStoreCredentials.Username)
				    || string.IsNullOrWhiteSpace (_job.Company.AppleAppStoreCredentials.Password)) 
                {
					UpdateJob("Skipping download of provisioning profile, missing Apple Store Credentials");
					return;
				}

				if (appId == null) 
                {
					UpdateJob("Skipping download of provisioning profile, missing App Identifier (CompanySettings[Package])");
					return;
				}

				if (_job.IosAdhoc) 
                {
					UpdateJob ("Downloading/installing Adhoc provisioning profile");
					var message = await _customerPortalRepository.DownloadProfile (
						             _job.Company.AppleAppStoreCredentials.Username, 
						             _job.Company.AppleAppStoreCredentials.Password,
						             _job.Company.AppleAppStoreCredentials.Team,
						             appId,
									true);
					UpdateJob (message);
				}

				if (_job.IosAppStore) 
                {
					UpdateJob ("Downloading/installing AppStore provisioning profile");
					var message = await _customerPortalRepository.DownloadProfile (
						_job.Company.AppleAppStoreCredentials.Username, 
						_job.Company.AppleAppStoreCredentials.Password,
						_job.Company.AppleAppStoreCredentials.Team,
						appId, false
						);
					UpdateJob (message);
				}
			}
		}

        private DeployInfo Deploy (Company company, string ipaAdHocPath, string ipaAppStorePath, string apkPath, string apkPathCallBox, string apkBlackBerryPath, string barPath)
		{
			var result = new DeployInfo ();
		    if (!_job.Android && !_job.CallBox && !_job.IosAdhoc && !_job.IosAppStore && !_job.BlackBerry)
		    {
		        return result;
		    }

		    var targetDirWithoutFileName = Path.Combine (
                ConfigurationManager.AppSettings ["DeployDir"], 
		        company.CompanyKey, 
                string.Format ("{0}.{1}", _job.Revision.Tag, _job.Revision.Commit));

		    if (!Directory.Exists (targetDirWithoutFileName))
		    {
		        Directory.CreateDirectory (targetDirWithoutFileName);
		    }

		    result.RootPath = targetDirWithoutFileName;

		    if (_job.Android)
		    {
		        _logger.DebugFormat (String.Format("Copying Apk, {0}", apkPath));

		        var apkFile = GetAndroidFile(apkPath);
		        if (apkFile != null)
		        {
		            var fileInfo = new FileInfo (apkFile); 
		            var targetDir = Path.Combine (targetDirWithoutFileName, fileInfo.Name);
		            if (File.Exists(targetDir))
		            {
		                File.Delete(targetDir);
		            }
		            File.Copy (apkFile, targetDir);

		            result.AndroidApkFileName = fileInfo.Name;
		        } 
		        else
		        {
		            throw new Exception (String.Format("Can't find the APK file in the release dir: {0}", apkPath));
		        }
		    }

            if (_job.BlackBerry) 
            {
                _logger.DebugFormat (String.Format("Copying BlackBerry Apk, {0} ", apkBlackBerryPath));

                var apkBlackBerryFile = GetBlackBerryApkFile(apkBlackBerryPath);
                if (apkBlackBerryFile != null) 
                {
                    var fileInfo = new FileInfo (apkBlackBerryFile); 
                    var newName = fileInfo.Name.Replace (".apk", "_blackberry.apk");
                    var targetDir = Path.Combine (targetDirWithoutFileName, newName);
                    if (File.Exists(targetDir))
                    {
                        File.Delete (targetDir);
                    }
                    File.Copy (apkBlackBerryFile, targetDir);
                    result.BlackBerryApkFileName = newName;
                } 
                else 
                {
                    throw new Exception (String.Format("Can't find the APK BlackBerry file in the release dir: {0}", apkBlackBerryPath));
                }

                var barFile = GetBlackBerryBarFile(barPath);
                if (barFile != null) 
                {
                    var fileInfo = new FileInfo (barFile); 
                    var targetDir = Path.Combine (targetDirWithoutFileName, fileInfo.Name);
                    if (File.Exists(targetDir))
                    {
                        File.Delete (targetDir);
                    }
                    File.Copy (barFile, targetDir);
                    result.BlackBerryBarFileName = fileInfo.Name;
                } 
                else 
                {
                    throw new Exception (String.Format("Can't find the BAR BlackBerry file in the release dir: {0}", barPath));
                }
            }

		    if (_job.CallBox)
		    {
		        _logger.DebugFormat (String.Format("Copying CallBox Apk, {0}", apkPathCallBox));

		        var apkFile = GetAndroidFile(apkPathCallBox);
		        if (apkFile != null)
		        {
                    var fileInfo = new FileInfo (apkFile); 
		            var targetDir = Path.Combine (targetDirWithoutFileName, fileInfo.Name);
		            if (File.Exists(targetDir))
		            {
		                File.Delete(targetDir);
		            }
		            File.Copy (apkFile, targetDir);
		            result.CallboxApkFileName = fileInfo.Name;
		        } 
		        else
		        {
		            throw new Exception (String.Format("Can't find the CallBox APK file in the release dir: {0}", apkPathCallBox));
		        }
		    }

		    if (_job.IosAdhoc)
		    {
		        _logger.DebugFormat (String.Format("Uploading and copying IPA AdHoc, {0}", ipaAdHocPath));

		        var ipaFile = GetiOSFile(ipaAdHocPath);
		        if (ipaFile != null)
		        {
					UpdateJob("Found AdHoc IPA");
		            var fileInfo = new FileInfo (ipaFile); 
		            var targetDir = Path.Combine (targetDirWithoutFileName, fileInfo.Name);
                    if (File.Exists(targetDir))
                    {
                        File.Delete (targetDir);
                    }
		            File.Copy (ipaFile, targetDir);
		            result.iOSAdhocFileName = fileInfo.Name;
		        } 
		        else
		        {
		            throw new Exception (String.Format("Can't find the IPA file in the AdHoc dir: {0}", ipaAdHocPath));
		        }
		    }

		    if (_job.IosAppStore)
		    {
		        _logger.DebugFormat (String.Format("Uploading and copying IPA AppStore, {0}", ipaAppStorePath));

		        var ipaFile = GetiOSFile(ipaAppStorePath);
		        if (ipaFile != null)
		        {
					UpdateJob("Found AppStore IPA");
					var fileInfo = new FileInfo (ipaFile); 
		            var newName = fileInfo.Name.Replace (".ipa", ".appstore.ipa");
		            var targetDir = Path.Combine (targetDirWithoutFileName, newName);
                    if (File.Exists(targetDir))
                    {
                        File.Delete(targetDir);
                    }
		            File.Copy (ipaFile, targetDir);
		            result.iOSAppStoreFileName = newName;
		        } 
		        else
		        {
		            throw new Exception (String.Format("Can't find the IPA file in the AppStore dir: {0}", ipaAppStorePath));
		        }
		    }

		    return result;
		}

		private void Customize (string sourceDirectory, DeploymentJob job)
		{
			var company = job.Company;
			UpdateJob("Service Url : " + job.ServerUrl);
			UpdateJob("Customize - Build Config Tool Customization");
			 
			var sln = string.Format ("{0}/ConfigTool.iOS.sln", Path.Combine (sourceDirectory, "Src", "ConfigTool"));
			var projectName = "NinePatchMaker.Lib";
			if (_builder.ProjectIsInSolution (sln, projectName))
            {
				var ninePatchProjectConfi = String.Format ("\"--project:{0}\" \"--configuration:{1}\"", projectName, "Release");
                _builder.BuildProjectUsingMdTool (string.Format ("build " + ninePatchProjectConfi + "  \"{0}\"", sln));
			}
            else
            {
				UpdateJob("Skipping NinePatch.Lib because it does not exist on this version");
			}

			projectName = "NinePatchMaker";
			if (_builder.ProjectIsInSolution (sln, projectName))
            {
				var ninePatchProjectConfi = String.Format ("\"--project:{0}\" \"--configuration:{1}\"", projectName, "Release");
                _builder.BuildProjectUsingMdTool (string.Format ("build " + ninePatchProjectConfi + "  \"{0}\"", sln));
			}
            else
            {
				UpdateJob ("Skipping NinePatch because it does not exist on this version");
			}
			
			var mainConfig = String.Format ("\"--project:{0}\" \"--configuration:{1}\"", "apcurium.MK.Booking.ConfigTool", "Release");
            _builder.BuildProjectUsingMdTool (string.Format ("build " + mainConfig + "  \"{0}/ConfigTool.iOS.sln\"", Path.Combine (sourceDirectory, "Src", "ConfigTool")));

			UpdateJob ("Run Config Tool Customization");

			var workingDirectory = Path.Combine (sourceDirectory, "Src", "ConfigTool", "apcurium.MK.Booking.ConfigTool.Console", "bin", "Release");
			var configToolRun = ProcessEx.GetProcess ("/usr/local/bin/mono", string.Format ("apcurium.MK.Booking.ConfigTool.exe {0} {1}", company.CompanyKey, job.ServerUrl), workingDirectory);

            using (var exeProcess = Process.Start(configToolRun))
            {
                var output = ProcessEx.GetOutput(exeProcess);
                if (exeProcess != null && exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during customization, " + output);
                }
                UpdateJob("Customize Successful");
            }

            SleepFor(5);

			UpdateJob("Customization Finished");
			UpdateJob("Run Localization tool for Android");

			var localizationToolRun = new ProcessStartInfo {
				FileName = "/usr/local/bin/mono",
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				WorkingDirectory = Path.Combine (sourceDirectory, "Src", "LocalizationTool"),
				Arguments = "output/LocalizationTool.exe -t=android -m=\"../Mobile/Common/Localization/Master.resx\" -d=\"../Mobile/Android/Resources/Values/String.xml\" -s=\"../Mobile/Common/Settings/Settings.json\""
			};

            using (var exeProcess = Process.Start(localizationToolRun))
            {
                var outputAndroid = ProcessEx.GetOutput(exeProcess);
                if (exeProcess != null && exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during localization tool for android");
                }
                UpdateJob(outputAndroid);
            }

            SleepFor(5);

			UpdateJob("Run Localization tool for Android Finished");
			UpdateJob("Run Localization tool for iOS");

            localizationToolRun = new ProcessStartInfo
            {
                FileName = "/usr/local/bin/mono",
				RedirectStandardOutput = true,
				RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = Path.Combine(sourceDirectory, "Src", "LocalizationTool"),
                Arguments = "output/LocalizationTool.exe -t=ios -m=\"../Mobile/Common/Localization/Master.resx\" -d=\"../Mobile/iOS/en.lproj/Localizable.strings\" -s=\"../Mobile/Common/Settings/Settings.json\""
            };
            
            using (var exeProcess = Process.Start(localizationToolRun))
            {
				var outputiOS = ProcessEx.GetOutput (exeProcess);
                if (exeProcess != null && exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during localization tool for iOS");
                }
				UpdateJob (outputiOS);
            }

            SleepFor(5);
            UpdateJob("Run Localization tool for iOS Finished");

		    if (!job.CallBox)
		    {
		        return;
		    }

            UpdateJob("Run Localization tool for Callbox");

            localizationToolRun = new ProcessStartInfo
            {
                FileName = "/usr/local/bin/mono",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = Path.Combine(sourceDirectory, "Src", "LocalizationTool"),
                Arguments = "output/LocalizationTool.exe -t=callbox -m=\"../Mobile/Common/Localization/Master.resx\" -d=\"../Mobile/MK.Callbox.Mobile.Client.Android/Resources/Values/Strings.xml\" -s=\"../Mobile/Common/Settings/Settings.json\""
            };

            using (var exeProcess = Process.Start(localizationToolRun))
            {
                var outputCallbox = ProcessEx.GetOutput(exeProcess);
                if (exeProcess != null && exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during localization tool for callbox");
                }
                UpdateJob(outputCallbox);
            }

            SleepFor(5);
            UpdateJob("Run Localization tool for Callbox Finished");
        }

        private IDisposable StartStopwatch (string message)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            UpdateJob(message);

            return Disposable.Create (() => 
            {
                stopwatch.Stop();
                UpdateJob(message + string.Format(" done ({0}min {1}sec)", stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds));
            });
        }

		private void BuildMobile (string sourceDirectory, string releaseiOSAdHocDir, string releaseiOSAppStoreDir)
		{			
			var sourceMobileFolder = Path.Combine (sourceDirectory, "Src", "Mobile");

            RestoreNuGetPackages(sourceMobileFolder);
            SleepFor(5);

			if (_job.IosAdhoc)
            {		       
                using(StartStopwatch("Build iOS AdHoc"))
                {
                    //_builder.BuildProjectUsingMdTool(string.Format("build \"--configuration:{0}\" \"{1}/TaxiHail.sln\"", "AdHoc|iPhone", sourceMobileFolder));
					_builder.BuildProjectUsingXBuild(string.Format("/p:Configuration={0} /p:Platform=iPhone /p:BuildIpa=true /p:IpaPackageDir=\"{2}\" /target:Build \"{1}/TaxiHail.sln\"", "AdHoc", sourceMobileFolder, releaseiOSAdHocDir));
				}
                SleepFor(10);
			}

			if (_job.IosAppStore)
            {	
                using (StartStopwatch("Build iOS AppStore"))
                {
					//_builder.BuildProjectUsingMdTool(string.Format("build \"--configuration:{0}\" \"{1}/TaxiHail.sln\"", "AppStore|iPhone", sourceMobileFolder));
					_builder.BuildProjectUsingXBuild(string.Format("/p:Configuration={0} /p:Platform=iPhone /p:BuildIpa=true /p:IpaPackageDir=\"{2}\" /target:Build \"{1}/TaxiHail.sln\"", "AppStore", sourceMobileFolder, releaseiOSAppStoreDir));
                }
                SleepFor(10);
			}

            if (!_job.Android && !_job.CallBox && !_job.BlackBerry)
            {
                return;
            }
				
            if (_job.Android)
            {
                using (StartStopwatch("Building Android"))
                {
                    _builder.BuildProjectUsingXBuild(string.Format("/t:SignAndroidPackage /p:Configuration={0} {1}/Android/TaxiHail.csproj", "Release", sourceMobileFolder));
                }
                SleepFor(10);
            }

            if (_job.BlackBerry)
            {
                using (StartStopwatch("Building BlackBerry"))
                {
                    _builder.BuildProjectUsingXBuild(string.Format("/t:SignAndroidPackage /p:Configuration={0} {1}/TaxiHail.BlackBerry/TaxiHail.BlackBerry.csproj", "Release", sourceMobileFolder));
                }

                SleepFor(10);
                UpdateJob("Copying BlackBerry Apk For Packaging .Bar");
                var releaseBlackBerryDir = Path.Combine (sourceMobileFolder, "TaxiHail.BlackBerry", "bin", "Release");
                var bbToolsPath = Path.Combine (sourceDirectory, "Src", "BBTools");
                var apkBlackBerryFile = GetBlackBerryApkFile(releaseBlackBerryDir);

                if (apkBlackBerryFile != null) 
                {
                    var fileInfo = new FileInfo (apkBlackBerryFile); 
                    var targetDir = Path.Combine (bbToolsPath, "Outputs", fileInfo.Name);
                    if (File.Exists(targetDir))
                    {
                        File.Delete (targetDir);
                    }
                    File.Copy (apkBlackBerryFile, targetDir);
                    SleepFor(5);
                    var barFile = fileInfo.Name.Replace(".apk",".bar");
                    _builder.SignAndGenerateBlackBerryProject(bbToolsPath, barFile);
                } 
                else 
                {
                    throw new Exception ("Can't find the APK BlackBerry file in the release dir");
                }
            }

            if (_job.CallBox)
            {
                using (StartStopwatch("Building Callbox"))
                {
                    _builder.BuildProjectUsingXBuild(string.Format("/t:SignAndroidPackage /p:Configuration={0} {1}/MK.Callbox.Mobile.Client.Android/MK.Callbox.Mobile.Client.Android.csproj", "Release", sourceMobileFolder));
                }
                SleepFor(10);
            }
		}

		private static string GetSettingsFilePath (string sourceDirectory, string companyName)
		{
			return Path.Combine (sourceDirectory, "Config", companyName, "Settings.json");
		}

		private static void CopySettingsFileToOutputDir (string jsonSettingsFile, string targetFile)
		{
			var sb = new StringBuilder ();
			var reader = new JsonTextReader (new StreamReader (jsonSettingsFile));
			while (reader.Read ())
            {
                if (reader.Value == null)
                {
                    continue;
                }

				if (reader.TokenType == JsonToken.PropertyName) 
                {
					sb.Append (string.Format ("{0}: ", reader.Value));
				} 
                else 
                {
					sb.AppendLine (reader.Value.ToString ());
				}
			}
            using (var outfile = new StreamWriter(targetFile, false))
            {
                outfile.Write(sb.ToString());
            }
		}

        private void RestoreNuGetPackages(string sourceMobileFolder)
        {
            UpdateJob ("Restore NuGet Packages");

            var restoreProcess = ProcessEx.GetProcess ("nuget", string.Format ("restore \"{0}/TaxiHail.sln\"", sourceMobileFolder), sourceMobileFolder);

            using (var exeProcess = Process.Start(restoreProcess))
            {
                var output = ProcessEx.GetOutput(exeProcess);
                if (exeProcess.ExitCode > 0)
                {
                    UpdateJob("Error during restore NuGet Packages");
                }
                UpdateJob("Restore NuGet Packages Successful");
            }
        }

        private void UpdateJob (string details = null, JobStatus? jobStatus = null)
        {
            _logger.Debug (details);

            new DeploymentJobServiceClient ().UpdateStatus (_job.Id, details, jobStatus);
        }

        private string GetAndroidFile(string apkPath)
        {
            return Directory.Exists (apkPath) 
                ? Directory.EnumerateFiles (apkPath, "*-Signed.apk", SearchOption.TopDirectoryOnly).FirstOrDefault ()
                : null;
        }

        private string GetBlackBerryApkFile(string apkPath)
        {
            return Directory.Exists (apkPath)
                ? Directory.EnumerateFiles (apkPath, "*-Signed.apk", SearchOption.TopDirectoryOnly).FirstOrDefault ()
                : null;
        }

        private string GetBlackBerryBarFile(string barPath)
        {
            return Directory.Exists(barPath)
                ? Directory.EnumerateFiles(barPath, "*.bar", SearchOption.TopDirectoryOnly).FirstOrDefault()
                : null;
        }

        private string GetiOSFile(string ipaPath)
        {
            return Directory.Exists (ipaPath) 
                ? Directory.EnumerateFiles(ipaPath, "*.ipa", SearchOption.TopDirectoryOnly).FirstOrDefault() 
                : null;
        }

        private void CreateNewVersionInCustomerPortal(DeployInfo deployment)
        {
            UpdateJob("Creating new version in Customer Portal");
            var message = _customerPortalRepository.CreateNewVersion(_job.Company.CompanyKey, _job.Revision.Tag, _job.ServerUrl, deployment);
            UpdateJob (message);
        }

        private void DeleteTempAppStoreFile (string ipaPath)
        {
            if (Directory.Exists (ipaPath)) 
            {
                var file =  Directory.EnumerateFiles(ipaPath, "*appstore.ipa", SearchOption.TopDirectoryOnly).FirstOrDefault();
                File.Delete (file);
            }
        }

        private void SleepFor(int seconds)
        {
            Thread.Sleep(seconds * 10);
        }
	}
}