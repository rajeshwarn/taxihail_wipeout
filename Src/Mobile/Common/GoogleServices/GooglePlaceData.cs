using System;
using System.Collections.Generic;

namespace TaxiMobileApp.Lib.GoogleServices
{
	public class GooglePlaceData
	{
		public Geometry geometry { get; set; }
		public string icon { get; set; }
		public string id { get; set; }
		public string name { get; set; }
		public float rating { get; set; }
		public string reference { get; set; }
		public List<string> types { get; set; }
		public string vicinity { get; set; }
		public List<Event> events { get; set; }
	}

	public class Geometry
	{
		public Location location { get; set; }
	}

	public class Location
	{
		public double lat { get; set; }
		public double lng { get; set; }
	}

	public class Event
	{
		public string event_id { get; set; }
		public string summary { get; set; }
		public string url { get; set; }
	}
}

