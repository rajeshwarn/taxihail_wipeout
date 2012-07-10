using System;

namespace TaxiMobile.Lib.Infrastructure
{
	public interface ILogger
	{
		void LogError( Exception ex );
		
		void LogMessage( string message );
		
		void StartStopwatch( string message );
		
		void StopStopwatch( string message );
	}
}

