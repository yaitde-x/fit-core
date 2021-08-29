﻿// Copyright © 2011 Syterra Software Inc. All rights reserved.
// The use and distribution terms for this software are covered by the Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file license.txt at the root of this distribution. By using this software in any fashion, you are agreeing
// to be bound by the terms of this license. You must not remove this notice, or any other, from this software.

using fitSharp.Machine.Model;

namespace fitSharp.Machine.Engine {
    public class BasicProcessor: ProcessorBase<string, BasicProcessor> {
        private readonly Operators<string, BasicProcessor> operators;

        public BasicProcessor(): this(new TypeDictionary()) {
        }

        public BasicProcessor(Memory memory): base(memory) {
            operators = new Operators<string, BasicProcessor>(this);
            AddOperator(new DefaultCompose());
            AddOperator(new DefaultParse<string, BasicProcessor>());
            AddOperator(new ParseType<string, BasicProcessor>());
            AddOperator(new ParseEnum());
            AddOperator(new ParseNullable());
        }

        protected override Operators<string, BasicProcessor> Operators {
            get { return operators; }
        }

        private class DefaultCompose: Operator<string, BasicProcessor>, ComposeOperator<string> {
            public bool CanCompose(TypedValue instance) {
                return true;
            }

            public Tree<string> Compose(TypedValue instance) {
                return new TreeList<string>(instance.ValueString);
            }
        }
    }
}