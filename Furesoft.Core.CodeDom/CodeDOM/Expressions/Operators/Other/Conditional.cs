using System.Collections.Generic;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a conditional if/then/else (ternary) expression.
    /// </summary>
    public class Conditional : Operator
    {
        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = false;

        /// <summary>
        /// The token used to parse the 'then' part.
        /// </summary>
        public const string ParseToken1 = "?";

        /// <summary>
        /// The token used to parse the 'else' part.
        /// </summary>
        public const string ParseToken2 = ":";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 400;

        protected Expression _else;
        protected Expression _if;
        protected Expression _then;

        /// <summary>
        /// Create a <see cref="Conditional"/> operator.
        /// </summary>
        public Conditional(Expression @if, Expression then, Expression @else)
        {
            If = @if;
            Then = then;
            Else = @else;
        }

        protected Conditional(Parser parser, CodeObject parent)
                    : base(parser, parent)
        {
            IsFirstOnLine = false;
            Expression conditional = parser.RemoveLastUnusedExpression();
            if (conditional != null)
            {
                MoveFormatting(conditional);
                SetField(ref _if, conditional, false);
                if (conditional.IsFirstOnLine)
                    IsFirstOnLine = true;
                // Move any comments before the '?' to the conditional expression
                conditional.MoveCommentsAsPost(parser.LastToken);
            }

            // If the '?' clause is indented less than the parent object, set the NoIndentation flag to prevent
            // it from being formatted relative to the parent object.
            if (parser.CurrentTokenIndentedLessThan(parser.ParentStartingToken))
                SetFormatFlag(FormatFlags.NoIndentation, true);

            Token ifToken = parser.Token;
            parser.NextToken();  // Move past '?'
            ++parser.ConditionalNestingLevel;
            SetField(ref _then, Parse(parser, this, false, ParseToken2), false);
            --parser.ConditionalNestingLevel;
            if (_then != null)
            {
                if (ifToken.IsFirstOnLine)
                    _then.IsFirstOnLine = true;
                _then.MoveCommentsToLeftMost(ifToken, false);
                // Move any comments before the ':' to the then expression
                _then.MoveCommentsAsPost(parser.LastToken);
            }

            Token elseToken = parser.Token;
            ParseExpectedToken(parser, ParseToken2);  // Move past ':'
            SetField(ref _else, Parse(parser, this), false);
            if (_else != null)
            {
                if (elseToken.IsFirstOnLine)
                    _else.IsFirstOnLine = true;
                _else.MoveCommentsToLeftMost(elseToken, false);
                // Move any comments at the end to the else expression
                _else.MoveCommentsAsPost(parser.LastToken);
            }

            // If the else clause isn't on a new line, set the NoIndentation flag
            if ((!elseToken.IsFirstOnLine && (_else == null || !_else.IsFirstOnLine)))
                SetFormatFlag(FormatFlags.NoIndentation, true);
        }

        /// <summary>
        /// The 'else' expression.
        /// </summary>
        public Expression Else
        {
            get { return _else; }
            set { SetField(ref _else, value, true); }
        }

        /// <summary>
        /// True if the expression should have parens by default.
        /// </summary>
        public override bool HasParensDefault
        {
            get { return true; }
        }

        /// <summary>
        /// The 'if' expression.
        /// </summary>
        public Expression If
        {
            get { return _if; }
            set { SetField(ref _if, value, true); }
        }

        /// <summary>
        /// True if the expression is const.
        /// </summary>
        public override bool IsConst
        {
            // A Conditional is const if both result clauses are const
            get { return (_then != null && _then.IsConst && _else != null && _else.IsConst); }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get
            {
                return (base.IsSingleLine && (_if == null || _if.IsSingleLine)
                    && (_then == null || (!_then.IsFirstOnLine && _then.IsSingleLine))
                    && (_else == null || (!_else.IsFirstOnLine && _else.IsSingleLine)));
            }
            set
            {
                base.IsSingleLine = value;
                if (_if != null)
                {
                    _if.IsFirstOnLine = false;
                    _if.IsSingleLine = value;
                }
                if (_then != null)
                {
                    _then.IsFirstOnLine = !value;
                    _then.IsSingleLine = value;
                }
                if (_else != null)
                {
                    _else.IsFirstOnLine = !value;
                    _else.IsSingleLine = value;
                }
            }
        }

        /// <summary>
        /// The 'then' expression.
        /// </summary>
        public Expression Then
        {
            get { return _then; }
            set { SetField(ref _then, value, true); }
        }

        /// <summary>
        /// Parse a <see cref="Conditional"/> operator.
        /// </summary>
        public static Conditional Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Disallow conditionals at the TypeDecl or NamespaceDecl levels - this prevents the possibility of matching when
            // parsing a return type expression that is a nullable type for a generic method that has constraints.
            if (parent is TypeDecl || parent is NamespaceDecl)
                return null;

            // Verify that we have a matching ':' for the '?', otherwise abort (so TypeRef can try parsing it as a nullable type).
            // If we're nested without parens, we must find one extra ':' for each nested level.
            if (PeekConditional(parser, parent, parser.ConditionalNestingLevel + 1, flags))
                return new Conditional(parser, parent);

            return null;
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            bool relativeToParent = (!HasNoIndentation && _then != null && _then.IsFirstOnLine && _if != null && _if.IsSingleLine);
            if (relativeToParent)
            {
                writer.SetParentOffset();
                writer.BeginIndentOnNewLineRelativeToParentOffset(this, true);
            }

            if (_if != null)
                _if.AsText(writer, flags);

            RenderFlags thenFlags = flags;
            if (_then != null && _then.IsFirstOnLine)
            {
                writer.WriteLine();
                thenFlags |= RenderFlags.SuppressNewLine;
            }
            else
                writer.Write(" ");
            UpdateLineCol(writer, flags);
            writer.Write("? ");
            if (_then != null)
                _then.AsText(writer, thenFlags);

            if (_else != null && _else.IsFirstOnLine)
            {
                writer.WriteLine();
                writer.Write(": ");
                flags |= RenderFlags.SuppressNewLine;
            }
            else
                writer.Write(" : ");
            if (_else != null)
                _else.AsText(writer, flags);

            if (relativeToParent)
                writer.EndIndentation(this);
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            Conditional clone = (Conditional)base.Clone();
            clone.CloneField(ref clone._if, _if);
            clone.CloneField(ref clone._then, _then);
            clone.CloneField(ref clone._else, _else);
            return clone;
        }

        /// <summary>
        /// Get the precedence of the operator.
        /// </summary>
        public override int GetPrecedence()
        {
            return Precedence;
        }

        /// <summary>
        /// Move any comments from the specified <see cref="Token"/> to the left-most sub-expression.
        /// </summary>
        public override void MoveCommentsToLeftMost(Token token, bool skipParens)
        {
            if ((HasParens && !skipParens) || _if == null)
                MoveAllComments(token);
            else
                _if.MoveCommentsToLeftMost(token, false);
        }

        internal static new void AddParsePoints()
        {
            // Use a parse-priority of 0 (TypeRef uses 100 for nullable types)
            Parser.AddOperatorParsePoint(ParseToken1, Precedence, LeftAssociative, false, Parse);
        }

        protected static bool PeekConditional(Parser parser, CodeObject parent, int colonCount, ParseFlags flags)
        {
            // Unfortunately, determining if the '?' is definitely part of a '?:' pair as opposed to a nullable type declaration
            // isn't easy - in fact, it's the single hardest thing to parse in the entire language.  Nicely formatted code always
            // has a space before it in the first case, and not in the second, but code is often poorly formatted.  The only way
            // to be sure how to parse it is to peek ahead looking for the matching ':'.  We can parse in a very simplified manner
            // for efficiency, just keeping track of '()', '[]', '{}' pairs, aborting if we hit a ';' anywhere, or a ',' that's
            // not in a nested scope, or finally if we find the matching ':' (not in a nested scope).  If we're in the '?' clause
            // of a nested Conditional without parens, then we need to find an extra ':' for each nested level (colonCount).
            // One more thing - we have to handle '<>' with generic arguments in order to avoid aborting on a possible ','
            // inside them, but we also have to avoid any confusion with LessThan/GreatherThan operators.

            bool firstToken = true;
            Stack<string> stack = new Stack<string>(8);
            while (true)
            {
                Token next = parser.PeekNextToken();
                if (next == null)
                    break;
                check:
                if (next.IsSymbol)
                {
                    // Abort if any invalid symbols appear immediately after the '?'
                    if (firstToken)
                    {
                        firstToken = false;
                        if (">)]};,".Contains(next.Text))
                            break;
                    }

                    // If we have a '<', skip over any possible generic type parameters so that we don't abort on a ','
                    if (next.Text == TypeRefBase.ParseTokenArgumentStart)
                    {
                        TypeRefBase.PeekTypeArguments(parser, TypeRefBase.ParseTokenArgumentEnd, flags);
                        next = parser.LastPeekedToken;
                    }

                    // Keep track of nested parens, brackets (new arrays), braces (initializers or generics in doc comments)
                    string nextText = next.Text;
                    if (nextText == ParseTokenStartGroup || nextText == NewArray.ParseTokenStart || nextText == Initializer.ParseTokenStart)
                        stack.Push(nextText);
                    else if (nextText == ParseTokenEndGroup)
                    {
                        // If we hit an unexpected ')', abort
                        if (stack.Count == 0 || stack.Peek() != ParseTokenStartGroup)
                            break;
                        stack.Pop();
                    }
                    else if (nextText == NewArray.ParseTokenEnd)
                    {
                        // If we hit an unexpected ']', abort
                        if (stack.Count == 0 || stack.Peek() != NewArray.ParseTokenStart)
                            break;
                        stack.Pop();
                    }
                    else if (nextText == Initializer.ParseTokenEnd)
                    {
                        // If we hit an unexpected '}', abort
                        if (stack.Count == 0 || stack.Peek() != Initializer.ParseTokenStart)
                            break;
                        stack.Pop();
                    }
                    else if (nextText == ParseToken1)
                    {
                        // If we found a '?', recursively call this routine to process it (in order to
                        // differentiate between a nested nullable type or another Conditional).
                        if (!PeekConditional(parser, parent, 1, flags))
                        {
                            // If it wasn't a Conditional, get the last token and check it
                            next = parser.LastPeekedToken;
                            goto check;
                        }
                    }
                    else if (stack.Count == 0)
                    {
                        // We're not inside any nested parens/brackets/braces/angle brackets.  Check for certain symbols:

                        // Terminate on a ',' or ';' (we ignore if nested because of anonymous method bodies)
                        if (nextText == ParseTokenSeparator || nextText == Statement.ParseTokenTerminator)
                            break;

                        // Process a ':'
                        if (nextText == ParseToken2)
                        {
                            // Assume we have a valid Conditional if the expected number of colons has been found
                            if (--colonCount == 0)
                                return true;
                        }
                    }
                }
                else if (next.Text == NewOperator.ParseToken)
                {
                    // If we found a 'new', treat the following as a type (in order to avoid any trailing '?' of
                    // a nullable type from being treated as a nested conditional).
                    TypeRefBase.PeekType(parser, parser.PeekNextToken(), true, flags | ParseFlags.Type);
                    // Whether it worked or not, pick up with the last peeked token
                    next = parser.LastPeekedToken;
                    firstToken = false;
                    goto check;
                }
                firstToken = false;
            }
            return false;
        }
    }
}