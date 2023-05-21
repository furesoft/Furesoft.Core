using Furesoft.Core.Rules.DSL.Parselets;
using Furesoft.PrattParser;
using Furesoft.PrattParser.Matcher;
using Furesoft.PrattParser.Nodes;
using Furesoft.PrattParser.Parselets;

namespace Furesoft.Core.Rules.DSL;

public class Grammar : Parser<AstNode>
{
    public Grammar()
    {
        Register(PredefinedSymbols.Name, new NameParselet());
        this.AddCommonLiterals();
        
        Register("=", new AssignParselet());

        Ternary("?", ":", (int)BindingPower.Conditional);
        Register("(", new CallParselet());
        Group("(", ")");
        
        Register("error", new ErrorParselet());

        Prefix("+", (int)BindingPower.Prefix);
        Prefix("-", (int)BindingPower.Prefix);
        Prefix("~", (int)BindingPower.Prefix);
        Prefix("!", (int)BindingPower.Prefix);
        
        Register("is", new ConditionParselet());
        //Register("if", new IfParselet());

        Postfix("!", (int)BindingPower.PostFix);

        InfixLeft("+", (int)BindingPower.Sum);
        InfixLeft("-", (int)BindingPower.Sum);
        InfixLeft("*", (int)BindingPower.Product);
        InfixLeft("/", (int)BindingPower.Product);
        InfixRight("^", (int)BindingPower.Exponent);
        
        InfixLeft("->", (int)BindingPower.Product);
    }

    protected override void InitLexer(Lexer lexer)
    {
        lexer.Ignore(' ');
        lexer.Ignore('\t');
        lexer.UseString("'", "'");
        lexer.AddPart(new IntegerMatcher());
        
        lexer.AddSymbol("is");
        lexer.AddSymbol("equal");
        lexer.AddSymbol("not");
        lexer.AddSymbol("less");
        lexer.AddSymbol("greater");
        lexer.AddSymbol("than");
        lexer.AddSymbol("to");
    }
}