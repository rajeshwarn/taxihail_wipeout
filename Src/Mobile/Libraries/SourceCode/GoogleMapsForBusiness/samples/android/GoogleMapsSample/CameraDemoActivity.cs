using System;
using Android.Support.V4.App;
using Android.Widget;
using Java.Interop;
using Android.Views;
using Google.Android.M4b.Maps;
using Google.Android.M4b.Maps.Model;
using Android.App;

namespace GoogleMapsSample
{
	/**
	 * This shows how to change the camera position for the map.
	 */
	[Activity(Label="@string/camera_demo_label")]
	public class CameraDemoActivity : FragmentActivity
	{
		/**
	     * The amount by which to scroll the camera. Note that this amount is in raw pixels, not dp
	     * (density-independent pixels).
	     */
		private static int SCROLL_BY_PX = 100;

		public static CameraPosition BONDI =
			new CameraPosition.Builder ().Target (new LatLng (-33.891614, 151.276417))
				.Zoom (15.5f)
				.Bearing (300)
				.Tilt (50)
				.Build ();

		public static CameraPosition SYDNEY =
			new CameraPosition.Builder ().Target (new LatLng (-33.87365, 151.20689))
				.Zoom (15.5f)
				.Bearing (0)
				.Tilt (25)
				.Build ();

		GoogleMap mMap;

		CompoundButton mAnimateToggle;
		CompoundButton mCustomDurationToggle;
		SeekBar mCustomDurationBar;

		protected override void OnCreate (Android.OS.Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			SetContentView (Resource.Layout.camera_demo);

			mAnimateToggle = FindViewById<CompoundButton> (Resource.Id.animate);
			mCustomDurationToggle = FindViewById<CompoundButton> (Resource.Id.duration_toggle);
			mCustomDurationBar = FindViewById<SeekBar> (Resource.Id.duration_bar);

			UpdateEnabledState ();

			SetUpMapIfNeeded ();
		}

		protected override void OnResume ()
		{
			base.OnResume ();

			UpdateEnabledState ();
			SetUpMapIfNeeded ();
		}

		void SetUpMapIfNeeded ()
		{
			if (mMap == null) {
				mMap = SupportFragmentManager.FindFragmentById (Resource.Id.map).JavaCast <SupportMapFragment> ().Map;
				if (mMap != null) {
					SetUpMap ();
				}
			}
		}

		void SetUpMap ()
		{
			// We will provide our own zoom controls.
			mMap.UiSettings.ZoomControlsEnabled = false;

			// Show Sydney
			mMap.MoveCamera (CameraUpdateFactory.NewLatLngZoom (new LatLng (-33.87365, 151.20689), 10));
		}

		/**
	     * When the map is not ready the CameraUpdateFactory cannot be used. This should be called on
	     * all entry points that call methods on the Google Maps API.
	     */
		bool CheckReady ()
		{
			if (mMap == null) {
				Toast.MakeText (this, Resource.String.map_not_ready, ToastLength.Long).Show ();
				return false;
			}
			return true;
		}

		/**
	     * Called when the Go To Bondi button is clicked.
	     */
		[Export("onGoToBondi")]
		public void OnGoToBondi (View view)
		{
			if (!CheckReady ()) {
				return;
			}

			ChangeCamera (CameraUpdateFactory.NewCameraPosition (BONDI));
		}

		class GoToSydneyCancelableCallback : Java.Lang.Object, GoogleMap.ICancelableCallback
		{
			CameraDemoActivity outter;

			public GoToSydneyCancelableCallback (CameraDemoActivity activity)
			{
				outter = activity;
			}

			public void OnFinish ()
			{
				Toast.MakeText (outter.BaseContext, "Animation to Sydney complete", ToastLength.Short)
					.Show ();
			}

			public void OnCancel ()
			{
				Toast.MakeText (outter.BaseContext, "Animation to Sydney canceled", ToastLength.Short)
					.Show ();
			}
		}

		/**
	     * Called when the Animate To Sydney button is clicked.
	     */
		[Export("onGoToSydney")]
		public void OnGoToSydney (View view)
		{
			if (!CheckReady ()) {
				return;
			}

			ChangeCamera (CameraUpdateFactory.NewCameraPosition (SYDNEY), new GoToSydneyCancelableCallback (this));
		}

		/**
	     * Called when the stop button is clicked.
	     */
		[Export ("onStopAnimation")]
		public void OnStopAnimation (View view)
		{
			if (!CheckReady ()) {
				return;
			}

			mMap.StopAnimation ();
		}

