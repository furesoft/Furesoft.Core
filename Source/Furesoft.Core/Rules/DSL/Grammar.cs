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
        this.AddArithmeticOperators();
        this.AddLogicalOperators();
        
        Register("=", new AssignParselet());

        Ternary("?", ":", (int)BindingPower.Conditional);
        Register("(", new CallParselet());

        Register("error", new ErrorParselet());

        Register("is", new ConditionParselet());
        Register("if", new IfParselet());
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
        lexer.AddSymbol("then");
        lexer.AddSymbol("to");

        lexer.AddSymbol("if");
    }
}