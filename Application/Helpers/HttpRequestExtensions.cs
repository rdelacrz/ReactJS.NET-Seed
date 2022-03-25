using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Web;

namespace Application.Helpers
{
    /// <summary>
    /// Contains HTTP Request functions that extend existing API Controller functionality.
    /// </summary>
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Generates HTTP Response Message for file data to be sent.
        /// </summary>
        /// <param name="fileName">Name of file.</param>
        /// <param name="fileData">File data (in bytes).</param>
        /// <returns>HTTP Response Message object containing file name, file data, and other appropriate attributes related to the file.</returns>
        public static HttpResponseMessage GenerateFileResponse(string fileName, byte[] fileData)
        {
            // Determines file stream parameters for the download
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(fileData)
            };
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };

            // Determines content type of file (this will determine how browser opens file)
            string mimeType = MimeMapping.GetMimeMapping(fileName);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

            return response;
        }
    }
}