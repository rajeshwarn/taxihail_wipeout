using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TaxiMobile.Lib.Data;

namespace TaxiMobile.Book
{
	public class SimilarAddressTableDelegate : UITableViewDelegate
	{		
		//private UITextField _parent;
		private  Action<LocationData> _refresh;
		public SimilarAddressTableDelegate ( Action<LocationData> refresh)
		{
			_refresh = refresh;
		//	_parent = parent;
			Similars = new LocationData[0];
		}
		
		public IEnumerable<LocationData>  Similars {
			get;
			set;
		}
		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			
			//_parent.Text = Similars.ElementAt( indexPath.Row ).Display;						
			//_parent.ResignFirstResponder();
			
			tableView.Hidden = true;
		
			if ( _refresh != null )
			{
				_refresh( Similars.ElementAt( indexPath.Row )  );
			}
		}
		
	}
}

