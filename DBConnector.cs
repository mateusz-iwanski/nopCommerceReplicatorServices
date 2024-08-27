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
    public class DBConnector
    {
        private readonly string _connectionString;
        private SqlConnection _connection;

        public DBConnector(string connectionStringFromSettings)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json", optional: false, reloadOnChange: true)
                .Build();

            _connectionString = configuration.GetSection("DbConnectionStrings").GetValue<string>(connectionStringFromSettings);

            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("The connection string is not initialized.");
            }
        }

        public void InitializeAndOpenConnection()
        {
            _connection = new SqlConnection(_connectionString);
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
