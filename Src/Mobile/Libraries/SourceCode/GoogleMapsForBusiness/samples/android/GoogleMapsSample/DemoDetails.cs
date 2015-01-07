using System;
using Android.Support.V4.App;

namespace GoogleMapsSample
{
	/**
	 * A simple POJO that holds the details about the demo that are used by the List Adapter.
	 */
	public class DemoDetails
	{
		/**
	     * The resource id of the title of the demo.
	     */
		public int TitleId { get; set; }

		/**
	     * The resources id of the description of the demo.
	     */
		public int DescriptionId { get; set; }

		/**
	     * The demo activity's class.
	     */
		public Type ActivityClass { get; set; }

		public DemoDetails (
			int titleId, int descriptionId, Type activityClass)
		{
			TitleId = titleId;
			DescriptionId = descriptionId;
			ActivityClass = activityClass;
		}
	}
}

