using System;
using MonoTouch.FacebookConnect;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MonoTouch.Foundation;

namespace SocialNetworks.Services.MonoTouch.FacebookHelpers
{
    internal class RequestHandler : FBRequestDelegate {

        static List<RequestHandler> handlers = new List<RequestHandler> ();
        Action<FBRequest, NSObject> loadedHandler;
        
        public RequestHandler (Action<FBRequest, NSObject> loadedHandler)
        {
            handlers.Add (this);
            this.loadedHandler = loadedHandler;
        }
        
        public override void FailedWithError (FBRequest request, NSError error)
        {
            var u = new UIAlertView ("Request Error", "Failed with " + error.ToString (), null, "ok");
            u.Dismissed += delegate {
                handlers.Remove (this);
            };
            u.Show ();
        }
        
        public override void RequestLoaded (FBRequest request, NSObject result)
        {
            loadedHandler (request, result);
            handlers.Remove (this);
        }
    }
}

