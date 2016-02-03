using System;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Text.Method;
using Braintree.Device_Collector;

namespace DeviceCollectorSample
{
	[Activity (Label = "DeviceCollectorSample", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity, Braintree.Device_Collector.DeviceCollector.IStatusListener
	{
		// Below are the test URL and Merchant ID provided for CMT
		const string deviceCollectorURL = "https://tst.kaptcha.com/logo.htm";
		const string merchantId = "160700";

		// Create a new deviceCollector
		DeviceCollector deviceCollector;

		// Android control members
		TextView merc_id;
		TextView collectURL;
		TextView session_id;
		TextView outputWindow;
		Button generateSessionButton;
		Button sendDeviceInformation;

		protected void WriteLine(String text)
		{
			RunOnUiThread (() => outputWindow.Append ("\n" + DateTime.Now.ToLongTimeString() + ": " + text));
		}

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			this.ActionBar.Hide();

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// get a handle to out output window for logging
			outputWindow = FindViewById<TextView> (Resource.Id.outputtext);
			outputWindow.MovementMethod = new ScrollingMovementMethod ();

			// this session ID should be unique, and used for both the RIS call and the collector call, this likely is created on the front end and passed to the backend
			// so RIS can later use it on the backend. But this is a backend design decision. Right now I'm using a unique GUID
			String generatedGuid = Guid.NewGuid ().ToString ("N");
			WriteLine("Created guid to use as session ID : " + generatedGuid);

			// Set a default collection URL
			collectURL = FindViewById<TextView> (Resource.Id.collectvalue);
			collectURL.Text = deviceCollectorURL;

			// Set a default merchant ID
			merc_id = FindViewById<TextView> (Resource.Id.merchidvalue);
			merc_id.Text = merchantId;

			// get a handle to the session id and create a default
			session_id = FindViewById<TextView> (Resource.Id.guidtext);
			session_id.Text = generatedGuid;

			// Creating a button reference and attaching our delegate listener to it so we can start collecting.
			sendDeviceInformation = FindViewById<Button> (Resource.Id.sendDataButton);
			sendDeviceInformation.Click += SendDeviceInformation_Click;

			generateSessionButton = FindViewById<Button> (Resource.Id.refreshguid);
			generateSessionButton.Click += GenerateSessionButton_Click;
		}

		void SendDeviceInformation_Click (object sender, EventArgs e)
		{
			int i = 0;
			if (collectURL.Text != String.Empty && merc_id.Text != String.Empty && Int32.TryParse(merc_id.Text, out i) && session_id.Text != String.Empty)
			{
				sendDeviceInformation.Enabled= false;
				generateSessionButton.Enabled = false;

				WriteLine ("Created New DeviceCollector");
				deviceCollector = new DeviceCollector(this);

				WriteLine ("Setting listeners to this activity");
				deviceCollector.SetStatusListener(this);

				WriteLine("Setting Collector URL : " + collectURL.Text);
				deviceCollector.SetCollectorUrl (collectURL.Text);

				WriteLine("Setting Merchant ID : " + merc_id.Text);
				deviceCollector.SetMerchantId (merc_id.Text);

				WriteLine("Called DeviceCollector for Session : " + session_id.Text);
				deviceCollector.Collect(session_id.Text);
			}
			else
			{
				WriteLine("ERROR : Either the collect URL, merchant ID or Session ID are not set correctly");
			}
		}

		void GenerateSessionButton_Click (object sender, EventArgs e)
		{
			String generatedGuid = Guid.NewGuid ().ToString ("N");
			WriteLine("Created guid to use as session ID : " + generatedGuid);
			session_id.Text = generatedGuid;
		}
			
		public void OnCollectorError (DeviceCollector.ErrorCode errorCode, Java.Lang.Exception javaException)
		{
			String errorReason = String.Empty;
			String errorCodeName = errorCode.Name();
			WriteLine (errorCodeName + " - " + errorReason);
			ProcessJavaException (javaException);
			RunOnUiThread (() => sendDeviceInformation.Enabled = true);
			RunOnUiThread (() => generateSessionButton.Enabled = true);
		}

		public void OnCollectorStart ()
		{
			WriteLine ("Device Collector Process Started");
		}

		public void OnCollectorSuccess ()
		{
			RunOnUiThread (() => sendDeviceInformation.Enabled= true);
			RunOnUiThread (() => generateSessionButton.Enabled = true);
			WriteLine ("Collector Success");
		}

		public void ProcessJavaException (Java.Lang.Exception ex)
		{
			if (ex != null) 
			{
				foreach (Java.Lang.StackTraceElement element in ex.GetStackTrace()) 
				{
					WriteLine(element.ClassName + " " + element.MethodName + "(" + element.LineNumber + ")");
				} 
			}
		}

	}
}


