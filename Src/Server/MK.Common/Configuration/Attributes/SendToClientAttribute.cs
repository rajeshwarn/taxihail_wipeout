using System;

namespace MK.Common.Configuration
{
    public class SendToClientAttribute : Attribute
    {
        /// <summary>
        /// If true, the attribute will not be returned to the web app (company settings)
        /// </summary>
        public bool ExcludeFromWebApp { get; private set; }

        /// <summary>
        /// Attribute used to mark the setting properties that need to be return to the client.
        /// </summary>
        public SendToClientAttribute()
        {
        }
    }
}