using System;
using Android.Widget;
using Android.Content;
using Android.Views;

namespace GoogleMapsSample
{
	/**
	 * A widget that describes an activity that demonstrates a feature.
	 */
	public class FeatureView : FrameLayout
	{
		/**
	     * Constructs a feature view by inflating layout/feature.xml.
	     */
		public FeatureView (Context context) : base (context)
		{

			LayoutInflater layoutInflater =
				(LayoutInflater)context.GetSystemService (Context.LayoutInflaterService);
			layoutInflater.Inflate (Resource.Layout.feature, this);
		}

		object synchronizedLock = new object ();

		/**
	     * Set the resource id of the title of the demo.
	     *
	     * @param titleId the resource id of the title of the demo
	     */
		public void SetTitleId (int titleId)
		{
			lock (synchronizedLock) {
				(FindViewById <TextView> (Resource.Id.title)).SetText (titleId);
			}
		}

		/**
	     * Set the resource id of the description of the demo.
	     *
	     * @param descriptionId the resource id of the description of the demo
	     */
		public void SetDescriptionId (int descriptionId)
		{
			lock (synchronizedLock) {
				(FindViewById <TextView> (Resource.Id.description)).SetText (descriptionId);
			}
		}
	}
}

