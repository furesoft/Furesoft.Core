// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections.Specialized;
using System.IO;

namespace Furesoft.Core.CodeDom.Utilities
{
    /// <summary>
    /// Helper methods for working with files and directories.
    /// </summary>
    public static class FileUtil
    {
        #region /* STATIC HELPER METHODS */

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
                int path1LastSlash = path1.LastIndexOf('\\');
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
        /// Get the base directory of the executing application, using the project directory if running in an output directory.
        /// </summary>
        public static string GetBaseDirectory()
        {
            string baseDirectory = Environment.CurrentDirectory + @"\";

            // If we're in an output directory, move up above it
            if (baseDirectory.EndsWith(@"\Debug\") || baseDirectory.EndsWith(@"\Release\"))
                baseDirectory = RemoveLastDirectory(baseDirectory);
            if (!baseDirectory.EndsWith(@"\bin\"))
                baseDirectory = RemoveLastDirectory(baseDirectory);
            if (baseDirectory.EndsWith(@"\bin\"))
                baseDirectory = RemoveLastDirectory(baseDirectory);
            return baseDirectory;
        }

        /// <summary>
        /// Remove the last directory from the specified path.
        /// </summary>
        public static string RemoveLastDirectory(string path)
        {
            return path.Substring(0, path.Trim('\\').LastIndexOf('\\') + 1);
        }

        /// <summary>
        /// Find the specified file name in the base directory of the application or any subdirectories.
        /// </summary>
        public static string FindFile(string fileName)
        {
            // Look in the current directory first
            string baseDirectory = Environment.CurrentDirectory;
            string[] files = Directory.GetFiles(baseDirectory, fileName, SearchOption.AllDirectories);
            if (files.Length > 0)
                return files[0];

            // Get the project base directory if we're running in an output directory
            baseDirectory = GetBaseDirectory();
            files = Directory.GetFiles(baseDirectory, fileName, SearchOption.AllDirectories);
            if (files.Length > 0)
                return files[0];

            return null;
        }

        #endregion
    }
}
