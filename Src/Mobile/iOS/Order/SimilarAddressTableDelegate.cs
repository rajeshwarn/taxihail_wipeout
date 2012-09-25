using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class SimilarAddressTableDelegate : UITableViewDelegate
	{		
		//private UITextField _parent;
		private  Action<Address> _refresh;
		public SimilarAddressTableDelegate ( Action<Address> refresh)
		{
			_refresh = refresh;
		//	_parent = parent;
			Similars = new Address[0];
		}
		
		public IEnumerable<Address>  Similars {
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

