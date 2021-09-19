using System.Collections.Generic;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a user-overloaded operator.
    /// Conversion operators use the derived class <see cref="ConversionOperatorDecl"/>.
    /// </summary>
    /// <remarks>
    /// All overloaded operators must be public and static.
    /// Overloaded unary operators have a single parameter which must be of the containing type.
    /// Overloaded binary operators have two parameters, at least one of which must be of the containing type.
    /// </remarks>
    public class OperatorDecl : MethodDecl
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "operator";

        protected string _symbol;  // The overloaded operator's symbol (used for rendering)

        private static readonly string[,] _binaryMapData =
                            {
                { CodeDOM.Add.ParseToken,      CodeDOM.Add.InternalName      },
                { Subtract.ParseToken,         Subtract.InternalName         },
                { Multiply.ParseToken,         Multiply.InternalName         },
                { Divide.ParseToken,           Divide.InternalName           },
                { Mod.ParseToken,              Mod.InternalName              },
                { BitwiseAnd.ParseToken,       BitwiseAnd.InternalName       },
                { BitwiseOr.ParseToken,        BitwiseOr.InternalName        },
                { BitwiseXor.ParseToken,       BitwiseXor.InternalName       },
                { Equal.ParseToken,            Equal.InternalName            },
                { NotEqual.ParseToken,         NotEqual.InternalName         },
                { LessThan.ParseToken,         LessThan.InternalName         },
                { GreaterThan.ParseToken,      GreaterThan.InternalName      },
                { LessThanEqual.ParseToken,    LessThanEqual.InternalName    },
                { GreaterThanEqual.ParseToken, GreaterThanEqual.InternalName },
                { LeftShift.ParseToken,        LeftShift.InternalName        },
                { RightShift.ParseToken,       RightShift.InternalName       }
            };

        // Data arrays used to initialize the dictionaries above
        private static readonly string[,] _unaryMapData =
            {
                { Not.ParseToken,        Not.InternalName        },
                { Complement.ParseToken, Complement.InternalName },
                { Positive.ParseToken,   Positive.InternalName   },
                { Negative.ParseToken,   Negative.InternalName   },
                { Increment.ParseToken,  Increment.InternalName  },
                { Decrement.ParseToken,  Decrement.InternalName  }
            };

        private static Dictionary<string, string> _internalNameToSymbolMap;

        // Dictionaries for looking up operator names from their symbols and vice-versa
        private static Dictionary<string, string> _symbolToInternalNameMap;

        /// <summary>
        /// Create an <see cref="OperatorDecl"/> for the specified operator symbol, return type, and modifiers.
        /// </summary>
        public OperatorDecl(string symbol, Expression returnType, Modifiers modifiers, CodeObject body, params ParameterDecl[] parameters)
            : base(GetOperatorInternalName(symbol, (parameters != null ? parameters.Length : 0)), returnType, modifiers, body, parameters)
        {
            _symbol = symbol;
        }

        /// <summary>
        /// Create an <see cref="OperatorDecl"/> for the specified operator symbol, return type, and modifiers.
        /// </summary>
        public OperatorDecl(string symbol, Expression returnType, Modifiers modifiers, params ParameterDecl[] parameters)
            : this(symbol, returnType, modifiers, new Block(), parameters)
        { }

        protected OperatorDecl(Parser parser, CodeObject parent, bool parse, ParseFlags flags)
                    : base(parser, parent, false, flags)
        {
            if (parse)
            {
                parser.NextToken();  // Move past 'operator'
                if (parser.TokenText != ParseTokenStart)
                {
                    _symbol = parser.TokenText;  // Parse the symbol
                    parser.NextToken();          // Move past the symbol
                }
                //else // Parse error if symbol is missing

                ParseUnusedType(parser, ref _returnType);  // Parse the return type from the Unused list
                ParseParameters(parser);
                _name = GetOperatorInternalName(_symbol, (_parameters != null ? _parameters.Count : 0));

                // Parse any optional attributes and/or modifiers.  Do this after the main part of the
                // statement has been parsed so that it can be replicated if necessary with different
                // modifiers on each part, but before any base-type list or constraints.
                ParseModifiersAndAnnotations(parser);

                ParseTerminatorOrBody(parser, flags);
            }
        }

        /// <summary>
        /// The keyword associated with the <see cref="Statement"/>.
        /// </summary>
        public override string Keyword
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// The associated operator symbol.
        /// </summary>
        public string Symbol
        {
            get { return _symbol; }
        }

        /// <summary>
        /// Determine the symbol for an operator given its internal name.
        /// </summary>
        public static string GetOperatorSymbol(string internalName)
        {
            if (_internalNameToSymbolMap == null)
            {
                // If it hasn't been done yet, build the necessary dictionary
                _internalNameToSymbolMap = new Dictionary<string, string>();
                for (int i = 0; i < _unaryMapData.GetLength(0); ++i)
                    _internalNameToSymbolMap.Add(_unaryMapData[i, 1], _unaryMapData[i, 0]);
                for (int i = 0; i < _binaryMapData.GetLength(0); ++i)
                    _internalNameToSymbolMap.Add(_binaryMapData[i, 1], _binaryMapData[i, 0]);
            }
            string symbol;
            _internalNameToSymbolMap.TryGetValue(internalName, out symbol);
            return symbol;
        }

        /// <summary>
        /// Parse an <see cref="OperatorDecl"/>.
        /// </summary>
        public static new OperatorDecl Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Handle conversion operators
            if (ModifiersHelpers.IsModifier(parser.LastUnusedTokenText))
                return new ConversionOperatorDecl(parser, parent, flags);
            // Handle other operators
            return new OperatorDecl(parser, parent, true, flags);
        }

        /// <summary>
        /// Create a reference to the <see cref="OperatorDecl"/>.
        /// </summary>
        /// <param name="isFirstOnLine">True if the reference should be displayed on a new line.</param>
        /// <returns>An <see cref="OperatorRef"/>.</returns>
        public override SymbolicRef CreateRef(bool isFirstOnLine)
        {
            return new OperatorRef(this, isFirstOnLine);
        }

        /// <summary>
        /// Get the full name of the <see cref="INamedCodeObject"/>, including any namespace name.
        /// </summary>
        /// <param name="descriptive">True to display type parameters and method parameters, otherwise false.</param>
        public override string GetFullName(bool descriptive)
        {
            string name = ParseToken + " " + _symbol;
            if (descriptive)
                name += GetParametersAsString();
            if (_parent is TypeDecl)
                name = ((TypeDecl)_parent).GetFullName(descriptive) + "." + name;
            return name;
        }

        internal static new void AddParsePoints()
        {
            // Operator declarations are only valid with a TypeDecl parent, but we'll allow any IBlock so that we can
            // properly parse them if they accidentally end up at the wrong level (only to flag them as errors).
            // This also allows for them to be embedded in a DocCode object.
            Parser.AddParsePoint(ParseToken, Parse, typeof(IBlock));
        }

        internal override void AsTextName(CodeWriter writer, RenderFlags flags)
        {
            writer.Write(_symbol);
        }

        // Determine the internal name for an operator given its symbol and parameter count (1 or 2).
        protected static string GetOperatorInternalName(string symbol, int parameterCount)
        {
            if (_symbolToInternalNameMap == null)
            {
                // If it hasn't been done yet, build the necessary dictionary
                _symbolToInternalNameMap = new Dictionary<string, string>();
                for (int i = 0; i < _unaryMapData.GetLength(0); ++i)
                    _symbolToInternalNameMap.Add(_unaryMapData[i, 0] + "`1", _unaryMapData[i, 1]);
                for (int i = 0; i < _binaryMapData.GetLength(0); ++i)
                    _symbolToInternalNameMap.Add(_binaryMapData[i, 0] + "`2", _binaryMapData[i, 1]);
            }
            string name;
            _symbolToInternalNameMap.TryGetValue(symbol + '`' + parameterCount, out name);
            return name ?? symbol;
        }

        protected override void AsTextStatement(CodeWriter writer, RenderFlags flags)
        {
            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            if (_returnType != null)
                _returnType.AsText(writer, passFlags | RenderFlags.IsPrefix);
            UpdateLineCol(writer, flags);
            writer.Write(ParseToken + " ");
            if (flags.HasFlag(RenderFlags.Description) && _parent is TypeDecl)
            {
                ((TypeDecl)_parent).AsTextName(writer, flags);
                writer.Write(Dot.ParseToken);
            }
            AsTextName(writer, flags);
        }
    }
}