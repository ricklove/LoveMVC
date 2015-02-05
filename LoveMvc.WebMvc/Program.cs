using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LoveMvc.Razor;

namespace LoveMvc.WebMvc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //WebMvcMarkupExpressionEvaluator.Register();
            //Host(FakeControllerContext.CreateControllerContext(), );
        }
        
        
        //public static void Host(System.Web.Mvc.ControllerContext controllerContext)
        //{
        //    var sourcePath = @"D:\UserData\Projects\Products\Frameworks\LoveMVC\LoveMvc.WebMvc\TestDocs\Todos.love.cshtml";
        //    sourcePath = Path.GetFullPath(sourcePath);

        //    
        //    var source = new StreamReader(sourcePath);
        //    var model = new TodosViewModel();

        //    Host(controllerContext, source, model);
        //}

        public static void Host(System.Web.Mvc.ControllerContext controllerContext, IViewViewModelPair source)
        {
            Host(controllerContext, source.ViewSource, source.Model);
        }

        public static void Host(System.Web.Mvc.ControllerContext controllerContext, TextReader source, object model)
        {
            var parser = new RazorParser();
            var results = parser.Parse(source);
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
