namespace Furesoft.Core.CLI;

	using System;
	using System.Collections;

	public class ConsoleTable
	{
		#region Align enum

		public enum Align
		{
			Left,
			Right
		};

		#endregion Align enum

		private readonly Align CellAlignment = Align.Left;
		private readonly string[] headers;
		private readonly int tableYStart;

		/// <summary>
		/// The last line of the table (gotton from Console.CursorTop). -1 = No printed data
		/// </summary>
		public int LastPrintEnd = -1;

		/// <summary>
		/// Helps create a table
		/// </summary>
		/// <param name="tableStart">What line to start the table on.</param>
		/// <param name="alignment">The alignment of each cell\'s text.</param>
		public ConsoleTable(int tableStart, Align alignment, string[] headersi)
		{
			headers = headersi;
			CellAlignment = alignment;
			tableYStart = tableStart;
		}

		public void ClearData()
		{
			//Clear Previous data
			if (LastPrintEnd != -1) //A set of data has already been printed
			{
				for (var i = tableYStart; i < LastPrintEnd; i++)
				{
					ClearLine(i);
				}
			}
			LastPrintEnd = -1;
		}

		public void RePrint(ArrayList data)
		{
			//Set buffers
			if (data.Count > Console.BufferHeight)
				Console.BufferHeight = data.Count;
			//Clear Previous data
			ClearData();

			Console.CursorTop = tableYStart;
			Console.CursorLeft = 0;
			if (data.Count == 0)
			{
				Console.WriteLine("No Records");
				LastPrintEnd = Console.CursorTop;
				return;
			}

			//Get max lengths on each column
			var comWidth = ((string[])data[0]).Length * 2 + 1;
			var columnLengths = new int[((string[])data[0]).Length];

			foreach (string[] row in data)
			{
				for (var i = 0; i < row.Length; i++)
				{
					if (row[i].Length > columnLengths[i])
					{
						comWidth -= columnLengths[i];
						columnLengths[i] = row[i].Length;
						comWidth += columnLengths[i];
					}
				}
			}
			//Don't forget to check headers
			for (var i = 0; i < headers.Length; i++)
			{
				if (headers[i].Length > columnLengths[i])
				{
					comWidth -= columnLengths[i];
					columnLengths[i] = headers[i].Length;
					comWidth += columnLengths[i];
				}
			}

			if (Console.BufferWidth < comWidth)
				Console.BufferWidth = comWidth + 1;
			PrintLine(comWidth);
			//Print Data
			var first = true;
			foreach (string[] row in data)
			{
				if (first)
				{
					//Print Header
					PrintRow(headers, columnLengths);
					PrintLine(comWidth);
					first = false;
				}
				PrintRow(row, columnLengths);
			}
			PrintLine(comWidth);
			LastPrintEnd = Console.CursorTop;
		}

		private void ClearLine(int line)
		{
			var oldtop = Console.CursorTop;
			Console.CursorTop = line;
			var oldleft = Console.CursorLeft;
			Console.CursorLeft = 0;
			var top = Console.CursorTop;

			while (Console.CursorTop == top)
			{
				Console.Write(" ");
			}
			Console.CursorLeft = oldleft;
			Console.CursorTop = oldtop;
		}

		private void PrintLine(int width)
		{
			Console.WriteLine(new string('-', width));
		}

		private void PrintRow(string[] row, int[] widths)
		{
			var s = "|";
			for (var i = 0; i < row.Length; i++)
			{
				if (CellAlignment == Align.Left)
					s += row[i] + new string(' ', widths[i] - row[i].Length + 1) + "|";
				else if (CellAlignment == Align.Right)
					s += new string(' ', widths[i] - row[i].Length + 1) + row[i] + "|";
			}
			if (s == "|")
				throw new Exception("PrintRow input must not be empty");

			Console.WriteLine(s);
		}
	}