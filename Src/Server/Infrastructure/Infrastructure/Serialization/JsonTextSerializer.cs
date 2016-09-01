// ==============================================================================================================
// Microsoft patterns & practices
// CQRS Journey project
// ==============================================================================================================
// ©2012 Microsoft. All rights reserved. Certain content used with permission from contributors
// http://go.microsoft.com/fwlink/p/?LinkID=258575
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance 
// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is 
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and limitations under the License.
// ==============================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Infrastructure.Serialization
{
    using System.IO;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    public class JsonTextSerializer : ITextSerializer
    {
        private readonly JsonSerializer _serializer;
        private readonly JsonSerializer _serializerCatch;

        public JsonTextSerializer()
            : this(JsonSerializer.Create(new JsonSerializerSettings
            {
                // Allows deserializing to the actual runtime type
                TypeNameHandling = TypeNameHandling.All,
                // In a version resilient way
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            }))
        {
        }

        public JsonTextSerializer(JsonSerializer serializer)
        {
            _serializer = serializer;

            _serializerCatch = JsonSerializer.Create(new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                Binder = new NamespaceMappingSerializationBinder()
            });
        }

        public void Serialize(TextWriter writer, object graph)
        {
            var jsonWriter = new JsonTextWriter(writer);
#if DEBUG
            jsonWriter.Formatting = Formatting.Indented;
#endif

            _serializer.Serialize(jsonWriter, graph);

            // We don't close the stream as it's owned by the message.
            writer.Flush();
        }

        public object Deserialize(TextReader reader)
        {
            var content = reader.ReadToEnd();
            var jsonReader = new JsonTextReader(new StringReader(content));

            try
            {
                return _serializer.Deserialize(jsonReader);
            }
            catch (JsonSerializationException e)
            {
                try
                {
                    return _serializerCatch.Deserialize(new JsonTextReader(new StringReader(content)));
                }
                catch (JsonSerializationException ex)
                {
                    // Wrap in a standard .NET exception.
                    throw new SerializationException(ex.Message, ex);
                } 
            }
        }
    }

    public class PeekingTextReader : StringReader
    {
        private Queue<string> _peeks;

        public PeekingTextReader(string s): base(s)
        {
            _peeks = new Queue<string>();
        }

        public override string ReadLine()
        {
            if (_peeks.Count > 0)
            {
                var nextLine = _peeks.Dequeue();
                return nextLine;
            }
            return base.ReadLine();
        }

        public string PeekReadLine()
        {
            var nextLine = ReadLine();
            _peeks.Enqueue(nextLine);
            return nextLine;
        }
    }
}
