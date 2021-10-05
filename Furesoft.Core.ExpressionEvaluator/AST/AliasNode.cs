using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.ExpressionEvaluator.AST
{
    public class AliasNode : Statement, IBindable
    {
        public AliasNode(Parser parser, CodeObject parent) : base(parser, parent)
        {
        }

        public Expression Name { get; set; }
        public Expression Value { get; set; }

        public static void AddParsePoints()
        {
            Parser.AddParsePoint("alias", Parse);
        }

        public CodeObject Bind(ExpressionParser ep)
        {
            if (Name is UnresolvedRef nameRef)
            {
                string name = nameRef.Reference.ToString();

                if (!ep.RootScope.Aliases.ContainsKey(name))
                {
                    ep.RootScope.Aliases.Add(name, Value);
                }
                else
                {
                    AttachMessage($"Alias '{name}' already exists.", MessageSeverity.Error, MessageSource.Resolve);
                }
            }

            return this;
        }

        private static CodeObject Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            var node = new AliasNode(parser, parent);

            parser.NextToken();
            node.Value = Expression.Parse(parser, node);

            if (!node.ParseExpectedToken(parser, "as"))
                return null;

            node.Name = Expression.Parse(parser, node, false, ";");

            //alias b as bogenmaß;

            return node;
        }
    }
}