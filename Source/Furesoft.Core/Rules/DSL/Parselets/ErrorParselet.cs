using Furesoft.Core.Rules.DSL.Nodes;
using Furesoft.PrattParser;
using Furesoft.PrattParser.Nodes;
using Furesoft.PrattParser.Parselets;

namespace Furesoft.Core.Rules.DSL.Parselets;

public class ErrorParselet : IPrefixParselet
{
    public AstNode Parse(Parser parser, Token token)
    {
        var messageToken = parser.Consume(PredefinedSymbols.String);

        return new ErrorNode(messageToken.ToString()).WithRange(token, messageToken);
    }
}