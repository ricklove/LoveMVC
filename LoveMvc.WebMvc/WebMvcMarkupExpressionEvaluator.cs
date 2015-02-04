using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace LoveMvc.WebMvc
{
    public class WebMvcMarkupExpressionEvaluator : IMarkupExpressionEvaluator
    {
        public ControllerContext ControllerContext { get; private set; }
        public WebMvcMarkupExpressionEvaluator(ControllerContext controllerContext)
        {
            ControllerContext = controllerContext;
        }

        public HtmlHelper<T> CreateHtmlHelper<T>(T model)
        {
            return new HtmlHelper<T>(new ViewContext(ControllerContext, new WebFormView(ControllerContext, "VIEW_PATH"), new ViewDataDictionary(), new TempDataDictionary(), new StringWriter()), new ViewPage()); ;
        }

        public LoveBlock Evaluate<T>(LoveMarkupExpression expression, T model)
        {
            if (expression.Content.Trim().StartsWith("Html."))
            {
                var html = CreateHtmlHelper(model);

                //var html = new HtmlHelper<T>(new FakeViewContext(), new FakeViewDataContainer(model));
                //html.EditorFor(m => m.);


                throw new NotImplementedException();
            }

            throw new InvalidOperationException();
        }

        //public static string Test<T>(HtmlHelper<T> html)
        //{
        //    var assembly = typeof(System.Web.Mvc.Html.DisplayExtensions).Assembly;
        //    var t = assembly.GetType("System.Web.Mvc.Html.TemplateHelpers");
        //    var mName = "GetDefaultActions";
        //    var m = t.GetMethod(mName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        //    var defaultActionsEditor = (Dictionary<string, Func<HtmlHelper, string>>)m.Invoke(null, new object[] { System.Web.UI.WebControls.DataBoundControlMode.Edit });
        //    var defaultActionsReadOnly = (Dictionary<string, Func<HtmlHelper, string>>)m.Invoke(null, new object[] { System.Web.UI.WebControls.DataBoundControlMode.ReadOnly });


        //    var action = defaultActionsEditor["Text"];
        //    var result = action(html);
        //    var r2 = defaultActionsReadOnly["Text"](html);

        //    throw new NotImplementedException();

        //    //var label = html.LabelFor(x => x.NewTodoText);
        //    //var display = html.DisplayFor(x => x.NewTodoText);
        //    //var editor = html.EditorFor(x => x.NewTodoText);
        //    //var text = editor.ToString();
        //    //return text;
        //}

        ////class FakeHtmlHelper<T> : HtmlHelper<T>
        ////{

        ////}

        ////class FakeViewContext : ViewContext
        ////{
        ////}

        //class FakeViewDataContainer : IViewDataContainer
        //{
        //    public FakeViewDataContainer(object model)
        //    {
        //        ViewData = new ViewDataDictionary(model);
        //    }

        //    public ViewDataDictionary ViewData { get; set; }
        //}
    }
}
