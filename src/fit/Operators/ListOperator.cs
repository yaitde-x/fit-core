// Copyright � 2011 Syterra Software Inc.
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License version 2.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.

using System;
using System.Collections;
using System.Linq;
using fit.Model;
using fitSharp.Fit.Engine;
using fitSharp.Fit.Operators;
using fitSharp.Machine.Engine;
using fitSharp.Machine.Model;

namespace fit.Operators {
    public class ListOperator: CellOperator, CheckOperator, ParseOperator<Cell> { //todo: split, eliminate fit dependency
        public bool CanCheck(CellOperationValue actualValue, Tree<Cell> expectedCell) {
            return !expectedCell.IsLeaf && typeof (IList).IsAssignableFrom(actualValue.GetTypedActual(Processor).Type);
        }

        public TypedValue Check(CellOperationValue actualValue, Tree<Cell> expectedCell) {
            var cell = (Parse)expectedCell.Value;
            var matcher = new ListMatcher(Processor, new ArrayMatchStrategy(Processor, cell.Parts.Parts));
            matcher.MarkCell(actualValue.GetActual<IEnumerable>(Processor).Cast<object>(), cell.Parts, 0); //todo: encapsulate part in celloperationcontext??
            return TypedValue.Void;
        }

        public bool CanParse(Type type, TypedValue instance, Tree<Cell> parameters) {
            return typeof(IList).IsAssignableFrom(type) && parameters.Branches.Count > 0;
        }

        public TypedValue Parse(Type type, TypedValue instance, Tree<Cell> parameters) {
            var cell = (Parse) parameters;
            //if (cell.Parts == null) throw new FitFailureException("No embedded table.");
            Parse headerCells = cell.Parts.Parts.Parts;
            Parse dataRows = cell.Parts.Parts.More;
            return new TypedValue(
                new CellRange(dataRows).Cells.Aggregate(new ArrayList(), (list, row) => {
                    list.Add(
                        Processor.ExecuteWithThrow(instance.Value, new CellRange(headerCells),
                                                                new CellRange(row.Parts), row.Parts).Value);
                    return list;
                }));
        }

    }
}
