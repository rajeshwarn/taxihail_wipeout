using CustomerPortal.Web.Services.Impl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerPortal.Web.Services
{
    public static class SourceControlFactory
    {
        public static ISourceControl GetInstance()
        {
            var isGitHub = (bool) new AppSettingsReader().GetValue("IsGitHubSourceControl", typeof(bool));

            return isGitHub ? (ISourceControl)new GitHubVersionUpdater() : (ISourceControl)new BitBucketVersionUpdater();
        }
    }
}
