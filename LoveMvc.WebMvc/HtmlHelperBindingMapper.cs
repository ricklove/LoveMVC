using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LoveMvc.WebMvc
{
    public class HtmlHelperBindingMapper
    {
        public static LoveBlock MapBinding(LoveMarkupExpression htmlHelperExpression, string normalHtml, string simpleHtml, string simpleExpression)
        {
            var indices = normalHtml.Find(simpleHtml).ToList();

            if (indices.Count == 0)
            {
                // Apparently Static
                var b = new LoveBlock(0, 0);
                b.Children.Add(new LoveMarkup(0, 0, normalHtml));
                return b;
            }
            else
            {
                // Create a block with bindings
                var b = new LoveBlock(0, 0);

                // Add before binding
                b.Children.Add(new LoveMarkup(0, 0, normalHtml.Substring(0, indices[0])));

                for (int i = 0; i < indices.Count; i++)
                {
                    var index = indices[i];
                    var nextIndex = i < indices.Count - 1 ? indices[i + 1] : (int?)null;

                    // Add binding
                    var bindingContent = simpleExpression;
                    b.Children.Add(new LoveBinding(0, 0, bindingContent));

                    // Add after binding
                    var afterIndex = index + simpleHtml.Length;

                    if (nextIndex.HasValue)
                    {
                        b.Children.Add(new LoveMarkup(0, 0, normalHtml.Substring(afterIndex, nextIndex.Value - afterIndex)));
                    }
                    else
                    {
                        b.Children.Add(new LoveMarkup(0, 0, normalHtml.Substring(afterIndex)));
                    }
                }

                return b;
            }
        }

        public static string GetSimpleRazorExpression(LoveMarkupExpression htmlHelperExpression)
        {
            var simpleRazorValue = "";

            var exp = GetHtmlHelperLambdaExpression(htmlHelperExpression);

            if (exp != null)
            {
                if (exp.Expression == exp.VariableName || exp.Expression.StartsWith(exp.VariableName + "."))
                {
                    return exp.Expression.ReplaceStart(exp.VariableName, "Model");
                }
                else
                {
                    return exp.Expression;
                }
            }

            // TODO: Handle non-lamda expression
            throw new NotImplementedException();
        }

        public class SimpleLamdaExpression
        {
            public string VariableName { get; private set; }
            public string Expression { get; private set; }

            public SimpleLamdaExpression(string variableName, string expression)
            {
                VariableName = variableName;
                Expression = expression;
            }
        }

        private static SimpleLamdaExpression GetHtmlHelperLambdaExpression(LoveMarkupExpression htmlHelperExpression)
        {
            var regexName = @"(?:\s*[A-Za-z0-9_]+\s*)";
            var regexNamePath = @"{name}(?:\.{name})*";
            var regexHtmlHelper = @"^\s*[Hh]tml\s*.{name}\(({name})=>({path})[,)]";

            regexNamePath = regexNamePath.Replace("{name}", regexName);
            regexHtmlHelper = regexHtmlHelper.Replace("{name}", regexName);
            regexHtmlHelper = regexHtmlHelper.Replace("{path}", regexNamePath);

            var m = Regex.Match(htmlHelperExpression.Content, regexHtmlHelper);

            if (m.Success)
            {
                var varName = m.Groups[1].Value.Trim();
                var bindingExpression = m.Groups[2].Value.Trim();

                return new SimpleLamdaExpression(varName, bindingExpression);
            }

            return null;
        }

    }


}
