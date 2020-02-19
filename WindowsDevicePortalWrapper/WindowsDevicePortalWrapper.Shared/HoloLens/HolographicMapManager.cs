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
        /// <summary>
        /// API to upload a map
        /// </summary>
        public static readonly string UploadMapFileApi = "api/holographic/mapmanager/upload";

        /// <summary>
        /// API to upload a map
        /// </summary>
        public static readonly string ImportdMapFileApi = "api/holographic/mapmanager/import";

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

    }
}
