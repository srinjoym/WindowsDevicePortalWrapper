//----------------------------------------------------------------------------------------------
// <copyright file="AppFileExplorer.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Wrappers for Map Manager APIs
    /// </content>
    public partial class DevicePortal
    {
        public class MapFileResponse
        {
            long Date = 0;
            string Id = "";
            long Size = 0;
        }

        public class MapFilesResponse
        {
            MapFileResponse[] files = new MapFileResponse[] { };
        }

        /// <summary>
        /// API to upload a map
        /// </summary>
        public static readonly string UploadMapFileApi = "api/holographic/mapmanager/upload";

        /// <summary>
        /// API to upload a map
        /// </summary>
        public static readonly string ImportdMapFileApi = "api/holographic/mapmanager/import";

        /// <summary>
        /// API to export a map
        /// </summary>
        public static readonly string ExportMapFileApi = "api/holographic/mapmanager/export";

        /// <summary>
        /// API to download a map
        /// </summary>
        public static readonly string DownloadMapFileApi = "api/holographic/mapmanager/download";

        /// <summary>
        /// API to list maps
        /// </summary>
        public static readonly string ListMapFilesApi = "api/holographic/mapmanager/mapFiles";

        /// <summary>
        /// Import a head tracking map file to the HoloLens
        /// </summary>
        /// <param name="filename">Name of the tracking map file to import</param>
        /// <returns></returns>
        public async Task ImportMapManagerFileAsync(string filename)
        {
            try
            {
                await this.PostAsync(ImportdMapFileApi, $"FileName={filename}");
            }
            catch(SerializationException)
            {
                // Sigh - the response from this call is not proper .json.
                // so ignore this exception - everything likely worked

            }
        }

        public async Task ExportMapManagerFileAsync()
        {
            try
            {
                await this.PostAsync(ExportMapFileApi);
            }
            catch (SerializationException)
            {
                // Sigh - the response from this call is not proper .json.
                // so ignore this exception - everything likely worked

            }
        }

        public async Task<MapFilesResponse> ListMapsAsync()
        {
            return await this.GetAsync<MapFilesResponse>(ListMapFilesApi);
        }

        public async Task DownloadMapManagerFileAsync(string fileName)
        {
            try
            {
                Uri uri = new Uri(DownloadMapFileApi + $"?FileName={fileName}");
                Stream dataStream = await this.GetAsync(uri);
            }
            catch (SerializationException)
            {
                // Sigh - the response from this call is not proper .json.
                // so ignore this exception - everything likely worked

            }
        }
    }
}
