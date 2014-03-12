using System;

namespace apcurium.MK.Common.Diagnostic
{
	public interface IErrorHandler
	{
		void HandleError( Exception ex );
	}
}

