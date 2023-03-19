﻿namespace Furesoft.Core.Parsing.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class PostUnaryOperatorInfoAttribute : OperatorInfoAttribute
{
    public PostUnaryOperatorInfoAttribute(int precedence) : base(precedence, true, true)
    {
    }
}
