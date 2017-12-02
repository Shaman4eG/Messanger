using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Messenger.DataLayer.Sql
{
    [System.Diagnostics.DebuggerStepThrough]
    /// <summary>
    /// Useful methods within Messenger.Api.
    /// </summary>
    static class Utility
    {
        /// <summary>
        /// Throws HttpResponsException with info.
        /// </summary>
        /// <param name="statusCode"> Code of response </param>
        /// <param name="reasonPhrase"> Reason of response </param>
        /// <param name="content"> Content of response </param>
        internal static void GenerateResponseMessage(HttpStatusCode statusCode, string reasonPhrase, string content)
        {
            var response = new HttpResponseMessage()
            {
                StatusCode = statusCode,
                ReasonPhrase = reasonPhrase,
                Content = new StringContent(content)
            };
            throw new HttpResponseException(response);
        }
    }
}
