namespace Furesoft.Core.ObjectDB.Meta.Compare;

internal interface IObjectInfoComparator
{
    bool HasChanged(AbstractObjectInfo aoi1, AbstractObjectInfo aoi2);

    void Clear();

    int GetNbChanges();
}