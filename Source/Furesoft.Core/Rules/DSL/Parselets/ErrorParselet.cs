using Furesoft.Core.Rules.DSL.Nodes;
using Furesoft.PrattParser;
using Furesoft.PrattParser.Nodes;
using Furesoft.PrattParser.Parselets;

namespace Furesoft.Core.Rules.DSL.Parselets;

public class ErrorParselet : IPrefixParselet<AstNode>
{
    public AstNode Parse(Parser<AstNode> parser, Token token)
    {
        var messageToken = parser.Consume(PredefinedSymbols.String);

        return new ErrorNode(messageToken.Text.Span.ToString()).WithRange(token, messageToken);
    }
}