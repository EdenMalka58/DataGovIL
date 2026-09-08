using System.Net;
using DataGovIL.Client.Models;

namespace DataGovIL.Client;

/// <summary>
/// Thrown when the CKAN API returns success == false, or when the HTTP call itself fails.
/// </summary>
public class CkanApiException : Exception
{
    public HttpStatusCode? StatusCode { get; }
    public CkanError? CkanError { get; }

    public CkanApiException(string message, HttpStatusCode? statusCode = null, CkanError? ckanError = null, Exception? inner = null)
        : base(message, inner)
    {
        StatusCode = statusCode;
        CkanError = ckanError;
    }
}
