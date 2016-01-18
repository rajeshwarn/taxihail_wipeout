using System;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace DeviceCollectorBindingsIOS
{
	// @interface DeviceCollectorSDKDelegate
	[BaseType (typeof (NSObject))]
	[Model, Protocol]
	interface DeviceCollectorSDKDelegate
	{
		// -(void)onCollectorStart;
		[Export ("onCollectorStart")]
		void OnCollectorStart ();

		// -(void)onCollectorSuccess;
		[Export ("onCollectorSuccess")]
		void OnCollectorSuccess ();

		// -(void)onCollectorError:(int)errorCode withError:(NSError *)error;
		[Export ("onCollectorError:withError:")]
		void OnCollectorError (int errorCode, NSError error);
	}

	interface IDeviceCollectorSDKDelegate {}

	// @interface DeviceCollectorSDK : NSObject <UIWebViewDelegate>
	[BaseType (typeof(NSObject))]
	interface DeviceCollectorSDK : IUIWebViewDelegate
	{
		// @property (nonatomic, strong) NSArray * skipList;
		[Export ("skipList", ArgumentSemantic.Strong)]
		NSObject[] SkipList { get; set; }

		// -(DeviceCollectorSDK *)initWithDebugOn:(_Bool)debugLogging;
		[Export ("initWithDebugOn:")]
		IntPtr Constructor (bool debugLogging);

		// -(void)setCollectorUrl:(NSString *)url;
		[Export ("setCollectorUrl:")]
		void SetCollectorUrl (String url);

		// -(void)setMerchantId:(NSString *)merc;
		[Export ("setMerchantId:")]
		void SetMerchantId (String merc);

		// -(void)collect:(NSString *)sessionId;
		[Export ("collect:")]
		void Collect (String sessionId);

		// -(void)setDelegate:(id<DeviceCollectorSDKDelegate>)delegate;
		[Export ("setDelegate:")]
		void SetDelegate (IDeviceCollectorSDKDelegate @delegate);

		[Export ("getProtocol")]
		IDeviceCollectorSDKDelegate GetProtocol ();
	}
}
