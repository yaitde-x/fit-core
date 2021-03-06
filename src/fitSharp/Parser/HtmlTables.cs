// Copyright © 2016 Syterra Software Inc. All rights reserved.
// The use and distribution terms for this software are covered by the Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file license.txt at the root of this distribution. By using this software in any fashion, you are agreeing
// to be bound by the terms of this license. You must not remove this notice, or any other, from this software.

using System;
using System.Collections.Generic;
using fitSharp.Machine.Model;

namespace fitSharp.Parser {

    // Parses a HTML string, recognizing tables with embedded lists and tables.
    // Uses a recursive descent parsing approach.
    // The lexical analyzer is unusual - it skips everything until it finds the next expected token.
    public class HtmlTables {

        public HtmlTables(Func<string, Tree<Cell>> makeTreeCell) {
            this.makeTreeCell = makeTreeCell;
        }

        public Tree<Cell> Parse(string input) {
            var alternationParser = new AlternationParser();
            var cells = new ListParser("td", alternationParser, false, makeTreeCell);
            var rows = new ListParser("tr", cells, true, makeTreeCell);
            var tables = new ListParser("table", rows, true, makeTreeCell);
            var items = new ListParser("li", alternationParser, false, makeTreeCell);
            var lists = new ListParser("ul", items, true, makeTreeCell);
            alternationParser.ChildParsers = new [] {tables, lists};
            var result = makeTreeCell(string.Empty);
            result.Value.SetTag("div");
            foreach (var branch in tables.Parse(new LexicalAnalyzer(input))) result.Add(branch);
            return result;
        }

        readonly Func<string, Tree<Cell>> makeTreeCell;

        interface ElementParser {
            List<Tree<Cell>> Parse(LexicalAnalyzer theAnalyzer);
            string Keyword {get;}
        }

        class ListParser: ElementParser {

            readonly ElementParser myChildParser;
            readonly string myKeyword;
            readonly bool IRequireChildren;
            readonly Func<string, Tree<Cell>> makeTreeCell;

            public ListParser(string theKeyword, ElementParser theChildParser, bool thisRequiresChildren, Func<string, Tree<Cell>> makeTreeCell) {
                myChildParser = theChildParser;
                myKeyword = theKeyword;
                IRequireChildren = thisRequiresChildren;
                this.makeTreeCell = makeTreeCell;
            }

            public string Keyword {get {return myKeyword;}}

            public List<Tree<Cell>> Parse(LexicalAnalyzer theAnalyzer) {
                var list = new List<Tree<Cell>>();
                Tree<Cell> first = ParseOne(theAnalyzer);
                if (first != null) {
                    list.Add(first);
                    var rest = Parse(theAnalyzer);
                    list.AddRange(rest);
                    if (rest.Count == 0) {
                        var trailer = theAnalyzer.Trailer;
                        if (trailer.Length > 0) first.Value.SetAttribute(CellAttribute.Trailer, trailer);
                    }
                }
                return list;
            }

            public Tree<Cell> ParseOne(LexicalAnalyzer theAnalyzer) {
                theAnalyzer.GoToNextToken(myKeyword);
                if (theAnalyzer.Token.Length == 0) return null;
                return ParseElement(theAnalyzer);
            }

            Tree<Cell> ParseElement(LexicalAnalyzer theAnalyzer) {
                string tag = theAnalyzer.Token;
                string leader = theAnalyzer.Leader;
                theAnalyzer.PushEnd("/" + myKeyword);
                var children = myChildParser.Parse(theAnalyzer);
                if (IRequireChildren && children.Count == 0) {
                    throw new ApplicationException(string.Format("Can't find tag: {0}", myChildParser.Keyword));
                }
                theAnalyzer.PopEnd();
                theAnalyzer.GoToNextToken("/" + myKeyword);
                if (theAnalyzer.Token.Length == 0) throw new ApplicationException("expected /" + myKeyword + " tag");
                var result = makeTreeCell(HtmlToText(theAnalyzer.Leader));
                result.Value.SetAttribute(CellAttribute.Body, theAnalyzer.Leader);
                result.Value.SetAttribute(CellAttribute.EndTag, theAnalyzer.Token);
                if (leader.Length > 0) result.Value.SetAttribute(CellAttribute.Leader, leader);
                result.Value.SetAttribute(CellAttribute.StartTag, tag);
                foreach (var child in children) result.Add(child);
                return result;
            }
        }

