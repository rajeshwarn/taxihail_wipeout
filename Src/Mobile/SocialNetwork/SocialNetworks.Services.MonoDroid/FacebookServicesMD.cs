using System;
using SocialNetworks.Services.Entities;
using Android.App;
using Android.OS;
using System.Json;
using System.Collections.Generic;
using com.facebook.droid;
using Com.Facebook.Android;
using Com.Facebook;
using Com.Facebook.Model;
using Android.Runtime;
using SocialNetworks.Services.MonoDroid.Callbacks;
using System.Linq;

namespace SocialNetworks.Services.MonoDroid
{
    public class FacebookServicesMD : IFacebookService
    {
		private Activity _mainActivity;

		public FacebookServicesMD(string appId, Activity mainActivity)
        {
			_mainActivity = mainActivity;
        }

		public void AuthorizeCallback (int requestCode, int resultCode, Android.Content.Intent data)
		{
			if (Session.ActiveSession != null)
				Session.ActiveSession.OnActivityResult(_mainActivity, requestCode, resultCode, data);

			Session currentSession = Session.ActiveSession;
			if (currentSession == null || currentSession.IsClosed) {
				Session session = new Session.Builder(_mainActivity).Build();
				Session.ActiveSession = session;
				currentSession = session;
			}

			if (currentSession.IsOpened) {
				Session.OpenActiveSession (_mainActivity, true, new StatusCallback ((session,state,exception) => {
					ConnectionStatusChanged (this, new FacebookStatus (state == SessionState.Opened));
				}));
			}
		}
		
		#region IFacebookService implementation
        public bool IsConnected
        {
			get { return Session.ActiveSession != null && Session.ActiveSession.State == SessionState.Opened;  }
        }

		public void Connect(string permissions)
        {
			Session currentSession = Session.ActiveSession;
			if (currentSession == null || currentSession.IsClosed ) {
				Session session = new Session.Builder(_mainActivity).Build();
				Session.ActiveSession = session;
				currentSession = session;
			}

			if ( currentSession.IsOpened ) {
		

			} else if (!currentSession.IsOpened) {
				Session.OpenRequest op = new Session.OpenRequest(_mainActivity);

				op.SetLoginBehavior(SessionLoginBehavior.SuppressSso);
				op.SetCallback (null);

				List<string> perms = permissions.Split (',').ToList ();
				op.SetPermissions(perms);

				Session s = new Session.Builder(_mainActivity).Build();
				Session.ActiveSession = s;
				s.OpenForRead(op);
			}
        }

        public void GetUserInfos(Action<UserInfos> onRequestDone, Action onError)
        {
			Request.ExecuteGraphPathRequestAsync (Session.ActiveSession, "me", new RequestCallback ( (r) => {
				var user = GraphObjectFactory.Create( r.GraphObject.InnerJSONObject ).AsMap();

				if( user != null )
				{
					var infos = new UserInfos();
					infos.Id = user["id"].ToString();
					infos.Email = user["email"].ToString();
					infos.Firstname = user["first_name"].ToString();
					infos.Lastname = user["last_name"].ToString();

					if (onRequestDone != null)
					{
						onRequestDone(infos);
					}
				}
				else
				{
					onError();
				}

			} ));
        }

		public void GetLikes( Action<List<UserLike>> onRequestDone )                   
		{
//			var asyncRunner = new AsyncFacebookRunner (_facebookClient);
//			asyncRunner.Request ("me/likes", new RequestListener((response, obj) => {
//					
//				List<UserLike> likesList = new List<UserLike>();
//				if(response != null)
//				{
//					var data = JsonObject.Parse(response);
//					var jv = data["data"];
//					foreach( JsonValue item in jv )
//					{
//						var like = new UserLike();
//						like.Id = item["id"];
//						like.Category = item["category"];
//						like.Name = item["name"];
//						like.CreatedTime = item["created_time"];
//
//						likesList.Add( like );
//					}
//				}
//
//				if(onRequestDone != null)
//				{
//					onRequestDone(likesList);
//				}
//			}				
//		    ));	
		}

        public void SetCurrentContext(object context)
        {
            _mainActivity = context as Activity;
        }

        public void Like( string objectId )
		{
//			var asyncRunner = new AsyncFacebookRunner (_facebookClient);
//			asyncRunner.Request( objectId + "/likes", new RequestListener((response, obj) => {
//				Console.WriteLine( obj.ToString() );
//				Console.WriteLine( response );
//
//			}));
		}

        public void Share(Post post, Action onRequestDone)
        {
//			Bundle parameters = new Bundle ();
//			parameters.PutString ("message", post.Message);
//			parameters.PutString ("link", post.Url);
//			parameters.PutString ("name", post.Name);
//			parameters.PutString ("description", post.Description);
//			parameters.PutString ("picture", post.Picture);
//			
//            var asyncRunner = new AsyncFacebookRunner (_facebookClient);
//			asyncRunner.Request("me/feed",parameters, "POST", new RequestListener((response, obj) => {
//				if(onRequestDone != null) onRequestDone();
//			}), null);
        }

        public void Disconnect()
        {
			Session.ActiveSession.CloseAndClearTokenInformation ();
        }

		public event EventHandler<FacebookStatus> ConnectionStatusChanged = delegate {};
		#endregion
    }    

}
