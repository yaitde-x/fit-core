// Copyright ? 2015 Syterra Software Inc. All rights reserved.
// The use and distribution terms for this software are covered by the Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file license.txt at the root of this distribution. By using this software in any fashion, you are agreeing
// to be bound by the terms of this license. You must not remove this notice, or any other, from this software.

using fitSharp.Fit.Engine;
using fitSharp.Fit.Model;
using fitSharp.Machine.Model;

namespace fitSharp.Fit.Fixtures {
    public class Include: Interpreter {
        public void Interpret(CellProcessor processor, Tree<Cell> table) {
            var action = new IncludeAction(processor);
            var currentRow = table.Branches[0].Skip(1);
            currentRow.ExecuteMethod(processor, new SequenceRowSelector(), action).ThrowExceptionIfNotValid();
            table.ValueAt(0, 0).SetAttribute(CellAttribute.Folded, action.Result);
        }
    }
}
