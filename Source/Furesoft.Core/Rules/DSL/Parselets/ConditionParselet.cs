using Furesoft.PrattParser;
using Furesoft.PrattParser.Nodes;
using Furesoft.PrattParser.Nodes.Operators;
using Furesoft.PrattParser.Parselets;
using Furesoft.PrattParser.Text;

namespace Furesoft.Core.Rules.DSL.Parselets;

public class ConditionParselet : IInfixParselet
{
    private readonly Dictionary<string[], Symbol> _tokenMappins = [];
    private readonly int bindingPower;

    public ConditionParselet(int bindingPower)
    {
        _tokenMappins.Add(["less", "than"], "<");
        _tokenMappins.Add(["less", "than", "or", "equal"], "<=");
        _tokenMappins.Add(["greater", "than", "or", "equal"], ">=");
        _tokenMappins.Add(["greater", "than"], ">");
        _tokenMappins.Add(["equal", "to"], "==");
        _tokenMappins.Add(["not", "equal", "to"], "!=");

        this.bindingPower = bindingPower;
    }

    public AstNode Parse(Parser parser, AstNode left, Token token)
    {
        var symbol = MatchesMultipleTokensAsSingleToken(parser);

        if (symbol.Name != "") return BuildNode(parser, symbol, left);

        var currentToken = parser.Consume();

        token.Document.Messages.Add(
            Message.Error("Unknown Condition Pattern",
                new(token.Document, token.GetSourceSpanStart(), currentToken.GetSourceSpanEnd())));

        return null;
    }

    public int GetBindingPower() => bindingPower;

    private AstNode BuildNode(Parser parser, Symbol op, AstNode left)
    {
        var right = parser.Parse(GetBindingPower() - 1);

        return new BinaryOperatorNode(left, op, right);
    }

    private Symbol MatchesMultipleTokensAsSingleToken(Parser parser)
    {
        foreach (var mapping in _tokenMappins)
        {
            for (uint i = 0; i < mapping.Key.Length; i++)
                if (!parser.IsMatch(mapping.Key[i], i))
                    goto nextMapping;

            parser.ConsumeMany((uint) mapping.Key.Length);
            return mapping.Value;

            nextMapping: ;
        }

        return "";
    }
}