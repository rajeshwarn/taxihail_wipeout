using System;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
	/// <summary>
	/// This service provide methods with native OS code to exit the application.
	/// </summary>
	public class QuitApplicationService : IQuitApplicationService
	{
		private readonly ILogger _logger;

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="apcurium.MK.Booking.Mobile.Client.PlatformIntegration.QuitApplicationService"/> class.
		/// </summary>
		public QuitApplicationService (ILogger logger)
		{
			_logger = logger;
		}

		/// <summary>
		/// Quit application.
		/// </summary>
		public void Quit()
		{
			_logger.LogMessage ("Application exited because of version is obsolete");
			Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
		}
	}
}


