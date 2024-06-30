using Furesoft.Core.Rules.DSL.Parselets;
using Silverfly;
using Silverfly.Parselets;
using Silverfly.Parselets.Literals;
using static Silverfly.PredefinedSymbols;

namespace Furesoft.Core.Rules.DSL;

public class Grammar : Parser
{
    private void AddOperators()
    {
        Prefix("not");
        Prefix("and");
        Prefix("or");

        InfixLeft("==", "Sum");

        Register("set", new AssignmentParselet(BindingPowers.Get("Assignment")));

        Postfix("%");
    }

    protected override void InitLexer(Lexer lexer)
    {
        lexer.Ignore(' ');
        lexer.Ignore('\t');
        lexer.MatchString("'", "'");
        lexer.MatchNumber(allowHex: true, allowBin: true);
        lexer.MatchBoolean(ignoreCasing: true);

        lexer.AddSymbols("equal", "less", "greater", "then", "than", "to");
        lexer.AddSymbols("divisible", "by");
        lexer.AddSymbol("set");

        lexer.AddSymbols("d", "h", "min", "s", "ms", "qs");
        lexer.AddSymbol(".");
    }

    protected override void InitParselets()
    {
        Block(SOF, EOF, Dot);

        Register("error", new ErrorParselet());
        Register(Name, new NameParselet());

        Register(Number, new TimeLiteralParselet());
        Register(PredefinedSymbols.Boolean, new BooleanLiteralParselet());
        Register(PredefinedSymbols.String, new StringLiteralParselet());

        this.AddArithmeticOperators();
        this.AddLogicalOperators();
        this.AddCommonAssignmentOperators();

        AddOperators();

        Register("(", new CallParselet(BindingPowers.Get("Call")));

        Register("is", new ConditionParselet(BindingPowers.Get("Product")));
        Register("if", new IfParselet(BindingPowers.Get("Product") - 1));
    }
}