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
using Windows.Storage;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Wrappers for App File explorer methods
    /// </content>
    public partial class DevicePortal
    {

        /// <summary>
        /// Uploads a file to a Known Folder (e.g. LocalAppData)
        /// </summary>
        /// <param name="knownFolderId">The known folder id for the root of the path.</param>
        /// <param name="storageFile">StorageFile for the file we are uploading.</param>
        /// <param name="subPath">An optional subpath to the folder.</param>
        /// <param name="packageFullName">The package full name if using LocalAppData.</param>
        /// <returns>Task tracking completion of the upload request.</returns>
        public async Task UploadFileAsync(
            string knownFolderId,
            StorageFile storageFile,
            string subPath = null,
            string packageFullName = null)
        {
            Dictionary<string, string> payload = this.BuildCommonFilePayload(knownFolderId, subPath, packageFullName);

            List<StorageFile> files = new List<StorageFile>();
            files.Add(storageFile);

            await this.PostAsync(GetFileApi, files, Utilities.BuildQueryString(payload));
        }
    }
}
