using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;

namespace Infrastructure.Serialization
{
    public class NamespaceMappingSerializationBinder : DefaultSerializationBinder
    {
        private IEnumerable<Type> _assemblyCollection;
        private Dictionary<string, Type> _assemblyDictionary;

        public NamespaceMappingSerializationBinder()
        {
            _assemblyCollection = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                      from type in assembly.GetTypes()
                                      where type.FullName.Contains("MK.Common")
                                      select type;

            _assemblyDictionary = new Dictionary<string, Type>();
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            // do nothing
            if (!typeName.Contains("MK.Common"))
            {
                return base.BindToType(assemblyName, typeName);
            }

            // for generic type
            if (typeName.Contains("[["))
            {
                typeName = typeName.Substring(0, typeName.IndexOf('[', 0)) + Parse(typeName);
                return base.BindToType(assemblyName, typeName);
            }            

            bool containsArray = false;
            var className = typeName.Substring(typeName.LastIndexOf('.') + 1, typeName.Length - typeName.LastIndexOf('.') - 1);
            if (className.Contains("[]"))
            {
                containsArray = true;
                className = className.Trim(']').Trim('[');
            }

            Type assemblyInfo;
            if (_assemblyDictionary.ContainsKey(className))
            {
                assemblyInfo = _assemblyDictionary[className];
            }
            else
            {
                assemblyInfo = _assemblyCollection.SingleOrDefault(x => x.Name == className);
                _assemblyDictionary.Add(className, assemblyInfo);
            }

            if (assemblyInfo != null)
            {
                return base.BindToType(assemblyInfo.Assembly.FullName.Substring(0, assemblyInfo.Assembly.FullName.IndexOf(',')), containsArray? assemblyInfo.FullName+"[]": assemblyInfo.FullName);
            }
            else
            {
                return base.BindToType(assemblyName, typeName);
            }
        }

        private string Parse(string data)
        {
            if (data.Contains('['))
            {
                return "[" + Parse(data.Substring(data.IndexOf('[', 0) + 1, data.LastIndexOf(']') - data.IndexOf('[', 0) - 1)) + "]";
            }

            var extract = data.Split(',');
            if (extract.Length == 2)
            {
                var className = extract[0].Substring(extract[0].LastIndexOf('.') + 1, extract[0].Length - extract[0].LastIndexOf('.') - 1);

                Type assemblyInfo;
                if (_assemblyDictionary.ContainsKey(className))
                {
                    assemblyInfo = _assemblyDictionary[className];
                }
                else
                {
                    assemblyInfo = _assemblyCollection.SingleOrDefault(x => x.Name == className);
                    _assemblyDictionary.Add(className, assemblyInfo);
                }

                if (assemblyInfo != null)
                {
                    data = ReplaceFirstOccurrence(data, extract[0].Trim(), assemblyInfo.FullName);
                    data = ReplaceLastOccurrence(data, extract[1].Trim(), assemblyInfo.Assembly.FullName.Substring(0, assemblyInfo.Assembly.FullName.IndexOf(',')));
                }
            }

            return data;
        }


        private string ReplaceFirstOccurrence(string source, string find, string replace)
        {
            int place = source.IndexOf(find);
            string result = source.Remove(place, find.Length).Insert(place, replace);
            return result;
        }

        private string ReplaceLastOccurrence(string source, string find, string replace)
        {
            int place = source.LastIndexOf(find);
            string result = source.Remove(place, find.Length).Insert(place, replace);
            return result;
        }
    }
}
