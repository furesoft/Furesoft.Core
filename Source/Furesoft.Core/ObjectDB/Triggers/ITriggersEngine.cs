using Furesoft.Core.ObjectDB.Api.Triggers;

namespace Furesoft.Core.ObjectDB.Triggers;

	public interface ITriggersEngine
	{
		void AddUpdateTriggerFor(Type type, UpdateTrigger trigger);

		void AddInsertTriggerFor(Type type, InsertTrigger trigger);

		void AddDeleteTriggerFor(Type type, DeleteTrigger trigger);

		void AddSelectTriggerFor(Type type, SelectTrigger trigger);
	}