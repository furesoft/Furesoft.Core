using System;
using System.Diagnostics;

namespace Furesoft.Core.Measure;

/// <summary>
/// A <see cref="ExponentMeasureUnit"/> is an exponentiation of a <see cref="AtomicMeasureUnit"/>
/// (like "m3", "s-1", "Pa3" or "dm2").
/// </summary>
public class ExponentMeasureUnit : MeasureUnit, IComparable<ExponentMeasureUnit>
{
    internal ExponentMeasureUnit( MeasureContext ctx, (string A, string N) names, int exp, AtomicMeasureUnit u )
        : base( ctx, names.A, names.N, u.IsNormalized )
    {
        Debug.Assert( exp != 0 );
        Exponent = exp;
        AtomicMeasureUnit = u;
    }

    private protected ExponentMeasureUnit( MeasureContext ctx, string abbreviation, string name, bool isNormalized )
        : base( ctx, abbreviation, name, isNormalized )
    {
        Exponent = 1;
        AtomicMeasureUnit = (AtomicMeasureUnit)this;
    }

    /// <summary>
    /// Gets the exponent that applies to the <see cref="AtomicMeasureUnit"/>.
    /// When this is itself a <see cref="AtomicMeasureUnit"/> it is 1 (even for
    /// the special, unique, <see cref="MeasureUnit.None"/>).
    /// It can never be 0.
    /// </summary>
    public int Exponent { get; }

    /// <summary>
    /// Gets the atomic measure that is exponentiated.
    /// When this is itself a <see cref="AtomicMeasureUnit"/> it is this (and <see cref="Exponent"/> is 1).
    /// </summary>
    public AtomicMeasureUnit AtomicMeasureUnit { get; }

    internal static (string A, string N) ComputeNames( int exp, AtomicMeasureUnit u )
    {
        if( exp == 1 ) return (u.Abbreviation, u.Name);
        var e = exp.ToString();
        return (u.Abbreviation + e, u.Name + "^" + e);
    }

    /// <summary>
    /// ExponentMeasureUnit are ordered first by decreasing order of their <see cref="Exponent"/>
    /// and then by their <see cref="AtomicMeasureUnit"/> (that use their <see cref="MeasureUnit.Abbreviation"/>).
    /// </summary>
    /// <param name="other">The other exponent unit to compare to. Can be null.</param>
    /// <returns>Standard comparison result (positive, zero or negative).</returns>
    public int CompareTo( ExponentMeasureUnit other )
    {
        if( other == null ) return 1;
        var cmp = Exponent.CompareTo( other.Exponent );
        return cmp == 0 ? AtomicMeasureUnit.CompareTo( other.AtomicMeasureUnit ) : -cmp;
    }

    private protected override (MeasureUnit, FullFactor) GetNormalization()
    {
        return (
                 AtomicMeasureUnit.Normalization.Power( Exponent ),
                 AtomicMeasureUnit.NormalizationFactor.Power( Exponent )
               );
    }

}