		/**
	     * Called when the zoom in button (the one with the +) is clicked.
	     */
		[Export ("onZoomIn")]
		public void OnZoomIn (View view)
		{
			if (!CheckReady ()) {
				return;
			}

			ChangeCamera (CameraUpdateFactory.ZoomIn ());
		}

		/**
	     * Called when the zoom out button (the one with the -) is clicked.
	     */
		[Export("onZoomOut")]
		public void OnZoomOut (View view)
		{
			if (!CheckReady ()) {
				return;
			}

			ChangeCamera (CameraUpdateFactory.ZoomOut ());
		}

		/**
	     * Called when the tilt more button (the one with the /) is clicked.
	     */
		[Export("onTiltMore")]
		public void OnTiltMore (View view)
		{
			CameraPosition currentCameraPosition = mMap.CameraPosition;
			float currentTilt = currentCameraPosition.Tilt;
			float newTilt = currentTilt + 10;

			newTilt = (newTilt > 90) ? 90 : newTilt;

			CameraPosition cameraPosition = new CameraPosition.Builder (currentCameraPosition)
				.Tilt (newTilt).Build ();

			ChangeCamera (CameraUpdateFactory.NewCameraPosition (cameraPosition));
		}

		/**
	     * Called when the tilt less button (the one with the \) is clicked.
	     */
		[Export ("onTiltLess")]
		public void OnTiltLess (View view)
		{
			CameraPosition currentCameraPosition = mMap.CameraPosition;

			float currentTilt = currentCameraPosition.Tilt;

			float newTilt = currentTilt - 10;
			newTilt = (newTilt > 0) ? newTilt : 0;

			CameraPosition cameraPosition = new CameraPosition.Builder (currentCameraPosition)
				.Tilt (newTilt).Build ();

			ChangeCamera (CameraUpdateFactory.NewCameraPosition (cameraPosition));
		}

		/**
	     * Called when the left arrow button is clicked. This causes the camera to move to the left
	     */
		[Export("onScrollLeft")]
		public void OnScrollLeft (View view)
		{
			if (!CheckReady ()) {
				return;
			}

			ChangeCamera (CameraUpdateFactory.ScrollBy (-SCROLL_BY_PX, 0));
		}

		/**
	     * Called when the right arrow button is clicked. This causes the camera to move to the right.
	     */
		[Export("onScrollRight")]
		public void OnScrollRight (View view)
		{
			if (!CheckReady ()) {
				return;
			}

			ChangeCamera (CameraUpdateFactory.ScrollBy (SCROLL_BY_PX, 0));
		}

		/**
	     * Called when the up arrow button is clicked. The causes the camera to move up.
	     */
		[Export ("onScrollUp")]
		public void OnScrollUp (View view)
		{
			if (!CheckReady ()) {
				return;
			}

			ChangeCamera (CameraUpdateFactory.ScrollBy (0, -SCROLL_BY_PX));
		}

		/**
	     * Called when the down arrow button is clicked. This causes the camera to move down.
	     */
		[Export("onScrollDown")]
		public void OnScrollDown (View view)
		{
			if (!CheckReady ()) {
				return;
			}

			ChangeCamera (CameraUpdateFactory.ScrollBy (0, SCROLL_BY_PX));
		}

		/**
	     * Called when the animate button is toggled
	     */
		[Export ("onToggleAnimate")]
		public void OnToggleAnimate (View view)
		{
			UpdateEnabledState ();
		}

		/**
	     * Called when the custom duration checkbox is toggled
	     */
		[Export("onToggleCustomDuration")]
		public void OnToggleCustomDuration (View view)
		{
			UpdateEnabledState ();
		}

		/**
	     * Update the enabled state of the custom duration controls.
	     */
		private void UpdateEnabledState ()
		{
			mCustomDurationToggle.Enabled = mAnimateToggle.Checked;
			mCustomDurationBar
				.Enabled = mAnimateToggle.Checked && mCustomDurationToggle.Checked;
		}

		private void ChangeCamera (CameraUpdate update)
		{
			ChangeCamera (update, null);
		}

		/**
	     * Change the camera position by moving or animating the camera depending on the state of the
	     * animate toggle button.
	     */
		private void ChangeCamera (CameraUpdate update, GoogleMap.ICancelableCallback callback)
		{
			if (mAnimateToggle.Checked) {
				if (mCustomDurationToggle.Checked) {
					int duration = mCustomDurationBar.Progress;
					// The duration must be strictly positive so we make it at least 1.
					mMap.AnimateCamera (update, Math.Max (duration, 1), callback);
				} else {
					mMap.AnimateCamera (update, callback);
				}
			} else {
				mMap.MoveCamera (update);
			}
		}

	}
}

