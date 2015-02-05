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

        internal void DecorateTree()
        {
            Document.DecorateChildren();
        }

        public IEnumerable<LoveNode> Flatten()
        {
            return Document.Flatten();
        }

        public override string ToString()
        {
            return Document.ToString();
        }
    }

    public class LoveScope
    {
        public LoveSpan Name { get; private set; }
        public LoveBinding Expression { get; private set; }
        public LoveScopeType ScopeType { get; private set; }

        public LoveScope(LoveSpan name, LoveBinding expression, LoveScopeType scopeType)
        {
            Name = name;
            Expression = expression;
            ScopeType = scopeType;
        }
    }

    public enum LoveScopeType
    {
        Foreach
    }

    public class LoveNode
    {
        private static int __nextID = 0;

        public int ID { get; private set; }

        public int Start { get; private set; }
        public int Length { get; private set; }

        public LoveBlock Parent { get; set; }
        public List<LoveScope> GetScopes()
        {
            var scopes = new List<LoveScope>();
            GetScopesInner(scopes);

            return scopes;
        }

        private void GetScopesInner(List<LoveScope> scopes)
        {
            if (Parent != null)
            {
                // Add parent's own scopes first
                Parent.GetScopesInner(scopes);

                // Add scope from parent itself
                var pScope = Parent.GetThisScope();

                if (pScope != null)
                {
                    scopes.Add(pScope);
                }
            }
        }

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

        internal void DecorateChildren()
        {
            foreach (var c in Children)
            {
                c.Parent = this;

                if (c is LoveBlock)
                {
                    (c as LoveBlock).DecorateChildren();
                }
            }
        }

        protected internal virtual LoveScope GetThisScope()
        {
            return null;
        }

        internal IEnumerable<LoveNode> Flatten()
        {
            yield return this;

            foreach (var c in Children)
            {
                if (c is LoveBlock)
                {
                    foreach (var cItem in (c as LoveBlock).Flatten())
                    {
                        yield return cItem;
                    }
                }
                else
                {
                    yield return c;
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

        protected internal override LoveScope GetThisScope()
        {
            return new LoveScope(ItemName, Expression, LoveScopeType.Foreach);
        }

        public override string ToString()
        {
            return string.Format(@"<<<foreach_{0} item-name='{3}' binding='{1}'>>>{2}<<</foreach_{0}>>>", ID, Expression, Body, ItemName);
        }
    }

}
