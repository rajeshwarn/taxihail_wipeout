// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 2.0.50727.1433
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace TaxiMobile.Locations {
	
	
	// Base type probably should be MonoTouch.UIKit.UIViewController or subclass
	[MonoTouch.Foundation.Register("LocationsTabView")]
	public partial class LocationsTabView {
		
		private MonoTouch.UIKit.UIView __mt_view;
		
		private MonoTouch.UIKit.UITableView __mt_tableLocations;
		
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
		
		[MonoTouch.Foundation.Connect("tableLocations")]
		private MonoTouch.UIKit.UITableView tableLocations {
			get {
				this.__mt_tableLocations = ((MonoTouch.UIKit.UITableView)(this.GetNativeField("tableLocations")));
				return this.__mt_tableLocations;
			}
			set {
				this.__mt_tableLocations = value;
				this.SetNativeField("tableLocations", value);
			}
		}
	}
}
