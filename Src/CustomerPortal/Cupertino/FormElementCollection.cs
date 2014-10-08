#region

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using HtmlAgilityPack;

#endregion

namespace Cupertino
{
    /// <summary>
    /// Represents a combined list and collection of Form Elements.
    /// </summary>
    public class FormElementCollection : Dictionary<string, string>
    {
        /// <summary>
        /// Constructor. Parses the HtmlDocument to get all form input elements. 
        /// </summary>
        public FormElementCollection(HtmlDocument htmlDoc)
        {
            var inputs = htmlDoc.DocumentNode.Descendants("input");
            foreach (var element in inputs.Where(x => x.GetAttributeValue("type", "") != "image"))
            {
                string name = element.GetAttributeValue("name", "undefined");
                string value = element.GetAttributeValue("value", "");

                if (!ContainsKey(name))
                {
                    if (!name.Equals("undefined"))
                    {
                        Add(name, value);
                    }
                }
            }
        }

        /// <summary>
        /// Assembles all form elements and values to POST. Also html encodes the values.  
        /// </summary>
        public string AssemblePostPayload()
        {
            var sb = new StringBuilder();
            foreach (var element in this)
            {
                string value = HttpUtility.UrlEncode(element.Value);
                sb.Append("&" + element.Key + "=" + value);
            }
            return sb.ToString().Substring(1);
        }
    }
}