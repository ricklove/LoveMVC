using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace LoveMvc.WebMvc
{
    public class LoveVirtualViewProvider : IViewRenderer
    {
        // FROM: http://stackoverflow.com/questions/7865598/asp-net-mvc-3-get-the-current-controller-instance-not-just-name
        public class LoveControllerFactory : DefaultControllerFactory
        {
            public override IController CreateController(RequestContext requestContext, string controllerName)
            {
                var controller = base.CreateController(requestContext, controllerName);
                HttpContext.Current.Session["controllerInstance"] = controller;
                return controller;
            }

            public static IController CurrentController
            {
                get
                {
                    var controller = (IController)HttpContext.Current.Session["controllerInstance"];
                    return controller;
                }
            }

            public static ControllerContext CurrentControllerContext
            {
                get
                {
                    return (CurrentController as ControllerBase).ControllerContext;
                }
            }
        }

        public static void Initialize()
        {
            ControllerBuilder.Current.SetControllerFactory(new LoveControllerFactory());
            System.Web.Hosting.HostingEnvironment.RegisterVirtualPathProvider(LoveVirtualPathProvider.Initialize());

            Providers.ViewRenderer = new LoveVirtualViewProvider();
        }

        public static LoveVirtualViewProvider Instance
        {
            get
            {
                return Providers.ViewRenderer as LoveVirtualViewProvider;
            }
        }

        private LoveVirtualViewProvider()
        {
        }

        public string RenderView<T>(IViewViewModelPair<T> viewViewModelPair)
        {
            var source = viewViewModelPair.ViewSource.ReadToEnd().Trim();

            // Remove model line
            if (source.StartsWith("@model"))
            {
                source = source.Substring(source.IndexOf("\r\n")).Trim();
            }

            var partialView = CreatePartialView(viewViewModelPair.Model, source);
            var result = Render(viewViewModelPair.Model, partialView);
            var normalResults = GetTextBetweenTags(result, "START_NORMAL", "END_NORMAL");

            return normalResults;
        }

        public ExpressionEvaluationResults GetExpressionEvaluation<T>(T model, string normal, string simple, List<LoveScope> scopes)
        {
            // Register view
            var partialView = CreateExpressionEvaluationView(model, "@" + normal + "", "@" + simple + "", scopes);
            var result = Render<T>(model, partialView);
            var normalResults = GetTextBetweenTags(result, "START_NORMAL", "END_NORMAL");
            var simpleResults = GetTextBetweenTags(result, "START_SIMPLE", "END_SIMPLE");

            return new ExpressionEvaluationResults(normalResults, simpleResults);
        }

        private string Render<T>(T model, string partialView)
        {
            var mainPath = LoveVirtualPathProvider.Instance.RegisterExpression("H_" + partialView.GetHashCode() + "_" + model.GetHashCode(), partialView);

            // Intercept response
            var sb = new StringBuilder();
            var writer = new StringWriter(sb);
            var html = CreateHtmlHelper(model, writer);
            html.RenderPartial(mainPath, model);

            var result = sb.ToString().Trim();
            return result;
        }

        public class ExpressionEvaluationResults
        {
            public string NormalResults { get; private set; }
            public string SimpleResults { get; private set; }
            public ExpressionEvaluationResults(string normal, string simple)
            {
                NormalResults = normal;
                SimpleResults = simple;
            }
        }

        private static string GetTextBetweenTags(string text, string startTag, string endTag)
        {
            var sIndex = text.IndexOf(startTag);
            text = text.Substring(sIndex + startTag.Length);
            var eIndex = text.IndexOf(endTag);
            text = text.Substring(0, eIndex).Trim();
            return text;
        }

        public HtmlHelper<T> CreateHtmlHelper<T>(T model, StringWriter writer)
        {
            ControllerContext controllerContext = LoveControllerFactory.CurrentControllerContext;
            return new HtmlHelper<T>(new ViewContext(controllerContext, new WebFormView(controllerContext, "VIEW_PATH"), new ViewDataDictionary(model), new TempDataDictionary(), writer), new ViewPage()); ;
        }

        private string CreatePartialView<T>(T model, string content)
        {
            return CreateExpressionEvaluationView(model, content, "", new LoveScope[0]);
        }

        private string CreateExpressionEvaluationView<T>(T model, string expression, string simpleExpression, IEnumerable<LoveScope> scopes)
        {
            var scopePreSB = new StringBuilder();

            foreach (var s in scopes)
            {
                if (s.ScopeType == LoveScopeType.Foreach)
                {
                    scopePreSB.AppendFormat(@"@foreach( var {0} in {1} ) {{<text>", s.Name.Content, s.Expression.Content);
                }
            }

            var scopePostSB = new StringBuilder();

            foreach (var s in scopes.Reverse())
            {
                if (s.ScopeType == LoveScopeType.Foreach)
                {
                    scopePostSB.Append("</text>}");
                    //scopePostSB.Append("} @Erro");
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

{3}

START_NORMAL
{1}
END_NORMAL

START_SIMPLE
{2}
END_SIMPLE

{4}

", model.GetType().FullName, expression, simpleExpression, scopePreSB.ToString(), scopePostSB.ToString());
        }

        public class LoveVirtualPathProvider : VirtualPathProvider
        {
            public static LoveVirtualPathProvider Instance;
            public static LoveVirtualPathProvider Initialize()
            {
                if (Instance != null)
                {
                    throw new InvalidOperationException("Only initialize the LoveVirtualPathProvider once.");
                }

                Instance = new LoveVirtualPathProvider();

                return Instance;
            }

            public Dictionary<string, string> Expressions { get; private set; }

            private LoveVirtualPathProvider()
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
