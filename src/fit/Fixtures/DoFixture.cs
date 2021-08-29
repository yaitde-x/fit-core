// Copyright � 2011 Syterra Software Inc. Includes work � 2003-2006 Rick Mugridge, University of Auckland, New Zealand.
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License version 2.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.

using fit;

namespace fitlibrary {

    public class DoFixture: DoFixtureBase	{

        public DoFixture() {}
        public DoFixture(object theSystemUnderTest): base(theSystemUnderTest) {}

        public override void DoTable(Parse table) {
            ProcessFlowRows(table);
        }

    }
}
