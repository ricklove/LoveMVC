using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
            var treeText = tree.ToString();

            return tree;
        }

        private LoveSyntaxTree VisitTree(ParserResults results)
        {
            var document = VisitBlock(results.Document);

            // Simplify blocks
            SimplifyChildren(document);

            // Handle "@model View.Model" which is cut into a binding and a markup
            HandleModelStatement(document);

            var tree = new LoveSyntaxTree(document);
            return tree;
        }

        private void HandleModelStatement(LoveBlock document)
        {
            if (document.Children.Count <= 1) { return; }

            var first = document.Children[0] as LoveSpan;
            var second = document.Children[1] as LoveSpan;

            if (first == null || second == null) { return; }

            var modelMatch = Regex.Match(second.Content, @"^(\s*[A-Za-z0-9\.]+(?:\r\n)?)([\s\S]*)?$");

            if (first.Content == "model" && modelMatch.Success)
            {
                var modelLine = modelMatch.Groups[1].Value;
                var remaining = modelMatch.Groups.Count > 1 ? modelMatch.Groups[2].Value : "";

                document.Children[0] = new LoveModelStatement(first.Start, first.Length + modelLine.Length, modelLine.Trim());

                if (!string.IsNullOrWhiteSpace(remaining))
                {
                    second.Modify(second.Start + modelLine.Length, remaining.Length, remaining);
                }
                else
                {
                    document.Children.RemoveAt(1);
                }
            }
        }

        private void SimplifyChildren(LoveBlock block)
        {
            for (int i = 0; i < block.Children.Count; i++)
            {
                var child = block.Children[i];

                // Go deeper
                if (child is LoveBlock)
                {
                    SimplifyChildren(child as LoveBlock);
                }

                // Remove Bindings with only whitespace
                if (child is LoveBindingBase)
                {
                    var cBinding = child as LoveBindingBase;

                    if (string.IsNullOrEmpty(cBinding.Content.Trim()))
                    {
                        block.Children.RemoveAt(i);
                        i--;
                        continue;
                    }
                }

                // Simplify child blocks
                if (child.GetType() == typeof(LoveBlock))
                {
                    var cBlock = child as LoveBlock;

                    // Remove if no children
                    if (cBlock.Children.Count == 0)
                    {
                        block.Children.RemoveAt(i);
                        i--;
                        continue;
                    }

                    // Flatten if only one child
                    if (cBlock.Children.Count == 1)
                    {
                        block.Children[i] = cBlock.Children[0];
                    }
                }
            }
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
                    else if (cSpan is LoveMarkupExpression)
                    {
                        newChild = new LoveMarkupExpression(newSpan.Start, newSpan.Length + cSpan.Length, newSpan.Content + cSpan.Content);
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

            // Simplify children
            for (int i = 0; i < lBlock.Children.Count; i++)
            {
                var child = lBlock.Children[i];

                // Convert bindings to specific bindings if possible
                if (child is LoveBinding)
                {
                    lBlock.Children[i] = ConvertBinding(child as LoveBinding);
                }

            }

            // Convert to control blocks if Possible
            var cBlock = ConvertToControlBlock(lBlock);
            if (cBlock != null) { return cBlock; }

            return lBlock;
        }

        private static LoveNode ConvertBinding(LoveBindingBase binding)
        {
            var conversions = new List<Func<LoveBindingBase, LoveNode>>() {
                ConvertToMarkupExpression,
                CovertToNotBinding
            };

            foreach (var conversion in conversions)
            {
                var converted = conversion(binding);

                if (converted != null)
                {
                    return converted;
                }
            }

            return binding;
        }

        private static LoveNotBinding CovertToNotBinding(LoveBindingBase loveBinding)
        {
            if (loveBinding.Content.StartsWith("!"))
            {
                return new LoveNotBinding(loveBinding.Start + 1, loveBinding.Length - 1, loveBinding.Content.Substring(1));
            }

            return null;
        }

        private static LoveMarkupExpression ConvertToMarkupExpression(LoveBindingBase loveBinding)
        {
            var regexHtmlHelper = @"\s*Html\.";

            var m = Regex.Match(loveBinding.Content, regexHtmlHelper);

            if (m.Success)
            {
                return new LoveMarkupExpression(loveBinding.Start, loveBinding.Length, loveBinding.Content);
            }

            return null;
        }

        private LoveControlBlock ConvertToControlBlock(LoveBlock block)
        {
            if (block.Children.Count == 0) { return null; }

            var firstNode = block.Children.First();
            if (firstNode is LoveBinding)
            {
                var firstBinding = firstNode as LoveBinding;

                var regexControl = @"\s*((?:if)|(?:foreach))\s*\((.*)\)\s*({)?\s*";

                var m = Regex.Match(firstBinding.Content, regexControl);
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
                LoveBindingBase statementBinding = CreateStatement(firstBinding, statement);

                return new LoveIfBlock(block.Start, block.Length,
                    statementBinding,
                    bodyBlock);
            }
            else if (controlType == "foreach")
            {
                var regexForeach = @"\s*(\S+)\s+in\s+(\S+)\s*";

                var m = Regex.Match(statement, regexForeach);
                if (m.Success)
                {
                    var itemName = m.Groups[1].Value;
                    statement = m.Groups[2].Value;

                    var itemNameSpan = new LoveSpan(firstBinding.Start + firstBinding.Content.IndexOf(itemName), itemName.Length, itemName);

                    LoveBindingBase statementBinding = CreateStatement(firstBinding, statement);
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

        private static LoveBindingBase CreateStatement(LoveBinding firstBinding, string statement)
        {
            LoveBindingBase statementBinding = new LoveBinding(firstBinding.Start + firstBinding.Content.IndexOf(statement), statement.Length, statement);
            var converted = ConvertBinding(statementBinding);
            if (converted is LoveBindingBase)
            {
                statementBinding = converted as LoveBindingBase;
            }
            return statementBinding;
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