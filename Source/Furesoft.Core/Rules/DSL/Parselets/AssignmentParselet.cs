using Silverfly;
using Silverfly.Nodes;
using Silverfly.Nodes.Operators;
using Silverfly.Parselets;

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