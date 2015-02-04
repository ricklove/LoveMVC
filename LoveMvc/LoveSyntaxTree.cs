using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoveMvc
{
    public class LoveSyntaxTree
    {
        public LoveBlock Document { get; private set; }

        public LoveSyntaxTree(LoveBlock document)
        {
            Document = document;
        }

        public override string ToString()
        {
            return Document.ToString();
        }
    }

    public class LoveNode
    {
        private static int __nextID = 0;

        public int ID { get; private set; }

        public int Start { get; private set; }
        public int Length { get; private set; }

        public LoveNode(int start, int length)
        {
            Start = start;
            Length = length;

            ID = __nextID++;
        }

        public override string ToString()
        {
            return string.Format("[{0}:{0}+{1}]", ID, Start, Length);
        }
    }

    public class LoveBlock : LoveNode
    {
        public List<LoveNode> Children { get; private set; }

        public LoveBlock(int start, int length)
            : base(start, length)
        {
            Children = new List<LoveNode>();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var c in Children)
            {
                sb.Append(c.ToString());
            }

            return sb.ToString();
        }

        public IEnumerable<LoveNodeWithContext> Flatten(LoveBlock parent = null)
        {
            yield return new LoveNodeWithContext(this, parent);

            foreach (var c in Children)
            {
                if (c is LoveBlock)
                {
                    foreach (var cItem in (c as LoveBlock).Flatten(this))
                    {
                        yield return cItem;
                    }
                }
                else
                {
                    yield return new LoveNodeWithContext(c, this);
                }
            }
        }

        public void Replace(LoveNode toReplace, LoveNode replacement)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i] == toReplace)
                {
                    Children[i] = replacement;
                }
            }
        }
    }

    public class LoveNodeWithContext
    {
        public LoveNode Node { get; private set; }
        public LoveBlock Parent { get; private set; }

        public LoveNodeWithContext(LoveNode node, LoveBlock parent)
        {
            Node = node;
            Parent = parent;
        }
    }

    public class LoveSpan : LoveNode
    {
        public string Content { get; private set; }

        public LoveSpan(int start, int length, string content)
            : base(start, length)
        {
            Content = content;
        }

        public override string ToString()
        {
            return Content;
        }
    }

    public class LoveMarkup : LoveSpan
    {
        public LoveMarkup(int start, int length, string content)
            : base(start, length, content)
        {
        }
    }

    public class LoveMarkupExpression : LoveSpan
    {
        public LoveMarkupExpression(int start, int length, string content)
            : base(start, length, content)
        {
        }

        public override string ToString()
        {
            return "[[[" + Content + "]]]";
        }
    }

    public class LoveBinding : LoveSpan
    {
        public LoveBinding(int start, int length, string content)
            : base(start, length, content)
        {
        }

        public override string ToString()
        {
            return "{{{" + Content + "}}}";
        }
    }

    public class LoveControlBlock : LoveBlock
    {
        public LoveBinding Expression { get; set; }
        public LoveBlock Body { get; set; }

        public LoveControlBlock(int start, int length, LoveBinding expression, LoveBlock body)
            : base(start, length)
        {
            Expression = expression;
            Body = body;

            Children.Add(expression);
            Children.Add(body);
        }

    }

    public class LoveIfBlock : LoveControlBlock
    {
        public LoveIfBlock(int start, int length, LoveBinding expression, LoveBlock body) :
            base(start, length, expression, body)
        {
        }

        public override string ToString()
        {
            return string.Format(@"<<<if_{0} binding='{1}'>>>{2}<<</if_{0}>>>", ID, Expression, Body);
        }
    }
    public class LoveForeachBlock : LoveControlBlock
    {
        public LoveSpan ItemName { get; private set; }

        public LoveForeachBlock(int start, int length, LoveSpan itemName, LoveBinding expression, LoveBlock body) :
            base(start, length, expression, body)
        {
            ItemName = itemName;
        }

        public override string ToString()
        {
            return string.Format(@"<<<foreach_{0} item-name='{3}' binding='{1}'>>>{2}<<</foreach_{0}>>>", ID, Expression, Body, ItemName);
        }
    }

}
