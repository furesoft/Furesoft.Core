using Furesoft.Core.ObjectDB.Meta;

namespace Furesoft.Core.ObjectDB.Core.Query;

internal interface IQueryExecutionPlan
{
    bool UseIndex();

    ClassInfoIndex GetIndex();

    string GetDetails();

    void Start();

    void End();
}