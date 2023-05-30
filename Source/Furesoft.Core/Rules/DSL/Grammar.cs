using Furesoft.Core.Rules.DSL.Parselets;
using Furesoft.PrattParser;
using Furesoft.PrattParser.Nodes;
using Furesoft.PrattParser.Parselets;

namespace Furesoft.Core.Rules.DSL;

public class Grammar : Parser<AstNode>
{
    public Grammar()
    {
        Register("error", new ErrorParselet());
        Register(PredefinedSymbols.Name, new NameParselet());
        
        this.AddCommonLiterals();
        this.AddArithmeticOperators();
        this.AddLogicalOperators();
        
        AddOperators();

        Register("=", new AssignParselet());
        Register("(", new CallParselet());
        
        Register("is", new ConditionParselet());
        Register("if", new IfParselet());
    }

    private void AddOperators()
    {
        Prefix("not", BindingPower.Prefix);
        Prefix("and", BindingPower.Product);
        Prefix("or", BindingPower.Sum);
        InfixLeft("==", BindingPower.Sum);
    }

    protected override void InitLexer(Lexer lexer)
    {
        lexer.Ignore(' ');
        lexer.Ignore('\t');
        lexer.MatchString("'", "'");
        lexer.MatchNumber(true, true);
        lexer.MatchBoolean();
        
        lexer.AddSymbol("equal");
        lexer.AddSymbol("less");
        lexer.AddSymbol("greater");
        lexer.AddSymbol("than");
        lexer.AddSymbol("then");
        lexer.AddSymbol("to");
    }
}