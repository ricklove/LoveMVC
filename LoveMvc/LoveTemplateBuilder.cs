using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoveMvc
{
    public class LoveTemplateBuilder
    {
        public static LoveTemplate Build<T>(ITemplateParser parser, IMarkupExpressionEvaluator evaluator, IViewViewModelPair<T> pair)
        {
            var syntaxTree = parser.Parse(pair.ViewSource);
            syntaxTree.DecorateTree();

            var expressions = new List<LoveNode>();

            foreach (var n in syntaxTree.Flatten())
            {
                if (n is LoveMarkupExpression)
                {
                    expressions.Add(n);
                }
            }

            foreach (var n in expressions)
            {
                var expression = n as LoveMarkupExpression;
                var evaluated = evaluator.Evaluate(expression, pair.Model);
                n.Parent.Replace(expression, evaluated);
            }

            var t = new LoveTemplate(syntaxTree);

            // Redecorate tree
            t._syntaxTree.DecorateTree();

            var tText = t.ToString();

            return t;
        }
    }

    public class LoveTemplate
    {
        internal LoveSyntaxTree _syntaxTree;
        public LoveBlock Document { get { return _syntaxTree.Document; } }

        public LoveTemplate(LoveSyntaxTree syntaxTree)
        {
            _syntaxTree = syntaxTree;
        }

        public override string ToString()
        {
            return _syntaxTree.ToString();
        }

        public IEnumerable<LoveNode> Flatten()
        {
            return _syntaxTree.Flatten();
        }
    }
}