        static string HtmlToText(string theHtml) {
            return new HtmlString(theHtml).ToPlainText();
        }

        class AlternationParser: ElementParser {
            public List<Tree<Cell>> Parse(LexicalAnalyzer theAnalyzer) {
                var result = new List<Tree<Cell>>();
                ListParser firstChildParser = null;
                int firstPosition = int.MaxValue;
                foreach (ListParser childParser in myChildParsers) {
                    int contentPosition = theAnalyzer.FindPosition(childParser.Keyword);
                    if (contentPosition >= 0 && contentPosition < firstPosition) {
                        firstPosition = contentPosition;
                        firstChildParser = childParser;
                    }
                }
                if (firstChildParser != null) {
                    result.Add(firstChildParser.ParseOne(theAnalyzer));
                    result.AddRange(Parse(theAnalyzer));
                }
                return result;
            }

            public string Keyword {get {return string.Empty;}}

            public ListParser[] ChildParsers {set {myChildParsers = value;}}

            ListParser[] myChildParsers;
        }

        class LexicalAnalyzer {

            readonly string myInput;
            int myPosition;
            readonly Stack<string> myEndTokens;

            public LexicalAnalyzer(string theInput) {
                myInput = theInput;
                myPosition = 0;
                myEndTokens = new Stack<string>();
            }

            public void GoToNextToken(string theToken) {
                Token = string.Empty;
                int start = IndexOfSkipComments(myInput, "<" + theToken, myPosition);
                if (start < 0 || start > EndPosition) return;
                Leader = myInput.Substring(myPosition, start - myPosition);
                int end = myInput.IndexOf('>', start);
                if (end < 0) return;
                Token = myInput.Substring(start, end - start + 1);
                myPosition = end + 1;
            }

            public int FindPosition(string theToken) {
                int start = IndexOfSkipComments(myInput, "<" + theToken, myPosition, Math.Min(EndPosition, myInput.Length) - myPosition);
                if (start < 0 || start > EndPosition) return -1;
                int end = myInput.IndexOf('>', start);
                if (end < 0) return -1;
                return start;
            }

            private int IndexOfSkipComments(string input, string value, int position) {
                return IndexOfSkipComments(input, value, position, input.Length - position);
            }

            private int IndexOfSkipComments(string input, string value, int position, int count) {
                int valueIndex = input.IndexOf(value, position, count, StringComparison.OrdinalIgnoreCase);
                if (valueIndex == -1)
                    return -1;

                // Scan backwards to see find the last comment that begins before the value we're looking for
                int commentIndex = input.LastIndexOf("<!--", valueIndex, valueIndex - position + 1, StringComparison.Ordinal);
                if (commentIndex != -1 && commentIndex < valueIndex) {
                    // From found comment, look for closing token
                    int endCommentIndex = input.IndexOf("-->", commentIndex + 4, StringComparison.Ordinal);
                    if (endCommentIndex == -1) {
                        // Unclosed comment
                        return -1;
                    }

                    // If value is between opening and closing comment tokens, value is enclosed in a comment.
                    // Find next value after said comment.
                    if (endCommentIndex > valueIndex) {
                        int afterComment = endCommentIndex + 3;
                        return IndexOfSkipComments(input, value, afterComment, count - (afterComment - position));
                    }
                }

                return valueIndex;
            }

            public string Trailer {
                get {
                    int endPosition = EndPosition;
                    string result = myInput.Substring(myPosition, endPosition - myPosition);
                    myPosition = endPosition;
                    return result;
                }
            }

            string PeekEnd() {
                string endToken = null;
                try {
                    endToken = myEndTokens.Peek();
                }
                catch (InvalidOperationException) {}
                return endToken;
            }

            public void PushEnd(string theToken) {
                myEndTokens.Push(theToken);
            }

            public void PopEnd() {
                myEndTokens.Pop();
            }

            public string Leader { get; private set; }
            public string Token { get; private set; }

            int EndPosition {
                get {
                    int endInput = -1;
                    string endToken = PeekEnd();
                    if (endToken != null) {
                        endInput = myInput.IndexOf("<" + endToken, myPosition, StringComparison.OrdinalIgnoreCase);
                    }
                    if (endInput < 0) endInput = myInput.Length;
                    return endInput;
                }
            }
        }
    }
}
