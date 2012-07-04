using System;

namespace MoveOn.Common.Extensions
{
	public static class GuidExtensions
	{

		public static bool IsNullOrEmpty( this Guid instance )
		{
			return instance == Guid.Empty ;
		}
		
		public static bool IsNullOrEmpty( this Guid? instance )
		{
			return instance == Guid.Empty || !instance.HasValue;
		}		
	}
}

