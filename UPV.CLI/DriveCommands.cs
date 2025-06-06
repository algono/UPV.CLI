﻿using Cocona;
using System.Diagnostics;
using UPV.CLI.Connectors.Drive;
using UPV.CLI.Connectors.Helpers;

namespace UPV.CLI
{
    // Drive Commands
    public class DriveCommands
    {
        [Command("connect")]
        public void Connect([Argument] string user, [Argument] UPVDomain domain, [Option] string? driveLetter)
        {
            char? letter = null;
            if (driveLetter is not null && !DriveLetterTools.TryGetLetter(driveLetter, out letter))
            {
                Console.Error.WriteLine($"Invalid drive letter format: {driveLetter}. Please provide a valid drive letter (e.g., 'W:').");
                return;
            }

            if (letter is not null && !DriveLetterTools.IsAvailable(letter.Value))
            {
                Console.Error.WriteLine($"Drive letter not available: {letter}. Please choose a different one.");
                return;
            }

            letter ??= DriveLetterTools.GetFirstAvailable(prioritize: DriveFactory.WDriveDefaultLetter);

            if (letter is null)
            {
                Console.Error.WriteLine("No available drive letters found. Please free up a drive letter and try again.");
                return;
            }

            Console.WriteLine($"Connecting to UPV drive with username {user} and domain {domain} at letter {letter}:");

            try
            {
                var drive = DriveFactory.GetDriveW(user: user, domain: domain, letter.Value);
                var process = drive.Connect();
                var result = CmdHelper.WaitAndCheck(process.Process);
                var error = drive.OnProcessConnected(process, result);

                if (error is null)
                {
                    Console.WriteLine($"Successfully connected to {drive.Name} at {drive.ConnectedDriveLetter}");
                }
                else
                {
                    Console.Error.WriteLine(error.GetErrorMessage(showFullError: false));
                    Debug.WriteLine($"Error connecting to drive: {error.GetErrorMessage(showFullError: true)}");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An unexpected error occurred while connecting to the drive:\n{ex.Message}");
            }
        }

        [Command("disconnect")]
        public void Disconnect([Argument] string driveLetter = "W:", [Option] bool force = false)
        {
            if (!DriveLetterTools.TryNormalizeDriveLetter(driveLetter, out var normalizedDriveLetter))
            {
                Console.Error.WriteLine($"Invalid drive letter format: {driveLetter}. Please provide a valid drive letter (e.g., 'W:').");
                return;
            }

            Console.WriteLine($"Disconnecting drive at: {normalizedDriveLetter}");

            try
            {
                var process = DriveConnectionHelper.Disconnect(normalizedDriveLetter, force);
                var result = CmdHelper.WaitAndCheck(process);
                var error = DriveConnectionHelper.CheckDriveDisconnectionErrors(normalizedDriveLetter, result);

                if (error is not null)
                {
                    Console.Error.WriteLine(error.GetErrorMessage(showFullError: false));
                    Debug.WriteLine($"Error disconnecting from drive: {error.GetErrorMessage(showFullError: true)}");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An unexpected error occurred while disconnecting from the drive:\n{ex.Message}");
            }
        }
    }
}