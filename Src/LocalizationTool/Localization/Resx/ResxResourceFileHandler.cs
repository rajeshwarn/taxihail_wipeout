using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.Tools.Localization.Resx
{
   public class ResxResourceFileHandler : ResourceFileHandlerBase
    {
       public ResxResourceFileHandler(string filePath) : base(filePath)
       {
       }

       protected override string GetFileText()
       {
           throw new NotImplementedException();
       }
    }
}
