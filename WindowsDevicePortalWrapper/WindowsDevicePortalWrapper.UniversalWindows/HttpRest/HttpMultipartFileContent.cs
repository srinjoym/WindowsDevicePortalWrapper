//----------------------------------------------------------------------------------------------
// <copyright file="HttpMultipartFileContent.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <summary>
    /// This class mimicks <see cref="HttpMultipartContent"/>, with two main differences
    /// 1. Simplifies posting files by taking file names instead of managing streams.
    /// 2. Does not quote the boundaries, due to a bug in the device portal
    /// </summary>
    internal sealed class HttpMultipartFileContent : IHttpContent
    {
        private class FileInfo
        {
            public StorageFile StorageFile { get; set; }
            public string Path { get; set; }
            public string Name { get; set; }
            public string FileType { get; set; }
            public ulong Length { get; set; }
        }

        /// <summary>
        /// List of items to transfer
        /// </summary>
        private List<FileInfo> items = new List<FileInfo>();

        /// <summary>
        /// Boundary string
        /// </summary>
        private string boundaryString;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpMultipartFileContent" /> class.
        /// </summary>
        public HttpMultipartFileContent() : this(Guid.NewGuid().ToString())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpMultipartFileContent" /> class.
        /// </summary>
        /// <param name="boundary">The boundary string for file content.</param>
        public HttpMultipartFileContent(string boundary)
        {
            this.boundaryString = boundary;
            this.Headers.ContentType = new HttpMediaTypeHeaderValue(string.Format("multipart/form-data; boundary={0}", this.boundaryString));
        }

        /// <summary>
        /// Gets the Http Headers
        /// </summary>
        public HttpContentHeaderCollection Headers { get; } = new HttpContentHeaderCollection();

        /// <summary>
        /// Adds a file to the list of items to transfer
        /// </summary>
        /// <param name="filename">The name of the file to add</param>
        public async Task Add(string filename)
        {
            // TODO - revert this code back to File.IO stuff for non Windows Universal apps
            if (filename != null)
            {
                var storageFile = await StorageFile.GetFileFromPathAsync(filename);
                var properties = await storageFile.GetBasicPropertiesAsync();

                this.items.Add(new FileInfo()
                {
                    Path = filename,
                    Name = storageFile.Name,
                    FileType = storageFile.FileType,
                    Length = properties.Size,
                });
            }
        }

        /// <summary>
        /// Adds a file to the list of items to transfer
        /// </summary>
        /// <param name="storageFile">File to add</param>
        public async Task Add(StorageFile storageFile)
        {
            var properties = await storageFile.GetBasicPropertiesAsync();

            this.items.Add(new FileInfo()
            {
                StorageFile = storageFile,
                Path = storageFile.Path,
                Name = storageFile.Name,
                FileType = storageFile.FileType,
                Length = properties.Size,
            });
        }

        /// <summary>
        /// Adds a range of files to the list of items to transfer
        /// </summary>
        /// <param name="filenames">List of files to add</param>
        public async Task AddRange(IEnumerable<string> filenames)
        {
            if (filenames != null)
            {
                foreach(var filename in filenames)
                {
                    await this.Add(filename);
                }
            }
        }

        /// <summary>
        /// Adds a range of files to the list of items to transfer
        /// </summary>
        /// <param name="storageFiles">List of storage files to add</param>
        public async Task AddRange(IEnumerable<StorageFile> storageFiles)
        {
            if (storageFiles != null)
            {
                foreach(var storageFile in storageFiles)
                {
                    await this.Add(storageFile);
                }
            }
        }

        /// <summary>
        /// This method is unimplemented.
        /// </summary>
        /// <returns>Throws an exception</returns>
        IAsyncOperationWithProgress<ulong, ulong> IHttpContent.BufferAllAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Dispose method for cleanup
        /// </summary>
        void IDisposable.Dispose()
        {
            this.items.Clear();
        }

        /// <summary>
        /// This method is unimplemented.
        /// </summary>
        /// <returns>Throws an exception</returns>
        IAsyncOperationWithProgress<IBuffer, ulong> IHttpContent.ReadAsBufferAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is unimplemented.
        /// </summary>
        /// <returns>Throws an exception</returns>
        IAsyncOperationWithProgress<IInputStream, ulong> IHttpContent.ReadAsInputStreamAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is unimplemented.
        /// </summary>
        /// <returns>Throws an exception</returns>
        IAsyncOperationWithProgress<string, ulong> IHttpContent.ReadAsStringAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Computes required length for the transfer.
        /// </summary>
        /// <param name="length">The computed length value</param>
        /// <returns>Whether or not the length was successfully computed</returns>
        bool IHttpContent.TryComputeLength(out ulong length)
        {
            length = 0;
            var boundaryLength = (ulong)Encoding.ASCII.GetBytes(string.Format("--{0}\r\n", this.boundaryString)).Length;
            foreach (var item in this.items)
            {
                var headerdata = GetFileHeader(item.Name, item.FileType);
                length += boundaryLength + (ulong)headerdata.Length + item.Length + 2;
            }

            length += (ulong)(boundaryLength + 2);
            return true;
        }

        /// <summary>
        /// Serializes the stream.
        /// </summary>
        /// <param name="outputStream">Serialized Stream</param>
        /// <returns>Task tracking progress</returns>
        IAsyncOperationWithProgress<ulong, ulong> IHttpContent.WriteToStreamAsync(IOutputStream outputStream)
        {
            return System.Runtime.InteropServices.WindowsRuntime.AsyncInfo.Run<ulong, ulong>((token, progress) =>
            {
                return WriteToStreamAsyncTask(outputStream, (ulong p) => progress.Report(p));
            });
        }

        /// <summary>
        /// Gets the file header for the transfer
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="fileType">Type of the file</param>
        /// <returns>A byte array with the file header information</returns>
        private static byte[] GetFileHeader(string fileName, string fileType)
        {
            string contentType = "application/octet-stream";
            if (fileType.ToLower() == ".cer")
            {
                contentType = "application/x-x509-ca-cert";
            }

            return Encoding.ASCII.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{0}\"\r\nContent-Type: {1}\r\n\r\n", fileName, contentType));
        }

        /// <summary>
        /// Serializes the stream.
        /// </summary>
        /// <param name="outputStream">Serialized Stream</param>
        /// <param name="progress">Progress tracking</param>
        /// <returns>Task tracking progress</returns>
        private async Task<ulong> WriteToStreamAsyncTask(IOutputStream outputStream, Action<ulong> progress)
        {
            ulong bytesWritten = 0;
            var outStream = outputStream.AsStreamForWrite();
            var boundary = Encoding.ASCII.GetBytes($"--{boundaryString}\r\n");
            var newline = Encoding.ASCII.GetBytes("\r\n");
            foreach (var item in this.items)
            {
                outStream.Write(boundary, 0, boundary.Length);
                bytesWritten += (ulong)boundary.Length;
                var headerdata = GetFileHeader(item.Name, item.FileType);
                outStream.Write(headerdata, 0, headerdata.Length);
                bytesWritten += (ulong)headerdata.Length;
                // TODO - this is goofy.  Probably just have a different HttpMultipartFileContent class that deals with StoargeFiles and one that deals with filenames
                var storageFile = item.StorageFile != null ? item.StorageFile : await StorageFile.GetFileFromPathAsync(item.Path);
                using (var readStream = await storageFile.OpenReadAsync())
                {
                    var stream = readStream.AsStream();
                    await stream.CopyToAsync(outStream);
                    bytesWritten += (ulong)stream.Position;
                }

                outStream.Write(newline, 0, newline.Length);
                bytesWritten += (ulong)newline.Length;
                await outStream.FlushAsync();
                progress(bytesWritten);
            }

            // Close the installation request data.
            boundary = Encoding.ASCII.GetBytes($"--{boundaryString}--\r\n");
            outStream.Write(boundary, 0, boundary.Length);
            await outStream.FlushAsync();
            bytesWritten += (ulong)boundary.Length;
            return bytesWritten;
        }
    }
}
