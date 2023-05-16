using Furesoft.PrattParser;
using Furesoft.PrattParser.Nodes;

namespace Furesoft.Core.Rules.DSL;

public class Grammar : Parser<AstNode>
{
    public Grammar(Lexer tokens) : base(tokens)
    {
        Group("(", ")");
        
        Prefix("-", (int)BindingPower.Prefix);
    }
}