using System;
using SocialNetworks.Services.Entities;
using Android.App;
using Android.OS;
using System.Json;
using System.Collections.Generic;
using com.facebook.droid;
using Com.Facebook.Android;

namespace SocialNetworks.Services.MonoDroid
{
    public class FacebookServicesMD : IFacebookService, SessionEvents.IAuthListener, SessionEvents.ILogoutListener
    {
        private static Facebook _facebookClient;
		private Activity _mainActivity;
		private Handler _handler;
        
		public FacebookServicesMD(string appId, Activity mainActivity)
        {
			Console.WriteLine (mainActivity.ToString ());
			if (_facebookClient == null) {
				_facebookClient = new Facebook (appId);
			}

			SessionEvents.AddAuthListener (this);
			SessionEvents.AddLogoutListener (this);

			_mainActivity = mainActivity;
            SessionStore.Clear(_mainActivity.BaseContext);
			SessionStore.Restore(_facebookClient, _mainActivity.BaseContext);
			_handler = new Handler();
        }

		public void AuthorizeCallback (int requestCode, int resultCode, Android.Content.Intent data)
		{
			_facebookClient.AuthorizeCallback (requestCode, resultCode, data);

            GetUserInfos(u =>
                {
                    u.ToString();
                }, () => Console.Write("ee"));
            //ConnectionStatusChanged(this, new FacebookStatus(false));
		}
		
		#region IFacebookService implementation
        public bool IsConnected
        {
			get { return _facebookClient.IsSessionValid;  }
        }

		public void Connect(string permissions)
        {
			if(!_facebookClient.IsSessionValid)
			{	
				_facebookClient.Authorize(_mainActivity, new string[] { permissions }, new LoginDialogListener ());
			}
        }

        public void GetUserInfos(Action<UserInfos> onRequestDone, Action onError)
        {
            var asyncRunner = new AsyncFacebookRunner (_facebookClient);
			asyncRunner.Request("me", new RequestListener((response, obj) => {
				var data = (JsonObject) JsonValue.Parse (response);
                if (!data.ContainsKey("id"))
                {
                    onError();
                    return;
                }
				var infos = new UserInfos();
				infos = new UserInfos();
				infos.Id = data["id"];
				infos.Email = data["email"];
				infos.Firstname = data["first_name"];
				infos.Lastname = data["last_name"];

                if (onRequestDone != null)
                {
                    onRequestDone(infos);
                }
                else
                {
                    onError();
                }
			}));
        }

		public void GetLikes( Action<List<UserLike>> onRequestDone )                   
		{
			var asyncRunner = new AsyncFacebookRunner (_facebookClient);
			asyncRunner.Request ("me/likes", new RequestListener((response, obj) => {
					
				List<UserLike> likesList = new List<UserLike>();
				if(response != null)
				{
					var data = JsonObject.Parse(response);
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

        public void SetCurrentContext(object context)
        {
            _mainActivity = context as Activity;
        }

        public void Like( string objectId )
		{
			var asyncRunner = new AsyncFacebookRunner (_facebookClient);
			asyncRunner.Request( objectId + "/likes", new RequestListener((response, obj) => {
				Console.WriteLine( obj.ToString() );
				Console.WriteLine( response );

			}));
		}

        public void Share(Post post, Action onRequestDone)
        {
			Bundle parameters = new Bundle ();
			parameters.PutString ("message", post.Message);
			parameters.PutString ("link", post.Url);
			parameters.PutString ("name", post.Name);
			parameters.PutString ("description", post.Description);
			parameters.PutString ("picture", post.Picture);
			
            var asyncRunner = new AsyncFacebookRunner (_facebookClient);
			asyncRunner.Request("me/feed",parameters, "POST", new RequestListener((response, obj) => {
				if(onRequestDone != null) onRequestDone();
			}), null);
        }

        public void Disconnect()
        {
            SessionEvents.OnLogoutBegin ();
			var asyncRunner = new AsyncFacebookRunner (_facebookClient);
			asyncRunner.Logout(_mainActivity.BaseContext, new RequestListener ((r,o) => {
				_handler.Post (delegate {
					SessionEvents.OnLogoutFinish ();
				});
			}));
        }

		public event EventHandler<FacebookStatus> ConnectionStatusChanged = delegate {};
		#endregion

		#region IAuthListener implementation
		public void OnAuthSucceed ()
		{
			if (_mainActivity != null) {
				SessionStore.Save (_facebookClient, _mainActivity.BaseContext);
			}
			ConnectionStatusChanged (this, new FacebookStatus (true));
		}

		public void OnAuthFail (string error)
		{
			ConnectionStatusChanged(this, new FacebookStatus(false));
		}
		#endregion

		#region ILogoutListener implementation
		public void OnLogoutBegin ()
		{
			
		}

		public void OnLogoutFinish ()
		{
			if (_mainActivity != null) {
				SessionStore.Clear (_mainActivity.BaseContext);
			}
			ConnectionStatusChanged(this, new FacebookStatus(false));
		}
		#endregion
		
		class LoginDialogListener : BaseDialogListener
		{
			public override void OnComplete (Bundle values)
			{

				SessionEvents.OnLoginSuccess ();
			}			
		}		
			
    }    
}
