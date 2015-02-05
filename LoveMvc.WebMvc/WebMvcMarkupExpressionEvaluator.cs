using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace LoveMvc.WebMvc
{
    public class WebMvcMarkupExpressionEvaluator : IMarkupExpressionEvaluator
    {
        private static ExpressionProvider _provider;

        public static void Register()
        {
            _provider = new ExpressionProvider();
            System.Web.Hosting.HostingEnvironment.RegisterVirtualPathProvider(_provider);
            //ViewEngines.Engines.Insert(0, _engine);
        }

        public ControllerContext ControllerContext { get; private set; }
        public WebMvcMarkupExpressionEvaluator(ControllerContext controllerContext)
        {
            ControllerContext = controllerContext;
        }

        public LoveBlock Evaluate<T>(LoveMarkupExpression expression, T model, Action<string, object> doRenderExpression)
        {

            if (expression.Content.Trim().StartsWith("Html."))
            {
                // Register view
                var pView = CreatePartialView(model, "@" + expression.Content + "", expression.GetScopes());
                var mainPath = _provider.RegisterExpression("H_" + expression.Content.GetHashCode(), pView);

                // This works to render to client

                //HttpResponse response = HttpContext.Current.Response;
                //var oldFilter = response.Filter;
                //response.Flush();

                //var filter = new MemoryStream();
                //response.Filter = filter;

                //doRenderExpression(mainPath, model);

                //response.Flush();
                //response.Filter = oldFilter;

                //var result = new StreamReader(filter).ReadToEnd();
                //var result = "";

                // Trying to intercept response
                var sb = new StringBuilder();
                var writer = new StringWriter(sb);
                var html = CreateHtmlHelper(model, writer);
                html.RenderPartial(mainPath, model);

                var result = sb.ToString();

                // TODO: Parse the markup
                var b = new LoveBlock(0, 0);
                b.Children.Add(new LoveMarkup(0, 0, result));
                return b;

                //var html = new HtmlHelper<T>(new FakeViewContext(), new FakeViewDataContainer(model));
                //html.EditorFor(m => m.);


                //throw new NotImplementedException();
            }

            //throw new InvalidOperationException();

            return null;
        }

        private string CreatePartialView<T>(T model, string content, IEnumerable<LoveScope> scopes)
        {
            var scopePreSB = new StringBuilder();

            foreach (var s in scopes)
            {
                if (s.ScopeType == LoveScopeType.Foreach)
                {
                    scopePreSB.AppendFormat(@"@foreach( var {0} in {1} ) {{", s.Name.Content, s.Expression.Content);
                }
            }

            var scopePostSB = new StringBuilder();

            foreach (var s in scopes.Reverse())
            {
                if (s.ScopeType == LoveScopeType.Foreach)
                {
                    scopePostSB.Append("}");
                }
            }

            return string.Format(@"
@using System.Web.Mvc;
@using System.Web.Mvc.Ajax;
@using System.Web.Mvc.Html;
@using System.Web.Routing;

@inherits System.Web.Mvc.WebViewPage<{0}>

@{{
    Layout = null;
}}

{2}

{1}

{3}

", model.GetType().FullName, content, scopePreSB.ToString(), scopePostSB.ToString());
        }

        public HtmlHelper<T> CreateHtmlHelper<T>(T model, StringWriter writer)
        {
            var controllerContext = FakeControllerContext.CreateControllerContext();
            //var controllerContext = ControllerContext;
            return new HtmlHelper<T>(new ViewContext(controllerContext, new WebFormView(controllerContext, "VIEW_PATH"), new ViewDataDictionary(), new TempDataDictionary(), writer), new ViewPage()); ;
        }



        public class ExpressionProvider : VirtualPathProvider
        {
            public Dictionary<string, string> Expressions { get; private set; }

            public ExpressionProvider()
            {
                Expressions = new Dictionary<string, string>();
            }

            public string RegisterExpression(string name, string expression)
            {
                Expressions[name] = expression;
                return GetVirtualPathFromName(name);
            }

            private static string PathPrefix = "/Love_Expression/";
            private static string PathSuffix = ".love.expression.cshtml";

            public static string GetVirtualPathFromName(string name)
            {
                return "~" + PathPrefix + name + PathSuffix;
            }

            private static string GetExpressionName(string virtualPath)
            {
                var relativePath = VirtualPathUtility.ToAppRelative(virtualPath);
                relativePath = relativePath.TrimStart('~');

                if (!relativePath.StartsWith(PathPrefix)
                    || !relativePath.EndsWith(PathSuffix))
                {
                    return "";
                }

                var name = relativePath.Substring(PathPrefix.Length);
                name = name.Substring(0, name.Length - PathSuffix.Length);
                return name;
            }

            private bool DoesFileExist(string virtualPath)
            {
                return Expressions.ContainsKey(GetExpressionName(virtualPath));
            }

            private string GetExpression(string virtualPath)
            {
                return Expressions[GetExpressionName(virtualPath)];
            }

            private VirtualFile DoGetFile(string virtualPath)
            {
                if (DoesFileExist(virtualPath))
                {
                    return new LoveVirtualFile(virtualPath, GetExpression(virtualPath));
                }
                else
                {
                    return null;
                }
            }

            public override bool FileExists(string virtualPath)
            {
                if (DoesFileExist(virtualPath))
                {
                    return true;
                }
                else
                {
                    return Previous.FileExists(virtualPath);
                }
            }

            public override VirtualFile GetFile(string virtualPath)
            {
                var file = DoGetFile(virtualPath);

                if (file != null)
                {
                    return file;
                }
                else
                {
                    return Previous.GetFile(virtualPath);
                }
            }

            public override System.Web.Caching.CacheDependency
                   GetCacheDependency(string virtualPath,
                   System.Collections.IEnumerable virtualPathDependencies,
                   DateTime utcStart)
            {
                if (DoesFileExist(virtualPath))
                {
                    return null;
                }
                else
                {
                    return Previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
                }
            }
        }

        public class LoveVirtualFile : VirtualFile
        {
            public string Text { get; private set; }

            public LoveVirtualFile(string path, string text)
                : base(path)
            {
                Text = text;
            }

            public override Stream Open()
            {
                return new MemoryStream(Encoding.UTF8.GetBytes(Text ?? ""));
            }
        }

    }
}
