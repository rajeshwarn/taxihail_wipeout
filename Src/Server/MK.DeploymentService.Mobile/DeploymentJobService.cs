using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using DeploymentServiceTools;
using log4net;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MK.DeploymentService.Service;
using CustomerPortal.Web.Entities;
using System.Threading.Tasks;

namespace MK.DeploymentService.Mobile
{
	public class DeploymentJobService
	{

		private Timer _timer;
		private readonly ILog _logger;
		DeploymentJob _job;
		private MonoBuilder _builder;
		private CustomerPortalRepository _customerPortalRepository;
		const string HG_PATH = "/usr/local/bin/hg";
		public bool _isWorking = false;

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

		private void TimerOnElapsed (object state)
		{
			if (!_isWorking){
				_isWorking = true;
				CheckAndRunJobWithBuild ();
				_isWorking = false;
			}
		}

		void UpdateJob (string details = null, JobStatus? jobStatus = null)
		{
			_logger.Debug (details);

			new DeploymentJobServiceClient ().UpdateStatus (_job.Id, details, jobStatus);

		}

		private void CheckAndRunJobWithBuild ()
		{
			try {
				var job = new DeploymentJobServiceClient ().GetNext ();

				if (job == null)
					return;

				_job = job;

				try {

					_logger.Debug ("Begin work on " + job.Company.CompanyName);

					UpdateJob ("Starting", JobStatus.Inprogress);

					var sourceDirectoryConfig = System.Configuration.ConfigurationManager.AppSettings["CheckoutDir"];

					var sourceDirectory = string.IsNullOrEmpty(sourceDirectoryConfig)
						? Path.Combine (Path.GetTempPath (), "TaxiHailSourceNewService")
						: sourceDirectoryConfig;

					var releaseiOSAdHocDir = Path.Combine (sourceDirectory, "Src", "Mobile", "iOS", "bin", "iPhone", "AdHoc");
					if (Directory.Exists (releaseiOSAdHocDir))
						Directory.Delete (releaseiOSAdHocDir, true);

					var releaseiOSAppStoreDir = Path.Combine (sourceDirectory, "Src", "Mobile", "iOS", "bin", "iPhone", "AppStore");
					if (Directory.Exists (releaseiOSAppStoreDir))
						Directory.Delete (releaseiOSAppStoreDir, true);

					var releaseAndroidDir = Path.Combine (sourceDirectory, "Src", "Mobile", "Android", "bin", "Release");
					if (Directory.Exists (releaseAndroidDir))
						Directory.Delete (releaseAndroidDir, true);

					var releaseCallboxAndroidDir = Path.Combine (sourceDirectory, "Src", "Mobile", "MK.Callbox.Mobile.Client.Android", "bin", "Release");
					if (Directory.Exists (releaseCallboxAndroidDir))
						Directory.Delete (releaseCallboxAndroidDir, true);

					if (!Directory.Exists (sourceDirectory))
						Directory.CreateDirectory( sourceDirectory);

					DownloadAndInstallProfileIfNecessary();

					var taxiRepo = new TaxiRepository (HG_PATH, sourceDirectory);
					UpdateJob ("FetchSource");
					taxiRepo.FetchSource (_job.Revision.Commit, str => UpdateJob (str));

					UpdateJob ("Customize");
					Customize (sourceDirectory, _job);

					UpdateJob ("Build");
					BuildMobile (sourceDirectory);

					UpdateJob ("Deploy");
					var deploymentInfo = Deploy (sourceDirectory, _job.Company, releaseiOSAdHocDir, releaseiOSAppStoreDir, releaseAndroidDir, releaseCallboxAndroidDir);

					CreateNewVersionInCustomerPortal(deploymentInfo);
					UpdateJob ("Done", JobStatus.Success);


					_logger.Debug ("Deployment finished without error");

				} catch (Exception e) {
					_logger.Error (e.Message);
					UpdateJob (e.Message, JobStatus.Error);
				}
			} catch (Exception e) {
				_logger.Error (e.Message);
			}
		}


		private void CreateNewVersionInCustomerPortal(DeployInfo deployment)
		{
			UpdateJob("Creating new version in Customer Portal");


		    var message = _customerPortalRepository.CreateNewVersion(_job.Company.CompanyKey, 
                                                                        _job.Revision.Tag, 
																		_job.ServerUrl, 
																		deployment);
			UpdateJob (message);
		}

