// Copyright ? 2011 Syterra Software Inc. All rights reserved.
// The use and distribution terms for this software are covered by the Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file license.txt at the root of this distribution. By using this software in any fashion, you are agreeing
// to be bound by the terms of this license. You must not remove this notice, or any other, from this software.

using fitSharp.Machine.Model;

namespace fitSharp.Fit.Engine {
    public interface ExecuteOperator {
        bool CanExecute(object systemUnderTest, Tree<Cell> memberName, Tree<Cell> parameters, Cell targetCell);
        TypedValue Execute(object systemUnderTest, Tree<Cell> memberName, Tree<Cell> parameters, Cell targetCell);
    }
}
