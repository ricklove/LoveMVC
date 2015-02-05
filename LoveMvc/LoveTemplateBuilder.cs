using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoveMvc
{
    public class LoveTemplateBuilder
    {
        public static LoveTemplate Build(ITemplateParser parser, IMarkupExpressionEvaluator evaluator, IViewViewModelPair pair)
        {
            var results = parser.Parse(pair.ViewSource);

            var expressions = new List<LoveNode>();

            foreach (var n in results.Flatten())
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

            throw new NotImplementedException();
        }
    }

    public class LoveTemplate
    {

    }
}
