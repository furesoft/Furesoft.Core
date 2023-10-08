using System.Collections.Immutable;
using Furesoft.Core.Rules.DSL.Nodes;
using Furesoft.PrattParser;
using Furesoft.PrattParser.Nodes;
using Furesoft.PrattParser.Nodes.Operators;
using Furesoft.PrattParser.Parselets;
using Furesoft.PrattParser.Parselets.Literals;

namespace Furesoft.Core.Rules.DSL.Parselets;

public class TimeLiteralParselet : IPrefixParselet
{
    public static readonly ImmutableDictionary<string, string> TimePostfixConverters = new Dictionary<string, string>
    {
        ["s"] = "FromSeconds",
        ["min"] = "FromMinutes",
        ["d"] = "FromDays",
        ["h"] = "FromHours",
        ["ms"] = "FromMilliseconds",
        ["qs"] = "FromMicroseconds"
    }.ToImmutableDictionary();

    public AstNode Parse(Parser parser, Token token)
    {
        var numberParselet = new NumberParselet();
        List<PostfixOperatorNode> subLiterals = new();

        var firstToken = token;
        while (token.Type == PredefinedSymbols.Number)
            if (IsTimeSubLiteral(parser.LookAhead(0)))
            {
                var number = numberParselet.Parse(parser, token);
                subLiterals.Add(new(number, parser.Consume().Type));
                token = parser.Consume();
            }
            else
            {
                break;
            }

        if (subLiterals.Count == 0) return numberParselet.Parse(parser, firstToken);

        return new TimeLiteral(subLiterals)
            .WithRange(token.Document, subLiterals[0].Expr.Range.Start, token.GetRange().End);
    }

    private bool IsTimeSubLiteral(Token token)
    {
        return TimePostfixConverters.ContainsKey(token.Type.Name);
    }
}