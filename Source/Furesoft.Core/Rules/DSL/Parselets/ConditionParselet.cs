using Silverfly;
using Silverfly.Nodes;
using Silverfly.Nodes.Operators;
using Silverfly.Parselets;
using Silverfly.Text;

namespace Furesoft.Core.Rules.DSL.Parselets;

public class ConditionParselet : IInfixParselet
{
    private readonly Dictionary<string[], Symbol> _tokenMappings = [];
    private readonly int bindingPower;

    public ConditionParselet(int bindingPower)
    {
        _tokenMappings.Add(["less", "than"], "<");
        _tokenMappings.Add(["less", "than", "or", "equal"], "<=");
        _tokenMappings.Add(["greater", "than", "or", "equal"], ">=");
        _tokenMappings.Add(["greater", "than"], ">");
        _tokenMappings.Add(["equal", "to"], "==");
        _tokenMappings.Add(["not", "equal", "to"], "!=");

        _tokenMappings.Add(["divisible", "by"], "%.");

        this.bindingPower = bindingPower;
    }

    public AstNode Parse(Parser parser, AstNode left, Token token)
    {
        var symbol = MatchesMultipleTokensAsSingleToken(parser);

        if (symbol.Name != "") return BuildNode(parser, symbol, left);

        var currentToken = parser.Consume();

        token.Document.Messages.Add(
            Message.Error($"Unknown Condition Pattern '{currentToken.Text}'",
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
        foreach (var mapping in _tokenMappings)
        {
            for (uint i = 0; i < mapping.Key.Length; i++)
                if (!parser.IsMatch(mapping.Key[i], i))
                    goto nextMapping;

            parser.ConsumeMany((uint)mapping.Key.Length);
            return mapping.Value;

        nextMapping:;
        }

        return "";
    }
}