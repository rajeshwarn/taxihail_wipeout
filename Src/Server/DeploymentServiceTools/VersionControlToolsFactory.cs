using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeploymentServiceTools
{
    public class VersionControlToolsFactory
    {
        public static IVersionControlTools GetInstance(string exePath, string sourceDirectory, bool isGitHub)
        {
            return isGitHub ? (IVersionControlTools)new GitTools(exePath, sourceDirectory) : (IVersionControlTools)new MecurialTools(exePath, sourceDirectory);
        }
    }
}
