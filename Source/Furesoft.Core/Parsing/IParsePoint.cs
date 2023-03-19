namespace Furesoft.Core.Parsing;

public interface IParsePoint<T>
{
    static abstract T Parse(TokenIterator iterator, Parser parser);
}