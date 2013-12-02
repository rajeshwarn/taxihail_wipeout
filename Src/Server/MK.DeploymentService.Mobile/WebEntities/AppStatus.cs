using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomerPortal.Web.Entities
{
    public enum AppStatus
    {
        Open,
        LayoutCompleted,
        LayoutRejected,
        AssetReady,
        Test,
        Production,
        TestingNewVersion,
        DemoSystem,
    }
}