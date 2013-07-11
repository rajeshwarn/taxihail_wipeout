using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using DeploymentServiceTools;
using log4net;
using MK.ConfigurationManager.Entities;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using PetaPoco;

namespace MK.DeploymentService.Mobile
{
	public class DeploymentJobService
	{
		private readonly Timer _timer;
		private readonly Object _resourceLock = new System.Object();
        private readonly ILog _logger;
        Database _db;
        DeploymentJob _job;

		const string HG_PATH = "/usr/local/bin/hg";
		
		public DeploymentJobService()
		{
			_timer = new Timer(TimerOnElapsed, null, Timeout.Infinite, Timeout.Infinite);
			_logger = LogManager.GetLogger("DeploymentJobService");
		}
		
		public void Start()
		{
			_timer.Change(0, 2000);
		}
		
		private void TimerOnElapsed(object state)
		{
			lock (_resourceLock)
			{
				CheckAndRunJobWithBuild ();
			}
		}

		void UpdateJob (string details = null, JobStatus? jobStatus = null)
		{
            _logger.Debug(details);

			if(jobStatus.HasValue)
			{
				_job.Status = jobStatus.Value;
			}
			if(details != null)
			{
				_job.Details += details + "\n";
			}

			_db.Update ("[MkConfig].[DeploymentJob]", "Id", new {
				status = _job.Status,
				details = _job.Details
			}, _job.Id);
		}


		private void CheckAndRunJobWithBuild ()
		{
			try {
				_db = new Database ("MKConfig");
				_job = _db.FirstOrDefault<DeploymentJob> ("Select * from [MkConfig].[DeploymentJob] where Status=0 AND (ANDROID=1 OR iOS_AdHoc=1 OR iOS_AppStore=1 OR CallBox=1)");
				try {


					if (_job != null) {
						var company = _db.First<Company> ("Select * from [MkConfig].[Company] where Id=@0", _job.Company_Id);
						_job.Company = company;
						var taxiHailEnv = _db.First<TaxiHailEnvironment> ("Select * from [MkConfig].[TaxiHailEnvironment] where Id=@0", _job.TaxHailEnv_Id);
						_job.TaxHailEnv = taxiHailEnv;

						if(_job.Version_Id != Guid.Empty)
						{
							_job.Version = _db.First<AppVersion> ("Select * from [MkConfig].[AppVersion] where Id=@0", _job.Version_Id);
						}

						_logger.Debug ("Begin work on " + company.Name);

						UpdateJob("Starting",JobStatus.INPROGRESS);

						var sourceDirectory = Path.Combine (Path.GetTempPath (), "TaxiHailSource");
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

                        var taxiRepo = new TaxiRepository(HG_PATH, sourceDirectory);
                        UpdateJob("FetchSource");
					    taxiRepo.FetchSource(_job.GetRevisionNumber(), str => UpdateJob(str));

						UpdateJob("Customize");
						Customize (sourceDirectory, company, taxiHailEnv);

						UpdateJob("Build");
						Build (sourceDirectory, company);

						UpdateJob("Deploy");
						Deploy (sourceDirectory, company, releaseiOSAdHocDir, releaseiOSAppStoreDir, releaseAndroidDir, releaseCallboxAndroidDir);

						UpdateJob("Done",JobStatus.SUCCESS);

						_logger.Debug("Deployment finished without error");
					}
				} catch (Exception e) {
					_logger.Error (e.Message);
					UpdateJob (e.Message,JobStatus.ERROR);
				}
			} catch (Exception e) {
				_logger.Error (e.Message);
			}
		}

		public void Stop()
		{
			_timer.Change(Timeout.Infinite, Timeout.Infinite);
		}

