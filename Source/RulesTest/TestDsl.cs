using Furesoft.PrattParser;
using Xunit;

namespace RulesTest;

public class TestDsl
{
    [Fact]
    public void Not_Should_Pass()
    {
        var lexer = new Lexer("!5");
        lexer.AddKeyword("not");
        //lexer.AddSymbol("!");
        
        var parser = new Furesoft.Core.Rules.DSL.Grammar(lexer);

        var node = parser.Parse();
    }
}