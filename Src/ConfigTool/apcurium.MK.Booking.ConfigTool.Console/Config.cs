using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.ConfigTool
{
    public abstract class Config
    {
        public Config(AppConfig parent)
        {
            Parent = parent;
        }

        public AppConfig Parent { get; set; }

        public abstract void Apply();        

    }
}
