using Microsoft.Extensions.Configuration;
using System;
using System.Data.Common;
using System.IO;
using Npgsql; // For PostgreSQL
using System.Data.SqlClient;
using nopCommerceReplicatorServices.Exceptions; // For MSSQL

namespace nopCommerceReplicatorServices
{
    /// <summary>
    /// <c>DBConnector</c> is a class that provides a connection to SQL the database.
    /// </summary>
    public class DBConnector
    {
        private readonly string _connectionString;
        private readonly string _dbType;
        public DbConnection _connection { get; private set; }

        /// <summary>
        /// Constructor for DBConnector 
        /// </summary>
        /// <param name="connectionStringFromSettings">read from settings</param>
        /// <param name="dbType">mssql, postgresql</param>
        public DBConnector(string connectionStringFromSettings, string dbType)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("settings.json", optional: false, reloadOnChange: true)
                .Build();

            _connectionString = configuration.GetSection("DbConnectionStrings").GetValue<string>(connectionStringFromSettings) ??
                    throw new CustomException($"In configuration DbConnectionStrings->{connectionStringFromSettings} not exists"); 

            _dbType = dbType;

            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("The connection string has not been initialized.");
            }

            if (string.IsNullOrEmpty(_dbType))
            {
                throw new InvalidOperationException("The database type has not been specified.");
            }
        }

        public void Initialize()
        {
            _connection = CreateConnection(_dbType, _connectionString);
        }

        private DbConnection CreateConnection(string dbType, string connectionString)
        {
            return dbType.ToLower() switch
            {
                "mssql" => new SqlConnection(connectionString),
                "postgresql" => new NpgsqlConnection(connectionString),
                _ => throw new InvalidOperationException("Unsupported database type.")
            };
        }

        public void OpenConnection()
        {
            if (_connection == null)
                throw new Exception("DBConnector has no initialized connection. Use Initialize() before OpenConnection()");
            _connection.Open();
        }

        public void ExecuteQuery(string query, Action<DbDataReader> work)
        {
            using (DbCommand command = _connection.CreateCommand())
            {
                command.CommandText = query;
                using (DbDataReader reader = command.ExecuteReader())
                {
                    work(reader);
                }
            }
        }

        public void ExecuteNonQuery(string query)
        {
            using (DbCommand command = _connection.CreateCommand())
            {
                command.CommandText = query;
                command.ExecuteNonQuery();
            }
        }

        public void CloseConnection()
        {
            _connection.Close();
        }
    }
}