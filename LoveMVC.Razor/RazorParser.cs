using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Razor;
using System.Web.Razor.Parser.SyntaxTree;

namespace LoveMvc.Razor
{
    public class RazorParser : ITemplateParser
    {
        public LoveSyntaxTree Parse(TextReader source)
        {
            var language = new CSharpRazorCodeLanguage();
            var host = new RazorEngineHost(language) { };
            var parser = new RazorTemplateEngine(host);
            var results = parser.ParseTemplate(source);

            //var debugText = RazorDebugView.ToDebug(results);

            var tree = VisitTree(results);
            //var treeText = tree.ToString();

            return tree;
        }

        private LoveSyntaxTree VisitTree(ParserResults results)
        {
            var document = VisitBlock(results.Document);
            var tree = new LoveSyntaxTree(document);
            return tree;
        }

        private LoveNode VisitNode(SyntaxTreeNode node)
        {
            if (node.IsBlock) { return VisitBlock(node as Block); }
            else { return VisitNonBlock(node); }
        }

        private LoveBlock VisitBlock(Block block)
        {
            // Handle regular block
            var lBlock = new LoveBlock(block.Start.AbsoluteIndex, block.Length);

            foreach (var c in block.Children)
            {
                var node = VisitNode(c);

                if (node != null)
                {
                    lBlock.Children.Add(node);
                }
            }

            // Merge blocks of same type
            var newChildren = new List<LoveNode>();
            LoveNode newChild = null;

            foreach (var c in lBlock.Children)
            {
                if (newChild != null && newChild.GetType() == c.GetType() && c is LoveSpan)
                {
                    var cSpan = c as LoveSpan;
                    var newSpan = newChild as LoveSpan;

                    if (cSpan is LoveBinding)
                    {
                        newChild = new LoveBinding(newSpan.Start, newSpan.Length + cSpan.Length, newSpan.Content + cSpan.Content);
                    }
                    else if (cSpan is LoveMarkup)
                    {
                        newChild = new LoveMarkup(newSpan.Start, newSpan.Length + cSpan.Length, newSpan.Content + cSpan.Content);
                    }
                }
                else
                {
                    if (newChild != null)
                    {
                        newChildren.Add(newChild);
                        newChild = null;
                    }

                    newChild = c;
                }
            }

            if (newChild != null)
            {
                newChildren.Add(newChild);
                newChild = null;
            }

            lBlock.Children.Clear();
            lBlock.Children.AddRange(newChildren);

            if (lBlock.Children.Count == 0) { return null; }


            // Convert to control blocks if Possible
            var cBlock = ConvertToControlBlock(lBlock);
            if (cBlock != null) { return cBlock; }

            return lBlock;
        }

        private LoveControlBlock ConvertToControlBlock(LoveBlock block)
        {
            if (block.Children.Count == 0) { return null; }

            var firstNode = block.Children.First();
            if (firstNode is LoveBinding)
            {
                var firstBinding = firstNode as LoveBinding;

                var regexControl = @"\s*((?:if)|(?:foreach))\s*\((.*)\)\s*({)?\s*";

                var m = System.Text.RegularExpressions.Regex.Match(firstBinding.Content, regexControl);
                if (m.Success)
                {
                    var controlType = m.Groups[1].Value;
                    var statement = m.Groups[2].Value;
                    var openBraces = m.Groups.Count > 2 ? m.Groups[3].Value : "";

                    return CreateControlBlock(block, firstBinding, controlType, statement, openBraces);
                }
            }

            return null;
        }

        private static LoveControlBlock CreateControlBlock(LoveBlock block, LoveBinding firstBinding, string controlType, string statement, string openBraces)
        {
            var lastChild = block.Children.Last();

            var hasClosingBraces = openBraces == "{"
                && lastChild is LoveBinding
                && (lastChild as LoveBinding).Content.Trim() == "}";

            var bodyLength = block.Children.Count - 1;
            bodyLength -= hasClosingBraces ? 1 : 0;

            var bodyChildren = block.Children.Skip(1).Take(bodyLength);
            var bodyBlock = new LoveBlock(bodyChildren.First().Start, bodyChildren.Sum(c => c.Length));
            bodyBlock.Children.AddRange(bodyChildren);

            if (controlType == "if")
            {
                var statementBinding = new LoveBinding(firstBinding.Start + firstBinding.Content.IndexOf(statement), statement.Length, statement);

                return new LoveIfBlock(block.Start, block.Length,
                    statementBinding,
                    bodyBlock);
            }
            else if (controlType == "foreach")
            {
                var regexForeach = @"\s*(\S+)\s+in\s+(\S+)\s*";

                var m = System.Text.RegularExpressions.Regex.Match(statement, regexForeach);
                if (m.Success)
                {
                    var itemName = m.Groups[1].Value;
                    statement = m.Groups[2].Value;

                    var itemNameSpan = new LoveSpan(firstBinding.Start + firstBinding.Content.IndexOf(itemName), itemName.Length, itemName);

                    var statementBinding = new LoveBinding(firstBinding.Start + firstBinding.Content.IndexOf(statement), statement.Length, statement);
                    return new LoveForeachBlock(block.Start, block.Length,
                        itemNameSpan,
                        statementBinding,
                        bodyBlock);
                }
                else
                {
                    return null;
                }
            }

            return null;
        }

        private LoveNode VisitNonBlock(SyntaxTreeNode node)
        {
            var span = node as Span;

            if (span != null)
            {
                if (span.Kind == SpanKind.Code)
                {
                    return new LoveBinding(span.Start.AbsoluteIndex, span.Length, span.Content);
                }
                else if (span.Kind == SpanKind.Markup)
                {
                    return new LoveMarkup(span.Start.AbsoluteIndex, span.Length, span.Content);
                }
            }

            return null;
        }


    }
}