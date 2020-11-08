namespace Furesoft.Core.ObjectDB.Services
{
	public interface ICommitListener
	{
		void BeforeCommit();

		void AfterCommit();
	}
}