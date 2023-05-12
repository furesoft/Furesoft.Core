using Furesoft.Core.CodeDom.CodeDOM.Annotations;

namespace Furesoft.Core.ExpressionEvaluator.AST;

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
        if (Name is UnresolvedRef nameRef && nameRef.Reference is string name)
        {
            if (name == name.ToUpper())
            {
                if (Condition != null)
                {
                    Condition = binder.BindNumberSet(Condition); //ToDo: need to fix no variables
                }

                if (!ep.RootScope.SetDefinitions.ContainsKey(name))
                {
                    if (Value is SetDefinitionExpression setExpr)
                    {
                        Value = BindSetDefinitionExpression(setExpr);
                    }

                    if (Condition != null)
                    {
                        ep.RootScope.SetDefinitions.Add(name, new And(Value, Condition));
                    }
                    else
                    {
                        ep.RootScope.SetDefinitions.Add(name, Value);
                    }
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

    private Expression BindSetDefinitionExpression(SetDefinitionExpression setExpr)
    {
        if (setExpr.Value is ChildList<Expression> nodes)
        {
            return BindSetList(nodes);
        }

        return setExpr;
    }

    private Expression BindSetList(ChildList<Expression> nodes)
    {
        var reference = new UnresolvedRef("x");

        if (nodes.Count > 0)
        {
            var value = nodes[0];
            nodes.RemoveAt(0);

            return new Or(new Equal(reference, value), BindSetList(nodes));
        }

        return new And(new Literal(1), new Literal(2));
    }
}