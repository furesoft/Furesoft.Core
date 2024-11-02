using Furesoft.Core.Rules.DSL.Parselets;
using Silverfly;
using Silverfly.Parselets;
using Silverfly.Parselets.Literals;
using static Silverfly.PredefinedSymbols;

namespace Furesoft.Core.Rules.DSL;

public class Grammar : Parser
{
    private void AddOperators(ParserDefinition def)
    {
        def.Prefix("not");
        def.Prefix("and");
        def.Prefix("or");

        def.InfixLeft("==", "Sum");

        def.Register("set", new AssignmentParselet(def.PrecedenceLevels.GetPrecedence("Assignment")));

        def.Postfix("%");
    }

    protected override void InitLexer(LexerConfig lexer)
    {
        lexer.Ignore(' ');
        lexer.Ignore('\t');
        lexer.MatchString("'", "'");
        lexer.MatchNumber(allowHex: true, allowBin: true);
        lexer.MatchBoolean();

        lexer.AddSymbols("equal", "less", "greater", "then", "than", "to");
        lexer.AddSymbols("divisible", "by");
        lexer.AddSymbol("set");

        lexer.AddSymbols("d", "h", "min", "s", "ms", "qs");
        lexer.AddSymbol(".");
    }

    protected override void InitParser(ParserDefinition def)
    {
        def.Block(SOF, EOF, ".");

        def.Register("error", new ErrorParselet());
        def.Register(Name, new NameParselet());

        def.Register(Number, new TimeLiteralParselet());
        def.Register(PredefinedSymbols.Boolean, new BooleanLiteralParselet());
        def.Register(PredefinedSymbols.String, new StringLiteralParselet());

        def.AddArithmeticOperators();
        def.AddLogicalOperators();
        def.AddCommonAssignmentOperators();

        AddOperators(def);

        def.Register("(", new CallParselet(def.PrecedenceLevels.GetPrecedence("Call")));

        def.Register("is", new ConditionParselet(def.PrecedenceLevels.GetPrecedence("Product")));
        def.Register("if", new IfParselet(def.PrecedenceLevels.GetPrecedence("Product") - 1));
    }
}