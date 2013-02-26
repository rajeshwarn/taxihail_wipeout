using System;
using apcurium.MK.Common.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace MK.Booking.Api.Client.iOS.Cmt
{
    public class CmtConfigurationClientService :  IConfigurationManager
    {
        Dictionary<string,string> settings;

        public CmtConfigurationClientService ()
        {
            settings = new Dictionary<string,string> ();
            settings ["Client.HideCallDispatchButton"] = "true";
            settings ["Client.HideReportProblem"] = "true";
            settings ["Client.ShowRingCodeField"] = "false"; 
            settings ["Client.NumberOfCharInRefineAddress"] = "5";   
        }

        #region IConfigurationManager implementation

        public void Reset ()
        {

        }

        public string GetSetting (string key)
        {
            return settings[key];
        }

        public System.Collections.Generic.IDictionary<string, string> GetSettings ()
        {
            return new Dictionary<string,string> ();
        }

        public void SetSettings (System.Collections.Generic.IDictionary<string, string> appSettings)
        {
            settings = settings.Concat (appSettings).ToDictionary(x => x.Key, y => y.Value);
        }

        #endregion
    }
}

