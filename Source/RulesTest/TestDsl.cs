using System.Runtime.CompilerServices;
using Argon;
using Furesoft.Core.Rules;
using Furesoft.Core.Rules.DSL;
using Furesoft.PrattParser;
using Furesoft.PrattParser.Text;
using RulesTest.Models;

namespace RulesTest;

[UsesVerify]
public class TestDsl
{
    class SymbolConverter : WriteOnlyJsonConverter<Symbol>
    {
        public override void Write(VerifyJsonWriter writer, Symbol value)
        {
            writer.WriteRawValue($"{value.Name}");
        }
    }
    
    class RangeConverter : WriteOnlyJsonConverter<SourceRange>
    {
        public override void Write(VerifyJsonWriter writer, SourceRange value)
        {
            writer.WriteRawValue($"{value.Start}-{value.End}");
        }
    }
    
    class DocumentConverter : WriteOnlyJsonConverter<SourceDocument>
    {
        public override void Write(VerifyJsonWriter writer, SourceDocument value)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Filename");
            writer.WriteRawValue(value.Filename);
            
            writer.WritePropertyName("Source");
            writer.WriteRawValue(value.Source.ToString());
            
            writer.WriteEndObject();
        }
    }
    
    [ModuleInitializer]
    public static void Init()
    {
        VerifierSettings.AddExtraSettings(_ =>
        {
            _.Converters.Add(new SymbolConverter());
            _.Converters.Add(new DocumentConverter());
            _.Converters.Add(new RangeConverter());
            _.TypeNameHandling = TypeNameHandling.All;
        });
    }

    [Fact]
    public Task Not_Should_Pass()
    {
        var node = Grammar.Parse<Grammar>("not 5", "test.dsl");
        
        return Verify(node);
    }
    
    [Fact]
    public Task Percent_Should_Pass()
    {
        var node = Grammar.Parse<Grammar>("5%", "test.dsl");
        
        return Verify(node);
    }
    
    [Fact]
    public Task Comparison_Should_Pass()
    {
        var node = Grammar.Parse<Grammar>("5 is equal to 5", "test.dsl");

        return Verify(node);
    }

    [Fact]
    public Task If_Should_Pass() {
        var node = Grammar.Parse<Grammar>("if 5 is equal to 5 then error 'something went wrong'", "test.dsl");

        return Verify(node);
    }
    
    [Fact]
    public Task Set_Should_Pass() {
        var node = Grammar.Parse<Grammar>("set x to 42.", "test.dsl");

        return Verify(node);
    }

    [Fact]
    public Task SimpleRule_Should_Pass()
    {
        var engine = RuleEngine<Product>.GetInstance(new Product() { Description = "hello world", Price = 999});
        
        engine.AddRule("if Description == 'hello world' then error 'wrong key'");

        var result = engine.Execute();
        
        return Verify(result);
    }
}