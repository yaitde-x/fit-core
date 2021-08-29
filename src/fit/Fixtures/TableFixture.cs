// Copyright � 2009 Syterra Software Inc. Includes work by Object Mentor, Inc., � 2002 Cunningham & Cunningham, Inc.
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License version 2.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.

using fit;
using fitSharp.Machine.Model;

namespace fitnesse.fixtures
{
	public abstract class TableFixture : Fixture
	{
		private Parse rows;
		
		protected abstract void DoStaticTable(int rows);

		public override void DoRows(Parse rows)
		{
			this.rows = rows;
			DoStaticTable(rows.Size);
		}

		protected Parse GetCell(int row, int column)
		{
			return rows.At(row, column);
		}

		protected string GetString(int row, int column)
		{
			Parse cell = GetCell(row, column);
			if (cell == null)
			{
				return null;
			}
			return cell.Text;
		}

		protected void Right(int row, int column)
		{
			Parse cell = rows.At(row, column);
			TestStatus.MarkRight(cell);
		}

		protected void Wrong(int row, int column)
		{
			Parse cell = rows.At(row, column);
			TestStatus.MarkWrong(cell);
		}

		protected void Wrong(int row, int column, string actual)
		{
			Parse cell = rows.At(row, column);
			TestStatus.MarkWrong(cell, actual);
		}

		protected void Ignore(int row, int column)
		{
			Parse cell = rows.At(row, column);
			TestStatus.MarkIgnore(cell);
		}

		protected int GetInt(int row, int column)
		{
            return Processor.Parse(typeof(int), new TypedValue(this), GetCell(row, column)).GetValue<int>();
		}

		protected bool Blank(int row, int column)
		{
			Parse cell = rows.At(row, column);
			return "".Equals(cell.Text.Trim());
		}
	}
}
