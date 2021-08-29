﻿// Copyright © 2013 Syterra Software Inc. All rights reserved.
// The use and distribution terms for this software are covered by the Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file license.txt at the root of this distribution. By using this software in any fashion, you are agreeing
// to be bound by the terms of this license. You must not remove this notice, or any other, from this software.

using fitSharp.Machine.Engine;
using fitSharp.Machine.Exception;
using fitSharp.Machine.Model;

namespace fitSharp.Fit.Engine {
    public class CellOperationValue {
        public static CellOperationValue Make(object systemUnderTest, Tree<Cell> memberName, Tree<Cell> parameters, bool isVolatile) {
            return new CellOperationValue(systemUnderTest, memberName, parameters, isVolatile);
        }

        public static CellOperationValue Make(TypedValue actualValue) {
            return new CellOperationValue(actualValue);
        }

        public object GetActual(CellProcessor processor) {
            return GetTypedActual(processor).Value;
        }

        public T GetActual<T>(CellProcessor processor) {
            return GetTypedActual(processor).GetValue<T>();
        }

        public TypedValue GetTypedActual(CellProcessor processor) {
            if (!actualValue.HasValue || IsVolatile) {
                try {
                    actualValue = processor.Invoke(new TypedValue(systemUnderTest), GetMemberName(processor), parameters);
                }
                catch (ParseException<Cell> e) {
                    processor.TestStatus.MarkException(e.Subject, e);
                    throw new IgnoredException();
                }
            }
            return actualValue.Value;
        }

        public object SystemUnderTest { get { return systemUnderTest; } }

        public bool IsVolatile { get; private set; }

        CellOperationValue(object systemUnderTest, Tree<Cell> memberName, Tree<Cell> parameters, bool isVolatile) {
            this.systemUnderTest = systemUnderTest;
            this.memberName = memberName;
            this.parameters = parameters;
            IsVolatile = isVolatile;
        }

        CellOperationValue(TypedValue actualValue) {
            this.actualValue = actualValue;
        }

        MemberName GetMemberName(Processor<Cell> processor) {
            return processor.ParseTree<Cell, MemberName>(memberName);
        }

        readonly object systemUnderTest;
        readonly Tree<Cell> memberName;
        readonly Tree<Cell> parameters;

        TypedValue? actualValue;
    }
}
