﻿// Copyright © 2019 Syterra Software Inc. All rights reserved.
// The use and distribution terms for this software are covered by the Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file license.txt at the root of this distribution. By using this software in any fashion, you are agreeing
// to be bound by the terms of this license. You must not remove this notice, or any other, from this software.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace fitSharp.Machine.Model {
    public static class EnumerableExtension {
        public static IEnumerable<T> Alternate<T>(this IEnumerable<T> collection) {
            var alternate = true;
            foreach (var unit in collection) {
                if (alternate) yield return unit;
                alternate = !alternate;
            }
        }

        public static T First<T>(this IEnumerable<T> collection, Func<T, bool> filter, Action notFound) {
            var result = collection.FirstOrDefault(filter);
            if (result.Equals(default(T))) notFound();
            return result;
        }

        public static void ForFirst<T>(this IEnumerable<T> collection, Action<T> firstAction, Action noneAction) {
            foreach (var item in collection) {
                firstAction(item);
                return;
            }
            noneAction();
        }

        public static R ForFirst<T,R>(this IEnumerable<T> collection, Func<T, R> firstAction, Func<R> noneAction) {
            foreach (var item in collection) {
                return firstAction(item);
            }
            return noneAction();
        }

        public static void ForFirst<T>(this IEnumerable<T> collection, Action<T> firstAction) {
            collection.ForFirst(firstAction, () => { });
        }

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action) {
            foreach (var item in collection) action(item);
        }

        public static void Replace<T>(this ICollection<T> list, T newItem) {
            foreach (var item in list.Where(item => newItem.Equals(item))) {
                list.Remove(item);
                break;
            }
            list.Add(newItem);
        }

        public static string Join<T>(this IEnumerable<T> list, string separator) {
            var result = new StringBuilder();
            foreach (var item in list) {
                if (result.Length > 0) result.Append(separator);
                result.Append(item.ToString());
            }
            return result.ToString();
        }

        public static IEnumerable<int> Count(this int count) {
            for (var i = 0; i < count; i++) yield return i;
        }
    }
}