		void Deploy (string sourceDirectory, Company company, string ipaAdHocPath, string ipaAppStorePath, string apkPath, string apkPathCallBox)
		{
		    var hg = new MecurialTools(HG_PATH, sourceDirectory);

			if (_job.Android || _job.CallBox || _job.iOS_AdHoc || _job.iOS_AppStore) {
			    string targetDirWithoutFileName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["DeployDir"], 
			                                                   company.Name, 
			                                                   _job.Version != null 
			                                                       ? string.Format("{0}.{1}", _job.Version.Display, _job.Version.Revision)
			                                                       : string.Format("Not versioned/{0}", string.IsNullOrEmpty(_job.Revision)
			                                                                                                ? hg.GetTipRevisionNumber()
			                                                                                                : _job.Revision
			                                                             ));
				if (!Directory.Exists (targetDirWithoutFileName)) {
					Directory.CreateDirectory(targetDirWithoutFileName);
				}

				CopySettingsFileToOutputDir(GetSettingsFilePath(sourceDirectory, company.Name), Path.Combine(targetDirWithoutFileName, "Settings.txt"));

				if (_job.Android) {
					_logger.DebugFormat("Copying Apk");
					if (!Directory.Exists (apkPath)) {
						throw new Exception("Android release dir does not exist, there probably was a problem with the build or a project was added to the solution without being added in the list of projects to build.");
					}
					var apkFile = Directory.EnumerateFiles(apkPath, "*-Signed.apk", SearchOption.TopDirectoryOnly).FirstOrDefault();
					if(apkFile != null)
					{
						var fileInfo = new FileInfo(apkFile); 
						var targetDir = Path.Combine(targetDirWithoutFileName, fileInfo.Name);
						if(File.Exists(targetDir)) File.Delete(targetDir);
						File.Copy(apkFile, targetDir);
					}
					else
					{
						throw new Exception("Can't find the APK file in the release dir");
					}
				}
				
				if(_job.CallBox)
				{
					_logger.DebugFormat("Copying CallBox Apk");
					var apkFile = Directory.EnumerateFiles(apkPathCallBox, "*-Signed.apk", SearchOption.TopDirectoryOnly).FirstOrDefault();
					if(apkFile != null)
					{
						var fileInfo = new FileInfo(apkFile); 
						var targetDir = Path.Combine(targetDirWithoutFileName, fileInfo.Name);
						if(File.Exists(targetDir)) File.Delete(targetDir);
						File.Copy(apkFile, targetDir);
					}
					else
					{
						throw new Exception("Can't find the CallBox APK file in the release dir");
					}
				}
				
				if (_job.iOS_AdHoc) {
					_logger.DebugFormat ("Uploading and copying IPA AdHoc");
					var ipaFile = Directory.EnumerateFiles(ipaAdHocPath, "*.ipa", SearchOption.TopDirectoryOnly).FirstOrDefault();
					if(ipaFile != null)
					{
						var fileUplaoder = new FileUploader();
						fileUplaoder.Upload(ipaFile);
						
						var fileInfo = new FileInfo(ipaFile); 
						var targetDir = Path.Combine(targetDirWithoutFileName, fileInfo.Name);
						if(File.Exists(targetDir)) File.Delete(targetDir);
						File.Copy(ipaFile, targetDir);
					}
					else
					{
						throw new Exception("Can't find the IPA file in the AdHoc dir");
					}
				}

				if (_job.iOS_AppStore) {
					_logger.DebugFormat ("Uploading and copying IPA AppStore");
					var ipaFile = Directory.EnumerateFiles(ipaAppStorePath, "*.ipa", SearchOption.TopDirectoryOnly).FirstOrDefault();
					if(ipaFile != null)
					{
										
						var fileInfo = new FileInfo(ipaFile); 
						var newName = fileInfo.Name.Replace(".ipa", ".appstore.ipa");
						var targetDir = Path.Combine(targetDirWithoutFileName, newName);
						if(File.Exists(targetDir)) File.Delete(targetDir);
						File.Copy(ipaFile, targetDir);
					}
					else
					{
						throw new Exception("Can't find the IPA file in the AppStore dir");
					}
				}
			}
		}



