﻿namespace Furesoft.Core.CodeDom.Compiler.Core.Constants;

/// <summary>
/// Helpers for Boolean constants.
/// </summary>
public static class BooleanConstant
{
    /// <summary>
    /// Gets a Boolean constant for 'false.'
    /// </summary>
    /// <returns>The 'false' constant.</returns>
    public static readonly IntegerConstant False =
        new(false);

    /// <summary>
    /// Gets a Boolean constant for 'true.'
    /// </summary>
    /// <returns>The 'true' constant.</returns>
    public static readonly IntegerConstant True =
        new(true);

    /// <summary>
    /// Creates a Boolean constant from a value.
    /// </summary>
    /// <param name="value">The constant value.</param>
    public static IntegerConstant Create(bool value)
    {
        return value ? True : False;
    }
}