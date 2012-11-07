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
                    if (keyValue.Count == 2)
                    {
                        TryAdd(keyValue.First(), Decode(keyValue.ElementAt(1)));
                    }
                    else
                    {
                        throw new Exception("Conflict with line in localizations:" + line);
                    }
                }
            }
        }

        public iOSResourceFileHandler(string filePath, IDictionary<string, string> dictionary)
            : base(filePath, dictionary)
        {

        }

        protected string Decode(string text)
        {
            return DecodeXML(text);
        }

        protected virtual string DecodeXML(string text)
        {
            //Others invalid characters does not look to be escaped...
            //encodedXml = xml.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
            return text.Replace("&lt;", "<").Replace("&gt;", ">");
        }

        protected override string GetFileText()
        {
            var stringBuilder = new StringBuilder();

            //"key"="value"\n;
            foreach (var resource in this)
            {
                stringBuilder.AppendFormat("\"{0}\"=\"{1}\";\n", resource.Key, Encode(resource.Value));
            }

            return stringBuilder.ToString();
        }

        protected virtual string EncodeXML(string text)
        {
            //Others invalid characters does not look to be escaped...
            //encodedXml = xml.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
            return text.Replace("<", "&lt;").Replace(">", "&gt;");
        }

        protected virtual string Encode(string text)
        {
            return EncodeXML(text);
        }
    }
}
