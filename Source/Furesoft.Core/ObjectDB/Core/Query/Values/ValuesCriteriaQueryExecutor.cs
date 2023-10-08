using Furesoft.Core.ObjectDB.Api;
using Furesoft.Core.ObjectDB.Api.Query;
using Furesoft.Core.ObjectDB.Core.Query.Criteria;
using Furesoft.Core.ObjectDB.Meta;
using Furesoft.Core.ObjectDB.Tool.Wrappers;

namespace Furesoft.Core.ObjectDB.Core.Query.Values;

internal sealed class ValuesCriteriaQueryExecutor : GenericQueryExecutor
{
    private IOdbList<string> _involvedFields;
    private SodaQuery _sodaQuery;

    private AttributeValuesMap _values;

    public ValuesCriteriaQueryExecutor(IQuery query, IStorageEngine engine) : base(query, engine)
    {
        _sodaQuery = (SodaQuery) query;
    }

    protected override IQueryExecutionPlan GetExecutionPlan()
    {
        IQueryExecutionPlan plan = new CriteriaQueryExecutionPlan(ClassInfo, Query);
        return plan;
    }

    protected override void PrepareQuery()
    {
        _sodaQuery = Query;
        ((IInternalQuery) _sodaQuery).SetQueryEngine(StorageEngine);
        _involvedFields = _sodaQuery.GetAllInvolvedFields();
    }

    protected override bool MatchObjectWithOid(OID oid, bool returnObject, bool inMemory)
    {
        CurrentOid = oid;

        // Gets a map with the values with the fields involved in the query
        _values = ObjectReader.ReadObjectInfoValuesFromOID(ClassInfo, CurrentOid, true, _involvedFields,
            _involvedFields, 0);

        var objectMatches = true;
        if (!_sodaQuery.IsForSingleOid())
            // Then apply the query on the field values
            objectMatches = _sodaQuery.Match(_values);

        var objectInfoHeader = _values.GetObjectInfoHeader();
        // Stores the next position
        NextOID = objectInfoHeader.GetNextObjectOID();
        return objectMatches;
    }

    protected override object GetCurrentObjectMetaRepresentation()
    {
        return _values;
    }
}