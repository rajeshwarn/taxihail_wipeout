using System;
using System.Collections.Generic;

namespace TaxiMobileApp.Lib.GoogleServices
{
	public class GooglePlaceResponseData<T> where T : new()
	{
		public List<string> html_attributions { get; set; }
		public string next_page_token { get; set; }
		public T results { get; set; }
		public string status { get; set; }
	}
}

