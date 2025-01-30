using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PSAppDeployToolkit.CustomRunspaceDemo
{
    internal class Program
    {
        private static void WriteLogEntry(string message)
        {
            // Write a log entry to the console.
            Console.WriteLine($"[{DateTime.Now.ToString("O")}] :: {message}");
        }

        static void Main()
        {
            // Set up the initial session state for the new runspace.
            WriteLogEntry("Setting up the initial session state for the new runspace.");
            var iss = InitialSessionState.CreateDefault();

            // Import the PSAppDeployToolkit module into the initial session state.
            WriteLogEntry("Importing the PSAppDeployToolkit module into the initial session state.");
            iss.ImportPSModule(new[] { "D:\\Repos\\PSAppDeployToolkit\\src\\PSAppDeployToolkit\\PSAppDeployToolkit.psd1" });

            // Create a new runspace with the initial session state.
            WriteLogEntry("Creating a new runspace with the initial session state.");
            using (var rs = RunspaceFactory.CreateRunspace(iss))
            {
                // Open the runspace to enable the use of the session state.
                WriteLogEntry("Opening the runspace to enable the use of the session state.");
                rs.Open();

                // Create a new PowerShell instance with our runspace.
                WriteLogEntry("Creating a new PowerShell instance.");
                using (var ps = PowerShell.Create())
                {
                    // Set the runspace for the PowerShell instance.
                    WriteLogEntry("Setting the runspace for the PowerShell instance.");
                    ps.Runspace = rs;

                    // Define the script to be executed.
                    string script = "Get-Command -Name Initialize-ADTFunction -Module PSAppDeployToolkit | Out-String; Set-ADTRegistryKey -Key 'HKEY_CURRENT_USER\\SOFTWARE\\PSAppDeployToolkit.CustomRunspaceDemo' -Name 'Testing' -Type 'String' -Value 'This is a test'";

                    // Perform the invocation.
                    try
                    {
                        // Add the script to the PowerShell instance and invoke it.
                        WriteLogEntry($"Adding the script [{script}] to the PowerShell instance and invoking it.");
                        var output = ps.AddScript(script).Invoke();

                        // Throw if we have any errors from the invocation.
                        if (ps.HadErrors)
                        {
                            throw ps.Streams.Error.First().Exception;
                        }

                        // Print any output to the console.
                        WriteLogEntry("Invocation has completed with zero errors. Captured output:");
                        foreach (var item in output)
                        {
                            Console.WriteLine(item);
                        }
                    }
                    catch
                    {
                        // Rethrow any exceptions that occurred during the invocation.
                        throw;
                    }
                    finally
                    {
                        // Close the runspace.
                        WriteLogEntry("Closing the runspace and exiting process.");
                        ps.Runspace.Close();
                    }
                }
            }
        }
    }
}
