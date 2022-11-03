namespace Furesoft.Core.ExpressionEvaluator.AST;

public class MatrixExpression : Expression, IEvaluatableExpression
{
    public MatrixExpression(Parser parser, CodeObject parent) : base(parser, parent)
    {
    }

    public ChildList<Expression> Storage { get; set; }

    public new static void AddParsePoints()
    {
        Parser.AddParsePoint("[", 1, Parse);
    }

    public static CodeObject Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        parser.NextToken();

        var result = new MatrixExpression(parser, parent);

        result.Storage = Expression.ParseList(parser, parent, "]");

        if (!result.ParseExpectedToken(parser, "]"))
            return null;

        return result;
    }

    public override T Accept<T>(VisitorBase<T> visitor)
    {
        return visitor.Visit(this);
    }

    public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
    {
        writer.Write("[");

        foreach (var s in Storage)
        {
            s.AsTextExpression(writer, flags);
            writer.Write(" ");
        }

        writer.Write("]");
    }

    public ValueType Evaluate(ExpressionParser ep, Scope scope)
    {
        return Matrix<double>.Build.Dense(1, Storage.Count, Storage.Select(_ => ep.EvaluateExpression(_, scope).Get<double>()).ToArray());
    }
}