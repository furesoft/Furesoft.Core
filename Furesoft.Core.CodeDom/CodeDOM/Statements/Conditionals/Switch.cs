// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using Mono.Cecil;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals;
using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Conditionals.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Conditionals;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Jumps;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Methods;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Resolving;
using Furesoft.Core.CodeDom.Utilities.Reflection;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Conditionals
{
    /// <summary>
    /// Represents conditional flow control, and consists of a constant expression (of integral or string type)
    /// along with one or more <see cref="Case"/> or <see cref="Default"/> child statements.
    /// </summary>
    public class Switch : BlockStatement
    {
        #region /* FIELDS */

        protected Expression _target;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="Switch"/> on the specified target <see cref="Expression"/>.
        /// </summary>
        public Switch(Expression target)
        {
            Target = target;
        }

        /// <summary>
        /// Create a <see cref="Switch"/> on the specified target <see cref="Expression"/>.
        /// </summary>
        public Switch(Expression target, params SwitchItem[] items)
            : this(target)
        {
            foreach (SwitchItem item in items)
                Add(item);
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The target <see cref="Expression"/>.
        /// </summary>
        public Expression Target
        {
            get { return _target; }
            set { SetField(ref _target, value, true); }
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
        /// Add a <see cref="SwitchItem"/>.
        /// </summary>
        public void Add(SwitchItem item)
        {
            base.Add(item);
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            Switch clone = (Switch)base.Clone();
            clone.CloneField(ref clone._target, _target);
            return clone;
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "switch";

        internal static void AddParsePoints()
        {
            Parser.AddParsePoint(ParseToken, Parse, typeof(IBlock));
        }

        /// <summary>
        /// Parse a <see cref="Switch"/>.
        /// </summary>
        public static Switch Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Switch(parser, parent);
        }

        protected Switch(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            // Parse keyword, argument, and body
            // Do NOT do any post-processing in the body parsing, because we're going to do it below
            ParseKeywordArgumentBody(parser, ref _target, false, true);

            // Do some special processing of switch items:
            for (int i = _body.Count - 1; i >= 0; --i)
            {
                CodeObject item = _body[i];
                if (item is Break || item is Return)
                {
                    // Check for Break or Return statements outside of a SwitchItem block, and move them inside
                    // the previous block (this can occur when they are outside the SwitchItem's curly braces).
                    if (i > 0)
                    {
                        CodeObject previousItem = _body[i - 1];
                        if (previousItem is SwitchItem)
                        {
                            ((SwitchItem)previousItem).Body.Add(item);
                            _body.RemoveAt(i);
                        }
                    }
                }
                else if (item is CommentBase)
                {
                    // Merge any comments into adjacent SwitchItems if appropriate
                    _body.PostProcessComment((CommentBase)item, i);
                }
                else if (item is SwitchItem)
                {
                    // Check for SwitchItems without braces where the last object in the block is a conditional
                    // directive, and move them to the parent block if they don't seem to "belong" to the child.
                    // Also, if the last object is a DocComment, move it to the parent block.
                    Block itemBody = ((SwitchItem)item).Body;
                    if (itemBody != null && !itemBody.HasBraces)
                    {
                        bool move = false;
                        CodeObject last = itemBody.Last;
                        if (last is CompilerDirective)
                        {
                            move = true;
                            if (last is EndIfDirective)
                            {
                                // Don't move the endif if there's an associated conditional in the same block
                                for (int n = itemBody.Count - 2; n >= 0; --n)
                                {
                                    CodeObject itemMember = itemBody[n];
                                    if (itemMember is ConditionalDirective)
                                    {
                                        move = false;
                                        break;
                                    }
                                }
                            }
                        }
                        else if (last is DocComment)
                            move = true;
                        if (move)
                        {
                            // Move the object up to the parent block
                            _body.Insert(i + 1, last);
                            itemBody.RemoveAt(itemBody.Count - 1);
                        }
                    }
                }
            }
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve all child symbolic references, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            _target = (Expression)_target.Resolve(ResolveCategory.Expression, flags);

            // To allow forward references by any 'goto case ...' statements, resolve the constant expressions of all
            // child Case statements before resolving the body.
            if (_body != null)
            {
                foreach (Case @case in _body.Find<Case>())
                    @case.ResolveConstantExpression(flags);
            }

            return base.Resolve(ResolveCategory.CodeObject, flags);
        }

        /// <summary>
        /// Resolve child code objects that match the specified name, moving up the tree until a complete match is found.
        /// </summary>
        public override void ResolveRefUp(string name, Resolver resolver)
        {
            // If we're on our way up the code tree from a SwitchItem (case/default), we have to search the top block
            // of any other items that preceed the one we originated from *if* those items don't have braces around
            // their top block, so that we can match any local variable declarations.
            foreach (SwitchItem switchItem in _body.Find<SwitchItem>())
            {
                // It would be nice to skip searching the origin item, but it's hardly worth the trouble.  As with
                // locals elsewhere at the same level, we will match forward references, but the analysis phase will
                // detect these and report them as errors.
                Block body = switchItem.Body;
                if (body != null && !body.HasBraces)
                    switchItem.ResolveRef(name, resolver);
            }

            if (_parent != null && !resolver.HasCompleteMatch)
                _parent.ResolveRefUp(name, resolver);
        }

        /// <summary>
        /// Resolve child code objects that match the specified name and are valid goto targets, moving up the tree until a complete match is found.
        /// </summary>
        public override void ResolveGotoTargetUp(string name, Resolver resolver)
        {
            if (name.StartsWith("case "))
            {
                // Handle a "goto case ...":
                if (_body != null)
                {
                    // Although case statements are named objects, we can't just match on their name - we need to match if there's
                    // an implicit conversion between the goto and target expressions.  Also, a case expression doesn't have to be
                    // a Literal, but can be a constant expression with a Cast or other operators.
                    Goto associatedGoto = resolver.UnresolvedRef.Parent as Goto;
                    if (associatedGoto != null)
                    {
                        TypeRef targetTypeRef = associatedGoto.ConstantExpression.EvaluateType() as TypeRef;
                        TypeRef switchTypeRef = Target.EvaluateType() as TypeRef;
                        if (targetTypeRef != null && switchTypeRef != null)
                        {
                            // Verify that the target type is implicitly convertible to the Switch type.
                            // If not, create a warning, but continue to attempt resolution of the target reference anyway.
                            associatedGoto.RemoveAllMessages(MessageSource.Resolve);
                            if (!targetTypeRef.IsImplicitlyConvertibleTo(switchTypeRef))
                            {
                                associatedGoto.AttachMessage("The 'goto case' value isn't implicitly convertible to the 'switch' type '" + switchTypeRef.AsString() + "'.",
                                    MessageSeverity.Warning, MessageSource.Resolve);
                            }
                            if (targetTypeRef.IsEnum)
                            {
                                // Handle enum types
                                foreach (Case @case in _body.Find<Case>())
                                {
                                    // Verify that the enum types are the same
                                    TypeRefBase caseConstantRef = @case.ConstantExpression.EvaluateType();
                                    if (targetTypeRef.IsSameRef(caseConstantRef))
                                    {
                                        // Verify that the enum constant values are identical using Equals().  Also, verify that they're
                                        // indeed constants (they might not be if the enum members were still UnresolvedRefs).
                                        if (targetTypeRef.IsConst && caseConstantRef.IsConst)
                                        {
                                            if (((EnumConstant)targetTypeRef.GetConstantValue()).ConstantValue.Equals(((EnumConstant)caseConstantRef.GetConstantValue()).ConstantValue))
                                                resolver.AddMatch(@case);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Handle built-in types - convert the targetTypeRef constant to the type of the Switch.
                                Type switchType = (switchTypeRef.Reference is TypeReference ? TypeRef.GetEquivalentType((TypeReference)switchTypeRef.Reference) : switchTypeRef.Reference as Type);
                                object targetConstant = targetTypeRef.GetConstantValue();

                                // If the target constant isn't a null reference, try to convert it to the Switch type
                                bool conversionSuccessful = true;
                                if (targetConstant != null)
                                {
                                    targetConstant = TypeUtil.ChangeType(targetConstant, switchType);
                                    if (targetConstant == null)
                                        conversionSuccessful = false;
                                }
                                if (conversionSuccessful)
                                {
                                    foreach (Case @case in _body.Find<Case>())
                                    {
                                        // Get the case constant, and make sure it's also of the Switch type (it might not be)
                                        object caseConstant = @case.ConstantExpression.EvaluateType().GetConstantValue();
                                        caseConstant = TypeUtil.ChangeType(caseConstant, switchType);

                                        // Compare the target constant object to the case constant using Equals() to do a value compare instead of a reference compare
                                        bool matches = (caseConstant == null ? targetConstant == null : caseConstant.Equals(targetConstant));
                                        if (matches)
                                            resolver.AddMatch(@case);
                                    }
                                }
                            }
                        }
                    }
                }
                // The search for a "goto case ..." stops at the first enclosing Switch statement
            }
            else if (name == Default.ParseToken)
            {
                // Handle a "goto default":
                resolver.AddMatch(Find<Default>());
                // The search for a "goto default" stops at the first enclosing Switch statement
            }
            else
            {
                // Handle a "goto <label>":

                // As we're on our way up the code tree from a SwitchItem (case/default), we have to search
                // the top block of any other SwitchItems (preceeding OR following) for any labels *if* they
                // don't have braces around their top block.
                foreach (SwitchItem switchItem in Find<SwitchItem>())
                {
                    // It would be nice to skip searching the origin item, but it's hardly worth the trouble
                    Block body = switchItem.Body;
                    if (body != null && !body.HasBraces)
                        body.ResolveGotoTargetUp(name, resolver);
                }

                // If we didn't find a match, continue looking through parent scopes
                if (_parent != null && !resolver.HasCompleteMatch)
                    _parent.ResolveGotoTargetUp(name, resolver);
            }
        }

        /// <summary>
        /// Returns true if the code object is an <see cref="UnresolvedRef"/> or has any <see cref="UnresolvedRef"/> children.
        /// </summary>
        public override bool HasUnresolvedRef()
        {
            if (_target != null && _target.HasUnresolvedRef())
                return true;
            return base.HasUnresolvedRef();
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// True if the <see cref="Statement"/> has an argument.
        /// </summary>
        public override bool HasArgument
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

        protected override void AsTextAfter(CodeWriter writer, RenderFlags flags)
        {
            if (!flags.HasFlag(RenderFlags.NoPostAnnotations))
                AsTextAnnotations(writer, AnnotationFlags.IsPostfix, flags);

            if (_body != null && !flags.HasFlag(RenderFlags.Description))
            {
                // Check for alignment of same-line single-line SwitchItem bodies
                int alignmentOffset = 0;
                foreach (SwitchItem switchItem in _body.Find<SwitchItem>())
                {
                    // If the SwitchItem body is on the same line (and is a single line), then calculate the
                    // common alignment offset.  Ignore any SwitchItems if they're not first-on-line.
                    bool formatOK = false;
                    if (switchItem.IsFirstOnLine)
                    {
                        Block switchItemBody = switchItem.Body;
                        if (switchItemBody != null && switchItemBody.Count > 0 && !switchItemBody.IsFirstOnLine && switchItemBody.IsSingleLine)
                        {
                            formatOK = true;
                            int bodyOffset = switchItem.AsTextLength(RenderFlags.Description | RenderFlags.LengthFlags);
                            if (bodyOffset > alignmentOffset)
                                alignmentOffset = bodyOffset;
                        }
                    }
                    if (!formatOK)
                    {
                        // If the SwitchItem doesn't fit the right pattern, abort the formatting
                        alignmentOffset = 0;
                        break;
                    }
                }

                // If we're aligning, create an alignment state to hold the alignment offset value so the SwitchItems can find it
                if (alignmentOffset > 0)
                    writer.BeginAlignment(this, new[] { alignmentOffset });

                _body.AsText(writer, flags);

                if (alignmentOffset > 0)
                    writer.EndAlignment(this);
            }
        }

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            _target.AsText(writer, flags);
        }

        #endregion
    }
}
