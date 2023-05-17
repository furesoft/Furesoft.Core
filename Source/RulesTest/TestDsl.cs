using Furesoft.PrattParser;
using Xunit;

namespace RulesTest;

public class TestDsl
{
    [Fact]
    public void Not_Should_Pass()
    {
        var lexer = new Lexer("not 5", "test.dsl");
        lexer.Ignore('\r');
        lexer.Ignore(' ');
        lexer.Ignore('\t');
        lexer.AddSymbol("not");

        var parser = new Furesoft.Core.Rules.DSL.Grammar(lexer);

        var node = parser.Parse();
    }
}