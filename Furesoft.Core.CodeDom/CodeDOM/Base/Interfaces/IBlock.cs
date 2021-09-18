// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

namespace Nova.CodeDOM
{
    /// <summary>
    /// This interface is implemented by all code objects that can have a <see cref="Block"/> for a body
    /// (subclasses of <see cref="BlockStatement"/>, <see cref="AnonymousMethod"/>, <see cref="DocCode"/>).
    /// </summary>
    public interface IBlock
    {
        /// <summary>
        /// The <see cref="Block"/> body.
        /// </summary>
        Block Body { get; set; }

        /// <summary>
        /// True if the <see cref="Block"/> has a header.
        /// </summary>
        bool HasHeader { get; }

        /// <summary>
        /// True if the <see cref="Block"/> is at the top-level.
        /// </summary>
        bool IsTopLevel { get; }

        /// <summary>
        /// Add a <see cref="CodeObject"/> to the <see cref="Block"/>.
        /// </summary>
        void Add(CodeObject codeObject);

        /// <summary>
        /// Add multiple <see cref="CodeObject"/>s to the <see cref="Block"/>.
        /// </summary>
        void Add(params CodeObject[] codeObjects);

        /// <summary>
        /// Insert a <see cref="CodeObject"/> at the specified index.
        /// </summary>
        void Insert(int index, CodeObject codeObject);

        /// <summary>
        /// Remove the specified <see cref="CodeObject"/>.
        /// </summary>
        void Remove(CodeObject codeObject);

        /// <summary>
        /// Remove all objects from the <see cref="Block"/>.
        /// </summary>
        void RemoveAll();

        /// <summary>
        /// Reformat the <see cref="Block"/> body.
        /// </summary>
        void ReformatBlock();

        /// <summary>
        /// True if the <see cref="Block"/> is formatted on a single line.
        /// </summary>
        bool IsSingleLine { get; set; }
    }
}
