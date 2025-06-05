﻿using System.Text.Json;
using static UPV.CLI.Connectors.PowerShellHelper;

namespace UPV.CLI.Connectors.VPN
{
    public static class VPNPowerShell
    {
        public static bool Create(string name, string server, Action<PowerShellCommandBuilder>? configureCommand)
        {
            return CreateAsync(name, server, configureCommand).GetAwaiter().GetResult();
        }

        public static async Task<bool> CreateAsync(string name, string server, Action<PowerShellCommandBuilder>? configureCommand)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrEmpty(server)) throw new ArgumentNullException(nameof(server));

            var script = BuildAddVpnScript(name, server, configureCommand);
            return await ExecutePowerShellScript(script);
        }

        private static string BuildAddVpnScript(string name, string server, Action<PowerShellCommandBuilder>? configureCommand)
        {
            var pb = new PowerShellCommandBuilder();
            pb.AddCommand("Add-VpnConnection");
            pb.AddParameter("Name", name);
            pb.AddParameter("ServerAddress", server);
            pb.AddParameter("RememberCredential", true);

            configureCommand?.Invoke(pb);

            return pb.Build();
        }

        public static bool Any(string server) => AnyAsync(server).GetAwaiter().GetResult();

        public static IEnumerable<string> FindNames(string server)
            => FindNamesAsync(server).GetAwaiter().GetResult();

        public static async Task<bool> AnyAsync(string server)
            => (await FindNamesAsync(server)).Any();

        public static async Task<IEnumerable<string>> FindNamesAsync(string server)
        {
            var script = $"Get-VpnConnection | Where-Object {{$_.ServerAddress -eq {EscapeString(server)}}} | Select-Object -ExpandProperty Name | ConvertTo-Json";

            var result = await ExecutePowerShellScriptWithOutput(script);
            if (string.IsNullOrWhiteSpace(result))
                return [];

            try
            {
                // Handle both single item (string) and array cases
                if (result.Trim().StartsWith('['))
                {
                    var names = JsonSerializer.Deserialize<string[]>(result);
                    return names ?? [];
                }
                else
                {
                    var name = JsonSerializer.Deserialize<string>(result);
                    return name != null ? [name] : [];
                }
            }
            catch (JsonException)
            {
                // Fallback: treat as plain text, split by lines
                return result.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                            .Select(line => line.Trim())
                            .Where(line => !string.IsNullOrEmpty(line));
            }
        }
    }
}