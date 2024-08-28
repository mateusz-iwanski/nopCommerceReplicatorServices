using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data.Common;
using System.IO;

namespace nopCommerceReplicatorServices
{
    /// <summary>
    /// <c>DBConnector</c> is a class that provides a connection to the database.
    /// </summary>
    public class DBConnector
    {
        private readonly string _connectionString;
        private SqlConnection _connection { get; set; }

        public DBConnector(string connectionStringFromSettings)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json", optional: false, reloadOnChange: true)
                .Build();

            _connectionString = configuration.GetSection("DbConnectionStrings").GetValue<string>(connectionStringFromSettings);

            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("The connection string has not been initialized.");
            }
        }

        public void Initialize()
        {
            _connection = new SqlConnection(_connectionString);
        }

        public void OpenConnection()
        {
            if (_connection == null)
                throw new Exception("DBConnector has no initialize connection. Use Initialize() before OpenConnection()");
            _connection.Open();
        }

        public SqlDataReader ExecuteQuery(string query)
        {
            using (SqlCommand command = new SqlCommand(query, _connection))
            {
                return command.ExecuteReader();
            }
        }

        public void ExecuteNonQuery(string query)
        {
            using (SqlCommand command = new SqlCommand(query, _connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public void CloseConnection()
        {
            _connection.Close();
        }
    }
}
