// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Text.RegularExpressions;

namespace WorkflowEngine
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
    public class Version : Attribute
    {
        public static readonly Version DefaultVersion = new Version(1, 0);
        private const string Major = @"major";
        private const string Minor = @"minor";
        private static readonly Regex VersionRegex = new Regex($@"^(?<{Major}>\d+)\.(?<{Minor}>\d+)$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(50));

        public Version()
        {
        }

        public Version(string version)
        {
            if (string.IsNullOrEmpty(version))
                throw new ArgumentNullException(nameof(version));

            var matches = VersionRegex.Match(version);
            if (!matches.Success)
                throw new ArgumentException($"{version} is not a valid version string", nameof(version));

            MajorVersion = uint.Parse(matches.Groups[Major].Value);
            MinorVersion = uint.Parse(matches.Groups[Minor].Value);
        }

        public Version(uint major, uint minor)
        {
            MajorVersion = major;
            MinorVersion = minor;
        }

        public uint MajorVersion { get; set; }

        public uint MinorVersion { get; set; }

        public static bool TryParse(string input, out Version version)
        {
            try
            {
                version = new Version(input);
                return true;
            }
            catch (Exception)
            {
                version = null;
                return false;
            }
        }

        /// <summary>
        /// Checks if the passed in version is backwards compatible with this version
        /// i.e. This version is of the same major version and equal or newer minor version
        /// </summary>
        public bool Compatible(Version other)
        {
            if (other == null)
                return false;

            return MajorVersion == other.MajorVersion &&
                   MinorVersion >= other.MinorVersion;
        }

        public override string ToString()
        {
            return $"{MajorVersion}.{MinorVersion}";
        }
    }
}
