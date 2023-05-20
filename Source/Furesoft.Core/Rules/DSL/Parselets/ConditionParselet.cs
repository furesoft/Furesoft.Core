using Furesoft.PrattParser;
using Furesoft.PrattParser.Nodes;
using Furesoft.PrattParser.Nodes.Operators;
using Furesoft.PrattParser.Parselets;

namespace Furesoft.Core.Rules.DSL.Parselets;

public class ConditionParselet : IInfixParselet<AstNode>
{
    private AstNode BuildNode(Parser<AstNode> parser, Symbol op, AstNode left)
    {
        var right = parser.Parse(GetBindingPower() - 1);

        return new BinaryOperatorNode(left, "<", right);
    }
    
    public AstNode Parse(Parser<AstNode> parser, AstNode left, Token token)
    {
        //ToDo: Cleanup ConditionParselet
        if (parser.Match("less"))
        {
            if (parser.Match("than"))
            {
                return BuildNode(parser, "<", left);
            }
        }
        else if (parser.Match("greater"))
        {
            if (parser.Match("than"))
            {
                return BuildNode(parser, ">", left);
            }
        }

        var currentToken = parser.Consume();
        
        token.Document.Messages.Add(
            Message.Error("Unknown Condition Pattern", 
                new(token.Document, token.GetSourceSpanStart(), currentToken.GetSourceSpanEnd())));

        return null;
    }

    public int GetBindingPower()
    {
        return (int)BindingPower.Product;
    }
}