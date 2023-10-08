namespace Furesoft.Core.Measure;

/// <summary>
///     An <see cref="AliasMeasureUnit" /> is a <see cref="AtomicMeasureUnit" /> that has its own unique abbreviation and
///     name,
///     is defined by a <see cref="FullFactor" /> applied to a <see cref="MeasureUnit" />.
/// </summary>
public sealed class AliasMeasureUnit : AtomicMeasureUnit
{
    internal AliasMeasureUnit(
        MeasureContext ctx,
        string abbreviation, string name,
        FullFactor definitionFactor,
        MeasureUnit definition,
        AutoStandardPrefix stdPrefix)
        : base(ctx, abbreviation, name, stdPrefix, false)
    {
        DefinitionFactor = definitionFactor;
        Definition = definition;
    }

    /// <summary>
    ///     Gets the definition factor.
    /// </summary>
    public FullFactor DefinitionFactor { get; }

    /// <summary>
    ///     Gets the unit definition.
    /// </summary>
    public MeasureUnit Definition { get; }

    private protected override (MeasureUnit, FullFactor) GetNormalization()
    {
        return (
            Definition.Normalization,
            Definition.NormalizationFactor.Multiply(DefinitionFactor)
        );
    }
}