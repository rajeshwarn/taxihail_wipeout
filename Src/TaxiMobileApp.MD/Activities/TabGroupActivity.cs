using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace TaxiMobile.Activities
{
	public class TabGroupActivity : ActivityGroup
	{
	    private List<string> viewdIds;
	   
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			if (viewdIds == null) 
			{
				viewdIds = new List<string>();
			}
		}
		public override void FinishFromChild ( Activity child )
		{
			LocalActivityManager manager = LocalActivityManager;
			int index = viewdIds.Count - 1;
			
			if ( index < 1 ) 
			{
				Finish();
			  	return;
			}
			
			manager.DestroyActivity( viewdIds.ElementAt( index ), true);
			viewdIds.RemoveAt( index );
			index--;
			string lastId = viewdIds.ElementAt( index );
			Intent lastIntent = manager.GetActivity( lastId ).Intent;
			Window newWindow = manager.StartActivity( lastId, lastIntent );
			SetContentView( newWindow.DecorView );
		}
	
	 
		public void StartChildActivity( string id, Intent intent )
		{     
			LocalActivityManager manager = LocalActivityManager;		 
			Window window = manager.StartActivity( id, intent.AddFlags( ActivityFlags.ClearTop ) );
		 	if( window != null )
			{
		    	viewdIds.Add( id );
		      	SetContentView( window.DecorView ); 
		  	}    
		}
	
		public override bool OnKeyDown (Keycode keyCode, KeyEvent e)
		{
			if ( keyCode == Keycode.Back )
			{
			  //preventing default implementation previous to android.os.Build.VERSION_CODES.ECLAIR
			  return true;
			}
			return base.OnKeyDown (keyCode, e);
		}
		
		public override bool OnKeyUp (Keycode keyCode, KeyEvent e)
		{
			if ( keyCode == Keycode.Back )
			{
			  OnBackPressed();
			  return true;
			}
			return base.OnKeyUp (keyCode, e);
		}
		
		public override void OnBackPressed ()
		{
			int length = viewdIds.Count;
			if ( length >= 1 )
			{
			  	LocalActivityManager manager = LocalActivityManager;
				Activity current = manager.GetActivity( viewdIds.ElementAt( length - 1 ) );
			  	current.Finish();
			}
		}
		
		public void FinishAllChilds ( )
		{
			LocalActivityManager manager = LocalActivityManager;
			
			for( int index = viewdIds.Count - 1; index >= 1; index-- )
			{
				manager.DestroyActivity( viewdIds.ElementAt( index ), true);
				viewdIds.RemoveAt( index );
			}
			string lastId = viewdIds.ElementAt( 0 );
			Intent lastIntent = manager.GetActivity( lastId ).Intent;
			Window newWindow = manager.StartActivity( lastId, lastIntent );
			SetContentView( newWindow.DecorView );
		}
	
	}
}