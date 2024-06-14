using Furesoft.PrattParser;
using Furesoft.PrattParser.Nodes;
using Furesoft.PrattParser.Nodes.Operators;
using Furesoft.PrattParser.Parselets;

namespace Furesoft.Core.Rules.DSL.Parselets;

public class AssignmentParselet(int bindingPower) : IPrefixParselet
{
    public AstNode Parse(Parser parser, Token token)
    {
        // set x to value
        var name = parser.ParseExpression();

        parser.Match("to");
        var value = parser.Parse(bindingPower - 1);

        return new BinaryOperatorNode(name, "=", value);
    }

    public int GetBindingPower() => bindingPower;
}