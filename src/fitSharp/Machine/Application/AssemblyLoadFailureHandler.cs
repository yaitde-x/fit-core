﻿// Copyright © 2015 Syterra Software Inc. All rights reserved.
// The use and distribution terms for this software are covered by the Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file license.txt at the root of this distribution. By using this software in any fashion, you are agreeing
// to be bound by the terms of this license. You must not remove this notice, or any other, from this software.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace fitSharp.Machine.Application
{
    /// <summary>
    /// Handles cases where an Assembly.LoadFrom() fails, potentially due to attempting to load external dependencies.
    /// </summary>
    public static class AssemblyLoadFailureHandler
    {
        static AssemblyLoadFailureHandler()
        {
            /* Add in the handlers for Assembly and Reflection-Only Resolution Failure */
            EnableAssemblyResolutionHandling();
        }

        private static bool isInitialized;

        private static readonly ISet<string> _folders = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        public static void EnableAssemblyResolutionHandling()
        {
            if (!isInitialized)
            {
                /* Add in the appropriate hooks for resolving Assemblies both during normal and reflection-only cases */
                AppDomain.CurrentDomain.AssemblyResolve += LoadFromSameFolder;
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += LoadFromSameFolder;

                isInitialized = true;
            }
        }

        private static Assembly LoadFromSameFolder(object sender, ResolveEventArgs args)
        {
            lock (_folders)
            {
                /* Find the path that actually contains the DLL in question */
                var file = _folders
                    .Select(f => Path.Combine(f, new AssemblyName(args.Name).Name + ".dll"))
                    .FirstOrDefault(f => File.Exists(f));

                /* If its null, fail */
                if (file == null) return null;

                Console.WriteLine("Using Assembly: {0}", file);

                return Assembly.LoadFrom(file);
            }
        }

        public static void AddFolder(string folderName)
        {
            lock (_folders)
            {
                if (!_folders.Contains(folderName))
                {
                    _folders.Add(folderName);
                }
            }
        }
    }
}
