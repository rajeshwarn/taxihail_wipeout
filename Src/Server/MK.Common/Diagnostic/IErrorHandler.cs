using System;

namespace apcurium.MK.Common.Diagnostic
{
	public interface IErrorHandler
	{
		bool HandleError( Exception ex );
	}
}

