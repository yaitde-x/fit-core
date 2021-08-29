﻿// Copyright © 2016 Syterra Software Inc. All rights reserved.
// The use and distribution terms for this software are covered by the Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file license.txt at the root of this distribution. By using this software in any fashion, you are agreeing
// to be bound by the terms of this license. You must not remove this notice, or any other, from this software.

using System;
using System.Web;
using fitSharp.Machine.Model;

namespace fitSharp.Parser {
    public class TextTables {
        readonly TextTableScanner scanner;
        readonly Func<string, Tree<Cell>> makeTreeCell;
        string[] startTags;
        string[] endTags;

        public TextTables(TextTableScanner scanner, Func<string, Tree<Cell>> makeTreeCell) {
            this.scanner = scanner;
            this.makeTreeCell = makeTreeCell;
        }

        public Tree<Cell> Parse() {
            var result = makeTreeCell(string.Empty);
            MakeTables(result);
            return result;
        }

        void MakeTables(Tree<Cell> result) {
            string leader = string.Empty;
            Tree<Cell> table = null;
            scanner.MoveNext();
            do {
                if (scanner.Current.Type == TokenType.Word) {
                    SetTags();
                    table = makeTreeCell(string.Empty);
                    table.Value.SetAttribute(CellAttribute.StartTag, startTags[0]);
                    table.Value.SetAttribute(CellAttribute.EndTag, endTags[0]);
                    if (leader.Length > 0) {
                        table.Value.SetAttribute(CellAttribute.Leader, leader);
                        leader = string.Empty;
                    }
                    result.Add(table);
                    MakeRows(table);
                    if (scanner.Current.Type == TokenType.Newline) {
                        leader += scanner.Current.Content;
                        scanner.MoveNext();
                    }
                }
                else {
                    leader += scanner.Current.Content;
                    scanner.MoveNext();
                }
            } while (scanner.Current.Type != TokenType.End && scanner.Current.Type != TokenType.EndCell);

            leader += scanner.Current.Content;
        
            if (table != null && leader.Length > 0) {
                 table.Value.SetAttribute(CellAttribute.Trailer, leader);
            }
        }

        void SetTags() {
            if (scanner.StartOfLine == CharacterType.Separator) {
                startTags = new [] {"<table class=\"fit_table\">", "<tr>", "<td>"};
                endTags = new [] {"</table>", "</tr>", "</td> "};
            }
            else {
                startTags = new [] {"<div>", "<table><tr>", "<td>"};
                endTags = new [] {"</div>", "</tr></table>", "</td> "};
            }
        }

        void MakeRows(Tree<Cell> table) {
            do {
                var row = makeTreeCell(string.Empty);
                row.Value.SetAttribute(CellAttribute.StartTag, startTags[1]);
                row.Value.SetAttribute(CellAttribute.EndTag, endTags[1]);
                table.Add(row);
                MakeCells(row);
                if (scanner.Current.Type == TokenType.Newline) scanner.MoveNext();
            } while (scanner.Current.Type == TokenType.BeginCell || scanner.Current.Type == TokenType.Word);
        }

        void MakeCells(Tree<Cell> row) {
            while (scanner.Current.Type == TokenType.BeginCell || scanner.Current.Type == TokenType.Word) {
                var cell = makeTreeCell(scanner.Current.Content);
                cell.Value.SetAttribute(CellAttribute.StartTag, startTags[2]);
                cell.Value.SetAttribute(CellAttribute.EndTag, endTags[2]);
                if (scanner.Current.Type == TokenType.BeginCell) {
                    MakeTables(cell);
                }
                else if (scanner.Current.Type == TokenType.Word) {
                    cell.Value.SetAttribute(CellAttribute.Body, HttpUtility.HtmlEncode(scanner.Current.Content));
                }
                row.Add(cell);
                scanner.MoveNext();
            }
        }
    }
}