		void DeleteTempAppStoreFile (string ipaPath)
		{
			if (Directory.Exists (ipaPath)) {
				var file =  Directory.EnumerateFiles(ipaPath, "*appstore.ipa", SearchOption.TopDirectoryOnly).FirstOrDefault();
					File.Delete (file);

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
				   || string.IsNullOrWhiteSpace (_job.Company.AppleAppStoreCredentials.Password)) {
					UpdateJob("Skipping download of provisioning profile, missing Apple Store Credentials");
					return;
				}

				if (appId == null) {
					UpdateJob("Skipping download of provisioning profile, missing App Identifier (CompanySettings[Package])");
					return;
				}

				if (_job.IosAdhoc) {
					UpdateJob ("Downloading/installing Adhoc provisioning profile");
					var message = await _customerPortalRepository.DownloadProfile (
						             _job.Company.AppleAppStoreCredentials.Username, 
						             _job.Company.AppleAppStoreCredentials.Password,
						             _job.Company.AppleAppStoreCredentials.Team,
						             appId,
									true);
					UpdateJob (message);
				}

				if (_job.IosAppStore) {
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

		public void Stop()
		{
			_timer.Change (Timeout.Infinite, Timeout.Infinite);
		}

		private string GetAndroidFile(string apkPath)
		{
			if (Directory.Exists (apkPath)) {
				return Directory.EnumerateFiles (apkPath, "*-Signed.apk", SearchOption.TopDirectoryOnly).FirstOrDefault ();
			}
			return null;
		}

		private string GetAndroidCallboxFile(string apkPathCallBox)
		{
			if (Directory.Exists (apkPathCallBox)) {
				return Directory.EnumerateFiles(apkPathCallBox, "*-Signed.apk", SearchOption.TopDirectoryOnly).FirstOrDefault();
			}
			return null;
		}

		private string GetiOSFile(string ipaPath)
		{
					if (Directory.Exists (ipaPath)) {
						return Directory.EnumerateFiles(ipaPath, "*.ipa", SearchOption.TopDirectoryOnly).FirstOrDefault();
			}
			return null;
		}

		

		DeployInfo Deploy (string sourceDirectory, Company company, string ipaAdHocPath, string ipaAppStorePath, string apkPath, string apkPathCallBox)
		{

			var result = new DeployInfo ();
			if (_job.Android || _job.CallBox || _job.IosAdhoc || _job.IosAppStore) {
				string targetDirWithoutFileName = Path.Combine (System.Configuration.ConfigurationManager.AppSettings ["DeployDir"], 
					company.CompanyKey,
					string.Format ("{0}.{1}", _job.Revision.Tag,_job.Revision.Commit));
				if (!Directory.Exists (targetDirWithoutFileName)) {
					Directory.CreateDirectory (targetDirWithoutFileName);
				}

				result.RootPath = targetDirWithoutFileName;

				if (_job.Android) {
					_logger.DebugFormat ("Copying Apk");
					var apkFile = GetAndroidFile(apkPath);

					if (apkFile != null) {
						var fileInfo = new FileInfo (apkFile); 
						var targetDir = Path.Combine (targetDirWithoutFileName, fileInfo.Name);
						if (File.Exists (targetDir))
							File.Delete (targetDir);
						File.Copy (apkFile, targetDir);

						result.AndroidApkFileName = fileInfo.Name;

					} else {
						throw new Exception ("Can't find the APK file in the release dir");
					}
				}

				if (_job.CallBox) {
					_logger.DebugFormat ("Copying CallBox Apk");
					var apkFile = GetAndroidCallboxFile(apkPathCallBox);
					if (apkFile != null) {
						var fileInfo = new FileInfo (apkFile); 
						var targetDir = Path.Combine (targetDirWithoutFileName, fileInfo.Name);
						if (File.Exists (targetDir))
							File.Delete (targetDir);
						File.Copy (apkFile, targetDir);
						result.CallboxApkFileName = fileInfo.Name;
					} else {
						throw new Exception ("Can't find the CallBox APK file in the release dir");
					}
				}

				if (_job.IosAdhoc) {
					_logger.DebugFormat ("Uploading and copying IPA AdHoc");
					var ipaFile = GetiOSFile(ipaAdHocPath);
					if (ipaFile != null) {

						var fileInfo = new FileInfo (ipaFile); 
						var targetDir = Path.Combine (targetDirWithoutFileName, fileInfo.Name);
						if (File.Exists (targetDir))
							File.Delete (targetDir);
						File.Copy (ipaFile, targetDir);
						result.iOSAdhocFileName = fileInfo.Name;

					} else {
						throw new Exception ("Can't find the IPA file in the AdHoc dir");
					}
				}

				if (_job.IosAppStore) {
					_logger.DebugFormat ("Uploading and copying IPA AppStore");
					var ipaFile = GetiOSFile(ipaAppStorePath);
					if (ipaFile != null) {

						var fileInfo = new FileInfo (ipaFile); 
						var newName = fileInfo.Name.Replace (".ipa", ".appstore.ipa");
						var targetDir = Path.Combine (targetDirWithoutFileName, newName);
						if (File.Exists (targetDir))
							File.Delete (targetDir);
						File.Copy (ipaFile, targetDir);
						result.iOSAppStoreFileName = newName;
					} else {
						throw new Exception ("Can't find the IPA file in the AppStore dir");
					}
				}
			}

			return result;
		}

		void Customize (string sourceDirectory, DeploymentJob job)
		{
			Company company = job.Company;
			CustomerPortal.Web.Entities.Environment taxiHailEnv = job.Server;
			UpdateJob ("Service Url : " + job.ServerUrl);

			_logger.DebugFormat ("Build Config Tool Customization");
			UpdateJob("Customize - Build Config Tool Customization");
			 
			var sln = string.Format ("{0}/ConfigTool.iOS.sln", Path.Combine (sourceDirectory, "Src", "ConfigTool"));
			var projectName = "NinePatchMaker.Lib";
			if (_builder.ProjectIsInSolution (sln, projectName)) {
				var ninePatchProjectConfi = String.Format ("\"--project:{0}\" \"--configuration:{1}\"", projectName, "Release");
				_builder.BuildProject (string.Format ("build " + ninePatchProjectConfi + "  \"{0}\"", sln));
			} else {
				UpdateJob("Skipping NinePatch.Lib because it does not exist on this version");
			}

			projectName = "NinePatchMaker";
			if (_builder.ProjectIsInSolution (sln, projectName)) {
				var ninePatchProjectConfi = String.Format ("\"--project:{0}\" \"--configuration:{1}\"", projectName, "Release");
				_builder.BuildProject (string.Format ("build " + ninePatchProjectConfi + "  \"{0}\"", sln));
			} else {
				UpdateJob ("Skipping NinePatch because it does not exist on this version");
			}
			

			var mainConfig = String.Format ("\"--project:{0}\" \"--configuration:{1}\"", "apcurium.MK.Booking.ConfigTool", "Release");
			_builder.BuildProject (string.Format ("build " + mainConfig + "  \"{0}/ConfigTool.iOS.sln\"", Path.Combine (sourceDirectory, "Src", "ConfigTool")));

			UpdateJob ("Run Config Tool Customization");

			var workingDirectory = Path.Combine (sourceDirectory, "Src", "ConfigTool", "apcurium.MK.Booking.ConfigTool.Console", "bin", "Release");
			var configToolRun = ProcessEx.GetProcess ("mono", string.Format ("apcurium.MK.Booking.ConfigTool.exe {0} {1}", company.CompanyKey, job.ServerUrl), workingDirectory);

			using (var exeProcess = Process.Start (configToolRun)) {
				var output = ProcessEx.GetOutput (exeProcess);
				if (exeProcess.ExitCode > 0) {
					throw new Exception ("Error during customization, " + output);
				}
				UpdateJob ("Customize Successful");
			}

			UpdateJob("Customization Finished");
			UpdateJob ("Run Localization tool for Android");

			var localizationToolRun = new ProcessStartInfo {
				FileName = "mono",
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				WorkingDirectory = Path.Combine (sourceDirectory, "Src", "LocalizationTool"),
				Arguments = "output/LocalizationTool.exe -t=android -m=\"../Mobile/Common/Localization/Master.resx\" -d=\"../Mobile/Android/Resources/Values/String.xml\" -s=\"../Mobile/Common/Settings/Settings.json\""
			};

			using (var exeProcess = Process.Start (localizationToolRun)) {
				var outputAndroid = ProcessEx.GetOutput (exeProcess);
				if (exeProcess.ExitCode > 0) {
					throw new Exception ("Error during localization tool for android");
				}
				UpdateJob (outputAndroid);
			}

			UpdateJob ("Run Localization tool for Android Finished");

			UpdateJob("Run Localization tool for iOS");

            localizationToolRun = new ProcessStartInfo
            {
                FileName = "mono",
				RedirectStandardOutput = true,
				RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = Path.Combine(sourceDirectory, "Src", "LocalizationTool"),
                Arguments = "output/LocalizationTool.exe -t=ios -m=\"../Mobile/Common/Localization/Master.resx\" -d=\"../Mobile/iOS/en.lproj/Localizable.strings\" -s=\"../Mobile/Common/Settings/Settings.json\""
            };
            
            using (var exeProcess = Process.Start(localizationToolRun))
            {
				var outputiOS = ProcessEx.GetOutput (exeProcess);
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during localization tool for iOS");
                }
				UpdateJob (outputiOS);
            }

			UpdateJob("Run Localization tool for iOS Finished");

		}

		private void BuildMobile (string sourceDirectory)
		{			
			//Build
			var sourceMobileFolder = Path.Combine (sourceDirectory, "Src", "Mobile");

			_logger.DebugFormat ("Restore NuGet Packages");
			var restoreProcess = ProcessEx.GetProcess ("mono", string.Format ("--runtime=v4.0 \"/Users/apcurium/Library/Application Support/XamarinStudio-4.0/LocalInstall/Addins/MonoDevelop.PackageManagement.0.9/NuGet.exe\" restore \"{0}/TaxiHail.sln\"", 
										sourceMobileFolder), sourceMobileFolder);

			using (var exeProcess = Process.Start (restoreProcess)) {
				var output = ProcessEx.GetOutput (exeProcess);
				if (exeProcess.ExitCode > 0) {
                    UpdateJob("Error during restore NuGet Packages");
					//throw new Exception ("Error during Restore NuGet Packages, " + output);
				}
				UpdateJob ("Restore NuGet Packages Successful");
			}

			_logger.DebugFormat ("Build Solution");

			if (_job.IosAdhoc) {			

				_logger.DebugFormat ("Build iOS AdHoc");
				UpdateJob ("Build iOS AdHoc");
				var buildArgs = string.Format ("build \"--configuration:{0}\"  \"{1}/MK.Booking.Mobile.Solution.iOS.sln\"",
					"AdHoc|iPhone",
					sourceMobileFolder);

				_builder.BuildProject (buildArgs);


				_logger.Debug ("Build iOS AdHoc done");
			}

			if (_job.IosAppStore) {	

				_logger.DebugFormat ("Build iOS AppStore");
				UpdateJob ("Build iOS AppStore");
				var buildArgs = string.Format ("build \"--configuration:{0}\"  \"{1}/MK.Booking.Mobile.Solution.iOS.sln\"",				                             
					"AppStore|iPhone",
					sourceMobileFolder);

				_builder.BuildProject (buildArgs);


				_logger.Debug ("Build iOS AppStore done");
			}

			if (!_job.Android && !_job.CallBox)
				return;

			const string configAndroid = "Release";
			var projectLists = new List<string> {
                "GoogleMaps.M4B",
				"MK.Common.Android",
				"MK.Booking.Google.Android", 
				"MK.Booking.Maps.Android", 
				"MK.Booking.Api.Contract.Android", 
				"MK.Booking.Api.Client.Android",
				"MK.Booking.Mobile.Android"
			};

			_builder.BuildAndroidProject (projectLists, configAndroid, string.Format ("{0}/MK.Booking.Mobile.Solution.Android.sln", sourceMobileFolder));

			if (_job.Android) {
				UpdateJob ("Building project  Android");

				var buildClient = string.Format ("build \"--project:TaxiHail\" \"--configuration:{0}\" \"--target:SignAndroidPackage\"  \"{1}/MK.Booking.Mobile.Solution.Android.sln\"",
					configAndroid,
					sourceMobileFolder);
				_builder.BuildProject (buildClient);

				_logger.Debug ("Build Android done");
			}

			if (!_job.CallBox)
				return;

			UpdateJob ("Callbox project");
			var args = string.Format ("build \"--project:{0}\" \"--configuration:{1}\" \"--target:SignAndroidPackage\"  \"{2}/MK.Booking.Mobile.Solution.Android.sln\"",
				"MK.Callbox.Mobile.Client.Android",
				configAndroid,
				sourceMobileFolder);

			_builder.BuildProject (args);

			_logger.Debug ("Build Android CallBox done");
		}

		private static string GetSettingsFilePath (string sourceDirectory, string companyName)
		{
			return Path.Combine (sourceDirectory, "Config", companyName, "Settings.json");
		}

		private static void CopySettingsFileToOutputDir (string jsonSettingsFile, string targetFile)
		{
			var sb = new StringBuilder ();
			var reader = new JsonTextReader (new StreamReader (jsonSettingsFile));
			while (reader.Read ()) {
				if (reader.Value == null)
					continue;

				if (reader.TokenType == JsonToken.PropertyName) {
					sb.Append (string.Format ("{0}: ", reader.Value));
				} else {
					sb.AppendLine (reader.Value.ToString ());
				}
			}
			using (var outfile = new StreamWriter (targetFile, false)) {
				outfile.Write (sb.ToString ());
			}
		}
	}
}