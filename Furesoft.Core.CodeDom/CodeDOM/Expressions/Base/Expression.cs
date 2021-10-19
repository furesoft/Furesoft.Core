using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Arithmetic;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Methods;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Properties;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Variables;
using Furesoft.Core.CodeDom.CodeDOM.Namespaces;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Generics;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;
using System;
using System.Collections;
using System.Reflection;
using static Furesoft.Core.CodeDom.Parsing.Parser;
using Index = Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other.Index;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Base
{
    /// <summary>
    /// The common base class of all expressions.
    /// </summary>
    /// <remarks>
    /// Expressions often occur in trees, and can include operators, indexers, method (and property) calls, which operate
    /// on variables, constants, fields, etc., resulting in a specific type and value.
    /// Expressions which are valid as statements are: Assignment (and all compound assignments), Call,
    /// Increment/Decrement, PostIncrement/Decrement, Dot, and NewObject.
    /// </remarks>
    public abstract class Expression : CodeObject
    {
        /// <summary>
        /// The token used to parse the end of a group.
        /// </summary>
        public const string ParseTokenEndGroup = ")";

        /// <summary>
        /// The token used to parse between expressions in a list.
        /// </summary>
        public const string ParseTokenSeparator = ",";

        /// <summary>
        /// The token used to parse the start of a group.
        /// </summary>
        public const string ParseTokenStartGroup = "(";

        protected Expression()
        {
            SetFormatFlag(FormatFlags.Grouping, HasParensDefault);
        }

        /// <summary>
        /// Parse an <see cref="Expression"/>.
        /// </summary>
        protected Expression(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        /// <summary>
        /// True if the expression is surrounded by parens.
        /// </summary>
        public bool HasParens
        {
            get { return _formatFlags.HasFlag(FormatFlags.Grouping); }
            set
            {
                SetFormatFlag(FormatFlags.Grouping, value);
                _formatFlags |= FormatFlags.GroupingSet;
            }
        }

        /// <summary>
        /// True if the expression should have parens by default.
        /// </summary>
        public virtual bool HasParensDefault
        {
            get { return false; }  // Default is no parens
        }

        /// <summary>
        /// True if the expression is const.
        /// </summary>
        public virtual bool IsConst
        {
            get { return false; }
        }

        /// <summary>
        /// True if the expression evaluates to a delegate type.
        /// </summary>
        public virtual bool IsDelegateType
        {
            get
            {
                TypeRefBase typeRefBase = SkipPrefixes() as TypeRefBase;
                return (typeRefBase != null && typeRefBase.IsDelegateType);
            }
        }

        /// <summary>
        /// True if the closing paren or bracket is on a new line.
        /// </summary>
        public virtual bool IsEndFirstOnLine
        {
            get { return _formatFlags.HasFlag(FormatFlags.InfixNewLine); }
            set
            {
                SetFormatFlag(FormatFlags.InfixNewLine, value);
                if (value)
                    _formatFlags |= FormatFlags.NewLinesSet;
            }
        }

        /// <summary>
        /// True if the code object defaults to starting on a new line.
        /// </summary>
        public override bool IsFirstOnLineDefault
        {
            get { return HasFirstOnLineAnnotations; }
        }

        /// <summary>
        /// True if the expression evaluates to a delegate, or unresolved type or a <see cref="TypeParameterRef"/>.
        /// </summary>
        public virtual bool IsPossibleDelegateType
        {
            get { return IsDelegateType; }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get { return (base.IsSingleLine && !IsEndFirstOnLine); }
            set
            {
                base.IsSingleLine = value;
                if (value)
                    IsEndFirstOnLine = false;
            }
        }

        public static void AddParsePoints()
        {
            // Use a parse-priority of 400 (ConstructorDecl uses 0, MethodDecl uses 50, LambdaExpression uses 100, Call uses 200, Cast uses 300)
            Parser.AddParsePoint(ParseTokenStartGroup, 400, ParseParenthesizedExpression);
        }

        /// <summary>
        /// Implicit conversion of a <see cref="Namespace"/> to an <see cref="Expression"/> (actually, a <see cref="NamespaceRef"/>).
        /// </summary>
        /// <remarks>This allows <see cref="Namespace"/> to be passed directly to any method expecting an <see cref="Expression"/> type
        /// without having to create a reference first.</remarks>
        /// <param name="namespace">The <see cref="Namespace"/> to be converted.</param>
        /// <returns>A generated <see cref="NamespaceRef"/> to the specified <see cref="Namespace"/>.</returns>
        public static implicit operator Expression(Namespace @namespace)
        {
            return @namespace.CreateRef();
        }

        /// <summary>
        /// Implicit conversion of a <see cref="Type"/> to an <see cref="Expression"/> (actually, a <see cref="TypeRef"/>).
        /// </summary>
        /// <remarks>This allows <see cref="Type"/>s such as <c>typeof(int)</c> to be passed directly to any method
        /// expecting an <see cref="Expression"/> type without having to create a reference first.</remarks>
        /// <param name="type">The <see cref="Type"/> to be converted.</param>
        /// <returns>A generated <see cref="TypeRef"/> to the specified <see cref="Type"/>.</returns>
        public static implicit operator Expression(Type type)
        {
            if (type.IsGenericTypeDefinition)
            {
                // If the type is a generic type definition, such as 'typeof(Dictonary<,>)', then default the
                // type arguments to 'null'.  If the user needs the declared type arguments instead, they will
                // have to call TypeRef.Create() directly.
                return TypeRef.Create(type, ChildList<Expression>.CreateListOfNulls(type.GetGenericArguments().Length));
            }
            return TypeRef.Create(type);
        }

        /// <summary>
        /// Implicit conversion of a <see cref="MethodBase"/> to an <see cref="Expression"/> (actually, a <see cref="MethodRef"/> or <see cref="ConstructorRef"/>).
        /// </summary>
        /// <remarks>This allows <see cref="MethodBase"/>s (<see cref="MethodInfo"/>s or <see cref="ConstructorInfo"/>s) to be passed directly
        /// to any method expecting an <see cref="Expression"/> type without having to create a reference first.</remarks>
        /// <param name="methodBase">The <see cref="MethodBase"/> to be converted.</param>
        /// <returns>A generated <see cref="MethodRef"/> to the specified <see cref="MethodBase"/>.</returns>
        public static implicit operator Expression(MethodBase methodBase)
        {
            return MethodRef.Create(methodBase);
        }

        /// <summary>
        /// Implicit conversion of a <see cref="PropertyInfo"/> to an <see cref="Expression"/> (actually, a <see cref="PropertyRef"/>).
        /// </summary>
        /// <remarks>This allows <see cref="PropertyInfo"/>s to be passed directly to any method expecting an <see cref="Expression"/> type without
        /// having to create a reference first.</remarks>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> to be converted.</param>
        /// <returns>A generated <see cref="PropertyRef"/> to the specified <see cref="PropertyInfo"/>.</returns>
        public static implicit operator Expression(PropertyInfo propertyInfo)
        {
            return new PropertyRef(propertyInfo);
        }

        /// <summary>
        /// Implicit conversion of a <see cref="EventInfo"/> to an <see cref="Expression"/> (actually, a <see cref="EventRef"/>).
        /// </summary>
        /// <remarks>This allows <see cref="EventInfo"/>s to be passed directly to any method expecting an <see cref="Expression"/> type without
        /// having to create a reference first.</remarks>
        /// <param name="eventInfo">The <see cref="EventInfo"/> to be converted.</param>
        /// <returns>A generated <see cref="EventRef"/> to the specified <see cref="EventInfo"/>.</returns>
        public static implicit operator Expression(EventInfo eventInfo)
        {
            return new EventRef(eventInfo);
        }

        /// <summary>
        /// Implicit conversion of a <see cref="FieldInfo"/> to an <see cref="Expression"/> (actually, a <see cref="FieldRef"/>).
        /// </summary>
        /// <remarks>This allows <see cref="FieldInfo"/>s to be passed directly to any method expecting an <see cref="Expression"/> type without
        /// having to create a reference first.</remarks>
        /// <param name="fieldInfo">The <see cref="FieldInfo"/> to be converted.</param>
        /// <returns>A generated <see cref="FieldRef"/> to the specified <see cref="FieldInfo"/>.</returns>
        public static implicit operator Expression(FieldInfo fieldInfo)
        {
            return new FieldRef(fieldInfo);
        }

        /// <summary>
        /// Implicit conversion of a <see cref="TypeParameter"/> to an <see cref="Expression"/> (actually, a <see cref="TypeParameterRef"/>).
        /// </summary>
        /// <remarks>This allows <see cref="TypeParameter"/>s to be passed directly to any method expecting an <see cref="Expression"/> type
        /// without having to create a reference first.</remarks>
        /// <param name="typeParameter">The <see cref="TypeParameter"/> to be converted.</param>
        /// <returns>A generated <see cref="TypeParameterRef"/> to the specified <see cref="TypeParameter"/>.</returns>
        public static implicit operator Expression(TypeParameter typeParameter)
        {
            return typeParameter.CreateRef();
        }

        /// <summary>
        /// Implicit conversion of a <see cref="Statement"/> to an <see cref="Expression"/>.
        /// </summary>
        /// <remarks>This allows declarations to be passed directly to any method expecting an <see cref="Expression"/>
        /// type without having to create a reference first.</remarks>
        /// <param name="statement">The <see cref="Statement"/> to be converted.</param>
        /// <returns>A generated <see cref="SymbolicRef"/> to the specified <see cref="Statement"/>.</returns>
        public static implicit operator Expression(Statement statement)
        {
            return statement.CreateRef();
        }

        /// <summary>
        /// Implicit conversion of a <c>string</c> to an <see cref="Expression"/> (actually, a <see cref="Literal"/>).
        /// </summary>
        /// <remarks>This allows strings to be passed directly to any method expecting
        /// an <see cref="Expression"/> type without having to do a <c>new Literal(value)</c>.</remarks>
        /// <param name="value">The <c>string</c> to be converted.</param>
        /// <returns>A generated <see cref="Literal"/> for the specified <c>string</c>.</returns>
        public static implicit operator Expression(string value)
        {
            return new Literal(value);
        }

        /// <summary>
        /// Implicit conversion of an <c>int</c> to an <see cref="Expression"/> (actually, a <see cref="Literal"/>).
        /// </summary>
        /// <remarks>This allows ints to be passed directly to any method expecting
        /// an <see cref="Expression"/> type without having to do a <c>new Literal(value)</c>.</remarks>
        /// <param name="value">The <c>int</c> to be converted.</param>
        /// <returns>A generated <see cref="Literal"/> for the specified <c>int</c>.</returns>
        public static implicit operator Expression(int value)
        {
            return new Literal(value);
        }

        /// <summary>
        /// Implicit conversion of a <c>uint</c> to an <see cref="Expression"/> (actually, a <see cref="Literal"/>).
        /// </summary>
        /// <remarks>This allows uints to be passed directly to any method expecting
        /// an <see cref="Expression"/> type without having to do a <c>new Literal(value)</c>.</remarks>
        /// <param name="value">The <c>uint</c> to be converted.</param>
        /// <returns>A generated <see cref="Literal"/> for the specified <c>uint</c>.</returns>
        public static implicit operator Expression(uint value)
        {
            return new Literal(value);
        }

        /// <summary>
        /// Implicit conversion of a <c>bool</c> to an <see cref="Expression"/> (actually, a <see cref="Literal"/>).
        /// </summary>
        /// <remarks>This allows bools to be passed directly to any method expecting
        /// an <see cref="Expression"/> type without having to do a <c>new Literal(value)</c>.</remarks>
        /// <param name="value">The <c>bool</c> to be converted.</param>
        /// <returns>A generated <see cref="Literal"/> for the specified <c>bool</c>.</returns>
        public static implicit operator Expression(bool value)
        {
            return new Literal(value);
        }

        /// <summary>
        /// Implicit conversion of a <c>char</c> to an <see cref="Expression"/> (actually, a <see cref="Literal"/>).
        /// </summary>
        /// <remarks>This allows chars to be passed directly to any method expecting
        /// an <see cref="Expression"/> type without having to do a <c>new Literal(value)</c>.</remarks>
        /// <param name="value">The <c>char</c> to be converted.</param>
        /// <returns>A generated <see cref="Literal"/> for the specified <c>char</c>.</returns>
        public static implicit operator Expression(char value)
        {
            return new Literal(value);
        }

        /// <summary>
        /// Implicit conversion of a <c>long</c> to an <see cref="Expression"/> (actually, a <see cref="Literal"/>).
        /// </summary>
        /// <remarks>This allows longs to be passed directly to any method expecting
        /// an <see cref="Expression"/> type without having to do a <c>new Literal(value)</c>.</remarks>
        /// <param name="value">The <c>long</c> to be converted.</param>
        /// <returns>A generated <see cref="Literal"/> for the specified <c>long</c>.</returns>
        public static implicit operator Expression(long value)
        {
            return new Literal(value);
        }

        /// <summary>
        /// Implicit conversion of a <c>ulong</c> to an <see cref="Expression"/> (actually, a <see cref="Literal"/>).
        /// </summary>
        /// <remarks>This allows ulongs to be passed directly to any method expecting
        /// an <see cref="Expression"/> type without having to do a <c>new Literal(value)</c>.</remarks>
        /// <param name="value">The <c>ulong</c> to be converted.</param>
        /// <returns>A generated <see cref="Literal"/> for the specified <c>ulong</c>.</returns>
        public static implicit operator Expression(ulong value)
        {
            return new Literal(value);
        }

        /// <summary>
        /// Implicit conversion of a <c>float</c> to an <see cref="Expression"/> (actually, a <see cref="Literal"/>).
        /// </summary>
        /// <remarks>This allows floats to be passed directly to any method expecting
        /// an <see cref="Expression"/> type without having to do a <c>new Literal(value)</c>.</remarks>
        /// <param name="value">The <c>float</c> to be converted.</param>
        /// <returns>A generated <see cref="Literal"/> for the specified <c>float</c>.</returns>
        public static implicit operator Expression(float value)
        {
            return new Literal(value);
        }

        /// <summary>
        /// Implicit conversion of a <c>double</c> to an <see cref="Expression"/> (actually, a <see cref="Literal"/>).
        /// </summary>
        /// <remarks>This allows doubles to be passed directly to any method expecting
        /// an <see cref="Expression"/> type without having to do a <c>new Literal(value)</c>.</remarks>
        /// <param name="value">The <c>double</c> to be converted.</param>
        /// <returns>A generated <see cref="Literal"/> for the specified <c>double</c>.</returns>
        public static implicit operator Expression(double value)
        {
            return new Literal(value);
        }

        /// <summary>
        /// Implicit conversion of a <c>decimal</c> to an <see cref="Expression"/> (actually, a <see cref="Literal"/>).
        /// </summary>
        /// <remarks>This allows decimals to be passed directly to any method expecting
        /// an <see cref="Expression"/> type without having to do a <c>new Literal(value)</c>.</remarks>
        /// <param name="value">The <c>decimal</c> to be converted.</param>
        /// <returns>A generated <see cref="Literal"/> for the specified <c>decimal</c>.</returns>
        public static implicit operator Expression(decimal value)
        {
            return new Literal(value);
        }

        public static Expression operator -(Expression l, Expression r)
        {
            return new Subtract(l, r);
        }

        public static Expression operator -(Expression expr)
        {
            return new Negative(expr);
        }

        public static Expression operator *(Expression l, Expression r)
        {
            return new Multiply(l, r);
        }

        public static Expression operator /(Expression l, Expression r)
        {
            return new Divide(l, r);
        }

        public static Expression operator +(Expression l, Expression r)
        {
            return new Add(l, r);
        }

        /// <summary>
        /// Parse an expression, stopping when default terminators, or the specified terminators, or a higher-precedence
        /// operator is encountered.
        /// </summary>
        /// <param name="parser">The parser object.</param>
        /// <param name="parent">The parent object.</param>
        /// <param name="isTopLevel">True if EOL comments can be associated with the expression during parsing - generally
        /// true if the parent is a statement or expression list, but with some exceptions.</param>
        /// <param name="terminator">Optional terminating characters (null if none).</param>
        /// <param name="flags">Parsing flags.</param>
        /// <returns>The parsed <see cref="Expression"/>.</returns>
        public static Expression Parse(Parser parser, CodeObject parent, bool isTopLevel, string terminator, ParseFlags flags)
        {
            // Save the starting token of the expression for later
            Token startingToken = parser.Token;

            // Start a new Unused list in the parser
            parser.PushUnusedList();

            // Parse an expression, which can be in one of the following formats:
            //   - An identifier token, optionally followed by an operator (which is parsed only if precedence rules determine it should be)
            //   - An operator (which will itself look for previous and/or following expressions when parsed)
            //   - An open paren, expression, close paren sequence (handled by the installed parse-point above), optionally followed by an operator
            // Any other sequence will cause parsing of the expression to cease.
            // The expression will be terminated by any of ';,}]', or other specified terminator.

            // Create a string of possible terminators (assuming 1 char terminators for now)
            string terminators = Statement.ParseTokenTerminator + ParseTokenSeparator + Block.ParseTokenEnd + Index.ParseTokenEnd + terminator;

            // Keep a reference to the last token so we can move any skipped non-EOL comments to the expression later
            Token lastToken = parser.LastToken;

            // Loop until EOF or we find a terminator, or for directive expressions stop if we find a comment or a token on a new line.
            // NOTE: Keep this logic in sync with the 'if' statement further down in the loop that checks for termination.
            while (parser.TokenText != null
                && (parser.TokenText.Length != 1 || terminators.IndexOf(parser.TokenText[0]) < 0)
                && (!parser.InDirectiveExpression || ((parser.LastToken.TrailingComments == null || parser.LastToken.TrailingComments.Count == 0) && !parser.Token.IsFirstOnLine)))
            {
            process_next:
                bool skipTerminationCheck = false;

                // Process the current token (will process operators)
                CodeObject obj = parser.ProcessToken(parent, flags | ParseFlags.Expression);
                if (obj != null)
                {
                    // If we got something, save it for later.
                    // Don't move any EOL comments here - they should have already been processed.

                    if (obj is CompilerDirective)
                    {
                        // If we have a compiler directive, and there's a preceeding unused object, add it there
                        CodeObject lastUnusedCodeObject = parser.LastUnusedCodeObject;
                        if (lastUnusedCodeObject != null && !(lastUnusedCodeObject is CompilerDirective))
                            lastUnusedCodeObject.AttachAnnotation((CompilerDirective)obj, AnnotationFlags.IsPostfix);
                        else
                        {
                            parser.AddUnused(obj);  // Add the object to the unused list
                            skipTerminationCheck = true;
                        }
                    }
                    else
                    {
                        obj.ParseUnusedAnnotations(parser, parent, true);  // Parse any annotations from the Unused list
                        parser.AddUnused(obj);                             // Add the object to the unused list
                    }
                }

                // Stop if EOF or we find a terminator, or for directive expressions stop if we find a comment or a token on a new line.
                // NOTE: Keep this logic in sync with that in the condition of the parent 'while' loop.
                if (parser.TokenText == null
                    || (parser.TokenText.Length == 1 && terminators.IndexOf(parser.TokenText[0]) >= 0)
                    || (parser.InDirectiveExpression && ((parser.LastToken.TrailingComments != null && parser.LastToken.TrailingComments.Count != 0) || parser.Token.IsFirstOnLine)))
                {
                    // Don't abort here on a '{' terminator if we're in a doc comment and we appear to have type arguments using
                    // braces (as opposed to an Initializer after a NewObject).  Go process the next object immediately instead.
                    if (parser.InDocComment && parser.TokenText == TypeRefBase.ParseTokenAltArgumentStart && parser.HasUnusedIdentifier
                        && TypeRefBase.PeekTypeArguments(parser, TypeRefBase.ParseTokenAltArgumentEnd, flags))
                        goto process_next;
                    break;
                }

                // If the current token is the start of a compiler directive, check for special situations in which we want to skip
                // the termination check below.  This allows the directive to be attached to preceeding code objects such as literals
                // or operators, while not attaching to simple name or type expressions which might be part of a namespace or type header.
                if (parser.TokenText == CompilerDirective.ParseToken)
                {
                    CodeObject lastUnusedCodeObject = parser.LastUnusedCodeObject;
                    if (lastUnusedCodeObject is Literal || lastUnusedCodeObject is Operator)
                    {
                        skipTerminationCheck = true;
                        // Also, capture any pending trailing comments
                        if (obj != null)
                            obj.MoveCommentsAsPost(parser.LastToken);
                    }
                }

                // If we don't have a specific terminating character, then we're parsing a sub-expression and we should stop when we
                // get to an invalid operator, or an operator of greater precedence.  Skip this check if we just parsed a compiler
                // directive and didn't have a preceeding code object to attach it to, or if we're about to parse a compiler directive
                // and we have an unused code object that we'd like to attach it to.
                if (terminator == null && !skipTerminationCheck)
                {
                    // Check for '{' when used inside a doc comment in a generic type constructor or generic method instance
                    if (parser.InDocComment && parser.TokenText == TypeRefBase.ParseTokenAltArgumentStart
                        && TypeRefBase.PeekTypeArguments(parser, TypeRefBase.ParseTokenAltArgumentEnd, flags))
                        continue;

                    // Check if the current token represents a valid operator
                    OperatorInfo operatorInfo = parser.GetOperatorInfoForToken();

                    // If the current token doesn't look like a valid operator, we're done with the expression
                    if (operatorInfo == null)
                        break;

                    // We have an operator - check if our parent is also an operator
                    if (parent is Operator)
                    {
                        // Special cases for Types:  Some operator symbols are overloaded and can also be part
                        // of a type name.  We must detect these here, and continue processing in these cases,
                        // skipping the operator precedence checks below that terminate the current expression.

                        // Check for '[' when used in an array type name
                        if (parser.TokenText == TypeRefBase.ParseTokenArrayStart && TypeRefBase.PeekArrayRanks(parser))
                            continue;

                        // Check for '<' when used in a generic type constructor or generic method instance
                        if (parser.TokenText == TypeRefBase.ParseTokenArgumentStart && TypeRefBase.PeekTypeArguments(parser, TypeRefBase.ParseTokenArgumentEnd, flags))
                            continue;

                        // Do NOT check for '?' used for nullable types, because it applies to the entire
                        // expression on the left, so we DO want to terminate processing.

                        // Determine the precedence of the parent operator
                        // NOTE: See the bottom of Operator.cs for a quick-reference of operator precedence.
                        int parentPrecedence = ((Operator)parent).GetPrecedence();

                        // Stop parsing if the parent operator has higher precedence
                        if (parentPrecedence < operatorInfo.Precedence)
                            break;

                        // If the parent has the same precedence, stop parsing if the operator is left-associative
                        if (parentPrecedence == operatorInfo.Precedence && operatorInfo.LeftAssociative)
                            break;
                    }
                }
            }

            // Get the expression
            Expression expression = parser.RemoveLastUnusedExpression();
            if (expression != null)
            {
                // Attach any skipped non-EOL comments from the front of the expression, but only if we're a top-level expression
                // (otherwise, comments that preceed a sub-expression will get attached to an outer expression instead).  This
                // prevents lost comments in places such as between a 'return' and the expression that follows.
                if (isTopLevel)
                    expression.MoveAllComments(lastToken);

                // If this is a top-level expression or if the next token is a close paren, move any trailing comments on the last
                // token of the expression as post comments. This prevents lost comments in places such as when some trailing parts of
                // an 'if' conditional expression are commented-out, or the trailing parts of any sub-expression before a close paren.
                if ((isTopLevel || parser.TokenText == ParseTokenEndGroup) && parser.LastToken.HasTrailingComments && !parser.InDirectiveExpression)
                    expression.MoveCommentsAsPost(parser.LastToken);
            }

            // Flush remaining unused objects as Unrecognized objects
            while (parser.HasUnused)
            {
                Expression preceedingUnused = parser.RemoveLastUnusedExpression(true);
                if (preceedingUnused != null)
                {
                    if (expression == null)
                        expression = new Unrecognized(false, parser.InDocComment, preceedingUnused);
                    else if (expression is Unrecognized && !expression.HasParens)
                        ((Unrecognized)expression).AddLeft(preceedingUnused);
                    else
                        expression = new Unrecognized(false, parser.InDocComment, preceedingUnused, expression);
                }
                else
                {
                    // If we have no expression to put them on, then parse any preceeding compiler directives into a temp object for later retrieval
                    if (expression == null)
                        expression = new TempExpr();
                    expression.ParseUnusedAnnotations(parser, parent, true);
                    break;
                }
            }
            if (expression is Unrecognized)
                ((Unrecognized)expression).UpdateMessage();

            parser.Unused.Clear();

            // Restore the previous Unused list in the parser
            parser.PopUnusedList();

            if (expression != null)
            {
                // Get any EOL comments
                if (parser.LastToken.HasTrailingComments)
                    expression.MoveEOLComment(parser.LastToken);

                // Set the parent starting token to the beginning of the expression
                parser.ParentStartingToken = startingToken;
            }

            return expression;
        }

        /// <summary>
        /// Parse an expression, stopping when default terminators, or the specified terminators, or a higher-precedence
        /// operator is encountered.
        /// </summary>
        /// <param name="parser">The parser object.</param>
        /// <param name="parent">The parent object.</param>
        /// <param name="isTopLevel">True if EOL comments can be associated with the expression during parsing - generally
        /// true if the parent is a statement or expression list, but with some exceptions.</param>
        /// <param name="terminator">Optional terminating characters (null if none).</param>
        /// <returns>The parsed <see cref="Expression"/>.</returns>
        public static Expression Parse(Parser parser, CodeObject parent, bool isTopLevel, string terminator)
        {
            return Parse(parser, parent, isTopLevel, terminator, ParseFlags.None);
        }

        /// <summary>
        /// Parse an expression, stopping when default terminators, or the specified terminators, or a higher-precedence
        /// operator is encountered.
        /// </summary>
        /// <param name="parser">The parser object.</param>
        /// <param name="parent">The parent object.</param>
        /// <param name="isTopLevel">True if EOL comments can be associated with the expression during parsing - generally
        /// true if the parent is a statement or expression list, but with some exceptions.</param>
        /// <returns>The parsed <see cref="Expression"/>.</returns>
        public static Expression Parse(Parser parser, CodeObject parent, bool isTopLevel)
        {
            return Parse(parser, parent, isTopLevel, null, ParseFlags.None);
        }

        /// <summary>
        /// Parse an expression, stopping when default terminators, or the specified terminators, or a higher-precedence
        /// operator is encountered.
        /// </summary>
        /// <param name="parser">The parser object.</param>
        /// <param name="parent">The parent object.</param>
        /// <returns>The parsed <see cref="Expression"/>.</returns>
        public static Expression Parse(Parser parser, CodeObject parent)
        {
            return Parse(parser, parent, false, null, ParseFlags.None);
        }

        public static Expression Parse(string src, out CodeUnit rootObject)
        {
            var root = new CodeUnit("inline-parse", src);
            Parser parser = new(root, ParseFlags.Expression);
            rootObject = root;

            // Parse the body until we hit EOF
            return Parse(parser, root);
        }

        /// <summary>
        /// Parse an <see cref="Expression"/>.
        /// </summary>
        public static Expression Parse(Parser parser, CodeObject parent, bool isTopLevel, ParseFlags flags)
        {
            return Parse(parser, parent, isTopLevel, null, flags);
        }

        /// <summary>
        /// Parse a directive <see cref="Expression"/>.
        /// </summary>
        public static Expression ParseDirectiveExpression(Parser parser, CodeObject parent)
        {
            parser.InDirectiveExpression = true;
            Expression expression = Parse(parser, parent, true);
            parser.InDirectiveExpression = false;
            return expression;
        }

        /// <summary>
        /// Parse a list of <see cref="Expression"/>s.
        /// </summary>
        public static ChildList<Expression> ParseList(Parser parser, CodeObject parent, string terminator, ParseFlags flags, bool allowSingleNullList)
        {
            ChildList<Expression> list = null;
            bool skipStatementTerminators = (terminator == Initializer.ParseTokenEnd);
            bool lastCommaFirstOnLine = false;
            while (true)
            {
                Expression expression = Parse(parser, parent, true, terminator, flags);
                bool hasComma = (parser.TokenText == ParseTokenSeparator);
                if (expression != null)
                {
                    // Force the expression to first-on-line if the last comma was (handles special-case
                    // formatting where the commas preceed the list items instead of following them).
                    if (lastCommaFirstOnLine)
                        expression.IsFirstOnLine = true;

                    // Get rid of any parens around the expression if they're not used on the code object by default
                    if (AutomaticFormattingCleanup && !parser.IsGenerated && expression.HasParens && !expression.HasParensDefault)
                        expression.HasParens = false;
                }

                if (expression is TempExpr)
                {
                    // If we got a TempExpr, move any directives as postfix on the previous expression
                    if (list != null && list.Count > 0)
                    {
                        Expression previous = list.Last;
                        foreach (Annotation annotation in expression.Annotations)
                        {
                            if (annotation is CompilerDirective)
                                previous.AttachAnnotation(annotation, AnnotationFlags.IsPostfix);
                        }
                    }
                }
                else if (expression != null || allowSingleNullList || hasComma)
                {
                    if (list == null)
                        list = new ChildList<Expression>(parent);
                    list.Add(expression);
                }

                // Continue processing if we have a ','.  Also treat ';' like a comma if we're parsing an Initializer
                // for better parsing of bad code (such as statements where an expression is expected).
                if (hasComma || (skipStatementTerminators && parser.TokenText == Statement.ParseTokenTerminator))
                {
                    lastCommaFirstOnLine = parser.Token.IsFirstOnLine;
                    parser.NextToken();  // Move past ',' (or ';')
                }
                else
                    break;
            }
            return list;
        }

        /// <summary>
        /// Parse a list of <see cref="Expression"/>s.
        /// </summary>
        public static ChildList<Expression> ParseList(Parser parser, CodeObject parent, string terminator)
        {
            return ParseList(parser, parent, terminator, ParseFlags.None, false);
        }

        /// <summary>
        /// Parse a parenthesized <see cref="Expression"/>.
        /// </summary>
        public static Expression ParseParenthesizedExpression(Parser parser, CodeObject parent, ParseFlags flags)
        {
            parser.SaveAndNextToken();  // Save '(' for now (remove when we find the matching close)

            // Parse the expression with our parent set to block bubble-up normalization of EOL comments.
            // This also handles proper parsing of nested Conditional expressions (resetting tracking if nested expressions have parens).
            parser.PushNormalizationBlocker(parent);
            Expression expression = Parse(parser, parent, false, ParseTokenEndGroup);
            parser.PopNormalizationBlocker();

            if (expression != null)
            {
                if (expression.ParseExpectedToken(parser, ParseTokenEndGroup))  // Move past ')'
                {
                    Token open = parser.RemoveLastUnusedToken();  // Remove '('
                    expression.MoveFormatting(open);
                    expression.HasParens = true;
                    if (parser.LastToken.IsFirstOnLine)
                        expression.IsEndFirstOnLine = true;

                    // Move any comments after the '(' to the argument expression as regular (inline if necessary) comments
                    expression.MoveCommentsToLeftMost(open, true);

                    // Associate any trailing EOL or inline comment on the ')'.
                    // Skip this for directive expressions, because there is no terminating character for
                    // directive expressions to be associated with, and they'll end up here all the time.
                    if (!parser.InDirectiveExpression)
                        expression.MoveEOLComment(parser.LastToken);
                }
            }
            return expression;
        }

        /// <summary>
        /// Determine if the specified comment should be associated with the current code object during parsing.
        /// </summary>
        public override bool AssociateCommentWhenParsing(CommentBase comment)
        {
            // Only associate regular comments with expressions, not doc comments
            return (comment is Comment);
        }

        public override void AsText(CodeWriter writer, RenderFlags flags)
        {
            int newLines = NewLines;
            bool isPrefix = flags.HasFlag(RenderFlags.IsPrefix);
            if (!isPrefix && newLines > 0)
            {
                if (!flags.HasFlag(RenderFlags.SuppressNewLine))
                    writer.WriteLines(newLines);
            }
            else if (flags.HasFlag(RenderFlags.PrefixSpace))
                writer.Write(" ");

            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            AsTextBefore(writer, passFlags | RenderFlags.IsPrefix);

            // Increase the indent level for any newlines that occur within the expression
            bool increaseIndent = flags.HasFlag(RenderFlags.IncreaseIndent);
            if (increaseIndent)
                writer.BeginIndentOnNewLine(this);

            bool hasParens = HasParens;
            if (hasParens)
                writer.Write(ParseTokenStartGroup);
            AsTextExpression(writer, passFlags | (flags & (RenderFlags.Attribute | RenderFlags.HasDotPrefix | RenderFlags.Declaration)));
            if (hasParens)
            {
                if (IsEndFirstOnLine)
                    writer.WriteLine();
                writer.Write(ParseTokenEndGroup);
            }
            if (HasTerminator && !flags.HasFlag(RenderFlags.Description))
            {
                writer.Write(Statement.ParseTokenTerminator);
                CheckForAlignment(writer);  // Check for alignment of any EOL comments
            }
            if (!flags.HasFlag(RenderFlags.NoEOLComments))
                AsTextEOLComments(writer, flags);

            AsTextAfter(writer, passFlags | (flags & RenderFlags.NoPostAnnotations));

            if (increaseIndent)
                writer.EndIndentation(this);

            if (isPrefix)
            {
                // If this object is rendered as a child prefix object of another, then any whitespace is
                // rendered here *after* the object instead of before it.
                if (newLines > 0)
                    writer.WriteLines(newLines);
                else if (!flags.HasFlag(RenderFlags.NoSpaceSuffix))
                    writer.Write(" ");
            }
        }

        public abstract void AsTextExpression(CodeWriter writer, RenderFlags flags);

        /// <summary>
        /// Get the expression on the left of the left-most <see cref="Dot"/> operator.
        /// </summary>
        public virtual Expression FirstPrefix()
        {
            return this;
        }

        /// <summary>
        /// Format an expression assigned as an argument to another code object (turns off any parentheses).
        /// </summary>
        public void FormatAsArgument()
        {
            // Clear the grouping (parentheses) flag regardless of if it was manually set
            _formatFlags &= ~(FormatFlags.Grouping | FormatFlags.GroupingSet);
        }

        /// <summary>
        /// Get the delegate parameters if the expression evaluates to a delegate type.
        /// </summary>
        public virtual ICollection GetDelegateParameters()
        {
            TypeRefBase typeRefBase = SkipPrefixes() as TypeRefBase;
            return (typeRefBase != null ? typeRefBase.GetDelegateParameters() : null);
        }

        /// <summary>
        /// Get the delegate return type if the expression evaluates to a delegate type.
        /// </summary>
        public virtual TypeRefBase GetDelegateReturnType()
        {
            TypeRefBase typeRefBase = SkipPrefixes() as TypeRefBase;
            return (typeRefBase != null ? typeRefBase.GetDelegateReturnType() : null);
        }

        /// <summary>
        /// Move any comments from the specified <see cref="Token"/> to the left-most sub-expression.
        /// </summary>
        public virtual void MoveCommentsToLeftMost(Token token, bool skipParens)
        {
            MoveAllComments(token);
        }

        /// <summary>
        /// Get the expression on the right of the right-most <see cref="Lookup"/> or <see cref="Dot"/> operator (bypass any '::' and '.' prefixes).
        /// </summary>
        public virtual Expression SkipPrefixes()
        {
            return this;
        }

        /// <summary>
        /// Default format the code object.
        /// </summary>
        protected internal override void DefaultFormat()
        {
            base.DefaultFormat();

            // Default the parens if they haven't been explicitly set
            if (!IsGroupingSet)
                _formatFlags = ((_formatFlags & ~FormatFlags.Grouping) | (HasParensDefault ? FormatFlags.Grouping : 0));
        }
    }
}