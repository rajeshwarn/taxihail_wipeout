using System;
using apcurium.MK.Common.Extensions;
using Newtonsoft.Json;

namespace apcurium.MK.Common.Serializer
{
    public class NewtonsoftJsonSerializer
    {
        private readonly JsonSerializer _serializer;

        public NewtonsoftJsonSerializer(JsonSerializer serializer)
        {
            _serializer = serializer;
        }


        public T DeserializeObject<T>(string inputText)
        {
            return typeof(string) == typeof(T) && !inputText.Contains("\"")
                ? InnerDeserializeObject<T>("\""+inputText+"\"")
                : InnerDeserializeObject<T>(inputText);
        }

        private TResult InnerDeserializeObject<TResult>(string inputText)
        {
            using (var stringReader = new System.IO.StringReader(inputText))
            {
                var reader = new JsonTextReader(stringReader);
                return _serializer.Deserialize<TResult>(reader);
            }
        }

        public string SerializeObject(object toSerialise)
        {
            using (var stringWriter = new System.IO.StringWriter())
            {
                var writer = new JsonTextWriter(stringWriter);

                _serializer.Serialize(writer, toSerialise);

                return stringWriter.GetStringBuilder().ToString();
            }
        }

        public object DeserializeObject(Type type, string inputText)
        {
            using (var stringReader = new System.IO.StringReader(inputText))
            {
                var reader = new JsonTextReader(stringReader);
                return _serializer.Deserialize(reader,type);
            }
        }
    }
}