		void Customize (string sourceDirectory, Company company, TaxiHailEnvironment taxiHailEnv)
		{
			_logger.DebugFormat ("Generate Settings");

			var jsonSettings = new JObject ();
			foreach (var setting in company.MobileConfigurationProperties) {
				jsonSettings.Add (setting.Key, JToken.FromObject (setting.Value));
			}

			var serviceUrl = string.Format ("{0}/{1}/api/", taxiHailEnv.Url, company.ConfigurationProperties["TaxiHail.ServerCompanyName"]);
			if (company.MobileConfigurationProperties.ContainsKey ("IsCMT")) 
			{
				var isCMT = bool.Parse(company.MobileConfigurationProperties["IsCMT"]);
				if(isCMT)
				{
					serviceUrl = taxiHailEnv.Url;
				}
			}

			if (company.MobileConfigurationProperties.ContainsKey ("ServiceUrl")) 
			{
				jsonSettings["ServiceUrl"] = JToken.FromObject(serviceUrl);
			} else 
			{
				jsonSettings.Add("ServiceUrl", JToken.FromObject(serviceUrl));
			}

			var jsonSettingsFile = GetSettingsFilePath(sourceDirectory, company.Name);
			var stringBuilder = new StringBuilder();
			jsonSettings.WriteTo(new JsonTextWriter(new StringWriter(stringBuilder)));
			File.WriteAllText(jsonSettingsFile, stringBuilder.ToString());

			_logger.DebugFormat ("Build Config Tool Customization");
			UpdateJob ("Customize - Build Config Tool Customization");

			
			var ninePatchProjectConfi = String.Format ("\"--project:{0}\" \"--configuration:{1}\"", "NinePatchMaker", "Debug");
			BuildProject( string.Format("build "+ninePatchProjectConfi+"  \"{0}/ConfigTool.iOS.sln\"", Path.Combine (sourceDirectory,"Src","ConfigTool")));

			var mainConfig = String.Format ("\"--project:{0}\" \"--configuration:{1}\"", "apcurium.MK.Booking.ConfigTool", "Debug|x86");
			BuildProject( string.Format("build "+mainConfig+"  \"{0}/ConfigTool.iOS.sln\"", Path.Combine (sourceDirectory,"Src","ConfigTool")));




			_logger.DebugFormat ("Run Config Tool Customization");

			var workingDirectory = Path.Combine (sourceDirectory, "Src", "ConfigTool", "apcurium.MK.Booking.ConfigTool.Console", "bin", "Debug");
			var configToolRun = ProcessEx.GetProcess ( "mono", string.Format("apcurium.MK.Booking.ConfigTool.exe {0}", company.Name),  workingDirectory);

			using (var exeProcess = Process.Start(configToolRun))
			{
				var output = ProcessEx.GetOutput (exeProcess);
				if (exeProcess.ExitCode > 0)
				{
					throw new Exception("Error during customization, "+output);
				}
			    UpdateJob ("Customize Successful");
			}

			_logger.DebugFormat ("Customization Finished");
			_logger.DebugFormat ("Run Localization tool for Android");

			var localizationToolRun = new ProcessStartInfo
			{
				FileName = "mono",
				UseShellExecute = false,
				WorkingDirectory = Path.Combine (sourceDirectory,"Src", "LocalizationTool"),
				Arguments = "output/LocalizationTool.exe -t=android -m=\"../Mobile/Common/Localization/Master.resx\" -d=\"../Mobile/Android/Resources/Values/String.xml\" -s=\"../Mobile/Common/Settings/Settings.json\""
			};
			
			using (var exeProcess = Process.Start(localizationToolRun))
			{
				exeProcess.WaitForExit();
				if (exeProcess.ExitCode > 0)
				{
					throw new Exception("Error during localization tool");
				}
			}

			_logger.DebugFormat ("Run Localization tool for Android Finished");
		}

