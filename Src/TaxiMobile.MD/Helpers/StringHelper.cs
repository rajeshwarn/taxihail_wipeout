using System;
using System.Globalization;
using System.Text;

namespace TaxiMobile.Helpers
{
	public class StringHelper
	{
		public StringHelper ()
		{
		}
		
		public static String RemoveDiacritics ( String s )
		{
			String normalizedString = s.Normalize ( NormalizationForm.FormD );
			StringBuilder stringBuilder = new StringBuilder (  );
			
			for ( int i = 0; i < normalizedString.Length; i++ )
			{
				Char c = normalizedString[i];
				if ( CharUnicodeInfo.GetUnicodeCategory ( c ) != UnicodeCategory.NonSpacingMark )
					stringBuilder.Append ( c );
			}
			
			return stringBuilder.ToString (  );
		}
	}
}

