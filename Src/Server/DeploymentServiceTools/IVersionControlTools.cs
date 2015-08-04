using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeploymentServiceTools
{
    public interface IVersionControlTools
    {
        void Pull();

        void Clone(string revisionNumber);

        void Revert();

        void Purge();

        void Update(string revisionNumber);
    }
}
