using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.Tools.Localization.iOS
{
    public class iOSResourceFileHandler : ResourceFileHandlerBase
    {
        public iOSResourceFileHandler(string filePath)
            : base(filePath)
        {
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim(' ', ';');

                var keyValue = trimmedLine.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().Trim('"')).ToList();

                if (keyValue.Any())
                {
                    if(keyValue.Count == 2)
                    {
                        TryAdd(keyValue.First(), keyValue.ElementAt(1));
                    }
                    else
                    {
                        throw new Exception("Conflict with line in localizations:" + line);
                    }
                }
            }
        }
    }
}
