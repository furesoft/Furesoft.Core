namespace Furesoft.Core.Parsing;

public class ParsePoints<T> : Dictionary<TokenType, Func<TokenIterator, Parser, T>>
{ }
