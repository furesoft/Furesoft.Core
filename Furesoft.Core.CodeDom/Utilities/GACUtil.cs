// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Furesoft.Core.CodeDom.Utilities.Reflection;
using Furesoft.Core.CodeDom.Utilities;

namespace Furesoft.Core.CodeDom.Utilities
{
    /// <summary>
    /// GAC is a utility class for accessing the Global Assembly Cache.
    /// </summary>
    public static class GACUtil
    {
        private static readonly Dictionary<string, List<GACEntry>> Entries = new Dictionary<string, List<GACEntry>>();

        /// <summary>
        /// Load the GAC.
        /// </summary>
        public static void LoadGAC()
        {
            string systemDir = Environment.GetEnvironmentVariable("windir");
            string dotNetGacPath = systemDir + "\\Microsoft.NET\\assembly";
            LoadGACPath(dotNetGacPath);
            string mainGacPath = systemDir + "\\assembly";
            LoadGACPath(mainGacPath);
        }

        private static void LoadGACPath(string gacPath)
        {
            foreach (string architectureDir in Directory.GetDirectories(gacPath))
            {
                // Load all "GAC", "GAC_32", "GAC_64", and "GAC_MSIL" subdirectories, while ignoring any "NativeImages"
                // or other subdirectories.
                string architecture = architectureDir.Substring(architectureDir.LastIndexOf('\\') + 1);
                if (architecture.StartsWith("GAC"))
                {
                    foreach (string assemblyDir in Directory.GetDirectories(architectureDir))
                    {
                        string assemblyName = assemblyDir.Substring(assemblyDir.LastIndexOf('\\') + 1);
                        string[] versionDirectories = Directory.GetDirectories(assemblyDir);
                        if (versionDirectories.Length > 0)
                        {
                            foreach (string subDir in versionDirectories)
                            {
                                // Parse the subdirectory name
                                string subDirName = subDir.Substring(subDir.LastIndexOf('\\') + 1);
                                string[] parts = subDirName.Split('_');
                                if (parts.Length > 0)
                                {
                                    int index = 0;
                                    string version = null, culture = null, publicKeyToken = null;
                                    if (parts[index].StartsWith("v"))  // Skip any "v4.0" prefix part
                                        ++index;
                                    if (index < parts.Length)
                                    {
                                        version = parts[index++];
                                        if (index < parts.Length)
                                        {
                                            culture = parts[index++];
                                            if (index < parts.Length)
                                                publicKeyToken = parts[index];
                                        }
                                    }
                                    AddGACEntry(assemblyName, version, culture, publicKeyToken, architecture, subDir);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void AddGACEntry(string assemblyName, string version, string culture, string publicKeyToken, string architecture, string path)
        {
            string key = assemblyName.ToLowerInvariant();
            GACEntry gacEntry = new GACEntry(assemblyName, version, culture, publicKeyToken, architecture, path);
            List<GACEntry> versionEntries;
            if (Entries.TryGetValue(key, out versionEntries))
            {
                // Ignore any duplicate entries between the .NET and Main GACs
                string uniqueName = gacEntry.FullUniqueName;
                if (!Enumerable.Any(versionEntries, delegate(GACEntry versionEntry) { return versionEntry.FullUniqueName == uniqueName; }))
                    versionEntries.Add(gacEntry);
            }
            else
                Entries.Add(key, new List<GACEntry> { gacEntry });
        }

        /// <summary>
        /// Find the entry of the assembly with the specified display name or short name and optional maximum version number.
        /// </summary>
        /// <param name="assemblyName">Assembly display name or short name.</param>
        /// <param name="maxVersion">Maximum version number (empty or null if none, ignored if display name is used).</param>
        /// <returns>The display name of the assembly if found, otherwise null.</returns>
        public static GACEntry FindAssembly(string assemblyName, string maxVersion)
        {
            lock (Entries)
            {
                if (Entries.Count == 0)
                    LoadGAC();
            }

            // Check for display name vs file spec vs short name
            bool isDisplayName = AssemblyUtil.IsDisplayName(assemblyName);
            string specificVersion = (isDisplayName ? AssemblyUtil.GetVersion(assemblyName) : null);
            bool isFileName = (assemblyName.IndexOf('\\') >= 0);
            string shortName = (isDisplayName ? assemblyName.Substring(0, assemblyName.IndexOf(','))
                : (isFileName ? (Path.GetFileNameWithoutExtension(assemblyName) ?? assemblyName) : assemblyName));

            // Find any entries matching the short name
            List<GACEntry> nameEntries;
            if (Entries.TryGetValue(shortName.ToLowerInvariant(), out nameEntries))
            {
                // If there's a specific or max version, filter as appropriate
                if (!string.IsNullOrEmpty(specificVersion))
                    nameEntries = Enumerable.ToList(Enumerable.Where(nameEntries, delegate(GACEntry entry) { return entry.CompareVersion(specificVersion) == 0; }));
                else if (!string.IsNullOrEmpty(maxVersion))
                    nameEntries = Enumerable.ToList(Enumerable.Where(nameEntries, delegate(GACEntry entry) { return entry.CompareVersion(maxVersion) <= 0; }));
                if ((nameEntries == null || nameEntries.Count == 0))
                    return null;

                // Find all entries with the highest version
                List<GACEntry> matchingEntries = new List<GACEntry> { nameEntries[0] };
                for (int i = 1; i < nameEntries.Count; ++i)
                {
                    int result = nameEntries[i].CompareVersion(matchingEntries[0].Version);
                    if (result > 0)
                        matchingEntries.Clear();
                    if (result >= 0)
                        matchingEntries.Add(nameEntries[i]);
                }

                // If we found a single match, we're done
                if (matchingEntries.Count == 1)
                    return matchingEntries[0];

                // Narrow the list by culture
                foreach (GACEntry entry in matchingEntries)
                {
                    if (!string.IsNullOrEmpty(entry.Culture) && entry.Culture != "en")
                        matchingEntries.Remove(entry);
                    if (matchingEntries.Count == 1)
                        return matchingEntries[0];
                }

                // Narrow the list by architecture
                foreach (GACEntry entry in matchingEntries)
                {
                    if (entry.Architecture != "GAC_MSIL")
                        matchingEntries.Remove(entry);
                    if (matchingEntries.Count == 1)
                        return matchingEntries[0];
                }

                Log.WriteLine("ERROR: Unable to determine a single matching assembly for: " + assemblyName);
            }

            return null;
        }

        /// <summary>
        /// Find the entry of the assembly with the specified display name or short name and optional maximum version number.
        /// </summary>
        /// <param name="assemblyName">Assembly display name or short name.</param>
        /// <returns>The display name of the assembly if found, otherwise null.</returns>
        public static GACEntry FindAssembly(string assemblyName)
        {
            return FindAssembly(assemblyName, null);
        }

        /// <summary>
        /// Compare the specified version strings in 'n.n.n.n' format.
        /// If one version string has fewer parts than other, the additional parts of the other version are ignored.
        /// </summary>
        /// <returns>0 if the versions are equal, -1 if version1 is less than version2, or 1 if version1 is greater than version2.</returns>
        public static int CompareVersions(string version1, string version2)
        {
            string[] v1a = version1.Split('.');
            string[] v2a = version2.Split('.');
            for (int i = 0; i < v1a.Length && i < v2a.Length; ++i)
            {
                int v1n = StringUtil.ParseInt(v1a[i]);
                int v2n = StringUtil.ParseInt(v2a[i]);
                if (v1n < v2n) return -1;
                if (v1n > v2n) return 1;
            }
            return 0;
        }

    }

    #region /* GACEntry class */

    public class GACEntry
    {
        public readonly string AssemblyName;
        public readonly string Version;
        public readonly string Culture;
        public readonly string PublicKeyToken;
        public readonly string Architecture;
        public readonly string Path;

        public GACEntry(string assemblyName, string version, string culture, string publicKeyToken, string architecture, string path)
        {
            AssemblyName = assemblyName;
            Culture = culture;
            Version = version;
            PublicKeyToken = publicKeyToken;
            Architecture = architecture;
            Path = path;
        }

        /// <summary>
        /// Compare the specified version string in 'n.n.n.n' format to the version string of the current GACEntry.
        /// If one version string has fewer parts than other, the additional parts of the other version are ignored.
        /// </summary>
        /// <returns>0 if they're equal, -1 if current version is less than the
        /// specified version, or 1 if the current version is greater than the specified version.</returns>
        public int CompareVersion(string version)
        {
            return GACUtil.CompareVersions(Version, version);
        }

        public string DisplayName
        {
            get { return AssemblyName + ", Version=" + Version + ", Culture=" + (string.IsNullOrEmpty(Culture) ? "neutral" : Culture) + ", PublicKeyToken=" + PublicKeyToken; }
        }

        public string FullUniqueName
        {
            get { return DisplayName + ", ProcessorArchitecture=" + Architecture; }
        }

        public string FileName
        {
            get { return Path + "\\" + AssemblyName + ".dll"; }
        }
    }

    #endregion
}
