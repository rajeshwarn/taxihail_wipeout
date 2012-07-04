using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MoveOn.Common.Extensions
{
    public static class EventExtension
    {
        public static void Raise( this EventHandler eventHandler)
        {
            if ( eventHandler != null )
            {
                eventHandler(null, EventArgs.Empty);
            }
        }
        public static void Raise(this EventHandler eventHandler, object sender)
        {
            if (eventHandler != null)
            {
                eventHandler(sender, EventArgs.Empty);
            }
        }
    }
}