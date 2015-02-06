using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LoveMvc.WebMvc.Host.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Test()
        {
            return View(new LoveMvc.TestDocs.WebMvc.TodosViewModel());
        }

        public ActionResult Index()
        {
            Response.Write("<html><body>");

            foreach (var doc in LoveMvc.TestDocs.WebMvc._TestDocs.GetDocs())
            {
                Response.Write("<h2>" + doc.Name + "</h2>");
                Response.Flush();

                var source = doc.ViewSource.ReadToEnd();
                var sourceView = ControllerContext.RenderPartialViewToString("Text", source);
                Response.Write(sourceView);
                Response.Flush();

                var parsed = new LoveMvc.Razor.RazorParser().Parse(doc.ViewSource);
                var parsedView = ControllerContext.RenderPartialViewToString("LoveRoot", parsed.Document);
                Response.Write(parsedView);
                Response.Flush();

                var template = LoveMvc.WebMvc.Program.Host(ControllerContext, doc);
                var templateView = ControllerContext.RenderPartialViewToString("LoveRoot", template.Document);
                Response.Write(templateView);
                Response.Flush();

                var testResults = TestDocs.Tests.CommonTests.RunTests(template, doc);
                var testResultsView = ControllerContext.RenderPartialViewToString("TestResults", testResults);
                Response.Write(testResultsView);
                Response.Flush();
            }

            Response.Write("</body></html>");
            Response.End();
            return null;
        }
    }

    // FROM: http://stackoverflow.com/questions/12632770/merge-output-of-partial-view
    public static class HtmlExtensions
    {
        public static string RenderPartialViewToString(this ControllerContext context, string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = context.RouteData.GetRequiredString("action");
            }

            context.Controller.ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(context, viewName);
                var viewContext = new ViewContext(context, viewResult.View, context.Controller.ViewData, context.Controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }
    }
}