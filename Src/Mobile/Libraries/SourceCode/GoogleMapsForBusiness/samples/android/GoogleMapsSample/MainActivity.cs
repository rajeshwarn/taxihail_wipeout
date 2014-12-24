using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace GoogleMapsSample
{
	[Activity (MainLauncher = true)]
	public class MainActivity : ListActivity
	{
		/**
	     * A custom array adapter that shows a {@link FeatureView} containing details about the demo.
	     */
		private class CustomArrayAdapter : BaseAdapter<DemoDetails>
		{
			Context context;
			DemoDetails[] demos;

			/**
	         * @param demos An array containing the details of the demos to be displayed.
	         */
			public CustomArrayAdapter (Context context, DemoDetails[] demos)
			{
				this.context = context;
				this.demos = demos;
			}

			override public View GetView (int position, View convertView, ViewGroup parent)
			{
				FeatureView featureView;
				if (convertView is FeatureView) {
					featureView = (FeatureView)convertView;
				} else {
					featureView = new FeatureView (context);
				}

				DemoDetails demo = this [position];

				featureView.SetTitleId (demo.TitleId);
				featureView.SetDescriptionId (demo.DescriptionId);

				return featureView;
			}

			public override long GetItemId (int position)
			{
				return position;
			}

			public override int Count {
				get {
					return demos.Length;
				}
			}

			public override DemoDetails this [int index] {
				get {
					return demos [index];
				}
			}
		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			var adapter = new CustomArrayAdapter (this, DemoDetailsList.Demos);

			ListAdapter = adapter;
		}

		override public bool OnCreateOptionsMenu (IMenu menu)
		{
			// Inflate the menu; this adds items to the action bar if it is present.
			MenuInflater.Inflate (Resource.Menu.activity_main, menu);
			return true;
		}

		override public bool OnOptionsItemSelected (IMenuItem item)
		{
			// Handle item selection
			if (item.ItemId == Resource.Id.menu_legal) {
				StartActivity (new Intent (this, typeof(LegalInfoActivity)));
				return true;
			}
			return OnOptionsItemSelected (item);
		}

		override protected void OnListItemClick (ListView l, View v, int position, long id)
		{
			DemoDetails demo = DemoDetailsList.Demos [position];
			StartActivity (new Intent (this, demo.ActivityClass));
		}
	}
}


