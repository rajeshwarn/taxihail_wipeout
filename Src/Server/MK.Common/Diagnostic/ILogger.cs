using System;

namespace apcurium.MK.Common.Diagnostic
{
	public interface ILogger
	{

		void LogError( Exception ex );
		
		void LogMessage( string message );
		
		void LogStack( );

        void StartStopwatch( string message );

        void StopStopwatch(string message);
        

            
	}
}

