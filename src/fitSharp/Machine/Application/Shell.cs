// Copyright © 2020 Syterra Software Inc. All rights reserved.
// The use and distribution terms for this software are covered by the Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file license.txt at the root of this distribution. By using this software in any fashion, you are agreeing
// to be bound by the terms of this license. You must not remove this notice, or any other, from this software.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using fitSharp.IO;
using fitSharp.Machine.Engine;
using fitSharp.Machine.Model;

namespace fitSharp.Machine.Application {

    public class Shell: MarshalByRefObject {
        
        public static int Run(string[] arguments) {
            return new Shell(new ConsoleReporter(), new ShellArguments(new FileSystemModel(), arguments)).Run();
        }
        
        public Shell(ProgressReporter progressReporter, ShellArguments arguments) {
            this.progressReporter = progressReporter;
            this.arguments = arguments;
        }

        public Runnable Runner { get; private set; }

        public int Run() {
            try {
                return arguments.LoadMemory().Select(ReportError, RunInDomain);
            }
            catch (System.Exception e) {
                progressReporter.WriteLine(e.ToString());
                return 1;
            }
        }

        int ReportError(Error error) {
            progressReporter.Write(error.Message);
            progressReporter.WriteLine(Usage);
            return 1;
        }
        
        static string Usage =>
            $"Usage:\n\t{Process.GetCurrentProcess().ProcessName} [ -r runnerClass ][ -a appConfigFile ][ -c suiteConfigFile ] ...";

        int RunInDomain(Memory memory) {
            return RunInCurrentDomain(memory);
        }

        int RunInCurrentDomain(Memory memory) {
            Runner = new BasicProcessor().Create(memory.GetItem<Settings>().Runner).GetValue<Runnable>();
            ExecuteInApartment(memory);
            return result;
        }

        void ExecuteInApartment(Memory memory) {
            Run(memory);
        }

        void Run(Memory memory) {
            result = Runner.Run(arguments.Extras.ToList(), memory, progressReporter);
        }

        readonly ProgressReporter progressReporter;
        readonly ShellArguments arguments;

        int result;
    }
}
