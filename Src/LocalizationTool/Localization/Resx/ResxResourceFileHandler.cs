using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.Tools.Localization.Resx
{
   public class ResxResourceFileHandler : ResourceFileHandlerBase
    {
       public ResxResourceFileHandler(string filePath) : base(filePath)
       {
           var resXResourceReader = new ResXResourceReader(filePath) {UseResXDataNodes = true};

           foreach (DictionaryEntry de in resXResourceReader)
           {
               var node = (ResXDataNode)de.Value;

               //FileRef is null if it is not a file reference.
               if (node.FileRef == null)
               {
					var value = node.GetValue ((ITypeResolutionService)null);
					if (value == null) 
					{
						throw new Exception ("Missing value for Resource " + node.Name);
					}
                   	TryAdd(node.Name, value.ToString());
               } 
           }
       }

       protected override string GetFileText()
       {
           throw new NotImplementedException();
       }
    }
}
