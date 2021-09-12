// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Collections.Generic;
using System.Linq;

using Nova.CodeDOM;

namespace Nova.Resolving
{
    /// <summary>
    /// Holds a collection of possible matches for an <see cref="UnresolvedRef"/>.
    /// </summary>
    public class MatchCandidates : List<MatchCandidate>
    {
        protected bool _isMethodGroup;
        protected bool _isCategoryMatch;
        protected bool _isCompleteMatch;  // True if the matches are complete (any type & parameter arguments and static mode also match)

        /// <summary>
        /// Create a <see cref="MatchCandidates"/> collection.
        /// </summary>
        public MatchCandidates(bool isMethodGroup, bool isCategoryMatch, bool isCompleteMatch)
        {
            _isMethodGroup = isMethodGroup;
            _isCategoryMatch = isCategoryMatch;
            _isCompleteMatch = isCompleteMatch;
        }

        /// <summary>
        /// True if the matched objects represent one or more methods.
        /// </summary>
        public bool IsMethodGroup
        {
            get { return _isMethodGroup; }
        }

        /// <summary>
        /// True if the types of the matching objects are valid for the target category.
        /// </summary>
        public bool IsCategoryMatch
        {
            get { return _isCategoryMatch; }
        }

        /// <summary>
        /// True if the matches are complete (any type and parameter arguments and static mode also match).
        /// </summary>
        public bool IsCompleteMatch
        {
            get { return _isCompleteMatch; }
        }

        /// <summary>
        /// Determine if the collection contains the specified <see cref="CodeObject"/>.
        /// </summary>
        public bool Contains(CodeObject codeObject)
        {
            // NOTE: A bug in .NET causes an exception if Equals is called to compare a MethodInfo
            // for a generic method declaration to any non-MethodInfo object.   We currently only
            // need to search for CodeObjects, so we look for them specifically to avoid the bug.
            return Enumerable.Any(this, delegate(MatchCandidate candidate) { return candidate.Object is CodeObject && candidate.Object.Equals(codeObject); });
        }

        /// <summary>
        /// Create a new <see cref="MatchCandidates"/> collection with the same status flags as the current one (but empty).
        /// </summary>
        public MatchCandidates New()
        {
            return new MatchCandidates(_isMethodGroup, _isCategoryMatch, _isCompleteMatch);
        }
    }
}
