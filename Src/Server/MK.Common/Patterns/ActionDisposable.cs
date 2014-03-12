using System;

namespace MK.Common.iOS.Patterns
{
    public class ActionDisposable : IDisposable
    {
        Action _onDispose;
        public ActionDisposable (Action onDispose)
        {
            _onDispose = onDispose;
        }


        public void Dispose ()
        {
            _onDispose();
        }

    }
}

