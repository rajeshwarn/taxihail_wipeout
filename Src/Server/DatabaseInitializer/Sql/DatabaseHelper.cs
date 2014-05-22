﻿#region

using System;
using System.Collections.Generic;
using System.Data.SqlClient;

#endregion

namespace DatabaseInitializer.Sql
{
    public class DatabaseHelper
    {
        public static string CreateConnectionString(string server, bool integratedSecurity, string userId,
            string password)
        {
            return CreateConnectionString(server, null, integratedSecurity, userId, password);
        }

        public static string CreateConnectionString(string server, string database, bool integratedSecurity,
            string userId, string password)
        {
            if (integratedSecurity)
            {
                return CreateConnectionStringWithWindowsAuthentification(server, database);
            }

            return CreateConnectionStringWithSqlServerAuthentification(server, database, userId, password);
        }

        private static string CreateConnectionStringWithWindowsAuthentification(string server, string database)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = database ?? "master",
                IntegratedSecurity = true
            };

            return connectionStringBuilder.ConnectionString;
        }

        private static string CreateConnectionStringWithSqlServerAuthentification(string server, string database,
            string userId, string password)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = database ?? "master",
                IntegratedSecurity = false,
                UserID = userId,
                Password = password
            };

            return connectionStringBuilder.ConnectionString;
        }


        public static void ExecuteNonQuery(string connectionString, string cmdText)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var sqlCommandCreate = new SqlCommand(cmdText) {Connection = connection, CommandTimeout = 600};

                connection.Open();
                sqlCommandCreate.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static T ExecuteScalarQuery<T>(string connectionString, string cmdText)
        {
            object result;
            using (var connection = new SqlConnection(connectionString))
            {
                var sqlCommandCreate = new SqlCommand(cmdText) {Connection = connection, CommandTimeout = 600};

                connection.Open();
                result = sqlCommandCreate.ExecuteScalar();
                connection.Close();
            }
            return (T) result;
        }

        public static Nullable<T> ExecuteNullableScalarQuery<T>(string connectionString, string cmdText) where T : struct
        {
            object result;
            using (var connection = new SqlConnection(connectionString))
            {
                var sqlCommandCreate = new SqlCommand(cmdText) { Connection = connection, CommandTimeout = 600 };

                connection.Open();
                result = sqlCommandCreate.ExecuteScalar();
                connection.Close();
            }
            
            return result == DBNull.Value ? (Nullable<T>)null : (T)result;
        }
        public static IEnumerable<T> ExecuteListQuery<T>(string connectionString, string cmdText)
        {
            var result = new List<T>(); ;
            using (var connection = new SqlConnection(connectionString))
            {
                var sqlCommandCreate = new SqlCommand(cmdText) { Connection = connection, CommandTimeout = 600 };

                connection.Open();
                var reader = sqlCommandCreate.ExecuteReader();

                while (reader.Read())
                {
                    var v = (T) reader.GetValue(0); //The 0 stands for "the 0'th column", so the first column of the result.
                    // Do somthing with this rows string, for example to put them in to a list
                    result.Add(v);
                }

                connection.Close();
            }
            return result;
        }
    }
}