// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Text;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// This interface is implemented by all code objects that can be stored as files, which includes
    /// <see cref="Solution"/>, <see cref="Project"/>, and <see cref="CodeUnit"/>.
    /// </summary>
    public interface IFile
    {
        /// <summary>
        /// The encoding of the file (normally UTF8).
        /// </summary>
        Encoding FileEncoding { get; set; }

        /// <summary>
        /// True if the file exists.
        /// </summary>
        bool FileExists { get; }

        /// <summary>
        /// True if the file has a UTF8 byte-order-mark.
        /// </summary>
        bool FileHasUTF8BOM { get; set; }

        /// <summary>
        /// The file name.
        /// </summary>
        string FileName { get; set; }

        /// <summary>
        /// True if the file is formatted using tabs, otherwise false (using spaces).
        /// </summary>
        bool FileUsingTabs { get; set; }

        /// <summary>
        /// True if the file is newly created and hasn't been saved yet.
        /// </summary>
        bool IsNew { get; }

        /// <summary>
        /// Save the file.
        /// </summary>
        void Save();

        /// <summary>
        /// Save the file to the specified file name.
        /// </summary>
        void SaveAs(string fileName);
    }
}