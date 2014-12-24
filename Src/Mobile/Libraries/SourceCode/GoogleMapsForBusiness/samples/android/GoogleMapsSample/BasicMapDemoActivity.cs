using System;
using Android.App;
using Android.Support.V4.App;
using Java.Interop;
using Google.Android.M4b.Maps;
using Google.Android.M4b.Maps.Model;

namespace GoogleMapsSample
{
	/**
	 * This shows how to create a simple activity with a map and a marker on the map.
	 * <p>
	 * Notice how we deal with the possibility that the Google Play services APK is not
	 * installed/enabled/updated on a user's device.
	 */
	[Activity (Label = "@string/basic_map_demo_label")]
	public class BasicMapDemoActivity : FragmentActivity
	{
		/**
	     * Note that this may be null if the Google Play services APK is not available.
	     */
		private GoogleMap mMap;

		protected override void OnCreate (Android.OS.Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			SetContentView (Resource.Layout.basic_demo);
			SetUpMapIfNeeded ();
		}

		protected override void OnResume ()
		{
			base.OnResume ();
			SetUpMapIfNeeded ();
		}

		/**
	     * Sets up the map if it is possible to do so (i.e., the Google Play services APK is correctly
	     * installed) and the map has not already been instantiated.. This will ensure that we only ever
	     * call {@link #setUpMap()} once when {@link #mMap} is not null.
	     * <p>
	     * If it isn't installed {@link SupportMapFragment} (and
	     * {@link com.google.android.m4b.maps.MapView MapView}) will show a prompt for the user to
	     * install/update the Google Play services APK on their device.
	     * <p>
	     * A user can return to this FragmentActivity after following the prompt and correctly
	     * installing/updating/enabling the Google Play services. Since the FragmentActivity may not
	     * have been completely destroyed during this process (it is likely that it would only be
	     * stopped or paused), {@link #onCreate(Bundle)} may not be called again so we should call this
	     * method in {@link #onResume()} to guarantee that it will be called.
	     */
		private void SetUpMapIfNeeded ()
		{
			// Do a null check to confirm that we have not already instantiated the map.
			if (mMap == null) {
				// Try to obtain the map from the SupportMapFragment.
				mMap = SupportFragmentManager.FindFragmentById (Resource.Id.map).JavaCast <SupportMapFragment> ().Map;
				// Check if we were successful in obtaining the map.
				if (mMap != null) {
					SetUpMap ();
				}
			}
		}

		/**
	     * This is where we can add markers or lines, add listeners or move the camera. In this case, we
	     * just add a marker near Africa.
	     * <p>
	     * This should only be called once and when we are sure that {@link #mMap} is not null.
	     */
		private void SetUpMap ()
		{
			mMap.AddMarker (new MarkerOptions ().SetPosition (new LatLng (0, 0)).SetTitle ("Marker"));
		}
	}
}

