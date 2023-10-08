using Furesoft.Core.ObjectDB.Meta;

namespace Furesoft.Core.ObjectDB.Services;

internal interface IMetaModelCompabilityChecker
{
	/// <summary>
	///     Receive the current class info (loaded from current classes present on runtime and check against the persisted meta
	///     model
	/// </summary>
	bool Check(IDictionary<Type, ClassInfo> currentCIs, IMetaModelService metaModelService);
}