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
            Host(FakeControllerContext.CreateControllerContext(), null);
        }

        //public static void Test(System.Web.Mvc.HtmlHelper<TodosViewModel> html)
        //{
        //    var label = html.LabelFor(x => x.NewTodoText);
        //    var display = html.DisplayFor(x => x.NewTodoText);
        //    var editor = html.EditorFor(x => x.NewTodoText);
        //    var text = editor.ToString();
        //    //return text;
        //}


        public static void Host(System.Web.Mvc.ControllerContext controllerContext, Action<string, object> doRenderView)
        {
            //var sourcePath = Path.Combine(Directory.GetCurrentDirectory() + @"\..\..\TestDocs\Todos.love.cshtml");
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
                var evaluated = evaluator.Evaluate(expression, model, doRenderView);
                n.Parent.Replace(expression, evaluated);
            }
        }
    }
}
