using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LoveMvc.Razor;
using LoveMvc.WebMvc.TestDocs;

namespace LoveMvc.WebMvc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //WebMvcMarkupExpressionEvaluator.Register();
            Host(FakeControllerContext.CreateControllerContext());
        }

        public static void Host(System.Web.Mvc.ControllerContext controllerContext)
        {
            var sourcePath = @"D:\UserData\Projects\Products\Frameworks\LoveMVC\LoveMvc.WebMvc\TestDocs\Todos.love.cshtml";
            sourcePath = Path.GetFullPath(sourcePath);

            var parser = new RazorParser();
            var results = parser.Parse(new StreamReader(sourcePath));

            var rText = results.ToString();

            var model = new TodosViewModel();
            var evaluator = new WebMvcMarkupExpressionEvaluator(controllerContext);

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
                var evaluated = evaluator.Evaluate(expression, model);
                n.Parent.Replace(expression, evaluated);
            }
        }
    }
}
