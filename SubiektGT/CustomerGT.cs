using Microsoft.Extensions.Configuration;
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

namespace nopCommerceReplicatorServices.SubiektGT
{
    /// <summary>
    /// Represents a customer in Subiekt GT
    /// </summary>
    public class CustomerGT : ICustomer
    {        
        private ICustomerService _customerApi { get; set; }
        private DBConnector dbConnector { get; set; }
        public record kh__Kontrahent(
            int kh_Id, 
            string kh_Imie, 
            string kh_Nazwisko, 
            string kh_Symbol, 
            string kh_email, 
            string adr_Nazwa,
            string adr_NIP,
            string adr_Adres,
            string adr_Telefon,
            string adr_Miejscowosc,
            string woj_nazwa
            );

        public CustomerGT(IApiConfigurationServices apiServices)
        {
            _customerApi = apiServices.CustomerService;

            dbConnector = new DBConnector("SubiektGTConnection");
            dbConnector.Initialize();
        }

        protected CustomerCreatePLDto? GetCustomerFromSubiekt(int customerId)
        {

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
                LEFT JOIN sl_Wojewodztwo  on woj_id=adr_idwojewodztwo WHERE kH_Id = {customerId}
            ";

            dbConnector.OpenConnection();

            SqlDataReader reader = dbConnector.ExecuteQuery(query);

            if (reader.Read())
            {
                var product = new CustomerCreatePLDto
                {
                    City = reader.GetString(reader.GetOrdinal("adr_Miejscowosc")),
                    Company = string.IsNullOrEmpty(reader.GetString(reader.GetOrdinal("adr_NIP"))) ?
                                    null :
                                    reader.GetString(reader.GetOrdinal("adr_Miejscowosc")),
                    County = reader.GetString(reader.GetOrdinal("woj_nazwa")),
                    Email = reader.GetString(reader.GetOrdinal("kh_email")),
                    FirstName = reader.GetString(reader.GetOrdinal("kh_Imie")),
                    LastName = reader.GetString(reader.GetOrdinal("kh_Nazwisko")),
                    Password = reader.GetString(reader.GetOrdinal("kh_email")),  // default password is email
                    Phone = reader.GetString(reader.GetOrdinal("adr_Telefon")),
                    StreetAddress = reader.GetString(reader.GetOrdinal("adr_Adres")),
                    StreetAddress2 = null,
                    Username = reader.GetString(reader.GetOrdinal("kh_email")),
                    ZipPostalCode = reader.GetString(reader.GetOrdinal("adr_Kod")),
                };

                dbConnector.CloseConnection();

                return product;
            }

            dbConnector.CloseConnection();

            return null;
        }

        [DeserializeResponse]
        public async Task<HttpResponseMessage> CreatePLById(int customerId)
        {
            var customer = GetCustomerFromSubiekt(customerId) ?? throw new ArgumentException($"The Subiekt GT customer with the ID {customerId} does not exist");
            var response = await CreatePL(customer);
            return response;
        }

        [DeserializeResponse]
        public async Task<HttpResponseMessage> CreatePL(CustomerCreatePLDto customer)
        {
            var response = await _customerApi.CreatePLAsync(customer);

            return response;
        }

    }
}
