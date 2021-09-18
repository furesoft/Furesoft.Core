// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;

using Nova.Parsing;
using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Has an optional <see cref="Expression"/> or <see cref="LocalDecl"/> (which must evaluate to an <see cref="Exception"/>
    /// type), plus a body that is executed if an exception of the specified type is caught.  The <see cref="Expression"/>
    /// may be omitted (null), in which case all exceptions are caught.
    /// </summary>
    public class Catch : BlockStatement
    {
        #region /* FIELDS */

        /// <summary>
        /// Can be null (catch all), an Expression that evaluates to a TypeRef of type Exception, or
        /// a LocalDecl of type Exception.
        /// </summary>
        protected CodeObject _target;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="Catch"/>.
        /// </summary>
        public Catch(CodeObject body)
            : base(body, false)
        { }

        /// <summary>
        /// Create a <see cref="Catch"/>.
        /// </summary>
        public Catch()
            : base(null, false)
        { }

        /// <summary>
        /// Create a <see cref="Catch"/>.
        /// </summary>
        public Catch(LocalDecl localDecl, CodeObject body)
            : base(body, false)
        {
            Target = localDecl;
        }

        /// <summary>
        /// Create a <see cref="Catch"/>.
        /// </summary>
        public Catch(LocalDecl localDecl)
            : base(null, false)
        {
            Target = localDecl;
        }

        /// <summary>
        /// Create a <see cref="Catch"/>.
        /// </summary>
        public Catch(Expression type, CodeObject body)
            : base(body, false)
        {
            Target = type;
        }

        /// <summary>
        /// Create a <see cref="Catch"/>.
        /// </summary>
        public Catch(Expression type)
            : base(null, false)
        {
            Target = type;
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The optional target <see cref="Expression"/> or <see cref="LocalDecl"/>.
        /// </summary>
        public CodeObject Target
        {
            get { return _target; }
            set
            {
                if (value is LocalDecl && value.Parent != null)
                    throw new Exception("The LocalDecl used for the target variable of a Catch must be new, not one already owned by another Parent object.");
                SetField(ref _target, value, true);
            }
        }

        /// <summary>
        /// The keyword associated with the <see cref="Statement"/>.
        /// </summary>
        public override string Keyword
        {
            get { return ParseToken; }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            Catch clone = (Catch)base.Clone();
            clone.CloneField(ref clone._target, _target);
            return clone;
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "catch";

        internal static void AddParsePoints()
        {
            // Normally, a 'catch' is parsed by the 'try' parse logic (see Try).
            // This parse-point exists only to catch an orphaned 'catch' statement.
            Parser.AddParsePoint(ParseToken, ParseOrphan, typeof(IBlock));
        }

        /// <summary>
        /// Parse an orphaned <see cref="Catch"/>.
        /// </summary>
        public static Catch ParseOrphan(Parser parser, CodeObject parent, ParseFlags flags)
        {
            Token token = parser.Token;
            Catch @catch = Parse(parser, parent);
            parser.AttachMessage(@catch, "Orphaned 'catch' - missing parent 'try'", token);
            return @catch;
        }

        /// <summary>
        /// Parse a <see cref="Catch"/>.
        /// </summary>
        public static Catch Parse(Parser parser, CodeObject parent)
        {
            return new Catch(parser, parent);
        }

        protected Catch(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            MoveComments(parser.LastToken);  // Get any comments before 'catch'
            parser.NextToken();              // Move past 'catch'

            // Check for compiler directives, storing them as infix annotations on the parent
            Block.ParseCompilerDirectives(parser, this, AnnotationFlags.IsInfix1);

            // Check if 'catch' has parens
            if (parser.TokenText == Expression.ParseTokenStartGroup)
            {
                parser.NextToken();  // Move past '('

                // Parse either LocalDecl or Expression (TypeRef)
                if (LocalDecl.PeekLocalDecl(parser))
                    SetField(ref _target, LocalDecl.Parse(parser, this, false, false), false);
                else
                    SetField(ref _target, Expression.Parse(parser, this, true, Expression.ParseTokenEndGroup), false);

                ParseExpectedToken(parser, Expression.ParseTokenEndGroup);  // Move past ')'
            }

            new Block(out _body, parser, this, true);    // Parse the body
            ParseUnusedAnnotations(parser, this, true);  // Parse any annotations from the Unused list

            // Remove any preceeding blank lines if auto-cleanup is on
            if (AutomaticFormattingCleanup && NewLines > 1)
                NewLines = 1;
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// True if the <see cref="Statement"/> has an argument.
        /// </summary>
        public override bool HasArgument
        {
            get { return (_target != null); }
        }

        /// <summary>
        /// True if the <see cref="BlockStatement"/> has compact empty braces by default.
        /// </summary>
        public override bool IsCompactIfEmptyDefault
        {
            get { return true; }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get { return (base.IsSingleLine && (_target == null || (!_target.IsFirstOnLine && _target.IsSingleLine))); }
            set
            {
                base.IsSingleLine = value;
                if (value && _target != null)
                {
                    _target.IsFirstOnLine = false;
                    _target.IsSingleLine = true;
                }
            }
        }

        #endregion

        #region /* RENDERING */

        protected override void AsTextStatement(CodeWriter writer, RenderFlags flags)
        {
            base.AsTextStatement(writer, flags);
            AsTextAnnotations(writer, AnnotationFlags.IsInfix1, flags);
        }

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            if (_target != null)
                _target.AsText(writer, flags);
        }

        #endregion
    }
}
