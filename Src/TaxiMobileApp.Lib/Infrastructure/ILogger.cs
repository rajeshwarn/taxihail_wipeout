using System;

namespace TaxiMobileApp
{
	public interface ILogger
	{
		void LogError( Exception ex );
		
		void LogMessage( string message );
		
		void StartStopwatch( string message );
		
		void StopStopwatch( string message );
	}
}

