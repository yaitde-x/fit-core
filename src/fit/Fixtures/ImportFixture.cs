// Copyright ? 2009 Syterra Software Inc. Includes work by Object Mentor, Inc., ? 2002 Cunningham & Cunningham, Inc.
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License version 2.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.

using System;

namespace fit
{
	[Obsolete("Use suite configuration file")]
    public class ImportFixture : Fixture
	{
        public override bool IsVisible {
            get { return false; }
        }

		public override void DoCell(Parse cell, int columnNumber)
		{
            Processor.AddNamespace(cell.Text);
		}
	}
}

