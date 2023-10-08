using Furesoft.Core.Rules.DSL.Nodes;
using Furesoft.PrattParser;
using Furesoft.PrattParser.Nodes;
using Furesoft.PrattParser.Parselets;

namespace Furesoft.Core.Rules.DSL.Parselets;

public class IfParselet : IPrefixParselet
{
    public AstNode Parse(Parser parser, Token token)
    {
        var condition = parser.Parse(GetBindingPower() - 1);
        parser.Match("then");
        var body = parser.Parse();

        return new IfNode(condition, body);
    }

    public int GetBindingPower()
    {
        return (int) BindingPower.Product - 1;
    }
}