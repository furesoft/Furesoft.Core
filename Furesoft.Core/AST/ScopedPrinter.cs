using System;

namespace Furesoft.Core.AST
{
	public class ScopedPrinter : IDisposable
	{
		public ScopedPrinter(IPrinter printer)
		{
			Printer.Default = printer;
		}

		public void Dispose()
		{
			Printer.Default = new DefaultPrinter();
		}
	}
}