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
        /// API for getting the kiosk mode status
        /// </summary>
        public static readonly string HolographicKioskModeStatusApi = "api/holographic/kioskmode/status";

        /// <summary>
        /// API for changing the kiosk mode settings
        /// </summary>
        public static readonly string HolographicKioskModeSettingsApi = "api/holographic/kioskmode/settings";

        /// <summary>
        /// Gets the kiosk mode status of this HoloLens.
        /// </summary>
        /// <returns>KioskModeStatus object describing the state of kiosk mode.</returns>
        /// <remarks>This method is only supported on HoloLens.</remarks>
        public async Task<KioskModeStatus> GetKioskModeStatusAsync()
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            KioskModeStatus status;

            try
            {
                status = await this.GetAsync<KioskModeStatus>(HolographicKioskModeStatusApi);
                status.StatusAvailable = true;
            }
            catch (DevicePortalException)
            {
                status = new KioskModeStatus() { StatusAvailable = false, KioskModeEnabled = false, StartupAppPackageName = String.Empty };
            }
            return status;
        }

        /// <summary>
        /// Sets the kiosk mode settings of the device.
        /// </summary>
        /// <param name="kioskModeEnabled">True to enable kiosk mode</param>
        /// <param name="startupAppPackageName">Package of the startup app</param>
        /// <returns>Task for tracking the POST call</returns>
        /// <remarks>This method is only supported on HoloLens.</remarks>
        public async Task SetKioskModeSettingsAsync(bool kioskModeEnabled, string startupAppPackageName)
        {
            if (!Utilities.IsHoloLens(this.Platform, this.DeviceFamily))
            {
                throw new NotSupportedException("This method is only supported on HoloLens.");
            }

            StringBuilder payload = new StringBuilder("kioskModeEnabled=" + kioskModeEnabled.ToString().ToLower());
            if(!string.IsNullOrWhiteSpace(startupAppPackageName))
            {
                payload.Append("&startupApp=");
                payload.Append(startupAppPackageName);
            }

            await this.PostAsync(
                HolographicKioskModeSettingsApi,
                payload.ToString());
        }

        #region Data contract
        /// <summary>
        /// Object reporesentation of the status of kiosk mode
        /// </summary>
        [DataContract]
        public class KioskModeStatus
        {
            /// <summary>
            /// Set when we can get the status.  We can only get the status when the HoloLens is an Enterprise SKU
            /// </summary>
            public bool StatusAvailable { get; internal set; }

            /// <summary>
            /// Gets the status for the collection of holographic services
            /// </summary>
            [DataMember(Name = "KioskModeEnabled")]
            public bool KioskModeEnabled { get; internal set; }

            /// <summary>
            /// Gets the package name of the app that starts up in kiosk mode
            /// </summary>
            [DataMember(Name = "StartupApp")]
            public string StartupAppPackageName { get; internal set; }
        }
        #endregion // Data contract
    }
}
