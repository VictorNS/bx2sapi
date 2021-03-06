﻿using System.Xml.Serialization;

namespace Excel
{
	/// <summary>
	/// (c) 2014 Vienna, Dietmar Schoder
	/// https://www.codeproject.com/Tips/801032/Csharp-How-To-Read-xlsx-Excel-File-With-Lines-of
	/// Code Project Open License (CPOL) 1.02
	/// Deals with an Excel row
	/// </summary>
	public class Row
	{
		[XmlElement("c")]
		public Cell[] FilledCells;
		[XmlIgnore]
		public Cell[] Cells;

		public void ExpandCells(int NumberOfColumns)
		{
			Cells = new Cell[NumberOfColumns];
			foreach (var cell in FilledCells)
				Cells[cell.ColumnIndex] = cell;
			FilledCells = null;
		}

		public string GetText(int index)
		{
			return this.Cells[index] == null ? "" : this.Cells[index].Text;
		}
	}
}
