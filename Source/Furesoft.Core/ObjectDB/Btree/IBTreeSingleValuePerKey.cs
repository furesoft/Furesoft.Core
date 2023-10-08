namespace Furesoft.Core.ObjectDB.Btree;

internal interface IBTreeSingleValuePerKey : IBTree
{
    object Search(IComparable key);
}