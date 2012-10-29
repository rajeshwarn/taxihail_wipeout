using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
#if NETFWK
	using System.ComponentModel.DataAnnotations.Schema;
#endif
using System.IO;
using System.Runtime.Serialization;

namespace MK.ConfigurationManager.Entities
{
    public class Company
    {
        public Company()
        {
            ConfigurationProperties = new Dictionary<string, string>();
            MobileConfigurationProperties = new Dictionary<string, string>();
        }

        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }

        [NotMapped]
        public Dictionary<string, string> ConfigurationProperties
        {
            get;
            set;
        }

        [NotMapped]
        public Dictionary<string, string> MobileConfigurationProperties
        {
            get;
            set;
        }

        public string DictionaryAsXml
        {
            get
            {
                return ToXml(ConfigurationProperties);
            }
            set
            {
                ConfigurationProperties = FromXml(value);
            }
        }

        public string MObileDictionaryAsXml
        {
            get
            {
                return ToXml(MobileConfigurationProperties);
            }
            set
            {
                MobileConfigurationProperties = FromXml(value);
            }
        }

        private Dictionary<string, string> FromXml(string value)
        {
            if(value == null) return new Dictionary<string, string>();
            using (Stream stream = new MemoryStream())
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(value);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                var deserializer = new DataContractSerializer(typeof(Dictionary<string, string>));
                return (Dictionary<string, string>) deserializer.ReadObject(stream);
            }

        }

        private string ToXml(Dictionary<string, string> myDictionary)
        {
            using (var memStm = new MemoryStream())
            {
                var serializer = new DataContractSerializer(typeof (Dictionary<string, string>));
                serializer.WriteObject(memStm, myDictionary);
                memStm.Seek(0, SeekOrigin.Begin);
                var result = new StreamReader(memStm).ReadToEnd();
                return result;
            }
        }
    }
}