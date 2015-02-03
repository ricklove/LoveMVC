using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Razor;

namespace LoveMvc.WebMvc
{
    public class RazorParser : ITemplateParser
    {
        public LoveSyntaxTree Parse(TextReader source)
        {
            var language = new CSharpRazorCodeLanguage();
            var host = new RazorEngineHost(language) { };
            //{
            //    DefaultBaseClass = "OrderInfoTemplateBase",
            //    DefaultClassName = "OrderInfoTemplate",
            //    DefaultNamespace = "CompiledRazorTemplates",
            //};

            var parser = new RazorTemplateEngine(host);
            var results = parser.ParseTemplate(source);

            var debugText = RazorDebugView.ToDebug(results);

            throw new NotImplementedException();
        }


    }
}