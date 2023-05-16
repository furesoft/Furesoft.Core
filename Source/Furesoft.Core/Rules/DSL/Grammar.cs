using Furesoft.PrattParser;
using Furesoft.PrattParser.Expressions;

namespace Furesoft.Core.Rules.DSL;

public class Grammar : Parser<IAstNode>
{
    public Grammar(ILexer lexer) : base(lexer)
    {
        Prefix("!", (int)BindingPower.Prefix);
        Prefix("not", (int)BindingPower.Prefix);
    }
}