		private void Build (string sourceDirectory, Company company)
		{			
			//Build
			_logger.DebugFormat ("Launch Customization");
			var sourceMobileFolder = Path.Combine (sourceDirectory, "Src", "Mobile");
			
			_logger.DebugFormat ("Build Solution");

			if (_job.iOS_AdHoc) {			
				
				_logger.DebugFormat ("Build iOS AdHoc");
				UpdateJob("Build iOS AdHoc");
				var buildArgs = string.Format("build \"--configuration:{0}\"  \"{1}/MK.Booking.Mobile.Solution.iOS.sln\"",
				                              "AdHoc|iPhone",
				                              sourceMobileFolder);
				
				BuildProject(buildArgs);
				
				
				_logger.Debug("Build iOS AdHoc done");
			}
			
			if (_job.iOS_AppStore) {	

				_logger.DebugFormat ("Build iOS AppStore");
				UpdateJob("Build iOS AppStore");
				var buildArgs = string.Format("build \"--configuration:{0}\"  \"{1}/MK.Booking.Mobile.Solution.iOS.sln\"",				                             
				                              "AppStore|iPhone",
				                              sourceMobileFolder);
				
				BuildProject(buildArgs);
				
				
				_logger.Debug("Build iOS AppStore done");
			}

			if (_job.Android || _job.CallBox) {

				var configAndroid = "Release";
				var projectLists = new List<string>{
					"Android_System.Reactive.Interfaces", 
					"Android_System.Reactive.Core", 
					"Android_System.Reactive.PlatformServices", 
					"Android_System.Reactive.Linq",
					"PushSharp.Client.MonoForAndroid.Gcm",
					"Newtonsoft.Json.MonoDroid", 
					"Cirrious.MvvmCross.Android", 
					"Cirrious.MvvmCross.Binding.Android", 
					"Cirrious.MvvmCross.Android.Maps",
					"BraintreeEncryption.Library.Android",
					"MK.Common.Android",
					"MK.Booking.Google.Android", 
					"MK.Booking.Maps.Android", 
					"MK.Booking.Api.Contract.Android", 
					"MK.Booking.Api.Client.Android",
					"MK.Booking.Mobile.Android"
				};

				UpdateJob("Build android");
				int i = 1;
				foreach (var projectName in projectLists) {
					var config = string.Format ("\"--project:{0}\" \"--configuration:{1}\"", projectName, configAndroid)+" ";
					var buildArgs = string.Format("build "+config+"\"{0}/MK.Booking.Mobile.Solution.Android.sln\"",
					                              sourceMobileFolder);

					UpdateJob ("Step " + (i++) + "/" + projectLists.Count);
					BuildProject(buildArgs);
				}
				UpdateJob("Step "+i+"/"+projectLists.Count);

				if (_job.Android) {



					var buildClient = string.Format("build \"--project:{0}\" \"--configuration:{1}\" \"--target:SignAndroidPackage\"  \"{2}/MK.Booking.Mobile.Solution.Android.sln\"",
					                                "MK.Booking.Mobile.Client.Android",
					                                configAndroid,
					                                sourceMobileFolder);
					BuildProject(buildClient);
					
					_logger.Debug("Build Android done");
				}
				
				if(_job.CallBox)
				{
					var buildClient = string.Format("build \"--project:{0}\" \"--configuration:{1}\" \"--target:SignAndroidPackage\"  \"{2}/MK.Booking.Mobile.Solution.Android.sln\"",
					                                "MK.Callbox.Mobile.Client.Android",
					                                configAndroid,
					                                sourceMobileFolder);
					BuildProject(buildClient);
					
					_logger.Debug("Build Android CallBox done");
				}
			}
		}

		private void BuildProject (string buildArgs)
		{
			UpdateJob ("Running Build - " + buildArgs);

			var buildiOSproject = ProcessEx.GetProcess("/Applications/Xamarin Studio.app/Contents/MacOS/mdtool", buildArgs);
			using (var exeProcess = Process.Start(buildiOSproject))
			{

				var output = ProcessEx.GetOutput(exeProcess,40000);
				if (exeProcess.ExitCode > 0)
				{
					throw new Exception("Error during build project step" + output.Replace("\n","\r\n"));
				}
			    UpdateJob ("Build Successful");
			}
		}


		private string GetSettingsFilePath(string sourceDirectory, string companyName)
		{
			return Path.Combine(sourceDirectory, "Config" , companyName, "Settings.json");
		}

		private void CopySettingsFileToOutputDir(string jsonSettingsFile, string targetFile)
		{
			var sb = new StringBuilder();
			var reader = new JsonTextReader(new StreamReader(jsonSettingsFile));
			while (reader.Read())
			{
				if (reader.Value != null) {
					if (reader.TokenType == JsonToken.PropertyName){
						sb.Append(string.Format("{0}: ", reader.Value));
					}
					else
					{
						sb.AppendLine(reader.Value.ToString());
					}
				}
			}
			using (var outfile = new StreamWriter(targetFile, false))
			{
				outfile.Write(sb.ToString());
			}
		}




	}
}