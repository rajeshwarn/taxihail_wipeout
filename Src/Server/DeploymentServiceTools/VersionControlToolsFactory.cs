using System.Configuration;

namespace DeploymentServiceTools
{
    public class VersionControlToolsFactory
    {
        public static IVersionControlTools GetInstance(string sourceDirectory, bool isGitHub)
        {
            return isGitHub 
                ? (IVersionControlTools)new GitTools(ConfigurationManager.AppSettings["GitPath"], sourceDirectory) 
                : (IVersionControlTools)new MecurialTools(ConfigurationManager.AppSettings["BitBucketPath"], sourceDirectory);
        }
    }
}
