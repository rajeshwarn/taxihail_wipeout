// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace DeviceCollectorSample
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField collectURL { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton generateSessionButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIImageView logo { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIView mainUIView { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField merc_id { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextView outputWindow { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton sendDeviceInformation { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UITextField session_id { get; set; }

		[Action ("sendDeviceInfo_Click:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void SendDeviceInformation_Click (UIButton sender);

		[Action ("sessionId_Click:")]
		[GeneratedCode ("iOS Designer", "1.0")]
		partial void GenerateSessionButton_Click (UIButton sender);

		void ReleaseDesignerOutlets ()
		{
			if (collectURL != null) {
				collectURL.Dispose ();
				collectURL = null;
			}
			if (generateSessionButton != null) {
				generateSessionButton.Dispose ();
				generateSessionButton = null;
			}
			if (logo != null) {
				logo.Dispose ();
				logo = null;
			}
			if (mainUIView != null) {
				mainUIView.Dispose ();
				mainUIView = null;
			}
			if (merc_id != null) {
				merc_id.Dispose ();
				merc_id = null;
			}
			if (outputWindow != null) {
				outputWindow.Dispose ();
				outputWindow = null;
			}
			if (sendDeviceInformation != null) {
				sendDeviceInformation.Dispose ();
				sendDeviceInformation = null;
			}
			if (session_id != null) {
				session_id.Dispose ();
				session_id = null;
			}
		}
	}
}
