// Copyright � 2009 Syterra Software Inc.
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License version 2.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.

using System;
using fitSharp.Fit.Engine;
using fitSharp.Machine.Application;

namespace fit {
    [Obsolete("Use suite configuration file")]
    public class FitVersionFixture : Fixture {

        public override bool IsVisible {
            get { return false; }
        }

        public override void DoTable(Parse theTable) {
            if (Args.Length > 0) {
                string behavior = Args[0].Trim().ToLower();
                Processor.Get<Settings>().Behavior = behavior;
            }
        }
    }
}
