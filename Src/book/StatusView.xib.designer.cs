// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 2.0.50727.1433
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace TaxiMobileApp {
	
	
	// Base type probably should be MonoTouch.UIKit.UIViewController or subclass
	[MonoTouch.Foundation.Register("StatusView")]
	public partial class StatusView {
		
		private MonoTouch.UIKit.UIView __mt_view;
		
		private MonoTouch.UIKit.UIButton __mt_btnCall;
		
		private MonoTouch.UIKit.UIButton __mt_btnChangeBooking;
		
		private MonoTouch.UIKit.UILabel __mt_lblTitle;
		
		private MonoTouch.MapKit.MKMapView __mt_mapStatus;
		
		private MonoTouch.UIKit.UIButton __mt_btnRefresh;
		
		private MonoTouch.UIKit.UILabel __mt_lblStatus;
		
		#pragma warning disable 0169
		[MonoTouch.Foundation.Connect("view")]
		private MonoTouch.UIKit.UIView view {
			get {
				this.__mt_view = ((MonoTouch.UIKit.UIView)(this.GetNativeField("view")));
				return this.__mt_view;
			}
			set {
				this.__mt_view = value;
				this.SetNativeField("view", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("btnCall")]
		private MonoTouch.UIKit.UIButton btnCall {
			get {
				this.__mt_btnCall = ((MonoTouch.UIKit.UIButton)(this.GetNativeField("btnCall")));
				return this.__mt_btnCall;
			}
			set {
				this.__mt_btnCall = value;
				this.SetNativeField("btnCall", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("btnChangeBooking")]
		private MonoTouch.UIKit.UIButton btnChangeBooking {
			get {
				this.__mt_btnChangeBooking = ((MonoTouch.UIKit.UIButton)(this.GetNativeField("btnChangeBooking")));
				return this.__mt_btnChangeBooking;
			}
			set {
				this.__mt_btnChangeBooking = value;
				this.SetNativeField("btnChangeBooking", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("lblTitle")]
		private MonoTouch.UIKit.UILabel lblTitle {
			get {
				this.__mt_lblTitle = ((MonoTouch.UIKit.UILabel)(this.GetNativeField("lblTitle")));
				return this.__mt_lblTitle;
			}
			set {
				this.__mt_lblTitle = value;
				this.SetNativeField("lblTitle", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("mapStatus")]
		private MonoTouch.MapKit.MKMapView mapStatus {
			get {
				this.__mt_mapStatus = ((MonoTouch.MapKit.MKMapView)(this.GetNativeField("mapStatus")));
				return this.__mt_mapStatus;
			}
			set {
				this.__mt_mapStatus = value;
				this.SetNativeField("mapStatus", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("btnRefresh")]
		private MonoTouch.UIKit.UIButton btnRefresh {
			get {
				this.__mt_btnRefresh = ((MonoTouch.UIKit.UIButton)(this.GetNativeField("btnRefresh")));
				return this.__mt_btnRefresh;
			}
			set {
				this.__mt_btnRefresh = value;
				this.SetNativeField("btnRefresh", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("lblStatus")]
		private MonoTouch.UIKit.UILabel lblStatus {
			get {
				this.__mt_lblStatus = ((MonoTouch.UIKit.UILabel)(this.GetNativeField("lblStatus")));
				return this.__mt_lblStatus;
			}
			set {
				this.__mt_lblStatus = value;
				this.SetNativeField("lblStatus", value);
			}
		}
	}
}
