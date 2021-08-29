﻿// Copyright © 2015 Syterra Software Inc. All rights reserved.
// The use and distribution terms for this software are covered by the Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file license.txt at the root of this distribution. By using this software in any fashion, you are agreeing
// to be bound by the terms of this license. You must not remove this notice, or any other, from this software.

using System;

namespace fitSharp.Machine.Model {
    public class Traverse<T> {
        public Traverse<T> Rows { get { return this; } }

        public Traverse<T> All(Action<Tree<T>> visit) {
            all = visit;
            return this;
        }

        public Traverse<T> First(Action<Tree<T>> visit) {
            first = visit;
            return this;
        }

        public Traverse<T> Header(Action<Tree<T>> visit) {
            header = visit;
            return this;
        }

        public Traverse<T> Rest(Action<Tree<T>> visit) {
            rest = visit;
            return this;
        }

        public void VisitTable(Tree<T> table) {
            var action = header;
            var firstRow = true;
            foreach (var row in table.Branches) {
                if (firstRow) {
                    first(row);
                    firstRow = false;
                }
                else {
                    all(row);
                    action(row);
                    action = rest;
                }
            }
        }

        Action<Tree<T>> first = t => { }; 
        Action<Tree<T>> header = t => {};
        Action<Tree<T>> rest = t => {};
        Action<Tree<T>> all = t => {};
    }
}
