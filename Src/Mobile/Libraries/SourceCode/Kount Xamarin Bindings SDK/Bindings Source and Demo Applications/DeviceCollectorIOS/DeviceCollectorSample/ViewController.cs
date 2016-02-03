using System;
using UIKit;
using Braintree;

namespace DeviceCollectorSample
{
    public partial class ViewController : UIViewController
	{
		// Default Constructor
		public ViewController (IntPtr handle) : base (handle) {}

		// Below are the test URL and Merchant ID provided for CMT
		const string deviceCollectorURL = "https://tst.kaptcha.com/logo.htm";
		const string merchantId = "160700";

		// Create a new deviceCollector with debug logging enabled
		DeviceCollectorSDK deviceCollector;

		protected void WriteLine(String text)
		{
			InvokeOnMainThread (() => { outputWindow.Text = outputWindow.Text + ("\n" + DateTime.Now.ToLongTimeString() + ": " + text); scrollOutputWindowToBottom(); });
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			// End editing when clicking outside the text fields
			var g = new UITapGestureRecognizer(() => View.EndEditing(true));
			View.AddGestureRecognizer(g);

			// this session ID should be unique, and used for both the RIS call and the collector call, this likely is created on the front end and passed to the backend
			// so RIS can later use it on the backend. But this is a backend design decision. Right now I'm using a unique GUID
			String generatedGuid = Guid.NewGuid().ToString("N");
			WriteLine("Created guid to use as session ID : " + generatedGuid);

			// Set a default collection URL
			collectURL.Text = deviceCollectorURL;

			// Set a default merchant ID
			merc_id.Text = merchantId;

			// get a handle to the session id and create a default
			session_id.Text = generatedGuid;
		}

		partial void SendDeviceInformation_Click (UIButton sender)
		{
			int i = 0;
			if (collectURL.Text != String.Empty && merc_id.Text != String.Empty && Int32.TryParse(merc_id.Text, out i) && session_id.Text != String.Empty)
			{
				sendDeviceInformation.Enabled= false;
				generateSessionButton.Enabled = false;

				WriteLine ("Created New DeviceCollector");
				deviceCollector = new DeviceCollectorSDK(true);

//				WriteLine ("Setting listeners to this activity");
//				deviceCollector.SetDelegate(this);

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

		partial void GenerateSessionButton_Click (UIButton sender)
		{
			String generatedGuid = Guid.NewGuid ().ToString ("N");
			WriteLine("Created guid to use as session ID : " + generatedGuid);
			session_id.Text = generatedGuid;
		}

		public void ProcessNSError (Foundation.NSError error)
		{
			if (error != null) 
			{
				WriteLine ("NSError : domain = " + error.Domain + " code = " + error.Code.ToString());
			}
		}

		public void scrollOutputWindowToBottom()
		{
			if(outputWindow.Text.Length > 0) 
			{
				Foundation.NSRange bottom = new Foundation.NSRange (outputWindow.Text.Length - 1, 1);
				outputWindow.ScrollRangeToVisible(bottom);
			}
		}
	}
}

