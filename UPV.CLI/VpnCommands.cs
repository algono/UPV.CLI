﻿using Cocona;
using System.Diagnostics;
using UPV.CLI.Connectors.Helpers;
using UPV.CLI.Connectors.VPN;

namespace UPV.CLI
{
    // VPN Commands
    public class VpnCommands
    {
        [Command("create")]
        public void Create([Argument] string name, [Option(Description = "Automatically try to connect to the VPN after creating it")] bool connect = false)
        {
            Console.WriteLine($"Creating VPN connection: {name}");
            var process = VPNHelper.Create(name);

            if (process == null)
            {
                Console.Error.WriteLine($"Failed to start creation process for VPN connection: {name}");
                Environment.Exit(1);
                return;
            }

            var result = CmdHelper.WaitAndCheck(process);

            if (result.Succeeded)
            {
                Console.WriteLine($"Successfully created VPN connection: {name}");

                if (connect)
                {
                    Connect(name); // Automatically connect after creation (if the connect option is specified)
                }
            }
            else
            {
                Console.Error.WriteLine($"Failed to add VPN connection: {name}");
                Debug.WriteLine($"Failed to add VPN connection: {name}\n\nFull error:\n{result.Error}");
                Environment.Exit(1);
                return;
            }
        }

        [Command("delete")]
        public void Delete([Argument] string name, [Option(Description = "Delete without asking for confirmation")] bool force = false)
        {
            if (!(force || CommandsHelper.GetYesNoConfirmation($"Are you sure you want to delete the VPN connection '{name}'? This action cannot be undone.")))
            {
                Console.WriteLine("Deletion cancelled.");
                return;
            }

            Console.WriteLine($"Deleting VPN connection: {name}");
            var process = VPNHelper.Delete(name);

            if (process == null)
            {
                Console.Error.WriteLine($"Failed to start deletion process for VPN connection: {name}");
                Environment.Exit(1);
                return;
            }

            var result = CmdHelper.WaitAndCheck(process);

            if (result.Succeeded)
            {
                Console.WriteLine($"Successfully deleted VPN connection: {name}");
            }
            else
            {
                Console.Error.WriteLine($"Failed to delete VPN connection: {name}\nError: {result.Error}");
                Environment.Exit(1);
                return;
            }
        }

        [Command("connect")]
        public void Connect([Argument] string name)
        {
            Console.WriteLine($"Connecting to VPN: {name}");
            try
            {
                var process = VPNHelper.Connect(name);

                var result = CmdHelper.WaitAndCheck(process);

                if (result.Succeeded)
                {
                    Console.WriteLine($"Successfully opened connection dialog for VPN: {name}.\nCheck your VPN settings to see if the connection was successful or not.\nAlternatively, you can use the \"{typeof(VpnCommands).Assembly.GetName().Name} vpn check\" command to see if your current IP is in the UPV network.");
                }
                else
                {
                    Console.Error.WriteLine($"Failed to connect to VPN: {name}\nError: {result.Error}");
                    Environment.Exit(1);
                    return;
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.Error.WriteLine($"Failed to start connection process for VPN: {name}\nError: {ex.Message}");
                Environment.Exit(1);
                return;
            }
        }

        [Command("disconnect")]
        public void Disconnect([Argument] string name)
        {
            Console.WriteLine($"Disconnecting from VPN: {name}");
            try
            {
                var process = VPNHelper.Disconnect(name);

                var result = CmdHelper.WaitAndCheck(process);

                if (result.Succeeded)
                {
                    Console.WriteLine($"Successfully disconnected from VPN: {name}");
                }
                else
                {
                    Console.Error.WriteLine($"Failed to disconnect from VPN: {name}\nError: {result.Error}");
                    Environment.Exit(1);
                    return;
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.Error.WriteLine($"Failed to start disconnection process from VPN: {ex.Message}");
                Environment.Exit(1);
                return;
            }
        }

        [Command("check")]
        public void Check()
        {
            //Console.WriteLine($"Checking if your IP is in the UPV network...");
            try
            {
                var isConnected = VPNHelper.IsConnected();
                string isInUPVNetwork = isConnected ? "Yes" : "No";
                Console.WriteLine($"In UPV Network: {isInUPVNetwork}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error checking UPV network: {ex.Message}");
                Environment.Exit(1);
                return;
            }
        }

        [Command("list")]
        public void List()
        {
            try
            {
                var vpnNames = VPNHelper.List();
                Console.WriteLine("Available VPN connections:");
                foreach (var vpnName in vpnNames)
                {
                    Console.WriteLine($"- {vpnName}");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error listing VPN connections: {ex.Message}");
                Environment.Exit(1);
                return;
            }
        }
    }
}