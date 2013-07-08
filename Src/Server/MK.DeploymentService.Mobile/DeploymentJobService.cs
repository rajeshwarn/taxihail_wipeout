using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using log4net;
using MK.ConfigurationManager.Entities;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace MK.DeploymentService.Mobile
{
	public class DeploymentJobService
	{
		private readonly Timer timer;
		private System.Object resourceLock = new System.Object();
		private readonly ILog logger;

		const string HgPath = "/usr/local/bin/hg";
		
		public DeploymentJobService()
		{
			timer = new Timer(TimerOnElapsed, null, Timeout.Infinite, Timeout.Infinite);
			logger = LogManager.GetLogger("DeploymentJobService");
		}
		
		public void Start()
		{
			timer.Change(0, 2000);
		}
		
		private void TimerOnElapsed(object state)
		{
			lock (resourceLock)
			{
				CheckAndRunJobWithBuild ();
			}
		}
		
		private void CheckAndRunJobWithBuild ()
		{
			try {
				var db = new PetaPoco.Database ("MKConfig");
				var job = db.FirstOrDefault<DeploymentJob> ("Select * from [MkConfig].[DeploymentJob] where Status=0 AND (ANDROID=1 OR iOS_AdHoc=1 OR iOS_AppStore=1 OR CallBox=1)");
				try {

					if (job != null) {
						var company = db.First<Company> ("Select * from [MkConfig].[Company] where Id=@0", job.Company_Id);
						job.Company = company;
						var taxiHailEnv = db.First<TaxiHailEnvironment> ("Select * from [MkConfig].[TaxiHailEnvironment] where Id=@0", job.TaxHailEnv_Id);
						job.TaxHailEnv = taxiHailEnv;

						if(job.Version_Id != Guid.Empty)
						{
							job.Version = db.First<AppVersion> ("Select * from [MkConfig].[AppVersion] where Id=@0", job.Version_Id);
						}

						logger.Debug ("Begin work on " + company.Name);
						db.Update ("[MkConfig].[DeploymentJob]", "Id", new { status = JobStatus.INPROGRESS }, job.Id);

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

						FetchSource (job, sourceDirectory, company);

						Customize (sourceDirectory, company, taxiHailEnv);

						Build (job, sourceDirectory, company);

						Deploy (job, sourceDirectory, company, releaseiOSAdHocDir, releaseiOSAppStoreDir, releaseAndroidDir, releaseCallboxAndroidDir);

						db.Update ("[MkConfig].[DeploymentJob]", "Id", new { status = JobStatus.SUCCESS }, job.Id);

						logger.Debug("Deployment finished without error");
					}
				} catch (Exception e) {
					logger.Error (e.Message);
					db.Update ("[MkConfig].[DeploymentJob]", "Id", new { status = JobStatus.ERROR, details = e.Message }, job.Id);
				}
			} catch (Exception e) {
				logger.Error (e.Message);
			}
		}

		public void Stop()
		{
			timer.Change(Timeout.Infinite, Timeout.Infinite);
		}

		void Deploy (DeploymentJob job, string sourceDirectory, Company company, string ipaAdHocPath, string ipaAppStorePath, string apkPath, string apkPathCallBox)
		{
			if (job.Android || job.CallBox || job.iOS_AdHoc || job.iOS_AppStore) {
				var targetDirWithoutFileName = string.Empty;
				targetDirWithoutFileName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["DeployDir"], 
				                                        company.Name, 
				                                        job.Version != null 
				                                        	? string.Format("{0}.{1}", job.Version.Display, job.Version.Revision)
				                                        	: string.Format("Not versioned/{0}", string.IsNullOrEmpty(job.Revision) 
				                																				? GetLatestRevision(sourceDirectory)
				                																				: job.Revision
				                                        ));
				if (!Directory.Exists (targetDirWithoutFileName)) {
					Directory.CreateDirectory(targetDirWithoutFileName);
				}

				CopySettingsFileToOutputDir(GetSettingsFilePath(sourceDirectory, company.Name), Path.Combine(targetDirWithoutFileName, "Settings.txt"));

				if (job.Android) {
					logger.DebugFormat("Copying Apk");
					if (!Directory.Exists (apkPath)) {
						throw new Exception("Android release dir does not exist, there probably was a problem with the build.");
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
				
				if(job.CallBox)
				{
					logger.DebugFormat("Copying CallBox Apk");
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
				
				if (job.iOS_AdHoc) {
					logger.DebugFormat ("Uploading and copying IPA AdHoc");
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

				if (job.iOS_AppStore) {
					logger.DebugFormat ("Uploading and copying IPA AppStore");
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

		private void FetchSource (DeploymentJob job, string sourceDirectory, Company company)
		{
			//pull source from bitbucket if not done yet
			string revision = string.Empty;
			if(job.Version != null)
			{
				revision = "-r " + job.Version.Revision;

			}else{
				revision = string.IsNullOrEmpty (job.Revision) ? string.Empty : "-r " + job.Revision;
			}
			 			
			logger.DebugFormat("Fetching revision: {0}", revision == string.Empty ? GetLatestRevision(sourceDirectory) : revision);

			if (!Directory.Exists (sourceDirectory)) {
				logger.DebugFormat ("Clone Source Code");

				var args = string.Format (@"clone {1} https://buildapcurium:apcurium5200!@bitbucket.org/apcurium/mk-taxi {0}", sourceDirectory, revision);
				var hgClone = GetProcess(HgPath, args);
				using (var exeProcess = Process.Start(hgClone))
				{
					var output = GetOutput(exeProcess);
					if (exeProcess.ExitCode > 0)
					{
						throw new Exception("Error during clone source code step" + output);
					}
				}
			} else {
				logger.DebugFormat ("Revert, Purge and Update Source Code");
				//already clone just do a revert and update the source
				RevertAndPull (sourceDirectory);
			}
			
			//fetch revision if needed
			if (!string.IsNullOrEmpty (revision)) {
				logger.DebugFormat ("Update to revision {0}", revision);

				var hgUpdate = GetProcess(HgPath, string.Format("update --repository {0} {1}", sourceDirectory, revision));
				using (var exeProcess = Process.Start(hgUpdate))
				{
					var output = GetOutput(exeProcess);
					if (exeProcess.ExitCode > 0)
					{
						throw new Exception("Error during updating to revision step" + output);
					}
				}
			}
		}

		void Customize (string sourceDirectory, Company company, TaxiHailEnvironment taxiHailEnv)
		{
			logger.DebugFormat ("Generate Settings");

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

			logger.DebugFormat ("Build Config Tool Customization");

			var buildArgs = string.Format("build \"--project:{0}\" \"--configuration:{1}\"  \"{2}/ConfigTool.iOS.sln\"",
			                              "apcurium.MK.Booking.ConfigTool",
			                              "Debug|x86",
			                              Path.Combine (sourceDirectory,"Src","ConfigTool"));
			
			BuildProject(buildArgs);

			logger.DebugFormat ("Run Config Tool Customization");

			var configToolRun = new ProcessStartInfo
			{
				FileName = "mono",
				UseShellExecute = false,
				WorkingDirectory = Path.Combine (sourceDirectory,"Src", "ConfigTool", "apcurium.MK.Booking.ConfigTool.Console", "bin", "Debug"),
				Arguments = string.Format("apcurium.MK.Booking.ConfigTool.exe {0}", company.Name)
			};
			
			using (var exeProcess = Process.Start(configToolRun))
			{
				exeProcess.WaitForExit();
				if (exeProcess.ExitCode > 0)
				{
					throw new Exception("Error during customization");
				}
			}

			logger.DebugFormat ("Customization Finished");
			logger.DebugFormat ("Run Localization tool for Android");

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

			logger.DebugFormat ("Run Localization tool for Android Finished");
		}

		private void Build (DeploymentJob job, string sourceDirectory, Company company)
		{			
			//Build
			logger.DebugFormat ("Launch Customization");
			var sourceMobileFolder = Path.Combine (sourceDirectory, "Src", "Mobile");
			
			logger.DebugFormat ("Build Solution");

			if (job.iOS_AdHoc) {			
				
				logger.DebugFormat ("Build iOS AdHoc");
				var buildArgs = string.Format("build \"--configuration:{0}\"  \"{1}/MK.Booking.Mobile.Solution.iOS.sln\"",
				                              "AdHoc|iPhone",
				                              sourceMobileFolder);
				
				BuildProject(buildArgs);
				
				
				logger.Debug("Build iOS AdHoc done");
			}
			
			if (job.iOS_AppStore) {	

				logger.DebugFormat ("Build iOS AppStore");
				var buildArgs = string.Format("build \"--configuration:{0}\"  \"{1}/MK.Booking.Mobile.Solution.iOS.sln\"",				                             
				                              "AppStore|iPhone",
				                              sourceMobileFolder);
				
				BuildProject(buildArgs);
				
				
				logger.Debug("Build iOS AppStore done");
			}

			if (job.Android || job.CallBox) {

				var configAndroid = "Release";
				var projectLists = new List<string>{
					"Android_System.Reactive.Interfaces", "Android_System.Reactive.Core", "Android_System.Reactive.PlatformServices", "Android_System.Reactive.Linq",
					"PushSharp.Client.MonoForAndroid.Gcm",
					"Newtonsoft.Json.MonoDroid", "Cirrious.MvvmCross.Android", "Cirrious.MvvmCross.Binding.Android", "Cirrious.MvvmCross.Android.Maps",
					"MK.Common.Android", "MK.Booking.Google.Android", "MK.Booking.Maps.Android", "MK.Booking.Api.Contract.Android", "MK.Booking.Api.Client.Android",
					"MK.Booking.Mobile.Android"
				};
				
				foreach (var projectName in projectLists) {
					
					var buildArgs = string.Format("build \"--project:{0}\" \"--configuration:{1}\"  \"{2}/MK.Booking.Mobile.Solution.Android.sln\"",
					                              projectName,
					                              configAndroid,
					                              sourceMobileFolder);
					
					BuildProject(buildArgs);
				}

				if (job.Android) {
					
					//the client needs a target
					var buildClient = string.Format("build \"--project:{0}\" \"--configuration:{1}\" \"--target:SignAndroidPackage\"  \"{2}/MK.Booking.Mobile.Solution.Android.sln\"",
					                                "MK.Booking.Mobile.Client.Android",
					                                configAndroid,
					                                sourceMobileFolder);
					BuildProject(buildClient);
					
					logger.Debug("Build Android done");
				}
				
				if(job.CallBox)
				{
					var buildClient = string.Format("build \"--project:{0}\" \"--configuration:{1}\" \"--target:SignAndroidPackage\"  \"{2}/MK.Booking.Mobile.Solution.Android.sln\"",
					                                "MK.Callbox.Mobile.Client.Android",
					                                configAndroid,
					                                sourceMobileFolder);
					BuildProject(buildClient);
					
					logger.Debug("Build Android CallBox done");
				}
			}
		}

		private void BuildProject (string buildArgs)
		{
			logger.Debug("Build Project : " + buildArgs);
			var buildiOSproject = new ProcessStartInfo
			{
				FileName = "/Applications/Xamarin Studio.app/Contents/MacOS/mdtool",
				UseShellExecute = false,
				Arguments = buildArgs
			};
			var exeProcess = Process.Start(buildiOSproject);
			exeProcess.WaitForExit();

//			var buildiOSproject = GetProcess("/Applications/Xamarin Studio.app/Contents/MacOS/mdtool", buildArgs);
//			using (var exeProcess = Process.Start(buildiOSproject))
//			{
//				var output = GetOutput(exeProcess);
//				if (exeProcess.ExitCode > 0)
//				{
//					throw new Exception("Error during build project step" + output);
//				}
//			}
		}

		private void RevertAndPull(string repository)
		{
			var hgRevert = GetProcess(HgPath, string.Format("update --repository {0} -C", repository));
			using (var exeProcess = Process.Start(hgRevert))
			{
				var output = GetOutput(exeProcess);
				if (exeProcess.ExitCode > 0)
				{
					throw new Exception("Error during revert source code step" + output);
				}
			}

			var hgPurge = GetProcess(HgPath, string.Format("purge --all --repository {0}", repository));
			using (var exeProcess = Process.Start(hgPurge))
			{
				var output = GetOutput(exeProcess);
				if (exeProcess.ExitCode > 0)
				{
					throw new Exception("Error during purge source code step" + output);
				}
			}

			var hgPull = GetProcess(HgPath, string.Format("pull https://buildapcurium:apcurium5200!@bitbucket.org/apcurium/mk-taxi --repository {0}", repository));
			using (var exeProcess = Process.Start(hgPull))
			{
				var output = GetOutput(exeProcess);
				if (exeProcess.ExitCode > 0)
				{
					throw new Exception("Error during pull source code step" + output);
				}
			}

			var hgUpdate = GetProcess(HgPath, string.Format ("update --repository {0}", repository));
			using (var exeProcess = Process.Start(hgUpdate))
			{
				var output = GetOutput(exeProcess);
				if (exeProcess.ExitCode > 0)
				{
					throw new Exception("Error during update source code step" + output);
				}
			}
		}

		private string GetSettingsFilePath(string sourceDirectory, string companyName)
		{
			return Path.Combine(sourceDirectory, "Config" , companyName, "Settings.json");
		}

		private void CopySettingsFileToOutputDir(string jsonSettingsFile, string targetFile)
		{
			StringBuilder sb = new StringBuilder();
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
			using (StreamWriter outfile = new StreamWriter(targetFile, false))
			{
				outfile.Write(sb.ToString());
			}
		}

		private string GetLatestRevision(string repository)
		{
			var revision = string.Empty;
			var hgRevert = new ProcessStartInfo
			{
				FileName = HgPath,
				UseShellExecute = false,
				Arguments = string.Format("identify --repository \"{0}\"", repository),
				RedirectStandardOutput = true,

			};
			
			using (var exeProcess = Process.Start(hgRevert))
			{
				exeProcess.WaitForExit();
				if (exeProcess.ExitCode > 0)
				{
					throw new Exception("Error during get of latest revision name");
				}
				revision = exeProcess.StandardOutput.ReadLine();
			}

			return revision;
		}

		private ProcessStartInfo GetProcess(string filename, string args)
		{
			logger.DebugFormat("Starting process {0} with args {1}", filename, args);
			return new ProcessStartInfo
			{
				FileName = filename,
				UseShellExecute = false,
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
	}
}