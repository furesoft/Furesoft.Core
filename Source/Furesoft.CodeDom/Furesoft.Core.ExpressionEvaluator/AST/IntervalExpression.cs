namespace Furesoft.Core.ExpressionEvaluator.AST;

public class IntervalExpression : Expression
{
    public IntervalExpression(Parser parser, CodeObject parent) : base(parser, parent)
    {
    }

    public bool IsMaximumInclusive { get; set; }

    public bool IsMinimumInclusive { get; set; }

    public Token Left { get; set; }

    public Expression Maximum { get; set; }

    public Expression Minimum { get; set; }

    public Token Right { get; set; }

    public new static void AddParsePoints()
    {
        Parser.AddParsePoint("[", Parse, typeof(FunctionArgumentConditionDefinition));
        Parser.AddParsePoint("]", Parse, typeof(FunctionArgumentConditionDefinition));
    }

    public static CodeObject Parse(Parser parser, CodeObject parent, ParseFlags flags)
    {
        var result = new IntervalExpression(parser, parent);

        result.Left = parser.Token;

        parser.NextToken();

        result.Minimum = Expression.Parse(parser, result);
        result.IsMinimumInclusive = result.Left.Text == "[";

        if (!result.ParseExpectedToken(parser, ","))
        {
            return null;
        }

        result.Maximum = Expression.Parse(parser, result);

        result.Right = parser.Token; //push after
        result.IsMaximumInclusive = result.Right.Text == "]";

        parser.NextToken();

        return result;
    }

    public override T Accept<T>(VisitorBase<T> visitor)
    {
        return visitor.Visit(this);
    }

    public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
    {
    }

    private static Expression BindMaximum(IntervalExpression interval, Expression variable)
    {
        if (interval.IsMaximumInclusive)
        {
            return new LessThanEqual(variable, interval.Maximum);
        }
        else
        {
            return new LessThan(variable, interval.Maximum);
        }
    }

    private static Expression BindMinimum(IntervalExpression interval, Expression variable)
    {
        if (interval.IsMinimumInclusive)
        {
            if (interval.Minimum is Negative neg && neg.Expression is InfinityRef)
            {
                interval.Minimum = BindNegInfinity((FunctionArgumentConditionDefinition)interval.Parent);
            }
            if (interval.Maximum is InfinityRef)
            {
                interval.Maximum = BindPosInfinity((FunctionArgumentConditionDefinition)interval.Parent);
            }

            return new GreaterThanEqual(variable, interval.Minimum);
        }
        else
        {
            return new GreaterThan(variable, interval.Minimum);
        }
    }

    private static Expression BindNegInfinity(FunctionArgumentConditionDefinition facd)
    {
        return facd.NumberRoom switch
        {
            "N" => uint.MinValue,
            "Z" => int.MinValue,
            "R" => double.MinValue,
            _ => false,
        };
    }

    private static Expression BindPosInfinity(FunctionArgumentConditionDefinition facd)
    {
        return facd.NumberRoom switch
        {
            "N" => uint.MaxValue,
            "Z" => int.MaxValue,
            "R" => double.MaxValue,
            _ => false,
        };
    }
}