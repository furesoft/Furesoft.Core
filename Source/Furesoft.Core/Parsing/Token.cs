﻿namespace Furesoft.Core.Parsing;

public class Token
{
    public static Token Invalid = new(TokenType.Invalid);

    public Token(TokenType type, string text, int start, int end, int line, int column)
    {
        Type = type;
        Text = text;
        Start = start;
        End = end;
        Line = line;
        Column = column;
    }

    public Token(TokenType type)
    {
        Type = type;
        Text = string.Empty;
    }

    public Token(TokenType type, string text)
    {
        Type = type;
        Text = text;
    }

    public int Column { get; }
    public int End { get; set; }
    public int Line { get; set; }
    public int Start { get; set; }
    public string Text { get; set; }

    public TokenType Type { get; set; }
}