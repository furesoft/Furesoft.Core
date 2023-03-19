﻿namespace Furesoft.Core.Parsing;

public class SourceDocument
{
    public SourceDocument(string filename)
    {
        Filename = filename;
        Source = File.ReadAllText(filename);
    }

    public SourceDocument(string filename, string source)
    {
        Filename = filename;
        Source = source;
    }

    public string Filename { get; set; }
    public string Source { get; set; }
}
