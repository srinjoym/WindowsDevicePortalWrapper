// <copyright file="HolographicOs.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Wrappers for Holographic Kiosk Mode settings. Note that these are only
    /// available on Enterprise SKUs of HoloLens
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API for sending a virtual key down
        /// </summary>
        public static readonly string HolographicSendVkeyDownApi = "api/holographic/input/keyboard/vkey/down";

        /// <summary>
        /// API for sending a virtual key up
        /// </summary>
        public static readonly string HolographicSendVkeyUpApi = "api/holographic/input/keyboard/vkey/up";

        /// <summary>
        /// API for sending text
        /// </summary>
        public static readonly string HolographicSendTextApi = "api/holographic/kioskmode/settings";

        /// <summary>
        /// Sends a virtual key up or down to the device
        /// </summary>
        /// <returns>Task for tracking the POST call</returns>
        public async Task SendVkey(bool down, int code)
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            string api = down ? HolographicSendVkeyDownApi : HolographicSendVkeyUpApi;

            await this.PostAsync(api, "code=" + code);
        }

        /// <summary>
        /// Sends text as keyboard input on the HoloLens
        /// </summary>
        /// <returns>Task for tracking the POST call</returns>
        public async Task SendText(string text)
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            await this.PostAsync(
                HolographicKioskModeSettingsApi,
                "text=" + text);
        }
    }
}
