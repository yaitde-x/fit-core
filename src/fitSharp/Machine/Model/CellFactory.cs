﻿// Copyright © 2019 Syterra Software Inc. All rights reserved.
// The use and distribution terms for this software are covered by the Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file license.txt at the root of this distribution. By using this software in any fashion, you are agreeing
// to be bound by the terms of this license. You must not remove this notice, or any other, from this software.

using System.Collections.Generic;

namespace fitSharp.Machine.Model {
    public interface CellFactory {
        Tree<Cell> MakeCell(string text, string tag, IEnumerable<Tree<Cell>> branches);
    }

    public static class CellFactoryExtension {
        public static Tree<Cell> MakeEmptyCell(this CellFactory factory, string text) {
            return factory.MakeCell(text, string.Empty, new TreeList<Cell>[] {});
        }

    }
}
