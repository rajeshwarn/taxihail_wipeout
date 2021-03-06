﻿#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Serialization
{
    public class CsvCustomSerializer
    {
        public static void SerializeToStream<T>(IRequestContext requestContext, T response, Stream stream)
        {
            if (response is List<Dictionary<string, string>>)
            {
                var responseSpe = response as List<Dictionary<string, string>>;
                SerializeDictionaryToStream(responseSpe, stream);
            }
        }

        private static void SerializeDictionaryToStream(List<Dictionary<string, string>> response, Stream stream)
        {
            if (response == null)
            {
                return;
            }

            using (var writer = new CsvWriter(new StreamWriter(stream)))
            {
                var orderedColumns = response.OrderByDescending(x => x.Keys.Count);
                var columns = orderedColumns.Any() ? orderedColumns.First() : new Dictionary<string, string>();

                string columnsName = null;

                foreach (var column in columns)
                {
                    if (columnsName == null)
                        columnsName = "\"" + column.Key + "\"";
                    else
                        columnsName += "," + "\"" + column.Key + "\"";
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