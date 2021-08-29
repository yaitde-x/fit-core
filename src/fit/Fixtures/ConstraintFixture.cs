// Copyright � 2011 Syterra Software Inc.
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License version 2.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.

using fit;
using fitlibrary.exception;
using fitSharp.Fit.Engine;
using fitSharp.Fit.Exception;
using fitSharp.Fit.Model;
using fitSharp.Machine.Exception;
using fitSharp.Machine.Model;

namespace fitlibrary {

	public class ConstraintFixture: DoFixtureBase {

        private readonly bool expectedCondition;
	    private Tree<Cell> memberNameCells;
	    private int rowWidth;
        private ValueArray valueCells;

        public ConstraintFixture(): this(true, null) {}
        public ConstraintFixture(bool expectedCondition): this(expectedCondition, null) {}
        public ConstraintFixture(object systemUnderTest): this(true, systemUnderTest) {}
        public ConstraintFixture(bool expectedCondition, object systemUnderTest): base(systemUnderTest) {
            this.expectedCondition = expectedCondition;
        }

	    public string RepeatString { get; set; }

	    public override void DoRows(Parse rows) {
            memberNameCells = rows;
	        rowWidth = rows.Parts.Size;
            valueCells = new ValueArray(RepeatString);
            base.DoRows(rows.More);
        }

	    public override void DoRow(Parse row) {
	        if (row.Parts.Size != rowWidth) {
	            TestStatus.MarkException(row.Parts, new RowWidthException(rowWidth));
	        }
	        else {
	            try {
	                TypedValue result = Processor.ExecuteWithThrow(this, memberNameCells,
	                    valueCells.GetCells(row.Branches), row.Parts);
	                if (result.Type != typeof (bool)) {
	                    throw new InvalidMethodException("Method does not return boolean.");
	                }
	                if (result.GetValue<bool>() == expectedCondition) {
	                    TestStatus.MarkRight(row);
	                }
	                else {
	                    TestStatus.MarkWrong(row);
	                }
	            }
	            catch (ParseException<Cell> e) {
	                TestStatus.MarkException(e.Subject, e.InnerException);
	            }
	        }
	    }
	}
}
