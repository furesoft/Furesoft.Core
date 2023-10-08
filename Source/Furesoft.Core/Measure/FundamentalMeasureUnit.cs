namespace Furesoft.Core.Measure;

/// <summary>
///     A fundamental measure unit is semantically bound to a dimension, and can be identified
///     by its <see cref="MeasureUnit.Abbreviation" />.
///     See http://en.wikipedia.org/wiki/SI_base_unit.
/// </summary>
public class FundamentalMeasureUnit : AtomicMeasureUnit
{
    internal FundamentalMeasureUnit(MeasureContext ctx, string abbreviation, string name, AutoStandardPrefix stdPrefix,
        bool isNormalized)
        : base(ctx, abbreviation, name, stdPrefix, isNormalized)
    {
    }
}