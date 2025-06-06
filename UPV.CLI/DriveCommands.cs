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
        public int Connect([Argument] string user, [Argument] string domain, [Option] string? driveLetter, [Option] bool open = false)
        {
            if (!CommandsHelper.TryValidateAndParseEnum<UPVDomain>(domain, nameof(domain), out var enumDomain))
            {
                return 1;
            }

            char? letter = null;
            if (driveLetter is not null && !DriveLetterTools.TryGetLetter(driveLetter, out letter))
            {
                Console.Error.WriteLine($"Invalid drive letter format: {driveLetter}. Please provide a valid drive letter (e.g., 'W:').");
                return 1;
            }

            if (letter is not null && !DriveLetterTools.IsAvailable(letter.Value))
            {
                Console.Error.WriteLine($"Drive letter not available: {letter}. Please choose a different one.");
                return 1;
            }

            letter ??= DriveLetterTools.GetFirstAvailable(prioritize: DriveFactory.WDriveDefaultLetter);

            if (letter is null)
            {
                Console.Error.WriteLine("No available drive letters found. Please free up a drive letter and try again.");
                return 1;
            }

            Console.WriteLine($"Connecting to UPV drive with username {user} and domain {domain} at letter {letter}:");

            try
            {
                var drive = DriveFactory.GetDriveW(user: user, domain: enumDomain, letter.Value);
                var process = drive.Connect();
                var result = CmdHelper.WaitAndCheck(process.Process);
                var error = drive.OnProcessConnected(process, result);

                if (error is null)
                {
                    Console.WriteLine($"Successfully connected to {drive.Name} at {drive.ConnectedDriveLetter}");

                    if (open)
                    {
                        drive.Open();
                    }
                }
                else
                {
                    Console.Error.WriteLine(error.GetErrorMessage(showFullError: false));
                    Debug.WriteLine($"Error connecting to drive: {error.GetErrorMessage(showFullError: true)}");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An unexpected error occurred while connecting to the drive:\n{ex.Message}");
                return 1;
            }

            return 0;
        }

        [Command("disconnect")]
        public int Disconnect([Argument] string driveLetter = "W:", [Option] bool force = false)
        {
            if (!DriveLetterTools.TryNormalizeDriveLetter(driveLetter, out var normalizedDriveLetter))
            {
                Console.Error.WriteLine($"Invalid drive letter format: {driveLetter}. Please provide a valid drive letter (e.g., 'W:').");
                return 1;
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
                    return 1;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An unexpected error occurred while disconnecting from the drive:\n{ex.Message}");
                return 1;
            }

            Console.WriteLine($"Successfully disconnected drive at: {normalizedDriveLetter}");
            return 0;
        }
    }
}