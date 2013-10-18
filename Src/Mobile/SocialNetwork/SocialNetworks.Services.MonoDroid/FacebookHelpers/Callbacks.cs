using System;
using Com.Facebook;

namespace SocialNetworks.Services.MonoDroid.Callbacks
{
	public class StatusCallback : Java.Lang.Object, Session.IStatusCallback
	{
		private Action<Session,SessionState, Java.Lang.Exception> _action;

		public StatusCallback( Action<Session,SessionState, Java.Lang.Exception> action )
		{
			_action = action;
		}

		public void Call (Session session, SessionState state, Java.Lang.Exception exception)
		{
			_action (session, state, exception);
		}
	}

	public class RequestCallback : Java.Lang.Object, Request.ICallback
	{
		private Action<Response> _action;

		public RequestCallback(Action<Response> action)
		{
			_action = action;
		}

		public void OnCompleted(Response response) 
		{
			_action (response);
		}
	}
}

