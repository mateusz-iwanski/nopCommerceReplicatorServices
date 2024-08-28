using nopCommerceReplicatorServices.SubiektGT;
using nopCommerceWebApiClient.Helpers;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

public static class AttributeHelper
{
    /// <summary>
    /// <c>CheckAndDeserializeResponseAsync</c> is used to deserialize responses from the API
    /// <example>
    /// <code>
    /// [DeserializeResponse]
    /// CreatePLById(customerId);
    /// ...
    /// var response = await customerService.CreatePLById(customerId);
    /// var methodInfo = typeof(CustomerGT).GetMethod("CreatePLById");
    /// await AttributeHelper.CheckAndDeserializeResponseAsync(methodInfo, response);
    /// </code>
    /// </example>
    /// </summary>
    /// <remarks>
    /// Check the status code and deserialize the API response to ValidationErrorResponse if the response fails.
    /// Executed after executing the function with the attribute DeserializeResponse
    /// </remarks>
    public static async Task CheckAndDeserializeResponseAsync(MethodInfo method, HttpResponseMessage response)
    {
        if (method.GetCustomAttribute<DeserializeResponseAttribute>() != null)
        {
            Console.WriteLine($"Check response method: {method.Name}");

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
            else
            {
                var statusCode = (int)response.StatusCode;
                Console.WriteLine($"Status code: {statusCode}");
            }

        }
    }
}