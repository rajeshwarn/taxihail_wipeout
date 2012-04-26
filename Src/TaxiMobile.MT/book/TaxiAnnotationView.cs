using System;
using MonoTouch.Foundation;
using MonoTouch.MapKit;
using MonoTouch.UIKit;

namespace TaxiMobile.Book
{
	public class TaxiAnnotationView : MKAnnotationView
	{
	
		
		[Export( "initWithCoder:" )]
		public TaxiAnnotationView ( NSCoder coder ) : base( coder )
		{
			
		}

		public TaxiAnnotationView ( IntPtr ptr ) : base( ptr )
		{
			
		}

		public TaxiAnnotationView ( AddressAnnotation annotation, string id ) : base( annotation, id )
		{
			Annotation = annotation;
			Image = UIImage.FromFile( "Assets/Status.png" );				
		}
		
		
			
		public override NSObject Annotation
		{
			get { return base.Annotation; }
			set
			{
				if ( value == null )
				{
					base.Annotation.Dispose (  );					
					return;
				}
				
				base.Annotation = value;										
			}
		}
		
		
	}
}

