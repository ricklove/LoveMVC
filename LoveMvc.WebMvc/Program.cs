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

        public static void Host(System.Web.Mvc.ControllerContext controllerContext, IViewViewModelPair source)
        {
            var parser = new RazorParser();
            var evaluator = new WebMvcMarkupExpressionEvaluator(controllerContext);

            LoveTemplateBuilder.Build(parser, evaluator, source);
        }
    }
}
