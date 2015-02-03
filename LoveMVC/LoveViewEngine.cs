using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace LoveMVC
{
    public class LoveViewEngine : RazorViewEngine
    {
        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            partialPath = GenerateRazorFile(controllerContext, partialPath);

            return base.CreatePartialView(controllerContext, partialPath);
        }

        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            viewPath = GenerateRazorFile(controllerContext, viewPath);
            masterPath = GenerateRazorFile(controllerContext, masterPath);

            return CreateViewInner(controllerContext, viewPath, masterPath);

            //return base.CreateView(controllerContext, viewPath, masterPath);
        }

        private IView CreateViewInner(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            bool runViewStartPages = true;
            IEnumerable<string> fileExtensions = base.FileExtensions;
            IViewPageActivator viewPageActivator = base.ViewPageActivator;
            return new LoveRazorView(controllerContext, viewPath, masterPath, runViewStartPages, fileExtensions, viewPageActivator)
            {
                DisplayModeProvider = base.DisplayModeProvider
            };
        }

        private static string GenerateRazorFile(ControllerContext controllerContext, string viewPath)
        {
            var server = controllerContext.HttpContext.Server;
            var genFilePath = viewPath.Replace(".love.cshtml", ".love.gen.cshtml");

            var loveFileInfo = new FileInfo(server.MapPath(viewPath));
            var genFileInfo = new FileInfo(server.MapPath(genFilePath));

            if (loveFileInfo.FullName != genFileInfo.FullName)
            {

                if (true)//genFileInfo.LastWriteTimeUtc < loveFileInfo.LastWriteTimeUtc)
                {
                    // Generate 
                    var source = File.ReadAllText(loveFileInfo.FullName);

                    var language = new System.Web.Razor.CSharpRazorCodeLanguage();
                    var host = new System.Web.Razor.RazorEngineHost(language){};
                    //{
                    //    DefaultBaseClass = "OrderInfoTemplateBase",
                    //    DefaultClassName = "OrderInfoTemplate",
                    //    DefaultNamespace = "CompiledRazorTemplates",
                    //};

                    var parser = new System.Web.Razor.RazorTemplateEngine(host);
                    var parsed = parser.ParseTemplate(new StreamReader(loveFileInfo.FullName));

                    var debugText = RazorTreeViewer.ToDebug(parsed, source, true);

                    File.WriteAllText(genFileInfo.FullName, source);
                }

            }

            return genFilePath;
        }



        private static string[] LoveViewFormats = new[] {
            "~/Views/{1}/{0}.love.cshtml",
            "~/Views/Shared/{0}.love.cshtml"
        };

        public LoveViewEngine()
        {
            base.ViewLocationFormats = base.ViewLocationFormats.Union(LoveViewFormats).ToArray();
            base.PartialViewLocationFormats = base.PartialViewLocationFormats.Union(LoveViewFormats).ToArray();
        }

    }

    class LoveRazorView : RazorView
    {
        public LoveRazorView(ControllerContext controllerContext, string viewPath, string layoutPath, bool runViewStartPages, IEnumerable<string> viewStartFileExtensions)
            : base(controllerContext, viewPath, layoutPath, runViewStartPages, viewStartFileExtensions)
        {
        }
        public LoveRazorView(ControllerContext controllerContext, string viewPath, string layoutPath, bool runViewStartPages, IEnumerable<string> viewStartFileExtensions, IViewPageActivator viewPageActivator)
            : base(controllerContext, viewPath, layoutPath, runViewStartPages, viewStartFileExtensions, viewPageActivator)
        {
        }

        internal System.Web.WebPages.DisplayModeProvider DisplayModeProvider
        {
            get { return GetDisplayModeProviderPropertyInfo().GetValue(this) as System.Web.WebPages.DisplayModeProvider; }
            set { GetDisplayModeProviderPropertyInfo().SetValue(this, value); }
        }

        private System.Reflection.PropertyInfo GetDisplayModeProviderPropertyInfo()
        {
            var prop = GetType().BaseType.GetProperty("DisplayModeProvider",
                    BindingFlags.Instance |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly |
                    BindingFlags.Public);
            return prop;
        }

        protected override void RenderView(ViewContext viewContext, TextWriter writer, object instance)
        {
            var webViewPage = instance as WebViewPage;

            base.RenderView(viewContext, writer, instance);
        }
    }
}
