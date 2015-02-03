using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Razor.Parser.SyntaxTree;

namespace LoveMvc.WebMvc
{
    public class RazorDebugView
    {
        public static string ToDebug(System.Web.Razor.ParserResults tree)
        {
            var sourceSb = new StringBuilder();

            foreach (var f in tree.Document.Flatten())
            {
                sourceSb.Append((f as Span).Content);
            }

            var source = sourceSb.ToString();

            var sb = new StringBuilder();


            sb.AppendLine("--------------------------------");
            sb.AppendLine("-----Grouping-------------------");
            sb.AppendLine("--------------------------------");
            ToDebugInnerWithGrouping(sb, source, tree.Document, 0);


            sb.AppendLine("--------------------------------");
            sb.AppendLine("-----Outline--------------------");
            sb.AppendLine("--------------------------------");
            ToDebugInner(sb, source, tree.Document, true, 0);


            sb.AppendLine("--------------------------------");
            sb.AppendLine("-----Flat-----------------------");
            sb.AppendLine("--------------------------------");

            foreach (var f in tree.Document.Flatten())
            {
                ToDebugInner(sb, source, f, false, 1);
            }


            sb.AppendLine("--------------------------------");
            sb.AppendLine("-----All------------------------");
            sb.AppendLine("--------------------------------");

            ToDebugInner(sb, source, tree.Document, false, 0);

            return sb.ToString();
        }

        private static void ToDebugInner(StringBuilder sb, string source, SyntaxTreeNode node, bool shouldIncludeBlocksOnly, int indentation)
        {
            if (node.IsBlock)
            {
                var block = node as Block;
                WriteBlock(sb, source, block, indentation);

                foreach (var c in block.Children)
                {
                    ToDebugInner(sb, source, c, shouldIncludeBlocksOnly, indentation + 1);
                }
            }
            else
            {
                if (shouldIncludeBlocksOnly) { return; }
                WriteNonBlock(sb, source, node, indentation);
            }
        }

        private static void ToDebugInnerWithGrouping(StringBuilder sb, string source, Block block, int indentation)
        {
            WriteBlock(sb, source, block, indentation);

            List<SyntaxTreeGroup> groups = SyntaxTreeGroup.GroupChildren(block);

            foreach (var g in groups)
            {
                if (g.Block != null)
                {
                    ToDebugInnerWithGrouping(sb, source, g.Block, indentation + 1);
                }
                else
                {
                    WriteNonBlockGroup(sb, source, g.NonBlockGroup, indentation + 1);
                }
            }
        }

        private static void WriteIndentation(StringBuilder sb, int indentation, string indentText = "-")
        {
            for (int i = 0; i < indentation; i++)
            {
                sb.Append(indentText);
            }
        }

        private static void WriteNodeSource(StringBuilder sb, string source, SyntaxTreeNode node, bool shouldTrimWhitespace = false)
        {
            var text = source.Substring(node.Start.AbsoluteIndex, node.Length);

            if (shouldTrimWhitespace) { text = text.Trim(); }

            text = text.Replace("\r\n", "\\r\\n");
            sb.Append(text);
        }

        private static void WriteNonBlock(StringBuilder sb, string source, SyntaxTreeNode node, int indentation)
        {
            WriteIndentation(sb, indentation);

            if (node is Span)
            {
                sb.Append("[[[" + (node as Span).Kind + "]]]");
            }
            else
            {
                sb.Append("[[[?]]]");
            }

            WriteNodeSource(sb, source, node);
            sb.AppendLine();
        }

        private static void WriteNonBlockGroup(StringBuilder sb, string source, IEnumerable<SyntaxTreeNode> group, int indentation, bool shouldSimplify = true)
        {
            WriteIndentation(sb, indentation, ":");

            SpanKind lastKind = SpanKind.Comment;

            foreach (var node in group)
            {
                if (shouldSimplify)
                {
                    var s = node as Span;

                    if (s != null && s.Kind == SpanKind.Code || s.Kind == SpanKind.Markup)
                    {
                        if (s.Kind != lastKind)
                        {
                            sb.Append("[[[" + (node as Span).Kind + "]]]");
                        }
                        else
                        {
                            sb.Append(" ");
                        }

                        lastKind = (node as Span).Kind;

                        WriteNodeSource(sb, source, node, true);
                    }
                }
                else
                {
                    if (node is Span)
                    {
                        sb.Append("[[[" + (node as Span).Kind + "]]]");
                    }
                    else
                    {
                        sb.Append("[[[?]]]");
                    }

                    WriteNodeSource(sb, source, node);
                }
            }
            sb.AppendLine();
        }

        private static void WriteBlock(StringBuilder sb, string source, Block block, int indentation)
        {
            WriteIndentation(sb, indentation);
            sb.Append("[[[" + block.Name + ":" + block.Type + "]]]");
            WriteNodeSource(sb, source, block);
            sb.AppendLine();
        }



    }

    public class SyntaxTreeGroup
    {
        public List<SyntaxTreeNode> NonBlockGroup { get; set; }
        public Block Block { get; set; }

        public static List<SyntaxTreeGroup> GroupChildren(Block block)
        {
            var groups = new List<SyntaxTreeGroup>();

            foreach (var c in block.Children)
            {
                var lastGroup = groups.LastOrDefault();

                if (!c.IsBlock)
                {
                    if (lastGroup != null && lastGroup.NonBlockGroup != null)
                    {
                        lastGroup.NonBlockGroup.Add(c);
                    }
                    else
                    {
                        groups.Add(new SyntaxTreeGroup() { NonBlockGroup = new List<SyntaxTreeNode>() { c } });
                    }
                }
                else
                {
                    groups.Add(new SyntaxTreeGroup() { Block = c as Block });
                }
            }

            return groups;
        }

    }
}
