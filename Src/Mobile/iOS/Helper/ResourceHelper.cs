using System;
using System.IO;
using System.Reflection;
namespace TaxiMobileApp
{
	public class RessourceHelper
	{
		public RessourceHelper ()
		{
		}

		public static string GetFromResources ( string resourceName )
		{
			Assembly assem = typeof(RessourceHelper).Assembly;
			using ( Stream stream = assem.GetManifestResourceStream ( resourceName ) )
			{
				try
				{
					using ( StreamReader reader = new StreamReader ( stream ) )
					{
						return reader.ReadToEnd (  );
					}
				}
				catch ( Exception e )
				{
					throw new Exception ( "Error retrieving from Resources. Tried '" + resourceName + "'\r\n" + e.ToString (  ) );
				}
			}
		}
	}
}

