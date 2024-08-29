using System;

/// <summary>
/// <c>DeserializeResponseAttribute</c> is used to deserialize the response from the Web Api server
/// </summary>
/// <remarks>
/// It works with the DeserializeResponseAsync.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public class DeserializeResponseAttribute : Attribute
{
}