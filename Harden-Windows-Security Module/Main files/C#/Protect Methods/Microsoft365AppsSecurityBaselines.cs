using System;
using System.Diagnostics;
using System.IO;

#nullable enable

namespace HardenWindowsSecurity
{
    public class Microsoft365AppsSecurityBaselines
    {
        /// <summary>
        /// Runs the Microsoft 365 Apps Security Baseline category
        /// </summary>
        public static void Invoke()
        {

            if (HardenWindowsSecurity.GlobalVars.Microsoft365SecurityBaselinePath == null)
            {
                throw new Exception("The path to the Microsoft 365 Apps Security Baseline has not been set.");
            }

            HardenWindowsSecurity.Logger.LogMessage("Applying the Microsoft 365 Apps Security Baseline", LogTypeIntel.Information);
            HardenWindowsSecurity.Logger.LogMessage("Running the official PowerShell script included in the Microsoft 365 Apps Security Baseline file downloaded from Microsoft servers", LogTypeIntel.Information);

            string M365AppsBaselineScriptPath = Path.Combine(
               HardenWindowsSecurity.GlobalVars.Microsoft365SecurityBaselinePath,
               "Scripts",
               "Baseline-LocalInstall.ps1"
           );

            // Get the directory of the script
            string scriptDirectory = Path.GetDirectoryName(M365AppsBaselineScriptPath)!;

            // Set up the PowerShell command to be executed
            string Command = $"""
Set-Location -Path "{scriptDirectory}"; .\Baseline-LocalInstall.ps1 4>&1
""";

            PowerShellExecutor.ExecuteScript(Command, false, true);

        }
    }
}
