// Copyright � 2012 Syterra Software Inc.
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License version 2.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.

using fit;
using System;
using fitSharp.Fit.Engine;
using fitSharp.Fit.Model;
using fitSharp.Fit.Operators;
using fitSharp.Machine.Engine;
using fitSharp.Machine.Model;

namespace fitlibrary {

	public abstract class FlowFixtureBase: Fixture, FlowInterpreter {

	    public abstract MethodRowSelector MethodRowSelector { get; }

	    protected FlowFixtureBase() {}

	    protected FlowFixtureBase(object theSystemUnderTest): this() {
            mySystemUnderTest = theSystemUnderTest;
        }

        public virtual bool IsInFlow(int tableCount) { return tableCount == 1; }
        
	    public void DoSetUp(CellProcessor processor, Tree<Cell> table) {
	        Prepare(processor, table.Branches[0]);
            ExecuteOptionalMethod(MemberName.SetUp, (Parse)table.Branches[0].Branches[0]);
	    }

	    public void DoTearDown(Tree<Cell> table) {
            ExecuteOptionalMethod(MemberName.TearDown, (Parse)table.Branches[0].Branches[0]);
	    }

	    protected void ProcessFlowRows(Parse table) {
            new InterpretFlow(Processor, this).DoTableFlow(table, 1);
        }

        void ExecuteOptionalMethod(MemberName theMethodName, Parse theCell) {
            try {
                Processor.Invoke(this, theMethodName, theCell); //todo: invokewiththrow?
            }
            catch (Exception e) { 
                TestStatus.MarkException(theCell, e);
            }
        }

        public object NamedFixture(string theName) {
            return Symbols.HasValue(theName) ? Symbols.GetValue(theName) : null;
        }

        public void AddNamedFixture(string name, object fixture) { Processor.Get<Symbols>().Save(name, fixture); }

        public override object GetTargetObject() {
            return this;
        }
	}
}
