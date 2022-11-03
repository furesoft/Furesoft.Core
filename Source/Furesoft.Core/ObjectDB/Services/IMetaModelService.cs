using System.Collections.Generic;
using Furesoft.Core.ObjectDB.Meta;

namespace Furesoft.Core.ObjectDB.Services;

	public interface IMetaModelService
	{
		IEnumerable<ClassInfo> GetAllClasses();
	}