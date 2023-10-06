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
        this.AddCommonAssignmentOperators();
        
        AddOperators();
        
        Register("(", new CallParselet());
        
        Register("is", new ConditionParselet());
        Register("if", new IfParselet());
        
        Block(PredefinedSymbols.Dot, PredefinedSymbols.EOF);
    }

    private void AddOperators()
    {
        Prefix("not", BindingPower.Prefix);
        Prefix("and", BindingPower.Product);
        Prefix("or", BindingPower.Sum);
        InfixLeft("==", BindingPower.Sum);
        
        Register("set", new AssignmentParselet());
        
        Postfix("%", BindingPower.PostFix);


        Postfix("d", BindingPower.PostFix);
        Postfix("h", BindingPower.PostFix);
        Postfix("m", BindingPower.PostFix);
        Postfix("s", BindingPower.PostFix);
        Postfix("ms", BindingPower.PostFix);
        Postfix("qs", BindingPower.PostFix);
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
        
        lexer.AddSymbol("set");
    }
}