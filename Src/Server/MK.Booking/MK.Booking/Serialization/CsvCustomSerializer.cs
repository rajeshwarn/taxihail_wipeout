using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.ServiceHost;
using CsvHelper;

namespace apcurium.MK.Booking.Serialization
{
    public class CsvCustomSerializer
    {
        static CsvCustomSerializer()
        {
        }

        public static void SerializeToStream<T>(IRequestContext requestContext, T response, Stream stream)
        {
            if (response is List<Dictionary<string, string>>)
            {
                var responseSpe = response as List<Dictionary<string, string>>;
                SerializeDictionaryToStream(requestContext, responseSpe, stream);
            }

        }


        private static void SerializeDictionaryToStream(IRequestContext requestContext, List<Dictionary<string, string>> response, Stream stream)
        {
            if (response == null) return;

            using (var writer = new CsvWriter(new StreamWriter(stream)))
            {
                var columns = response[0];
                string columnsName = null;
               
                foreach (var column in columns)
                {
                    if (columnsName == null)
                        columnsName = "\"" + column.Key + "\"";
                    else
                        columnsName += "," + "\""  + column.Key + "\"";
                }

                writer.WriteField(columnsName, false);
                writer.NextRecord();

                foreach (var line in response)
                {
                    string columnValue = null;
                    foreach (var column in line)
                    {
                        if (columnValue == null)
                            columnValue = "\"" + column.Value + "\"";
                        else
                            columnValue += "," + "\"" + column.Value + "\"";
                    }
                    writer.WriteField(columnValue, false);
                    writer.NextRecord();

                }
            }
        }

  
        public static object DeserializeFromStream(Type type, Stream stream)
        {
            throw new NotImplementedException();
        }

    }
}
