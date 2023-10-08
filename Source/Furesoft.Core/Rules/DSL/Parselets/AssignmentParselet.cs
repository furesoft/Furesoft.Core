using Furesoft.PrattParser;
using Furesoft.PrattParser.Nodes;
using Furesoft.PrattParser.Nodes.Operators;
using Furesoft.PrattParser.Parselets;

namespace Furesoft.Core.Rules.DSL.Parselets;

public class AssignmentParselet : IPrefixParselet
{
    public AstNode Parse(Parser parser, Token token)
    {
        // set x to value
        var name = parser.Parse();

        parser.Match("to");
        var value = parser.Parse(BindingPower.Assignment - 1);

        return new BinaryOperatorNode(name, "=", value);
    }

    public int GetBindingPower()
    {
        return BindingPower.Assignment;
    }
}