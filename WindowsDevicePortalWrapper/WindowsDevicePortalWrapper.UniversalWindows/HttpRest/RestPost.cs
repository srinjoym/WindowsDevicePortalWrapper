//----------------------------------------------------------------------------------------------
// <copyright file="RestPost.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Universal Windows Platform implementation of HTTP PostAsync
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Submits the http post request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the post request will be issued.</param>
        /// <param name="requestStream">Optional stream containing data for the request body.</param>
        /// <param name="requestStreamContentType">The type of that request body data.</param>
        /// <returns>Task tracking the completion of the POST request</returns>
#pragma warning disable 1998
        public async Task<Stream> PostAsync(
            Uri uri,
            Stream requestStream = null,
            string requestStreamContentType = null)
        {
            HttpStreamContent requestContent = null;
            
            if (requestStream != null)
            {
                requestContent = new HttpStreamContent(requestStream.AsInputStream());
                requestContent.Headers.Remove(ContentTypeHeaderName);
                requestContent.Headers.TryAppendWithoutValidation(ContentTypeHeaderName, requestStreamContentType);
            }

            return await this.PostAsync(uri, requestContent);
        }

        /// <summary>
        /// Calls the specified API with the provided body. This signature leaves
        /// off the optional response so callers who don't need a response body
        /// don't need to specify a type for it.
        /// </summary>
        /// <param name="apiPath">The relative portion of the uri path that specifies the API to call.</param>
        /// <param name="files">List of files that we want to include in the post request.</param>
        /// <param name="payload">The query string portion of the uri path that provides the parameterized data.</param>
        /// <returns>Task tracking the POST completion.</returns>
        public async Task PostAsync(
            string apiPath,
            List<StorageFile> files,
            string payload = null)
        {
            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                apiPath,
                payload);

            var content = new HttpMultipartFileContent();
            await content.AddRange(files);
            await this.PostAsync(uri, content);
        }

        /// <summary>
        /// Calls the specified API with the provided body. This signature leaves
        /// off the optional response so callers who don't need a response body
        /// don't need to specify a type for it.
        /// </summary>
        /// <param name="apiPath">The relative portion of the uri path that specifies the API to call.</param>
        /// <param name="files">List of files that we want to include in the post request.</param>
        /// <param name="payload">The query string portion of the uri path that provides the parameterized data.</param>
        /// <returns>Task tracking the POST completion.</returns>
        public async Task PostAsync(
            string apiPath,
            List<string> files,
            string payload = null)
        {
            throw new NotImplementedException("Posting using file names directly not supported on UWP.  Convert your code to use the StorageFile API");
        }

        /// <summary>
        /// Submits the http post request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the post request will be issued.</param>
        /// <param name="requestContent">Optional content for the request body.</param>
        /// <returns>Task tracking the completion of the POST request</returns>
        public async Task<Stream> PostAsync(
            Uri uri,
            IHttpContent requestContent)
        {
            IBuffer dataBuffer = null;
            HttpBaseProtocolFilter httpFilter = new HttpBaseProtocolFilter();
            httpFilter.AllowUI = false;

            if (this.deviceConnection.Credentials != null)
            {
                httpFilter.ServerCredential = new PasswordCredential();
                httpFilter.ServerCredential.UserName = this.deviceConnection.Credentials.UserName;
                httpFilter.ServerCredential.Password = this.deviceConnection.Credentials.Password;
            }

            using (HttpClient client = new HttpClient(httpFilter))
            {
                this.ApplyHttpHeaders(client, HttpMethods.Post);
                using (HttpResponseMessage response = await client.PostAsync(uri, requestContent))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw await DevicePortalException.CreateAsync(response);
                    }

                    this.RetrieveCsrfToken(response);

                    if (response.Content != null)
                    {
                        using (IHttpContent messageContent = response.Content)
                        {
                            dataBuffer = await messageContent.ReadAsBufferAsync();
                        }
                    }
                }
            }

            return (dataBuffer != null) ? dataBuffer.AsStream() : null;
        }
#pragma warning restore 1998
    }
}
