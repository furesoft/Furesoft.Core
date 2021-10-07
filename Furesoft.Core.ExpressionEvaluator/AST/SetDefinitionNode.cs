using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Conditional;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.ExpressionEvaluator.AST
{
    public class SetDefinitionNode : Statement, IBindable
    {
        public SetDefinitionNode(Parser parser, CodeObject parent) : base(parser, parent)
        {
        }

        public Expression Condition { get; set; }
        public Expression Name { get; set; }
        public Expression Value { get; set; }

        public static void AddParsePoints()
        {
            Parser.AddParsePoint("set", Parse);
        }

        public CodeObject Bind(ExpressionParser ep, Binder binder)
        {
            if (Name is UnresolvedRef nameRef)
            {
                string name = nameRef.Reference.ToString();

                if (name == name.ToUpper())
                {
                    if (Condition != null)
                    {
                        Condition = binder.BindNumberRoom(Condition); //ToDo: need to fix no variables
                    }

                    if (!ep.RootScope.SetDefinitions.ContainsKey(name))
                    {
                        ep.RootScope.SetDefinitions.Add(name, new And(Value, Condition));
                    }
                    else
                    {
                        AttachMessage($"Set '{name}' already exists.", MessageSeverity.Error, MessageSource.Resolve);
                    }
                }
                else
                {
                    AttachMessage($"Set '{name}' need to be uppercase", MessageSeverity.Error, MessageSource.Parse);
                }
            }

            return this;
        }

        private static CodeObject Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            //  set P in N = 1 < x && x % 1 == 0 && x % x == 0;

            // set MP in P = x < 100;
            // oder:
            // set D = {0,1,2,3,4,5,6};
            var node = new SetDefinitionNode(parser, parent);

            parser.NextToken();

            node.Name = new UnresolvedRef(parser.GetIdentifierText());

            if (parser.GetIdentifierText() == "in")
            {
                node.Condition = new UnresolvedRef(parser.GetIdentifierText());
            }

            if (!node.ParseExpectedToken(parser, "="))
                return null;

            node.Value = Expression.Parse(parser, node);

            return node;
        }
    }
}