using System;
using MonoTouch.Foundation;
using MonoTouch.FacebookConnect;

namespace SocialNetworks.Services.MonoTouch.FacebookHelpers
{
    internal class DialogHandler: FBDialogDelegate {

        Action<NSUrl> callback;
        
        public DialogHandler (Action<NSUrl> callback)
        {
            this.callback = callback;
        }
        public override void CompletedWithUrl (NSUrl url)
        {
            callback (url);
        }
    }
}

