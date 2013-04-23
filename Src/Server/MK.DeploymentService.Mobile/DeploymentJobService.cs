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
				var job = db.FirstOrDefault<DeploymentJob> ("Select * from [MkConfig].[DeploymentJob] where Status=0 AND (ANDROID=1 OR iOS=1 OR CallBox=1)");
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
						var releaseiOSDir = Path.Combine (sourceDirectory, "Src", "Mobile", "iOS", "bin", "iPhone", "Release");
						if (Directory.Exists (releaseiOSDir))
							Directory.Delete (releaseiOSDir, true);

						var releaseAndroidDir = Path.Combine (sourceDirectory, "Src", "Mobile", "Android", "bin", "Release");
						if (Directory.Exists (releaseAndroidDir))
							Directory.Delete (releaseAndroidDir, true);

						var releaseCallboxAndroidDir = Path.Combine (sourceDirectory, "Src", "Mobile", "MK.Callbox.Mobile.Client.Android", "bin", "Release");
						if (Directory.Exists (releaseCallboxAndroidDir))
							Directory.Delete (releaseCallboxAndroidDir, true);

						FetchSource (job, sourceDirectory, company);

						Customize (sourceDirectory, company, taxiHailEnv);

						Build (job, sourceDirectory, company);

						Deploy (job, sourceDirectory, company, releaseiOSDir, releaseAndroidDir, releaseCallboxAndroidDir);

						db.Update ("[MkConfig].[DeploymentJob]", "Id", new { status = JobStatus.SUCCESS }, job.Id);

						logger.Debug("Deployment finished without error");
					}
				} catch (Exception e) {
					logger.Error (e.Message);
					db.Update ("[MkConfig].[DeploymentJob]", "Id", new { status = JobStatus.ERROR }, job.Id);
				}
			} catch (Exception e) {
				logger.Error (e.Message);
			}
		}

		public void Stop()
		{
			timer.Change(Timeout.Infinite, Timeout.Infinite);
		}

		void Deploy (DeploymentJob job, string sourceDirectory, Company company, string ipaPath, string apkPath, string apkPathCallBox)
		{
			var targetDirWithoutFileName = string.Empty;
			if (job.Android || job.CallBox || job.iOS) {
				targetDirWithoutFileName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["DeployDir"], 
				                                        company.Name, 
				                                        job.Version != null 
				                                        	? string.Format("{0}.{1}", job.Version.Display, job.Version.Revision)
				                                        	: string.Format("Not versioned/{0}", job.Revision)
				                                        );
				if (!Directory.Exists (targetDirWithoutFileName)) {
					Directory.CreateDirectory(targetDirWithoutFileName);
				}else
				{
					var contentOfTargetDirectory = new DirectoryInfo(targetDirWithoutFileName).GetFiles();
					foreach (FileInfo file in contentOfTargetDirectory)
					{
						file.Delete(); 
					}
				}
			}

			if (job.Android) {
                logger.DebugFormat("Copying Apk");
                var apkFile = Directory.EnumerateFiles(apkPath, "*-Signed.apk", SearchOption.TopDirectoryOnly).FirstOrDefault();
                if(apkFile != null)
			    {
					var fileInfo = new FileInfo(apkFile); 
					var targetDir = Path.Combine(targetDirWithoutFileName, fileInfo.Name);
					if(File.Exists(targetDir)) File.Delete(targetDir);
					File.Copy(apkFile, targetDir);
			    }else
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

			if (job.iOS) {
				logger.DebugFormat ("Uploading and copying IPA");
				var ipaFile = Directory.EnumerateFiles(ipaPath, "*.ipa", SearchOption.TopDirectoryOnly).FirstOrDefault();
				if(ipaFile != null)
				{
					var fileUplaoder = new FileUploader();
					fileUplaoder.Upload(ipaFile);

					var fileInfo = new FileInfo(ipaFile); 
					var targetDir = Path.Combine(targetDirWithoutFileName, fileInfo.Name);
					if(File.Exists(targetDir)) File.Delete(targetDir);
					File.Copy(ipaFile, targetDir);
				}else
				{
				    throw new Exception("Can't find the IPA file in the release dir");
				}
			}
		}

		private void FetchSource (DeploymentJob job, string sourceDirectory, Company company)
		{
			//pull source from bitbucket if not done yet
			string revision = string.Empty;
			if(job.Version != null)
			{
				revision = job.Version.Revision;

			}else{
				revision = string.IsNullOrEmpty (job.Revision) ? string.Empty : "-r " + job.Revision;
			}
			 			
			if (!Directory.Exists (sourceDirectory)) {
				logger.DebugFormat ("Clone Source Code");
				Directory.CreateDirectory (sourceDirectory);
				var args = string.Format (@"clone {1} https://buildapcurium:apcurium5200!@bitbucket.org/apcurium/mk-taxi {0}",
				                         sourceDirectory, revision);
				
				var hgClone = new ProcessStartInfo
				{
					FileName = HgPath,
					UseShellExecute = false,
					Arguments = args
				};
				
				using (var exeProcess = Process.Start(hgClone)) {
					exeProcess.WaitForExit ();
					if (exeProcess.ExitCode > 0) {
						throw new Exception ("Error during clone source code step");
					}
				}
			} else {
				logger.DebugFormat ("Revert, Purge and Update Source Code");
				//already clone just do a revert and update the source
				RevertAndPull (sourceDirectory);
			}
			
			//fetch revision if needed
			if (!string.IsNullOrEmpty (job.Revision)) {
				logger.DebugFormat ("Update to revision {0}", job.Revision);
				var hgUpdate = new ProcessStartInfo
				{
					FileName = HgPath,
					UseShellExecute = false,
					Arguments =
					string.Format("update --repository {0} -r {1}", sourceDirectory, job.Revision)
				};
				
				using (var exeProcess = Process.Start(hgUpdate)) {
					exeProcess.WaitForExit ();
					if (exeProcess.ExitCode > 0) {
						throw new Exception ("Error during revert source code step");
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
			
			var jsonSettingsFile = Path.Combine(sourceDirectory, "Config" , company.Name, "Settings.json");
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
			if (job.iOS) {
				
				var configIOS = "Release|iPhone";
				var projectLists = new List<string>{
					"Newtonsoft_Json_MonoTouch", "Cirrious.MvvmCross.Touch", "Cirrious.MvvmCross.Binding.Touch", "Cirrious.MvvmCross.Dialog.Touch",
					"SocialNetworks.Services.MonoTouch", "MK.Common.iOS", "MK.Booking.Google.iOS", "MK.Booking.Maps.iOS", "MK.Booking.Api.Contract.iOS", "MK.Booking.Api.Client.iOS",
					"MK.Booking.Mobile.iOS", "MK.Booking.Mobile.Client.iOS"
				};

				foreach (var projectName in projectLists) {

					var buildArgs = string.Format("build \"--project:{0}\" \"--configuration:{1}\"  \"{2}/MK.Booking.Mobile.Solution.iOS.sln\"",
					                              projectName,
					                              configIOS,
					                              sourceMobileFolder);
					
					BuildProject(buildArgs);
				}

				logger.Debug("Build iOS done");
			}

			if (job.Android || job.CallBox) {

				var configAndroid = "Release";
				var projectLists = new List<string>{
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
		}

		private void RevertAndPull(string repository)
		{
			var hgRevert = new ProcessStartInfo
			{
				FileName = HgPath,
				UseShellExecute = false,
				Arguments = string.Format("update --repository {0} -C", repository)
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
				FileName = HgPath,
				UseShellExecute = false,
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
				FileName = HgPath,
				UseShellExecute = false,
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
				FileName = HgPath,
				UseShellExecute = false,
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
	}
}


