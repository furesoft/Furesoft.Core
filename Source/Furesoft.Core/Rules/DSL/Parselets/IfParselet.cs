using Furesoft.Core.Rules.DSL.Nodes;
using Furesoft.PrattParser;
using Furesoft.PrattParser.Nodes;
using Furesoft.PrattParser.Parselets;

namespace Furesoft.Core.Rules.DSL.Parselets;

public class IfParselet : IPrefixParselet<AstNode>
{
    public int GetBindingPower()
    {
        return (int)BindingPower.Conditional + 1;
    }

    public AstNode Parse(Parser<AstNode> parser, Token token)
    {
        var condition = parser.Parse(GetBindingPower() - 1);
        parser.Match("then");
        var body = parser.Parse(GetBindingPower() - 1);

        return new IfNode(condition, body);
    }
}