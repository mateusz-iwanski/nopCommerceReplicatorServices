﻿using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NLog.Web;
using nopCommerceReplicatorServices.Services;
using nopCommerceWebApiClient;
using nopCommerceWebApiClient.Helpers;
using nopCommerceWebApiClient.Interfaces.Address;
using nopCommerceWebApiClient.Interfaces.Customer;
using nopCommerceWebApiClient.Objects.Customer;
using Refit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace nopCommerceReplicatorServices.SubiektGT
{
    /// <summary>
    /// Represents a customer in Subiekt GT
    /// </summary>
    public class CustomerGT : ICustomerSourceData
    {        
        private DBConnector dbConnector { get; set; }

        public CustomerGT()
        {
            dbConnector = new DBConnector("SubiektGTConnection", "mssql");
            dbConnector.Initialize();
            return;
        }

        public CustomerDto? GetById(int customerId)
        {
            return Get("kH_Id", customerId.ToString())?.FirstOrDefault();
        }

        /// <summary>
        /// Gets a customer by a specified field and value.
        /// </summary>
        /// <param name="fieldName">The field name to query by.</param>
        /// <param name="fieldValue">The field value to query by.</param>
        /// <returns>A CustomerDto object if found; otherwise, null.</returns>
        public IEnumerable<CustomerDto>? Get(string fieldName, object fieldValue)
        {
            List<CustomerDto> customers = new List<CustomerDto>();

            var query = @$"
                SELECT
                    kH_Id,
		            kh_Imie,
		            kh_Nazwisko,
                    kh_Symbol,
                    kh_email,
                    adr_Nazwa,
                    adr_NIP,
                    adr_Adres,
                    adr_Kod,
                    adr_Telefon,
                    adr_Miejscowosc,
                    woj_nazwa
                FROM kh__Kontrahent  INNER JOIN adr__Ewid  ON kh_Id = adr_IdObiektu AND adr_TypAdresu=1
                LEFT JOIN sl_Wojewodztwo  on woj_id=adr_idwojewodztwo WHERE {fieldName} = '{fieldValue}';
            ";            

            dbConnector.OpenConnection();


            dbConnector.ExecuteQuery(query, (reader) =>
            {
                while (reader.Read())
                {
                    if (string.IsNullOrEmpty(reader.GetString(reader.GetOrdinal("kh_email"))))
                    {
                        Console.WriteLine("A Subiekt GT customer must have an email address to be added to nopCommerce");
                    }
                    else
                    {                        
                        var customer = new CustomerDto
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("kH_Id")),
                            City = reader.IsDBNull("adr_Miejscowosc") ? null : reader.GetString(reader.GetOrdinal("adr_Miejscowosc")),
                            Company = reader.IsDBNull("adr_NIP") ? null : reader.GetString(reader.GetOrdinal("adr_NIP")),
                            County = reader.IsDBNull("woj_nazwa") ? null : reader.GetString(reader.GetOrdinal("woj_nazwa")),
                            Email = reader.GetString(reader.GetOrdinal("kh_email")),
                            FirstName = reader.IsDBNull("kh_Imie") ? null : reader.GetString(reader.GetOrdinal("kh_Imie")),
                            LastName = reader.IsDBNull("kh_Nazwisko") ? null : reader.GetString(reader.GetOrdinal("kh_Nazwisko")),
                            Phone = reader.IsDBNull("adr_Telefon") ? null : reader.GetString(reader.GetOrdinal("adr_Telefon")),
                            StreetAddress = reader.IsDBNull("adr_Adres") ? null : reader.GetString(reader.GetOrdinal("adr_Adres")),
                            StreetAddress2 = null,
                            Username = reader.GetString(reader.GetOrdinal("kh_email")), // default username is email
                            ZipPostalCode = reader.IsDBNull("adr_Kod") ? null : reader.GetString(reader.GetOrdinal("adr_Kod")),
                        };

                        customers.Add(customer);
                    }

                }
            });

            dbConnector.CloseConnection();

            return customers.Count > 0 ? customers : null;
        }

    }
}
