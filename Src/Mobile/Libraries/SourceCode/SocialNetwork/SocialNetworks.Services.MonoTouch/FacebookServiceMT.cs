using System;
using MonoTouch.FacebookConnect;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Json;
using SocialNetworks.Services.Entities;
using SocialNetworks.Services.MonoTouch.FacebookHelpers;
using System.Collections.Generic;



namespace SocialNetworks.Services.MonoTouch
{
    public class FacebookServiceMT : FBSessionDelegate, IFacebookService 
    {
        private readonly Facebook facebookClient;
        private const string FBAccessTokenKey = "FBAccessTokenKey";
        private const string FBExpirationDateKey = "FBExpirationDateKey";

		public event EventHandler<FacebookStatus> ConnectionStatusChanged = delegate{};

		public FacebookServiceMT(string appId, string urlSchemeSuffix = "" )
        {
            facebookClient = new Facebook(appId, this);
			facebookClient.UrlSchemeSuffix = urlSchemeSuffix;

            var defaults = NSUserDefaults.StandardUserDefaults;
            if (defaults [FBAccessTokenKey] != null && defaults [FBExpirationDateKey] != null){
                facebookClient.AccessToken = defaults ["FBAccessTokenKey"] as NSString;
                facebookClient.ExpirationDate = defaults ["FBExpirationDateKey"] as NSDate;
            }
        }
		
		#region IFacebookService implementation
        public void Connect(string permissions)
        {
            if (!facebookClient.IsSessionValid)
            {
                facebookClient.Authorize (new string [] { permissions });
            }
        }

        public void SetCurrentContext(object context)
        {
         
        }

		
		public bool IsConnected { get{ return facebookClient.IsSessionValid; } } 

        public void GetUserInfos(Action<UserInfos> onRequestDone, Action onError)
        {

            facebookClient.GraphRequest ("me", new NSMutableDictionary (), "GET", Handler ((request, obj) => {
				UserInfos infos = null;
				if(request.Error == null)
				{
					var data = obj as NSDictionary;
					infos = new UserInfos();
					infos.Id = data["id"].ToString();
					infos.Email = data["email"].ToString();
					infos.Firstname = data["first_name"].ToString();
					infos.Lastname = data["last_name"].ToString();
				}
				if(onRequestDone != null)
				{
					onRequestDone(infos);
				}
				else
				{
					onError();
				}
			}				
		    ));	
        }

		public void GetLikes( Action<List<UserLike>> onRequestDone )                   
		{
			facebookClient.GraphRequest ("me/likes", new NSMutableDictionary (), "GET", Handler ((request, obj) => {
					
				List<UserLike> likesList = new List<UserLike>();
				if(request.ResponseText != null)
				{
					var data = JsonObject.Parse(request.ResponseText.ToString());
					var jv = data["data"];
					foreach( JsonValue item in jv )
					{
						var like = new UserLike();
						like.Id = item["id"];
						like.Category = item["category"];
						like.Name = item["name"];
						like.CreatedTime = item["created_time"];

						likesList.Add( like );
					}
				}

				if(onRequestDone != null)
				{
					onRequestDone(likesList);
				}
			}				
		    ));	
		}

		public void Like( string objectId )
		{
			facebookClient.GraphRequest( objectId + "/likes", new NSMutableDictionary(), "POST", Handler ((request, obj) => {
				Console.WriteLine( obj.ToString() );
				Console.WriteLine( request.ToString() );

			}));
		}

        public void Share(Post post, Action onRequestDone)
        {
			var postData = NSMutableDictionary.FromObjectsAndKeys (
				new object [] { post.Message, post.Url, post.Name, post.Description, post.Picture },
				new object [] { "message", "link", "name", "description", "picture" });

            facebookClient.GraphRequest ("me/feed", postData, "POST", Handler ((request, obj) => {
					
				if(onRequestDone != null) onRequestDone();
			}				
		    ));	
        }

        public void Disconnect()
        {
            ClearAuthorization();
			facebookClient.Logout();
        }
        #endregion

        #region FBSessionDelegate overrides
        public override void DidLogin ()
        {
			if( facebookClient.IsSessionValid )
            	SaveAuthorization ();

			ConnectionStatusChanged(this, new FacebookStatus(facebookClient.IsSessionValid));
        }
        public override void DidLogout ()
        {
            ClearAuthorization ();
			ConnectionStatusChanged(this, new FacebookStatus(false));
        }
        
        public override void DidNotLogin (bool cancelled)
        {
            ConnectionStatusChanged(this, new FacebookStatus(false));
        }
        
        public override void SessionInvalidated ()
        {
            ConnectionStatusChanged(this, new FacebookStatus(false));
        }
        #endregion

        private void SaveAuthorization ()
        {
            var defaults = NSUserDefaults.StandardUserDefaults;
            defaults [FBAccessTokenKey] = new NSString (facebookClient.AccessToken);
            defaults [FBExpirationDateKey] = facebookClient.ExpirationDate;
            defaults.Synchronize ();
		}

        private void ClearAuthorization ()
        {
            var defaults = NSUserDefaults.StandardUserDefaults;
            defaults.RemoveObject (FBAccessTokenKey);
            defaults.RemoveObject (FBExpirationDateKey);
            defaults.Synchronize ();
		}
        
        public bool HandleOpenURL (UIApplication application, NSUrl url)
        {
            return facebookClient.HandleOpenURL (url);
        } 
		
		FBRequestDelegate Handler (Action<FBRequest,NSObject> handler)
		{
				return new RequestHandler (handler);
		}
		
		FBDialogDelegate DialogCallback (Action<NSUrl> callback)
		{
			return new DialogHandler (callback);
		}
        
    }
}

