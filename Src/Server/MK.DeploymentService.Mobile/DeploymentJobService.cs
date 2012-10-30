using System;
using System.Threading;
using log4net;
using MK.ConfigurationManager.Entities;
using System.IO;
using System.Diagnostics;
using apcurium.MK.Booking.ConfigTool;

namespace MK.DeploymentService.Mobile
{
	public class DeploymentJobService
	{
		private readonly Timer timer;
		private System.Object resourceLock = new System.Object();
		private readonly ILog logger;
		
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
			var db = new PetaPoco.Database ("MKConfig");
			var job = db.FirstOrDefault<DeploymentJob> ("Select * from [MkConfig].[DeploymentJob] where Status=0 AND (ANDROID=1 OR iOS=1)");
			try {


				if (job != null) {
					var company = db.First<Company>("Select * from [MkConfig].[Company] where Id=@0", job.Company_Id);
					var taxiHailEnv = db.First<TaxiHailEnvironment>("Select * from [MkConfig].[TaxiHailEnvironment] where Id=@0", job.TaxHailEnv_Id);
					logger.Debug ("Begin work on " + company.Name);
					db.Update("[MkConfig].[DeploymentJob]", "Id", new { status = JobStatus.INPROGRESS }, job.Id);

					var sourceDirectory = Path.Combine(Path.GetTempPath(), "TaxiHailSource");
					FetchSourceAndBuild(job, sourceDirectory, company);
				}
			} catch (Exception e) {
				logger.Error(e.Message);
				db.Update("[MkConfig].[DeploymentJob]", "Id", new { status = JobStatus.ERROR }, job.Id);
			}
		}

		public void Stop()
		{
			timer.Change(Timeout.Infinite, Timeout.Infinite);
		}

		private void FetchSourceAndBuild(DeploymentJob job, string sourceDirectory, Company company)
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
					FileName = "hg",
					UseShellExecute = false,
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
					FileName = "hg",
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

			//Customization of the app
			logger.DebugFormat("Run Customization");
			var configCompanyFolder = Path.Combine(sourceDirectory, "Config", company.Name);
			var sourceFolder = Path.Combine(sourceDirectory, "Src");
			var appConfigTool = new AppConfig(company.Name, configCompanyFolder, sourceFolder);
			appConfigTool.Apply();

			//Build
			logger.DebugFormat("Launch Customization");
			var buildScriptFolder = Path.Combine(sourceDirectory, "Deployment", "Mobile");
			var buildArgs = string.Format("{0} {1} {2} {3}", 
			                              	job.Android ? "Y" : "N",
			                              	job.iOS ? "Y" : "N",
			                              	job.iOS ? "Y" : "N",
			                              	company.Name);
			
			logger.DebugFormat("Build Solution");
			var buildPackage = new ProcessStartInfo
			{
				FileName = Path.Combine (buildScriptFolder, "Build.sh"),
				WorkingDirectory = buildScriptFolder,
				WindowStyle = ProcessWindowStyle.Hidden,
				UseShellExecute = false,
				CreateNoWindow = false,
				Arguments = buildArgs
			};
			
			using (var exeProcess = Process.Start(buildPackage))
			{
				exeProcess.WaitForExit();
				if (exeProcess.ExitCode > 0)
				{
					throw new Exception("Error during build step");
				}
			}
		}

		private void RevertAndPull(string repository)
		{
			var hgRevert = new ProcessStartInfo
			{
				FileName = "hg",
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
				FileName = "hg",
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
				FileName = "hg",
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
				FileName = "hg",
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
	}
}


