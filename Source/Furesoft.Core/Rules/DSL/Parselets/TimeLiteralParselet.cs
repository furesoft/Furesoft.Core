using System.Collections.Immutable;
using Furesoft.Core.Rules.DSL.Nodes;
using Silverfly;
using Silverfly.Nodes;
using Silverfly.Nodes.Operators;
using Silverfly.Parselets;
using Silverfly.Parselets.Literals;

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
        List<PostfixOperatorNode> subLiterals = [];

        var firstToken = token;
        while (token.Type == PredefinedSymbols.Number)
        {
            if (IsTimeSubLiteral(parser.LookAhead(0)))
            {
                var number = numberParselet.Parse(parser, token);
                subLiterals.Add(new(number, parser.Consume(), null));
                token = parser.Consume();
            }
            else
            {
                break;
            }
        }

        if (subLiterals.Count == 0) return numberParselet.Parse(parser, firstToken);

        return new TimeLiteral(subLiterals.ToImmutableList())
            .WithRange(token.Document, subLiterals[0].Expr.Range.Start, token.GetRange().End);
    }

    private static bool IsTimeSubLiteral(Token token)
    {
        return TimePostfixConverters.ContainsKey(token.Type.Name);
    }
}