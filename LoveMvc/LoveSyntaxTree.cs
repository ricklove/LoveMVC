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
