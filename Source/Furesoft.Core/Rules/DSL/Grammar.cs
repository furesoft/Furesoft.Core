using Furesoft.PrattParser;
using Furesoft.PrattParser.Expressions;
using Furesoft.PrattParser.Parselets;

namespace Furesoft.Core.Rules.DSL;

public class Grammar : Parser<IAstNode>
{
    public Grammar(ILexer lexer) : base(lexer)
    {
        Register(PredefinedSymbols.Integer, new IntegerLiteralParselet());
        
        Group("(", ")");

        Prefix("+", (int)BindingPower.Prefix);
        Prefix("-", (int)BindingPower.Prefix);
        Prefix("~", (int)BindingPower.Prefix);
        Prefix("!", (int)BindingPower.Prefix);

        Postfix("!", (int)BindingPower.PostFix);

        InfixLeft("+", (int)BindingPower.Sum);
        InfixLeft("-", (int)BindingPower.Sum);
        InfixLeft("*", (int)BindingPower.Product);
        InfixLeft("/", (int)BindingPower.Product);
        InfixRight("^", (int)BindingPower.Exponent);
    }
}