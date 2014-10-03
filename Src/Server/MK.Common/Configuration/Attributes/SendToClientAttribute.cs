using System;

namespace MK.Common.Configuration
{
    public class SendToClientAttribute : Attribute
    {
        public bool ExcludeFromWebApp { get; private set; }

        /// <summary>
        /// Attribute used to mark the setting properties that need to be return to the client.
        /// </summary>
        public SendToClientAttribute()
        {
        }

        /// <summary>
        /// Attribute used to mark the setting properties that need to be return to the client.
        /// </summary>
        /// <param name="excludeWebApp">If true, the attribute will not be returned to the web app.</param>
        public SendToClientAttribute(bool excludeWebApp)
        {
            ExcludeFromWebApp = excludeWebApp;
        }
    }
}