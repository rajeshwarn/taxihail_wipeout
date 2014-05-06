using System;
using System.IO;
using System.Net.Http;

namespace MK.DeploymentService.Mobile
{
	public class DeployInfo
	{
		public DeployInfo ()
		{


		}

		public string RootPath {
			get;
			set;
		}


		public string AndroidApkFileName {
			get;
			set;
		}

		public string CallboxApkFileName {
			get;
			set;
		}

		public string iOSAppStoreFileName {
			get;
			set;
		}

		public string iOSAdhocFileName {
			get;
			set;
		}

		public string iOSAdhocFile {
			get{ return Path.Combine (RootPath, iOSAdhocFileName); }
		}

		public bool iOSAdhocFileExist
		{
			get{ return !string.IsNullOrEmpty (RootPath) && !string.IsNullOrEmpty (iOSAdhocFileName) && File.Exists (iOSAdhocFile); }
		}

		public FileStream GetiOSAdhocStream()
		{
			return File.OpenRead (iOSAdhocFile);
		}

		public string iOSAppStoreFile {
			get{ return Path.Combine (RootPath, iOSAppStoreFileName); }
		}

		public bool iOSAppStoreFileExist
		{
			get{ return !string.IsNullOrEmpty (RootPath) && !string.IsNullOrEmpty (iOSAppStoreFileName) && File.Exists (iOSAppStoreFile); }
		}

		public FileStream GetiOSAppStoreStream()
		{
			return File.OpenRead (iOSAppStoreFile);
		}

		public string AndroidApkFile {
			get{ return Path.Combine (RootPath, AndroidApkFileName); }
		}

		public bool AndroidApkFileExist
		{
			get{ return !string.IsNullOrEmpty (RootPath) && !string.IsNullOrEmpty (AndroidApkFileName) && File.Exists (AndroidApkFile); }
		}

		public FileStream GetAndroidApkStream()
		{
			return File.OpenRead (AndroidApkFile);
		}


	}
}

