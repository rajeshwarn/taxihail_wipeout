using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using apcurium.Framework.Extensions;
using apcurium.Framework;

using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.Client
{

	public class SimilarAddressTableDatasource : UITableViewDataSource
	{

		static NSString kCellIdentifier = new NSString ("SimilarAddressTableDatasourceCellIdentifier");

		

		public SimilarAddressTableDatasource ()
		{
			Similars = new Address[0];	
		}
		
		public IEnumerable<Address> Similars {
			get;
			set;
		}

		public override int NumberOfSections (UITableView tableView)
		{
			return 1;
		}

	




		public override int RowsInSection (UITableView tableview, int section)
		{
			return Similars.Count();
			
		}


		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell (kCellIdentifier);
			if (cell == null) {				
				cell = new UITableViewCell (UITableViewCellStyle.Subtitle, kCellIdentifier);
			}
			cell.BackgroundColor = UIColor.Clear;
			cell.Accessory = UITableViewCellAccessory.None;
			cell.SelectionStyle = UITableViewCellSelectionStyle.Gray;
			cell.TextLabel.TextColor = UIColor.Black;
    		cell.TextLabel.Font = UIFont.BoldSystemFontOfSize( 14 );
			
			var b = Similars.ElementAt(indexPath.Row);
			
			if ( b.FriendlyName.HasValue() )
			{
				cell.TextLabel.Text = b.FriendlyName;
			}
			
			cell.TextLabel.Text = "  ";
			
			
			cell.DetailTextLabel.Font = UIFont.BoldSystemFontOfSize( 14 );
			cell.DetailTextLabel.Text =  b.FullAddress.ToSafeString();
			
			if ( b.Apartment.HasValue() ) 
			{
				cell.DetailTextLabel.Text += ", A:" + b.Apartment;
			}			
			
			if ( b.RingCode.HasValue() ) 
			{
				cell.DetailTextLabel.Text += ", C:" + b.RingCode;
			}
			
			
			
			return cell;
		}
	}
}


