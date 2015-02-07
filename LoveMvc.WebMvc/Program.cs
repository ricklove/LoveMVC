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

        public static LoveTemplate Host<T>(System.Web.Mvc.ControllerContext controllerContext, IViewViewModelPair<T> source)
        {
            return new WebMvcTemplateRenderer(controllerContext).BuildTemplate(source);
        }
    }

    public class WebMvcTemplateRenderer : ITemplateBuilder
    {
        System.Web.Mvc.ControllerContext _controllerContext;

        public WebMvcTemplateRenderer(System.Web.Mvc.ControllerContext controllerContext)
        {
            _controllerContext = controllerContext;
        }

        public LoveTemplate BuildTemplate<T>(IViewViewModelPair<T> viewViewModelPair)
        {
            var parser = new RazorParser();
            var evaluator = new WebMvcMarkupExpressionEvaluator(_controllerContext);

            return LoveTemplateBuilder.Build(parser, evaluator, viewViewModelPair);
        }
    }
}
