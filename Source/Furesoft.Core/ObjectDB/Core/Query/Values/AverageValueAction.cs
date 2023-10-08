using Furesoft.Core.ObjectDB.Api;
using Furesoft.Core.ObjectDB.Core.Query.Execution;
using Furesoft.Core.ObjectDB.Meta;

namespace Furesoft.Core.ObjectDB.Core.Query.Values;

/// <summary>
///     An action to compute the average value of a field
/// </summary>
internal sealed class AverageValueAction : AbstractQueryFieldAction
{
    private const int ScaleForAverageDivision = 2;
    private readonly int _scale;
    private decimal _average;
    private int _nbValues;
    private decimal _totalValue;

    public AverageValueAction(string attributeName, string alias) : base(attributeName, alias, false)
    {
        _totalValue = new(0);
        _nbValues = 0;
        AttributeName = attributeName;
        _scale = ScaleForAverageDivision;
    }

    public override void Execute(OID oid, AttributeValuesMap values)
    {
        var n = Convert.ToDecimal(values[AttributeName]);
        _totalValue = decimal.Add(_totalValue, ValuesUtil.Convert(n));
        _nbValues++;
    }

    public override object GetValue()
    {
        return _average;
    }

    public override void End()
    {
        var result = decimal.Divide(_totalValue, _nbValues);
        _average = decimal.Round(result, _scale, MidpointRounding.ToEven);
        //TODO: should we use _roundType here?
        //            _average = Decimal.Round(result, _scale, _roundType);
    }

    public override void Start()
    {
    }

    public override IQueryFieldAction Copy()
    {
        return new AverageValueAction(AttributeName, Alias);
    }
}