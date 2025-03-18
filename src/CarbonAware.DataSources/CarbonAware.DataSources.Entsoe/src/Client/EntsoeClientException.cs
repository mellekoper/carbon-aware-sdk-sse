using System;
using System.Net.Http;

namespace CarbonAware.DataSources.Entsoe.Exceptions;

/// <summary>
/// Exception class for handling ENTSO-E API errors.
/// </summary>
public class EntsoeClientException : Exception
{
    public HttpResponseMessage? Response { get; }

    public EntsoeClientException(string message) : base(message) { }

    public EntsoeClientException(string message, HttpResponseMessage response) : base(message)
    {
        Response = response;
    }
}
