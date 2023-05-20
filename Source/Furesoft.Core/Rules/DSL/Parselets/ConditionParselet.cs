using Furesoft.PrattParser;
using Furesoft.PrattParser.Nodes;
using Furesoft.PrattParser.Nodes.Operators;
using Furesoft.PrattParser.Parselets;

namespace Furesoft.Core.Rules.DSL.Parselets;

public class ConditionParselet : IInfixParselet<AstNode>
{
    private readonly Dictionary<string[], Symbol> tokenMappins = new();

    public ConditionParselet()
    {
        tokenMappins.Add(new[]{"less", "than"}, "<");
        tokenMappins.Add(new[]{"greater", "than"}, ">");
        tokenMappins.Add(new[]{"equal", "to"}, "==");
    }

    private AstNode BuildNode(Parser<AstNode> parser, Symbol op, AstNode left)
    {
        var right = parser.Parse(GetBindingPower() - 1);

        return new BinaryOperatorNode(left, "<", right);
    }

    private Symbol MatchesMultipleTokensAsSingleToken(Parser<AstNode> parser, out int matchedTokenCount)
    {
        foreach (var mapping in tokenMappins)
        {
            for (uint i = 0; i < mapping.Key.Length; i++)
            {
                if (!parser.IsMatch(mapping.Key[i], i))
                {
                    goto nextMapping;
                }
            }
            
            found:
                matchedTokenCount = mapping.Key.Length;
                return mapping.Value;
            
            nextMapping:
                continue;

        }

        matchedTokenCount = 0;
        return "";
    }
    
    public AstNode Parse(Parser<AstNode> parser, AstNode left, Token token)
    {
        var symbol = MatchesMultipleTokensAsSingleToken(parser, out var matchedtokens);

        if (symbol.Name != "")
        {
            //ToDo: Consume all tokens
            
            return BuildNode(parser, symbol, left);
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