using Furesoft.Core.Rules;
using Furesoft.PrattParser;
using RulesTest.Models;
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

    [Fact]
    public void SimpleRule_Should_Pass()
    {
        var engine = RuleEngine<Product>.GetInstance();
        
        engine.AddRule("1 + 1");

        var result = engine.Execute();
    }
}