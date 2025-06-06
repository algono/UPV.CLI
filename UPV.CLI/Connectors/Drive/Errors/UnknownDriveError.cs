﻿namespace UPV.CLI.Connectors.Drive.Errors
{
    public class UnknownDriveError : DriveError
    {
        /// <summary>
        /// This always returns null because this error does not have a specific error code.
        /// </summary>
        public override string? Code => null; // No specific error code for this error

        public const string DefaultMessageFormat = "An unknown error occurred while disconnecting the drive (error code: {1}).";

        public UnknownDriveError(string errorMessage) : base(errorMessage, null)
        {
        }
    }
}
