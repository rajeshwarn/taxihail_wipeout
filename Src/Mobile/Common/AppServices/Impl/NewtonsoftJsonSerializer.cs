using System;
using Cirrious.CrossCore.Platform;
using Newtonsoft.Json;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class NewtonsoftJsonSerializer : IMvxJsonConverter
    {
        private readonly JsonSerializer _serializer;

        public NewtonsoftJsonSerializer(JsonSerializer serializer)
        {
            _serializer = serializer;
        }


        public T DeserializeObject<T>(string inputText)
        {
            var stringReader = new System.IO.StringReader(inputText);
            var reader = new JsonTextReader(stringReader);

            return _serializer.Deserialize<T>(reader);
        }

        public string SerializeObject(object toSerialise)
        {
            var stringWriter = new System.IO.StringWriter();

            var writer = new JsonTextWriter(stringWriter);

            _serializer.Serialize(writer, toSerialise);

            return stringWriter.GetStringBuilder().ToString();
        }

        public object DeserializeObject(Type type, string inputText)
        {
            var stringReader = new System.IO.StringReader(inputText);
            var reader = new JsonTextReader(stringReader);

            return _serializer.Deserialize(reader, type);
        }
    }
}