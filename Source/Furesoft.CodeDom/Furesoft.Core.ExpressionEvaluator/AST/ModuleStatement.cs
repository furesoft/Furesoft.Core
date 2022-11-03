namespace Furesoft.Core.ExpressionEvaluator.AST;

public class ModuleStatement : Statement, IBindable
{
    public ModuleStatement(Parser parser, CodeObject parent) : base(parser, parent)
    {
    }

    public Expression ModuleName { get; set; }

    public static void AddParsePoints()
    {
        Parser.AddParsePoint("module", Parse);
    }

    public CodeObject Bind(ExpressionParser ep, Binder binder)
    {
        if (ModuleName is Dot dot)
        {
            ModuleName = new UnresolvedRef(dot._AsString);
        }

        return this;
    }

    private static CodeObject Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        var node = new ModuleStatement(parser, parent);
        parser.NextToken();

        node.ModuleName = Expression.Parse(parser, node, false, ";");

        return node;
    }
}