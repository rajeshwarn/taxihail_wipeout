// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 2.0.50727.1433
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace TaxiMobile.History {
	
	
	// Base type probably should be MonoTouch.UIKit.UIViewController or subclass
	[MonoTouch.Foundation.Register("HistoryDetailView")]
	public partial class HistoryDetailView {
		
		private MonoTouch.UIKit.UIView __mt_view;
		
		private MonoTouch.UIKit.UIButton __mt_btnHide;
		
		private MonoTouch.UIKit.UIButton __mt_btnRebook;
		
		private MonoTouch.UIKit.UILabel __mt_lblConfirmationNo;
		
		private MonoTouch.UIKit.UILabel __mt_lblDestination;
		
		private MonoTouch.UIKit.UILabel __mt_lblOrigin;
		
		private MonoTouch.UIKit.UILabel __mt_lblRequested;
		
		private MonoTouch.UIKit.UILabel __mt_txtConfirmationNo;
		
		private MonoTouch.UIKit.UILabel __mt_txtDestination;
		
		private MonoTouch.UIKit.UILabel __mt_txtOrigin;
		
		private MonoTouch.UIKit.UILabel __mt_txtRequested;
		
		private MonoTouch.UIKit.UILabel __mt_txtAptCode;
		
		private MonoTouch.UIKit.UILabel __mt_lblStatus;
		
		private MonoTouch.UIKit.UILabel __mt_txtStatus;
		
		private MonoTouch.UIKit.UILabel __mt_lblAptRingCode;
		
		private MonoTouch.UIKit.UILabel __mt_lblPickupDate;
		
		private MonoTouch.UIKit.UILabel __mt_txtPickupDate;
		
		private MonoTouch.UIKit.UIButton __mt_btnStatus;
		
		private MonoTouch.UIKit.UIButton __mt_btnCancel;
		
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
		
		[MonoTouch.Foundation.Connect("btnHide")]
		private MonoTouch.UIKit.UIButton btnHide {
			get {
				this.__mt_btnHide = ((MonoTouch.UIKit.UIButton)(this.GetNativeField("btnHide")));
				return this.__mt_btnHide;
			}
			set {
				this.__mt_btnHide = value;
				this.SetNativeField("btnHide", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("btnRebook")]
		private MonoTouch.UIKit.UIButton btnRebook {
			get {
				this.__mt_btnRebook = ((MonoTouch.UIKit.UIButton)(this.GetNativeField("btnRebook")));
				return this.__mt_btnRebook;
			}
			set {
				this.__mt_btnRebook = value;
				this.SetNativeField("btnRebook", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("lblConfirmationNo")]
		private MonoTouch.UIKit.UILabel lblConfirmationNo {
			get {
				this.__mt_lblConfirmationNo = ((MonoTouch.UIKit.UILabel)(this.GetNativeField("lblConfirmationNo")));
				return this.__mt_lblConfirmationNo;
			}
			set {
				this.__mt_lblConfirmationNo = value;
				this.SetNativeField("lblConfirmationNo", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("lblDestination")]
		private MonoTouch.UIKit.UILabel lblDestination {
			get {
				this.__mt_lblDestination = ((MonoTouch.UIKit.UILabel)(this.GetNativeField("lblDestination")));
				return this.__mt_lblDestination;
			}
			set {
				this.__mt_lblDestination = value;
				this.SetNativeField("lblDestination", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("lblOrigin")]
		private MonoTouch.UIKit.UILabel lblOrigin {
			get {
				this.__mt_lblOrigin = ((MonoTouch.UIKit.UILabel)(this.GetNativeField("lblOrigin")));
				return this.__mt_lblOrigin;
			}
			set {
				this.__mt_lblOrigin = value;
				this.SetNativeField("lblOrigin", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("lblRequested")]
		private MonoTouch.UIKit.UILabel lblRequested {
			get {
				this.__mt_lblRequested = ((MonoTouch.UIKit.UILabel)(this.GetNativeField("lblRequested")));
				return this.__mt_lblRequested;
			}
			set {
				this.__mt_lblRequested = value;
				this.SetNativeField("lblRequested", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("txtConfirmationNo")]
		private MonoTouch.UIKit.UILabel txtConfirmationNo {
			get {
				this.__mt_txtConfirmationNo = ((MonoTouch.UIKit.UILabel)(this.GetNativeField("txtConfirmationNo")));
				return this.__mt_txtConfirmationNo;
			}
			set {
				this.__mt_txtConfirmationNo = value;
				this.SetNativeField("txtConfirmationNo", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("txtDestination")]
		private MonoTouch.UIKit.UILabel txtDestination {
			get {
				this.__mt_txtDestination = ((MonoTouch.UIKit.UILabel)(this.GetNativeField("txtDestination")));
				return this.__mt_txtDestination;
			}
			set {
				this.__mt_txtDestination = value;
				this.SetNativeField("txtDestination", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("txtOrigin")]
		private MonoTouch.UIKit.UILabel txtOrigin {
			get {
				this.__mt_txtOrigin = ((MonoTouch.UIKit.UILabel)(this.GetNativeField("txtOrigin")));
				return this.__mt_txtOrigin;
			}
			set {
				this.__mt_txtOrigin = value;
				this.SetNativeField("txtOrigin", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("txtRequested")]
		private MonoTouch.UIKit.UILabel txtRequested {
			get {
				this.__mt_txtRequested = ((MonoTouch.UIKit.UILabel)(this.GetNativeField("txtRequested")));
				return this.__mt_txtRequested;
			}
			set {
				this.__mt_txtRequested = value;
				this.SetNativeField("txtRequested", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("txtAptCode")]
		private MonoTouch.UIKit.UILabel txtAptCode {
			get {
				this.__mt_txtAptCode = ((MonoTouch.UIKit.UILabel)(this.GetNativeField("txtAptCode")));
				return this.__mt_txtAptCode;
			}
			set {
				this.__mt_txtAptCode = value;
				this.SetNativeField("txtAptCode", value);
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
		
		[MonoTouch.Foundation.Connect("txtStatus")]
		private MonoTouch.UIKit.UILabel txtStatus {
			get {
				this.__mt_txtStatus = ((MonoTouch.UIKit.UILabel)(this.GetNativeField("txtStatus")));
				return this.__mt_txtStatus;
			}
			set {
				this.__mt_txtStatus = value;
				this.SetNativeField("txtStatus", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("lblAptRingCode")]
		private MonoTouch.UIKit.UILabel lblAptRingCode {
			get {
				this.__mt_lblAptRingCode = ((MonoTouch.UIKit.UILabel)(this.GetNativeField("lblAptRingCode")));
				return this.__mt_lblAptRingCode;
			}
			set {
				this.__mt_lblAptRingCode = value;
				this.SetNativeField("lblAptRingCode", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("lblPickupDate")]
		private MonoTouch.UIKit.UILabel lblPickupDate {
			get {
				this.__mt_lblPickupDate = ((MonoTouch.UIKit.UILabel)(this.GetNativeField("lblPickupDate")));
				return this.__mt_lblPickupDate;
			}
			set {
				this.__mt_lblPickupDate = value;
				this.SetNativeField("lblPickupDate", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("txtPickupDate")]
		private MonoTouch.UIKit.UILabel txtPickupDate {
			get {
				this.__mt_txtPickupDate = ((MonoTouch.UIKit.UILabel)(this.GetNativeField("txtPickupDate")));
				return this.__mt_txtPickupDate;
			}
			set {
				this.__mt_txtPickupDate = value;
				this.SetNativeField("txtPickupDate", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("btnStatus")]
		private MonoTouch.UIKit.UIButton btnStatus {
			get {
				this.__mt_btnStatus = ((MonoTouch.UIKit.UIButton)(this.GetNativeField("btnStatus")));
				return this.__mt_btnStatus;
			}
			set {
				this.__mt_btnStatus = value;
				this.SetNativeField("btnStatus", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("btnCancel")]
		private MonoTouch.UIKit.UIButton btnCancel {
			get {
				this.__mt_btnCancel = ((MonoTouch.UIKit.UIButton)(this.GetNativeField("btnCancel")));
				return this.__mt_btnCancel;
			}
			set {
				this.__mt_btnCancel = value;
				this.SetNativeField("btnCancel", value);
			}
		}
	}
}
