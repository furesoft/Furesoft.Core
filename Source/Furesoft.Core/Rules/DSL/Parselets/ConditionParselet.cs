using Furesoft.PrattParser;
using Furesoft.PrattParser.Nodes;
using Furesoft.PrattParser.Nodes.Operators;
using Furesoft.PrattParser.Parselets;
using Furesoft.PrattParser.Text;

namespace Furesoft.Core.Rules.DSL.Parselets;

public class ConditionParselet : IInfixParselet<AstNode>
{
    private readonly Dictionary<string[], Symbol> _tokenMappins = new();

    public ConditionParselet()
    {
        _tokenMappins.Add(new[]{"less", "than"}, "<");
        _tokenMappins.Add(new[]{"less", "than", "or", "equal"}, "<=");
        _tokenMappins.Add(new[]{"greater", "than", "or", "equal"}, ">=");
        _tokenMappins.Add(new[]{"greater", "than"}, ">");
        _tokenMappins.Add(new[]{"equal", "to"}, "==");
        _tokenMappins.Add(new[]{"not", "equal", "to"}, "!=");
    }

    private AstNode BuildNode(Parser<AstNode> parser, Symbol op, AstNode left)
    {
        var right = parser.Parse(GetBindingPower() - 1);

        return new BinaryOperatorNode(left, op, right);
    }

    private Symbol MatchesMultipleTokensAsSingleToken(Parser<AstNode> parser)
    {
        foreach (var mapping in _tokenMappins)
        {
            for (uint i = 0; i < mapping.Key.Length; i++)
            {
                if (!parser.IsMatch(mapping.Key[i], i))
                {
                    goto nextMapping;
                }
            }
            
            parser.ConsumeMany((uint)mapping.Key.Length);
            return mapping.Value;
            
            nextMapping:
                continue;

        }
        
        return "";
    }
    
    public AstNode Parse(Parser<AstNode> parser, AstNode left, Token token)
    {
        var symbol = MatchesMultipleTokensAsSingleToken(parser);

        if (symbol.Name != "")
        {
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