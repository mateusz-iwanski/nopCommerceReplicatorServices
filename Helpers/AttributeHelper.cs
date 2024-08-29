using nopCommerceReplicatorServices.nopCommerce;
using nopCommerceReplicatorServices.SubiektGT;
using nopCommerceWebApiClient.Helpers;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

public static class AttributeHelper
{
    /// <summary>
    /// <c>DeserializeResponseAsync</c> is used to deserialize responses from the API
    /// <example>
    /// <code>
    /// [DeserializeResponse]
    /// CreatePLById(customerId);
    /// ...
    /// var response = await customerService.CreatePLById(customerId);
    /// var methodInfo = typeof(CustomerGT).GetMethod("CreatePLById");
    /// await AttributeHelper.DeserializeResponseAsync(methodInfo, response);
    /// </code>
    /// </example>
    /// </summary>
    /// <remarks>
    /// Useful for displaying process details and errors to the client
    /// </remarks>
    public static async Task DeserializeResponseAsync(string methodName, HttpResponseMessage response)
    {
        var method = typeof(CustomerNopCommerce).GetMethod(methodName);

        if (method.GetCustomAttribute<DeserializeResponseAttribute>() != null)
        {
            var url = response.RequestMessage.RequestUri.ToString();

            Console.WriteLine($"Check request method: {method.Name}");
            Console.WriteLine($"URL: {url}");

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var validationError = JsonSerializer.Deserialize<ValidationErrorResponse>(content);

                Console.WriteLine($"Validation Error method: {method.Name}");

                foreach (var error in validationError.Errors)
                {
                    Console.WriteLine($"Validation Error key: {error.Key}");

                    foreach (var value in error.Value)
                    { 
                        Console.WriteLine($"Validation Error value: {value}");
                    }
                }
            }
        }
    }
}