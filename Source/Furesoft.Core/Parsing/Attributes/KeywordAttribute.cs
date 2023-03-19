﻿namespace Furesoft.Core.Parsing.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class KeywordAttribute : Attribute
{
    public KeywordAttribute(string keyword)
    {
        Keyword = keyword;
    }

    public string Keyword { get; set; }
}
