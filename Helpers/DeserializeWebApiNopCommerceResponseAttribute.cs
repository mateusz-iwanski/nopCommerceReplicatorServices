using System;

/// <summary>
/// <c>DeserializeWebApiNopCommerceResponseAttribute</c> is used to deserialize the response from the Web Api server
/// </summary>
/// <remarks>
/// It works with the DeserializeWebApiNopCommerceResponseAsync.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public class DeserializeWebApiNopCommerceResponseAttribute : Attribute
{
}