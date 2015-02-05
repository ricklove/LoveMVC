﻿using System;
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

            return new LoveTemplate(syntaxTree);
        }
    }

    public class LoveTemplate
    {
        private LoveSyntaxTree _syntaxTree;
        public LoveBlock Document { get { return _syntaxTree.Document; } }

        public LoveTemplate(LoveSyntaxTree syntaxTree)
        {
            _syntaxTree = syntaxTree;
        }
    }
}
