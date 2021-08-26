// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections.Specialized;
using System.IO;

namespace Nova.Utilities
{
    /// <summary>
    /// Helper methods for working with files and directories.
    /// </summary>
    public static class FileUtil
    {
        /// <summary>
        /// Combine the two specified paths, removing any "..\" prefixes from the 2nd path,
        /// while removing corresponding subdirectories from the 1st path at the same time.
        /// Also, ensure that only a single '\' is used to combine the paths.
        /// </summary>
        public static string CombineAndNormalizePath(string path1, string path2)
        {
            if (path1.EndsWith(@"\"))
                path1 = path1.Substring(0, path1.Length - 1);
            while (path2.StartsWith(@"..\"))
            {
                path2 = path2.Substring(3);
                var path1LastSlash = path1.LastIndexOf('\\');
                if (path1LastSlash >= 0)
                    path1 = path1.Substring(0, path1LastSlash);
            }
            // Must make sure any "C:" has a '\' after it, because Path.Combine() won't add one in this case
            // (because 'C:../directory' is actually legal and useful).
            if (path1.EndsWith(":"))
                path1 += @"\";
            // Also, call GetFullPath() in order to eliminate any embedded '\..\'s between other directory names
            return Path.GetFullPath(Path.Combine(path1, path2));
        }

        /// <summary>
        /// Find the specified file name in the base directory of the application or any subdirectories.
        /// </summary>
        public static string FindFile(string fileName)
        {
            // Look in the current directory first
            var baseDirectory = Environment.CurrentDirectory;
            var files = Directory.GetFiles(baseDirectory, fileName, SearchOption.AllDirectories);
            if (files.Length > 0)
                return files[0];

            // Get the project base directory if we're running in an output directory
            baseDirectory = GetBaseDirectory();
            files = Directory.GetFiles(baseDirectory, fileName, SearchOption.AllDirectories);
            if (files.Length > 0)
                return files[0];

            return null;
        }

        /// <summary>
        /// Get the base directory of the executing application, using the project directory if running in an output directory.
        /// </summary>
        public static string GetBaseDirectory()
        {
            var baseDirectory = Environment.CurrentDirectory + @"\";

            // If we're in an output directory, move up above it
            if (baseDirectory.EndsWith(@"\Debug\") || baseDirectory.EndsWith(@"\Release\"))
                baseDirectory = RemoveLastDirectory(baseDirectory);
            // Remove any '\bin\xxx' directories, or just '\bin'
            if (!baseDirectory.EndsWith(@"\bin\"))
            {
                var parentDirectory = RemoveLastDirectory(baseDirectory);
                if (parentDirectory.EndsWith(@"\bin\"))
                    baseDirectory = RemoveLastDirectory(parentDirectory);
            }
            else if (baseDirectory.EndsWith(@"\bin\"))
                baseDirectory = RemoveLastDirectory(baseDirectory);
            return baseDirectory;
        }

        /// <summary>
        /// Determine a relative path that navigates from the first specified path to the second one.
        /// </summary>
        public static string MakeRelative(string fromPath, string toPath)
        {
            if (fromPath == null)
                throw new ArgumentNullException("fromPath");
            if (toPath == null)
                throw new ArgumentNullException("toPath");

            // If both paths are rooted, but aren't the same, just return 'toPath'
            if (Path.IsPathRooted(fromPath) && Path.IsPathRooted(toPath))
            {
                if (string.Compare(Path.GetPathRoot(fromPath), Path.GetPathRoot(toPath), true) != 0)
                    return toPath;
            }

            var relativePath = new StringCollection();
            var fromDirectories = fromPath.Split(Path.DirectorySeparatorChar);
            var toDirectories = toPath.Split(Path.DirectorySeparatorChar);
            var length = Math.Min(fromDirectories.Length, toDirectories.Length);

            // find common root
            var lastCommonRoot = -1;
            for (var i = 0; i < length; i++)
            {
                if (string.Compare(fromDirectories[i], toDirectories[i], true) != 0)
                    break;
                lastCommonRoot = i;
            }
            if (lastCommonRoot == -1)
                return toPath;

            // add relative folders in from path
            for (var i = lastCommonRoot + 1; i < fromDirectories.Length; i++)
                if (fromDirectories[i].Length > 0)
                    relativePath.Add("..");

            // add folders in to path
            for (var i = lastCommonRoot + 1; i < toDirectories.Length; i++)
                relativePath.Add(toDirectories[i]);

            return StringUtil.ToString(relativePath, Path.DirectorySeparatorChar.ToString());
        }

        /// <summary>
        /// Remove the specified common root path from the specified file name.
        /// </summary>
        public static string RemoveCommonRootPath(string fileName, string rootFileOrDirectory)
        {
            var currentLocation = Path.GetDirectoryName(rootFileOrDirectory) + @"\";
            return (fileName.StartsWith(currentLocation) ? fileName.Substring(currentLocation.Length) : fileName);
        }

        /// <summary>
        /// Remove the last directory from the specified path.
        /// </summary>
        public static string RemoveLastDirectory(string path)
        {
            return path.Substring(0, path.Trim('\\').LastIndexOf('\\') + 1);
        }
    }
}