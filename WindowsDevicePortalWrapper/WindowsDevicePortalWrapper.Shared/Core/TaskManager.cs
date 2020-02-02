//----------------------------------------------------------------------------------------------
// <copyright file="TaskManager.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// Wrappers for Task Manager methods
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API for starting or stopping a modern application.
        /// </summary>
        public static readonly string TaskManagerApi = "api/taskmanager/app";

        /// <summary>
        /// API for killing a process directly.
        /// </summary>
        public static readonly string TaskManagerProcessApi = "api/taskmanager/process";

        /// <summary>
        /// Starts running the specified application.
        /// </summary>
        /// <param name="appid">Application ID</param>
        /// <param name="packageName">The name of the application package.</param>
        /// <returns>Process identifier for the application instance.</returns>
        public async Task<uint> LaunchApplicationAsync(
            string appid,
            string packageName)
        {
            string payload = string.Format(
                "appid={0}&package={1}",
                Utilities.Hex64Encode(appid),
                Utilities.Hex64Encode(packageName));

            await this.PostAsync(
                TaskManagerApi,
                payload);

            RunningProcesses runningApps = await this.GetRunningProcessesAsync();

            uint processId = 0;
            foreach (DeviceProcessInfo process in runningApps.Processes)
            {
                if (string.Compare(process.PackageFullName, packageName) == 0)
                {
                    processId = process.ProcessId;
                    break;
                }
            }

            return processId;
        }

        /// <summary>
        /// Stops the specified application from running.
        /// </summary>
        /// <param name="packageName">The name of the application package.</param>
        /// <param name="forceStop">Force Stop the process</param>
        /// <returns>
        /// Task for tracking termination completion
        /// </returns>
        public async Task TerminateApplicationAsync(string packageName, bool forceStop = false)
        {
            var sbPayload = new StringBuilder();
            sbPayload.Append("package=");
            sbPayload.Append(Utilities.Hex64Encode(packageName));
            if (forceStop)
            {
                sbPayload.Append("&forcestop=yes");
            }

            await this.DeleteAsync(TaskManagerApi, sbPayload.ToString());
        }

        /// <summary>
        /// Stops the specified application from running.
        /// </summary>
        /// <param name="pid">The id of the process.</param>
        /// <returns>
        /// Task for tracking termination completion
        /// </returns>
        public async Task TerminateProcessAsync(uint pid)
        {
            await this.DeleteAsync(
                TaskManagerProcessApi,
                string.Format("pid={0}", pid));
        }
    }
}

