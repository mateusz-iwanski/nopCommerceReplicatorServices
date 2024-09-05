using nopCommerceReplicatorServices.Services;
using nopCommerceWebApiClient.Objects.Customer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.Django
{
    public class CustomerDjango : ICustomerSourceData
    {
        private DBConnector dbConnector { get; set; }

        public CustomerDjango()
        {
            dbConnector = new DBConnector("Django", "postgresql");
            dbConnector.Initialize();
            return;
        }

        public IEnumerable<CustomerDto>? Get(string fieldName, object fieldValue)
        {

            var products = new List<CustomerDto>();

            var query = $@"
                SELECT 	
	                DISTINCT email,
                    auth_user.id, 
                    username, first_name, last_name, email, 
                    nazwa_firmy, nip, 
                    miasto, ulica, nr_budynku, nr_lokalu, kod_pocztowy
                FROM auth_user FULL OUTER JOIN faktura_faktura au on public.auth_user.id = au.user_id
                WHERE 
                    is_superuser = false AND is_staff = false AND is_active = true 
                    AND {fieldName} = '{fieldValue}';
            ";

            dbConnector.OpenConnection();


            dbConnector.ExecuteQuery(query, (reader) =>
            {
                while (reader.Read())
                {
                    if (string.IsNullOrEmpty(reader.GetString(reader.GetOrdinal("email"))))
                    {
                        Console.WriteLine("A Django customer must have an email address to be added to nopCommerce");
                    }
                    else
                    {

                        StringBuilder _streetAddress = new StringBuilder();
                        _streetAddress.Append(reader.IsDBNull("ulica") ? "" : reader.GetString(reader.GetOrdinal("ulica")));
                        _streetAddress.Append(reader.IsDBNull("nr_budynku") ? "" : $" {reader.GetString(reader.GetOrdinal("nr_budynku"))}");
                        if (!reader.IsDBNull("nr_lokalu"))
                        {
                            _streetAddress.AppendFormat("/{0}", reader.GetString(reader.GetOrdinal("nr_lokalu")));
                        }


                        var product = new CustomerDto
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            City = reader.IsDBNull("miasto") ? null : reader.GetString(reader.GetOrdinal("miasto")),
                            Company = reader.IsDBNull("nazwa_firmy") ? null : reader.GetString(reader.GetOrdinal("nazwa_firmy")),
                            County = null,
                            Email = reader.GetString(reader.GetOrdinal("email")),
                            FirstName = reader.IsDBNull("first_name") ? null : reader.GetString(reader.GetOrdinal("first_name")),
                            LastName = reader.IsDBNull("last_name") ? null : reader.GetString(reader.GetOrdinal("last_name")),                            
                            Phone = null,
                            StreetAddress = reader.IsDBNull("ulica") ? null : _streetAddress.ToString(),
                            StreetAddress2 = null,
                            Username = reader.GetString(reader.GetOrdinal("email")), 
                            ZipPostalCode = reader.IsDBNull("kod_pocztowy") ? null : reader.GetString(reader.GetOrdinal("kod_pocztowy")),
                        };

                        products.Add(product);
                    }

                }
            });

            dbConnector.CloseConnection();

            return products.Count > 0 ? products : null;
        }

        public CustomerDto? GetById(int customerId)
        {
            return Get("auth_user.id", customerId.ToString())?.FirstOrDefault();
        }
    }
}
