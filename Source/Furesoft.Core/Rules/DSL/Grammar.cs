using Furesoft.Core.Rules.DSL.Parselets;
using Furesoft.PrattParser;
using Furesoft.PrattParser.Parselets;
using Furesoft.PrattParser.Parselets.Literals;

namespace Furesoft.Core.Rules.DSL;

public class Grammar : Parser
{
    public Grammar()
    {
        Block(PredefinedSymbols.Dot, PredefinedSymbols.EOF, -1);

        Register("error", new ErrorParselet());
        Register(PredefinedSymbols.Name, new NameParselet());

        Register(PredefinedSymbols.Number, new TimeLiteralParselet());
        Register(PredefinedSymbols.Boolean, new BooleanLiteralParselet());
        Register(PredefinedSymbols.String, new StringLiteralParselet());

        this.AddArithmeticOperators();
        this.AddLogicalOperators();
        this.AddCommonAssignmentOperators();

        AddOperators();

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

        Register("set", new AssignmentParselet());

        Postfix("%", BindingPower.PostFix);
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

        lexer.AddSymbol("d");
        lexer.AddSymbol("h");
        lexer.AddSymbol("min");
        lexer.AddSymbol("s");
        lexer.AddSymbol("ms");
        lexer.AddSymbol("qs");
    }
}